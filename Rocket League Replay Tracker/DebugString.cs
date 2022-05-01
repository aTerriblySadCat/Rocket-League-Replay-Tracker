using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class DebugString
    {
        private int frameNumber;
        private string? username;
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
