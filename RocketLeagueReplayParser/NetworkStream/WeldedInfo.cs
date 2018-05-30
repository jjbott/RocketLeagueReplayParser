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
        public Rotator Rotation { get; private set; }

        public static WeldedInfo Deserialize(BitReader br, UInt32 netVersion)
        {
            var wi = new WeldedInfo();
            wi.Active = br.ReadBit();
            wi.ActorId = br.ReadInt32();
            wi.Offset = Vector3D.Deserialize(br, netVersion);
            wi.Mass = br.ReadFloat();
            wi.Rotation = Rotator.Deserialize(br);

            return wi;
        }

        public void Serialize(BitWriter bw, UInt32 netVersion)
        {
            bw.Write(Active);
            bw.Write((UInt32)ActorId);
            Offset.Serialize(bw, netVersion);
            bw.Write(Mass);
            Rotation.Serialize(bw);
        }
    }
}
