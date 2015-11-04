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
            var len = br.ReadInt32();
			if (len > 0)
			{
				var bytes = br.ReadBytes(len);
				return Encoding.ASCII.GetString(bytes, 0, len - 1);
			}
			else if ( len < 0 )
			{
				var bytes = br.ReadBytes(len * -2);
				return Encoding.Unicode.GetString(bytes, 0, (len * -2) - 2);
			}
            return "";
        }
    }
}
