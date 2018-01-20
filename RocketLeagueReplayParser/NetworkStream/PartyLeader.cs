using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class PartyLeader : UniqueId
    {

        public new static PartyLeader Deserialize(BitReader br, UInt32 licenseeVersion, UInt32 netVersion)
        {
            PartyLeader pl = new PartyLeader();

            List<object> data = new List<object>();
            pl.Type = (UniqueIdType)br.ReadByte();

            if (pl.Type != UniqueIdType.Unknown)
            {
                UniqueId.DeserializeId(br, pl, licenseeVersion, netVersion);
            }
            
            return pl;
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write((byte)Type);
            if (Type != UniqueIdType.Unknown)
            {
                SerializeId(bw);
            }
        }
    }
}
