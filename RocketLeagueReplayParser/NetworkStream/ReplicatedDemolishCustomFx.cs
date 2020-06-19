using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedDemolishCustomFx
    {
        public bool Unknown1 { get; private set; }
        public Int32 Unknown2 { get; private set; }
        public ReplicatedDemolish ReplicatedDemolish { get; private set; }

        public static ReplicatedDemolishCustomFx Deserialize(BitReader br, UInt32 netVersion)
        {
            var rd = new ReplicatedDemolishCustomFx();
            
            rd.Unknown1 = br.ReadBit();
            rd.Unknown2 = br.ReadInt32();
            rd.ReplicatedDemolish = ReplicatedDemolish.Deserialize(br, netVersion);

            return rd;
        }

        public void Serialize(BitWriter bw, UInt32 netVersion)
        {
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            ReplicatedDemolish.Serialize(bw, netVersion);
        }
    }
}
