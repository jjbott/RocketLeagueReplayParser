using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class UniqueId
    {
        public enum UniqueIdType { Nul = 0, Steam = 1, PS4 = 2 }

        public UniqueIdType Type { get; private set; }
        public byte[] Id { get; private set; }

        public static UniqueId Deserialize(BitReader br)
        {
            var uid = new UniqueId();

            List<object> data = new List<object>();
            uid.Type = (UniqueIdType)br.ReadByte();

            if (uid.Type == UniqueIdType.Steam)
            {
                uid.Id = br.ReadBytes(9);
            }
            else if (uid.Type == UniqueIdType.PS4)
            {
                uid.Id = br.ReadBytes(33); 
            }
            else if (uid.Type == UniqueIdType.Nul)
            {
                uid.Id = br.ReadBytes(4); // Will be 0
            }
            else
            {
                throw new ArgumentException("Invalid type: " + ((int)uid.Type).ToString());
            }
            return uid;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Type, BitConverter.ToString(Id).Replace("-", ""));
        }
    }
}
