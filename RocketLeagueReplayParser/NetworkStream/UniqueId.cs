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
        public enum UniqueIdType { Unknown = 0, Steam = 1, PS4 = 2, PS3 = 3, Xbox = 4, Switch = 6, Psynet = 7, Epic = 11 }

        public UniqueIdType Type { get; protected set; }
        public byte[] Id { get; private set; }
        public byte PlayerNumber { get; private set; } // Split screen player (0 when not split screen)

        public static UniqueId Deserialize(BitReader br, UInt32 licenseeVersion, UInt32 netVersion)
        {
            List<object> data = new List<object>();
            var type = (UniqueIdType)br.ReadByte();

            UniqueId uid = new UniqueId();
            if (type == UniqueIdType.Steam)
            {
                uid = new SteamId();
            }
            else if (type == UniqueIdType.PS4)
            {
                uid = new Ps4Id();
            }
            uid.Type = type;

            DeserializeId(br, uid, licenseeVersion, netVersion);
            return uid;
        }

        protected static void DeserializeId(BitReader br, UniqueId uid, UInt32 licenseeVersion, UInt32 netVersion)
        {
            if (uid.Type == UniqueIdType.Steam)
            {
                uid.Id = br.ReadBytes(8);
            }
            else if (uid.Type == UniqueIdType.PS4)
            {
                if (netVersion >= 1)
                {
                    uid.Id = br.ReadBytes(40);
                }
                else
                {
                    uid.Id = br.ReadBytes(32);
                }
            }
            else if (uid.Type == UniqueIdType.Unknown)
            {
                if (licenseeVersion >= 18 && netVersion == 0)
                {
                    return;
                }
                else
                {
                    uid.Id = br.ReadBytes(3); // Will be 0
                    if (uid.Id.Sum(x => x) != 0 && (licenseeVersion < 18 || netVersion > 0))
                    {
                        throw new Exception("Unknown id isn't 0, might be lost");
                    }
                }
            }
            else if (uid.Type == UniqueIdType.Xbox)
            {
                uid.Id = br.ReadBytes(8);
            }
            else if (uid.Type == UniqueIdType.Switch)
            {
                uid.Id = br.ReadBytes(32);
            }
            else if (uid.Type == UniqueIdType.Psynet)
            {
                if ( netVersion >= 10 )
                {
                    uid.Id = br.ReadBytes(8);
                }
                else
                {
                    uid.Id = br.ReadBytes(32);
                }
            }
            else if (uid.Type == UniqueIdType.Epic)
            {
                // This is really a "GetString", but keeping everything as bytes.
                var id = br.ReadBytes(4);
                var len = (id[3] << 24) + (id[2] << 16) + (id[1] << 8) + id[0];
#if DEBUG
                // Not yet sure if this is always true
                if (len != 33) { throw new Exception("Unknown2 ID length != 33"); }
#endif
                uid.Id = id.Concat(br.ReadBytes(len)).ToArray();
            }
            else
            {
                throw new ArgumentException(string.Format("Invalid type: {0}. Next bits are {1}", ((int)uid.Type).ToString(), br.GetBits(br.Position, Math.Min(4096, br.Length - br.Position)).ToBinaryString()));
            }
            uid.PlayerNumber = br.ReadByte();
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write((byte)Type);
            SerializeId(bw);
        }

        protected void SerializeId(BitWriter bw)
        {
            if (Id != null)
            {
                bw.Write(Id);
                bw.Write(PlayerNumber);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Type, BitConverter.ToString(Id ?? new byte[0]).Replace("-", ""), PlayerNumber);
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

    public class Ps4Id : UniqueId
    {
        public string PsnName
        {
            get
            {
                return ASCIIEncoding.ASCII.GetString(Id, 0, 16).Replace("\0", "");
            }
        }

        public string Unknown1
        {
            // "unknown stuff used internally by ps4 api" - Psyonix_Cone
            get
            {
                return ASCIIEncoding.ASCII.GetString(Id, 16, 8).Replace("\0", "");
            }
        }

        public UInt64 Unknown2
        {
            // "more unknown stuff" - Psyonix_Cone
            get
            {
                return BitConverter.ToUInt64(Id, 24);
            }
        }

        public UInt64? PsnId
        {
            get
            {
                if (Id.Length > 32)
                {
                    return BitConverter.ToUInt64(Id, 32);
                }
                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}), PlayerNumber {2}", PsnName, PsnId?.ToString("X") ?? "", PlayerNumber);
        }
    }
}
