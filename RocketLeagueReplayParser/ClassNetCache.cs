using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public interface IClassNetCache
    {
        Int32 ObjectIndex { get; }
        Int32 ParentId { get; }
        Int32 Id { get; }

        IClassNetCacheProperty GetProperty(int id);

        int MaxPropertyId { get; }
    }

    public class ClassNetCache : IClassNetCache
    {
        public Int32 ObjectIndex { get; private set;}
        public Int32 ParentId { get; private set;}
        public Int32 Id { get; private set;}
        public Int32 PropertiesLength { get; private set;}
        public IDictionary<int, IClassNetCacheProperty> Properties { get; private set; }
        public List<ClassNetCache> Children { get; private set; }

        public ClassNetCache Parent { get; set; }
        public bool Root;

        public static ClassNetCache Deserialize(BinaryReader br)
        {
            var classNetCache = new ClassNetCache();
            classNetCache.ObjectIndex = br.ReadInt32();
            classNetCache.ParentId = br.ReadInt32();
            classNetCache.Id = br.ReadInt32();

            classNetCache.Children = new List<ClassNetCache>();

            classNetCache.PropertiesLength = br.ReadInt32();

            classNetCache.Properties = new Dictionary<int, IClassNetCacheProperty>();
            for (int i = 0; i < classNetCache.PropertiesLength; ++i)
            {
                var prop = ClassNetCacheProperty.Deserialize(br);
                classNetCache.Properties[prop.Id] = prop;
            }

            return classNetCache;
        }

        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();

            result.AddRange(BitConverter.GetBytes(ObjectIndex));
            result.AddRange(BitConverter.GetBytes(ParentId));
            result.AddRange(BitConverter.GetBytes(Id));

            result.AddRange(BitConverter.GetBytes(Properties.Count));
            foreach(var propertyId in Properties.Keys.OrderBy(k => k))
            {
                result.AddRange(((ClassNetCacheProperty)(Properties[propertyId])).Serialize());
            }

            return result;
        }

        public IEnumerable<IClassNetCacheProperty> AllProperties
        {
            get
            {
                foreach(var prop in Properties.Values)
                {
                    yield return prop;
                }

                if ( Parent != null )
                {
                    foreach (var prop in Parent.AllProperties)
                    {
                        yield return prop;
                    }
                }
            }
        }

        private int? _maxPropertyId;
        public int MaxPropertyId
        {
            get
            {
                if ( _maxPropertyId == null)
                {
                    _maxPropertyId = AllProperties.Max(x => x.Id);
                }
                return _maxPropertyId.Value;
            }
        }

        public IClassNetCacheProperty GetProperty(int id)
        {
            IClassNetCacheProperty property;
            if (Properties.TryGetValue(id, out property))
            {
                return property;
            }
            else if (Parent != null)
            {
                return Parent.GetProperty(id);
            }
            else
            {
                return null;
            }
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

            foreach(var prop in Properties.Values.OrderBy(p => p.Id))
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
