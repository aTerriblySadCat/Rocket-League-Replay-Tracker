using System;
using System.Xml;
using System.Xml.Serialization;

namespace Rocket_League_Replay_Tracker
{
    internal class XmlManager
    {
        /// <summary>
        /// Either gets the config found at location {Current Directory}\\config.xml or creates a new one. Returns the Config object that was loaded in or created.
        /// </summary>
        /// <param name="args">The command line arguments containing flags and their parameters.</param>
        /// <param name="replayFolderPath">The path to the folder containing the replays.</param>
        /// <param name="replayFileDictionary">A Dictionary filled with all the replay files found in the folder.</param>
        /// <param name="googleFlag">The Google flag index.</param>
        /// <param name="updateFlag">The Update flag index.</param>
        /// <param name="directoryFlag">The Directory flag index.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Config GetConfig(string[] args, string replayFolderPath, Dictionary<string, DateTime> replayFileDictionary, int googleFlag, int updateFlag, int directoryFlag)
        {
            Config? config = new Config();
            XmlSerializer serializer = new XmlSerializer(config.GetType());
            if (File.Exists("config.xml"))
            {
                // Load existing config file
                using (FileStream fileStream = new FileStream("config.xml", FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    Config? tempConfig = serializer.Deserialize(fileStream) as Config;
                    if (tempConfig != null)
                    {
                        config = tempConfig;
                        // TODO - Update values when flags are set and write them to file
                        if (googleFlag != -1)
                        {
                            string newSpreadSheetId = args[googleFlag + 1];
                            if (!GoogleSheetsManager.DoesSpreadSheetExist(newSpreadSheetId))
                            {
                                throw new ArgumentException("Spreadsheet with ID (" + newSpreadSheetId + ") does not exist!\nPlease enter a valid Spreadsheet ID.");
                            }

                            config.googleSpreadSheetId = newSpreadSheetId;
                        }
                        if (config.replayDirectory != replayFolderPath)
                        {
                            string newDirectory = args[directoryFlag + 1];
                            if (!Directory.Exists(newDirectory))
                            {
                                throw new ArgumentException("The folder (" + replayFolderPath + ") does not exist!\nPlease choose a valid path using the -d, -directory flags.\nSee -h, -help for more info."); ;
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Could read the local XML file 'config.xml'.\nPlease delete the file and run the application again.");
                    }
                }
            }
            else
            {
                // Create new config file
                string newSpreadSheetId = "";
                if (googleFlag == -1)
                {
                    throw new ArgumentException("No Google Spreadsheet ID found!\nPlease enter a valid Spreadsheet ID using the -g or -google flag.\nUse -h or -help for more information.");
                }
                else
                {
                    newSpreadSheetId = args[googleFlag + 1];
                    if (!GoogleSheetsManager.DoesSpreadSheetExist(newSpreadSheetId))
                    {
                        throw new ArgumentException("Spreadsheet with ID (" + newSpreadSheetId + ") does not exist!\nPlease enter a valid Spreadsheet ID.");
                    }
                }

                string lastReplayFile = "";
                if (replayFileDictionary.Count > 0 && updateFlag == -1)
                {
                    lastReplayFile = replayFileDictionary.Keys.Last();
                }

                string newReplayDirectory = replayFolderPath;

                using (FileStream fileStream = new FileStream("config.xml", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    config = new Config();
                    config.googleSpreadSheetId = newSpreadSheetId;
                    config.lastReplayFileName = lastReplayFile;
                    config.replayDirectory = newReplayDirectory;
                    serializer.Serialize(fileStream, config);
                }
            }

            return config;
        }

        /// <summary>
        /// Write the given Config object to the {Current Directory}\\config.xml file.
        /// </summary>
        /// <param name="config">The Config object with the values to write.</param>
        public static void WriteConfig(Config config)
        {
            XmlSerializer serializer = new XmlSerializer(config.GetType());
            using (FileStream fileStream = new FileStream("config.xml", FileMode.Open, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(fileStream, config);
            }
        }
    }
}
