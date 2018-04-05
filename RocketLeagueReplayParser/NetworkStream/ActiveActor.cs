using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ActiveActor
    {
        public bool Active { get; private set; }
        public Int32 ActorId { get; private set; }

        public static ActiveActor Deserialize(BitReader br)
        {
            // If Active == false, ActorId will be -1
            var aa = new ActiveActor();
            aa.Active = br.ReadBit();
            aa.ActorId = br.ReadInt32();
            return aa;
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write(Active);
            bw.Write(ActorId);
        }

        public override string ToString()
        {
            return string.Format("Active: {0}, ActorId: {1}", Active, ActorId);
        }
    }
}
