using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class ClassNetCache
    {
        /// <summary>
        /// The index of the object.
        /// </summary>
        private int objectIndex;
        /// <summary>
        /// The ID of the parent.
        /// </summary>
        private int parentId;
        /// <summary>
        /// The current ID.
        /// </summary>
        private int id;
        /// <summary>
        /// A list of properties.
        /// </summary>
        private List<ClassNetCacheProperty>? properties;

        public void Deserialize(BinaryReader binaryReader)
        {
            objectIndex = binaryReader.ReadInt32();
            parentId = binaryReader.ReadInt32();
            id = binaryReader.ReadInt32();
            int propertiesCount = binaryReader.ReadInt32();
            properties = new List<ClassNetCacheProperty>(propertiesCount);
            for(int i = 0; i < propertiesCount; i++)
            {
                ClassNetCacheProperty property = new ClassNetCacheProperty();
                property.Deserialize(binaryReader);
                properties.Add(property);
            }
        }

        public override string ToString()
        {
            string returnString = "Class Net Cache Object Index: " + objectIndex + "\n";
            returnString += "Class Net Cache Parent ID: " + parentId + "\n";
            returnString += "Class Net Cache ID: " + id + "\n";
            if (properties != null)
            {
                foreach (ClassNetCacheProperty classNetCacheProperty in properties)
                {
                    returnString += classNetCacheProperty + "\n";
                }
            }

            return returnString;
        }
    }
}
