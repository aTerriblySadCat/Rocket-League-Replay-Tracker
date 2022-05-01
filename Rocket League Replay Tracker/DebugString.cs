using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class DebugString
    {
        /// <summary>
        /// The frame number at which the DebugString was written.
        /// </summary>
        private int frameNumber;
        /// <summary>
        /// The username belonging to the DebugString.
        /// </summary>
        private string? username;
        /// <summary>
        /// The DebugString text.
        /// </summary>
        private string? text;

        public void Deserialize(BinaryReader binaryReader)
        {
            frameNumber = binaryReader.ReadInt32();
            username = binaryReader.ReadLongString();
            text = binaryReader.ReadLongString();
        }

        public override string ToString()
        {
            string returnString = "Debug String Frame Number: " + frameNumber + "\n";
            returnString += "Debug String Username: " + username + "\n";
            returnString += "Debug String Text: " + text + "\n";

            return returnString;
        }
    }
}
