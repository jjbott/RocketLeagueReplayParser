using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public static class StringExtension
    {
        // This mirrors BinaryReader.ReadString2 and BitReader.ReadString
        public static IEnumerable<byte> Serialize(this string s)
        {
            var result = new List<byte>();

            int length = s.Length;
            bool isUnicode = s.Any(c => c > 255);
            if (isUnicode)
            {
                length *= -1;
            }

            if (s.Length > 0)
            {
                // add space for the trailing 0 to length
                if ( isUnicode )
                {
                    length -= 1;
                }
                else
                {
                    length += 1;
                }
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
#if !NET462
                    result.AddRange(CodePagesEncodingProvider.Instance.GetEncoding(1252).GetBytes(s));
#else
                    result.AddRange(Encoding.GetEncoding(1252).GetBytes(s));
#endif
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
