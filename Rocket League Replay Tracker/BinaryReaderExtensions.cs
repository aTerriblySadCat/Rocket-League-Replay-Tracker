using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    public static class BinaryReaderExtensions
    {
        public static string ReadLongString(this BinaryReader binaryReader)
        {
            int stringLength = binaryReader.ReadInt32();
            if (stringLength > 0)
            {
                byte[] bytes = binaryReader.ReadBytes(stringLength);
                return Encoding.ASCII.GetString(bytes, 0, stringLength - 1);
            }
            else if(stringLength < 0)
            {
                byte[] bytes = binaryReader.ReadBytes(stringLength * -2);
                return Encoding.Unicode.GetString(bytes, 0, (stringLength * -2) - 2);
            }

            return "";
        }
    }
}
