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

        public IEnumerable<byte> Serialize()
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(Time));
            result.AddRange(BitConverter.GetBytes(Frame));
            result.AddRange(BitConverter.GetBytes(FilePosition));
            return result;
        }

        public string ToDebugString()
        {
            return string.Format("Keyframe: Time {0} Frame {1} FilePosition {2}", Time, Frame, FilePosition);
        }
    }
}
