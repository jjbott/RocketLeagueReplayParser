using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json;

namespace RocketLeagueReplayParser.Serializers
{
    public class MetadataPropertyConverter : JsonConverter
    {
        bool _raw;
        Func<long, float> _frameToTimeCallback;

        public MetadataPropertyConverter(bool raw, Func<long, float> frameToTimeCallback)
        {
            _raw = raw;
            _frameToTimeCallback = frameToTimeCallback;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Property);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var p = value as Property;

            object propertyValue = p.Value;
            var name = p.Name;

            if (!_raw && name == "frame")
            {
                // If we're not in raw mode, frame numbers wont make sense since we may be removing some. Convert to times.
                name = "Time";
                propertyValue = _frameToTimeCallback((int)propertyValue); 
            }
            
            writer.WriteKeyValue(name, propertyValue, serializer);
        }
    }

    public class MetadataPropertyDictionaryConverter : JsonConverter
    {
        bool _raw;

        public MetadataPropertyDictionaryConverter(bool raw)
        {
            _raw = raw;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PropertyDictionary);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var pd = value as PropertyDictionary;
            
            var values = _raw ? pd.Values : pd.Values.Where(v => v.Name != "None");

            writer.WriteStartObject();
            foreach (var p in values)
            {
                if ( p is IEnumerable )
                {
                    writer.WriteKeyValue(p.Name, p.Value, serializer);
                }
                else
                {
                    serializer.Serialize(writer, p);
                }                
            }
            writer.WriteEndObject();
        }
    }
}
