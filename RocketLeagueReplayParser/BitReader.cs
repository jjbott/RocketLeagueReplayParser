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

        public bool ReadBit()
        {
            return _bits[Position++];
        }

        public byte ReadByte()
        {
            var bytes = ReadBitsAsBytes(8);
            return bytes[0];
        }

        public byte ReadFlippedByte()
        {
            var bytes = ReadBitsAsBytes(8, true);
            return bytes[0];
        }

        public Int32 ReadInt32(bool flippedBytes = false)
        {
            return ReadInt32FromBits(32, flippedBytes);
        }

        public byte[] ReadBitsAsBytes(int numBits, bool flipped = false)
        {
            var bytes = new byte[(int)Math.Ceiling((numBits / 8.0))];
            var selectedBits = new bool[numBits];
            for(int i = 0; i < numBits; ++i)
            {
                if (flipped)
                {
                    selectedBits[i] = _bits[Position + i];
                }
                else
                {
                    selectedBits[(numBits - i - 1)] = _bits[Position + i];
                }
            }
            Position += numBits;
            var ba = new BitArray(selectedBits);
            ba.CopyTo(bytes, 0);
            return bytes;
        }

        public int ReadInt32FromBits(int numBits, bool flippedBytes = false)
        {
            if (numBits > 32)
                throw new ArgumentException("Number of bits shall be at most 32 bits");

            var selectedBits = new bool[32];
            for (int i = 0; i < numBits; ++i)
            {
                selectedBits[i] = _bits[Position + i];
            }
            Position += numBits;
            var ba = new BitArray(selectedBits);
            if (flippedBytes)
            {
                var bytes = new byte[4];
                ba.CopyTo(bytes, 0);
                byte b = bytes[0];
                bytes[0] = bytes[3];
                bytes[3] = b;

                b = bytes[1];
                bytes[1] = bytes[2];
                bytes[2] = b;

                return BitConverter.ToInt32(bytes,0);
            }
            else
            {
                var intArray = new int[1];
                ba.CopyTo(intArray, 0);
                return intArray[0];
            }
        }

        public int ReadPackedInt32()
        {/*
            var bits = new bool[32];
            var bitPos = 0;
            do
            {
                for (int i = 0; i < 7; ++i)
                {
                    bits[(6-i) + bitPos] = ReadBit(); // This seems legit scrambled
                }
                bitPos += 7;
            } while (ReadBit());
            var ba = new BitArray(bits);
            var intArray = new int[1];
            ba.CopyTo(intArray, 0);
            return intArray[0];*/
            /*		Value = 0;
		uint8 cnt = 0;
		uint8 more = 1;
		while(more)
		{
			uint8 NextByte;
			Serialize(&NextByte, 1);			// Read next byte
​
			more = NextByte & 1;				// Check 1 bit to see if theres more after this
			NextByte = NextByte >> 1;			// Shift to get actual 7 bit value
			Value += NextByte << (7 * cnt++);	// Add to total value
		}*/
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
    }
}
