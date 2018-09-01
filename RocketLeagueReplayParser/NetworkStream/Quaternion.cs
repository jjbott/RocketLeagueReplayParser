using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    // https://github.com/jjbott/RocketLeagueReplayParser/issues/30#issuecomment-410515375
    public class Quaternion
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float W { get; private set; }

        private const int NUM_BITS = 18;
        private const float MAX_QUAT_VALUE = 0.7071067811865475244f; // 1/sqrt(2)
        private const float INV_MAX_QUAT_VALUE = 1.0f / MAX_QUAT_VALUE;

        private enum Component
        {
            X,
            Y,
            Z,
            W
        }

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        

        private static float UncompressComponent(UInt32 iValue)
        {
            const int MaxValue = (1 << NUM_BITS) - 1;
            float positiveRangedValue = iValue / (float)MaxValue;
            float rangedValue = (positiveRangedValue - 0.50f) * 2.0f;
            return rangedValue * MAX_QUAT_VALUE;
        }

        private static UInt32 CompressComponent(float value)
        {
            const int MaxValue = (1 << NUM_BITS) - 1;
            float rangedValue = value / MAX_QUAT_VALUE;
            float positiveRangedValue = (rangedValue / 2f) + .5f;
            return (UInt32)Math.Round(MaxValue * positiveRangedValue);
        }

        public static Quaternion Deserialize(BitReader br)
        {
            Component largestComponent = (Component)br.ReadInt32FromBits(2);
            
            float a = UncompressComponent(br.ReadUInt32FromBits(NUM_BITS));
            float b = UncompressComponent(br.ReadUInt32FromBits(NUM_BITS));
            float c = UncompressComponent(br.ReadUInt32FromBits(NUM_BITS));
            float missing = (float)Math.Sqrt(1.0f - (a * a) - (b * b) - (c * c));

            switch (largestComponent)
            {
                case Component.X:
                    return new Quaternion(missing, a, b, c);
                case Component.Y:
                    return new Quaternion(a, missing, b, c);
                case Component.Z:
                    return new Quaternion(a, b, missing, c);
                case Component.W:
                default:
                    return new Quaternion(a, b, c, missing);
            }
        }

        public void Serialize(BitWriter bw)
        {
            var components = new List<float> { X, Y, Z, W };
            var largestComponentValue = components.Max();

            // This weirdness is only to help with round trip tests. Without it, there would be minor changes to rotation values.
            // There are cases where we want to serialize the largest value, and leave another value as the "missing" value.
            //
            // For example, we may have components with values like 0.707106769 (A) and 0.707106054 (B)
            // If we treat A as the largest (since it is), we'll find that the values have swapped when deserializing
            // The issue is that they both serialize to 262143, and 262143 deserializes to 0.707106769.
            // If we serialize B, we'll read it as 0.707106769 due to precision lost in compression. Calculating A will give us 0.707106054.
            //
            // That probably doesn't make an ounce of sense.
            // Basically, since we've calculated one component, that one component may lose a lot of precision in compression.
            // All other components came from compressed values, so they wont lose any precision (assuming we're using data straight from RL, unedited)
            // So we know that the component that round trips to a different value is the one we originally calculated.
            // Use that one as the largest, as long as it's close to the actual largest.
            //
            // That probably still doesnt make sense. Here be dragons?            
            var calculatedComponent = components.Cast<float?>().FirstOrDefault(c => c != UncompressComponent(CompressComponent(c.Value))) ?? largestComponentValue;
            if ( largestComponentValue != calculatedComponent && Math.Abs(largestComponentValue - calculatedComponent) < 0.00001)
            {
                largestComponentValue = calculatedComponent;
            }

            if ( largestComponentValue == X )
            {
                Write(bw, Component.X, Y, Z, W);
            }
            else if(largestComponentValue == Y)
            {
                Write(bw, Component.Y, X, Z, W);
            }
            else if (largestComponentValue == Z)
            {
                Write(bw, Component.Z, X, Y, W);
            }
            else
            {
                Write(bw, Component.W, X, Y, Z);
            }
        }

        private void Write(BitWriter bw, Component largest, float a, float b, float c)
        {
            bw.WriteFixedBitCount((UInt32)largest, 2);
            bw.WriteFixedBitCount(CompressComponent(a), NUM_BITS);
            bw.WriteFixedBitCount(CompressComponent(b), NUM_BITS);
            bw.WriteFixedBitCount(CompressComponent(c), NUM_BITS);
        }

        public override string ToString()
        {
            return string.Format("(X:{0}, Y:{1}, Z:{2}), W:{3}", X, Y, Z, W);
        }
    }
}
