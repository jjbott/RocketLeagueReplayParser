using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ActorStateListProperty : ActorStateProperty
    {
        public ActorStateListProperty(ActorStateProperty convertFromProperty) : base()
        {
            PropertyId = convertFromProperty.PropertyId;
            PropertyName = convertFromProperty.PropertyName;
            Data = new List<object> { convertFromProperty.Data };
        }

        public void Add(ActorStateProperty property)
        {
            if ( PropertyId != property.PropertyId)
            {
                throw new ArgumentException("Property id mismatch, can not add to list");
            }

            // Could check name too, but I'll be getting rid of it at some point. 

            ((List<object>)Data).Add(property.Data);
        }

        public override void Serialize(int maxPropId, UInt32 engineVersion, UInt32 licenseeVersion, BitWriter bw)
        {
            var list = (List<object>)Data;

            for (int i = 0; i < list.Count; ++i)
            {
                if ( i != 0 )
                {
                    bw.Write(true);
                }

                bw.Write(PropertyId, (UInt32)maxPropId + 1);
                SerializeData(engineVersion, licenseeVersion, bw, PropertyName, list[i]);
            }
        }
    }
}
