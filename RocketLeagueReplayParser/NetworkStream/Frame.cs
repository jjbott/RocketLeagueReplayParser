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
        

        public List<ActorState> ActorStates {get; private set; }

#if DEBUG
		private bool[] RawData { get; set; }
        private List<bool> UnknownBits = new List<bool>();

        // These are public because, when in debug mode, the caller may decide this frame is bad based on previous frames
        public bool Complete { get; set; }
        public bool Failed { get; set; }
#endif

        public static Frame Deserialize(int maxChannels, ref Dictionary<UInt32, ActorState> existingActorStates, string[] objectIdToName, IDictionary<string, ClassNetCache> classNetCacheByName, UInt32 engineVersion, UInt32 licenseeVersion, UInt32 netVersion, UInt32 changelist, BitReader br)
        {
            
            var f = new Frame();

            f.Position = br.Position;

            f.Time = br.ReadFloat();
            f.Delta = br.ReadFloat();


            if (f.Time < 0 || f.Delta < 0 
                || (f.Time > 0 && f.Time < 1E-10)
                || (f.Delta > 0 && f.Delta < 1E-10))
            {
                string error = string.Format("\"Frame\" at postion {0} has time values that are negative or suspicious. The parser got lost. Check the previous frame for bad data. Time {1}, Delta {2}", f.Position, f.Time, f.Delta);
#if DEBUG
                Console.WriteLine(error);
                f.Failed = true;
                return f;
#endif
                throw new Exception(error);
            }



            f.ActorStates = new List<ActorState>();

            ActorState lastActorState = null;
            while (br.ReadBit())
            {
                lastActorState = ActorState.Deserialize(maxChannels, existingActorStates, f.ActorStates, objectIdToName, classNetCacheByName, engineVersion, licenseeVersion, netVersion, changelist, br);

                ActorState existingActor;
                existingActorStates.TryGetValue(lastActorState.Id, out existingActor);
                if (lastActorState.State != ActorStateState.Deleted)
                {
                    if (existingActor == null)
                    {
                        existingActorStates[lastActorState.Id] = lastActorState;
                    }
                }
                else
                {
                    existingActorStates.Remove(lastActorState.Id);
                }

                f.ActorStates.Add(lastActorState);

                ValidateActorState(lastActorState, existingActorStates);
#if DEBUG
                if (!lastActorState.Complete)
                {
                    break;
                }
#endif
            }
#if DEBUG
            if (lastActorState == null || lastActorState.Complete)
            {
                f.Complete = true;
            }

            if (lastActorState != null && lastActorState.Failed)
            {
                f.Failed = true;
            }

            f.RawData = br.GetBits(f.Position, br.Position - f.Position).ToArray();
#endif
            return f;
        }

        private static void ValidateActorState(ActorState actorState, Dictionary<UInt32, ActorState> existingActorStates)
        {
#if DEBUG
            if (actorState.Properties != null)
            {
                foreach(var activeActorProperty in actorState.Properties.Values.Where(p => p.Data is ActiveActor))
                {
                    var actorId = ((ActiveActor)activeActorProperty.Data).ActorId;
                    if (actorId != -1 && !existingActorStates.ContainsKey((UInt32)actorId))
                    {
                        throw new Exception($"Found ActiveActor that points to an unknown actor id. ActorId {actorId}, PropertyName {activeActorProperty.PropertyId}");
                    }
                }
            }
#endif
        }

        public void Serialize(int maxChannels, string[] objectNames, UInt32 engineVersion, UInt32 licenseeVersion, UInt32 netVersion, UInt32 changelist, BitWriter bw)
        {
            bw.Write(Time);
            bw.Write(Delta); // TODO: recalculate

            foreach (var deletedActor in ActorStates.Where(a => a.State == ActorStateState.Deleted))
            {
                bw.Write(true); // There is another actor state
                deletedActor.Serialize(maxChannels, objectNames, engineVersion, licenseeVersion, netVersion, changelist, bw);
            }

            foreach (var newActor in ActorStates.Where(a => a.State == ActorStateState.New))
            {
                bw.Write(true); // There is another actor state
                
                newActor.Serialize(maxChannels, objectNames, engineVersion, licenseeVersion, netVersion, changelist, bw);
            }

            foreach (var existingActor in ActorStates.Where(a => a.State == ActorStateState.Existing))
            {
                bw.Write(true); // There is another actor state

                existingActor.Serialize(maxChannels, objectNames, engineVersion, licenseeVersion, netVersion, changelist, bw);
            }
  
            bw.Write(false); // No more actor states
        }


#if DEBUG
        public string ToDebugString(string[] objects, string[] names)
        {

            var s = string.Format("Frame: Position: {0} Time: {1} Delta {2} BitLength {3}\r\n\tBinary:{4}\r\n",
                Position, Time, Delta, BitLength, (RawData ?? new bool[0]).ToBinaryString());

            if (ActorStates != null)
            {
                foreach (var a in ActorStates)
                {
                    s += "    " + a.ToDebugString(objects, names) + "\r\n";
                }
            }

            if (UnknownBits != null && UnknownBits.Count > 0)
            {
                s += string.Format("    UnknownBits: {0}\r\n", UnknownBits.ToBinaryString());
            }

            return s;
        }
#endif
    }
}
