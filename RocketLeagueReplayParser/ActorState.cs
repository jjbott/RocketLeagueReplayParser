using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class ActorState
    {
        public int Id { get; private set; }
        public string State { get; private set; } // TODO: Enumify someday
        public bool Unknown1 { get; private set; } // Potentially to signal more data in a packed int, but that doesnt seem right
        public byte? TypeId { get; private set; }
        public string TypeName { get; private set; }
        public string ClassName { get; private set; }

        public byte? Rot1 { get; private set; }
        public byte? Rot2 { get; private set; }
        public byte? Rot3 { get; private set; }
        public Vector3D Position { get; private set; }
        public Vector3D UnknownVector1 { get; private set; }
        public Vector3D UnknownVector2 { get; private set; }

        public List<ActorStateProperty> Properties { get; private set; }

        public List<bool> KnownBits { get; set; }


        public bool Complete { get; private set; } // Set to true when we're sure we read the whole thing
        public bool Failed { get; private set; }

        public static ClassNetCache ObjectNameToClassNetCache(string objectName, IDictionary<int, string> objectIdToName, IEnumerable<ClassNetCache> classNetCache)
        {
            var name = Regex.Replace(objectName, @"_\d+", "")
                .Split('.').Last()
                .Split(':').Last()
                //.Split(new string[] { "_TA" }, StringSplitOptions.RemoveEmptyEntries).First()
                .Replace("_Default", "_TA")
                .Replace("Archetype", "")
                .Replace("_0", "")
                .Replace("0", "_TA")
                .Replace("1", "_TA")
                .Replace("Default__", "");

            var matches = classNetCache
                .Where(x => 
                    objectIdToName[x.ObjectIndex].Contains("." + name) );
            if ( matches.Count() == 0 )
            {
                return classNetCache.First();
            }
            return matches.Single();
        }

        public static ActorState Deserialize(List<ActorState> existingActorStates, List<ActorState> frameActorStates, IDictionary<int, string> objectIndexToName, IEnumerable<ClassNetCache> classNetCache, BitReader br)
        {
            //var a = new ActorState();
            var startPosition = br.Position;

            var actorId = br.ReadInt32FromBits(10);

            ActorState a = new ActorState();
            a.Id = actorId;

            try
            {
                var maxId = existingActorStates.Any() ? existingActorStates.Max(x => x.Id) : -1;
                if (actorId > (maxId + 20))
                {
                    // we're probably lost. Awwww.
                    a.KnownBits = br.GetBits(startPosition, br.Position - startPosition);
                    return a;
                }

                frameActorStates.Add(a);

                if (br.ReadBit())
                {
                    if (br.ReadBit())
                    {
                        a.State = "New";
                        a.Unknown1 = br.ReadBit();
                        a.TypeId = br.ReadByte();
                        a.Rot1 = br.ReadByte();
                        a.Rot2 = br.ReadByte();
                        a.Rot3 = br.ReadByte();
                        
                        a.TypeName = objectIndexToName[(int)a.TypeId.Value];
                        var classMap = ObjectNameToClassNetCache(a.TypeName, objectIndexToName, classNetCache);
                        a.ClassName = objectIndexToName[classMap.ObjectIndex];

                        if (a.ClassName == "TAGame.CrowdActor_TA"
                            || a.ClassName == "TAGame.CrowdManager_TA"
                            || a.ClassName == "TAGame.VehiclePickup_Boost_TA")
                        {
                            a.KnownBits = br.GetBits(startPosition, br.Position - startPosition);
                            a.Complete = true;
                            return a;
                        }

                        if (a.ClassName == "TAGame.Ball_TA")
                        {
                            a.Position = Vector3D.Deserialize(4, br);
                        }
                        else if (a.ClassName == "TAGame.Car_TA"
                            || a.ClassName == "TAGame.CarComponent_Boost_TA"
                            || a.ClassName == "TAGame.CarComponent_Jump_TA"
                            || a.ClassName == "TAGame.CarComponent_DoubleJump_TA"
                            || a.ClassName == "TAGame.CarComponent_Dodge_TA"
                            || a.ClassName == "TAGame.CarComponent_FlipCar_TA")
                        {
                            a.Position = Vector3D.Deserialize(10, br);
                        }
                        else
                        {
                            a.Position = Vector3D.Deserialize(br);
                        }

                        if (a.ClassName == "Core.Object"
                            || a.ClassName == "Engine.GameReplicationInfo"
                            || a.ClassName == "TAGame.GameEvent_SoccarSplitscreen_TA"
                            || a.ClassName == "TAGame.CarComponent_Boost_TA"
                            || a.ClassName == "TAGame.CarComponent_Jump_TA"
                            || a.ClassName == "TAGame.CarComponent_DoubleJump_TA"
                            || a.ClassName == "TAGame.CarComponent_Dodge_TA"
                            || a.ClassName == "TAGame.CarComponent_FlipCar_TA"
                            || a.ClassName == "TAGame.Team_TA"
                            || a.ClassName == "TAGame.PRI_TA")
                        {
                            a.Complete = true;
                        }
                        /*
                        if (a.TypeId == 44 
                            || a.TypeId == 69
                            || a.TypeId == 80
                            || a.TypeId == 81
                            || a.TypeId == 139) // probably should be base on name?
                        {
                            // these all have very similar, if not identical, data
                            for (int i = 0; i < 35 - 24 - bitsRead; ++i)
                            {
                                a.UnknownBits.Add(br.ReadBit());
                            }
                            a.Complete = true;
                        }
                        else */
                        else if (a.ClassName == "TAGame.Ball_TA"
                            || a.ClassName == "TAGame.Car_TA")
                        {
                            if (br.ReadBit())
                            {
                                br.ReadByte();
                            }
                            if (br.ReadBit())
                            {
                                br.ReadByte();
                            }
                            if (br.ReadBit())
                            {
                                br.ReadByte();
                            }
                            
                            a.Complete = true;
                        }
                        else
                        {

                            //a.Position = Vector3D.Deserialize(br);
                            //a.Rotation = Vector3D.Deserialize(br);
                        }
                    }
                    else
                    {
                        a.State = "Existing";

                        a.TypeId = existingActorStates.Where(x => x.Id == a.Id).Single().TypeId;
                        a.TypeName = objectIndexToName[(int)a.TypeId.Value];
                        var classMap = ObjectNameToClassNetCache(a.TypeName, objectIndexToName, classNetCache);
                        a.ClassName = objectIndexToName[classMap.ObjectIndex];

                        a.Properties = new List<ActorStateProperty>(); 
                        ActorStateProperty lastProp = null;
                        while ((lastProp == null || lastProp.IsComplete) && br.ReadBit())
                        {
                            lastProp = ActorStateProperty.Deserialize(classMap, objectIndexToName, br);
                            a.Properties.Add(lastProp);
                        }
                        a.Complete = lastProp.IsComplete;
                        var endPosition = br.Position;
                    }
                }
                else
                {
                    a.State = "Deleted";

                    var endPosition = br.Position;
                }
            }
            catch(Exception)
            {
                // eat exceptions for now
                int g = 56;
                a.Failed = true;
            }
            finally
            {
                a.KnownBits = br.GetBits(startPosition, br.Position - startPosition);
            }

            return a; 
        }

        public string ToDebugString(string[] objects)
        {
            var s = string.Format("ActorState: Id {0} State {1}\r\n", Id, State);
            if (TypeId != null)
            {
                if (objects != null)
                {
                    if ( TypeId >= objects.Length )
                    {
                        s += string.Format("    TypeID: {0} (BAD TYPE)\r\n",TypeId);
                    }
                    else
                    {
                        s += string.Format("    TypeID: {0} ({1})\r\n",TypeId, objects[TypeId.Value]);
                    }
                }
                else
                {
                    s += string.Format("    TypeID: {0}\r\n",TypeId);
                }
            }

            s += string.Format("    TypeName: {0}\r\n", TypeName);

            s += string.Format("    ClassName: {0}\r\n", ClassName);

            if (Position != null)
            {
                s += string.Format("    Position: {0}\r\n", Position.ToDebugString());
            }

            if (Rot1 != null)
            {
                s += string.Format("    Rotation: {0} {1} {2}\r\n", Rot1, Rot2, Rot3);
            }

            if (UnknownVector1 != null)
            {
                s += string.Format("    UnknownVector1: {0}\r\n", UnknownVector1.ToDebugString());
            }

            if (UnknownVector2 != null)
            {
                s += string.Format("    UnknownVector2: {0}\r\n", UnknownVector2.ToDebugString());
            }

            if (Properties != null)
            {
                foreach(var p in Properties)
                {
                    s += "    " + p.ToDebugString();
                }
            }

            if (KnownBits != null && KnownBits.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < KnownBits.Count; ++i)
                {
                    sb.Append((KnownBits[i] ? 1 : 0).ToString());
                }

                s += string.Format("    KnownBits: {0}\r\n", sb.ToString());
            }

            return s;
        }
    }
}
