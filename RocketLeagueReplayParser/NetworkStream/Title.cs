using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class Title
    {
        public bool Unknown1 { get; private set; }
        public bool Unknown2 { get; private set; }
        public UInt32 Unknown3 { get; private set; }
        public UInt32 Unknown4 { get; private set; }
        public UInt32 Unknown5 { get; private set; }
        public UInt32 Unknown6 { get; private set; }
        public UInt32 Unknown7 { get; private set; }
        public bool Unknown8 { get; private set; }

        public static Title Deserialize(BitReader br)
        {
            // Bit alignment is best guess based on a single example, so could be off.
            var t = new Title();
            t.Unknown1 = br.ReadBit();
            t.Unknown2 = br.ReadBit();
            t.Unknown3 = br.ReadUInt32();
            t.Unknown4 = br.ReadUInt32();
            t.Unknown5 = br.ReadUInt32();
            t.Unknown6 = br.ReadUInt32();
            t.Unknown7 = br.ReadUInt32();
            t.Unknown8 = br.ReadBit();

            return t;
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(Unknown3);
            bw.Write(Unknown4);
            bw.Write(Unknown5);
            bw.Write(Unknown6);
            bw.Write(Unknown7);
            bw.Write(Unknown8);
        }
    }
}
