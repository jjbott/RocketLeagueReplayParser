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

        // I dont like this as an object, but it'll do for now.
        // Can be Vector3D or Quaternion
        public object Rotation { get; private set; }
        
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
                rbs.Rotation = Quaternion.Deserialize(br);
            }
            else
            {
                rbs.Rotation = Vector3D.DeserializeFixed(br);
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
                ((Quaternion)Rotation).Serialize(bw);
            }
            else
            {
                ((Vector3D)Rotation).SerializeFixed(bw);
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
