using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class ActorStateProperty
    {
        public Int32 PropertyId { get; private set; }

        public bool IsComplete { get; private set; }

        public static ActorStateProperty Deserialize(BitReader br)
        {
            var asp = new ActorStateProperty();
            asp.PropertyId = br.ReadInt32FromBits(6); // might be 7 sometimes
            return asp;
        }

        public string ToDebugString()
        {
            return string.Format("Property: ID {0}", PropertyId);
        }
    }
}
