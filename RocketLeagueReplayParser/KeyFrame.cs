using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class KeyFrame
    {
        public float Time { get; private set; }
        public Int32 Frame { get; private set; }
        public Int32 FilePosition { get; private set; }

        public static KeyFrame Deserialize(BinaryReader bs)
        {
            var keyFrame = new KeyFrame();
            keyFrame.Time = bs.ReadSingle();
            keyFrame.Frame = bs.ReadInt32();
            keyFrame.FilePosition = bs.ReadInt32();
            return keyFrame;
        }

        public string ToDebugString()
        {
            return string.Format("Keyframe: Time {0} Frame {1} FilePosition {2}", Time, Frame, FilePosition);
        }
    }
}
