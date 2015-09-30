using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class Frame
    {
        public float Time { get; private set; }
        public float Delta { get; private set; }
        public int BitLength { get; private set; }
        public byte[] RawData { get; private set; }

        public static Frame Deserialize(bool[] bits)
        {
            var f = new Frame();
            f.BitLength = bits.Length;

            f.RawData = new byte[(int)Math.Ceiling(f.BitLength / 8.0)];
            var ba = new BitArray(bits);
            ba.CopyTo(f.RawData, 0);

            f.Time = BitConverter.ToSingle(f.RawData, 0);
            f.Delta = BitConverter.ToSingle(f.RawData, 4);

            return f;
        }

        public string ToDebugString()
        {


            var ascii = "";
            foreach(byte b in RawData)
            {
                if (b >= 32 && b <= 127 )
                {
                    ascii += (char)b;
                }
                else 
                { 
                    ascii += " "; 
                }
            }
            return string.Format("Frame: Time: {0} Delta {1} BitLength {4}\r\n\tHex:{3}\r\n\tASCII: {2}\r\n", Time, Delta, ascii, BitConverter.ToString(RawData).Replace('-', ' '), BitLength);
        }
    }
}
