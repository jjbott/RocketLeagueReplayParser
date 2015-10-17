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

            var maxPropId = classMap.AllProperties.Max(x => x.Id);
            //var idBitLen = Math.Floor(Math.Log10(maxPropId) / Math.Log10(2)) + 1;


            asp.PropertyId = br.ReadInt32Max(maxPropId);// br.ReadInt32FromBits((int)idBitLen);
            asp.PropertyName = objectIndexToName[classMap.AllProperties.Where(x => x.Id == asp.PropertyId).Single().Index];
            asp.Data = new List<object>();
            try
            {
                switch (asp.PropertyName)
                {
                    case "Engine.GameReplicationInfo:GameClass":
                        asp.Data = ReadData(28, 6, br);
                        asp.IsComplete = true;
                        break;
                    case "TAGame.GameEvent_TA:ReplicatedStateIndex":
                        asp.Data.Add(br.ReadInt32FromBits(7));
                        asp.IsComplete = true;
                        break;
                    case "TAGame.RBActor_TA:ReplicatedRBState":
                        //asp.Data = ReadData(14, 1, br);

                        //010011 0 0001 000101000111 100000000010 100001001001 1001111111100011 0100000010110111 1111111111101110 1111 10111111001110101 01000111000101000 00001001000000100 000101001100
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(Vector3D.Deserialize2(20, br));
                        var n = br.ReadInt16();
                        asp.Data.Add(n);
                        asp.Data.Add(br.ReadInt16());
                        asp.Data.Add(br.ReadInt16());
                        // Sometimes these two vectors are missing (only on car, missing on ball?)
                        asp.Data.Add(Vector3D.Deserialize2(20, br));
                        asp.Data.Add(Vector3D.Deserialize2(20, br));


                        asp.IsComplete = true;
                        break;
                    case "TAGame.Team_TA:GameEvent":
                    case "TAGame.CrowdActor_TA:ReplicatedOneShotSound":
                    case "Engine.Actor:Owner":
                        asp.Data.Add(br.ReadBit()); // Maybe an "if 1 then read.."? Not for OneShotSound anyways...
                        asp.Data.Add(br.ReadInt32());
                        asp.IsComplete = true;
                        break;
                    case "TAGame.CarComponent_TA:Vehicle":
                        asp.Data.Add(br.ReadBit()); // Maybe an "if 1 then read.."? Not for OneShotSound anyways...
                        asp.Data.Add(br.ReadByte());
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
                    case "TAGame.VehiclePickup_TA:ReplicatedPickupData":
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadInt32());
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "Engine.Actor:bNetOwner":
                    case "Engine.Actor:bBlockActors":
                        // this doesnt look right...
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "Engine.Pawn:DrivenVehicle":
                        asp.Data.Add(br.ReadInt32());
                        asp.Data.Add(br.ReadBit());
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "Engine.PlayerReplicationInfo:Ping":
                    case "Engine.Actor:DrawScale":

                    case "TAGame.Vehicle_TA:ReplicatedSteer":
                    case "TAGame.Vehicle_TA:ReplicatedThrottle":
                    case "TAGame.PRI_TA:CameraYaw":
                    case "TAGame.PRI_TA:CameraPitch":
                        asp.Data.Add(br.ReadByte());
                        asp.IsComplete = true;
                        break;
                    case "Engine.Actor:Location":
                        asp.Data.Add(Vector3D.Deserialize2(20, br));
                        asp.IsComplete = true;
                        break;
                    case "Engine.Actor:RelativeRotation":
                        //asp.Data.Add(Vector3D.Deserialize(5, br));
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "TAGame.PRI_TA:bUsingBehindView":
                    case "TAGame.PRI_TA:bUsingSecondaryCamera":
                        asp.Data.Add(br.ReadBit());
                        asp.IsComplete = true;
                        break;
                    case "TAGame.CarComponent_TA:ReplicatedActive":
                        asp.Data.Add(br.ReadBit());
                        if ( (bool)asp.Data[0])
                        {
                            asp.Data.Add(br.ReadInt32FromBits(7));
                        }
                        asp.IsComplete = true;
                        break;

                }
            }
            catch(Exception)
            {
                asp.Data.Add("FAILED");
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
