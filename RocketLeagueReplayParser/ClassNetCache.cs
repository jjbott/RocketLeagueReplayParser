using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class ClassNetCache
    {
        public Int32 ClassId { get; private set;}
        public Int32 StartIndex { get; private set;}
        public Int32 EndIndex { get; private set;}
        public Int32 PropertiesLength { get; private set;}
        public ClassNetCacheProperty[] Properties { get; private set; }
        public static ClassNetCache Deserialize(BinaryReader br)
        {
            var classNetCache = new ClassNetCache();
            classNetCache.ClassId = br.ReadInt32();
            classNetCache.StartIndex = br.ReadInt32();
            classNetCache.EndIndex = br.ReadInt32();
            classNetCache.PropertiesLength = br.ReadInt32();

            classNetCache.Properties = new ClassNetCacheProperty[classNetCache.PropertiesLength];
            for (int i = 0; i < classNetCache.PropertiesLength; ++i)
            {
                classNetCache.Properties[i] = ClassNetCacheProperty.Deserialize(br);
            }

            return classNetCache;
        }
    }
}
