using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class ActorStateProperty
    {
        public Int32 PropertyId { get; private set; }
        public string PropertyName { get; private set; }
        public List<object> Data { get; private set; }
        public List<bool> KnownBits { get; private set; }

        public bool IsComplete { get; private set; }

        public static ActorStateProperty Deserialize(IClassNetCache classMap, IDictionary<int, string> objectIndexToName, BitReader br)
        {
            var asp = new ActorStateProperty();
            var startPosition = br.Position;

            var maxPropId = classMap.MaxPropertyId;
            //var idBitLen = Math.Floor(Math.Log10(maxPropId) / Math.Log10(2)) + 1;

            var className = objectIndexToName[classMap.ObjectIndex];
            asp.PropertyId = br.ReadInt32Max(maxPropId + 1);// br.ReadInt32FromBits((int)idBitLen);
            asp.PropertyName = objectIndexToName[classMap.GetProperty(asp.PropertyId).Index];
            asp.Data = new List<object>();
            try
            {
                switch (asp.PropertyName)
                {
                    case "TAGame.GameEvent_TA:ReplicatedStateIndex":
                        asp.Data.Add(br.ReadInt32Max(140)); // number is made up, I dont know the max yet
                        asp.IsComplete = true;
                        break;
                    case "TAGame.RBActor_TA:ReplicatedRBState":
                        
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(Vector3D.Deserialize2(20, br));

                        var rot = Vector3D.DeserializeFixed(br);
                        asp.Data.Add(rot);
                        // Sometimes these two vectors are missing?
                        // Wild guess: They're momentum vectors, and only there when moving?
                        // Well, how do I know they're moving without momentum vectors... hm.
                        if (!(rot.X < -1 && rot.Y < -1 && rot.Z < -1))
                        {
                            asp.Data.Add(Vector3D.Deserialize2(20, br));
                            asp.Data.Add(Vector3D.Deserialize2(20, br));
                        }

                        asp.IsComplete = true;
                        break;
                    case "TAGame.Team_TA:GameEvent":
                    case "TAGame.CrowdActor_TA:ReplicatedOneShotSound":
                    case "TAGame.CrowdManager_TA:ReplicatedGlobalOneShotSound":
                    case "Engine.Actor:Owner":
                    case "TAGame.GameEvent_Soccar_TA:RoundNum":
                    case "Engine.GameReplicationInfo:GameClass":
                    case "TAGame.GameEvent_TA:BotSkill":
                    case "Engine.PlayerReplicationInfo:Team":
                        asp.Data.Add(br.ReadBit()); 
                        asp.Data.Add(br.ReadInt32());
                        asp.IsComplete = true;
                        break;
                    case "TAGame.CarComponent_TA:Vehicle":
                        asp.Data.Add(br.ReadBit());
                        if (className == "TAGame.CarComponent_Jump_TA"
                            || className == "TAGame.CarComponent_FlipCar_TA"
                            || className == "TAGame.CarComponent_Boost_TA"
                            || className == "TAGame.CarComponent_Dodge_TA"
                            || className == "TAGame.CarComponent_DoubleJump_TA")
                        {
                            asp.Data.Add(br.ReadInt32());
                        }
                        else
                        {
                            asp.Data.Add(br.ReadByte());
                        }
                        asp.IsComplete = true;
                        break;
                    case "Engine.PlayerReplicationInfo:PlayerName":
                    case "Engine.GameReplicationInfo:ServerName":
                        asp.Data.Add(br.ReadString());
                        asp.IsComplete = true;
                        break;
                    case "TAGame.GameEvent_Soccar_TA:SecondsRemaining":
                    case "TAGame.GameEvent_TA:ReplicatedGameStateTimeRemaining":
                    case "TAGame.CrowdActor_TA:ReplicatedCountDownNumber":
                    case "TAGame.CrowdActor_TA:ModifiedNoise":
                    case "TAGame.GameEvent_Team_TA:MaxTeamSize":
                        asp.Data.Add(br.ReadInt32());
                        asp.IsComplete = true;
                        break;
                    case "TAGame.VehiclePickup_TA:ReplicatedPickupData":
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadInt32());
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "Engine.Actor:bNetOwner":
                    case "Engine.Actor:bBlockActors":
                        // this doesnt look right...
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "Engine.Pawn:DrivenVehicle":
                        asp.Data.Add(br.ReadInt32());
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "Engine.PlayerReplicationInfo:Ping":
                    case "Engine.Actor:DrawScale":

                    case "TAGame.Vehicle_TA:ReplicatedSteer":
                    case "TAGame.Vehicle_TA:ReplicatedThrottle":
                    case "TAGame.PRI_TA:CameraYaw":
                    case "TAGame.PRI_TA:CameraPitch":
                        asp.Data.Add(br.ReadByte());
                        asp.IsComplete = true;
                        break;
                    case "Engine.Actor:Location":
                        asp.Data.Add(Vector3D.Deserialize2(20, br));
                        asp.IsComplete = true;
                        break;
                    
                    case "Engine.Actor:bCollideWorld":
                    case "Engine.PlayerReplicationInfo:bReadyToPlay":
                        //asp.Data.Add(Vector3D.Deserialize(5, br));
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "TAGame.PRI_TA:bUsingBehindView":
                    case "TAGame.PRI_TA:bUsingSecondaryCamera":
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "TAGame.CarComponent_TA:ReplicatedActive":
                        asp.Data.Add(br.ReadBit());
                        if ( (bool)asp.Data[0])
                        {
                            asp.Data.Add(br.ReadInt32FromBits(7));
                        }
                        asp.IsComplete = true;
                        break;
                    case "Engine.Actor:Role":
                        asp.Data.Add(br.ReadInt32FromBits(11));
                        asp.IsComplete = true;
                        break;
                    case "Engine.Actor:RelativeRotation": //SWAG
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadByte());
                        asp.Data.Add(br.ReadInt32());
                        asp.IsComplete = true;
                        break;
                    case "Engine.PlayerReplicationInfo:UniqueId":
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadByte());
                        asp.IsComplete = true;
                        break;
                }
            }
            catch(Exception)
            {
                asp.Data.Add("FAILED");
            }
            finally
            {
                asp.KnownBits = br.GetBits(startPosition, br.Position - startPosition);
            }

            return asp;
        }

        private static List<object> ReadData(int numBytes, int numBits, BitReader br)
        {
            List<object> data = new List<object>();
            for (int i = 0; i < numBytes; ++i)
            {
                data.Add(br.ReadByte());
            }
            for (int i = 0; i < numBits; ++i)
            {
                data.Add(br.ReadBit());
            }
            return data;
        }

        public string ToDebugString()
        {
            var s = string.Format("Property: ID {0} Name {1}\r\n", PropertyId, PropertyName);
            s += "    Data: " + string.Join(", ", Data) + "\r\n";
            
            // You know you should really functionify this, right?
            // yeah.
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
            //return string.Join(", ", Data);

        }
    }
}
