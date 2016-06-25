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

            clo.Version = br.ReadByte();
            clo.Unknown1 = br.ReadUInt32();
            clo.Unknown2 = br.ReadByte();
            clo.Count = br.ReadByte();
            for(int i = 0; i < clo.Count; ++i)
            {
                // TODO: Store
                br.ReadInt32();
                br.ReadUInt32Max(27); // made up number, need more data
                dump = true;
            }
            //TODO: Store
            br.ReadByte();
            br.ReadUInt32();

            if (clo.Version >= 12)
            {
                clo.Unknown3 = br.ReadByte();
            }
#if DEBUG
            if ( dump )
            {
                Console.WriteLine(br.GetBits(p, br.Position - p+50).ToBinaryString());
            }
#endif
            return clo;
        }
    }
}
