using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedDemolish
    {
        public bool Unknown1 { get; private set;}
        public Int32 DemolishedByActorId { get; private set;}
        public bool Unknown2 { get; private set;}
        public Int32 ActorId { get; private set;} // Always equals this actor's id
        public Vector3D Unknown3 { get; private set;}
        public Vector3D Unknown4 { get; private set;}

        public static ReplicatedDemolish Deserialize(BitReader br)
        {
            var rd = new ReplicatedDemolish();
            
            rd.Unknown1 = br.ReadBit();
            rd.DemolishedByActorId = br.ReadInt32();
            rd.Unknown2 = br.ReadBit();
            rd.ActorId = br.ReadInt32();
            rd.Unknown3 = Vector3D.Deserialize(br);
            rd.Unknown4 = Vector3D.Deserialize(br);

            return rd;
        }
    }
}
