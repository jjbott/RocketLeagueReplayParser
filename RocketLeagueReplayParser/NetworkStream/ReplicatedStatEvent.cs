using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedStatEvent
    {
        public bool Unknown1 { get; private set; }

        // If >= 0, will map to an object like "StatEvents.Events.EpicSave".
        // Finally!
        public Int32 ObjectId { get; private set; }

        public static ReplicatedStatEvent Deserialize(BitReader br)
        {
            var rse = new ReplicatedStatEvent();
            rse.Unknown1 = br.ReadBit();
            rse.ObjectId = br.ReadInt32();
            return rse;
        }

        public virtual void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(ObjectId);
        }

        public override string ToString()
        {
            return string.Format("Unknown1: {0}, ObjectId: {1}", Unknown1, ObjectId);
        }
    }
}
