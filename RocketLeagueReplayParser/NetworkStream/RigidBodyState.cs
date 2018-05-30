using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class RigidBodyState
    {
        public bool Sleeping { get; private set; }
        public IVector3D Position { get; private set; }
        public bool Unknown1 { get; private set; }
        public Vector3D Rotation { get; private set; } // Pitch/Yaw/Roll
        public bool Unknown2 { get; private set; }
        public Vector3D LinearVelocity { get; private set; }
        public Vector3D AngularVelocity { get; private set; }

        public static RigidBodyState Deserialize(BitReader br, UInt32 netVersion)
        {
            var rbs = new RigidBodyState();
            rbs.Sleeping = br.ReadBit();
            if (netVersion >= 5)
            {
                rbs.Position = FixedPointVector3D.Deserialize(br, netVersion);
            }
            else
            {
                rbs.Position = Vector3D.Deserialize(br, netVersion);
            }

            if (netVersion >= 7)
            {
                rbs.Unknown1 = br.ReadBit();
            }

            rbs.Rotation = Vector3D.DeserializeFixed(br, netVersion);

            if (netVersion >= 7)
            {
                rbs.Unknown2 = br.ReadBit();
            }

            if (!rbs.Sleeping)
            {
                rbs.LinearVelocity = Vector3D.Deserialize(br, netVersion);
                rbs.AngularVelocity = Vector3D.Deserialize(br, netVersion);
            }

            return rbs;
        }

        public void  Serialize(BitWriter bw, UInt32 netVersion)
        {
            bw.Write(Sleeping);
            Position.Serialize(bw, netVersion);
            
            if (netVersion >= 7)
            {
                bw.Write(Unknown1);
            }

            Rotation.SerializeFixed(bw, netVersion);

            if (netVersion >= 7)
            {
                bw.Write(Unknown2);
            }

            if (!Sleeping)
            {
                LinearVelocity.Serialize(bw, netVersion);
                AngularVelocity.Serialize(bw, netVersion);
            }
        }

        public override string ToString()
        {
            if ( !Sleeping )
            {
                return String.Format("Position: {0} Rotation {1} LinearVelocity {2} AngularVelocity {3}", Position, Rotation, LinearVelocity, AngularVelocity);
            }
            return String.Format("Position: {0} Rotation {1}", Position, Rotation);
        }
    }
}
