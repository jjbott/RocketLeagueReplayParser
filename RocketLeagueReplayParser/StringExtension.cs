using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public static class StringExtension
    {
        public static string Deserialize(BitReader br)
        {
            var length = br.ReadInt32();
            if (length > 0)
            {
                var bytes = br.ReadBytes(length);
                return Encoding.GetEncoding(1252).GetString(bytes, 0, length - 1);
            }
            else if (length < 0)
            {
                var bytes = br.ReadBytes(length * -2);
                return Encoding.Unicode.GetString(bytes, 0, (length * -2) - 2);
            }

            return "";
        }
    

        // This mirrors BinaryReader.ReadString2 and BitReader.ReadString
        public static IEnumerable<byte> Serialize(this string s)
        {
            var result = new List<byte>();

            int length = s.Length + 1; // +1 for trailing 0
            bool isUnicode = s.Any(c => c > 255);
            if (isUnicode)
            {
                length *= -1;
            }

            result.AddRange(BitConverter.GetBytes(length));

            if ( s.Length > 0 )
            {

                if (isUnicode)
                {
                    result.AddRange(Encoding.Unicode.GetBytes(s));

                    result.Add(0);// Trailing 0 (16 bits for unicode)
                    result.Add(0);
                }
                else
                {
                    result.AddRange(Encoding.GetEncoding(1252).GetBytes(s));
                    result.Add(0); // Trailing 0
                }
            }

            return result;
        }


        public static void Serialize(this string s, BitWriter bw)
        {
            bw.Write(s.Serialize());
        }
    }
}
