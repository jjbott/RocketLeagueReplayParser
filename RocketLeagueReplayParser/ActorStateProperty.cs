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
        public string PropertyName { get; private set; }
        public List<object> Data { get; private set; }

        public bool IsComplete { get; private set; }

        public static ActorStateProperty Deserialize(ClassNetCache classMap, IDictionary<int, string> objectIndexToName, BitReader br)
        {
            var asp = new ActorStateProperty();

            var maxPropId = classMap.Id -  1;
            var idBitLen = Math.Floor(Math.Log10(maxPropId) / Math.Log10(2)) + 1;


            asp.PropertyId = br.ReadInt32FromBits((int)idBitLen);
            asp.PropertyName = objectIndexToName[classMap.AllProperties.Where(x => x.Id == asp.PropertyId).Single().Index];
            asp.Data = new List<object>();

            switch(asp.PropertyName)
            {
                case "Engine.GameReplicationInfo:GameClass":
                    asp.Data = ReadData(28, 6, br);
                    asp.IsComplete = true;
                    break;
                case "TAGame.GameEvent_TA:ReplicatedStateIndex":
                    asp.Data = ReadData(10, 4, br);
                    asp.IsComplete = true;
                    break;
                case "TAGame.RBActor_TA:ReplicatedRBState":
                    //asp.Data = ReadData(14, 1, br);

                    //KnownBits: 0011000000 001000000011 111101100000 000000000000 00000000000000000000000000000000 00100000 00001000 00000010101
                    asp.Data.Add(Vector3D.Deserialize(10, br));
                    asp.Data.Add(br.ReadInt32());
                    asp.Data.Add(br.ReadByte());
                    asp.Data.Add(br.ReadByte());
                    asp.Data.Add(Vector3D.Deserialize(5, br));
                    
                    asp.IsComplete = true;
                    break;
                case "TAGame.CarComponent_TA:Vehicle":
                case "TAGame.Team_TA:GameEvent":
                    asp.Data.Add(br.ReadBit()); // Maybe an "if 1 then read.."?
                    asp.Data.Add(br.ReadInt32());
                    asp.IsComplete = true;
                    break;
                case "Engine.PlayerReplicationInfo:PlayerName":
                    asp.Data.Add(br.ReadString());
                    asp.IsComplete = true;
                    break;
            }

            return asp;
        }

        private static List<object> ReadData(int numBytes, int numBits, BitReader br)
        {
            List<object> data = new List<object>();
            for (int i = 0; i < numBytes; ++i)
            {
                data.Add(br.ReadByte());
            }
            for (int i = 0; i < numBits; ++i)
            {
                data.Add(br.ReadBit());
            }
            return data;
        }

        public string ToDebugString()
        {
            var s = string.Format("Property: ID {0} Name {1}\r\n", PropertyId, PropertyName);
            s += "    Data: " + string.Join(", ", Data) + "\r\n";
            return s;

        }
    }
}
