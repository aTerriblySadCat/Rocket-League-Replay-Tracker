using System;

namespace Rocket_League_Replay_Tracker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int helpFlagIndex = GetFlagIndex(args, new string[] { "-h", "-help" });

            if (helpFlagIndex != -1)
            {
                PrintHelp();
            }
            else
            {
                int playerFlagIndex = GetFlagIndex(args, new string[] { "-p", "-players" });
                int updateFlag = GetFlagIndex(args, new string[] { "-u", "-update" });
                int directoryFlag = GetFlagIndex(args, new string[] { "-d", "-directory" });
                int googleFlag = GetFlagIndex(args, new string[] { "-g", "-google" });
                int waitTimeFlag = GetFlagIndex(args, new string[] { "-w, -wait " });

                if(!FlagIsValid(args, playerFlagIndex) || !FlagIsValid(args, directoryFlag) || !FlagIsValid(args, googleFlag) || !FlagIsValid(args, waitTimeFlag))
                {
                    throw new ArgumentException("Invalid flag found!\nUse -h, -help to get information on how to use the flags.");
                }

                // Setup connection with Google Sheets
                GoogleSheetsManager.CreateService();
                Console.WriteLine("Debug - Connected with Google service!");

                int waitTime = 30000;
                if (waitTimeFlag != -1)
                {
                    if (!int.TryParse(args[waitTimeFlag + 1], out waitTime))
                    {
                        waitTime = 30000;
                    }
                }

                // Get the players to track from the command line
                List<string>? playersToTrack = null;
                if(playerFlagIndex != -1)
                {
                    playersToTrack = GetPlayersToTrack(args, playerFlagIndex);
                }

                // Setup the path to the replays
                string replayFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Rocket League\\TAGame\\Demos";
                if (directoryFlag != -1)
                {
                    replayFolderPath = args[directoryFlag + 1];
                }
                if (!Directory.Exists(replayFolderPath))
                {
                    throw new ArgumentException("The folder (" + replayFolderPath + ") does not exist!\nPlease choose a valid path using the -d, -directory flags.\nSee -h, -help for more info."); ;
                }

                // Get all the replay files from the folder
                Dictionary<string, DateTime> replayFileDictionary = new Dictionary<string, DateTime>();
                foreach (string replayFileName in Directory.EnumerateFiles(replayFolderPath))
                {
                    if (replayFileName.EndsWith(".replay"))
                    {
                        DateTime creationTime = File.GetLastWriteTimeUtc(replayFileName);
                        replayFileDictionary.Add(replayFileName, creationTime);
                    }
                }
                // Order replays from earliest to latest
                replayFileDictionary = replayFileDictionary.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                Config config = XmlManager.GetConfig(args, replayFolderPath, replayFileDictionary, googleFlag, updateFlag, directoryFlag);
                if (config.lastReplayFileName == "")
                {
                    // Remove all files so they all get updated
                    replayFileDictionary.Clear();
                }
                else if(updateFlag != -1)
                {
                    // Delete files that are newer than the lastReplayFileName so they get updated
                    // If the lastReplayFileName can't be found it will just ignore the -u flag
                    bool delete = false;
                    List<string> fileNames = replayFileDictionary.Keys.ToList();
                    foreach (string fileName in fileNames)
                    {
                        if(fileName == config.lastReplayFileName)
                        {
                            delete = true;
                            continue;
                        }

                        if (delete)
                        {
                            replayFileDictionary.Remove(fileName);
                        }
                    }

                    if(!delete)
                    {
                        throw new ArgumentException("The replay file to check for (" + config.lastReplayFileName + ") does not exist!\nPlease use the -u, -update flag to update this value.");
                    }
                }

                // Check if spreadsheet exists
                string? spreadSheetId = config.googleSpreadSheetId;
                if (spreadSheetId == null || !GoogleSheetsManager.DoesSpreadSheetExist(spreadSheetId))
                {
                    throw new ArgumentException("Spreadsheet with ID (" + spreadSheetId + ") does not exist!\nPlease use the -g, -google flag to update the value with a correct ID.");
                }

                // Infinite loop so it keeps running
                while (true)
                {
                    MainLoop(replayFolderPath, replayFileDictionary, playersToTrack, spreadSheetId, config);
                    Console.WriteLine("Debug - Waiting for files...");
                    Thread.Sleep(waitTime);
                }
            }
        }

        /// <summary>
        /// Is the given flagIndex valid with the given args?
        /// </summary>
        /// <param name="args">The parameters containing the flags and their parameters.</param>
        /// <param name="flagIndex">The index where the flag is located in the given args.</param>
        /// <returns></returns>
        private static bool FlagIsValid(string[] args, int flagIndex)
        {
            if (flagIndex == -1)
            {
                return true;
            }

            return (args.Length >= flagIndex + 1);
        }

        /// <summary>
        /// Gets the flag index by looking for the given flagNames in the given args. Returns the first occurance of any of the flag names. All others are ignored. Returns -1 if the flag could not be found.
        /// </summary>
        /// <param name="args">The parameters containing the flags.</param>
        /// <param name="flagNames">The possible names of the flags.</param>
        /// <returns></returns>
        private static int GetFlagIndex(string[] args, string[] flagNames)
        {
            int flagIndex = -1;
            foreach (string flagName in flagNames)
            {
                flagIndex = Array.IndexOf(args, flagName);
                if (flagIndex != -1)
                {
                    return flagIndex;
                }
            }
            return flagIndex;
        }

        /// <summary>
        /// Extract the players to track from the given args, starting at the flagIndex. It stops looking for players when the first string starting with "-" is found. Returns the list of player names. Returns an empty list if no players could be found.
        /// </summary>
        /// <param name="args">The parameters where the player names can be found.</param>
        /// <param name="flagIndex">The index of the flag, so from where to start looking in the given args.</param>
        /// <returns></returns>
        private static List<string> GetPlayersToTrack(string[] args, int flagIndex)
        {
            List<string> returnList = new List<string>();
            for (int i = flagIndex + 1; i < args.Length; i++)
            {
                if (!args[i].StartsWith("-"))
                {
                    returnList.Add(args[i]);
                    continue;
                }

                break;
            }

            return returnList;
        }

        /// <summary>
        /// The main loop where all files from the given replayFolderPath are gathered.
        /// If a file is not yet present in the given replayFileDictionary, the file will be analyzed and data is processed.
        /// </summary>
        /// <param name="replayFolderPath">The path to the folder containing the replays.</param>
        /// <param name="replayFileDictionary">The Dictionary containing the replay files that don't need to be analyzed.</param>
        /// <param name="playersToTrack">A list of player names to track. All other players are ignored.</param>
        /// <param name="spreadSheetId">The ID of the Google Sheets Spreadsheet to where the data is written.</param>
        /// <param name="config">The configuration class.</param>
        /// <exception cref="ArgumentException"></exception>
        private static void MainLoop(string replayFolderPath, Dictionary<string, DateTime> replayFileDictionary, List<string>? playersToTrack, string spreadSheetId, Config config)
        {
            foreach (string replayFileName in Directory.EnumerateFiles(replayFolderPath))
            {
                if (replayFileName.EndsWith(".replay") && !replayFileDictionary.ContainsKey(replayFileName))
                {
                    Console.WriteLine("Debug - New replay file found (" + replayFileName + ")");

                    // New replay file found
                    DateTime lastWriteTime = File.GetLastWriteTimeUtc(replayFileName);
                    replayFileDictionary.Add(replayFileName, lastWriteTime);

                    replayFileDictionary = replayFileDictionary.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                    config.lastReplayFileName = replayFileDictionary.Keys.Last();
                    XmlManager.WriteConfig(config);

                    // Analyze File
                    Unpacker unpacker = new Unpacker(replayFileName);

                    // Get all properties
                    List<Property> properties = unpacker.GetProperties();
                    Property? playerStatsProperty = properties.Find(x => x.GetName() == "PlayerStats" && x.GetType() == "ArrayProperty");
                    if (playerStatsProperty == null)
                    {
                        throw new ArgumentException("Could not find any Goals data!");
                    }

                    List<Property>? playerStatsProperties = playerStatsProperty.GetValue();
                    if (playerStatsProperties != null)
                    {
                        // Put properties in PlayerStats structs for better readability
                        List<PlayerStats> playersStatsToTrack = new List<PlayerStats>(playerStatsProperties.Count);
                        PlayerStats playerStats = new PlayerStats();
                        for (int i = 0; i < playerStatsProperties.Count; i += 10)
                        {
                            string? playerName = playerStatsProperties[i].GetValue();
                            if(playerName == null) 
                            {
                                playerStats.name = "";
                            }
                            else
                            {
                                playerStats.name = playerName;
                            }
                            string[]? byteProperty = playerStatsProperties[i + 1].GetValue();
                            if (byteProperty != null)
                            {
                                playerStats.platform = byteProperty[0];
                                playerStats.platformName = byteProperty[1];
                            }
                            playerStats.onlineId = playerStatsProperties[i + 2].GetValue();
                            playerStats.team = playerStatsProperties[i + 3].GetValue();
                            playerStats.score = playerStatsProperties[i + 4].GetValue();
                            playerStats.goals = playerStatsProperties[i + 5].GetValue();
                            playerStats.assists = playerStatsProperties[i + 6].GetValue();
                            playerStats.saves = playerStatsProperties[i + 7].GetValue();
                            playerStats.shots = playerStatsProperties[i + 8].GetValue();
                            playerStats.isBot = playerStatsProperties[i + 9].GetValue();
                            playersStatsToTrack.Add(playerStats);
                        }

                        if (playersToTrack != null)
                        {
                            // If specific players need to be tracked, get them here and ditch the others
                            playersStatsToTrack = playersStatsToTrack.FindAll(x => playersToTrack.Contains(x.name));
                        }

                        foreach (PlayerStats updatePlayerStats in playersStatsToTrack)
                        {
                            string playerSheetName = "RL - ";
                            if(updatePlayerStats.onlineId == 0)
                            {
                                playerSheetName += updatePlayerStats.name;
                            }
                            else 
                            {
                                playerSheetName += updatePlayerStats.onlineId;
                            }

                            if (!GoogleSheetsManager.DoesSheetExist(spreadSheetId, playerSheetName))
                            {
                                // TODO - Create new sheet
                                GoogleSheetsManager.CreateNewSheet(spreadSheetId, playerSheetName);
                                Console.WriteLine("Debug - Created a new sheet " + playerSheetName);
                            }

                            if (!GoogleSheetsManager.IsFirstRowTaken(spreadSheetId, playerSheetName))
                            {
                                GoogleSheetsManager.InsertFirstPlayerStatsRow(spreadSheetId, playerSheetName);
                                Console.WriteLine("Debug - Created first row in sheet " + playerSheetName);
                            }

                            List<object> appendPlayerStats = new List<object>() { updatePlayerStats.name, updatePlayerStats.onlineId.ToString(), updatePlayerStats.platformName, updatePlayerStats.team, updatePlayerStats.score, updatePlayerStats.goals, updatePlayerStats.assists, updatePlayerStats.saves, updatePlayerStats.shots };
                            GoogleSheetsManager.AppendValues(spreadSheetId, playerSheetName, appendPlayerStats);
                            Console.WriteLine("Debug - Appended player stats of player " + updatePlayerStats.name + " to " + playerSheetName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prints the entire help info to the console.
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine("Designed by The Blooper Troopers (Name Pending). Coded by A Very Sad Cat.");
            Console.WriteLine("This application tracks stats found in Rocket League replays.");
            Console.WriteLine("It does this continously in the background, so it can be kept running while playing.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("RocketLeagueReplayTracker.exe <googleSheetsId> <flags>");
            Console.WriteLine();
            Console.WriteLine("===== Command line parameters ======");
            Console.WriteLine("-p, -players");
            Console.WriteLine("Allows for a list of player names to track to be given to the application.");
            Console.WriteLine("If no players are specified all players in a match are tracked.");
            Console.WriteLine("Names are split by spaces. Names containing spaces should be surrounded by \"\".");
            Console.WriteLine("Example:");
            Console.WriteLine("RocketLeagueReplayTracker.exe -p \"A Very Sad Cat\" Lemon");
            Console.WriteLine();
            Console.WriteLine("-u, -update");
            Console.WriteLine("Goes through all the replays since the application was last run and analyzes them as if they are new. After this the application will continue as per usual.");
            Console.WriteLine("If this flag is not present the replay files that were created while the application wasn't running are ignored forever.");
            Console.WriteLine("Example:");
            Console.WriteLine("RocketLeagueReplayTracker.exe -u");
            Console.WriteLine();
            Console.WriteLine("-d, -directory");
            Console.WriteLine("Gives the application a different location to search for replays in.");
            Console.WriteLine("Without this flag the application will always search in \"(Current User)\\My Games\\Rocket League\\TAGame\\Demos\" for replay files.");
            Console.WriteLine("Example:");
            Console.WriteLine("RocketLeagueReplayTracker.exe -d \"C:\\Program Files\\Rocket League\\Replay Folder\"");
            Console.WriteLine();
            Console.WriteLine("-g, -google");
            Console.WriteLine("Update the stored spreadsheet ID value used to find the Google Spreadsheet.");
            Console.WriteLine("Example:");
            Console.WriteLine("RocketLeagueReplayTracker.exe -g 1nl4CmfsGxcaC5OuFVw4hn0Ig8i3Dhcr1Ui-AUANcLt9");
            Console.WriteLine();
            Console.WriteLine("-w, -wait");
            Console.WriteLine("The amount of time to wait before checking for another replay file, in milliseconds. Defaults to 30000 (30 seconds).");
            Console.WriteLine("Increasing this value could lead to performance hits.");
            Console.WriteLine("Example:");
            Console.WriteLine("RocketLeagueReplayTracker.exe -w 10000");
        }
    }
}