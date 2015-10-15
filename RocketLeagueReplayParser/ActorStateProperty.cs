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

                    //Knownbits: "0 0110 00000001 00000001 10111011 000000000000000000000000000000000000000000000000 0110 00000001 00000001 00001111 00000 010101
                    asp.Data.Add(br.ReadBit());
                    asp.Data.Add(Vector3D.Deserialize(4, br));
                    var n = br.ReadInt16();
                    asp.Data.Add(n);
                    asp.Data.Add(br.ReadInt16());
                    asp.Data.Add(br.ReadInt16());
                    asp.Data.Add(Vector3D.Deserialize(4, br));
                    if (n == 0)
                    {
                        asp.Data.Add(Vector3D.Deserialize(5, br));
                    }
                    else
                    {
                        asp.Data.Add(Vector3D.Deserialize(4, br));
                    }
                    
                    asp.IsComplete = true;
                    break;
                case "TAGame.CarComponent_TA:Vehicle":
                case "TAGame.Team_TA:GameEvent":
                case "TAGame.CrowdActor_TA:ReplicatedOneShotSound":
                    asp.Data.Add(br.ReadBit()); // Maybe an "if 1 then read.."? Not for OneShotSound anyways...
                    asp.Data.Add(br.ReadInt32());
                    asp.IsComplete = true;
                    break;
                case "Engine.PlayerReplicationInfo:PlayerName":
                    asp.Data.Add(br.ReadString());
                    asp.IsComplete = true;
                    break;
                case "TAGame.GameEvent_Soccar_TA:SecondsRemaining":
                case "TAGame.GameEvent_TA:ReplicatedGameStateTimeRemaining":
                case "TAGame.CrowdActor_TA:ReplicatedCountDownNumber":
                case "TAGame.CrowdActor_TA:ModifiedNoise":
                    asp.Data.Add(br.ReadInt32());
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
            //return string.Join(", ", Data);

        }
    }
}
