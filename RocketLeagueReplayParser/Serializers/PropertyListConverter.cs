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
    public class PropertyListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<Property>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var props = value as List<Property>;

            writer.WriteStartArray();
            foreach (var prop in props)
            {
                if (prop is StructProperty)
                {
                    serializer.Serialize(writer, prop);
                }
                else
                {
                    writer.WriteStartObject();
                    serializer.Serialize(writer, prop);
                    writer.WriteEndObject();
                }
            }
            writer.WriteEndArray();
        }
    }
}
