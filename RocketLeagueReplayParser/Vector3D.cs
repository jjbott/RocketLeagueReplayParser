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
        public Int32 Unknown { get; private set; }
        public Int32 DX { get; private set; }
        public Int32 DY { get; private set; }
        public Int32 DZ { get; private set; }

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

            // From ReadPackedVector

            v.NumBits = br.ReadInt32Max(maxBits-1);// br.ReadInt32FromBits(lengthBits);

            Int32 Bias = 1 << (v.NumBits + 1);
            Int32 Max = v.NumBits + 2;// 1 << (bits + 2);

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

        public static Vector3D DeserializeFixed(BitReader br)
        {
            var v = new Vector3D();

            v.X = ReadFixedCompressedFloat(1, 16, br);
            v.Y = ReadFixedCompressedFloat(1, 16, br);
            v.Z = ReadFixedCompressedFloat(1, 16, br);

            return v;
        }

        // This probably belongs in BitReader. This is the only calss that uses it though.
        static float ReadFixedCompressedFloat(Int32 maxValue, Int32 numBits, BitReader br)
        {
			float value = 0;
            									        // NumBits = 8:
	        var maxBitValue	= (1 << (numBits - 1)) - 1;	//   0111 1111 - Max abs value we will serialize
	        var bias        = (1 << (numBits - 1)) ;		//   1000 0000 - Bias to pivot around (in order to support signed values)
	        var serIntMax	= (1 << (numBits - 0));		// 1 0000 0000 - What we pass into SerializeInt
	        var maxDelta	= (1 << (numBits - 0)) - 1;	//   1111 1111 - Max delta is
	
	        Int32 delta = br.ReadInt32Max(serIntMax);
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

        // "Special" vectors dont exist!
        /*
        // Dont know the significance yet, but this reads vectors a bit differently from the normal way
        public static Vector3D DeserializeSpecial(BitReader br) 
        {
            var v = new Vector3D();

            v.NumBits = br.ReadInt32FromBits(5);
            v.Unknown = br.ReadInt32FromBits(5); // Unknown

            Int32 Bias = 1 << (v.NumBits - 1);
            Int32 Max = v.NumBits;
            
            v.DX = br.ReadInt32FromBits(Max);
            v.DY = br.ReadInt32FromBits(Max);
            v.DZ = br.ReadInt32FromBits(Max);

            // This is a best guess based on the normal deserialization
            v.X = v.DX - Bias;
            v.Y = v.DY - Bias;
            v.Z = v.DZ - Bias;

            return v;
        }
        */

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
