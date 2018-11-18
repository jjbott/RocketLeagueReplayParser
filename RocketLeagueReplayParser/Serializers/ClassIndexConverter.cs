using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RocketLeagueReplayParser.Serializers
{
    public class ClassIndexJsonConverter : JsonConverter
    {
        private readonly bool _raw;

        public ClassIndexJsonConverter(bool raw)
        {
            _raw = raw;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnumerable<ClassIndex>).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            IEnumerable<ClassIndex> classIndexes = (IEnumerable<ClassIndex>)value;

            writer.WriteStartObject();

            foreach(var ci in classIndexes)
            {
                writer.WriteKeyValue(ci.Class, ci.Index, serializer);
            }

            writer.WriteEndObject();
        }
    }
}
