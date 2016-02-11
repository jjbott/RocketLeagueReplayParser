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
        
        public Int32 BodyProductId { get; private set; }
        public Int32 SkinProductId { get; private set; }
        public Int32 WheelProductId { get; private set; }
        public Int32 BoostProductId { get; private set; }
        public Int32 AntennaProductId { get; private set; }
        public Int32 HatProductId { get; private set; }

        public Int32 Unknown2 { get; private set; } // Always 0. Future expansion room for a different product type?

        public static ClientLoadout Deserialize(BitReader br)
        {
            var cl = new ClientLoadout();

            cl.Unknown1 = br.ReadByte();
            cl.BodyProductId = br.ReadInt32();
            cl.SkinProductId = br.ReadInt32();
            cl.WheelProductId = br.ReadInt32();
            cl.BoostProductId = br.ReadInt32();
            cl.AntennaProductId = br.ReadInt32();
            cl.HatProductId = br.ReadInt32();
            cl.Unknown2 = br.ReadInt32();

			if (cl.Unknown1 > 10 )
			{
				br.ReadInt32();
			}

            return cl;
        }

		public override string ToString()
		{
			return string.Format("Unknown1 {0}, BodyProductId {1}, SkinProductId {2}, WheelProductId {3}, BoostProductId {4}, AntennaProductId {5}, HatProductId {6}, Unknown2 {7}", Unknown1, BodyProductId, SkinProductId, WheelProductId, BoostProductId, AntennaProductId, HatProductId, Unknown2);
		}
    }
}
