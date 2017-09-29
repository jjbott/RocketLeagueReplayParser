using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ClientLoadoutsOnline
    {
        public ClientLoadoutOnline LoadoutOnline1 { get; private set; }
        public ClientLoadoutOnline LoadoutOnline2 { get; private set; }
        public bool Unknown1 { get; private set; }
        public bool Unknown2 { get; private set; }

        public static ClientLoadoutsOnline Deserialize(BitReader br, UInt32 versionMajor, UInt32 versionMinor)
        {
            var clo = new ClientLoadoutsOnline();
            clo.LoadoutOnline1 = ClientLoadoutOnline.Deserialize(br, versionMajor, versionMinor);
            clo.LoadoutOnline2 = ClientLoadoutOnline.Deserialize(br, versionMajor, versionMinor);

            if ( clo.LoadoutOnline1.ThingLists.Count != clo.LoadoutOnline2.ThingLists.Count)
            {
                throw new Exception("ClientLoadoutOnline list counts must match");
            }

            clo.Unknown1 = br.ReadBit();
            clo.Unknown2 = br.ReadBit();
            return clo;
        }

        public void Serialize(BitWriter bw, UInt32 versionMajor, UInt32 versionMinor)
        {
            LoadoutOnline1.Serialize(bw, versionMajor, versionMinor);
            LoadoutOnline2.Serialize(bw, versionMajor, versionMinor);

            bw.Write(Unknown1);
            bw.Write(Unknown2);
        }
    }
}
