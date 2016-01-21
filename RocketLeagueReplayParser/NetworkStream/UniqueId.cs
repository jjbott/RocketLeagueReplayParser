using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class UniqueId
    {
        public enum UniqueIdType { Unknown = 0, Steam = 1, PS4 = 2 }

        public UniqueIdType Type { get; private set; }
        public byte[] Id { get; private set; }
        public byte PlayerNumber { get; private set; } // Split screen player (0 when not split screen)

        public static UniqueId Deserialize(BitReader br)
        {
            List<object> data = new List<object>();
            var type = (UniqueIdType)br.ReadByte();

            UniqueId uid = new UniqueId();
            if ( type == UniqueIdType.Steam)
            {
                uid = new SteamId();
            }
            uid.Type = type;

            if (uid.Type == UniqueIdType.Steam)
            {
                uid.Id = br.ReadBytes(8);
            }
            else if (uid.Type == UniqueIdType.PS4)
            {
                uid.Id = br.ReadBytes(32); 
            }
            else if (uid.Type == UniqueIdType.Unknown)
            {
                uid.Id = br.ReadBytes(3); // Will be 0
            }
            else
            {
                throw new ArgumentException("Invalid type: " + ((int)uid.Type).ToString());
            }
            uid.PlayerNumber = br.ReadByte();
            return uid;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Type, BitConverter.ToString(Id).Replace("-", ""), PlayerNumber);
        }
    }

    // Do I really need a separate type for this? 
    public class SteamId : UniqueId
    {
        public Int64 SteamID64
        {
            get
            {
                if (Type != UniqueIdType.Steam)
                {
                    throw new InvalidDataException(string.Format("Invalid type {0}, cant extract steam id", Type));
                }

                return BitConverter.ToInt64(Id, 0);
            }
        }

        public string SteamProfileUrl
        {
            get
            {
                return string.Format("http://steamcommunity.com/profiles/{0}", SteamID64);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}), PlayerNumber {2}", SteamID64, SteamProfileUrl, PlayerNumber);
        }
    }
}
