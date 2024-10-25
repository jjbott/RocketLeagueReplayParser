using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RocketLeagueReplayParser
{
    

    public class Property
    {
        public string Name { get; protected set; }
        public string Type { get; protected set; }
        public Int32 DataLength { get; protected set; }
        public Int32 ArrayIndex { get; protected set; }

        // Decided against giving value a stronger type for now. I'm not sure callers will care yet
        public object Value { get; protected set;
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        public virtual string ToDebugString()
        {
            switch (Type)
            {
                case "IntProperty":
                case "FloatProperty":
                case "StrProperty":
                case "NameProperty":
                    return string.Format("{0} {1} {2}", Name, Type, Value);
                case "ByteProperty":
                    var v = (EnumPropertyValue)Value;
                    return string.Format("{0} {1} {2} {3}", Name, Type, v.Type, v.Value);
                default:
                    return string.Format("{0} {1}", Name, Type);
            }
        }

        public static Property Deserialize(BinaryReader br)
        {
            var name = br.ReadString2();

            if (name == "None")
            {
                return new Property { Name = name };
            }
            
            var type = br.ReadString2();

            Property p;
            if ( type == "ArrayProperty" )
            {
                p = new ArrayProperty { Name = name, Type = type };
            } 
            else if (type == "StructProperty")
            {
                p = new StructProperty { Name = name, Type = type };
            }
            else
            {
                p = new Property { Name = name, Type = type };
            }

            p.DataLength = br.ReadInt32();
            p.ArrayIndex = br.ReadInt32();

            p.DeserializeValue(br);

            return p;
        }

        protected virtual void DeserializeValue(BinaryReader br)
        {
            if (Type == "IntProperty")
            {
                Value = br.ReadInt32();
            }
            else if (Type == "StrProperty" || Type == "NameProperty")
            {
                Value = br.ReadString2();
            }
            else if (Type == "FloatProperty")
            {
                Value = br.ReadSingle();
            }
            else if (Type == "ByteProperty")
            {
                // how is this a byte property?
                Value = EnumPropertyValue.Deserialize(br);
            }
            else if (Type == "BoolProperty")
            {
                Value = br.ReadByte();
            }
            else if (Type == "QWordProperty")
            {
                Value = br.ReadInt64();
            }
            else
            {
                throw new InvalidDataException("Unknown property: " + Type);
            }
        }

        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();

            result.AddRange(Name.Serialize());

            if (Name != "None")
            {
                result.AddRange(Type.Serialize());
                result.AddRange(BitConverter.GetBytes(DataLength));
                result.AddRange(BitConverter.GetBytes(ArrayIndex));

                result.AddRange(SerializeValue());
            }

            return result;
        }

        protected virtual IEnumerable<byte> SerializeValue()
        {
            var result = new List<byte>();

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
                result.AddRange(epv.Serialize());
            }
            else if (Type == "BoolProperty")
            {
                result.Add((byte)Value);
            }
            else if (Type == "QWordProperty")
            {
                result.AddRange(BitConverter.GetBytes((long)Value));
            }
            else
            {
                throw new InvalidDataException("Unknown property type: " + Type);
            }

            return result;
        }
    }

    public class ArrayProperty : Property, IEnumerable<IEnumerable<Property>>
    {
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

        public IEnumerator<IEnumerable<Property>> GetEnumerator()
        {
            return PropertyLists.Select(pd => pd.Values.Select(v => v)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return PropertyLists.Select(pd => pd.Values.Select(v => v)).GetEnumerator();
        }
        #endregion


        protected override void DeserializeValue(BinaryReader br)
        {
            if (Type == "ArrayProperty")
            {
                var propertyLists = new List<PropertyDictionary>();
                var len = br.ReadInt32();
                for (int i = 0; i < len; ++i)
                {
                    var properties = PropertyDictionary.Deserialize(br);
                    propertyLists.Add(properties);
                }
                Value = propertyLists;
            }
            else
            {
                throw new InvalidDataException("Unknown array property: " + Type);
            }
        }

        protected override IEnumerable<byte> SerializeValue()
        {
            var result = new List<byte>();
            
            var propertiesList = (List<PropertyDictionary>)Value;
            result.AddRange(BitConverter.GetBytes(propertiesList.Count));

            foreach (var properties in propertiesList)
            {
                result.AddRange(properties.Serialize());
            }

            return result;
        }

        public override string ToDebugString()
        {
            var r = string.Format("{0} {1}\r\n", Name, Type);
            foreach (var properties in PropertyLists)
            {
                foreach (var prop in properties.Values)
                {
                    r += "\t" + prop.ToDebugString() + "\r\n";
                }
            }
            return r;
        }
    }

    public class StructProperty : Property
    {
        public string Unknown2 { get; private set; }

        protected override void DeserializeValue(BinaryReader br)
        {
            if (Type == "StructProperty")
            {
                Unknown2 = br.ReadString2();

                var properties = new List<Property>();
                Property p;
                do
                {
                    p = Property.Deserialize(br);
                    properties.Add(p);
                } while (p.Name != "None");

                // A PropertyDictionry would be nicer, but there are duplicate names.
                // Looks like those are actually meant to be properties with an array of values.
                // We could convert them to nicer looking properties, 
                // but I dont think it'd be worth it.                

                Value = properties;
            }
            else
            {
                throw new InvalidDataException("Unknown struct property: " + Type);
            }
        }

        protected override IEnumerable<byte> SerializeValue()
        {
            var result = new List<byte>();

            result.AddRange(((string)Unknown2).Serialize());

            var propertiesList = (List<Property>)Value;

            foreach (var properties in propertiesList)
            {
                result.AddRange(properties.Serialize());
            }

            return result;
        }
    }
}
