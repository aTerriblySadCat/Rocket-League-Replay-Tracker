using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class ClassIndex
    {
        /// <summary>
        /// The name of the class.
        /// </summary>
        private string? className;
        /// <summary>
        /// The index of the class.
        /// </summary>
        private int index;

        public void Deserialize(BinaryReader binaryReader)
        {
            className = binaryReader.ReadLongString();
            index = binaryReader.ReadInt32();
        }

        public override string ToString()
        {
            string returnString = "Class Index Name: " + className + "\n";
            returnString += "Class Index: " + index + "\n";

            return returnString;
        }
    }
}
