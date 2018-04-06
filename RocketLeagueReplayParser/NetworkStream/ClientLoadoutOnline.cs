using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class ClientLoadoutOnline
    {
        public List<List<ProductAttribute>> ProductAttributeLists { get; private set; }

        public static ClientLoadoutOnline Deserialize(BitReader br, UInt32 engineVersion, UInt32 licenseeVersion, string[] objectNames)
        {
            var clo = new ClientLoadoutOnline();
            clo.ProductAttributeLists = new List<List<ProductAttribute>>();
            
            var listCount = br.ReadByte();
            for (int i = 0; i < listCount; ++i)
            {
                var productAttributes = new List<ProductAttribute>();

                var productAttributeCount = br.ReadByte();
                for (int j = 0; j < productAttributeCount; ++j)
                {
                    productAttributes.Add(ProductAttribute.Deserialize(br, engineVersion, licenseeVersion, objectNames));
                }

                clo.ProductAttributeLists.Add(productAttributes);
            }
            return clo;
        }

        public void Serialize(BitWriter bw, UInt32 engineVersion, UInt32 licenseeVersion)
        {
            bw.Write((byte)ProductAttributeLists.Count);
            foreach (var productAttributes in ProductAttributeLists)
            {
                bw.Write((byte)productAttributes.Count);
                foreach(var productAttribute in productAttributes)
                {
                    productAttribute.Serialize(bw, engineVersion, licenseeVersion);
                }
            }
        }
    }
}
