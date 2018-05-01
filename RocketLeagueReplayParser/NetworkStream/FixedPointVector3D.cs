using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    // Not sure about this name...
    public class FixedPointVector3D : IVector3D
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        private Vector3D _integerVector;

        public static FixedPointVector3D Deserialize(BitReader br)
        {
            var vector = new FixedPointVector3D();
            vector._integerVector = Vector3D.Deserialize(br);
            // Could make this generic for any number of decimal places, but this is all I need for now
            vector.X = vector._integerVector.X / 100;
            vector.Y = vector._integerVector.Y / 100;
            vector.Z = vector._integerVector.Z / 100;

            return vector;
        }

        public void Serialize(BitWriter bw)
        {
            _integerVector.Serialize(bw);
        }
    }
}
