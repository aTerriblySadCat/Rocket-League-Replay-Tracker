using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    public class Property
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string? name;
        /// <summary>
        /// The type of the property.
        /// </summary>
        public string? type;
        /// <summary>
        /// The length of the property.
        /// </summary>
        public int length;
        /// <summary>
        /// An unknown value.
        /// </summary>
        public int unknown;

        /// <summary>
        /// Used by type ArrayProperty.
        /// </summary>
        public List<Property>? valueArray;
        /// <summary>
        /// Used by type IntProperty.
        /// </summary>
        public int valueInt;
        /// <summary>
        /// Used by types StrProperty and NameProperty.
        /// </summary>
        public string? valueString;
        /// <summary>
        /// Used by type FloatProperty.
        /// </summary>
        public float valueFloat;
        /// <summary>
        /// Used by type ByteProperty.
        /// </summary>
        public string? valueBytePropertyType;
        /// <summary>
        /// Used by type ByteProperty.
        /// </summary>
        public string? valueBytePropertyValue;
        /// <summary>
        /// Used by type BoolProperty.
        /// </summary>
        public byte valueByte;
        /// <summary>
        /// Used by type QWordProperty.
        /// </summary>
        public long valueLong;

        public Property() { }
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

        public string? GetName()
        {
            return name;
        }

        public new string? GetType()
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

        /// <summary>
        /// Gets the value the property holds.
        /// Can be either List<Property>, int, string, float, string[2], byte, or long.
        /// </summary>
        /// <returns></returns>
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
                if (valueBytePropertyType != null && valueBytePropertyValue != null)
                {
                    return new string[] { valueBytePropertyType, valueBytePropertyValue };
                }

                return null;
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

            if (type == "ArrayProperty" && valueArray != null)
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
