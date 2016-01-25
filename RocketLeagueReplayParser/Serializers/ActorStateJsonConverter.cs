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

            // Can probably leave this out entirely. Always false I believe
            /*
            if (actorState.UnknownBit != null)
            {
                serialized["UnknownBit"] = actorState.UnknownBit;
            }
             * */

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

            foreach(var p in actorState.Properties)
            {
                if (p.Data.Count == 1)
                {
                    serialized[p.Name] = p.Data[0];
                }
                else
                {
                    serialized[p.Name] = p.Data;
                }

                // Adding extra info in this case to convert to a flag.
                // Maybe leave this out if I add a "raw" mode
                if (p.Name == "TAGame.CarComponent_TA:ReplicatedActive")
                {
                    serialized["TAGame.CarComponent_TA:Active"] = (Convert.ToInt32(p.Data[0]) % 2) != 0;
                }
            }

            return serialized;
        }
    }
}
