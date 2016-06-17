using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ClientLoadout
    {
        public byte Unknown1 { get; private set; } // Always 10, except when it's 11

        // Product Ids are in TAGame.upk in the ProductsDB content
        
        public UInt32 BodyProductId { get; private set; }
        public UInt32 SkinProductId { get; private set; }
        public UInt32 WheelProductId { get; private set; }
        public UInt32 BoostProductId { get; private set; }
        public UInt32 AntennaProductId { get; private set; }
        public UInt32 HatProductId { get; private set; }

        public UInt32 Unknown2 { get; private set; } // Always 0. Future expansion room for a different product type?

        public static ClientLoadout Deserialize(BitReader br)
        {
            var cl = new ClientLoadout();

            cl.Unknown1 = br.ReadByte();
            cl.BodyProductId = br.ReadUInt32();
            cl.SkinProductId = br.ReadUInt32();
            cl.WheelProductId = br.ReadUInt32();
            cl.BoostProductId = br.ReadUInt32();
            cl.AntennaProductId = br.ReadUInt32();
            cl.HatProductId = br.ReadUInt32();
            cl.Unknown2 = br.ReadUInt32();

			if (cl.Unknown1 > 10 )
			{
				br.ReadUInt32();
			}

            return cl;
        }

		public override string ToString()
		{
			return string.Format("Unknown1 {0}, BodyProductId {1}, SkinProductId {2}, WheelProductId {3}, BoostProductId {4}, AntennaProductId {5}, HatProductId {6}, Unknown2 {7}", Unknown1, BodyProductId, SkinProductId, WheelProductId, BoostProductId, AntennaProductId, HatProductId, Unknown2);
		}
    }
}
