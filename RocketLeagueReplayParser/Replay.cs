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
        public static Replay Deserialize(string filePath, out string log)
        {
            using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using(var br = new BinaryReader(fs))
            {
                return Deserialize(br, out log);
            }
        }

        public static Replay Deserialize(BinaryReader br, out string log)
        {
            var logSb = new StringBuilder();

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
            replay.Objects = new string[replay.ObjectLength];
            for (int i = 0; i < replay.ObjectLength; i++)
            {
                replay.Objects[i] = br.ReadAsciiString();
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

            replay.ClassNetCacheLength = br.ReadInt32();
            replay.ClassNetCaches = new ClassNetCache[replay.ClassNetCacheLength];
            for (int i = 0; i < replay.ClassNetCacheLength; i++)
            {
                replay.ClassNetCaches[i] = ClassNetCache.Deserialize(br);

                int j = 0;
                for(j = i-1; j >=0; --j)
                {
                    if ( replay.ClassNetCaches[i].ParentId == replay.ClassNetCaches[j].Id)
                    {
                        replay.ClassNetCaches[i].Parent = replay.ClassNetCaches[j];
                        replay.ClassNetCaches[j].Children.Add(replay.ClassNetCaches[i]);
                        break;
                    }
                }
                if ( j < 0 )
                {
                    replay.ClassNetCaches[i].Root = true;
                }
            }

            // break into frames, using best guesses
            var objectIndexToName = Enumerable.Range(0, replay.Objects.Length).ToDictionary(i => i, i => replay.Objects[i]);
            replay.Frames = ExtractFrames(replay.NetworkStream, replay.KeyFrames.Select(x => x.FilePosition), objectIndexToName, replay.ClassNetCaches, logSb);

            //var minSize = replay.Frames.Where(x => !x.Complete /*&& x.BitLength != 163*/ && x.ActorStates.Count > 0 && !string.IsNullOrWhiteSpace(x.ActorStates[0].TypeName)).Min(x => x.BitLength);
            foreach (var f in replay.Frames.Where(x => !x.Complete))// x.Failed) )//Complete && x.BitLength == minSize))
            {
                //if ( f.ActorStates.Count >= 1 && f.ActorStates.First().State == "New")
                //{
                    logSb.AppendLine(f.ToDebugString(replay.Objects));
                //}
            }
            
            //logSb.AppendLine(replay.Frames.First().ToDebugString(replay.Objects));

            if ( br.BaseStream.Position != br.BaseStream.Length )
            {
                throw new Exception("Extra data somewhere!");
            }

            log = logSb.ToString();
            //Console.WriteLine(log);

            return replay;
        }

        private static List<Frame> ExtractFrames(IEnumerable<byte> networkStream, IEnumerable<Int32> keyFramePositions, IDictionary<int, string> objectIdToName, IEnumerable<ClassNetCache> classNetCache, StringBuilder logSb)
        {
            List<ActorState> actorStates = new List<ActorState>();

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

                bool goodCandidate = (candidateTime > lastTime && candidateTime < (lastTime + 1) && (Math.Abs(actualDelta - candidateDelta) < 0.005));
                if ( !goodCandidate && keyFramePositions.Contains(curPos))
                {
                    logSb.AppendLine("Lost the chain! Picking it up again at a keyframe");
                    goodCandidate = true;
                }

                if (goodCandidate)
                {
                    // we found the start of the next frame maybe! woot.
                    var frameBits = new bool[curPos - frameStart];
                    for (int x = 0; x < (curPos - frameStart); ++x)
                    {
                        frameBits[x] = ba[frameStart + x];
                    }

                    frames.Add(Frame.Deserialize(ref actorStates, objectIdToName, classNetCache, frameStart, frameBits));

                    //logSb.AppendLine(string.Format("Found frame at position {0} with time {1} and delta {2}, actual delta {3}, delta diff {4}. Prev frame size is {5} bits", curPos, candidateTime, candidateDelta, actualDelta, (actualDelta - candidateDelta).ToString("F7"), (curPos - frameStart)));

                    lastTime = candidateTime;
                    lastDelta = candidateDelta;

                    frameStart = curPos;
                    curPos = frameStart + 64;

                }
                else
                {
                    curPos++;
                }
            }

            // TODO: this doesnt return the last frame right now, but we're not finding them all anyways yet

            return frames;
        }

        public void ToObj()
        {
            foreach (var f in Frames)
            {
                var frame = new { time = f.Time, actors = new List<object>() };
                if (f.ActorStates != null)
                {
                    foreach (var a in f.ActorStates.Where(x => x.TypeName == "Archetypes.Car.Car_Default" || x.TypeName == "Archetypes.Ball.Ball_Default"))
                    {
                        if (a.Properties != null)
                        {
                            var rb = a.Properties.Where(p => p.PropertyName == "TAGame.RBActor_TA:ReplicatedRBState").FirstOrDefault();
                            if (rb != null)
                            {
                                var pos = (Vector3D)rb.Data[1];
                                Console.WriteLine(string.Format("v {0} {1} {2}", pos.X, pos.Y, pos.Z));
                            }
                        }

                    }
                }
            }

           
        }

        public string ToPositionJson()
        {
            List<object> timeData = new List<object>();
            foreach (var f in Frames)
            {
                var frame = new { time = f.Time, actors = new List<object>() };
                if (f.ActorStates != null)
                {
                    foreach (var a in f.ActorStates.Where(x => x.TypeName == "Archetypes.Car.Car_Default" || x.TypeName == "Archetypes.Ball.Ball_Default"))
                    {
                        string type = a.TypeName == "Archetypes.Car.Car_Default" ? "car" : "ball";
                        if ( a.State == "Deleted")
                        {
                            // Move them far away. yeah, it's cheating.
                            frame.actors.Add(new { id = a.Id, type = type, x = -30000, y = 0, z = 0, pitch = 0, roll = 0, yaw = 0 });
                        }
                        else if (a.Properties != null)
                        {
                            var rb = a.Properties.Where(p => p.PropertyName == "TAGame.RBActor_TA:ReplicatedRBState").FirstOrDefault();
                            if (rb != null)
                            {
                                var pos = (Vector3D)rb.Data[1];
                                var rot = (Vector3D)rb.Data[2];
                                frame.actors.Add(new { id = a.Id, type = type, x = pos.X, y = pos.Y, z = pos.Z, pitch = rot.X, roll = rot.Y, yaw = rot.Z });
                            }
                        }

                    }
                }
                if (frame.actors.Count > 0)
                {
                    timeData.Add(frame);
                }
            }
            
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = 20*1024*1024;
            return serializer.Serialize(timeData);
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

        public List<Frame> Frames { get; private set; }

        public Int32 DebugStringLength { get; private set; }
        public List<string> DebugStrings { get; private set; }
        public Int32 TickMarkLength { get; private set; }
        public List<TickMark> TickMarks { get; private set; }
        public Int32 PackagesLength { get; private set; }
        public List<string> Packages { get; private set; }

        public Int32 ObjectLength { get; private set; }
        public string[] Objects { get; private set; } // Dictionary<int,string> might be better, since we'll need to look up by index
    //    - Array of strings for the Object table. Whenever a persistent object gets referenced for the network stream its path gets added to this array. Then its index in this array is used in the network stream.

        //public Int32 Unknown9 { get; private set; }
        public Int32 NamesLength { get; private set; }
        public List<string> Names { get; private set; } // Dictionary<int,string> might be better, since we'll need to look up by index
//- Array of strings for the Name table. "Names" are commonly used strings that get assigned an integer for use in the network stream.


        public Int32 ClassIndexLength { get; private set; }
        public List<ClassIndex> ClassIndexes { get; private set; } // Dictionary<int,string> might be better, since we'll need to look up by index
//- Map of string, integer pairs for the Class Index Map. Whenever a class is used in the network stream it is given an integer id by this map.

        public Int32 ClassNetCacheLength { get; private set; } 
        public ClassNetCache[] ClassNetCaches { get; private set; } 

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(Unknown5);
            foreach (var prop in Properties)
            {
                sb.AppendLine(prop.ToDebugString());
            }

            foreach (var t in TickMarks)
            {
                sb.AppendLine(t.ToDebugString());
            }

            foreach (var kf in KeyFrames)
            {
                sb.AppendLine(kf.ToDebugString());
            }

            for (int i = 0; i < Objects.Length; ++i)
            {
                sb.AppendLine(string.Format("Object: Index {0} Name {1}", i, Objects[i]));
            }

            for (int i = 0; i < Names.Count; ++i)
            {
                sb.AppendLine(string.Format("Name: Index {0} Name {1}", i, Names[i]));
            }



            foreach (var ci in ClassIndexes)
            {
                sb.AppendLine(ci.ToDebugString());
            }

            foreach(var c in ClassNetCaches.Where(x=>x.Root))
            {
                sb.AppendLine(c.ToDebugString(Objects));
            }

            return sb.ToString();
        }
    }
}
