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

        public static ClientLoadoutsOnline Deserialize(BitReader br)
        {
            var clo = new ClientLoadoutsOnline();
            clo.LoadoutOnline1 = ClientLoadoutOnline.Deserialize(br);
            clo.LoadoutOnline2 = ClientLoadoutOnline.Deserialize(br);

            clo.Unknown1 = br.ReadBit();
            clo.Unknown2 = br.ReadBit();
            return clo;
        }

        public void Serialize(BitWriter bw)
        {
            LoadoutOnline1.Serialize(bw);
            LoadoutOnline2.Serialize(bw);

            bw.Write(Unknown1);
            bw.Write(Unknown2);
        }
    }
}
