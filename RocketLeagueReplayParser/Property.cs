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
        public Int32 DataLength { get; private set; }
        public Int32 Unknown2 { get; private set; }
        public Int64? IntValue { get; private set; }
        public string StringValue { get; private set; }
        public string StringValue2 { get; private set; }
        public float FloatValue { get; private set; }
        public List<List<Property>> ArrayValue { get; private set; }

        public string ToDebugString()
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
                foreach (var propList in ArrayValue)
                {
                    foreach (var prop in propList)
                    {
                        r += "\t" + prop.ToDebugString() + "\r\n";
                    }
                }
                return r;
            }
            else if (Type == "ByteProperty")
            {
                return string.Format("{0} {1} {2} {3}", Name, Type, StringValue, StringValue2);
            }
            else
            {
                return string.Format("{0} {1}", Name, Type);
            }
        }

        public static Property Deserialize(BinaryReader bs)
        {
            var p = new Property();
            p.Name = bs.ReadString2();
            if (p.Name != "None")
            {
                p.Type = bs.ReadString2();

                p.DataLength = bs.ReadInt32();
                p.Unknown2 = bs.ReadInt32();

                if (p.Type == "IntProperty")
                {
                    p.IntValue = bs.ReadInt32();
                }
                else if (p.Type == "StrProperty" || p.Type == "NameProperty")
                {
                    p.StringValue = bs.ReadString2();
                }
                else if (p.Type == "FloatProperty")
                {
                    p.FloatValue = bs.ReadSingle();
                }
                else if (p.Type == "ByteProperty")
                {
                    // how is this a byte property?
                    p.StringValue = bs.ReadString2();
                    p.StringValue2 = bs.ReadString2();
                }
                else if (p.Type == "BoolProperty")
                {
                    p.IntValue = bs.ReadByte();
                }
                else if (p.Type == "QWordProperty")
                {
                    p.IntValue = bs.ReadInt64();
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
                else
                {
                    throw new InvalidDataException("Unknown property: " + p.Type);
                }

            }

            return p;
        }

        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();

            result.AddRange(Name.Serialize());

            if (Name != "None")
            {
                result.AddRange(Type.Serialize());
                result.AddRange(BitConverter.GetBytes(DataLength));
                result.AddRange(BitConverter.GetBytes(Unknown2));

                if (Type == "IntProperty")
                {
                    result.AddRange(BitConverter.GetBytes((Int32)IntValue.Value));
                }
                else if (Type == "StrProperty" || Type == "NameProperty")
                {
                    result.AddRange(StringValue.Serialize());
                }
                else if (Type == "FloatProperty")
                {
                    result.AddRange(BitConverter.GetBytes(FloatValue));
                }
                else if (Type == "ByteProperty")
                {
                    result.AddRange(StringValue.Serialize());
                    result.AddRange(StringValue2.Serialize());
                }
                else if (Type == "BoolProperty")
                {
                    result.Add((byte)IntValue.Value);
                }
                else if (Type == "QWordProperty")
                {
                    result.AddRange(BitConverter.GetBytes(IntValue.Value));
                }
                else if (Type == "ArrayProperty")
                {
                    result.AddRange(BitConverter.GetBytes(ArrayValue.Count));

                    foreach (var property in ArrayValue.SelectMany(p => p))
                    {
                        result.AddRange(property.Serialize());
                    }
                }
                else
                {
                    throw new InvalidDataException("Unknown property type: " + Type);
                }

            }

            return result;
        }


    }
}
