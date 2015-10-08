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

        public List<ActorState> ActorStates {get; private set; }

        public List<bool> UnknownBits = new List<bool>();

        public static Frame Deserialize(List<ActorState> existingActorStates, IDictionary<int, string> objectIdToName, IEnumerable<ClassNetCache> classNetCache, int filePosition, bool[] bits)
        {
            
            var f = new Frame();
            f.Position = filePosition;
            f.BitLength = bits.Length;

            f.RawData = new byte[(int)Math.Ceiling(f.BitLength / 8.0)];
            var ba = new BitArray(bits);
            ba.CopyTo(f.RawData, 0);

            f.Time = BitConverter.ToSingle(f.RawData, 0);
            f.Delta = BitConverter.ToSingle(f.RawData, 4);

            var br = new BitReader(bits);
            br.ReadBitsAsBytes(64); // we already read the time and delta

            f.ActorStates = new List<ActorState>();

            ActorState lastActorState = null;
            while ( (lastActorState == null || lastActorState.Complete) && ((br.Position + 12) < f.BitLength) && br.ReadBit() )// TODO: Switch to while(br.ReadBit()) // bit=1 means replicating another actor
            {
                lastActorState = ActorState.Deserialize(existingActorStates, f.ActorStates, objectIdToName, classNetCache, br);

                var existingActor = existingActorStates.Where(x => x.Id == lastActorState.Id).SingleOrDefault();
                if (existingActor == null)
                {
                    existingActorStates.Add(lastActorState);
                }

                //f.ActorStates.Add(lastActorState);
            }

            while (!br.EndOfStream)
            {
                f.UnknownBits.Add(br.ReadBit());
            }

            return f;
        }

        public string DataToBinaryString()
        {
            var ba = new BitArray(RawData);
            var sb = new StringBuilder();
            for (int i = 64; i < BitLength; ++i)
            {
                if (i != 0 && (i % 8) == 0)
                {
                   // sb.Append(" ");
                }
                sb.Append((ba[i] ? 1 : 0).ToString());
            }
            return sb.ToString();
        }

        public string ToDebugString(string[] objects)
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
            var s = string.Format("Frame: Position: {0} Time: {1} Delta {2} BitLength {3}\r\n\tBinary:{4}\r\n\tASCII: {5}\r\n",
                Position, Time, Delta, BitLength, DataToBinaryString(), ascii);
            foreach(var a in ActorStates)
            {
                s += "    " + a.ToDebugString(objects) + "\r\n";
            }

            if (UnknownBits != null && UnknownBits.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < UnknownBits.Count; ++i)
                {
                    sb.Append((UnknownBits[i] ? 1 : 0).ToString());
                }

                s += string.Format("    UnknownBits: {0}\r\n", sb.ToString());
            }

            return s;
        }
    }
}
