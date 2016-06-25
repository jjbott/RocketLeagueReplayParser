using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ClientLoadoutOnline
    {
        public byte Version { get; private set; }
        public UInt32 Unknown1 { get; private set; }
        public UInt32 Unknown2 { get; private set; }
        public byte Unknown3 { get; private set; }
        public byte Count { get; private set; }
        public static ClientLoadoutOnline Deserialize(BitReader br)
        {
            var p = br.Position;
            bool dump = false;
            var clo = new ClientLoadoutOnline();

            // TODO: Store all of this stuff. It's just throwing everything away right now.
            var count1 = br.ReadByte();
            for (int i = 0; i < count1; ++i)
            {
                var count2 = br.ReadByte();
                for (int j = 0; j < count2; ++j)
                {
                    br.ReadInt32();
                    br.ReadUInt32Max(27); // made up number, need more data
                    if ( i >= 21 )
                    {
                        br.ReadUInt32();
                    }
                    dump = true;
                }
            }
            
#if DEBUG
            if ( false && dump )
            {
                Console.WriteLine(br.GetBits(p, br.Position - p+50).ToBinaryString());
            }
#endif
            return clo;
        }
    }
}
