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

        public Int32 ReadInt32Max(Int32 maxValue)
        {
            var maxBits = Math.Floor(Math.Log10(maxValue) / Math.Log10(2)) + 1;

            Int32 value = 0;
            for(int i = 0; i < maxBits && (value + (1<< i)) < maxValue; ++i)
            {
                value += (ReadBit() ? 1: 0) << i;
            }

            if ( value > maxValue)
            {
                throw new Exception("ReadInt32Max overflowed!");
            }
            
            return value;

        }

        public Int32 ReadInt32()
        {
            return ReadUInt32FromBits(32);
        }

        public byte[] ReadBitsAsBytes(int numBits)
        {
            if  ( numBits <= 0 || numBits > 64 )
            {
                throw new InvalidOperationException(string.Format("Invalid number of bits to read {0}", numBits));
            }

            var bytes = new byte[(int)Math.Ceiling((numBits / 8.0))];
            var selectedBits = new bool[numBits];
            for(int i = 0; i < numBits; ++i)
            { 
                selectedBits[i] = _bits[Position + i];
            }
            Position += numBits;
            var ba = new BitArray(selectedBits);
            ba.CopyTo(bytes, 0);
            return bytes;
        }

        public int ReadUInt32FromBits(int numBits)
        {
            if (numBits <= 0 || numBits > 32)
                throw new ArgumentException("Number of bits shall be at most 32 bits");
            int result = 0;
            for(int i = 0; i < numBits; ++i)
            {
                result += (ReadBit() ? 1 : 0) << i;
            }
            return result;
        }

        public int ReadPackedInt32()
        {
            Int32 val = 0;
            byte cnt = 0;
            byte more = 1;
            while (more == 1)
            {
                byte NextByte = ReadByte();
                more = (byte)(NextByte & 1);
                NextByte = (byte)(NextByte >> 1);
                val += NextByte << (7 * cnt++);

            }

            return val;

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
                return Position >= _bits.Count;
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

        public byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            for(int i = 0; i < count; ++i)
            {
                bytes[i] = ReadByte();
            }
            return bytes;
        }

        public string ReadString()
        {
            var length = ReadInt32();
            if ( length > 0 )
            {
                var bytes = ReadBytes(length);
                return Encoding.GetEncoding(1252).GetString(bytes, 0, length-1);
            }
            else if (length < 0)
            {
                var bytes = ReadBytes(length * -2);
                return Encoding.Unicode.GetString(bytes, 0, (length * -2) -2);
            }

            return "";
        }
    }
}
