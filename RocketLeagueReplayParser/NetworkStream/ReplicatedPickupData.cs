using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedPickupData
    {
        public bool Unknown1 { get; private set; }
        public Int32 ActorId { get; private set; }
        public bool Unknown2 { get; private set; }

        public static ReplicatedPickupData Deserialize(BitReader br)
        {
            var rpd = new ReplicatedPickupData();

            rpd.Unknown1 = br.ReadBit();
            rpd.ActorId = br.ReadInt32();
            rpd.Unknown2 = br.ReadBit();

            return rpd;
        }

        public virtual void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(ActorId);
            bw.Write(Unknown2);
        }
    }
}
