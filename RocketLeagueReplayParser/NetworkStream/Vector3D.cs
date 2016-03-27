using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class Vector3D
    {
        private Int32 NumBits { get; set; }
        private Int32 DX { get; set; }
        private Int32 DY { get; set; }
        private Int32 DZ { get; set; }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public static Vector3D Deserialize(BitReader br)
        {
            return Deserialize2(20, br);
        }

        public static Vector3D Deserialize2(int maxBits, BitReader br)
        {
            var v = new Vector3D();

            // Simplified from ReadPackedVector

            v.NumBits = br.ReadInt32Max(maxBits);

            Int32 Bias = 1 << (v.NumBits + 1);
            Int32 Max = v.NumBits + 2;

            v.DX = br.ReadUInt32FromBits(Max);
            v.DY = br.ReadUInt32FromBits(Max);
            v.DZ = br.ReadUInt32FromBits(Max);
	
            v.X = v.DX-Bias;
            v.Y = v.DY-Bias;
            v.Z = v.DZ-Bias;

	        return v;
        }

        public static Vector3D DeserializeFixed(BitReader br)
        {
            var v = new Vector3D();

            v.X = ReadFixedCompressedFloat(1, 16, br);
            v.Y = ReadFixedCompressedFloat(1, 16, br);
            v.Z = ReadFixedCompressedFloat(1, 16, br);

            return v;
        }

        // This probably belongs in BitReader. This is the only class that uses it though.
        static float ReadFixedCompressedFloat(Int32 maxValue, Int32 numBits, BitReader br)
        {
			float value = 0;
            									        // NumBits = 8:
	        var maxBitValue	= (1 << (numBits - 1)) - 1;	//   0111 1111 - Max abs value we will serialize
	        var bias        = (1 << (numBits - 1)) ;    //   1000 0000 - Bias to pivot around (in order to support signed values)
	        var serIntMax	= (1 << (numBits - 0)) ;	// 1 0000 0000 - What we pass into SerializeInt
	        var maxDelta	= (1 << (numBits - 0)) - 1;	//   1111 1111 - Max delta is

            Int32 delta = br.ReadInt32Max(serIntMax); // Could just read 16 bits always, since numBits will always be 16 
	        float unscaledValue = delta - bias;

	        if ( maxValue > maxBitValue )
	        {
		        // We have to scale down, scale needs to be a float:
		        float invScale = maxValue / (float)maxBitValue;
		        value = unscaledValue * invScale;
	        }
	        else
	        {
		        var scale = maxBitValue / maxValue;
		        float invScale = 1.0f / (float)scale;

		        value = unscaledValue * invScale;
	        }

            return value;
        }

        public override string ToString()
        {
            return string.Format("(X:{0}, Y:{1}, Z:{2})", X, Y, Z); 
        }
        
        public string ToDebugString()
        {
            return string.Format("Vector: X {0} Y {1} Z {2} ({3} {4} {5} {6})", X, Y, Z, NumBits, DX, DY, DZ); 
        }
    }
}
