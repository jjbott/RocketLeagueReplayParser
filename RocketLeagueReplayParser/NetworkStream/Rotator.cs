using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class Rotator
    {
        public float Pitch { get; private set; }
        public float Yaw { get; private set; }
        public float Roll { get; private set; }

        private float Normalize(float axis)
        {
            axis %= 360f;
            if ( axis < 0 ) axis += 360;
            return axis;
        }

        private bool IsNearlyZero(float axis)
        {
            const float threshold = 1E-4f; //KINDA_SMALL_NUMBER
            return Math.Abs(Normalize(axis) - 180) < threshold;
        }

        public static Rotator Deserialize(BitReader br)
        {
            var r = new Rotator();
            if (br.ReadBit())
            {
                r.Pitch = br.ReadByte() / 256f * 360f;
            }
            if ( br.ReadBit() )
            {
                r.Yaw = br.ReadByte() / 256f * 360f;
            }
            if (br.ReadBit())
            {
                r.Roll = br.ReadByte() / 256f * 360f;
            }
            return r;
        }

        public void Serialize(BitWriter bw)
        {
            bool nearlyZero = IsNearlyZero(Pitch);
            bw.Write(!nearlyZero);
            if (!nearlyZero)
            {
                bw.Write((byte)Normalize(Pitch) / 360f * 256f);
            }
            nearlyZero = IsNearlyZero(Yaw);
            bw.Write(!nearlyZero);
            if (!nearlyZero)
            {
                bw.Write((byte)Normalize(Yaw) / 360f * 256f);
            }
            nearlyZero = IsNearlyZero(Roll);
            bw.Write(!nearlyZero);
            if (!nearlyZero)
            {
                bw.Write((byte)Normalize(Roll) / 360f * 256f);
            }
        }

        public string ToDebugString()
        {
            return string.Format("Pitch: {0}, Yaw: {1}, Roll: {2}", Pitch, Yaw, Roll);
        }
    }
}
