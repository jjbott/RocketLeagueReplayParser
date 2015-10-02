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
        int _position = 0;

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
            return _bits[_position++];
        }

        public byte[] ReadBitsAsBytes(int numBits)
        {
            var bytes = new byte[(int)Math.Ceiling((numBits / 8.0))];
            var selectedBits = new bool[numBits];
            for(int i = 0; i < numBits; ++i)
            {
                selectedBits[i] = _bits[_position + i];
            }
            _position += numBits;
            var ba = new BitArray(selectedBits);
            ba.CopyTo(bytes, 0);
            return bytes;
        }

        public int ReadInt32FromBits(int numBits)
        {
            if (numBits > 32)
                throw new ArgumentException("Number of bits shall be at most 32 bits");

            var selectedBits = new bool[numBits];
            for (int i = 0; i < numBits; ++i)
            {
                selectedBits[i] = _bits[_position + i];
            }
            _position += numBits;
            var ba = new BitArray(selectedBits);
            var intArray = new int[1];
            ba.CopyTo(intArray, 0);
            return intArray[0];
        }

        public int ReadVarInt32()
        {
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
            return intArray[0];
        }
    }
}
