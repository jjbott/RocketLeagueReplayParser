using System;
using System.Collections;
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
            using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
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


            foreach(var kf in replay.KeyFrames)
            {
                Console.WriteLine(kf.ToDebugString());
            }

            // break into frames, using best guesses
            List<Frame> frames = ExtractFrames(replay.NetworkStream);
            foreach(var f in frames)
            {
                Console.WriteLine(f.ToDebugString());
            }
            

            if ( br.BaseStream.Position != br.BaseStream.Length )
            {
                throw new Exception("Extra data somewhere!");
            }

            return replay;
        }

        private static List<Frame> ExtractFrames(IEnumerable<byte> networkStream)
        {
            var ba = new BitArray(networkStream.ToArray());
            List<Frame> frames = new List<Frame>();
            int frameStart = 0, curPos = 8;
            float lastTime = BitConverter.ToSingle(networkStream.ToArray(), 0);
            float lastDelta = BitConverter.ToSingle(networkStream.ToArray(), 4);
            while (curPos < (ba.Length - 64))
            {
                var candidateBits = new bool[64];
                var candidateBytes = new byte[8];
                for (int x = 0; x < 64; ++x)
                {
                    candidateBits[x] = ba[curPos + x];
                }
                var candidateBitArray = new BitArray(candidateBits);
                candidateBitArray.CopyTo(candidateBytes, 0);
                var candidateTime = BitConverter.ToSingle(candidateBytes, 0);
                var candidateDelta = BitConverter.ToSingle(candidateBytes, 4);
                var actualDelta = candidateTime - lastTime;
                if (candidateTime > lastTime && candidateTime < (lastTime + .5) && (Math.Abs(actualDelta - candidateDelta) < 0.000005))
                {
                    // we found the start of the next frame maybe! woot.
                    var frameBits = new bool[curPos - frameStart];
                    for (int x = 0; x < (curPos - frameStart); ++x)
                    {
                        frameBits[x] = ba[frameStart + x];
                    }

                    frames.Add(Frame.Deserialize(frameStart, frameBits));

                    Console.WriteLine(string.Format("Found frame at position {0} with time {1} and delta {2}, actual delta {3}, delta diff {4}. Prev frame size is {5} bits", curPos, candidateTime, candidateDelta, actualDelta, (actualDelta - candidateDelta).ToString("F7"), (curPos - frameStart)));

                    lastTime = candidateTime;
                    lastDelta = candidateDelta;

                    frameStart = curPos;
                    curPos = frameStart + 8;

                }
                else
                {
                    curPos++;
                }
            }

            // TODO: this doesnt return the last frame right now, but we're not finding them all anyways yet

            return frames;
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
