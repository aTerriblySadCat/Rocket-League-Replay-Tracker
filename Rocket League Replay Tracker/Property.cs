using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class Property
    {
        private string? name;
        private string? type;
        private int length;
        private int unknown;

        private List<Property>? valueArray;
        private int valueInt;
        private string? valueString;
        private float valueFloat;
        private string? valueBytePropertyType;
        private string? valueBytePropertyValue;
        private byte valueByte;
        private long valueLong;

        public Property(string name)
        {
            this.name = name;
        }

        public void Deserialize(BinaryReader binaryReader)
        {
            if (name == null) 
            {
                name = binaryReader.ReadLongString();
            }

            type = binaryReader.ReadLongString();
            length = binaryReader.ReadInt32();
            unknown = binaryReader.ReadInt32();

            if (type == "ArrayProperty")
            {
                int arrayCount = binaryReader.ReadInt32();
                valueArray = new List<Property>(arrayCount);
                for (int i = 0; i < arrayCount; i++)
                {
                    string? propertyName;
                    while ((propertyName = binaryReader.ReadLongString()) != "None")
                    {
                        Property propertyValue = new Property(propertyName);
                        propertyValue.Deserialize(binaryReader);
                        valueArray.Add(propertyValue);
                    }
                }
            }
            else if (type == "IntProperty")
            {
                valueInt = binaryReader.ReadInt32();
            }
            else if (type == "StrProperty" || type == "NameProperty")
            {
                valueString = binaryReader.ReadLongString();
            }
            else if (type == "FloatProperty")
            {
                valueFloat = binaryReader.ReadSingle();
            }
            else if (type == "ByteProperty")
            {
                valueBytePropertyType = binaryReader.ReadLongString();
                valueBytePropertyValue = binaryReader.ReadLongString();
            }
            else if (type == "BoolProperty")
            {
                valueByte = binaryReader.ReadByte();
            }
            else if (type == "QWordProperty")
            {
                valueLong = binaryReader.ReadInt64();
            }
            else
            {
                throw new ArgumentException("Unknown property type found: " + type);
            }
        }

        public string GetName()
        {
            return name;
        }

        public string GetType()
        {
            return type;
        }

        public int GetLength()
        {
            return length;
        }

        public int GetUnknown()
        {
            return unknown;
        }

        public dynamic? GetValue()
        {
            if (type == "ArrayProperty")
            {
                return valueArray;
            }
            else if (type == "IntProperty")
            {
                return valueInt;
            }
            else if (type == "StrProperty" || type == "NameProperty")
            {
                return valueString;
            }
            else if (type == "FloatProperty")
            {
                return valueFloat;
            }
            else if (type == "ByteProperty")
            {
                return new string[] { valueBytePropertyType, valueBytePropertyValue };
            }
            else if (type == "BoolProperty")
            {
                return valueByte;
            }
            else if (type == "QWordProperty")
            {
                return valueLong;
            }

            return null;
        }

        public override string ToString()
        {
            string returnString = "Property Name: " + name + "\n";
            returnString += "Property Type: " + type + "\n";
            returnString += "Property Length: " + length + "\n";
            returnString += "Property Unknown: " + unknown + "\n";

            if (type == "ArrayProperty")
            {
                returnString += "\n\n";
                foreach(Property property in valueArray)
                {
                    returnString += property.ToString() + "\n";
                }
                returnString += "\n";
            }
            else if (type == "IntProperty")
            {
                returnString += "Property Value: " + valueInt + "\n";
            }
            else if (type == "StrProperty" || type == "NameProperty")
            {
                returnString += "Property Value: " + valueString + "\n";
            }
            else if (type == "FloatProperty")
            {
                returnString += "Property Value: " + valueFloat + "\n";
            }
            else if (type == "ByteProperty")
            {
                returnString += "Property Special Type: " + valueBytePropertyType + "\n";
                returnString += "Property Special Value: " + valueBytePropertyValue + "\n";
            }
            else if (type == "BoolProperty")
            {
                returnString += "Property Value: " + valueByte + "\n";
            }
            else if (type == "QWordProperty")
            {
                returnString += "Property Value: " + valueLong + "\n";
            }

            return returnString;
        }
    }
}
