using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class CameraSettings
    {
        public byte Unknown1 { get; private set; }
        public byte Unknown2 { get; private set; }
        public Int32 FieldOfView { get; private set; }
        public Int32 Unknown3{ get; private set; }
        public Int32 Unknown4 { get; private set; }
        public Int32 Unknown5 { get; private set; }
        public Int32 Unknown6 { get; private set; }
        public byte Unknown7 { get; private set; }
        public byte Unknown8 { get; private set; }

        public static CameraSettings Deserialize(BitReader br)
        {
            var cs = new CameraSettings();

            // Invert Swivel Pitch: on/off / No effect on data
            // Invert Spectator Pitch: on/off // no effect on data
            // Camera Shake: on/off // No effect on data
            // FOV: 60 - 110
            // Height: 40 - 200
            // Angle: -45 - 0
            // Distance: 100 - 400
            // Stiffness: 0 - 1
            // Swivel Speed: 1 - 10
            // Ball Cam Indicator: on/off
            // Hold Ball Camera: on/off


            cs.Unknown1 = br.ReadByte();
            cs.Unknown2 = br.ReadByte();

            // These all seem fishy
            cs.FieldOfView = br.ReadInt32();
            cs.Unknown3 = br.ReadInt32();
            cs.Unknown4 = br.ReadInt32();
            cs.Unknown5 = br.ReadInt32();
            cs.Unknown6 = br.ReadInt32();
            cs.Unknown7 = br.ReadByte();
            cs.Unknown8 = br.ReadByte();

            return cs;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}", Unknown1, Unknown2, FieldOfView, Unknown3, Unknown4, Unknown5, Unknown6, Unknown7, Unknown8);
        }
        
    }
}
