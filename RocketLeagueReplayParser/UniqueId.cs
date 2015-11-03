using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class UniqueId
    {
        public enum UniqueIdType { Bot = 0, Steam = 1, PS4 = 2 }

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
            //else if (uid.Type == UniqueIdType.Bot) // Verify you really saw a zero here, and you're not just crazy
            //{
            //    uid.Id = br.ReadBytes(50); // read a ton of stuff just to see some data
            //}
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
