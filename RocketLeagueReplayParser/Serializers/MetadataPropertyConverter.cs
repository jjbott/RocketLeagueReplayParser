using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Collections;

namespace RocketLeagueReplayParser.Serializers
{
    public class MetadataPropertyConverter : JavaScriptConverter
    {
        bool _raw;
        Func<long, double> _frameToTimeCallback;

        public MetadataPropertyConverter(bool raw, Func<long, double> frameToTimeCallback)
        {
            _raw = raw;
            _frameToTimeCallback = frameToTimeCallback;
        }

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

            var serialized = properties.Where(p => p.Name != "None").ToDictionary(p => p.Name, p => (object)SerializeValue(p));
            if ( !_raw && serialized.ContainsKey("Goals"))
            {
                var newGoals = new List<Dictionary<string, object>>();
                foreach (object o in (IEnumerable<object>)(serialized["Goals"]))
                {
                    // Convert goal frame numbers to times, since when we're not in raw mode the frame numbers wont line up correctly
                    var goal = (Dictionary<string, object>)o;
                    goal["Time"] = _frameToTimeCallback((long)goal["frame"]);
                    goal.Remove("frame");
                    newGoals.Add(goal);
                }
                serialized["Goals"] = newGoals;
            }
            return serialized;
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
