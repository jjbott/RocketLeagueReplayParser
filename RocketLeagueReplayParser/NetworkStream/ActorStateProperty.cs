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
        public string PropertyName { get; private set; }
        public List<object> Data { get; private set; }

#if DEBUG
		private Int32 MaxPropertyId { get; set; }
        private List<bool> KnownDataBits { get;  set; }
        public bool IsComplete { get; private set; }
#endif

        public static ActorStateProperty Deserialize(IClassNetCache classMap, string[] objectIndexToName, BitReader br)
        {
            var asp = new ActorStateProperty();

            var maxPropId = classMap.MaxPropertyId;

            var className = objectIndexToName[classMap.ObjectIndex];
            asp.PropertyId = br.ReadInt32Max(maxPropId + 1);
#if DEBUG
            asp.MaxPropertyId = maxPropId;
#endif
            asp.PropertyName = objectIndexToName[classMap.GetProperty(asp.PropertyId).Index];

            var startPosition = br.Position;

            asp.Data = new List<object>();

            switch (asp.PropertyName)
            {
                case "TAGame.GameEvent_TA:ReplicatedStateIndex":
                    asp.Data.Add(br.ReadInt32Max(140)); // number is made up, I dont know the max yet // TODO: Revisit this. It might work well enough, but looks fishy
                    asp.MarkComplete();
                    break;
                case "TAGame.RBActor_TA:ReplicatedRBState":
                    asp.Data.Add(RigidBodyState.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "TAGame.CrowdActor_TA:ReplicatedOneShotSound":
                case "TAGame.CrowdManager_TA:ReplicatedGlobalOneShotSound":
                case "Engine.GameReplicationInfo:GameClass":
                case "TAGame.CrowdManager_TA:GameEvent":
                case "TAGame.CrowdActor_TA:GameEvent":
                case "TAGame.Team_TA:LogoData":
                case "TAGame.CameraSettingsActor_TA:PRI":
                case "TAGame.PRI_TA:PersistentCamera":
                    // Theres a good chance that most of these can be moved to the next section
                    asp.Data.Add(br.ReadBit()); 
                    asp.Data.Add(br.ReadInt32());
                    asp.MarkComplete();
                    break;
                case "TAGame.Team_TA:GameEvent":
                case "TAGame.Ball_TA:GameEvent":
                case "Engine.PlayerReplicationInfo:Team":
                case "Engine.Pawn:PlayerReplicationInfo":
                case "TAGame.PRI_TA:ReplicatedGameEvent":
                case "TAGame.CarComponent_TA:Vehicle":
                    // TODO: Use a real class so it can be accessed normally.
                    // If Active == false, ActorId will be -1
                    asp.Data.Add(new { Active = br.ReadBit(), ActorId = br.ReadInt32() });
                    asp.MarkComplete();
                    break;                   
                case "Engine.GameReplicationInfo:ServerName":
                case "Engine.PlayerReplicationInfo:PlayerName":
                case "TAGame.Team_TA:CustomTeamName":
                    asp.Data.Add(br.ReadString());
                    asp.MarkComplete();
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
                case "TAGame.GameEvent_TA:ReplicatedStateName":
                    asp.Data.Add(br.ReadInt32());
                    asp.MarkComplete();
                    break;
                case "TAGame.VehiclePickup_TA:ReplicatedPickupData":
                    // 1011101000000000000000000000000001
                    // 0111111111111111111111111111111110
                    // 1111001000000000000000000000000001
                    // 1000001000000000000000000000000001
                    // 1111110000000000000000000000000001
                    // 1101110000000000000000000000000001

                    // reverify the above data, especially the short stuff
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadInt32());
                    asp.Data.Add(br.ReadBit());

                    asp.MarkComplete();
                    break;
                case "Engine.PlayerReplicationInfo:Ping":
                case "TAGame.Vehicle_TA:ReplicatedSteer":
                case "TAGame.Vehicle_TA:ReplicatedThrottle": // 0: full reverse, 128: No throttle.  255 full throttle/boosting
                case "TAGame.PRI_TA:CameraYaw":
                case "TAGame.PRI_TA:CameraPitch":
                case "TAGame.Ball_TA:HitTeamNum":
                case "TAGame.GameEvent_Soccar_TA:ReplicatedScoredOnTeam":
				case "TAGame.CarComponent_Boost_TA:ReplicatedBoostAmount": // Always 255?
                case "TAGame.CameraSettingsActor_TA:CameraPitch":
                case "TAGame.CameraSettingsActor_TA:CameraYaw":
                    asp.Data.Add(br.ReadByte());
                    asp.MarkComplete();
                    break;
                case "Engine.Actor:Location":
                case "TAGame.CarComponent_Dodge_TA:DodgeTorque":
                    asp.Data.Add(Vector3D.Deserialize(br));
                    asp.MarkComplete();
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
                case "TAGame.CarComponent_FlipCar_TA:bFlipRight":
                case "Engine.PlayerReplicationInfo:bBot":
                case "Engine.PlayerReplicationInfo:bWaitingPlayer":
                case "TAGame.RBActor_TA:bReplayActor":
                case "TAGame.PRI_TA:bIsInSplitScreen":
                case "Engine.GameReplicationInfo:bMatchIsOver":
                case "TAGame.CarComponent_Boost_TA:bUnlimitedBoost":
                case "Engine.PlayerReplicationInfo:bIsSpectator":
				case "TAGame.GameEvent_Soccar_TA:bBallHasBeenHit":
                case "TAGame.CameraSettingsActor_TA:bUsingSecondaryCamera":
                case "TAGame.CameraSettingsActor_TA:bUsingBehindView":
                case "TAGame.PRI_TA:bOnlineLoadoutSet":
                    asp.Data.Add(br.ReadBit());
                    asp.MarkComplete();
                    break;
                case "TAGame.CarComponent_TA:ReplicatedActive":
                    // The car component is active if (ReplicatedValue%2)!=0 
                    // For now I am only adding that logic to the JSON serializer
                    asp.Data.Add(br.ReadByte());

                    asp.MarkComplete();
                    break;
                case "Engine.PlayerReplicationInfo:UniqueId":
                case "TAGame.PRI_TA:PartyLeader":
                    asp.Data.Add(UniqueId.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "TAGame.PRI_TA:ClientLoadout":
                    asp.Data.Add(ClientLoadout.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "TAGame.PRI_TA:CameraSettings":
                case "TAGame.CameraSettingsActor_TA:ProfileSettings":
                    asp.Data.Add(CameraSettings.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "TAGame.Car_TA:TeamPaint":
                    asp.Data.Add(TeamPaint.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "ProjectX.GRI_X:GameServerID":
                    asp.Data.Add(br.ReadInt32());
                    asp.Data.Add(br.ReadInt32());
                    asp.MarkComplete();
                    break;
                case "ProjectX.GRI_X:Reservations":
                    asp.Data.Add(Reservation.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "TAGame.Ball_TA:ReplicatedExplosionData":
                    // 0 01010111000000000000000000000000 0011 01010010000001 01111010001011 00000110000001
                    // 0 01110111000000000000000000000000 0011 10011110010001 11111010001011 01010110000001
                    // 0 11010111000000000000000000000000 0011 00001111011110 11000110001011 10111010000001
                    // 0 11010111000000000000000000000000 0011 10011000100001 00100110001011 10111010000001
                    // 0 00110111000000000000000000000000 0011 00000001010001 11000110001011 00100110000001
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadInt32());
                    asp.Data.Add(Vector3D.Deserialize(br)); // Almost definitely position
                    asp.MarkComplete();
                    break;
                case "TAGame.Car_TA:ReplicatedDemolish":
                    asp.Data.Add(ReplicatedDemolish.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "TAGame.GameEvent_Soccar_TA:ReplicatedMusicStinger":
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadByte());
                    asp.Data.Add(br.ReadInt32());
                    asp.MarkComplete();
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
                    asp.MarkComplete();
                    break;
                case "TAGame.GameEvent_SoccarPrivate_TA:MatchSettings":
                    asp.Data.Add(PrivateMatchSettings.Deserialize(br));
                    asp.MarkComplete();
                    break;
				case "TAGame.PRI_TA:ClientLoadoutOnline":
                    var version = br.ReadInt32();
                    asp.Data.Add(version);
					asp.Data.Add(br.ReadInt32());
					asp.Data.Add(br.ReadInt32());
                    if ( version >= 12 )
                    {
                        asp.Data.Add(br.ReadByte());
                    }
                    asp.MarkComplete();
					break;
                case "TAGame.GameEvent_TA:GameMode":
                    asp.Data.Add(br.ReadBitsAsBytes(2)); // Probably is actually a br.ReadInt32Max(3) or something? 
                    asp.MarkComplete();
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unknown property {0}. Next bits in the data are {1}. Figure it out!", asp.PropertyName, br.GetBits(br.Position, Math.Min(4096, br.Length - br.Position)).ToBinaryString()));
            }

#if DEBUG
            asp.KnownDataBits = br.GetBits(startPosition, br.Position - startPosition);
#endif
            return asp;
        }

		private void MarkComplete()
		{
#if DEBUG
			IsComplete = true;
#endif
		}

        public string ToDebugString()
        {
            var s = string.Format("Property: ID {0} Name {1}\r\n", PropertyId, PropertyName);
            s += "    Data: " + string.Join(", ", Data) + "\r\n";
#if DEBUG
			s += "    Max Prop Id: " + MaxPropertyId.ToString() + "\r\n";
            if (KnownDataBits != null && KnownDataBits.Count > 0)
            {
                s += string.Format("    KnownDataBits: {0}\r\n", KnownDataBits.ToBinaryString());
            }
#endif
            return s;

        }
    }
}
