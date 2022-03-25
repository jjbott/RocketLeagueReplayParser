using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class BitReader
    {
        BitArray _bits;
        public int Position { get; private set; }

        public BitReader(byte[] bytes)
        {
            _bits = new BitArray(bytes);
        }

        public BitReader(bool[] bits)
        {
            _bits = new BitArray(bits);
        }

        public Int32 Length
        {
            get
            {
                return _bits.Length;
            }
        }

        public BitReader(string bitString)
            : this(BitsFromString(bitString))
        {
        }

        public void Seek(int position)
        {
            if ( position < 0 || position >= _bits.Length)
            {
                throw new ArgumentOutOfRangeException("Position out of range");
            }

            Position = position;
        }

        private static bool[] BitsFromString(string bitString) // Should be a string like "10010010101"
        {
            var bits = new bool[bitString.Length];
            for (int i= 0; i < bitString.Length; ++i)
            {
                if (bitString[i] == '0')
                {
                    bits[i] = false;
                }
                else if (bitString[i] == '1')
                {
                    bits[i] = true;
                }
                else
                {
                    throw new ArgumentException("Bit string contains characters besides 0 and 1");
                }
            }
            return bits;
        }

        public bool PeekBit()
        {
            return _bits[Position];
        }

        public bool ReadBit()
        {
            return _bits[Position++];
        }

        public byte ReadByte()
        {
            var bytes = ReadBitsAsBytes(8);
            return bytes[0];
        }

        public UInt32 ReadUInt32Max(Int32 maxValue)
        {
            var maxBits = Math.Floor(Math.Log10(maxValue) / Math.Log10(2)) + 1;

            UInt32 value = 0;
            for(int i = 0; i < maxBits && (value + (1 << i)) < maxValue; ++i)
            {
                value += (ReadBit() ? 1U: 0U) << i;
            }

            if ( value > maxValue)
            {
                throw new Exception("ReadUInt32Max overflowed!");
            }
            
            return value;

        }

        public UInt32 ReadUInt32()
        {
            var value = ReadUInt32FromBits(32);
#if DEBUG
            if ( value > Int32.MaxValue )
            {
                // This is almost definitely supposed to be read as a signed int
                throw new Exception($"Read value {value} as a UInt32, would be {(int)value} if read as Int32");
            }
#endif
            return value;
        }

        public Int32 ReadInt32()
        {
            return ReadInt32FromBits(32);
        }

        public UInt64 ReadUInt64()
        {
            return ReadUInt32() + ((UInt64)ReadUInt32() << 32);
        }

        public byte[] ReadBitsAsBytes(int numBits)
        {
            if  ( numBits <= 0 || numBits > 64 )
            {
                throw new InvalidOperationException(string.Format("Invalid number of bits to read {0}", numBits));
            }

            var bytes = new byte[(int)Math.Ceiling((numBits / 8.0))];
            var byteIndex = 0;
            var bitIndex = 0;
            for (int i = 0; i < numBits; ++i)
            {
                if (_bits[Position + i])
                {
                    bytes[byteIndex] |= (byte)(1 << bitIndex);
                }
                ++bitIndex;
                if (bitIndex >= 8)
                {
                    ++byteIndex;
                    bitIndex = 0;
                }
            }
            Position += numBits;
            return bytes;
        }

        public UInt32 ReadUInt32FromBits(int numBits)
        {
            if (numBits <= 0 || numBits > 32)
                throw new ArgumentException("Number of bits shall be at most 32 bits");
            UInt32 result = 0;
            for(int i = 0; i < numBits; ++i)
            {
                result += (ReadBit() ? 1U : 0U) << i;
            }
            return result;
        }

        public Int32 ReadInt32FromBits(int numBits)
        {
            return (Int32)ReadUInt32FromBits(numBits);
        }

        public float ReadFloat()
        {
            var bytes = ReadBitsAsBytes(32);
            return BitConverter.ToSingle(bytes, 0);
        }

        public bool EndOfStream
        {
            get
            {
                return Position >= _bits.Length;
            }
        }

        public List<bool> GetBits(int startPosition, int count)
        {
            var r = new List<bool>();
            for(int i = 0; i < count; ++i)
            {
                r.Add(_bits[startPosition + i]);
            }

            return r;
        }

        public byte[] ReadBytes(Int32 count)
        {
            var bytes = new byte[count];
            for(int i = 0; i < count; ++i)
            {
                bytes[i] = ReadByte();
            }
            return bytes;
        }

        public string ReadString(int? fixedLength = null)
        {
            int length = fixedLength ?? ReadInt32();
            if (length > 0)
            {
                var bytes = ReadBytes(length);
#if !NET462
                return CodePagesEncodingProvider.Instance.GetEncoding(1252).GetString(bytes, 0, length - 1);
#else
                return Encoding.GetEncoding(1252).GetString(bytes, 0, length - 1);
#endif
            }
            else if (length < 0)
            {
                var bytes = ReadBytes(length * -2);
                return Encoding.Unicode.GetString(bytes, 0, (length * -2) - 2);
            }

            return "";
        }

        public float ReadFixedCompressedFloat(Int32 maxValue, Int32 numBits)
        {
            float value = 0;
            // NumBits = 8:
            var maxBitValue = (1 << (numBits - 1)) - 1; //   0111 1111 - Max abs value we will serialize
            var bias = (1 << (numBits - 1));    //   1000 0000 - Bias to pivot around (in order to support signed values)
            var serIntMax = (1 << (numBits - 0));   // 1 0000 0000 - What we pass into SerializeInt
            var maxDelta = (1 << (numBits - 0)) - 1;	//   1111 1111 - Max delta is

            Int32 delta = (Int32)ReadUInt32Max(serIntMax); 
            float unscaledValue = delta - bias;

            if (maxValue > maxBitValue)
            {
                // We have to scale down, scale needs to be a float:
                float invScale = maxValue / (float)maxBitValue;
                value = unscaledValue * invScale;
            }
            else
            {
                var scale = maxBitValue / maxValue;
                float invScale = 1.0f / (float)scale;

                value = unscaledValue * invScale;
            }

            return value;
        }

    }
}
