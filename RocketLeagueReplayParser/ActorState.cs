using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class ActorState
    {
        public int Id { get; private set; }
        public string State { get; private set; } // TODO: Enumify someday
        public bool Unknown1 { get; private set; } // Potentially to signal more data in a pakced int, but that doesnt seem right
        public byte? TypeId { get; private set; } // Internet says this is a packed int. But if I justr read a byte here, looks perfect

        public byte? Rot1 { get; private set; }
        public byte? Rot2 { get; private set; }
        public byte? Rot3 { get; private set; }
        public Vector3D Position { get; private set; }
        public Vector3D UnknownVector1 { get; private set; }
        public Vector3D UnknownVector2 { get; private set; }

        public List<ActorStateProperty> Properties { get; private set; }

        public List<bool> KnownBits { get; set; }
        public List<bool> UnknownBits { get; set; }


        public bool Complete { get; private set; } // Set to true when we're sure we read the whole thing

        public static ActorState Deserialize(BitReader br)
        {
            var a = new ActorState();
            var startPosition = br.Position;

            a.Id = br.ReadInt32FromBits(10);
            if ( br.ReadBit() )
            {
                if ( br.ReadBit() )
                {
                    a.State = "New";
                    a.Unknown1 = br.ReadBit();
                    a.TypeId = br.ReadByte();
                    a.Rot1 = br.ReadByte();
                    a.Rot2 = br.ReadByte();
                    a.Rot3 = br.ReadByte();

                    

                    int bitsRead = 0;

                    if (a.TypeId == 198 // eurostad_oob_audio_map.TheWorld:PersistentLevel.CrowdActor_TA_0
                        || a.TypeId == 202 // eurostad_oob_audio_map.TheWorld:PersistentLevel.CrowdManager_TA_0
                        || a.TypeId == 232 // eurostadium_p.TheWorld:PersistentLevel.VehiclePickup_Boost_TA_43
                        || a.TypeId == 233 // (eurostadium_p.TheWorld:PersistentLevel.VehiclePickup_Boost_TA_41)
                        || a.TypeId == 235) // (eurostadium_p.TheWorld:PersistentLevel.VehiclePickup_Boost_TA_19)
                    {
                        a.Complete = true;
                        return a;
                    }

                    a.Position = Vector3D.Deserialize(br, out bitsRead);

                    var endPosition = br.Position;
                    a.KnownBits = br.GetBits(startPosition, endPosition - startPosition);

                    a.UnknownBits = new List<bool>();

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
                    else if (a.TypeId == 183) //Archetypes.Ball.Ball_Default
                    {
                        // I bet these have a higher scale factor. Getting bit counts greater than 20
                        if ( br.ReadBit() )
                        {
                            a.UnknownVector1 = Vector3D.Deserialize(br, out bitsRead); 
                        }
                        if (br.ReadBit())
                        {
                            a.UnknownVector1 = Vector3D.Deserialize(br, out bitsRead);
                        }
                        endPosition = br.Position;
                        a.KnownBits = br.GetBits(startPosition, endPosition - startPosition);
                        a.Complete = true;
                    }
                     
                    else if (a.TypeId == 215 // Archetypes.CarComponents.CarComponent_Boost
                        || a.TypeId == 217 // Archetypes.CarComponents.CarComponent_Jump
                        || a.TypeId == 219 // Archetypes.CarComponents.CarComponent_DoubleJump
                        || a.TypeId == 222) // Archetypes.CarComponents.CarComponent_Dodge
                    {
                        for (int i = 0; i < 70 - 24 - bitsRead; ++i)
                        {
                            a.UnknownBits.Add(br.ReadBit());
                        }
                        a.Complete = true;
                    }
                    else if (a.TypeId == 192) // Archetypes.Car.Car_Default
                    {
                        a.UnknownBits.Add(br.ReadBit());
                        for (int i = 0; i < (a.UnknownBits[0] ? 9 : 25); ++i)
                        {
                            a.UnknownBits.Add(br.ReadBit());
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
                    a.Properties = new List<ActorStateProperty>();
                    ActorStateProperty lastProp = null;
                    while ( lastProp == null || (lastProp.IsComplete && br.ReadBit()))
                    {
                        lastProp = ActorStateProperty.Deserialize(br);
                        a.Properties.Add(lastProp);
                    }

                    var endPosition = br.Position;
                    a.KnownBits = br.GetBits(startPosition, endPosition - startPosition);
                }
            }
            else
            {
                a.State = "Deleted";

                var endPosition = br.Position;
                a.KnownBits = br.GetBits(startPosition, endPosition - startPosition);
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

            if (UnknownBits != null && UnknownBits.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < UnknownBits.Count; ++i)
                {
                    sb.Append((UnknownBits[i] ? 1 : 0).ToString());
                }
                
                s += string.Format("    UnknownBits: {0}\r\n", sb.ToString());
            }

            return s;
        }
    }
}
