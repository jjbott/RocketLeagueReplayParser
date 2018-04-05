using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{

    public class LogoData
    {
        public bool Unknown1 { get; private set; } // Betting this indicates team 0/1 or orange/blue palette? 
        public UInt32 LogoId { get; private set; } // Bit of a guess, havent gone fishing in the UPKs for matches

        public static LogoData Deserialize(BitReader br)
        {
            var aa = new LogoData();
            aa.Unknown1 = br.ReadBit();
            aa.LogoId = br.ReadUInt32();
            return aa;
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(LogoId);
        }

        public override string ToString()
        {
            return string.Format("Unknown1: {0}, LogoId: {1}", Unknown1, LogoId);
        }
    }
}
