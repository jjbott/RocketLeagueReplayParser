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
        public Vector3D Position { get; private set; }
        public Vector3D Rotation { get; private set; } // Pitch/Yaw/Roll
        public Vector3D LinearVelocity { get; private set; }
        public Vector3D AngularVelocity { get; private set; }

        public static RigidBodyState Deserialize(BitReader br)
        {
            var rbs = new RigidBodyState();
            rbs.Sleeping = br.ReadBit();
            rbs.Position = Vector3D.Deserialize(br);
            rbs.Rotation = Vector3D.DeserializeFixed(br);

            if (!rbs.Sleeping)
            {
                rbs.LinearVelocity = Vector3D.Deserialize(br);
                rbs.AngularVelocity = Vector3D.Deserialize(br);
            }

            return rbs;
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
