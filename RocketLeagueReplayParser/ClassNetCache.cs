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
        public Int32 ParentId { get; private set;}
        public Int32 Id { get; private set;}
        public Int32 PropertiesLength { get; private set;}
        public ClassNetCacheProperty[] Properties { get; private set; }
        public List<ClassNetCache> Children { get; private set; }
        public bool Root;
        public static ClassNetCache Deserialize(BinaryReader br)
        {
            var classNetCache = new ClassNetCache();
            classNetCache.ObjectIndex = br.ReadInt32();
            classNetCache.ParentId = br.ReadInt32();
            classNetCache.Id = br.ReadInt32();

            classNetCache.Children = new List<ClassNetCache>();

            classNetCache.PropertiesLength = br.ReadInt32();

            classNetCache.Properties = new ClassNetCacheProperty[classNetCache.PropertiesLength];
            for (int i = 0; i < classNetCache.PropertiesLength; ++i)
            {
                classNetCache.Properties[i] = ClassNetCacheProperty.Deserialize(br);
            }

            return classNetCache;
        }

        public string ToDebugString(string[] objects, int depth = 0)
        {
            string debugString = "";
            string indent = "";
            indent = indent.PadRight(depth * 4);

            if ( objects == null)
            {
                debugString = indent + string.Format("ClassNetCache: ObjectIndex {0} ParentId {1} Id {2}\r\n", ObjectIndex, ParentId, Id);
            }
            else
            {
                debugString = indent + string.Format("ClassNetCache: ObjectIndex {0} ({3} ParentId {1} Id {2}\r\n", ObjectIndex, ParentId, Id, objects[ObjectIndex]);
            }

            foreach(var prop in Properties)
            {
                debugString += indent + "    " + prop.ToDebugString(objects) + "\r\n";
            }

            foreach (var c in Children)
            {
                debugString += c.ToDebugString(objects, depth + 1);
            }

            return debugString;
        }
    }
}
