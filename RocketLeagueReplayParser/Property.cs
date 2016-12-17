using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace RocketLeagueReplayParser
{
    

    public class Property : IEnumerable<PropertyDictionary>
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public Int32 DataLength { get; private set; }
        public Int32 Unknown { get; private set; }

        // Decided against giving value a stronger type for now. I'm not sure callers will care yet
        public object Value { get; private set;
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        #region Hacks to make array properties easier to use
        private List<PropertyDictionary> PropertyLists
        {
            get
            {
                if (Type != "ArrayProperty")
                {
                    throw new InvalidOperationException("Can not use indexer on Property of type " + Type);
                }
                return (List<PropertyDictionary>)Value;

            }
        }
        public PropertyDictionary this[int i]
        {
            get
            {
                return PropertyLists[i];

            }
        }

        public IEnumerator<PropertyDictionary> GetEnumerator()
        {
            return PropertyLists.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return PropertyLists.GetEnumerator();
        }
        #endregion

        public string ToDebugString()
        {
            switch (Type)
            {
                case "IntProperty":
                case "FloatProperty":
                case "StrProperty":
                case "NameProperty":
                    return string.Format("{0} {1} {2}", Name, Type, Value);
                    break;
                case "ArrayProperty":
                    var r = string.Format("{0} {1}\r\n", Name, Type);
                    foreach (var properties in PropertyLists)
                    {
                        foreach (var prop in properties.Values)
                        {
                            r += "\t" + prop.ToDebugString() + "\r\n";
                        }
                    }
                    return r;
                case "ByteProperty":
                    var v = (EnumPropertyValue)Value;
                    return string.Format("{0} {1} {2} {3}", Name, Type, v.Type, v.Value);
                default:
                    return string.Format("{0} {1}", Name, Type);
            }
        }

        public static Property Deserialize(BinaryReader br)
        {
            var p = new Property();
            p.Name = br.ReadString2();
            if (p.Name != "None")
            {
                p.Type = br.ReadString2();
                p.DataLength = br.ReadInt32();
                p.Unknown = br.ReadInt32();

                if (p.Type == "IntProperty")
                {
                    p.Value = br.ReadInt32();
                }
                else if (p.Type == "StrProperty" || p.Type == "NameProperty")
                {
                    p.Value = br.ReadString2();
                }
                else if (p.Type == "FloatProperty")
                {
                    p.Value = br.ReadSingle();
                }
                else if (p.Type == "ByteProperty")
                {
                    // how is this a byte property?
                    p.Value = EnumPropertyValue.Deserialize(br);
                }
                else if (p.Type == "BoolProperty")
                {
                    p.Value = br.ReadByte();
                }
                else if (p.Type == "QWordProperty")
                {
                    p.Value = br.ReadInt64();
                }
                else if (p.Type == "ArrayProperty")
                {
                    var propertyLists = new List<PropertyDictionary>();
                    var len = br.ReadInt32();
                    for (int i = 0; i < len; ++i)
                    {
                        var properties = PropertyDictionary.Deserialize(br);
                        propertyLists.Add(properties);
                    }
                    p.Value = propertyLists;
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
                result.AddRange(BitConverter.GetBytes(Unknown));

                if (Type == "IntProperty")
                {
                    result.AddRange(BitConverter.GetBytes((Int32)Value));
                }
                else if (Type == "StrProperty" || Type == "NameProperty")
                {
                    result.AddRange(((string)Value).Serialize());
                }
                else if (Type == "FloatProperty")
                {
                    result.AddRange(BitConverter.GetBytes((float)Value));
                }
                else if (Type == "ByteProperty")
                {
                    var epv = (EnumPropertyValue)Value;
                    result.AddRange(epv.Type.Serialize());
                    result.AddRange(epv.Value.Serialize());
                }
                else if (Type == "BoolProperty")
                {
                    result.Add((byte)Value);
                }
                else if (Type == "QWordProperty")
                {
                    result.AddRange(BitConverter.GetBytes((long)Value));
                }
                else if (Type == "ArrayProperty")
                {
                    var propertiesList = (List<PropertyDictionary>)Value;
                    result.AddRange(BitConverter.GetBytes(propertiesList.Count));

                    foreach (var properties in propertiesList)
                    {
                        result.AddRange(properties.Serialize());
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
