using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ReplicatedMusicStinger
    {
        public bool Unknown1 { get; private set; }
        public UInt32 ObjectIndex { get; private set; }
        // Seems to start at 2 and increases by 1 every time it shows up.
        public byte Unknown2 { get; private set; }

        public static ReplicatedMusicStinger Deserialize(BitReader br)
        {
            var rms = new ReplicatedMusicStinger();

            rms.Unknown1 = br.ReadBit();
            rms.ObjectIndex = br.ReadUInt32();
            rms.Unknown2 = br.ReadByte();

            return rms;
        }

        public virtual void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(ObjectIndex);
            bw.Write(Unknown2);
        }
    }
}
