using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class Reservation
    {
        public List<object> Data { get; private set; } // TODO: Break this out better once it's analyzed
        IEnumerable<bool> _bits; /// Reanalyze this stuff someday. The loop is weird. But, it works!

        public static Reservation Deserialize(BitReader br)
        {
            var r = new Reservation();
            r.Data = new List<object>();

            var start = br.Position;
            

            r.Data.Add(br.ReadInt32FromBits(3));

            var id = UniqueId.Deserialize(br);
            r.Data.Add(id);

            if (id.Type != UniqueId.UniqueIdType.Nul)
            {
                r.Data.Add(br.ReadString());
            }
            r.Data.Add(br.ReadBit());
            r.Data.Add(br.ReadBit());

            r._bits = br.GetBits(start, br.Position - start);

            return r;
        }

        public override string ToString()
        {
            // this is going to indent funny. ah well.
            return "Bits: " + _bits.ToBinaryString() + "\r\n" 
                + string.Join("\r\n", Data); 
        }
    }
}
