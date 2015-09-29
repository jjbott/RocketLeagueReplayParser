using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class TickMark
    {
        public string Text { get; private set; }
        public Int32 Unknown1 { get; private set; } // Frame?

        public static TickMark Deserialize(BinaryReader bs)
        {
            var dbg = new TickMark();
            dbg.Text = bs.ReadAsciiString();
            dbg.Unknown1 = bs.ReadInt32();
            return dbg;
        }
    }
}
