using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class ClassNetCacheProperty
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
    }
}

