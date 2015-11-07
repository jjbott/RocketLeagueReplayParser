using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class Frame
    {
        public int Position { get; private set; }
        public float Time { get; private set; }
        public float Delta { get; private set; }
        public int BitLength { get; private set; }
        private bool[] RawData { get; set; }

        public List<ActorState> ActorStates {get; private set; }

        private List<bool> UnknownBits = new List<bool>();

        public bool Complete { get; private set; }
        public bool Failed { get; private set; }

        public static Frame Deserialize(ref List<ActorState> existingActorStates, IDictionary<int, string> objectIdToName, IEnumerable<ClassNetCache> classNetCache, BitReader br)
        {
            
            var f = new Frame();
            f.Position = br.Position;

            f.Time = br.ReadFloat();
            f.Delta = br.ReadFloat();

            f.ActorStates = new List<ActorState>();

            ActorState lastActorState = null;
            while (br.ReadBit())
            {
                lastActorState = ActorState.Deserialize(existingActorStates, f.ActorStates, objectIdToName, classNetCache, br);

                var existingActor = existingActorStates.Where(x => x.Id == lastActorState.Id).SingleOrDefault();
                if (lastActorState.State != ActorStateState.Deleted)
                {
                    if (existingActor == null)
                    {
                        existingActorStates.Add(lastActorState);
                    }
                }
                else
                {
                    existingActorStates = existingActorStates.Where(x => x.Id != lastActorState.Id).ToList();
                }

                f.ActorStates.Add(lastActorState);
            }

            if (lastActorState == null || lastActorState.Complete)
            {
                f.Complete = true;
            }

            if ( lastActorState != null &&lastActorState.Failed )
            {
                f.Failed = true;
            }

            f.RawData = br.GetBits(f.Position, br.Position - f.Position).ToArray();

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

            var s = string.Format("Frame: Position: {0} Time: {1} Delta {2} BitLength {3}\r\n\tBinary:{4}\r\n",
                Position, Time, Delta, BitLength, RawData.ToBinaryString());
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
