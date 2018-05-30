using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedExplosionDataExtended : ReplicatedExplosionData
    {
        public bool Unknown3 { get; private set; }
        public UInt32 Unknown4 { get; private set; }

        public new static ReplicatedExplosionDataExtended Deserialize(BitReader br, UInt32 netVersion)
        {
            var rede = new ReplicatedExplosionDataExtended();

            rede.DeserializeImpl(br, netVersion);

            return rede;
        }

        protected override void DeserializeImpl(BitReader br, UInt32 netVersion)
        {
            base.DeserializeImpl(br, netVersion);
            Unknown3 = br.ReadBit();
            Unknown4 = br.ReadUInt32();
        }

        public override void Serialize(BitWriter bw, UInt32 netVersion)
        {
            base.Serialize(bw, netVersion);
            bw.Write(Unknown3);
            bw.Write(Unknown4);
        }
    }
}
