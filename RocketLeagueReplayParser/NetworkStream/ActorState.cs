using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public enum ActorStateState // Ugh what a horrible name.
    {
        Deleted,
        New,
        Existing
    }

    public class ActorState
    {
        public int Id { get; private set; }
        public ActorStateState State { get; private set; } 
        public bool Unknown1 { get; private set; }
        public Int32? TypeId { get; private set; }
        public string TypeName { get; private set; }
        public string ClassName { get; private set; }

        public Vector3D Position { get; private set; }

        public List<ActorStateProperty> Properties { get; private set; }

#if DEBUG
        private List<bool> KnownBits { get; set; }
        private List<bool> UnknownBits { get; set; }


        public bool Complete { get; set; } // Set to true when we're sure we read the whole thing
        public bool ForcedComplete { get; set; } // Set to true externally if we found a way to skip to the next ActorState
        public bool Failed { get; private set; }
#endif
        
        public static ClassNetCache ObjectNameToClassNetCache(string objectName, string[] objectIdToName, IEnumerable<ClassNetCache> classNetCache)
        {
            // TODO: Make these manual conversions less messy
            if (objectName == "Archetypes.Ball.Ball_Basketball") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.Ball_TA").Single();
            if (objectName == "Archetypes.Ball.Ball_Puck") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.Ball_TA").Single();
            if (objectName == "Archetypes.Ball.CubeBall") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.Ball_TA").Single();
            if (objectName == "Archetypes.GameEvent.GameEvent_Basketball") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.GameEvent_Soccar_TA").Single();
            if (objectName == "Archetypes.GameEvent.GameEvent_BasketballPrivate") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.GameEvent_SoccarPrivate_TA").Single();
            if (objectName == "Archetypes.GameEvent.GameEvent_BasketballSplitscreen") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.GameEvent_SoccarSplitscreen_TA").Single();
            if (objectName == "Archetypes.GameEvent.GameEvent_HockeyPrivate") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.GameEvent_SoccarPrivate_TA").Single();
            if (objectName == "Archetypes.GameEvent.GameEvent_HockeySplitscreen") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.GameEvent_SoccarSplitscreen_TA").Single();
            if (objectName == "Archetypes.GameEvent.GameEvent_Season:CarArchetype") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.Car_Season_TA").Single();
            if (objectName == "GameInfo_Basketball.GameInfo.GameInfo_Basketball:GameReplicationInfoArchetype") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.GRI_TA").Single();
            if (objectName == "Gameinfo_Hockey.GameInfo.Gameinfo_Hockey:GameReplicationInfoArchetype") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.GRI_TA").Single();
            if (objectName == "GameInfo_Season.GameInfo.GameInfo_Season:GameReplicationInfoArchetype") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.GRI_TA").Single();
            if (objectName == "GameInfo_Soccar.GameInfo.GameInfo_Soccar:GameReplicationInfoArchetype") return classNetCache.Where(x => objectIdToName[x.ObjectIndex] == "TAGame.GRI_TA").Single();

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
                throw new NotSupportedException("Cant convert the following type to a class yet: " + objectName);
            }
            return matches.Single();
        }

        public static ActorState Deserialize(int maxChannels, List<ActorState> existingActorStates, List<ActorState> frameActorStates, string[] objectIndexToName, IEnumerable<ClassNetCache> classNetCache, BitReader br)
        {
            var startPosition = br.Position;
			ActorState a = new ActorState();

			try
			{
                var actorId = br.ReadInt32Max(maxChannels);

				a.Id = actorId;

				if (br.ReadBit())
				{
					if (br.ReadBit())
					{
						a.State = ActorStateState.New;
						a.Unknown1 = br.ReadBit();

						a.TypeId = br.ReadInt32();

						a.TypeName = objectIndexToName[(int)a.TypeId.Value];
						var classMap = ObjectNameToClassNetCache(a.TypeName, objectIndexToName, classNetCache);
						a.ClassName = objectIndexToName[classMap.ObjectIndex];

						if (a.ClassName == "TAGame.CrowdActor_TA"
							|| a.ClassName == "TAGame.CrowdManager_TA"
							|| a.ClassName == "TAGame.VehiclePickup_Boost_TA"
							|| a.ClassName == "Core.Object")
						{
#if DEBUG
							a.KnownBits = br.GetBits(startPosition, br.Position - startPosition);
							a.Complete = true;
#endif
							return a;
						}

						a.Position = Vector3D.Deserialize(br);

                        if (a.ClassName == "TAGame.Ball_TA"
                            || a.ClassName == "TAGame.Car_TA"
                            || a.ClassName == "TAGame.Car_Season_TA")
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
                        }

#if DEBUG
                        a.Complete = true;
#endif
					}
					else
					{
						a.State = ActorStateState.Existing;
						a.TypeId = existingActorStates.Where(x => x.Id == a.Id).Single().TypeId;
						a.TypeName = objectIndexToName[(int)a.TypeId.Value];
						var classMap = ObjectNameToClassNetCache(a.TypeName, objectIndexToName, classNetCache);
						a.ClassName = objectIndexToName[classMap.ObjectIndex];

						a.Properties = new List<ActorStateProperty>(); 
						ActorStateProperty lastProp = null;
						while (br.ReadBit())
						{
							lastProp = ActorStateProperty.Deserialize(classMap, objectIndexToName, br);
							a.Properties.Add(lastProp);

#if DEBUG
							if (!lastProp.IsComplete)
							{
								break;
							}
#endif
						}

#if DEBUG
						a.Complete = lastProp.IsComplete;
						if (lastProp.Data.Count > 0 && lastProp.Data.Last().ToString() == "FAILED")
						{
							a.Failed = true;
						}
#endif
						var endPosition = br.Position;
					}
				}
				else
				{
					a.State = ActorStateState.Deleted;

					var actor = existingActorStates.Where(x => x.Id == a.Id).SingleOrDefault();
					if (actor != null) // TODO remove this someday. Only here because we might be deleting objects we havent figured out how to parse yet
					{
						a.TypeId = actor.TypeId;
						a.TypeName = objectIndexToName[(int)a.TypeId.Value];
						var classMap = ObjectNameToClassNetCache(a.TypeName, objectIndexToName, classNetCache);
						a.ClassName = objectIndexToName[classMap.ObjectIndex];
					}
#if DEBUG
					a.Complete = true;
#endif
					var endPosition = br.Position;
				}
#if DEBUG
				if (!a.Complete)
				{
					// Read a bunch of data so we have something to look at in the logs
					// Otherwise the logs may not show any data bits for whatever is broken, which is hard to interpret
					br.ReadBytes(16);
				}

				a.KnownBits = br.GetBits(startPosition, br.Position - startPosition);
#endif
				return a;
			}
			catch(Exception e)
			{
#if DEBUG
                a.KnownBits = br.GetBits(startPosition, br.Position - startPosition);
                a.UnknownBits = br.GetBits(br.Position, 100);
                Console.WriteLine(e.ToString());
				a.Failed = true;
				a.Complete = false;
				return a;
#else
				throw;
#endif
			}
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
#if DEBUG
            if (KnownBits != null && KnownBits.Count > 0)
            {
                s += string.Format("    KnownBits: {0}\r\n", KnownBits.ToBinaryString());
            }
            if (UnknownBits != null && UnknownBits.Count > 0)
            {
                s += string.Format("    UnknownBits: {0}\r\n", UnknownBits.ToBinaryString());
            }
            if ( ForcedComplete )
            {
                s += "    Forced Complete!";
            }
            if (!Complete)
            {
                s += "    Incomplete!";
            }
#endif

            return s;
        }
    }
}
