using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace RocketLeagueReplayParser
{
    public class Replay
    {
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
            replay.VersionMajor = br.ReadUInt32();
            replay.VersionMinor = br.ReadUInt32();

            if (replay.VersionMajor >= 868 && replay.VersionMinor >= 18)
            {
                // Always 0?
                replay.Unknown6 = br.ReadUInt32();
            }
            replay.Unknown5 = br.ReadString2();

            replay.Properties = PropertyDictionary.Deserialize(br);

            return replay;
        }

        public static Replay Deserialize(BinaryReader br)
        {
			var replay = DeserializeHeader(br);

            try
			{
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
				replay.NetworkStream = new List<byte>();
				for (int i = 0; i < replay.NetworkStreamLength; ++i)
				{
					replay.NetworkStream.Add(br.ReadByte());
				}

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

                // Havent had problems with these yet. They (among others) can be ambiguous, 
                // but I havent found a replay yet where my parent choosing algorithm
                // (which picks the matching class that was most recently read) picks the wrong class.
                // Just a safeguard for now.
                replay.FixClassParent("Engine.TeamInfo", "Engine.ReplicationInfo");
                replay.FixClassParent("TAGame.Team_TA", "Engine.TeamInfo");

                replay.Frames = ExtractFrames(replay.MaxChannels(), replay.NetworkStream, replay.KeyFrames.Select(x => x.FilePosition), replay.Objects, replay.ClassNetCaches, replay.VersionMajor, replay.VersionMinor);

				if (br.BaseStream.Position != br.BaseStream.Length)
				{
					throw new Exception("Extra data somewhere!");
				}

				return replay;
			}
			catch(Exception)
			{
#if DEBUG
				return replay;
#else
				throw;
#endif

			}
        }

        private void FixClassParent(string childClassName, string parentClassName)
        {
            var parentClass = ClassNetCaches.Where(cnc => Objects[cnc.ObjectIndex] == parentClassName).SingleOrDefault();
            var childClass = ClassNetCaches.Where(cnc => Objects[cnc.ObjectIndex] == childClassName).SingleOrDefault();
            if (parentClass != null && childClass != null && (childClass.Parent == null || childClass.Parent != parentClass))
            {
#if DEBUG
                Console.WriteLine(string.Format("Fixing class {0}, setting its parent to {1} from {2}", childClassName, parentClassName, childClass.Parent == null ? "NULL" : Objects[childClass.Parent.ObjectIndex]));
#endif
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

            part1Bytes.AddRange(BitConverter.GetBytes(VersionMajor));
            part1Bytes.AddRange(BitConverter.GetBytes(VersionMinor));
            part1Bytes.AddRange(Unknown5.Serialize());

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
            Dictionary<UInt32, ActorState> newActorsById = new Dictionary<uint, ActorState>();
            var maxChannels = MaxChannels();
            foreach (Frame f in Frames)
            {
                f.Serialize(maxChannels, ref newActorsById, VersionMajor, VersionMinor, bw);
            }
            var networkStreamBytes = bw.GetBytes();
            part2Bytes.AddRange(BitConverter.GetBytes(networkStreamBytes.Length));
            part2Bytes.AddRange(networkStreamBytes);


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

            bytes = part2Bytes.ToArray();
            stream.Write(BitConverter.GetBytes(part2Bytes.Count), 0, 4);
            crc = Crc32.CalculateCrc(bytes, 0, bytes.Length, CRC_SEED);
            stream.Write(BitConverter.GetBytes(crc), 0, 4);
            stream.Write(bytes, 0, part2Bytes.Count);
        }

        private static List<Frame> ExtractFrames(int maxChannels, IEnumerable<byte> networkStream, IEnumerable<Int32> keyFramePositions, string[] objectIdToName, IEnumerable<ClassNetCache> classNetCache, UInt32 versionMajor, UInt32 versionMinor)
        {
            List<ActorState> actorStates = new List<ActorState>();

            IDictionary<string, ClassNetCache> classNetCacheByName = classNetCache.ToDictionary(k => objectIdToName[k.ObjectIndex], v => v);

            var br = new BitReader(networkStream.ToArray());
            List<Frame> frames = new List<Frame>();

            while (br.Position < (br.Length - 64))
            {
                var newFrame = Frame.Deserialize(maxChannels, ref actorStates, objectIdToName, classNetCacheByName, versionMajor, versionMinor, br);
                
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
                    foreach (var a in f.ActorStates.Where(x => x.TypeName == "Archetypes.Car.Car_Default" || x.TypeName == "Archetypes.Ball.Ball_Default" || x.TypeName == "Archetypes.Ball.CubeBall"))
                    {
                        string type = a.TypeName == "Archetypes.Car.Car_Default" ? "car" : "ball";
                        if ( a.State == ActorStateState.Deleted)
                        {
                            // Move them far away. yeah, it's cheating.
                            frame.actors.Add(new { id = a.Id, type = type, x = -30000, y = 0, z = 0, pitch = 0, roll = 0, yaw = 0 });
                        }
                        else if (a.Properties != null)
                        {
                            var rbp = a.Properties.Where(p => p.PropertyName == "TAGame.RBActor_TA:ReplicatedRBState").FirstOrDefault();
                            if (rbp != null)
                            {
                                var rb = (RigidBodyState)rbp.Data[0];
                                var pos = rb.Position;
                                var rot = rb.Rotation;
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


        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        public Color HeatMapColor(double value)
        {
            if ( value == 0)
            {
                return Color.FromArgb(0,0,0);
            }
            else if ( value < 0.25 )
            {
                return ColorFromHSV(240 - (120 * (value/.25)), 1, 1);
            }
            else if (value < 0.5)
            {
                return ColorFromHSV(120 - (60 * ((value-.25) / .25)), 1, 1);
            }
            else if (value < 0.75)
            {
                return ColorFromHSV(60 - (30 * ((value - .5) / .25)), 1, 1);
            }
            else if (value < 0.95)
            {
                return ColorFromHSV(30 - (30 * ((value - .75) / .20)), 1, 1);
            }
            else
            {
                return ColorFromHSV(0, ((value-.95)/.5), 1);
            }

        }

        public void ToHeatmap()
        {
            var teams = Frames.First().ActorStates.Where(x => x.ClassName == "TAGame.Team_TA");
            var players = Frames.SelectMany(x => x.ActorStates.Where(a => a.ClassName == "TAGame.PRI_TA" && a.Properties != null && a.Properties.Any()))
                .Select(a => new
                {
                    Id = a.Id,
                    Name = a.Properties.Where(p => p.PropertyName == "Engine.PlayerReplicationInfo:PlayerName").Single().Data[0].ToString(),
                    TeamActorId = (int)a.Properties.Where(p => p.PropertyName == "Engine.PlayerReplicationInfo:Team").Single().Data[1]
                })
                .Distinct();

            var positions = Frames.SelectMany(x => x.ActorStates.Where(a => a.ClassName == "TAGame.Car_TA" && a.Properties != null && a.Properties.Any(p => p.PropertyName == "TAGame.RBActor_TA:ReplicatedRBState")))
                .Select(a => new
                {
                    //PlayerActorId = (int)a.Properties.Where(p => p.PropertyName == "Engine.Pawn:PlayerReplicationInfo").Single().Data[1],
                    Position = ((RigidBodyState)a.Properties.Where(p => p.PropertyName == "TAGame.RBActor_TA:ReplicatedRBState").Single().Data[0]).Position
                });


            var minX = positions.Min(x => x.Position.X);
            var minY = positions.Min(x => x.Position.Y);
            var minZ = positions.Min(x => x.Position.Z);
            var maxX = positions.Max(x => x.Position.X);
            var maxY = positions.Max(x => x.Position.Y);
            var maxZ = positions.Max(x => x.Position.Z);

            var maxValue = 0;
            int heatMapWidth = (int)(maxX - minX) + 1;
            int heatMapHeight = (int)(maxY - minY) + 1;
            var heatmap = new byte[heatMapWidth, heatMapHeight];
            foreach(var p in positions)
            {
                int x = (int)(p.Position.X-minX);
                int y = (int)(p.Position.Y-minY);

                var radius = 50;
                var squaredRadius = Math.Pow(radius, 2);
                for (int cy = y - radius; cy <= y + radius; ++cy)
                {
                    for (int cx = x - radius; cx <= x + radius; ++cx)
                    {
                        var distanceSquared = Math.Pow(cx - x, 2) + Math.Pow(cy - y, 2);

                        if ((cx >= 0) && (cx < heatMapWidth) && (cy >= 0) && (cy < heatMapHeight) && (distanceSquared <= squaredRadius))
                        {
                            heatmap[cx, cy]++;
                            maxValue = Math.Max(maxValue, heatmap[cx, cy]);
                        }
                    }
                }
                    
                
            }

            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(heatMapWidth, heatMapHeight);
            for (int x = 0; x < heatMapWidth; x++)
            {
                for (int y = 0; y < heatMapHeight; y++)
                {
                    var value = ((double)heatmap[x, y] / (double)maxValue);//(int)(255 * ((double)heatmap[x, y]) / (double)maxValue);
                    bm.SetPixel(x,y, HeatMapColor(value));// System.Drawing.Color.FromArgb(value, value, value));
                }
            }
            bm.Save(@"D:\MyData\CodeProjects\RocketLeagueReplayParser\RocketLeagueReplayParserWeb\test.jpg");
            /*
            var heatMapData = new List<object>();
            foreach(var p in players)
            {
                heatMapData.Add(new {
                    PlayerName = p.Name,
                    Team = teams.Where(x => x.Id == p.TeamActorId).Single().TypeName == "Archetypes.Teams.Team0" ? 0 : 1,
                    Positions = positions.Where(x=>x.PlayerActorId == p.Id).Select(x=>x.Position)
                });
            }
             * *
             */
        }

        private const UInt32 CRC_SEED = 0xEFCBF201;

        // We have a good idea about what many of these unknowns are
        // But no solid confirmations yet, so I'm leaving them unknown, with comments
        public Int32 Part1Length { get; private set; }
        public UInt32 Part1Crc { get; private set; }
        public UInt32 VersionMajor { get; private set; }
        public UInt32 VersionMinor { get; private set; }
        public UInt32 Unknown6 { get; private set; }
        public string Unknown5 { get; private set; }
        public PropertyDictionary Properties { get; private set; }

        public Int32 Part2Length { get; private set; }
        public UInt32 Part2Crc { get; private set; }
        public Int32 LevelLength { get; private set; }
        public List<Level> Levels { get; private set; }
        public Int32 KeyFrameLength { get; private set; }
        public List<KeyFrame> KeyFrames { get; private set; }

        private Int32 NetworkStreamLength { get; set; }
        private List<byte> NetworkStream { get; set; }

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

        public int MaxChannels()
        {
            return (int?)Properties["MaxChannels"]?.Value ?? 1023;
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(Unknown5);
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
