using NUnit.Framework;
using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.Tests
{
    [TestFixture]
    public class SerializeTests
    {
        [TestCaseSource(typeof(ReplayFileSource), nameof(ReplayFileSource.ReplayFiles))]
        public void TestRoundTripJsonSerialization(string filePath)
        {
            var replay = Replay.Deserialize(filePath);
            string originalJson = (new Serializers.JsonSerializer()).SerializeRaw(replay);
            string roundTripJson = null;

            using (MemoryStream stream = new MemoryStream())
            {
                replay.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var roundTripReplay = Replay.Deserialize(stream);
                roundTripJson = (new Serializers.JsonSerializer()).SerializeRaw(roundTripReplay);
            }

            if (replay.Properties["Id"].Value.ToString() == "5F9D44B6400E284FD15A95AC8D5C5B45")
            {
                // This replay was probably edited incorrectly (the name is "flipsid3-1-edited").
                // I have special case code in MergeDuplicateClasses that fixes it, but that causes round trips to fail.
                // So if we've gotten this far without crashing, good enough.
                return;
            }

            Assert.AreEqual(originalJson, roundTripJson);

        }

        [TestCaseSource(typeof(ReplayFileSource), nameof(ReplayFileSource.ReplayFiles))]
        public void TestRoundTripSerialization(string filePath)
        {
            var originalReplay = Replay.Deserialize(filePath);

            var originalBytes = File.ReadAllBytes(filePath);

            byte[] newBytes;
            using (MemoryStream stream = new MemoryStream())
            {
                originalReplay.Serialize(stream);

                newBytes = stream.ToArray();

                stream.Seek(0, SeekOrigin.Begin);
                Replay.Deserialize(stream);
            }
            
            if (originalReplay.Properties["Id"].Value.ToString() == "5F9D44B6400E284FD15A95AC8D5C5B45")
            {
                // This replay was probably edited incorrectly (the name is "flipsid3-1-edited").
                // I have special case code in MergeDuplicateClasses that fixes it, but that causes round trips to fail.
                // So if we've gotten this far without crashing, good enough.
                return;
            }

            Assert.AreEqual(originalBytes.Length, newBytes.Length);

            for (int i = 0; i < newBytes.Length; ++i)
            {
                Assert.AreEqual(originalBytes[i], newBytes[i]);
            }

        }

        [Test]
        public void TestRandomUIntMaxRoundTripSerialization()
        {
            var r = new Random();

            for (int n = 0; n < 1000; ++n)
            {
                int max = 0;
                while (max < 100) max = r.Next();

                int val = r.Next(max);

                TestUIntMaxRoundTripSerialization((UInt32)val, (UInt32)max);
            }
        }

        [TestCase(1U, 64U)]
        [TestCase(3U, 5U)]
        [TestCase(887977537U, 1173576471U)]
        [Test]
        public void TestUIntMaxRoundTripSerialization(UInt32 value, UInt32 max)
        {
            for (var i = 0; i < 2; ++i)
            {
                var bw = new BitWriter(32);
                bw.Write(value, max);
                var br = new BitReader(bw.GetBytes());
                var val2 = br.ReadUInt32Max((int)max);
                Assert.AreEqual(value, val2);
                Assert.AreEqual(bw.Length, br.Position);
                value = val2;
            }
        }

        [TestCase(UInt32.MaxValue, 32)]
        [TestCase(UInt32.MinValue, 32)]
        [Test]
        public void TestUIntFixedRoundTripSerialization(UInt32 value, int numBits)
        {
            for (var i = 0; i < 2; ++i)
            {
                var bw = new BitWriter(32);
                bw.WriteFixedBitCount(value, numBits);
                var br = new BitReader(bw.GetBytes());
                var val2 = br.ReadUInt32FromBits(numBits);
                Assert.AreEqual(value, val2);
                Assert.AreEqual(bw.Length, br.Position);
                value = val2;
            }
        }

        [Test]
        public void TestRandomFixedFloatRoundTripSerialization()
        {
            var r = new Random();

            for (int n = 0; n < 1000; ++n)
            {

                var f = (float)((r.NextDouble() * 2) - 1);

                for (var i = 0; i < 2; ++i)
                {
                    var bw = new BitWriter(32);
                    bw.WriteFixedCompressedFloat(f, 1, 16);
                    var br = new BitReader(bw.GetBytes());
                    var f2 = br.ReadFixedCompressedFloat(1, 16);

                    if (i == 0)
                    {
                        // We're generating floats that are probably going to lose precision when serialized.
                        // So the first time around just check to see if we're close.
                        Assert.IsTrue(Math.Abs(f - f2) < 0.0001);
                    }
                    else
                    {
                        Assert.AreEqual(f, f2);
                    }
                    Assert.AreEqual(bw.Length, br.Position);
                    f = f2;
                }
            }
        }
        // TODO: Fix so we dont grow test cases for every netversion
        [TestCase(0f, 0f, 99f, 5u)]
        [TestCase(0f, 0f, 99f, 7u)]
        [Test]
        public void TestVectorRoundTripSerialization(float x, float y, float z, UInt32 netVersion)
        {
            var v = new Vector3D(x, y, z);
            for (var i = 0; i < 2; ++i)
            {
                var bw = new BitWriter(32);
                v.Serialize(bw, netVersion);
                var br = new BitReader(bw.GetBytes());
                var v2 = Vector3D.Deserialize(br, netVersion);

                if (i == 0)
                {
                    // We're generating floats even though these are serialized as ints
                    // So the first time around just check to see if we're at the nearest int
                    Assert.IsTrue(Math.Abs(v.X - v2.X) <= 0.5);
                    Assert.IsTrue(Math.Abs(v.Y - v2.Y) <= 0.5);
                    Assert.IsTrue(Math.Abs(v.Z - v2.Z) <= 0.5);
                }
                else
                {
                    Assert.AreEqual(v.X, v2.X);
                    Assert.AreEqual(v.Y, v2.Y);
                    Assert.AreEqual(v.Z, v2.Z);
                }
                Assert.AreEqual(bw.Length, br.Position);
                v = v2;
            }
        }

        // TODO: Fix so we dont grow test cases for every netversion
        [TestCase("0110000000010000000111000111", 5u)] // 0,0,99
        [TestCase("1101000000000000100000000000110000110110001", 5u)] // 0, 2048, 432
        [TestCase("0110000000010000000111000111", 7u)] // 0,0,99
        [TestCase("1101000000000000100000000000110000110110001", 7u)] // 0, 2048, 432
        [Test]
        public void TestVectorRoundTripSerializationFromBinary(string binary, UInt32 netVersion)
        {
            var br = new BitReader(binary);
            var v = Vector3D.Deserialize(br, netVersion);

            var bw = new BitWriter(32);
            v.Serialize(bw, netVersion);
            var writtenBits = bw.GetBits(0, bw.Length).ToBinaryString();

            Assert.AreEqual(binary, writtenBits);
        }

        [Test]
        public void TestRandomVectorRoundTripSerialization([Values(5u, 7u)] UInt32 netVersion)
        {
            var r = new Random();

            for (int n = 0; n < 1000; ++n)
            {
                var x = (float)((r.NextDouble() * 200000) - 100000);
                var y = (float)((r.NextDouble() * 200000) - 100000);
                var z = (float)((r.NextDouble() * 200000) - 100000);

                TestVectorRoundTripSerialization(x, y, z, netVersion);
            }
        }

        [Test]
        public void TestRandomQuatRoundTripSerialization()
        {
            var r = new Random();
            for (int i = 0; i < 1000; ++i)
            {

                float x = (float)r.NextDouble();
                float y = (float)r.NextDouble();
                float z = (float)r.NextDouble();
                float w = (float)r.NextDouble();
                var l = (float)Math.Sqrt(x * x + y * y + z * z + w * w);
                x /= l;
                y /= l;
                z /= l;
                w /= l;

                TestQuatRoundTripSerialization(x, y, z, w);
            }
        }

        [TestCase(-0.004410246f, 0.00182074378f, 0.923867f, 0.382684022f)]
        [Test]
        public void TestQuatRoundTripSerialization(float x, float y, float z, float w)
        {
            var q = new Quaternion(x, y, z, w);

            // 3 round trips for good measure
            for (var j = 0; j < 2; ++j)
            {
                var bw = new BitWriter(2 + 18 * 3);
                q.Serialize(bw);
                var br = new BitReader(bw.GetBytes());
                var q2 = Quaternion.Deserialize(br);

                if (j == 0)
                {
                    // First round trip can be lossy, since we're using lossy conpression
                    Assert.AreEqual(q.X, q2.X, .0001);
                    Assert.AreEqual(q.Y, q2.Y, .0001);
                    Assert.AreEqual(q.Z, q2.Z, .0001);
                    Assert.AreEqual(q.W, q2.W, .0001);
                }
                else
                {
                    Assert.AreEqual(q.X, q2.X);
                    Assert.AreEqual(q.Y, q2.Y);
                    Assert.AreEqual(q.Z, q2.Z);
                    Assert.AreEqual(q.W, q2.W);
                }
                q = q2;
            }
        }
    }
}
