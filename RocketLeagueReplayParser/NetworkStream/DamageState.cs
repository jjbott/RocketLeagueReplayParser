using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class DamageState
    {
        public byte Unknown1 { get; private set; }
        public bool Unknown2 { get; private set; }
        public Int32 Unknown3 { get; private set; }
        public Vector3D Unknown4 { get; private set; } // Position or Force maybe?
        public bool Unknown5 { get; private set; }
        public bool Unknown6 { get; private set; }

        public static DamageState Deserialize(BitReader br, UInt32 netVersion)
        {
            var ds = new DamageState();
            ds.Unknown1 = br.ReadByte();
            ds.Unknown2 = br.ReadBit();
            ds.Unknown3 = br.ReadInt32();
            ds.Unknown4 = Vector3D.Deserialize(br, netVersion);
            ds.Unknown5 = br.ReadBit();
            ds.Unknown6 = br.ReadBit();
            return ds;
        }

        public void Serialize(BitWriter bw, UInt32 netVersion)
        {
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(Unknown3);
            Unknown4.Serialize(bw, netVersion);
            bw.Write(Unknown5);
            bw.Write(Unknown6);
        }
    }
}
