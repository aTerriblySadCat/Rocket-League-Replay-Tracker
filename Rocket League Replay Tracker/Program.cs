using System;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace Rocket_League_Replay_Tracker
{
    internal class Program
    {
        private static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private static string ApplicationName = "Rocket League Replay Tracker";

        static void Main(string[] args)
        {
            if(args.Length == 0 || args[0].StartsWith("-"))
            {
                Console.WriteLine("No valid sheet ID found!");
                Console.WriteLine("Use -h or -help to see how the application is supposed to be used.");
                return;
            }

            List<string> commands = new List<string>();
            int helpFlagIndex = Array.IndexOf(args, "-h");
            if(helpFlagIndex == -1)
            {
                helpFlagIndex = Array.IndexOf(args, "-help");
            }

            if (helpFlagIndex != -1)
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
                Console.WriteLine("-s, -single");
                Console.WriteLine("Gives the application a single replay file to analyze after which it will shut down.");
                Console.WriteLine("Example:");
                Console.WriteLine("RocketLeagueReplayTracker.exe -s \"C:\\Users\\Me\\My Games\\Rocket League\\TAGame\\Demos\\ReplayFile.replay\"");
                Console.WriteLine();
                Console.WriteLine("-g, -google");
                Console.WriteLine("WIP.");
            }
            else
            {
                UserCredential userCredential;

                using(FileStream fileStream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(fileStream).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credentials saved to: " + credPath);
                }

                SheetsService service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = userCredential,
                    ApplicationName = ApplicationName,
                });

                string spreadSheetId = args[0];

                int playerFlagIndex = Array.IndexOf(args, "-p");
                if(playerFlagIndex == -1)
                {
                    playerFlagIndex = Array.IndexOf(args, "-players");
                }

                List<string>? playersToTrack = null;
                if(playerFlagIndex != -1)
                {
                    playersToTrack = new List<string>();
                    for(int i = playerFlagIndex; i < args.Length; i++)
                    {
                        if(!args[i].StartsWith("-"))
                        {
                            playersToTrack.Add(args[i]);
                            continue;
                        }

                        break;
                    }
                }

                int updateFlag = Array.IndexOf(args, "-u");
                if(updateFlag == -1)
                {
                    updateFlag = Array.IndexOf(args, "-update");
                }

                int directoryFlag = Array.IndexOf(args, "-d");
                if(directoryFlag == -1)
                {
                    directoryFlag = Array.IndexOf(args, "-directory");
                }

                int singleFlag = Array.IndexOf(args, "-s");
                if(singleFlag == -1)
                {
                    singleFlag = Array.IndexOf(args, "-single");
                }

                // TODO - Remove this and replace with going through all files
                string replayFilePath = @"C:\Users\Martijn van den Berk\Documents\My Games\Rocket League\TAGame\Demos\67015F844585B57C57C5C09B9D2E69DC.replay";
                Unpacker unpacker = new Unpacker(replayFilePath);

                Console.WriteLine(unpacker.ToString());

                List<Property> properties = unpacker.GetProperties();
                Property? playerStatsProperty = properties.Find(x => x.GetName() == "PlayerStats" && x.GetType() == "ArrayProperty");
                if (playerStatsProperty == null)
                {
                    throw new ArgumentException("Could not find any Goals data!");
                }

                List<Property>? playerStatsProperties = playerStatsProperty.GetValue();
                if (playerStatsProperties != null)
                {
                    List<PlayerStats> playersStatsToTrack = new List<PlayerStats>(playerStatsProperties.Count);
                    PlayerStats playerStats = new PlayerStats();
                    for(int i = 0; i < playerStatsProperties.Count; i += 10)
                    {
                        playerStats.name = playerStatsProperties[i].GetValue();
                        playerStats.platform = playerStatsProperties[i + 1].GetValue()[0];
                        playerStats.platformName = playerStatsProperties[i + 1].GetValue()[1];
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

                    if(playersToTrack != null)
                    {
                        playersStatsToTrack = playersStatsToTrack.FindAll(x => playersToTrack.Contains(x.name));
                    }

                    // TODO - Do all the three below things inside the loop
                    // Since replays aren't created that often it isn't that much overhead

                    // TODO - Find if RocketLeagueStats sheet already exists
                    // If not, create sheet
                    
                    // TODO - Check if first cell of first row (A1) has the value 'Name' in it
                    // If not, add the information column

                    // TODO - Find the first empty cell
                    // Keep getting cells until the lowest is found
                    // Do this inside the loop in case people change the sheet during runtime

                    string range = "RocketLeagueStats!A1";
                    ValueRange valueRange = new ValueRange();
                    valueRange.MajorDimension = "ROWS";
                    List<object> texts = new List<object>() { "Name", "Team ID", "Score", "Goals", "Assists", "Saves", "Shots" };
                    valueRange.Values = new List<IList<object>>() { texts };

                    SpreadsheetsResource.ValuesResource.UpdateRequest request = service.Spreadsheets.Values.Update(valueRange, spreadSheetId, range);
                    request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                    request.Execute();

                    int rowNumber = 1;
                    foreach (PlayerStats updatePlayerStats in playersStatsToTrack)
                    {
                        rowNumber += 1;
                        range = "RocketLeagueStats!A" + rowNumber;
                        valueRange = new ValueRange();
                        valueRange.MajorDimension = "ROWS";
                        texts = new List<object>() { updatePlayerStats.name, updatePlayerStats.team, updatePlayerStats.score, updatePlayerStats.goals, updatePlayerStats.assists, updatePlayerStats.saves, updatePlayerStats.shots };
                        valueRange.Values = new List<IList<object>>() { texts };

                        request = service.Spreadsheets.Values.Update(valueRange, spreadSheetId, range);
                        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                        request.Execute();
                    }
                }
            }
        }
    }
}