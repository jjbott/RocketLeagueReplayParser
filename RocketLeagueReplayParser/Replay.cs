using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class Replay
    {
        public static Replay Deserialize(string filePath)
        {
            using(var fs = new FileStream(filePath, FileMode.Open))
            using(var br = new BinaryReader(fs))
            {
                return Deserialize(br);
            }
        }

        public static Replay Deserialize(BinaryReader br)
        {
            var replay = new Replay();
            replay.Unknown1 = br.ReadInt32();
            replay.Unknown2 = br.ReadInt32();
            replay.Unknown3 = br.ReadInt32();
            replay.Unknown4 = br.ReadInt32();

            // This looks almost like an ArrayProperty, but without type and the unknown ints
            replay.Unknown5 = br.ReadAsciiString();
            replay.Properties = new List<Property>();
            Property prop;
            do
            {
                prop = Property.Deserialize(br);
                replay.Properties.Add(prop);
            }
            while (prop.Name != "None");

            replay.LengthOfRemainingData = br.ReadInt32();
            replay.Unknown7 = br.ReadInt32();
            replay.LevelLength = br.ReadInt32();

            // looks like sfx data, not level data. shrug
            replay.Levels = new List<Level>();
            for (int i = 0; i < replay.LevelLength; i++ )
            {
                replay.Levels.Add(Level.Deserialize(br));
            }

            replay.KeyFrameLength = br.ReadInt32();
            replay.KeyFrames = new List<KeyFrame>();
            for (int i = 0; i < replay.KeyFrameLength; i++)
            {
                replay.KeyFrames.Add(KeyFrame.Deserialize(br));
            }

            replay.NetworkStreamLength = br.ReadInt32();
            replay.NetworkStream = new List<byte>();
            for (int i = 0; i < replay.NetworkStreamLength; ++i)
            {
                replay.NetworkStream.Add(br.ReadByte());
            }

            replay.DebugStringLength = br.ReadInt32();
            replay.DebugStrings = new List<string>();
            for (int i = 0; i < replay.DebugStringLength; i++)
            {
                replay.DebugStrings.Add(br.ReadAsciiString());
            }

            replay.TickMarkLength = br.ReadInt32();
            replay.TickMarks = new List<TickMark>();
            for (int i = 0; i < replay.TickMarkLength; i++)
            {
                replay.TickMarks.Add(TickMark.Deserialize(br));
            }

            replay.PackagesLength = br.ReadInt32();
            replay.Packages = new List<string>();
            for (int i = 0; i < replay.PackagesLength; i++)
            {
                replay.Packages.Add(br.ReadAsciiString());
            }

            replay.ObjectLength = br.ReadInt32();
            replay.Objects = new List<string>();
            for (int i = 0; i < replay.ObjectLength; i++)
            {
                replay.Objects.Add(br.ReadAsciiString());
            }

            //replay.Unknown9 = br.ReadInt32();

            replay.NamesLength = br.ReadInt32();
            replay.Names = new List<string>();
            for (int i = 0; i < replay.NamesLength; i++)
            {
                replay.Names.Add(br.ReadAsciiString());
            }

            replay.ClassIndexLength = br.ReadInt32();
            replay.ClassIndexes = new List<ClassIndex>();
            for (int i = 0; i < replay.ClassIndexLength; i++)
            {
                replay.ClassIndexes.Add(ClassIndex.Deserialize(br));
            }

            //class net cache map. 
            var len = br.ReadInt32();
            for (int i = 0; i < len; ++i )
            {
                br.ReadInt32();
                br.ReadInt32();
            }

            Console.WriteLine(br.BaseStream.Position);
            Console.WriteLine(br.ReadInt32());

                /*
                replay.Unknown7 = new List<byte>();
                for (int i = 0; i < replay.LengthOfRemainingData; ++i )
                {
                    replay.Unknown7.Add(br.ReadByte());
                }*/

                replay.Unknown8 = br.ReadInt32();

            if ( br.BaseStream.Position != br.BaseStream.Length )
            {
                throw new Exception("Extra data somewhere!");
            }

            return replay;
        }

        public Int32 Unknown1 { get; private set; }
        public Int32 Unknown2 { get; private set; }
        public Int32 Unknown3 { get; private set; }
        public Int32 Unknown4 { get; private set; }
        public string Unknown5 { get; private set; }
        public List<Property> Properties { get; private set; }
        public Int32 LengthOfRemainingData { get; private set; }
        public Int32 Unknown7 { get; private set; } // crc?
        public Int32 LevelLength { get; private set; }
        public List<Level> Levels { get; private set; }
        public Int32 KeyFrameLength { get; private set; }
        public List<KeyFrame> KeyFrames { get; private set; }
        public Int32 NetworkStreamLength { get; private set; }
        public List<byte> NetworkStream { get; private set; }
        public Int32 DebugStringLength { get; private set; }
        public List<string> DebugStrings { get; private set; }
        public Int32 TickMarkLength { get; private set; }
        public List<TickMark> TickMarks { get; private set; }
        public Int32 PackagesLength { get; private set; }
        public List<string> Packages { get; private set; }

        public Int32 ObjectLength { get; private set; }
        public List<string> Objects { get; private set; } // Dictionary<int,string> might be better, since we'll need to look up by index
    //    - Array of strings for the Object table. Whenever a persistent object gets referenced for the network stream its path gets added to this array. Then its index in this array is used in the network stream.

        //public Int32 Unknown9 { get; private set; }
        public Int32 NamesLength { get; private set; }
        public List<string> Names { get; private set; } // Dictionary<int,string> might be better, since we'll need to look up by index
//- Array of strings for the Name table. "Names" are commonly used strings that get assigned an integer for use in the network stream.


        public Int32 ClassIndexLength { get; private set; }
        public List<ClassIndex> ClassIndexes { get; private set; } // Dictionary<int,string> might be better, since we'll need to look up by index
//- Map of string, integer pairs for the Class Index Map. Whenever a class is used in the network stream it is given an integer id by this map.


        //public List<byte> Unknown7 { get; private set; }
        public Int32 Unknown8 { get; private set; }


    }
}
