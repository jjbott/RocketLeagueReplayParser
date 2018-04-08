using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ActorStateProperty = RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateProperty;
using ActorStateJson = RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson;

namespace RocketLeagueReplayParser.Serializers
{
    public class ActorStateJsonConverter : JavaScriptConverter
    {
        private readonly bool _raw;
        private readonly bool _indexPropertiesByName;

        public ActorStateJsonConverter(bool raw, bool indexPropertiesByName)
        {
            _raw = raw;
            _indexPropertiesByName = indexPropertiesByName;
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new[] { typeof(ActorStateJson) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            ActorStateJson actorState = (ActorStateJson)obj;
            IDictionary<string, object> serialized = new Dictionary<string, object>();

            serialized["Id"] = actorState.Id;
            
            if (_raw && actorState.UnknownBit != null)
            {
                serialized["UnknownBit"] = actorState.UnknownBit;
            }

            if (actorState.TypeName != null)
            {
                serialized["TypeName"] = actorState.TypeName;
            }

            if (actorState.ClassName != null)
            {
                serialized["ClassName"] = actorState.ClassName;
            }

            if ( actorState.InitialPosition != null )
            {
                serialized["InitialPosition"] = actorState.InitialPosition;
            }

            foreach (var p in actorState.Properties.Values)
            {
                object data = p.Data;

                if ( !_raw && p.Name == "TAGame.CarComponent_TA:ReplicatedActive")
                {
                    // I used to inject this as a fake "TAGame.CarComponent_TA:Active" property.
                    // I changed to overwrite ReplicatedActive because otherwise I'd have to
                    // inject a new property into the object list when in "index by id" mode.
                    data = (Convert.ToInt32(p.Data) % 2) != 0;
                }

                if (_indexPropertiesByName)
                {
                    serialized[p.Name] = data;
                }
                else
                {
                    serialized[p.Id.ToString()] = data;
                }
            }

            return serialized;
        }
    }
}
