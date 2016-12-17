using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class PropertyDictionary : Dictionary<string, Property>
    {
        public static PropertyDictionary Deserialize(BinaryReader br)
        {
            PropertyDictionary pd = new PropertyDictionary();

            Property prop;
            do
            {
                prop = Property.Deserialize(br);
                pd[prop.Name] = prop;
            }
            while (prop.Name != "None");

            return pd;
        }

        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();

            foreach (var property in Values)
            {
                result.AddRange(property.Serialize());
            }

            return result;
        }
    }
}
