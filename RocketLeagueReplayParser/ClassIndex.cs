using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class ClassIndex
    {
        public string Class { get; private set; }
        public Int32 Index { get; private set; }

        public static ClassIndex Deserialize(BinaryReader bs)
        {
            var classIndex = new ClassIndex();
            classIndex.Class = bs.ReadString2();
            classIndex.Index = bs.ReadInt32();
            return classIndex;
        }

        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();

            result.AddRange(Class.Serialize());
            result.AddRange(BitConverter.GetBytes(Index));

            return result;
        }

        public string ToDebugString()
        {
            return string.Format("ClassIndex: Class {0} Index {1}", Class, Index);
        }
    }
}
