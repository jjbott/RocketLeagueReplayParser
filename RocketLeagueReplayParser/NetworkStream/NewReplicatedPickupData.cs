using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class NewReplicatedPickupData
    {
        public bool Unknown1 { get; private set; }
        public Int32 ActorId { get; private set; }
        public byte Unknown2 { get; private set; }

        public static NewReplicatedPickupData Deserialize(BitReader br)
        {
            var rpd = new NewReplicatedPickupData();

            rpd.Unknown1 = br.ReadBit();
            rpd.ActorId = br.ReadInt32();
            rpd.Unknown2 = br.ReadByte();

            return rpd;
        }

        public virtual void Serialize(BitWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(ActorId);
            bw.Write(Unknown2);
        }

        public override string ToString()
        {
            return string.Format("Unknown1: {0}, ActorId: {1}, Unknown2: {2}", Unknown1, ActorId, Unknown2);
        }
    }
}
