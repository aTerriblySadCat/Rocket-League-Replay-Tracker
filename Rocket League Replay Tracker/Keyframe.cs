using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class Keyframe
    {
        private float time;
        private int frame;
        private int position;

        public void Deserialize(BinaryReader binaryReader)
        {
            time = binaryReader.ReadSingle();
            frame = binaryReader.ReadInt32();
            position = binaryReader.ReadInt32();
        }

        public override string ToString()
        {
            string returnString = "Keyframe Time: " + time + "\n";
            returnString += "Keyframe Frame: " + frame + "\n";
            returnString += "Keyframe Position: " + position + "\n";

            return returnString;
        }
    }
}
