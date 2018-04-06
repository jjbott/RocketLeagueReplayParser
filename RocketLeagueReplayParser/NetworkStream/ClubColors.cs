using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ClubColors
    {
        public bool Unknown1 { get; private set; }
        public byte Unknown2 { get; private set; }
        public bool Unknown3 { get; private set; }
        public byte Unknown4 { get; private set; }
    
        public static ClubColors Deserialize(BitReader br)
        {
            var cc = new ClubColors();
            cc.Unknown1 = br.ReadBit();
            cc.Unknown2 = br.ReadByte();
            cc.Unknown3 = br.ReadBit();
            cc.Unknown4 = br.ReadByte();
            return cc;
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(Unknown3);
            bw.Write(Unknown4);
        }
    }
}
