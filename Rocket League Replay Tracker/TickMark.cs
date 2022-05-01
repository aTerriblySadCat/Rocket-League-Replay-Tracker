using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class TickMark
    {
        private string? type;
        private int frame;

        public void Deserialize(BinaryReader binaryReader)
        {
            type = binaryReader.ReadLongString();
            frame = binaryReader.ReadInt32();
        }
    }
}
