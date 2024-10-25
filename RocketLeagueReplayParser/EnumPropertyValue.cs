using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    // Value of "ByteProperty" properties
    // I decided they're Enum values, despite the type name. Shrug.
    public class EnumPropertyValue
    {
        public string Type { get; private set; }
        public string Value { get; private set; }

        public static EnumPropertyValue Deserialize(BinaryReader br)
        {
            var epv = new EnumPropertyValue()
            {
                Type = br.ReadString2()
            };

            if ( epv.Type == "None" )
            {
                br.ReadByte();
            }
            else
            {
                epv.Value = br.ReadString2();
            }
            return epv;
        }
        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();

            result.AddRange(Type.Serialize());
            if ( Type != "None")
            {
                result.AddRange(Value.Serialize());
            }
            else
            {
                result.Add(0);
            }

            return result;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
