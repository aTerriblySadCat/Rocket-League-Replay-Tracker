using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Rocket_League_Replay_Tracker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
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
                    int logFlag = GetFlagIndex(args, new string[] { "-l", "-log" });

                    if (!FlagIsValid(args, playerFlagIndex) || !FlagIsValid(args, directoryFlag) || !FlagIsValid(args, googleFlag))
                    {
                        throw new ArgumentException("Invalid flag found!\nUse -h, -help to get information on how to use the flags.");
                    }

                    // Setup connection with Google Sheets
                    GoogleSheetsManager.CreateService();
                    Console.WriteLine("Debug - Connected with Google service!");

                    // Default wait time is 30 seconds because of Google Sheets read/write quota
                    // 60 reads and 60 writes per user per minute
                    // Maximum reads per replay file are 18
                    // Maximum writes per replay is 27, though this is a very extreme scenario
                    int waitTime = 30000;

                    // Get the players to track from the command line
                    List<string>? playersToTrack = null;
                    if (playerFlagIndex != -1)
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
                    else if (updateFlag != -1)
                    {
                        // Delete files that are newer than the lastReplayFileName so they get updated
                        // If the lastReplayFileName can't be found it will just ignore the -u flag
                        bool delete = false;
                        List<string> fileNames = replayFileDictionary.Keys.ToList();
                        foreach (string fileName in fileNames)
                        {
                            if (fileName == config.lastReplayFileName)
                            {
                                delete = true;
                                continue;
                            }

                            if (delete)
                            {
                                replayFileDictionary.Remove(fileName);
                            }
                        }

                        if (!delete)
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
                        replayFileDictionary = MainLoop(replayFolderPath, replayFileDictionary, playersToTrack, spreadSheetId, config, logFlag);
                        Console.WriteLine("Debug - Waiting for timeout...");
                        Thread.Sleep(waitTime);
                    }
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR");
                Console.WriteLine(exc.Message);
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
        /// Returns the modified replayFileDictionary.
        /// </summary>
        /// <param name="replayFolderPath">The path to the folder containing the replays.</param>
        /// <param name="replayFileDictionary">The Dictionary containing the replay files that don't need to be analyzed.</param>
        /// <param name="playersToTrack">A list of player names to track. All other players are ignored.</param>
        /// <param name="spreadSheetId">The ID of the Google Sheets Spreadsheet to where the data is written.</param>
        /// <param name="config">The configuration class.</param>
        /// <exception cref="ArgumentException"></exception>
        private static Dictionary<string, DateTime> MainLoop(string replayFolderPath, Dictionary<string, DateTime> replayFileDictionary, List<string>? playersToTrack, string spreadSheetId, Config config, int logFlag)
        {
            foreach (string replayFileName in Directory.EnumerateFiles(replayFolderPath))
            {
                if (replayFileName.EndsWith(".replay") && !replayFileDictionary.ContainsKey(replayFileName))
                {
                    Console.WriteLine("Debug - New replay file found (" + replayFileName + ")");

                    DateTime currentDateTime = DateTime.UtcNow;
                    string gameId = "";
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(currentDateTime.ToLongDateString() + currentDateTime.ToLongTimeString()));
                        StringBuilder stringBuilder = new StringBuilder();
                        for (int i = 0; i < data.Length; i++)
                        {
                            stringBuilder.Append(data[i].ToString("x2"));
                        }
                        gameId = stringBuilder.ToString();
                    }

                    // Make sure the RL - Games sheet exists, and if not create it
                    string gamesSheet = "RL - Games";
                    if (!GoogleSheetsManager.DoesSheetExist(spreadSheetId, gamesSheet))
                    {
                        GoogleSheetsManager.CreateNewSheet(spreadSheetId, gamesSheet);
                        Console.WriteLine("Debug - Created a new sheet " + gamesSheet);
                    }

                    // New replay file found
                    DateTime lastWriteTime = File.GetLastWriteTimeUtc(replayFileName);
                    replayFileDictionary.Add(replayFileName, lastWriteTime);

                    replayFileDictionary = replayFileDictionary.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                    config.lastReplayFileName = replayFileDictionary.Keys.Last();
                    XmlManager.WriteConfig(config);

                    // Analyze File
                    Unpacker unpacker = new Unpacker(replayFileName);

                    if(logFlag != -1)
                    {
                        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blooper Troopers\\Rocket League Replay Tracker\\Logs\\";
                        if(!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        string logFileName = gameId;
                        using (FileStream fileStream = new FileStream(folderPath + gameId, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            XmlSerializer serializer = new XmlSerializer(unpacker.GetType());
                            serializer.Serialize(fileStream, unpacker);
                        }
                    }

                    // Get all properties
                    List<Property> properties = unpacker.GetProperties();

                    // Take out the PlayerStats properties
                    Property? playerStatsProperty = properties.Find(x => x.GetName() == "PlayerStats" && x.GetType() == "ArrayProperty");
                    if (playerStatsProperty == null)
                    {
                        throw new ArgumentException("Could not find any player stats!");
                    }

                    List<Property>? playerStatsProperties = playerStatsProperty.GetValue();
                    List<PlayerStats> playersStatsToTrack = new List<PlayerStats>();
                    if (playerStatsProperties != null)
                    {
                        // Put properties in PlayerStats structs for better readability
                        PlayerStats playerStats = new PlayerStats();
                        for (int i = 0; i < playerStatsProperties.Count; i += 10)
                        {
                            string? playerName = playerStatsProperties[i].GetValue();
                            if (playerName == null)
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
                    }

                    bool trackedPlayersFound = playersStatsToTrack.Count > 0 || playersToTrack == null;
                    if (trackedPlayersFound)
                    {
                        Property? team0ScoreProperty = properties.Find(x => x.GetName() == "Team0Score" && x.GetType() == "IntProperty");
                        Property? team1ScoreProperty = properties.Find(x => x.GetName() == "Team1Score" && x.GetType() == "IntProperty");
                        Property? goalsProperty = properties.Find(x => x.GetName() == "Goals" && x.GetType() == "ArrayProperty");
                        Property? playerNameProperty = properties.Find(x => x.GetName() == "PlayerName" && x.GetType() == "StrProperty");
                        Property? numFramesProperty = properties.Find(x => x.GetName() == "NumFrames" && x.GetType() == "IntProperty");

                        if (goalsProperty != null && playerNameProperty != null && numFramesProperty != null)
                        {
                            string? playerName = playerNameProperty.GetValue();
                            PlayerStats primaryPlayerStats = playersStatsToTrack.Find(x => x.name == playerName);
                            int primaryPlayerTeam = primaryPlayerStats.team;
                            int team0Score = 0;
                            if (team0ScoreProperty != null)
                            {
                                team0Score = team0ScoreProperty.GetValue();
                            }
                            int team1Score = 0;
                            if(team1ScoreProperty != null)
                            {
                                team1Score = team1ScoreProperty.GetValue();
                            }
                            List<Property>? goals = goalsProperty.GetValue();
                            if (goals != null && goals.Count > 2)
                            {
                                int numFrames = numFramesProperty.GetValue();
                                int lastGoalFrame = goals[goals.Count - 3].GetValue();
                                int firstGoalTeamId = goals[2].GetValue();

                                bool win = (primaryPlayerTeam == 0 && team0Score > team1Score) || (primaryPlayerTeam == 1 && team1Score > team0Score);
                                bool primaryPlayerTeamFirstGoal = primaryPlayerTeam == firstGoalTeamId;
                                bool overtime = (numFrames - lastGoalFrame) <= 100;

                                if (!GoogleSheetsManager.IsFirstRowTaken(spreadSheetId, gamesSheet))
                                {
                                    GoogleSheetsManager.InsertFirstGamesRow(spreadSheetId, gamesSheet);
                                    Console.WriteLine("Debug - Created first row in sheet " + gamesSheet);
                                }

                                List<object> appendGames = new List<object>() { gameId, win, primaryPlayerTeamFirstGoal, overtime };
                                GoogleSheetsManager.AppendValues(spreadSheetId, gamesSheet, appendGames);
                                Console.WriteLine("Debug - Appended games stats for game " + gameId + " to " + gamesSheet);
                            }
                        }

                        foreach (PlayerStats updatePlayerStats in playersStatsToTrack)
                        {
                            string playerSheetName = "RL - ";
                            if (updatePlayerStats.onlineId == 0)
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

                            List<object> appendPlayerStats = new List<object>() { updatePlayerStats.name, updatePlayerStats.onlineId.ToString(), updatePlayerStats.platformName, gameId, updatePlayerStats.team, updatePlayerStats.score, updatePlayerStats.goals, updatePlayerStats.assists, updatePlayerStats.saves, updatePlayerStats.shots };
                            GoogleSheetsManager.AppendValues(spreadSheetId, playerSheetName, appendPlayerStats);
                            Console.WriteLine("Debug - Appended player stats of player " + updatePlayerStats.name + " to " + playerSheetName);
                        }
                    }

                    break;
                }
            }

            return replayFileDictionary;
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
            Console.WriteLine("-l, -log");
            Console.WriteLine("Logs all data found in the replay file to a log file in the Application Data folder (%AppData%/Blooper Troopers/Rocket League Replay Tracker/Logs");
            Console.WriteLine("Example:");
            Console.WriteLine("RocketLeagueReplayTracker.exe -l");
        }
    }
}