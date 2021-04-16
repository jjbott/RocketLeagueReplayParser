using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class PickupInfo
    {
        // This breakdown of the bits is a wild guess, obviously
        public bool Unknown1;
        public bool Unknown2;
        public UInt32 Unknown3; // ActorId?
        public Int32 Unknown4;
        public Int32 Unknown5;
        public bool Unknown6;
        public bool Unknown7;

        public static PickupInfo Deserialize(BitReader br)
        {
            return new PickupInfo
            {
                Unknown1 = br.ReadBit(),
                Unknown2 = br.ReadBit(),
                Unknown3 = br.ReadUInt32(),
                Unknown4 = br.ReadInt32(),
                Unknown5 = br.ReadInt32(),
                Unknown6 = br.ReadBit(),
                Unknown7 = br.ReadBit()
            };
        }

        public void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(Unknown3);
            bw.Write(Unknown4);
            bw.Write(Unknown5);
            bw.Write(Unknown6);
            bw.Write(Unknown7);
        }

    }
}
