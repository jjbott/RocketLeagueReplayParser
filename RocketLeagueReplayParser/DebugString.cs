using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class DebugString
    {
        public Int32 FrameNumber { get; private set; }
        public string Username { get; private set; }
        public string Text { get; private set; }

        public static DebugString Deserialize(BinaryReader br)
        {
            var ds = new DebugString();
            ds.FrameNumber = br.ReadInt32();
            ds.Username = br.ReadAsciiString();
            ds.Text = br.ReadAsciiString();
            return ds;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", FrameNumber, Username, Text);
        }
    }
}
