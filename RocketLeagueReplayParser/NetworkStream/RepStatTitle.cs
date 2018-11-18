using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class RepStatTitle
    {
        public bool Unknown1 { get; private set; }
        public string Name { get; private set; }
        public ObjectTarget ObjectTarget { get; private set; }
        public UInt32 Value { get; private set; }

        public static RepStatTitle Deserialize(BitReader br)
        {
            var rst = new RepStatTitle();
            rst.Unknown1 = br.ReadBit();
            rst.Name = br.ReadString();
            rst.ObjectTarget = ObjectTarget.Deserialize(br);
            rst.Value = br.ReadUInt32();

            return rst;
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            Name.Serialize(bw);
            ObjectTarget.Serialize(bw);
            bw.Write(Value);
        }
    }
}
