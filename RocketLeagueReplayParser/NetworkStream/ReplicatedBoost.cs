using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueReplayParser.NetworkStream
{
    // Blatantly swiped from https://github.com/Bakkes/CPPRP/blob/cb9f7c1d7524e52963e491b38b042ac115c8e5fd/CPPRP/data/NetworkData.h#L335
    // I probably would have called it an Int32 and moved on. :)
    public class ReplicatedBoost
    {
        public byte GrantCount { get; private set; }
        public byte BoostAmount { get; private set; }
        public byte Unused1 { get; private set; }
        public byte Unused2 { get; private set; }

        public static ReplicatedBoost Deserialize(BitReader br)
        {
            var rb = new ReplicatedBoost();

            rb.GrantCount = br.ReadByte();
            rb.BoostAmount = br.ReadByte();
            rb.Unused1 = br.ReadByte();
            rb.Unused2 = br.ReadByte();

            return rb;
        }

        public virtual void Serialize(BitWriter bw)
        {
            bw.Write(GrantCount);
            bw.Write(BoostAmount);
            bw.Write(Unused1);
            bw.Write(Unused2);
        }
    }
}
