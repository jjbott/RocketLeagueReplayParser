using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class CameraSettings
    {
        public float FieldOfView { get; private set; }
        public float Height { get; private set; }
        public float Pitch { get; private set; }
        public float Distance { get; private set; }
        public float Stiffness { get; private set; }
        public float SwivelSpeed { get; private set; }
        public float TransitionSpeed { get; private set; }

        public static CameraSettings Deserialize(BitReader br, UInt32 engineVersion, UInt32 licenseeVersion)
        {
            var cs = new CameraSettings();

            cs.FieldOfView = br.ReadFloat();
            cs.Height = br.ReadFloat();
            cs.Pitch = br.ReadFloat();
            cs.Distance = br.ReadFloat();
            cs.Stiffness = br.ReadFloat();
            cs.SwivelSpeed = br.ReadFloat();

            if (engineVersion >= 868 && licenseeVersion >= 20)
            {
                cs.TransitionSpeed = br.ReadFloat();
            }

            return cs;
        }

        public void Serialize(BitWriter bw, UInt32 engineVersion, UInt32 licenseeVersion)
        {
            bw.Write(FieldOfView);
            bw.Write(Height);
            bw.Write(Pitch);
            bw.Write(Distance);
            bw.Write(Stiffness);
            bw.Write(SwivelSpeed);

            if (engineVersion >= 868 && licenseeVersion >= 20)
            {
                bw.Write(TransitionSpeed);
            }
        }

        public override string ToString()
        {
            return string.Format("FieldOfView:{0}, Height:{1}, Pitch:{2}, Distance:{3}, Stiffness:{4}, SwivelSpeed:{5}, TransitionSpeed:{6}", FieldOfView, Height, Pitch, Distance, Stiffness, SwivelSpeed, TransitionSpeed);
        }
        
    }
}
