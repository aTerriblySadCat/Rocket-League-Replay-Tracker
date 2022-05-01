using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class ClassNetCache
    {
        private int objectIndex;
        private int parentId;
        private int id;
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
            foreach(ClassNetCacheProperty classNetCacheProperty in properties)
            {
                returnString += classNetCacheProperty + "\n";
            }

            return returnString;
        }
    }
}
