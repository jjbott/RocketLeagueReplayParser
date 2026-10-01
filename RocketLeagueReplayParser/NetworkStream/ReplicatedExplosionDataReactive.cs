using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    /// <summary>
    /// Goal explosion data introduced in 868.34 (TAGame.Ball_TA:ReplicatedExplosionDataReactive).
    /// Layout is the "Extended" explosion data preceded by 16 unknown bits.
    /// In the base data, ActorId holds the object index of the goal (e.g. "...GoalVolume_TA_0.Goal_TA_0").
    /// </summary>
    public class ReplicatedExplosionDataReactive : ReplicatedExplosionDataExtended
    {
        public UInt32 Unknown0 { get; private set; }

        public new static ReplicatedExplosionDataReactive Deserialize(BitReader br, UInt32 netVersion)
        {
            var redr = new ReplicatedExplosionDataReactive();

            redr.DeserializeImpl(br, netVersion);

            return redr;
        }

        protected override void DeserializeImpl(BitReader br, UInt32 netVersion)
        {
            Unknown0 = br.ReadUInt32FromBits(16);
            base.DeserializeImpl(br, netVersion);
        }

        public override void Serialize(BitWriter bw, UInt32 netVersion)
        {
            bw.WriteFixedBitCount(Unknown0, 16);
            base.Serialize(bw, netVersion);
        }
    }
}