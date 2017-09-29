using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ClientLoadoutOnline
    {
        public List<List<ClientLoadoutOnlineThing>> ThingLists { get; private set; }

        public static ClientLoadoutOnline Deserialize(BitReader br, UInt32 versionMajor, UInt32 versionMinor)
        {
            var clo = new ClientLoadoutOnline();
            clo.ThingLists = new List<List<ClientLoadoutOnlineThing>>();
            
            var listCount = br.ReadByte();
            for (int i = 0; i < listCount; ++i)
            {
                var thingList = new List<ClientLoadoutOnlineThing>();

                var thingCount = br.ReadByte();
                for (int j = 0; j < thingCount; ++j)
                {
                    thingList.Add(ClientLoadoutOnlineThing.Deserialize(br, versionMajor, versionMinor));

                    if ( i >= 21 )
                    {
                        thingList.Add(ClientLoadoutOnlineThing.Deserialize(br, versionMajor, versionMinor));
                    }
                }

                clo.ThingLists.Add(thingList);
            }

            if (versionMajor >= 868 && versionMinor >= 20 && clo.ThingLists.SelectMany(t=>t).Any())
            {
                br.ReadBit();
            }

            return clo;
        }

        public void Serialize(BitWriter bw, UInt32 versionMajor, UInt32 versionMinor)
        {
            bw.Write((byte)ThingLists.Count);
            foreach (var thingList in ThingLists)
            {
                bw.Write((byte)thingList.Count);
                foreach(var thing in thingList)
                {
                    thing.Serialize(bw, versionMajor, versionMinor);
                    // "i >= 21" logic from Deserialize is handled automatically here. No special serialize logic needed.
                }
            }
        }
    }
}
