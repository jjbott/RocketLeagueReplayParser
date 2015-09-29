using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class Property
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public Int32 Unknown1 { get; private set; }
        public Int32 Unknown2 { get; private set; }
        public Int32? IntValue { get; private set; }
        public string StringValue { get; private set; }
        public float FloatValue { get; private set; }
        public List<List<Property>> ArrayValue { get; private set; }

        public string ToString()
        {
            if (Type == "IntProperty")
            {
                return string.Format("{0} {1} {2}", Name, Type, IntValue);
            }
            else if (Type == "FloatProperty")
            {
                return string.Format("{0} {1} {2}", Name, Type, FloatValue);
            }
            else if (Type == "StrProperty" || Type == "NameProperty")
            {
                return string.Format("{0} {1} {2}", Name, Type, StringValue);
            }
            else if (Type == "ArrayProperty")
            {
                var r = string.Format("{0} {1}\r\n", Name, Type);
                foreach(var propList in ArrayValue)
                {
                    foreach(var prop in propList)
                    {
                        r += "\t" + prop.ToString() + "\r\n";
                    }
                }
                return r;
            }
            else
            {
                return string.Format("{0} {1}", Name, Type);
            }
        }

        public static Property Deserialize(BinaryReader bs)
        {
            var p = new Property();
            p.Name = bs.ReadAsciiString();
            if (p.Name != "None")
            {
                p.Type = bs.ReadAsciiString();
                p.Unknown1 = bs.ReadInt32();
                p.Unknown2 = bs.ReadInt32();

                if (p.Type == "IntProperty")
                {
                    p.IntValue = bs.ReadInt32();
                }
                else if (p.Type == "StrProperty" || p.Type == "NameProperty")
                {
                    p.StringValue = bs.ReadAsciiString();
                }
                else if (p.Type == "FloatProperty")
                {
                    p.FloatValue = bs.ReadSingle();
                }
                else if (p.Type == "ArrayProperty")
                {
                    p.ArrayValue = new List<List<Property>>();
                    var len = bs.ReadInt32();
                    for (int i = 0; i < len; ++i)
                    {
                        var properties = new List<Property>();
                        Property prop;
                        do
                        {
                            prop = Property.Deserialize(bs);
                            properties.Add(prop);
                        }
                        while (prop.Name != "None");
                        p.ArrayValue.Add(properties);

                    }
                }

            }

            return p;
        }

    }
}
