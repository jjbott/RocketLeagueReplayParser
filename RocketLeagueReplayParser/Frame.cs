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
        public int Position { get; private set; }
        public float Time { get; private set; }
        public float Delta { get; private set; }
        public int BitLength { get; private set; }
        public byte[] RawData { get; private set; }

        public static Frame Deserialize(int filePosition, bool[] bits)
        {
            var f = new Frame();
            f.Position = filePosition;
            f.BitLength = bits.Length;

            f.RawData = new byte[(int)Math.Ceiling(f.BitLength / 8.0)];
            var ba = new BitArray(bits);
            ba.CopyTo(f.RawData, 0);

            f.Time = BitConverter.ToSingle(f.RawData, 0);
            f.Delta = BitConverter.ToSingle(f.RawData, 4);

            var br = new BitReader(f.RawData);
            br.ReadBitsAsBytes(64); // we already read the time and delta

            while (false) //while(br.ReadBit()) // bit=1 means replicating another actor
            {
                var actorId = br.ReadInt32FromBits(10);
                var channelStateOpen = br.ReadBit();

                if (channelStateOpen)
                    continue;

                var isNewActor = br.ReadBit();
                if (!isNewActor)
                {
                    while (br.ReadBit()) //Has some properties to read!!!
                    {
                        //read 6 bits = property id
                        //the rest is a data which is variable depending on size of property id
                    }
                    //check for and read the properties
                    continue;
                }

                //parse new actor!!!!!!!
                //read a variable sized int to get the type //typeIndex = readVarInt();
                //var ints are read 8 bits at a time, the first 7 bits are data (be careful about the order of these), the 8th bit signifies to keep reading another 8 bits (if 1) or to stop (if 0).
                //from here we can get the string representing this type by looking it up in the objects list given the type index

                bool keepReading;

                do
                {
                    byte[] varInt = br.ReadBitsAsBytes(8);
                    keepReading = (varInt[0] & 0x01) == 0x01; //Check 8th bit to see if we should expect another thingy

                    var typeIndex = 0xfe & varInt[0]; //Get first 7 bits which represents the type index
                } while (keepReading);
            }

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
            return string.Format("Frame: Position: {5} Time: {0} Delta {1} BitLength {4}\r\n\tHex:{3}\r\n\tASCII: {2}\r\n", Time, Delta, ascii, BitConverter.ToString(RawData).Replace('-', ' '), BitLength, Position);
        }
    }
}
