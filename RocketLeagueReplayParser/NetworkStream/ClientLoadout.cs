using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ClientLoadout
    {
        public byte Version { get; private set; } // Always 10, except when it's 11

        // Product Ids are in TAGame.upk in the ProductsDB content
        
        public UInt32 BodyProductId { get; private set; }
        public UInt32 SkinProductId { get; private set; }
        public UInt32 WheelProductId { get; private set; }
        public UInt32 BoostProductId { get; private set; }
        public UInt32 AntennaProductId { get; private set; }
        public UInt32 HatProductId { get; private set; }

        public UInt32 Unknown2 { get; private set; } // Always 0. Future expansion room for a different product type?

        public UInt32 Unknown3 { get; private set; }

        public UInt32 EngineAudioProductId { get; private set; }
        public UInt32 TrailProductId { get; private set; }
        public UInt32 GoalExplosionProductId { get; private set; }

        public UInt32 BannerProductId { get; private set; } // I didn't check if this is actually the banner id, but it's the only new customization I know of.

        public UInt32 Unknown4 { get; private set; }
        public UInt32 Unknown5 { get; private set; }
        public UInt32 Unknown6 { get; private set; }
        public UInt32 Unknown7 { get; private set; }

        public static ClientLoadout Deserialize(BitReader br)
        {
            var cl = new ClientLoadout();

            cl.Version = br.ReadByte();
            cl.BodyProductId = br.ReadUInt32();
            cl.SkinProductId = br.ReadUInt32();
            cl.WheelProductId = br.ReadUInt32();
            cl.BoostProductId = br.ReadUInt32();
            cl.AntennaProductId = br.ReadUInt32();
            cl.HatProductId = br.ReadUInt32();
            cl.Unknown2 = br.ReadUInt32();

			if (cl.Version > 10 )
			{
				cl.Unknown3 = br.ReadUInt32();
			}

            if (cl.Version >= 16)
            {
                cl.EngineAudioProductId = br.ReadUInt32();
                cl.TrailProductId = br.ReadUInt32();
                cl.GoalExplosionProductId = br.ReadUInt32();
            }

            if (cl.Version >= 17)
            {
                cl.BannerProductId = br.ReadUInt32();
            }

            if (cl.Version >= 19)
            {
                cl.Unknown4 = br.ReadUInt32();
            }

            if (cl.Version >= 22)
            {
                cl.Unknown5 = br.ReadUInt32();
                cl.Unknown6 = br.ReadUInt32();
                cl.Unknown7 = br.ReadUInt32();
            }

            return cl;
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write(Version);
            bw.Write(BodyProductId);
            bw.Write(SkinProductId);
            bw.Write(WheelProductId);
            bw.Write(BoostProductId);
            bw.Write(AntennaProductId);
            bw.Write(HatProductId);
            bw.Write(Unknown2);

            if (Version > 10)
            {
                bw.Write(Unknown3);
            }

            if (Version >= 16)
            {
                bw.Write(EngineAudioProductId);
                bw.Write(TrailProductId);
                bw.Write(GoalExplosionProductId);
            }

            if (Version >= 17)
            {
                bw.Write(BannerProductId);
            }

            if (Version >= 19)
            {
                bw.Write(Unknown4);
            }

            if (Version >= 22)
            {
                bw.Write(Unknown5);
                bw.Write(Unknown6);
                bw.Write(Unknown7);
            }
        }

		public override string ToString()
		{
			return string.Format("Version {0}, BodyProductId {1}, SkinProductId {2}, WheelProductId {3}, BoostProductId {4}, AntennaProductId {5}, HatProductId {6}, Unknown2 {7}", Version, BodyProductId, SkinProductId, WheelProductId, BoostProductId, AntennaProductId, HatProductId, Unknown2);
		}
    }
}
