using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ActorStateProperty
    {
        public Int32 PropertyId { get; private set; }
        private Int32 MaxPropertyId { get; set; }
        public string PropertyName { get; private set; }
        public List<object> Data { get; private set; }
        private List<bool> KnownDataBits { get;  set; }

        public bool IsComplete { get; private set; }

        public static ActorStateProperty Deserialize(IClassNetCache classMap, IDictionary<int, string> objectIndexToName, BitReader br)
        {
            var asp = new ActorStateProperty();

            var maxPropId = classMap.MaxPropertyId;
            //var idBitLen = Math.Floor(Math.Log10(maxPropId) / Math.Log10(2)) + 1;

            var className = objectIndexToName[classMap.ObjectIndex];
            asp.PropertyId = br.ReadInt32Max(maxPropId + 1);// br.ReadInt32FromBits((int)idBitLen);
            asp.MaxPropertyId = maxPropId;
            asp.PropertyName = objectIndexToName[classMap.GetProperty(asp.PropertyId).Index];

            var startPosition = br.Position;

            asp.Data = new List<object>();

            switch (asp.PropertyName)
            {
                case "TAGame.GameEvent_TA:ReplicatedStateIndex":
                    asp.Data.Add(br.ReadInt32Max(140)); // number is made up, I dont know the max yet // TODO: Revisit this. It might work well enough, but looks fishy
                    asp.IsComplete = true;
                    break;
                case "TAGame.RBActor_TA:ReplicatedRBState":
                    asp.Data.Add(RigidBodyState.Deserialize(br));
                    asp.IsComplete = true;
                    break;
                case "TAGame.Team_TA:GameEvent":
                case "TAGame.CrowdActor_TA:ReplicatedOneShotSound":
                case "TAGame.CrowdManager_TA:ReplicatedGlobalOneShotSound":
                case "Engine.Actor:Owner":
                case "Engine.GameReplicationInfo:GameClass":
                case "Engine.PlayerReplicationInfo:Team":
                case "TAGame.CrowdManager_TA:GameEvent":
                case "Engine.Pawn:PlayerReplicationInfo": // Actor Id. Ties cars to players
                case "TAGame.PRI_TA:ReplicatedGameEvent":
                case "TAGame.Ball_TA:GameEvent":
                case "Engine.Actor:ReplicatedCollisionType":
                case "TAGame.CrowdActor_TA:GameEvent":
                case "TAGame.Team_TA:LogoData":
                    asp.Data.Add(br.ReadBit()); 
                    asp.Data.Add(br.ReadInt32());
                    asp.IsComplete = true;
                    break;
                case "TAGame.CarComponent_TA:Vehicle":
                    // 110101111 // TAGame.CarComponent_Jump_TA
                    // 100111111 // TAGame.CarComponent_FlipCar_TA
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
                    
                case "Engine.GameReplicationInfo:ServerName":
                case "Engine.PlayerReplicationInfo:PlayerName":
                case "TAGame.Team_TA:CustomTeamName":
                    asp.Data.Add(br.ReadString());
                    asp.IsComplete = true;
                    break;
                case "TAGame.GameEvent_Soccar_TA:SecondsRemaining":
                case "TAGame.GameEvent_TA:ReplicatedGameStateTimeRemaining":
                case "TAGame.CrowdActor_TA:ReplicatedCountDownNumber":
                case "TAGame.GameEvent_Team_TA:MaxTeamSize":
                case "Engine.PlayerReplicationInfo:PlayerID":
                case "TAGame.PRI_TA:TotalXP":
                case "TAGame.PRI_TA:MatchScore":
                case "TAGame.GameEvent_Soccar_TA:RoundNum":
                case "TAGame.GameEvent_TA:BotSkill":
                case "TAGame.PRI_TA:MatchShots":
                case "TAGame.PRI_TA:MatchSaves":
                case "ProjectX.GRI_X:ReplicatedGamePlaylist":
                case "Engine.TeamInfo:Score":
                case "Engine.PlayerReplicationInfo:Score":
                case "TAGame.PRI_TA:MatchGoals":
                case "TAGame.PRI_TA:MatchAssists":
				case "ProjectX.GRI_X:ReplicatedGameMutatorIndex":
                case "TAGame.PRI_TA:Title":
                    asp.Data.Add(br.ReadInt32());
                    asp.IsComplete = true;
                    break;
                case "TAGame.VehiclePickup_TA:ReplicatedPickupData":
                    // 1011101000000000000000000000000001
                    // 0111111111111111111111111111111110
                    // 1111001000000000000000000000000001
                    // 1000001000000000000000000000000001
                    // 1111110000000000000000000000000001
                    // 1101110000000000000000000000000001
                    // 111111111
                    // 100000001
                    // 101001111

                    // reverify the above data, especially the short stuff
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadInt32());
                    asp.Data.Add(br.ReadBit());


                    /*
                    var bit1 = br.ReadBit();
                    var byt = br.ReadByte();
                    var bit2 = (byt & 0x80) > 0;
                    if (bit1 == bit2)
                    {
                        asp.Data.Add(bit1);
                        asp.Data.Add(byt);
                    }
                    else
                    {
                        asp.Data.Add(bit1);
                        var bytes = new byte[4];
                        bytes[0] = byt;
                        bytes[1] = br.ReadByte();
                        bytes[2] = br.ReadByte();
                        bytes[3] = br.ReadByte();
                        asp.Data.Add(BitConverter.ToInt32(bytes, 0));
                        asp.Data.Add(br.ReadBit());
                    }
                        * */
                    asp.IsComplete = true;
                    break;
                    
                case "Engine.Pawn:DrivenVehicle":
                    asp.Data.Add(br.ReadInt32());
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadBit());
                    asp.IsComplete = true;
                    break;
                case "Engine.PlayerReplicationInfo:Ping":
                case "TAGame.Vehicle_TA:ReplicatedSteer":
                case "TAGame.Vehicle_TA:ReplicatedThrottle":
                case "TAGame.PRI_TA:CameraYaw":
                case "TAGame.PRI_TA:CameraPitch":
                case "TAGame.Ball_TA:HitTeamNum":
                case "TAGame.GameEvent_Soccar_TA:ReplicatedScoredOnTeam":
                    asp.Data.Add(br.ReadByte());
                    asp.IsComplete = true;
                    break;
                case "Engine.Actor:Location":
                case "TAGame.CarComponent_Dodge_TA:DodgeTorque":
                    asp.Data.Add(Vector3D.Deserialize(br));
                    asp.IsComplete = true;
                    break;
                    
                case "Engine.Actor:bCollideWorld":
                case "Engine.PlayerReplicationInfo:bReadyToPlay":
                case "TAGame.Vehicle_TA:bReplicatedHandbrake":
                case "TAGame.Vehicle_TA:bDriving":
                case "Engine.Actor:bNetOwner":
                case "Engine.Actor:bBlockActors":
                case "TAGame.GameEvent_TA:bHasLeaveMatchPenalty":
                case "TAGame.PRI_TA:bUsingBehindView":
                case "TAGame.PRI_TA:bUsingSecondaryCamera":
                case "TAGame.GameEvent_TA:ActivatorCar":
                case "TAGame.GameEvent_Soccar_TA:bOverTime":
                case "ProjectX.GRI_X:bGameStarted":
                case "Engine.Actor:bCollideActors":
                case "TAGame.PRI_TA:bReady":
                case "TAGame.RBActor_TA:bFrozen":
                case "Engine.Actor:bHidden":
                case "Engine.Actor:bTearOff": // might not be used, parser might have been lost
                case "TAGame.CarComponent_FlipCar_TA:bFlipRight":
                case "Engine.PlayerReplicationInfo:bBot":
                case "Engine.PlayerReplicationInfo:bWaitingPlayer":
                case "TAGame.RBActor_TA:bReplayActor":
                case "TAGame.PRI_TA:bIsInSplitScreen":
                case "Engine.GameReplicationInfo:bMatchIsOver":
                case "TAGame.CarComponent_Boost_TA:bUnlimitedBoost":
                case "Engine.PlayerReplicationInfo:bIsSpectator":
                    asp.Data.Add(br.ReadBit());
                    asp.IsComplete = true;
                    break;
                case "TAGame.CarComponent_TA:ReplicatedActive":
                    // example data
                    // 0111111111111111111111111111111110

                    asp.Data.Add(br.ReadByte());
                    /*
                    asp.Data.Add(br.ReadBit());
                    if ( (bool)asp.Data[0])
                    {
                        asp.Data.Add(br.ReadInt32FromBits(7));
                    }*/
                    asp.IsComplete = true;
                    break;
                case "Engine.Actor:Role":
                    asp.Data.Add(br.ReadInt32FromBits(11));
                    asp.IsComplete = true;
                    break;
                case "Engine.PlayerReplicationInfo:UniqueId":
                case "TAGame.PRI_TA:PartyLeader":
                    asp.Data.Add(UniqueId.Deserialize(br));
                    asp.IsComplete = true;
                    break;
                case "TAGame.PRI_TA:ClientLoadout":
                    asp.Data.Add(ClientLoadout.Deserialize(br));
                    asp.IsComplete = true;
                    break;
                case "TAGame.PRI_TA:CameraSettings":
                    asp.Data.Add(CameraSettings.Deserialize(br));
                    asp.IsComplete = true;
                    break;
                case "TAGame.Car_TA:TeamPaint":
                    asp.Data.Add(TeamPaint.Deserialize(br));
                    asp.IsComplete = true;
                    break;
                case "ProjectX.GRI_X:GameServerID":
                    asp.Data.Add(br.ReadInt32());
                    asp.Data.Add(br.ReadInt32());
                    asp.IsComplete = true;
                    break;
                case "ProjectX.GRI_X:Reservations":
                    asp.Data.Add(Reservation.Deserialize(br));
                    asp.IsComplete = true;
                    break;
                case "TAGame.Ball_TA:ReplicatedExplosionData":
                    // 0 01010111000000000000000000000000 0011 01010010000001 01111010001011 00000110000001
                    // 0 01110111000000000000000000000000 0011 10011110010001 11111010001011 01010110000001
                    // 0 11010111000000000000000000000000 0011 00001111011110 11000110001011 10111010000001
                    // 0 11010111000000000000000000000000 0011 10011000100001 00100110001011 10111010000001
                    // 0 00110111000000000000000000000000 0011 00000001010001 11000110001011 00100110000001
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadInt32());
                    asp.Data.Add(Vector3D.Deserialize(br));
                    asp.IsComplete = true;
                    break;
                case "TAGame.Car_TA:ReplicatedDemolish":
                    asp.Data.Add(ReplicatedDemolish.Deserialize(br));
                    asp.IsComplete = true;
                    break;
                case "TAGame.GameEvent_Soccar_TA:ReplicatedMusicStinger":
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadByte());
                    asp.Data.Add(br.ReadInt32());
                    asp.IsComplete = true;
                    break;
                case "TAGame.CarComponent_FlipCar_TA:FlipCarTime":
				case "TAGame.Ball_TA:ReplicatedBallScale":
				case "TAGame.CarComponent_Boost_TA:RechargeDelay":
				case "TAGame.CarComponent_Boost_TA:RechargeRate":
                case "TAGame.Ball_TA:ReplicatedAddedCarBounceScale":
                case "TAGame.Ball_TA:ReplicatedBallMaxLinearSpeedScale":
                case "TAGame.Ball_TA:ReplicatedWorldBounceScale":
                case "TAGame.CarComponent_Boost_TA:BoostModifier":
                case "Engine.Actor:DrawScale":
                case "TAGame.CrowdActor_TA:ModifiedNoise":
					asp.Data.Add(br.ReadFloat());
                    asp.IsComplete = true;
                    break;
                case "TAGame.GameEvent_SoccarPrivate_TA:MatchSettings":
                    asp.Data.Add(PrivateMatchSettings.Deserialize(br));
                    asp.IsComplete = true;
                    break;
				default:
                    throw new NotSupportedException(string.Format("Unknown property {0}. Next bits in the data are {1}. Figure it out!", asp.PropertyName, br.GetBits(br.Position, Math.Min(128, br.Length - br.Position)).ToBinaryString()));
            }

            asp.KnownDataBits = br.GetBits(startPosition, br.Position - startPosition);

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
            s += "    Max Prop Id: " + MaxPropertyId.ToString() + "\r\n";
            s += "    Data: " + string.Join(", ", Data) + "\r\n";
            
            // You know you should really functionify this, right?
            // yeah.
            if (KnownDataBits != null && KnownDataBits.Count > 0)
            {
                s += string.Format("    KnownDataBits: {0}\r\n", KnownDataBits.ToBinaryString());
            }
            return s;
            //return string.Join(", ", Data);

        }
    }
}
