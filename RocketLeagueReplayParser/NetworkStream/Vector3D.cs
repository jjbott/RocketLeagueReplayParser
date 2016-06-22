using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class Vector3D
    {
        private UInt32 NumBits { get; set; }
        private UInt32 DX { get; set; }
        private UInt32 DY { get; set; }
        private UInt32 DZ { get; set; }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public static Vector3D Deserialize(BitReader br)
        {
            return Deserialize(20, br);
        }

        private Vector3D() { }

        public Vector3D(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        private static Vector3D Deserialize(int maxBits, BitReader br)
        {
            var v = new Vector3D();

            // Simplified from ReadPackedVector

            v.NumBits = br.ReadUInt32Max(maxBits);

            Int32 Bias = 1 << (int)(v.NumBits + 1);
            Int32 Max = (int)v.NumBits + 2;

            v.DX = br.ReadUInt32FromBits(Max);
            v.DY = br.ReadUInt32FromBits(Max);
            v.DZ = br.ReadUInt32FromBits(Max);
	
            v.X = v.DX-Bias;
            v.Y = v.DY-Bias;
            v.Z = v.DZ-Bias;

	        return v;
        }

        public void Serialize(BitWriter bw)
        {

            // Do basically FVector::SerializeCompressed
            Int32 IntX = (int)Math.Round(X);
            Int32 IntY = (int)Math.Round(Y);
            Int32 IntZ = (int)Math.Round(Z);

            Int32 maxValue = Math.Max(Math.Max(Math.Abs(IntX), Math.Abs(IntY)), Math.Abs(IntZ));
            int numBitsForValue = (int)Math.Ceiling(Math.Log10(maxValue + 1) / Math.Log10(2));

            const int maxBitsPerComponent = 20;

            UInt32 Bits = (UInt32)Math.Min(Math.Max(1, numBitsForValue), maxBitsPerComponent) - 1;

            bw.Write(Bits, maxBitsPerComponent);
            //Ar.SerializeInt(Bits, MaxBitsPerComponent);

            Int32 Bias = 1 << (int)(Bits + 1);
            UInt32 Max = (UInt32)(1 << (int)(Bits + 2));
            UInt32 DX = (UInt32)(IntX + Bias);
            UInt32 DY = (UInt32)(IntY + Bias);
            UInt32 DZ = (UInt32)(IntZ + Bias);

            bool clamp = false;

            if (DX >= Max) { clamp = true; DX = unchecked((Int32)DX) > 0 ? Max - 1 : 0; }
            if (DY >= Max) { clamp = true; DY = unchecked((Int32)DY) > 0 ? Max - 1 : 0; }
            if (DZ >= Max) { clamp = true; DZ = unchecked((Int32)DZ) > 0 ? Max - 1 : 0; }

            bw.Write(DX, Max);
            bw.Write(DY, Max);
            bw.Write(DZ, Max);

            //Ar.SerializeInt(DX, Max);
            //Ar.SerializeInt(DY, Max);
            //Ar.SerializeInt(DZ, Max);

            //return !clamp;

        }

        public static Vector3D DeserializeFixed(BitReader br)
        {
            var v = new Vector3D();

            v.X = br.ReadFixedCompressedFloat(1, 16);
            v.Y = br.ReadFixedCompressedFloat(1, 16);
            v.Z = br.ReadFixedCompressedFloat(1, 16);

            return v;
        }
        static Random r = new Random();

        public void SerializeFixed(BitWriter bw)
        {
            
            float x = (float)r.NextDouble() * 2 - 1;
            float y = (float)r.NextDouble() * 2 - 1;
            float z = (float)r.NextDouble() * 2 - 1;

            bw.WriteFixedCompressedFloat(x, 1, 16);
            bw.WriteFixedCompressedFloat(y, 1, 16);
            bw.WriteFixedCompressedFloat(z, 1, 16);
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
