using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class RigidBodyState
    {
        public bool UnknownDataMissing { get; private set; } // Rename once I figure out the unknown data
        public Vector3D Position { get; private set; }
        public Vector3D Rotation { get; private set; }
        public Vector3D UnknownVector1 { get; private set; }
        public Vector3D UnknownVector2 { get; private set; }

        public static RigidBodyState Deserialize(BitReader br)
        {
            var rbs = new RigidBodyState();
            rbs.UnknownDataMissing = br.ReadBit();
            rbs.Position = Vector3D.Deserialize(br);
            rbs.Rotation = Vector3D.DeserializeFixed(br);

            if (!rbs.UnknownDataMissing)
            {
                rbs.UnknownVector1 = Vector3D.Deserialize(br);
                rbs.UnknownVector2 = Vector3D.Deserialize(br);
            }

            return rbs;
        }

        public override string ToString()
        {
            if ( !UnknownDataMissing )
            {
                return String.Format("Position: {0} Rotation {1} UnknownVector1 {2} UnknownVector1 {3}", Position, Rotation, UnknownVector1, UnknownVector2);
            }
            return String.Format("Position: {0} Rotation {1}", Position, Rotation);
        }
    }
}
