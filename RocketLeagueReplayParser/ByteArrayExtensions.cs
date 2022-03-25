using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public static class BinaryReaderExtensions
    {
		// TODO: Better name would be cool. Dont use ReadString, it wont be called then (BinaryReader has its own ReadString method)
		public static string ReadString2(this BinaryReader br)
        {
            // TODO: This is the same implementation as BitReader's ReadString. Try to consolidate someday
            var length = br.ReadInt32();
            if (length > 0)
            {
                var bytes = br.ReadBytes(length);

#if !NET462
                return CodePagesEncodingProvider.Instance.GetEncoding(1252).GetString(bytes, 0, length - 1);
#else
                return Encoding.ASCII.GetString(bytes, 0, length - 1);
#endif
            }
            else if (length < 0)
            {
                var bytes = br.ReadBytes(length * -2);
                return Encoding.Unicode.GetString(bytes, 0, (length * -2) - 2);
            }

            return "";
        }
    }
}
