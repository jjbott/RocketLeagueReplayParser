using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    // There is a vector class in PresentationCore we can use. Eh.

    public class Vector3D
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public static Vector3D Deserialize(BitReader br)
        {
            var v = new Vector3D();

            // From ReadPackedVector

            Int32 bits = br.ReadInt32(true);

	        Int32  Bias = 1<<(bits+1);
	        //Int32 Max	= 1<<(bits+2); // Not used. Nice work.
            Int32 DX = br.ReadInt32(true);
            Int32 DY = br.ReadInt32(true);
            Int32 DZ = br.ReadInt32(true);
	
	        //float fact = 1; //(float)ScaleFactor; // 1 in our case, doesnt matter

	        //v.X = (float)(static_cast<int32>(DX)-Bias) / fact; // Why bother with the static_cast? Why not make DX an int32 instead of uint32 in the first place?
            // always integers, hey? 
            v.X = DX-Bias;
            v.Y = DY-Bias;
            v.Z = DZ-Bias;

	        return v;
        }
        
        public string ToDebugString()
        {
            return string.Format("Vector: X {0} Y {1} Z {2}", X, Y, Z); 
        }
    }
}
