using Newtonsoft.Json;
using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.Serializers
{
    public class ActorStatePropertyJsonConverter : JsonConverter
    {
        private readonly bool _raw;
        private readonly bool _indexByPropertyName;
        private readonly string[] _objectNames;

        public ActorStatePropertyJsonConverter(bool raw, bool indexByPropertyName, string[] objectNames)
        {
            _raw = raw;
            _indexByPropertyName = indexByPropertyName;
            _objectNames = objectNames;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ActorStateProperty).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var property = (ActorStateProperty)value;

            object data = property.Data;

            if (!_raw && property.PropertyName == "TAGame.CarComponent_TA:ReplicatedActive")
            {
                // I used to inject this as a fake "TAGame.CarComponent_TA:Active" property.
                // I changed to overwrite ReplicatedActive because otherwise I'd have to
                // inject a new property into the object list when in "index by id" mode.
                data = (Convert.ToInt32(data) % 2) != 0;
            }

            if (_indexByPropertyName)
            {
                writer.WriteKeyValue(property.PropertyId.ToString(), data, serializer);
            }
            else
            {
                writer.WriteKeyValue(property.PropertyName, data, serializer);
            }
        }
    }
}
