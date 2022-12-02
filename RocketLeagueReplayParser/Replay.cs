using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RocketLeagueReplayParser
{
    public class Replay
    {
#if NETCOREAPP2_0 || NETSTANDARD1_3
        static Replay()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
#endif
        public static Replay Deserialize(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Deserialize(fs);
            }
        }

        public static Replay Deserialize(Stream stream)
        {
            using (var br = new BinaryReader(stream))
            {
                return Deserialize(br);
            }
        }

        public static Replay DeserializeHeader(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var br = new BinaryReader(fs))
            {
                return DeserializeHeader(br);
            }
        }

        public static Replay DeserializeHeader(BinaryReader br)
        {
            var replay = new Replay();

            replay.Part1Length = br.ReadInt32();
            replay.Part1Crc = br.ReadUInt32();
            replay.EngineVersion = br.ReadUInt32();
            replay.LicenseeVersion = br.ReadUInt32();

            if (replay.EngineVersion >= 868 && replay.LicenseeVersion >= 18)
            {
                replay.NetVersion = br.ReadUInt32();
            }
            replay.TAGame_Replay_Soccar_TA = br.ReadString2();

            replay.Properties = PropertyDictionary.Deserialize(br);

            return replay;
        }

        public static Replay Deserialize(BinaryReader br)
        {
			var replay = DeserializeHeader(br);
            
            replay.Part2Length = br.ReadInt32();
            replay.Part2Crc = br.ReadUInt32();

            replay.LevelLength = br.ReadInt32();
			// looks like sfx data, not level data. shrug
			replay.Levels = new List<Level>();
			for (int i = 0; i < replay.LevelLength; i++)
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
            replay.NetworkStream = br.ReadBytes(replay.NetworkStreamLength); 
                
			replay.DebugStringLength = br.ReadInt32();
			replay.DebugStrings = new List<DebugString>();
			for (int i = 0; i < replay.DebugStringLength; i++)
			{
				replay.DebugStrings.Add(DebugString.Deserialize(br));
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
				replay.Packages.Add(br.ReadString2());
			}

			replay.ObjectLength = br.ReadInt32();
			replay.Objects = new string[replay.ObjectLength];
			for (int i = 0; i < replay.ObjectLength; i++)
			{
				replay.Objects[i] = br.ReadString2();
			}

			replay.NamesLength = br.ReadInt32();
			replay.Names = new string[replay.NamesLength];
			for (int i = 0; i < replay.NamesLength; i++)
			{
				replay.Names[i] = br.ReadString2();
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
                var classNetCache = ClassNetCache.Deserialize(br);
                replay.ClassNetCaches[i] = classNetCache;

				for (int j = i - 1; j >= 0; --j)
				{
					if (classNetCache.ParentId == replay.ClassNetCaches[j].Id)
					{
                        classNetCache.Parent = replay.ClassNetCaches[j];
						replay.ClassNetCaches[j].Children.Add(classNetCache);
						break;
					}
				}
                   
                if (replay.ClassNetCaches[i].Parent == null)
                {
                    replay.ClassNetCaches[i].Root = true;
                }
                    
			}

            if (replay.NetVersion >= 10)
            {
                replay.Unknown = br.ReadUInt32();
            }

            replay.MergeDuplicateClasses();

            // 2016/02/10 patch replays have TAGame.PRI_TA classes with no parent. 
            // Deserialization may have failed somehow, but for now manually fix it up.
            replay.FixClassParent("ProjectX.PRI_X", "Engine.PlayerReplicationInfo");
            replay.FixClassParent("TAGame.PRI_TA", "ProjectX.PRI_X");

            // A lot of replays have messed up class hierarchies, commonly giving 
            // both Engine.TeamInfo, TAGame.CarComponent_TA, and others the same id.
            // Some ambiguities may have become more common since the 2016-06-20 patch,
            // but there have always been issues.
            //
            // For example, from E8B66F8A4561A2DAACC61FA9FBB710CD:
            //    Index 26(TAGame.CarComponent_TA) ParentId 21 Id 24
            //        Index 28(TAGame.CarComponent_Dodge_TA) ParentId 24 Id 25
            //        Index 188(TAGame.CarComponent_Jump_TA) ParentId 24 Id 24
            //            Index 190(TAGame.CarComponent_DoubleJump_TA) ParentId 24 Id 24
            //    Index 30(Engine.Info) ParentId 21 Id 21
            //        Index 31(Engine.ReplicationInfo) ParentId 21 Id 21
            //            Index 195(Engine.TeamInfo) ParentId 21 Id 24
            //                Index 214(TAGame.CarComponent_Boost_TA) ParentId 24 Id 31
            //                Index 237(TAGame.CarComponent_FlipCar_TA) ParentId 24 Id 26
            // Problems: 
            //     TAGame.CarComponent_Jump_TA's parent id and id are both 24 (happens to work fine in this case)
            //     TAGame.CarComponent_DoubleJump_TA's parent id and id are both 24 (incorrectly picks CarComponent_Jump_TA as parent)
            //     Engine.TeamInfo's ID is 24, even though there are 3 other classes with that id
            //     TAGame.CarComponent_Boost_TA's parent is 24 (Incorrectly picks Engine.TeamInfo, since it's ambiguous)
            //     TAGame.CarComponent_FlipCar_TA's parent is 24 (Incorrectly picks Engine.TeamInfo, since it's ambiguous)
            //     Engine.ReplicationInfo and Engine.Info have the same parent id and id (no ill effects so far)
            //
            // Note: The heirarchy problems do not always cause parsing errors! But they can if you're unlucky.

            replay.FixClassParent("TAGame.CarComponent_Boost_TA", "TAGame.CarComponent_TA");
            replay.FixClassParent("TAGame.CarComponent_FlipCar_TA", "TAGame.CarComponent_TA");
            replay.FixClassParent("TAGame.CarComponent_Jump_TA", "TAGame.CarComponent_TA");
            replay.FixClassParent("TAGame.CarComponent_Dodge_TA", "TAGame.CarComponent_TA");
            replay.FixClassParent("TAGame.CarComponent_DoubleJump_TA", "TAGame.CarComponent_TA");
            replay.FixClassParent("TAGame.GameEvent_TA", "Engine.Actor");
            replay.FixClassParent("TAGame.SpecialPickup_TA", "TAGame.CarComponent_TA");
            replay.FixClassParent("TAGame.SpecialPickup_BallVelcro_TA", "TAGame.SpecialPickup_TA");
            replay.FixClassParent("TAGame.SpecialPickup_Targeted_TA", "TAGame.SpecialPickup_TA");
            replay.FixClassParent("TAGame.SpecialPickup_Spring_TA", "TAGame.SpecialPickup_Targeted_TA");
            replay.FixClassParent("TAGame.SpecialPickup_BallLasso_TA", "TAGame.SpecialPickup_Spring_TA");
            replay.FixClassParent("TAGame.SpecialPickup_BoostOverride_TA", "TAGame.SpecialPickup_Targeted_TA");
            replay.FixClassParent("TAGame.SpecialPickup_BallCarSpring_TA", "TAGame.SpecialPickup_Spring_TA");
            replay.FixClassParent("TAGame.SpecialPickup_BallFreeze_TA", "TAGame.SpecialPickup_Targeted_TA");
            replay.FixClassParent("TAGame.SpecialPickup_Swapper_TA", "TAGame.SpecialPickup_Targeted_TA");
            replay.FixClassParent("TAGame.SpecialPickup_GrapplingHook_TA", "TAGame.SpecialPickup_Targeted_TA");
            replay.FixClassParent("TAGame.SpecialPickup_BallGravity_TA", "TAGame.SpecialPickup_TA");
            replay.FixClassParent("TAGame.SpecialPickup_HitForce_TA", "TAGame.SpecialPickup_TA");
            replay.FixClassParent("TAGame.SpecialPickup_Tornado_TA", "TAGame.SpecialPickup_TA");
            replay.FixClassParent("TAGame.SpecialPickup_HauntedBallBeam_TA", "TAGame.SpecialPickup_TA");
            replay.FixClassParent("TAGame.CarComponent_TA", "Engine.Actor");
            replay.FixClassParent("Engine.Info", "Engine.Actor");
            replay.FixClassParent("Engine.Pawn", "Engine.Actor"); 

            // Havent had problems with these yet. They (among others) can be ambiguous, 
            // but I havent found a replay yet where my parent choosing algorithm
            // (which picks the matching class that was most recently read) picks the wrong class.
            // Just a safeguard for now.
            replay.FixClassParent("Engine.TeamInfo", "Engine.ReplicationInfo");
            replay.FixClassParent("TAGame.Team_TA", "Engine.TeamInfo");

            // Fixes https://ballchasing.com/dl/replay/c0d0b0e0-562d-40a9-be75-410fbfd4d698
            replay.FixClassParent("TAGame.PRI_Breakout_TA", "TAGame.PRI_TA");

            UInt32 changeList = 0;
            if ( replay.Properties.ContainsKey("Changelist"))
            {
                // int probably works just as well, but UInt32 matches everything else.
                changeList = (UInt32)(int)replay.Properties["Changelist"].Value;
            }

            replay.Frames = ExtractFrames(replay.MaxChannels(), replay.NetworkStream, replay.Objects, replay.ClassNetCaches, replay.EngineVersion, replay.LicenseeVersion, replay.NetVersion, changeList);

			if (br.BaseStream.Position != br.BaseStream.Length)
			{
				throw new Exception("Extra data somewhere!");
			}

			return replay;
        }

        private void FixClassParent(string childClassName, string parentClassName)
        {
            var parentClass = ClassNetCaches.Where(cnc => Objects[cnc.ObjectIndex] == parentClassName).SingleOrDefault();
            var childClass = ClassNetCaches.Where(cnc => Objects[cnc.ObjectIndex] == childClassName).SingleOrDefault();
            if (parentClass != null && childClass != null && (childClass.Parent == null || childClass.Parent != parentClass))
            {
                var oldParent = childClass.Parent == null ? "NULL" : Objects[childClass.Parent.ObjectIndex];
                System.Diagnostics.Trace.WriteLine($"Fixing class {childClassName}, setting its parent to {parentClassName} from {oldParent}");

                childClass.Root = false;
                if (childClass.Parent != null)
                {
                    childClass.Parent.Children.Remove(childClass);
                }
                childClass.Parent = parentClass;
                parentClass.Children.Add(childClass);
            }
        }

        private void MergeDuplicateClasses()
        {
            // Rarely, a class is defined multiple times. 
            // See replay 5F9D44B6400E284FD15A95AC8D5C5B45 which has 2 entries for TAGame.GameEvent_Soccar_TA
            // Merge their properties and drop the extras to keep everything from starting on fire

            var deletedClasses = new List<ClassNetCache>();

            var groupedClasses = ClassNetCaches.GroupBy(cnc => Objects[cnc.ObjectIndex]);
            foreach(var g in groupedClasses.Where(gc => gc.Count() > 1))
            {
                var goodClass = g.First();
                foreach(var badClass in g.Skip(1))
                {
                    foreach (var p in badClass.Properties)
                    {
                        goodClass.Properties[p.Key] = p.Value;
                    }
                    deletedClasses.Add(badClass);
                }
            }

            ClassNetCaches = ClassNetCaches.Where(cnc => !deletedClasses.Contains(cnc)).ToArray();
        }

        public static bool ValidateCrc(string filePath, bool onlyPartOne)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fs.Length <= 8)
                {
                    return false;
                }

                var intBuffer = new byte[4];
                if (fs.Read(intBuffer, 0, 4) != 4) return false;
                var part1Length = BitConverter.ToInt32(intBuffer, 0);
                if (fs.Read(intBuffer, 0, 4) != 4) return false;
                var part1Crc = BitConverter.ToUInt32(intBuffer, 0);
                var part1Bytes = new byte[part1Length];
                if (fs.Read(part1Bytes, 0, part1Length) != part1Length) return false;

                if (part1Crc != Crc32.CalculateCrc(part1Bytes, 0, part1Length, CRC_SEED))
                {
                    return false;
                }

                if ( !onlyPartOne )
                {
                    if (fs.Read(intBuffer, 0, 4) != 4) return false;
                    var part2Length = BitConverter.ToInt32(intBuffer, 0);
                    if (fs.Read(intBuffer, 0, 4) != 4) return false;
                    var part2Crc = BitConverter.ToUInt32(intBuffer, 0);
                    var part2Bytes = new byte[part2Length];
                    if (fs.Read(part2Bytes, 0, part2Length) != part2Length) return false;

                    if (part2Crc != Crc32.CalculateCrc(part2Bytes, 0, part2Length, CRC_SEED))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void Serialize(Stream stream)
        {
            List<byte> part1Bytes = new List<byte>();

            part1Bytes.AddRange(BitConverter.GetBytes(EngineVersion));
            part1Bytes.AddRange(BitConverter.GetBytes(LicenseeVersion));
            
            if (EngineVersion >= 868 && LicenseeVersion >= 18)
            {
                part1Bytes.AddRange(BitConverter.GetBytes(NetVersion));
            }

            part1Bytes.AddRange(TAGame_Replay_Soccar_TA.Serialize());
            
            part1Bytes.AddRange(Properties.Serialize());

            var bytes = part1Bytes.ToArray();
            stream.Write(BitConverter.GetBytes(part1Bytes.Count), 0, 4);
            var crc = Crc32.CalculateCrc(bytes, 0, bytes.Length, CRC_SEED);
            stream.Write(BitConverter.GetBytes(crc), 0, 4);
            stream.Write(bytes, 0, part1Bytes.Count);


            List<byte> part2Bytes = new List<byte>();

            part2Bytes.AddRange(BitConverter.GetBytes(Levels.Count));
            foreach (var level in Levels)
            {
                part2Bytes.AddRange(level.Serialize());
            }

            part2Bytes.AddRange(BitConverter.GetBytes(KeyFrames.Count));
            foreach (var keyFrame in KeyFrames)
            {
                part2Bytes.AddRange(keyFrame.Serialize());
            }

            var bw = new BitWriter(8 * 1024 * 1024); // 1MB is a decent starting point
            var maxChannels = MaxChannels();

            UInt32 changeList = 0;
            if (Properties.ContainsKey("Changelist"))
            {
                // int probably works just as well, but UInt32 matches everything else.
                changeList = (UInt32)(int)Properties["Changelist"].Value;
            }

            foreach (Frame f in Frames)
            {
                f.Serialize(maxChannels, Objects, EngineVersion, LicenseeVersion, NetVersion, changeList, bw);
            }
            
            var networkStreamBytes = bw.GetBytes();
            var paddingSize = 512 - (networkStreamBytes.Length % 512); // Padding needed to match original replay files, but dont think it's necessary to produce good replay
            if ( paddingSize == 512 )
            {
                paddingSize = 0;
            }

            part2Bytes.AddRange(BitConverter.GetBytes(networkStreamBytes.Length + paddingSize));
            part2Bytes.AddRange(networkStreamBytes);

            for (int i = 0; i < paddingSize; ++i)
            {
                part2Bytes.Add(0);
            }

            part2Bytes.AddRange(BitConverter.GetBytes(DebugStrings.Count));
            foreach (var debugString in DebugStrings)
            {
                part2Bytes.AddRange(debugString.Serialize());
            }

            part2Bytes.AddRange(BitConverter.GetBytes(TickMarks.Count));
            foreach (var tickMark in TickMarks)
            {
                part2Bytes.AddRange(tickMark.Serialize());
            }

            part2Bytes.AddRange(BitConverter.GetBytes(Packages.Count));
            foreach (var packages in Packages)
            {
                part2Bytes.AddRange(packages.Serialize());
            }

            part2Bytes.AddRange(BitConverter.GetBytes(Objects.Length));
            foreach (var obj in Objects)
            {
                part2Bytes.AddRange(obj.Serialize());
            }

            part2Bytes.AddRange(BitConverter.GetBytes(Names.Length));
            foreach (var name in Names)
            {
                part2Bytes.AddRange(name.Serialize());
            }

            part2Bytes.AddRange(BitConverter.GetBytes(ClassIndexes.Count));
            foreach (var classIndex in ClassIndexes)
            {
                part2Bytes.AddRange(classIndex.Serialize());
            }

            part2Bytes.AddRange(BitConverter.GetBytes(ClassNetCaches.Length));
            foreach (var classNetCache in ClassNetCaches)
            {
                part2Bytes.AddRange(classNetCache.Serialize());
            }

            if (NetVersion >= 10)
            {
                part2Bytes.AddRange(BitConverter.GetBytes(Unknown));
            }

            bytes = part2Bytes.ToArray();
            stream.Write(BitConverter.GetBytes(part2Bytes.Count), 0, 4);
            crc = Crc32.CalculateCrc(bytes, 0, bytes.Length, CRC_SEED);
            stream.Write(BitConverter.GetBytes(crc), 0, 4);
            stream.Write(bytes, 0, part2Bytes.Count);
        }

        private static List<Frame> ExtractFrames(int maxChannels, IEnumerable<byte> networkStream, string[] objectIdToName, IEnumerable<ClassNetCache> classNetCache, UInt32 engineVersion, UInt32 licenseeVersion, UInt32 netVersion, UInt32 changelist)
        {
            Dictionary<UInt32, ActorState> actorStates = new Dictionary<UInt32, ActorState>();

            IDictionary<string, ClassNetCache> classNetCacheByName = classNetCache.ToDictionary(k => objectIdToName[k.ObjectIndex], v => v);

            var br = new BitReader(networkStream.ToArray());
            List<Frame> frames = new List<Frame>();

            while (br.Position < (br.Length - 64))
            {
                var newFrame = Frame.Deserialize(maxChannels, ref actorStates, objectIdToName, classNetCacheByName, engineVersion, licenseeVersion, netVersion, changelist, br);
                
                if (frames.Any() && newFrame.Time != 0 && (newFrame.Time < frames.Last().Time)
#if DEBUG
                    && newFrame.Complete 
                    && !newFrame.Failed 
#endif
                    )
                {
                    var error = string.Format("Frame time is less than the previous frame's time. Parser is lost. Frame position {0}, Time {1}. Previous frame time {2}", newFrame.Position, newFrame.Time, frames.Last().Time);
#if DEBUG
                    frames.Add(newFrame);
                    Console.WriteLine(error);
                    newFrame.Failed = true;
                    newFrame.Complete = false;
                    break;
#endif
                    throw new Exception(error);
                }

                frames.Add(newFrame);

#if DEBUG
                if (frames.Any(f => !f.Complete ))
				{
					break;
				}
#endif
            }

            return frames;
        }
        
        private const UInt32 CRC_SEED = 0xEFCBF201;
        
        public Int32 Part1Length { get; private set; }
        public UInt32 Part1Crc { get; private set; }

        // TODO: May be worth putting the version numbers into a ReplayVersion object that can be passed around
        public UInt32 EngineVersion { get; private set; }
        public UInt32 LicenseeVersion { get; private set; }
        public UInt32 NetVersion { get; private set; }

        // Always the string "TAGame.Replay_Soccar_TA"
        public string TAGame_Replay_Soccar_TA { get; private set; }
        public PropertyDictionary Properties { get; private set; }

        public Int32 Part2Length { get; private set; }
        public UInt32 Part2Crc { get; private set; }
        public Int32 LevelLength { get; private set; }
        public List<Level> Levels { get; private set; }
        public Int32 KeyFrameLength { get; private set; }
        public List<KeyFrame> KeyFrames { get; private set; }

        private Int32 NetworkStreamLength { get; set; }
        private byte[] NetworkStream { get; set; }

        public List<Frame> Frames { get; private set; }

        public Int32 DebugStringLength { get; private set; }
        public List<DebugString> DebugStrings { get; private set; }
        public Int32 TickMarkLength { get; private set; }
        public List<TickMark> TickMarks { get; private set; }
        public Int32 PackagesLength { get; private set; }
        public List<string> Packages { get; private set; }

        public Int32 ObjectLength { get; private set; }
        public string[] Objects { get; private set; } 
        public Int32 NamesLength { get; private set; }
        public string[] Names { get; private set; } 

        public Int32 ClassIndexLength { get; private set; }
        public List<ClassIndex> ClassIndexes { get; private set; } // Dictionary<int,string> might be better, since we'll need to look up by index

        public Int32 ClassNetCacheLength { get; private set; }

        public ClassNetCache[] ClassNetCaches { get; private set; }
        public UInt32 Unknown { get; private set; }

        public int MaxChannels()
        {
            return (int?)Properties["MaxChannels"]?.Value ?? 1023;
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(TAGame_Replay_Soccar_TA);
            foreach (var prop in Properties.Values)
            {
                sb.AppendLine(prop.ToDebugString());
            }

            foreach (var ds in DebugStrings)
            {
                sb.AppendLine(ds.ToString());
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

            for (int i = 0; i < Names.Length; ++i)
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
