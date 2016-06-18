using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class BitWriter
    {
        BitArray _bits;
        public int Position { get; private set; }

        public BitWriter(int initialCapacity)
        {
            _bits = new BitArray(initialCapacity);
            Position = 0;
        }
        
        public Int32 Length
        {
            get
            {
                return Position;
            }
        }

        public void Write(bool value)
        {
            // grow if necessary
            while (Position >= _bits.Length)
            {
                if (_bits.Length == 0)
                {
                    _bits.Length = 1024;
                }
                else
                {
                    _bits.Length = _bits.Length * 2;
                }
            }

            _bits[Position++] = value;
        }

        public void Write(byte value)
        {
            var bits = new BitArray(new byte[] { value });
            foreach(var b in bits.Cast<bool>())
            {
                Write(b);
            }
        }

        public void Write(UInt32 value, UInt32 maxValue)
        {
            var maxBits = Math.Floor(Math.Log10(maxValue) / Math.Log10(2)) + 1;
            UInt32 writtenValue = 0;
            for (int i = 0; i < maxBits && (writtenValue + (1 << i)) < maxValue; ++i)
            {
                var bit = (value & (1 << i)) > 0;
                writtenValue += bit ? (UInt32)(1 << i) : 0U;
                Write(bit);
            }
        }

        public void Write(UInt32 value)
        {
            WriteFixedBitCount(value, 32);
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

        public void WriteFixedBitCount(UInt32 value, int numBits)
        {
            if (numBits <= 0 || numBits > 32)
                throw new ArgumentException("Number of bits shall be at most 32 bits");

            if ((1 << (numBits - 1)) < value)
                throw new ArgumentException("Value can be represented with the number of bits specified");

            for ( int i = (numBits - 1); i >= 0; --i)
            {
                var bitValue = 1 << i;
                Write((value & bitValue) == bitValue);
            }
        }

        public void Write(float value)
        {
            Write(BitConverter.GetBytes(value));
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

        public void Write(IEnumerable<byte> bytes)
        {
            foreach(var b in bytes)
            {
                Write(b);
            }
        }
        
        public void WriteFixedCompressedFloat(float Value, Int32 MaxValue, Int32 NumBits)
        {
            Int32 MaxBitValue = (1 << (NumBits - 1)) - 1;    //   0111 1111 - Max abs value we will serialize
            Int32 Bias = (1 << (NumBits - 1));       //   1000 0000 - Bias to pivot around (in order to support signed values)
            UInt32 SerIntMax = (UInt32)(1 << (NumBits - 0));      // 1 0000 0000 - What we pass into SerializeInt
            UInt32 MaxDelta = (UInt32)(1 << (NumBits - 0)) - 1;   //   1111 1111 - Max delta is

            bool clamp = false;
            Int32 ScaledValue;
            if (MaxValue > MaxBitValue)
            {
                // We have to scale this down, scale needs to be a float:
                float scale = (float)MaxBitValue / (float)MaxValue;
                ScaledValue = (Int32)(scale * Value);
            }
            else
            {
                // We will scale up to get extra precision. But keep is a whole number preserve whole values
                Int32 scale = MaxBitValue / MaxValue; // TODO: Check if int division is okay
                ScaledValue = (int)Math.Round(scale * Value);
            }

            UInt32 Delta = unchecked((UInt32)(ScaledValue + Bias));

            if (Delta > MaxDelta)
            {
                clamp = true;
                Delta = unchecked((Int32)Delta) > 0 ? MaxDelta : 0U;
            }

            Write(Delta, SerIntMax);
            //Ar.SerializeInt( Delta, SerIntMax );

            //return !clamp;
        }
    }
}
