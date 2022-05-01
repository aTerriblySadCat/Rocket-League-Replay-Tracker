using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class ClassNetCacheProperty
    {
        private int index;
        private int id;

        public void Deserialize(BinaryReader binaryReader)
        {
            index = binaryReader.ReadInt32();
            id = binaryReader.ReadInt32();
        }

        public override string ToString()
        {
            string returnString = "Class Net Cache Property Index: " + index + "\n";
            returnString += "Class Net Cache Property ID: " + id + "\n";

            return returnString;
        }
    }
}
