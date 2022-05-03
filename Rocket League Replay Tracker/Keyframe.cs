using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    public class Keyframe
    {
        /// <summary>
        /// The time of the keyframe.
        /// </summary>
        public float time;
        /// <summary>
        /// The frame that is the keyframe.
        /// </summary>
        public int frame;
        /// <summary>
        /// The position of the keyframe.
        /// </summary>
        public int position;

        public Keyframe() { }

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
