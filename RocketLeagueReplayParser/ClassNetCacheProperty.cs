using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public interface IClassNetCacheProperty
    {
        Int32 Index { get; }
        Int32 Id { get; }

        string ToDebugString(string[] objects);
    }

    public class ClassNetCacheProperty : IClassNetCacheProperty
    {
        public Int32 Index { get; private set; }
        public Int32 Id { get; private set; }
        public static ClassNetCacheProperty Deserialize(BinaryReader br)
        {
            var prop = new ClassNetCacheProperty();
            prop.Index = br.ReadInt32();
            prop.Id = br.ReadInt32();
            return prop;
        }

        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();

            result.AddRange(BitConverter.GetBytes(Index));
            result.AddRange(BitConverter.GetBytes(Id));

            return result;
        }

        public string ToDebugString(string[] objects)
        {
            if (objects == null)
            {
                return string.Format("ClassNetCacheProperty: Index {0} Id {1}", Index, Id);
            }
            else
            {
                return string.Format("ClassNetCacheProperty: Index {0} ({2}) Id {1}", Index, Id, objects[Index]);
            }
        }
    }
}

