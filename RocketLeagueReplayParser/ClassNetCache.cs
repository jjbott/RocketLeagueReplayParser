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
        public Int32 ObjectIndex { get; private set;}
        public Int32 StartId { get; private set;}
        public Int32 EndId { get; private set;}
        public Int32 PropertiesLength { get; private set;}
        public ClassNetCacheProperty[] Properties { get; private set; }
        public static ClassNetCache Deserialize(BinaryReader br)
        {
            var classNetCache = new ClassNetCache();
            classNetCache.ObjectIndex = br.ReadInt32();
            classNetCache.StartId = br.ReadInt32();
            classNetCache.EndId = br.ReadInt32();
            classNetCache.PropertiesLength = br.ReadInt32();

            classNetCache.Properties = new ClassNetCacheProperty[classNetCache.PropertiesLength];
            for (int i = 0; i < classNetCache.PropertiesLength; ++i)
            {
                classNetCache.Properties[i] = ClassNetCacheProperty.Deserialize(br);
            }

            return classNetCache;
        }

        public string ToDebugString(Object[] objects)
        {
            string debugString = "";

            if ( objects == null)
            {
                debugString = string.Format("ClassNetCache: ObjectIndex {0} StartId {1} EndId {2}\r\n", ObjectIndex, StartId, EndId);
            }
            else
            {
                debugString = string.Format("ClassNetCache: ObjectIndex {0} ({3} StartId {1} EndId {2}\r\n", ObjectIndex, StartId, EndId, objects[ObjectIndex]);
            }

            foreach(var prop in Properties)
            {
                debugString += "    " + prop.ToDebugString() + "\r\n";
            }

            return debugString;
        }
    }
}
