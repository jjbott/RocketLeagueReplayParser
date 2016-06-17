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
            tm.Type = bs.ReadString2();
            tm.Frame = bs.ReadInt32();
            return tm;
        }

        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();

            result.AddRange(Type.Serialize());
            result.AddRange(BitConverter.GetBytes(Frame));

            return result;
        }

        public string ToDebugString()
        {
            return string.Format("TickMark: Type {0} Frame {1}", Type, Frame);
        }
    }
}
