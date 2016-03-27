using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class Reservation
    {
        public Int32 Unknown3Bits { get; private set; }
        public UniqueId PlayerId { get; private set; }
        public string PlayerName { get; private set; }
        public bool UnknownBit1 { get; private set; }
        public bool UnknownBit2 { get; private set; }

        public static Reservation Deserialize(BitReader br)
        {
            var r = new Reservation();

            r.Unknown3Bits = br.ReadUInt32FromBits(3);

            r.PlayerId = UniqueId.Deserialize(br);

            if (r.PlayerId.Type != UniqueId.UniqueIdType.Unknown)
            {
                r.PlayerName = br.ReadString();
            }
            r.UnknownBit1 = br.ReadBit();
            r.UnknownBit2 = br.ReadBit();

            /*
                ReservationStatus_None,
    ReservationStatus_Reserved,
    ReservationStatus_Joining,
    ReservationStatus_InGame,
    ReservationStatus_MAX
             */

            return r;
        }

        public override string ToString()
        {
            return string.Format("Unk1: {0} ID: {1} Name: {2} Unk2: {3} Unk3: {4}", Unknown3Bits, PlayerId, PlayerName, UnknownBit1, UnknownBit2);
        }
    }
}
