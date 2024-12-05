using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedDemolishExtended : ReplicatedDemolish
    {
        public ObjectTarget AttackerPRI { get; private set;} 
        public ObjectTarget SelfDemoFX { get; private set;}
        public bool bSelfDemolish { get; set; }
        public ObjectTarget GoalExplosionOwner { get; set; }

        public new static ReplicatedDemolishExtended Deserialize(BitReader br, UInt32 netVersion)
        {
            var rde = new ReplicatedDemolishExtended();

            rde.DeserializeImpl(br, netVersion);

            return rde;
        }

        protected override void DeserializeImpl(BitReader br, UInt32 netVersion)
        {
            AttackerPRI = ObjectTarget.Deserialize(br);
            SelfDemoFX = ObjectTarget.Deserialize(br);
            bSelfDemolish = br.ReadBit();
            GoalExplosionOwner = ObjectTarget.Deserialize(br);

            base.DeserializeImpl(br, netVersion);
        }

        public override void Serialize(BitWriter bw, UInt32 netVersion)
        {
            AttackerPRI.Serialize(bw);
            SelfDemoFX.Serialize(bw);
            bw.Write(bSelfDemolish);
            GoalExplosionOwner.Serialize(bw);
            base.Serialize(bw, netVersion);
        }
    }
}
