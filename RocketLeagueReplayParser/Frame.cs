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

        public bool Complete { get; private set; }
        public bool Failed { get; private set; }

        public static Frame Deserialize(ref List<ActorState> existingActorStates, IDictionary<int, string> objectIdToName, IEnumerable<ClassNetCache> classNetCache, int filePosition, bool[] bits)
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

            try
            {


                ActorState lastActorState = null;
                while ((lastActorState == null || lastActorState.Complete || lastActorState.ForcedComplete) && br.ReadBit())
                {
                    lastActorState = ActorState.Deserialize(existingActorStates, f.ActorStates, objectIdToName, classNetCache, br);

                    var existingActor = existingActorStates.Where(x => x.Id == lastActorState.Id).SingleOrDefault();
                    if (lastActorState.State != "Deleted")
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

                    if( !lastActorState.Complete 
                        || (!br.PeekBit() && ((br.Length - br.Position) > 1) ) ) // 0 bit, signalling end of frame, but too many bits left to be truly complete
                    {
                        lastActorState.Complete = false;

                        // Try to find the next ActorState
                        // Always deleted first, then new, then existing
                        // Within those states id always increases
                        //     ^ Nope thats not true
                        // Will be stuck in New or Existing land, since Deleted states are easy.
                        // If last was New, next ActorState will look like 1XXXXXXXXXX1
                        // If last was Existing, next ActorState will look like 01XXXXXXXXXX1 (0 prefix from the end of last state (no more properties)
                        var startPosition = br.Position;
                        for(var p = br.Position; p < br.Length - 20; p++ ) // I think the smallest non-deleted actor state will be 20 bits (1 0000000000 10 1 0000 0 0)
                        {
                            br.Seek(p);
                            Int32 potentialId = -1;
                            if (br.ReadBit())
                            {
                                potentialId = br.ReadInt32FromBits(10);

                                if (br.ReadBit()) // 1 for New/Existing
                                {
                                    var newActor = br.ReadBit();

                                    if (newActor && lastActorState.State == "Existing")
                                    {
                                        // Nope, cant have a new actor after an existing actor
                                        continue;
                                    }

                                    if (newActor
                                        && (lastActorState.State == "New")
                                        && (potentialId > lastActorState.Id) // id is increasing
                                        && (potentialId < lastActorState.Id + 100)  // id isnt crazy
                                        && ((br.Length - br.Position) > 33) // have enough bits left for the typeid and unknown bit
                                        )
                                    {
                                        // Heck, lets give it a shot.
                                        lastActorState.ForcedComplete = true;
                                        lastActorState.UnknownBits = br.GetBits(startPosition, p - startPosition);
                                        br.Seek(p);
                                        break;
                                    }

                                    if (!newActor
                                        && (lastActorState.State == "New")
                                        //&& (potentialId <= lastActorState.Id) // Potentially first existing actor after a new actor. Id can be at most equal to last actor id
                                        // I think... We could get lucky enough to have only a New actor that doesnt need extra data, and then Existing data for something else...
                                        && (existingActorStates.Where(x => x.Id == potentialId).Any())  // We have a match in our list
                                        && br.ReadBit()  // Must have a 1 here for "new property
                                        && ((br.Length - br.Position) > 6) // have enough bits lefts for the smallest prop (4 bits for id, 1 for bool value, 1 bit for 'no more properties'
                                        )
                                    {
                                        // Heck, lets give it a shot.
                                        lastActorState.ForcedComplete = true;
                                        lastActorState.UnknownBits = br.GetBits(startPosition, p - startPosition);
                                        br.Seek(p);
                                        break;
                                    }

                                    if (!newActor
                                        && (lastActorState.State == "Existing")
                                        //&& (potentialId > lastActorState.Id) // Id is increasing
                                        && (existingActorStates.Where(x => x.Id == potentialId).Any())  // We have a match in our list
                                        && br.ReadBit()  // Must have a 1 here for "new property
                                        && ((br.Length - br.Position) > 6) // have enough bits lefts for the smallest prop (4 bits for id, 1 for bool value, 1 bit for 'no more properties'
                                        )
                                    {
                                        // Heck, lets give it a shot.
                                        // This same code is in here 3 times, silly. Make it better.
                                        lastActorState.ForcedComplete = true;
                                        lastActorState.UnknownBits = br.GetBits(startPosition, p - startPosition);
                                        br.Seek(p);
                                        break;
                                    }
                                }
                            }
                        }

                        if ( !lastActorState.Complete && !lastActorState.ForcedComplete )
                        {
                            // Didnt find anything. Reset position so unknown bits get recorded properly
                            br.Seek(startPosition);
                        }
                    }

                    //f.ActorStates.Add(lastActorState);
                }

                if (br.EndOfStream && (lastActorState == null || lastActorState.Complete))
                {
                    f.Complete = true;
                }

                if ( lastActorState != null &&lastActorState.Failed )
                {
                    f.Failed = true;
                }
            }
            catch (Exception) { }

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
