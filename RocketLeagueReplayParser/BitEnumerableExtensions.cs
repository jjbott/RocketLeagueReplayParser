using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public static class BitEnumerableExtensions
    {
        public static string ToBinaryString(this IEnumerable<bool> bits)
        {
            var sb = new StringBuilder();
            foreach(var bit in bits)
            {
                sb.Append(bit ? "1" : "0");
            }
            return sb.ToString();
        }
    }
}
