using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ClientLoadoutOnlineThing
    {
        const int MAX_UNKNOWN2 = 27; // This may need tweaking, but it works well enough for now

        public UInt32 Unknown1 { get; private set; } // Wild ass guess - tied to material "expression" id?
        public UInt32 Unknown2 { get; private set; }

        public static ClientLoadoutOnlineThing Deserialize(BitReader br, UInt32 versionMajor, UInt32 versionMinor)
        {
            var clot = new ClientLoadoutOnlineThing(); // ha, "clot"
            clot.Unknown1 = br.ReadUInt32();
            if (versionMajor >= 868 && versionMinor >= 20)
            {
                clot.Unknown2 = br.ReadUInt32Max(4); 
            }
            else if (versionMajor >= 868 && versionMinor >= 18)
            {
                clot.Unknown2 = br.ReadUInt32();
            }
            else
            {
                clot.Unknown2 = br.ReadUInt32Max(MAX_UNKNOWN2);
            }
            return clot;
        }

        public void Serialize(BitWriter bw, UInt32 versionMajor, UInt32 versionMinor)
        {
            bw.Write(Unknown1);
            if (versionMajor >= 868 && versionMinor >= 20)
            {
                bw.Write(Unknown2, MAX_UNKNOWN2);
            }
            else if (versionMajor >= 868 && versionMinor >= 18)
            {
                bw.Write(Unknown2);
            }
            else
            {
                bw.Write(Unknown2, MAX_UNKNOWN2);
            }
        }
    }
}
