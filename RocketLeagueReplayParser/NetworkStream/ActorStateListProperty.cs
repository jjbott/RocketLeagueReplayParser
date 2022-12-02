using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ActorStateListProperty : ActorStateProperty
    {
        public ActorStateListProperty(ActorStateProperty convertFromProperty) : base(convertFromProperty)
        {
            Data = new List<object> { convertFromProperty.Data };
        }

        public void Add(ActorStateProperty property)
        {
            if ( PropertyId != property.PropertyId)
            {
                throw new ArgumentException("Property id mismatch, can not add to list");
            }
 
            ((List<object>)Data).Add(property.Data);
        }

        public override void Serialize(UInt32 engineVersion, UInt32 licenseeVersion, UInt32 netVersion, UInt32 changelist, BitWriter bw)
        {
            var list = (List<object>)Data;

            for (int i = 0; i < list.Count; ++i)
            {
                if ( i != 0 )
                {
                    bw.Write(true);
                }

                bw.Write(PropertyId, (UInt32)_classNetCache.MaxPropertyId + 1);
                SerializeData(engineVersion, licenseeVersion, netVersion, changelist, bw, PropertyName, list[i]);
            }
        }
    }
}
