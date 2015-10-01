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
        public string Type { get; private set; }
        public Int32 Frame { get; private set; } // Frame?

        public static TickMark Deserialize(BinaryReader bs)
        {
            var tm = new TickMark();
            tm.Type = bs.ReadAsciiString();
            tm.Frame = bs.ReadInt32();
            return tm;
        }

        public string ToDebugString()
        {
            return string.Format("TickMark: Type {0} Frame {1}", Type, Frame);
        }
    }
}
