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

            while(br.ReadBit())
            {
                var actorId = br.ReadInt32FromBits(10);
                var channelStateOpen = br.ReadBit();

                if (channelStateOpen)
                    continue;

                var isNewActor = br.ReadBit();

                if (!isNewActor)
                {
                    //check for and read the properties
                    continue;
                }

                //parse new actor
            }

            /*
             * //while we have more actors
while (readBit() == 1) {
    actorId = readBits(10);
​
    channelState = readBit();
    if (channelState == 0) {
        //channel is closed, actor is destroyed
        continue;
    }
​
    //channel is open
​
    actorState = readBit();
    if (actorState == 0) {
        //existing actor
​
        while (readBit() == 1) {
            //read properties
        }
        continue;
    }
​
    //new actor
    typeIndex = readVarInt(); //var ints are read 8 bits at a time, the first 7 bits are data (be careful about the order of these), the 8th bit signifies to keep reading another 8 bits (if 1) or to stop (if 0).
    //from here we can get the string representing this type by looking it up in the objects list given the type index
    //read data
}*/

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
