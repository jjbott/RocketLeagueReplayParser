using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class AppliedDamage
    {
        public byte Unknown1 { get; private set; }
        public Vector3D Position { get; private set; }
        public Int32 Unknown3 { get; private set; }
        public Int32 Unknown4 { get; private set; } 

        public static AppliedDamage Deserialize(BitReader br, UInt32 netVersion)
        {
            var ad = new AppliedDamage();
            ad.Unknown1 = br.ReadByte();
            ad.Position = Vector3D.Deserialize(br, netVersion);
            ad.Unknown3 = br.ReadInt32();
            ad.Unknown4 = br.ReadInt32();
            return ad;
        }

        public void Serialize(BitWriter bw, UInt32 netVersion)
        {
            bw.Write(Unknown1);
            Position.Serialize(bw, netVersion);
            bw.Write(Unknown3);
            bw.Write(Unknown4);
        }
    }
}
