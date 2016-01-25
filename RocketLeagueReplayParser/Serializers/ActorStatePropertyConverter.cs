using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ActorStateProperty = RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateProperty;

namespace RocketLeagueReplayParser.Serializers
{
    public class ActorStatePropertyConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new[] { typeof(IEnumerable<ActorStateProperty>), typeof(List<ActorStateProperty>), typeof(ActorStateProperty[]) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            IEnumerable<ActorStateProperty> properties = (IEnumerable<ActorStateProperty>)obj;
            IDictionary<string, object> serialized = new Dictionary<string, object>();

            foreach (var p in properties)
            {
                if (p.Data.Count == 1)
                {
                    serialized[p.Name] = p.Data[0];
                }
                else
                {
                    serialized[p.Name] = p.Data;
                }

                // Adding extra info in this case to convert to a flag. Is that weird? I dunno...
                // Conflicted between creating raw JSON, and creating "nice" JSON
                if (p.Name == "TAGame.CarComponent_TA:ReplicatedActive")
                {
                    serialized["TAGame.CarComponent_TA:Active"] = (Convert.ToInt32(p.Data[0]) % 2) != 0;
                }
            }
            return serialized;
        }
    }
}
