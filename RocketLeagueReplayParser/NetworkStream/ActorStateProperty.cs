using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ActorStateProperty
    {
        public UInt32 PropertyId { get; private set; }
        public string PropertyName { get; private set; }
        public List<object> Data { get; private set; }

#if DEBUG
		private UInt32 MaxPropertyId { get; set; }
        private List<bool> KnownDataBits { get;  set; }
        public bool IsComplete { get; private set; }
#endif

        public static ActorStateProperty Deserialize(IClassNetCache classMap, string typeName, string[] objectIndexToName, UInt32 versionMajor, UInt32 versionMinor, BitReader br)
        {
            var asp = new ActorStateProperty();

            var maxPropId = classMap.MaxPropertyId;

            var className = objectIndexToName[classMap.ObjectIndex];
            asp.PropertyId = br.ReadUInt32Max(maxPropId + 1);
#if DEBUG
            asp.MaxPropertyId = (UInt32)maxPropId;
#endif
            asp.PropertyName = objectIndexToName[classMap.GetProperty((int)asp.PropertyId).Index];

            var startPosition = br.Position;

            asp.Data = new List<object>();

            switch (asp.PropertyName)
            {
                case "TAGame.GameEvent_TA:ReplicatedStateIndex":
                    asp.Data.Add(br.ReadUInt32Max(140)); // number is made up, I dont know the max yet // TODO: Revisit this. It might work well enough, but looks fishy
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
                case "TAGame.GameEvent_TA:MatchTypeClass":
                case "TAGame.GameEvent_Soccar_TA:SubRulesArchetype":
                    // Theres a good chance that most of these can be moved to the next section
                    asp.Data.Add(br.ReadBit()); 
                    asp.Data.Add(br.ReadUInt32());
                    asp.MarkComplete();
                    break;
                case "TAGame.Team_TA:GameEvent":
                case "TAGame.Ball_TA:GameEvent":
                case "Engine.PlayerReplicationInfo:Team":
                case "Engine.Pawn:PlayerReplicationInfo":
                case "TAGame.PRI_TA:ReplicatedGameEvent":
                case "TAGame.CarComponent_TA:Vehicle":
                case "TAGame.Car_TA:AttachedPickup":
                case "TAGame.SpecialPickup_Targeted_TA:Targeted":
                    // TODO: Use a real class so it can be accessed normally.
                    // If Active == false, ActorId will be -1
                    asp.Data.Add(ActiveActor.Deserialize(br));
                    asp.MarkComplete();
                    break;                   
                case "Engine.GameReplicationInfo:ServerName":
                case "Engine.PlayerReplicationInfo:PlayerName":
                case "TAGame.Team_TA:CustomTeamName":
                case "Engine.PlayerReplicationInfo:RemoteUserData":
                case "TAGame.GRI_TA:NewDedicatedServerIP":
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
                case "TAGame.Team_Soccar_TA:GameScore":
                case "TAGame.GameEvent_Soccar_TA:GameTime":
                case "TAGame.CarComponent_Boost_TA:UnlimitedBoostRefCount":
                case "TAGame.CrowdActor_TA:ReplicatedRoundCountDownNumber":
                    asp.Data.Add(br.ReadUInt32());
                    asp.MarkComplete();
                    break;
                case "TAGame.VehiclePickup_TA:ReplicatedPickupData":
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadUInt32());
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
                case "TAGame.PRI_TA:PawnType":
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
                case "TAGame.PRI_TA:bMatchMVP":
                case "TAGame.PRI_TA:bOnlineLoadoutsSet":
                case "TAGame.RBActor_TA:bIgnoreSyncing":
                case "TAGame.SpecialPickup_BallVelcro_TA:bHit":
                case "TAGame.GameEvent_TA:bCanVoteToForfeit":
                case "TAGame.SpecialPickup_BallVelcro_TA:bBroken":
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
                    asp.Data.Add(UniqueId.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "TAGame.PRI_TA:PartyLeader":
                    asp.Data.Add(PartyLeader.Deserialize(br));
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
                    asp.Data.Add(br.ReadUInt32());
                    asp.Data.Add(br.ReadUInt32());
                    asp.MarkComplete();
                    break;
                case "ProjectX.GRI_X:Reservations":
                    asp.Data.Add(Reservation.Deserialize(versionMajor, versionMinor, br));
                    asp.MarkComplete();
                    break;
                case "TAGame.Ball_TA:ReplicatedExplosionData":
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadUInt32());
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
                    asp.Data.Add(br.ReadUInt32());
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
                case "TAGame.CarComponent_TA:ReplicatedActivityTime":
                case "TAGame.SpecialPickup_BallFreeze_TA:RepOrigSpeed":
                case "TAGame.SpecialPickup_BallVelcro_TA:AttachTime":
                case "TAGame.SpecialPickup_BallVelcro_TA:BreakTime":
                case "TAGame.Car_TA:AddedCarForceMultiplier":
                case "TAGame.Car_TA:AddedBallForceMultiplier":
                    asp.Data.Add(br.ReadFloat());
                    asp.MarkComplete();
                    break;
                case "TAGame.GameEvent_SoccarPrivate_TA:MatchSettings":
                    asp.Data.Add(PrivateMatchSettings.Deserialize(br));
                    asp.MarkComplete();
                    break;
				case "TAGame.PRI_TA:ClientLoadoutOnline":
                    asp.Data.Add(ClientLoadoutOnline.Deserialize(br));
                    asp.MarkComplete();
					break;
                case "TAGame.GameEvent_TA:GameMode":
                    if (versionMajor >= 868 && versionMinor >= 12 && 
                        (typeName.Contains("Basketball") || typeName.Contains("Hockey") || typeName.Contains("Items")) ) // Might be unnecessary. The property seems to only show up on games that aren't standard soccar, which is all this check is for
                    {
                        asp.Data.Add(br.ReadByte());
                    }
                    else
                    {
                        asp.Data.Add(br.ReadUInt32Max(4));
                    }
                    asp.MarkComplete();
                    break;
                case "TAGame.PRI_TA:ClientLoadoutsOnline":
                    asp.Data.Add(ClientLoadoutsOnline.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "TAGame.PRI_TA:ClientLoadouts":
                    asp.Data.Add(ClientLoadouts.Deserialize(br));
                    asp.MarkComplete();
                    break;
                case "TAGame.Team_TA:ClubColors":
                case "TAGame.Car_TA:ClubColors":
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadByte());
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(br.ReadByte());
                    asp.MarkComplete();
                    break;
                case "TAGame.RBActor_TA:WeldedInfo":
                    asp.Data.Add(WeldedInfo.Deserialize(br));
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

        public void Serialize(int maxPropId, UInt32 versionMajor, UInt32 versionMinor, BitWriter bw)
        {
            bw.Write(PropertyId, (UInt32)maxPropId + 1);

            // TODO: Make it so each property is typed better, so I serialize/deserialize types 
            // instead of having separate serialize/deserialize logic for each property.
            // Will also make it do I dont have to do so much casting from object
            switch (PropertyName)
            {
                case "TAGame.GameEvent_TA:ReplicatedStateIndex":
                    bw.Write((UInt32)Data[0], 140); // number is made up, I dont know the max yet // TODO: Revisit this. It might work well enough, but looks fishy
                    break;
                case "TAGame.RBActor_TA:ReplicatedRBState":
                    ((RigidBodyState)Data[0]).Serialize(bw);
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
                    bw.Write((bool)Data[0]);
                    bw.Write((UInt32)Data[1]);
                    break;
                case "TAGame.Team_TA:GameEvent":
                case "TAGame.Ball_TA:GameEvent":
                case "Engine.PlayerReplicationInfo:Team":
                case "Engine.Pawn:PlayerReplicationInfo":
                case "TAGame.PRI_TA:ReplicatedGameEvent":
                case "TAGame.CarComponent_TA:Vehicle":
                    ((ActiveActor)Data[0]).Serialize(bw);
                    break;
                case "Engine.GameReplicationInfo:ServerName":
                case "Engine.PlayerReplicationInfo:PlayerName":
                case "TAGame.Team_TA:CustomTeamName":
                case "Engine.PlayerReplicationInfo:RemoteUserData":
                case "TAGame.GRI_TA:NewDedicatedServerIP":
                    ((string)Data[0]).Serialize(bw);
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
                case "TAGame.Team_Soccar_TA:GameScore":
                    bw.Write((UInt32)Data[0]);
                    break;
                case "TAGame.VehiclePickup_TA:ReplicatedPickupData":
                    bw.Write((bool)Data[0]);
                    bw.Write((UInt32)Data[1]);
                    bw.Write((bool)Data[2]);
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
                    bw.Write((byte)Data[0]);
                    break;
                case "Engine.Actor:Location":
                case "TAGame.CarComponent_Dodge_TA:DodgeTorque":
                    ((Vector3D)Data[0]).Serialize(bw);
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
                case "TAGame.PRI_TA:bMatchMVP":
                    bw.Write((bool)Data[0]);
                    break;
                case "TAGame.CarComponent_TA:ReplicatedActive":
                    bw.Write((byte)Data[0]);
                    break;
                case "Engine.PlayerReplicationInfo:UniqueId":
                    ((UniqueId)Data[0]).Serialize(bw);
                    break;
                case "TAGame.PRI_TA:PartyLeader":
                    ((PartyLeader)Data[0]).Serialize(bw);
                    break;
                case "TAGame.PRI_TA:ClientLoadout":
                    ((ClientLoadout)Data[0]).Serialize(bw);
                    break;
                case "TAGame.PRI_TA:CameraSettings":
                case "TAGame.CameraSettingsActor_TA:ProfileSettings":
                    ((CameraSettings)Data[0]).Serialize(bw);
                    break;
                case "TAGame.Car_TA:TeamPaint":
                    ((TeamPaint)Data[0]).Serialize(bw);
                    break;
                case "ProjectX.GRI_X:GameServerID":
                    bw.Write((UInt32)Data[0]);
                    bw.Write((UInt32)Data[1]);
                    break;
                case "ProjectX.GRI_X:Reservations":
                    ((Reservation)Data[0]).Serialize(versionMajor, versionMinor, bw);
                    break;
                case "TAGame.Ball_TA:ReplicatedExplosionData":
                    bw.Write((bool)Data[0]);
                    bw.Write((UInt32)Data[1]);
                    ((Vector3D)Data[2]).Serialize(bw);
                    break;
                case "TAGame.Car_TA:ReplicatedDemolish":
                    ((ReplicatedDemolish)Data[0]).Serialize(bw);
                    break;
                case "TAGame.GameEvent_Soccar_TA:ReplicatedMusicStinger":
                    bw.Write((bool)Data[0]);
                    bw.Write((byte)Data[1]);
                    bw.Write((UInt32)Data[2]);
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
                    bw.Write((float)Data[0]);
                    break;
                case "TAGame.GameEvent_SoccarPrivate_TA:MatchSettings":
                    ((PrivateMatchSettings)Data[0]).Serialize(bw);
                    break;
                case "TAGame.PRI_TA:ClientLoadoutOnline":
                    bw.Write((UInt32)Data[0]);
                    bw.Write((UInt32)Data[1]);
                    bw.Write((UInt32)Data[2]);
                    if ((UInt32)Data[0] >= 12)
                    {
                        bw.Write((byte)Data[3]);
                    }
                    break;
                case "TAGame.GameEvent_TA:GameMode":
                    bw.Write((UInt32)Data[0], 4);
                    break;
                default:
                    throw new NotSupportedException("Unknown property {0} foudn in serializer: " + PropertyName);
            }
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
