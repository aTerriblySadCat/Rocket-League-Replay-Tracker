using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    public class TickMark
    {
        /// <summary>
        /// The type of tick mark.
        /// </summary>
        public string? type;
        /// <summary>
        /// The frame at which the tick mark is set.
        /// </summary>
        public int frame;

        public TickMark() { }

        public void Deserialize(BinaryReader binaryReader)
        {
            type = binaryReader.ReadLongString();
            frame = binaryReader.ReadInt32();
        }
    }
}
