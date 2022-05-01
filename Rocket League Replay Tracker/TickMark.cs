using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class TickMark
    {
        /// <summary>
        /// The type of tick mark.
        /// </summary>
        private string? type;
        /// <summary>
        /// The frame at which the tick mark is set.
        /// </summary>
        private int frame;

        public void Deserialize(BinaryReader binaryReader)
        {
            type = binaryReader.ReadLongString();
            frame = binaryReader.ReadInt32();
        }
    }
}
