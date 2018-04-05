using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ObjectTarget // I can't think of a better name for whatever this is...
    {
        public bool Unknown1 { get; private set; }
        public Int32 ObjectIndex { get; private set; }

        public static ObjectTarget Deserialize(BitReader br)
        {
            var aa = new ObjectTarget();
            aa.Unknown1 = br.ReadBit();
            aa.ObjectIndex = br.ReadInt32();
            return aa;
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(ObjectIndex);
        }

        public override string ToString()
        {
            return string.Format("Unknown1: {0}, ObjectId: {1}", Unknown1, ObjectIndex);
        }
    }
}
