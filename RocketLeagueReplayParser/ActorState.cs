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
        public Int32? TypeId { get; private set; }
        public string TypeName { get; private set; }
        public string ClassName { get; private set; }

        public Vector3D Position { get; private set; }

        public List<ActorStateProperty> Properties { get; private set; }

        public List<bool> KnownBits { get; set; }
        public List<bool> UnknownBits { get; set; }


        public bool Complete { get; set; } // Set to true when we're sure we read the whole thing
        public bool ForcedComplete { get; set; } // Set to true externally if we found a way to skip to the next ActorState
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
               /* var maxId = existingActorStates.Any() ? existingActorStates.Max(x => x.Id) : -1;
                if (actorId > (maxId + 20))
                {
                    // we're probably lost. Awwww.
                    a.KnownBits = br.GetBits(startPosition, br.Position - startPosition);
                    return a;
                }
                */
                frameActorStates.Add(a);

                if (br.ReadBit())
                {
                    if (br.ReadBit())
                    {
                        a.State = "New";
                        a.Unknown1 = br.ReadBit();

                        a.TypeId = br.ReadInt32();

                        a.TypeName = objectIndexToName[(int)a.TypeId.Value];
                        var classMap = ObjectNameToClassNetCache(a.TypeName, objectIndexToName, classNetCache);
                        a.ClassName = objectIndexToName[classMap.ObjectIndex];

                        // Come up with something more foolproof for this
                        /*
                        if ( a.TypeName.Contains(":"))
                        {
                            // Nope, we think we're a property type. We're lost!
                            a.Failed = true;
                            return a;
                        }
                         * */


                        if (a.ClassName == "TAGame.CrowdActor_TA"
                            || a.ClassName == "TAGame.CrowdManager_TA"
                            || a.ClassName == "TAGame.VehiclePickup_Boost_TA"
                            || a.ClassName == "Core.Object")
                        {
                            a.KnownBits = br.GetBits(startPosition, br.Position - startPosition);
                            a.Complete = true;
                            return a;
                        }

                        a.Position = Vector3D.Deserialize(br);

                        if (a.ClassName == "Engine.GameReplicationInfo"
                            || a.ClassName == "TAGame.GameEvent_SoccarSplitscreen_TA"
                            || a.ClassName == "TAGame.CarComponent_Boost_TA"
                            || a.ClassName == "TAGame.CarComponent_Jump_TA"
                            || a.ClassName == "TAGame.CarComponent_DoubleJump_TA"
                            || a.ClassName == "TAGame.CarComponent_Dodge_TA"
                            || a.ClassName == "TAGame.CarComponent_FlipCar_TA"
                            || a.ClassName == "TAGame.Team_TA"
                            || a.ClassName == "TAGame.PRI_TA"
                            || a.ClassName == "TAGame.GameEvent_Soccar_TA")
                        {
                            a.Complete = true;
                        }
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
                        if ( lastProp.Data.Count > 0 && lastProp.Data.Last().ToString() == "FAILED")
                        {
                            a.Failed = true;
                        }
                        var endPosition = br.Position;
                    }
                }
                else
                {
                    a.State = "Deleted";

                    var actor = existingActorStates.Where(x => x.Id == a.Id).SingleOrDefault();
                    if (actor != null) // TODO remove this someday. Only here because we might be deleting objects we havent figured out how to parse yet
                    {
                        a.TypeId = actor.TypeId;
                        a.TypeName = objectIndexToName[(int)a.TypeId.Value];
                        var classMap = ObjectNameToClassNetCache(a.TypeName, objectIndexToName, classNetCache);
                        a.ClassName = objectIndexToName[classMap.ObjectIndex];
                    }
                    a.Complete = true;
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
                    if ( TypeId < 0 || TypeId >= objects.Length )
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

            if (Properties != null)
            {
                foreach(var p in Properties)
                {
                    s += "    " + p.ToDebugString();
                }
            }

            if (KnownBits != null && KnownBits.Count > 0)
            {
                s += string.Format("    KnownBits: {0}\r\n", KnownBits.ToBinaryString());
            }
            if (UnknownBits != null && UnknownBits.Count > 0)
            {
                s += string.Format("    UnknownBits: {0}\r\n", UnknownBits.ToBinaryString());
            }

            return s;
        }
    }
}
