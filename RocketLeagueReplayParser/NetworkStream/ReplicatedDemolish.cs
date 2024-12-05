using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedDemolish
    {
        public ObjectTarget Attacker { get; set; }
        public ObjectTarget Victim { get; set; }
        public Vector3D AttackerVelocity { get; private set; }
        public Vector3D VictimVelocity { get; private set; }

        public static ReplicatedDemolish Deserialize(BitReader br, UInt32 netVersion)
        {
            var rd = new ReplicatedDemolish();

            rd.DeserializeImpl(br, netVersion);

            return rd;
        }

        protected virtual void DeserializeImpl(BitReader br, uint netVersion)
        {
            Attacker = ObjectTarget.Deserialize(br);
            Victim = ObjectTarget.Deserialize(br);
            AttackerVelocity = Vector3D.Deserialize(br, netVersion);
            VictimVelocity = Vector3D.Deserialize(br, netVersion);
        }

        public virtual void Serialize(BitWriter bw, UInt32 netVersion)
        {
            Attacker.Serialize(bw);
            Victim.Serialize(bw);
            AttackerVelocity.Serialize(bw, netVersion);
            VictimVelocity.Serialize(bw, netVersion);
        }
    }
}
