using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class WeldedInfo
    {
        public bool Active { get; private set; }
        public Int32 ActorId { get; private set; }
        public Vector3D Offset { get; private set; }
        public float Mass { get; private set; }
        // Probably rotation. Look into the correct way to decode. Probably the same as in ActorState, which I guess I haven't decoded yet either
        public byte? Unknown1 { get; private set; }
        public byte? Unknown2 { get; private set; }
        public byte? Unknown3 { get; private set; }

        public static WeldedInfo Deserialize(BitReader br)
        {
            var wi = new WeldedInfo();
            wi.Active = br.ReadBit();
            wi.ActorId = br.ReadInt32();
            wi.Offset = Vector3D.Deserialize(br);
            wi.Mass = br.ReadFloat();

            if ( br.ReadBit())
            {
                wi.Unknown1 = br.ReadByte();
            }

            if (br.ReadBit())
            {
                wi.Unknown2 = br.ReadByte();
            }

            if (br.ReadBit())
            {
                wi.Unknown3 = br.ReadByte();
            }

            return wi;
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write(Active);
            bw.Write(ActorId);
            Offset.Serialize(bw);
            bw.Write(Mass);
            bw.Write(Unknown1.HasValue);
            if (Unknown1.HasValue)
            {
                bw.Write(Unknown1.Value);
            }
            bw.Write(Unknown2.HasValue);
            if (Unknown2.HasValue)
            {
                bw.Write(Unknown2.Value);
            }
            bw.Write(Unknown3.HasValue);
            if (Unknown3.HasValue)
            {
                bw.Write(Unknown3.Value);
            }
        }
    }
}
