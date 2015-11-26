using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepEqual.Syntax;

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
            replay.Unknown5 = br.ReadString2();
            
            var s = br.BaseStream.Position;
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
            //Frame lastFrame = null;
            //while ((lastFrame == null || lastFrame.Complete) && br.
            replay.Frames = ExtractFrames(replay.NetworkStream, replay.KeyFrames.Select(x => x.FilePosition), objectIndexToName, replay.ClassNetCaches, logSb);

            //var minSize = replay.Frames.Where(x => !x.Complete /*&& x.BitLength != 163*/ && x.ActorStates.Count > 0 && !string.IsNullOrWhiteSpace(x.ActorStates[0].TypeName)).Min(x => x.BitLength);
            foreach (var f in replay.Frames.Where(x => !x.Complete || x.ActorStates.Any(a=>a.ForcedComplete)))// x.Failed) )//Complete && x.BitLength == minSize))
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

            var br = new BitReader(networkStream.ToArray());
            List<Frame> frames = new List<Frame>();

            while (br.Position < (br.Length - 64))
            {
                frames.Add(Frame.Deserialize(ref actorStates, objectIdToName, classNetCache, br));
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

        public class ActorStateJson
        {
            public int Id { get; set; }
            public bool UnknownBit { get; set; }
            public string TypeName { get; set; }
            public string ClassName { get; set; }

            public Vector3D InitialPosition { get; set; }

            public List<ActorStatePropertyJson> Properties { get; set; } 
        }

        public class ActorStatePropertyJson
        {
            public string Name { get; set; }
            public List<object> Data { get; set; }
        }

        public string ToJson()
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<int, ActorStateJson> actorStates = new Dictionary<int, ActorStateJson>();
            var frameJson = new List<string>();
            foreach(var f in Frames.Where(x=>x.ActorStates.Count > 0))
            {
                List<Int32> deletedActorStateIds = new List<int>();
                Dictionary<int, ActorStateJson> newActorStates = new Dictionary<int, ActorStateJson>();

                foreach(var a in f.ActorStates.Where(x=>x.State == ActorStateState.Deleted))
                {
                    actorStates.Remove(a.Id);
                    deletedActorStateIds.Add(a.Id);
                }

                foreach (var a in f.ActorStates.Where(x => x.State == ActorStateState.New))
                {
                    // Skip anything thats not truly new. Dont want keyframes in our json (for now)
                    if (!actorStates.ContainsKey(a.Id))
                    { 
                        var actorState = new ActorStateJson();
                        actorState.Id = a.Id;
                        actorState.UnknownBit = a.Unknown1;
                        actorState.TypeName = a.TypeName;
                        actorState.ClassName = a.ClassName;
                        actorState.InitialPosition = a.Position;
                        actorState.Properties = new List<ActorStatePropertyJson>();
             
                        actorStates[a.Id] = actorState;
                    }
                }

                foreach (var a in f.ActorStates.Where(x => x.State == ActorStateState.Existing))
                {
                    var existingActorState = actorStates[a.Id];
                    ActorStateJson actorState = null;
                    if (newActorStates.ContainsKey(a.Id))
                    {
                        // new actor
                        actorState = newActorStates[a.Id];
                    }
                    else
                    {
                        // Existing actor. Start a new state object to track changes

                        actorState = new ActorStateJson();
                        actorState.Id = a.Id;
                        actorState.Properties = new List<ActorStatePropertyJson>();

                        // Maybe skip some of the following for existing actors
                        actorState.UnknownBit = existingActorState.UnknownBit;
                        actorState.TypeName = existingActorState.TypeName;
                        actorState.ClassName = existingActorState.ClassName;
                        actorState.InitialPosition = existingActorState.InitialPosition;
                    }

                    foreach(var p in a.Properties)
                    {
                        var property = new ActorStatePropertyJson
                        {
                            Name = p.PropertyName,
                            Data = p.Data
                        };

                        var existingProperty = existingActorState.Properties.Where(ep => ep.Name == property.Name).FirstOrDefault();
                        if ( existingProperty == null )
                        {
                            // new property
                            actorState.Properties.Add(property);
                            existingActorState.Properties.Add(property);
                        }
                        else
                        {
                            // Existing property. Only keep if it is truly different
                            if (!existingProperty.IsDeepEqual(property))
                            {
                                actorState.Properties.Add(property);

                                // replace the property in our main set of data, so we have it for the next frame
                                existingActorState.Properties = existingActorState.Properties.Where(ep => ep.Name != property.Name).ToList();
                                existingActorState.Properties.Add(property);
                            }
                        }
                    }

                    // Dont keep this state if we havent found any properties worth keeping
                    if (actorState.Properties.Any())
                    {
                        newActorStates[a.Id] = actorState;
                    }
                }

                // Serializing at each frame to make sure we capture the state at each step.
                // Otherwise, since we're not cloning objects at each step, we'd serialize only the most recent set of data
                if (deletedActorStateIds.Any() || newActorStates.Any())
                {
                    frameJson.Add(serializer.Serialize(new { Time = f.Time, DeletedActorIds = deletedActorStateIds, ActorUpdates = newActorStates.Values }));
                }
            }
            return "[" + string.Join(",", frameJson) + "]";
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
        
        private ClassNetCache[] ClassNetCaches { get; set; } 

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(Unknown5);
            foreach (var prop in Properties)
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
