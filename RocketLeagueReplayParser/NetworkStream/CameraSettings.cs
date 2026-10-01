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

        // Added in 868.34 (not present in 868.32). Meaning unknown; the first two were 5.8 and 10.8 for every
        // player in the sample replay, the third varied per player (3.2 - 6.0).
        public float? Unknown1 { get; private set; }
        public float? Unknown2 { get; private set; }
        public float? Unknown3 { get; private set; }

        // Also added in 868.34. 41 bits of unknown layout, kept raw so the replay can be re-serialized.
        [Newtonsoft.Json.JsonIgnore]
        public bool[] UnknownBits { get; private set; }

        private const int UnknownBitCount = 41;

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
                cs.Unknown1 = br.ReadFloat();
                cs.Unknown2 = br.ReadFloat();
                cs.Unknown3 = br.ReadFloat();
                cs.UnknownBits = br.GetBits(br.Position, UnknownBitCount).ToArray();
                br.Seek(br.Position + UnknownBitCount);
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
                bw.Write(Unknown1.Value);
                bw.Write(Unknown2.Value);
                bw.Write(Unknown3.Value);
                foreach (var bit in UnknownBits)
                {
                    bw.Write(bit);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("FieldOfView:{0}, Height:{1}, Pitch:{2}, Distance:{3}, Stiffness:{4}, SwivelSpeed:{5}, TransitionSpeed:{6}", FieldOfView, Height, Pitch, Distance, Stiffness, SwivelSpeed, TransitionSpeed);
        }
        
    }
}
