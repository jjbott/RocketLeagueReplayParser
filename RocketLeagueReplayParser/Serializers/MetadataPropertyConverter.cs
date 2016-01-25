using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RocketLeagueReplayParser.Serializers
{
    public class MetadataPropertyConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new[] { typeof(List<Property>), typeof(Property[]), typeof(IEnumerable<Property>) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            IEnumerable<Property> properties = (IEnumerable<Property>)obj;

            return properties.Where(p => p.Name != "None").ToDictionary(p => p.Name, p => (object)SerializeValue(p));
        }

        private object SerializeValue(Property prop)
        {
            switch(prop.Type)
            {
                case "IntProperty":
                case "QWordProperty":
                    return prop.IntValue;
                case "FloatProperty":
                    return prop.FloatValue;
                case "StrProperty":
                case "NameProperty":
                case "ByteProperty":
                    return prop.StringValue;
                case "BoolProperty":
                    return (prop.IntValue == 1);
                case "ArrayProperty":
                    var arrayPropDict = new Dictionary<string, object>();

                    IEnumerable<IEnumerable<KeyValuePair<string, object> > > serializedArray = prop.ArrayValue
                        .Select(l => l.Where(p => p.Name != "None").Select(p => new KeyValuePair<string, object>(p.Name, SerializeValue(p))));

                    // Combine each IEnumerable<KeyValuePair<string, object> > into a single IDictionary<string, object>
                    // Each KeyValuePair will be representing 1 property. We can combine them so we're left with a list of properties.

                    return serializedArray.Select(l => l.ToDictionary(kv => kv.Key, kv => kv.Value));
            }

            return null;
        }
   
    }
}
