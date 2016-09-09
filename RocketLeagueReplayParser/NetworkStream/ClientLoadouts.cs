using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ClientLoadouts
    {
        public ClientLoadout Loadout1 { get; private set; } // Blue or orange?
        public ClientLoadout Loadout2 { get; private set; }

        public static ClientLoadouts Deserialize(BitReader br)
        {
            var clo = new ClientLoadouts();
            clo.Loadout1 = ClientLoadout.Deserialize(br);
            clo.Loadout2 = ClientLoadout.Deserialize(br);
            return clo;
        }

        public void Serialize(BitWriter bw)
        {
            Loadout1.Serialize(bw);
            Loadout2.Serialize(bw);
        }
    }
}
