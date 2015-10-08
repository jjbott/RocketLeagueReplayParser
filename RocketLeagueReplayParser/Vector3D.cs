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
        public Int32 NumBits { get; private set; }
        public Int32 DX { get; private set; }
        public Int32 DY { get; private set; }
        public Int32 DZ { get; private set; }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public static Vector3D Deserialize(BitReader br)
        {
            return Deserialize(5, br);
        }

        public static Vector3D Deserialize(int lengthBits, BitReader br)
        {
            var v = new Vector3D();

            // From ReadPackedVector

            v.NumBits = br.ReadInt32FromBits(lengthBits);

            Int32 Bias = 1 << (v.NumBits + 1);
            Int32 Max = v.NumBits + 2;// 1 << (bits + 2);

            if ( lengthBits == 10 )
            {
                Bias = 1 << (v.NumBits - 1);
                Max = v.NumBits;
            }
            v.DX = br.ReadInt32FromBits(Max);
            v.DY = br.ReadInt32FromBits(Max);
            v.DZ = br.ReadInt32FromBits(Max);
	
	        //float fact = 1; //(float)ScaleFactor; // 1 in our case, doesnt matter

	        //v.X = (float)(static_cast<int32>(DX)-Bias) / fact; // Why bother with the static_cast? Why not make DX an int32 instead of uint32 in the first place? 
            // always integers, hey? 
            v.X = v.DX-Bias;
            v.Y = v.DY-Bias;
            v.Z = v.DZ-Bias;

	        return v;
        }
        
        public string ToDebugString()
        {
            return string.Format("Vector: X {0} Y {1} Z {2} ({3} {4} {5} {6})", X, Y, Z, NumBits, DX, DY, DZ); 
        }
    }
}
