using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedExplosionData
    {
        public bool Unknown1 { get; private set; }
        public UInt32 Unknown2 { get; private set; }
        public Vector3D Position { get; private set; }

        public static ReplicatedExplosionData Deserialize(BitReader br)
        {
            var red = new ReplicatedExplosionData();

            red.DeserializeImpl(br);

            return red;
        }

        protected virtual void DeserializeImpl(BitReader br)
        {
            Unknown1 = br.ReadBit();
            Unknown2 = br.ReadUInt32();
            Position = Vector3D.Deserialize(br);
        }

        public virtual void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            Position.Serialize(bw);
        }
    }
}
