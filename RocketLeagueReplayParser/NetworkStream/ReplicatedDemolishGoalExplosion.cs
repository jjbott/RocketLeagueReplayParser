using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedDemolishGoalExplosion
    {
        public bool GoalExplosionOwnerFlag { get; private set; }
        public Int32 GoalExplosionOwner { get; private set; }
        public bool AttackerFlag { get; private set; }
        public Int32 AttackerActorId { get; private set; }
        public bool VictimFlag { get; private set; }
        public UInt32 VictimActorId { get; private set; }
        public Vector3D AttackerVelocity { get; private set; }
        public Vector3D VictimVelocity { get; private set; }

        public static ReplicatedDemolishGoalExplosion Deserialize(BitReader br, UInt32 netVersion)
        {
            var rd = new ReplicatedDemolishGoalExplosion();

            rd.GoalExplosionOwnerFlag = br.ReadBit();
            rd.GoalExplosionOwner = br.ReadInt32();
            rd.AttackerFlag = br.ReadBit();
            rd.AttackerActorId = br.ReadInt32();
            rd.VictimFlag = br.ReadBit();
            rd.VictimActorId = br.ReadUInt32();
            rd.AttackerVelocity = Vector3D.Deserialize(br, netVersion);
            rd.VictimVelocity = Vector3D.Deserialize(br, netVersion);

            return rd;
        }

        public void Serialize(BitWriter bw, UInt32 netVersion)
        {
            bw.Write(GoalExplosionOwnerFlag);
            bw.Write(GoalExplosionOwner);
            bw.Write(AttackerFlag);
            bw.Write(AttackerActorId);
            bw.Write(VictimFlag);
            bw.Write(VictimActorId);
            AttackerVelocity.Serialize(bw, netVersion);
            VictimVelocity.Serialize(bw, netVersion);
        }
    }
}
