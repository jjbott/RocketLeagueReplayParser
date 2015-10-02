using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class ActorState
    {
        public int Id { get; private set; }
        public string State { get; private set; } // TODO: Enumify someday
        public Int32? TypeId { get; private set; }
        public static ActorState Deserialize(BitReader br)
        {
            var a = new ActorState();

            a.Id = br.ReadInt32FromBits(10);
            if ( br.ReadBit() )
            {
                if ( br.ReadBit() )
                {
                    a.State = "New";
                    a.TypeId = br.ReadVarInt32(); 
                }
                else
                {
                    a.State = "Existing";
                }
            }
            else
            {
                a.State = "Deleted";
            }

            return a;

        }

        public string ToDebugString()
        {
            return string.Format("ActorState: Id {0} State {1} TypeId {2}", Id, State, TypeId == null ? "NULL" : TypeId.ToString());
        }
    }
}
