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
            ds.Username = br.ReadString2();
            ds.Text = br.ReadString2();
            return ds;
        }

        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();

            result.AddRange(BitConverter.GetBytes(FrameNumber));
            result.AddRange(Username.Serialize());
            result.AddRange(Text.Serialize());

            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", FrameNumber, Username, Text);
        }
    }
}
