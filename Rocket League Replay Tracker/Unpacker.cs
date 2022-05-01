using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal class Unpacker
    {
        // Header data
        /// <summary>
        /// The length of header, in bytes.
        /// </summary>
        private int headerLength;
        /// <summary>
        /// The header's CRC.
        /// </summary>
        private uint headerCrc;
        /// <summary>
        /// The version of the engine.
        /// </summary>
        private uint engineVersion;
        /// <summary>
        /// The licensee version.
        /// </summary>
        private uint licenseeVersion;
        /// <summary>
        /// The net version.
        /// Only used when engineVersion >= 868 and licenseeVersion >= 18
        /// </summary>
        private uint netVersion;
        /// <summary>
        /// A static string.
        /// </summary>
        private string? taGame;
        /// <summary>
        /// A list of properties.
        /// </summary>
        private List<Property>? properties;

        // Body data
        /// <summary>
        /// The length of the body, in bytes.
        /// </summary>
        private int bodyLength;
        /// <summary>
        /// The body's CRC.
        /// </summary>
        private uint bodyCrc;
        /// <summary>
        /// A list of level names.
        /// </summary>
        private List<string>? levels;
        /// <summary>
        /// A list of keyframes.
        /// </summary>
        private List<Keyframe>? keyframes;
        /// <summary>
        /// The network stream.
        /// </summary>
        private byte[]? networkStream;
        /// <summary>
        /// A list of debug strings.
        /// </summary>
        private List<DebugString>? debugStrings;
        /// <summary>
        /// A list of tick marks.
        /// </summary>
        private List<TickMark>? tickMarks;
        /// <summary>
        /// A list of packages used.
        /// </summary>
        private List<string>? packages;
        /// <summary>
        /// A list of objects used.
        /// </summary>
        private List<string>? objects;
        /// <summary>
        /// Any strings that need to be used by the replay.
        /// </summary>
        private List<string>? names;
        /// <summary>
        /// A list of class indices.
        /// </summary>
        private List<ClassIndex>? classIndices;
        /// <summary>
        /// A list of class net caches.
        /// </summary>
        private List<ClassNetCache>? classNetCaches;

        // TODO - Extend with frame data

        /// <summary>
        /// Unpacks the replay file at the given replayFilePath. Fills the class's variables with the values found in the replay file.
        /// </summary>
        /// <param name="replayFilePath"></param>
        public Unpacker(string replayFilePath)
        {
            using (FileStream fileStream = new FileStream(replayFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    GetHeaderData(binaryReader);
                    GetBodyData(binaryReader);
                }
            }
        }

        /// <summary>
        /// Get the replay file's header data.
        /// </summary>
        /// <param name="binaryReader"></param>
        private void GetHeaderData(BinaryReader binaryReader)
        {
            headerLength = binaryReader.ReadInt32();
            headerCrc = binaryReader.ReadUInt32();
            engineVersion = binaryReader.ReadUInt32();
            licenseeVersion = binaryReader.ReadUInt32();

            if (engineVersion >= 868 && licenseeVersion >= 18)
            {
                netVersion = binaryReader.ReadUInt32();
            }

            taGame = binaryReader.ReadLongString();

            properties = new List<Property>();
            string? propertyName;
            while((propertyName = binaryReader.ReadLongString()) != "None")
            {
                Property property = new Property(propertyName);
                property.Deserialize(binaryReader);
                properties.Add(property);
            }
        }

        /// <summary>
        /// Get the replay file's body data.
        /// </summary>
        /// <param name="binaryReader"></param>
        private void GetBodyData(BinaryReader binaryReader)
        {
            bodyLength = binaryReader.ReadInt32();
            bodyCrc = binaryReader.ReadUInt32();

            int levelCount = binaryReader.ReadInt32();
            levels = new List<string>(levelCount);
            for(int i = 0; i < levelCount; i++)
            {
                levels.Add(binaryReader.ReadLongString());
            }

            int keyframesCount = binaryReader.ReadInt32();
            keyframes = new List<Keyframe>(keyframesCount);
            for(int i = 0; i < keyframesCount; i++)
            {
                Keyframe keyframe = new Keyframe();
                keyframe.Deserialize(binaryReader);
                keyframes.Add(keyframe);
            }

            int networkStreamLength = binaryReader.ReadInt32();
            networkStream = binaryReader.ReadBytes(networkStreamLength);

            int debugStringCount = binaryReader.ReadInt32();
            debugStrings = new List<DebugString>(debugStringCount);
            for(int i = 0; i < debugStringCount; i++)
            {
                DebugString debugString = new DebugString();
                debugString.Deserialize(binaryReader);
                debugStrings.Add(debugString);
            }

            int tickMarkCount = binaryReader.ReadInt32();
            tickMarks = new List<TickMark>(tickMarkCount);
            for(int i = 0; i < tickMarkCount; i++)
            {
                TickMark tickMark = new TickMark();
                tickMark.Deserialize(binaryReader);
                tickMarks.Add(tickMark);
            }

            int packageCount = binaryReader.ReadInt32();
            packages = new List<string>(packageCount);
            for(int i = 0; i < packageCount; i++)
            {
                packages.Add(binaryReader.ReadLongString());
            }

            int objectCount = binaryReader.ReadInt32();
            objects = new List<string>(objectCount);
            for(int i = 0; i < objectCount; i++)
            {
                objects.Add(binaryReader.ReadLongString());
            }

            int nameCount = binaryReader.ReadInt32();
            names = new List<string>(nameCount);
            for(int i = 0; i < nameCount; i++)
            {
                names.Add(binaryReader.ReadLongString());
            }

            int classIndexCount = binaryReader.ReadInt32();
            classIndices = new List<ClassIndex>(classIndexCount);
            for(int i = 0; i < classIndexCount; i++)
            {
                ClassIndex classIndex = new ClassIndex();
                classIndex.Deserialize(binaryReader);
                classIndices.Add(classIndex);
            }

            int classNetCacheCount = binaryReader.ReadInt32();
            classNetCaches = new List<ClassNetCache>(classNetCacheCount);
            for(int i = 0; i < classNetCacheCount; i++)
            {
                ClassNetCache classNetCache = new ClassNetCache();
                classNetCache.Deserialize(binaryReader);
                classNetCaches.Add(classNetCache);
            }

            // TODO
            // Interpret the frame data here for further and more advanced analysis
        }

        public int GetHeaderLength()
        {
            return headerLength;
        }

        public uint GetHeaderCrc()
        {
            return headerCrc;
        }

        public uint GetEngineVerion()
        {
            return engineVersion;
        }

        public uint GetLicenseeVersion()
        {
            return licenseeVersion;
        }

        public uint GetNetVersion()
        {
            return netVersion;
        }

        public string? GetTAGame()
        {
            return taGame;
        }

        public List<Property> GetProperties()
        {
            if (properties != null)
            {
                return new List<Property>(properties);
            }

            return new List<Property>();
        }

        public int GetBodyLength()
        {
            return bodyLength;
        }

        public uint GetBodyCrc()
        {
            return bodyCrc;
        }

        public List<string> GetLevels()
        {
            if(levels != null)
            {
                return new List<string>(levels);
            }

            return new List<string>();
        }

        public List<Keyframe> GetKeyframes()
        {
            if(keyframes != null)
            {
                return new List<Keyframe>(keyframes);
            }

            return new List<Keyframe>();
        }

        public byte[]? GetNetworkStream()
        {
            return networkStream;
        }

        public List<DebugString> GetDebugStrings()
        {
            if(debugStrings != null)
            {
                return new List<DebugString>(debugStrings);
            }

            return new List<DebugString>();
        }

        public List<TickMark> GetTickMarks()
        {
            if(tickMarks != null)
            {
                return new List<TickMark>(tickMarks);
            }

            return new List<TickMark>();
        }

        public List<string> GetPackages()
        {
            if(packages != null)
            {
                return new List<string>(packages);
            }

            return new List<string>();
        }

        public List<string> GetObjects()
        {
            if(objects != null)
            {
                return new List<string>(objects);
            }

            return new List<string>();
        }

        public List<string> GetNames()
        {
            if(names != null)
            {
                return new List<string>(names);
            }

            return new List<string>();
        }

        public List<ClassIndex> GetClassIndices()
        {
            if(classIndices != null)
            {
                return new List<ClassIndex>(classIndices);
            }

            return new List<ClassIndex>();
        }

        public List<ClassNetCache> GetClassNetCache()
        {
            if(classNetCaches != null)
            {
                return new List<ClassNetCache>(classNetCaches);
            }

            return new List<ClassNetCache>();
        }

        public override string ToString()
        {
            string returnString = "Header Length: " + headerLength + "\n";
            returnString += "Header CRC: " + headerCrc + "\n";
            returnString += "Engine Version: " + engineVersion + "\n";
            returnString += "Licensee Version: " + licenseeVersion + "\n";
            returnString += "Net Version: " + netVersion + "\n";
            returnString += "TA Game: " + taGame + "\n\n";

            if (properties != null)
            {
                foreach (Property property in properties)
                {
                    returnString += property + "\n";
                }
            }

            returnString += "Body Length: " + bodyLength + "\n";
            returnString += "Body CRC: " + bodyCrc + "\n";
            if (levels != null)
            {
                foreach (string level in levels)
                {
                    returnString += "\nLevel: " + level + "\n";
                }
            }
            if (keyframes != null)
            {
                foreach (Keyframe keyframe in keyframes)
                {
                    returnString += "\n" + keyframe.ToString() + "\n";
                }
            }
            if (networkStream != null)
            {
                returnString += "\nNetwork Stream Length: " + networkStream.Length + "\n";
            }
            if (debugStrings != null)
            {
                foreach (DebugString debugString in debugStrings)
                {
                    returnString += "\n" + debugString + "\n";
                }
            }
            if (tickMarks != null)
            {
                foreach (TickMark tickMark in tickMarks)
                {
                    returnString += "\n" + tickMark + "\n";
                }
            }
            if (packages != null)
            {
                foreach (string package in packages)
                {
                    returnString += "\nPackage: " + package + "\n";
                }
            }
            if (objects != null)
            {
                foreach (string obj in objects)
                {
                    returnString += "\nObject: " + obj + "\n";
                }
            }
            if (names != null)
            {
                foreach (string name in names)
                {
                    returnString += "\nName: " + name + "\n";
                }
            }
            if (classIndices != null)
            {
                foreach (ClassIndex classIndex in classIndices)
                {
                    returnString += "\n" + classIndex + "\n";
                }
            }
            if (classNetCaches != null)
            {
                foreach (ClassNetCache classNetCache in classNetCaches)
                {
                    returnString += "\n" + classNetCache + "\n";
                }
            }

            return returnString;
        }
    }
}
