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

        // Added in 868.34 (not present in 868.32).
        public float? AccelerationRate { get; private set; }
        public float? DecelerationRate { get; private set; }
        public float? FreeLookSpeed { get; private set; }
        public bool? UnconstrainRotation { get; private set; }
        public bool? FreeLookSmoothing { get; private set; }

        private static bool HasExtendedSettings(UInt32 engineVersion, UInt32 licenseeVersion)
        {
            return engineVersion >= 868 && licenseeVersion >= 34;
        }

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

            if (HasExtendedSettings(engineVersion, licenseeVersion))
            {
                cs.AccelerationRate = br.ReadFloat();
                cs.DecelerationRate = br.ReadFloat();
                cs.FreeLookSpeed = br.ReadFloat();
                cs.UnconstrainRotation = br.ReadBit();
                cs.FreeLookSmoothing = br.ReadBit();
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

            if (HasExtendedSettings(engineVersion, licenseeVersion))
            {
                bw.Write(AccelerationRate.Value);
                bw.Write(DecelerationRate.Value);
                bw.Write(FreeLookSpeed.Value);
                bw.Write(UnconstrainRotation.Value);
                bw.Write(FreeLookSmoothing.Value);
            }
        }

        public override string ToString()
        {
            return string.Format("FieldOfView:{0}, Height:{1}, Pitch:{2}, Distance:{3}, Stiffness:{4}, SwivelSpeed:{5}, TransitionSpeed:{6}", FieldOfView, Height, Pitch, Distance, Stiffness, SwivelSpeed, TransitionSpeed);
        }
        
    }
}
