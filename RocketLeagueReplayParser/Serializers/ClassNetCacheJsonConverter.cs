using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RocketLeagueReplayParser.Serializers
{
    public class ClassNetCacheJsonConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new[] { typeof(ClassNetCache) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            ClassNetCache classNetCache = (ClassNetCache)obj;
            var serialized = new Dictionary<string, object>();

            serialized["ObjectIndex"] = classNetCache.ObjectIndex;
            serialized["ParentId"] = classNetCache.ParentId;
            serialized["Id"] = classNetCache.Id;
            // Purposely leaving out PropertiesLength
            serialized["Properties"] = classNetCache.Properties.Values;
            serialized["Children"] = classNetCache.Children;

            return serialized;
        }
    }
}
