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
        public Int32 Time { get; private set; }
        public Int32 Frame { get; private set; }
        public Int32 FilePosition { get; private set; }

        public static KeyFrame Deserialize(BinaryReader bs)
        {
            var keyFrame = new KeyFrame();
            keyFrame.Time = bs.ReadInt32();
            keyFrame.Frame = bs.ReadInt32();
            keyFrame.FilePosition = bs.ReadInt32();
            return keyFrame;
        }
    }
}
