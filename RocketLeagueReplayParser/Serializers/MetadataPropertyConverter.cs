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
                return new[] { typeof(Property) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            var p = obj as Property;

            object value = p.Value;
            if ( p.Name == "frame" )
            {
                value = _frameToTimeCallback((int)value);
            }
            return new Dictionary<string, object> { { p.Name, value } };
        }
    }

    public class MetadataPropertyDictionaryConverter : JavaScriptConverter
    {
        bool _raw;
        Func<long, double> _frameToTimeCallback;
        MetadataPropertyConverter propertyConverter;

        public MetadataPropertyDictionaryConverter(bool raw, Func<long, double> frameToTimeCallback)
        {
            _raw = raw;
            _frameToTimeCallback = frameToTimeCallback;
            propertyConverter = new MetadataPropertyConverter(_raw, _frameToTimeCallback);
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new[] { typeof(PropertyDictionary) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            var pd = obj as PropertyDictionary;
            var result = new Dictionary<string, object>();
            foreach(var p in pd.Values.Where(v => v.Name != "None"))
            {
                var serialized = propertyConverter.Serialize(p, serializer);
                if (serialized.Any())
                {
                    result[serialized.First().Key] = serialized.First().Value;
                }
            }
            return result;
        }
    }
}
