using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    public class Config
    {
        /// <summary>
        /// The Google Sheets Spreadsheet ID to use for writing data to.
        /// </summary>
        public string? googleSpreadSheetId;
        /// <summary>
        /// The name of the newest replay file that was analyzed.
        /// </summary>
        public string? lastReplayFileName;
        /// <summary>
        /// The directory where in the replay files are located.
        /// </summary>
        public string? replayDirectory;
    }
}
