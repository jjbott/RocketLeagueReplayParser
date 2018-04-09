using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActorStateJson = RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson;
using Newtonsoft.Json;

namespace RocketLeagueReplayParser.Serializers
{
    public class ActorStateJsonConverter : JsonConverter
    {
        private readonly bool _raw;
        private readonly bool _minimal;
        private readonly string[] _objectNames;

        public ActorStateJsonConverter(bool raw, bool minimal, string[] objectNames)
        {
            _raw = raw;
            _minimal = minimal;
            _objectNames = objectNames;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ActorStateJson);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            ActorStateJson actorState = (ActorStateJson)value;

            writer.WriteStartObject();

            writer.WriteKeyValue(nameof(actorState.Id), actorState.Id, serializer);

            if (actorState.NameId != null)
            {
                writer.WriteKeyValue(nameof(actorState.NameId), actorState.NameId, serializer);
            }

            if (_raw && actorState.UnknownBit != null)
            {
                writer.WriteKeyValue(nameof(actorState.UnknownBit), actorState.UnknownBit, serializer);
            }

            if (actorState.TypeId != null)
            {
                if (_raw || _minimal)
                {
                    writer.WriteKeyValue(nameof(actorState.TypeId), actorState.TypeId, serializer);
                }
                else
                {
                    writer.WriteKeyValue("TypeName", _objectNames[actorState.TypeId.Value], serializer);
                }
            }

            if (actorState.ClassId != null)
            {
                if (_raw || _minimal)
                {
                    writer.WriteKeyValue(nameof(actorState.ClassId), actorState.ClassId, serializer);
                }
                else
                {
                    writer.WriteKeyValue("ClassName", _objectNames[actorState.ClassId.Value], serializer);
                }
            }

            if (actorState.InitialPosition != null)
            {
                writer.WriteKeyValue(nameof(actorState.InitialPosition), actorState.InitialPosition, serializer);
            }

            foreach (var p in actorState.Properties.Values)
            {
                serializer.Serialize(writer, p);
            }

            writer.WriteEndObject();
        }
    }
}
