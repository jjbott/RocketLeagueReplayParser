using NUnit.Framework;
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
        public IEnumerable<string> ReplayFiles
        {
            get
            {
				var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Rocket League\TAGame\Demos\");
                return Directory.EnumerateFiles(dir, "*.replay").OrderByDescending(x => File.GetCreationTime(x));
            }
        }

        [TestCaseSource("ReplayFiles")]
        public void TestRoundTripSerialization(string filePath)
        {
            var replay = Replay.Deserialize(filePath);

            var originalBytes = File.ReadAllBytes(filePath);

            byte[] newBytes;
            using (MemoryStream stream = new MemoryStream())
            {
                replay.Serialize(stream);

                newBytes = stream.ToArray();
            }
            
            Assert.AreEqual(originalBytes.Length, newBytes.Length);

            for (int i = 0; i < newBytes.Length; ++i)
            {
                Assert.AreEqual(originalBytes[i], newBytes[i]);
            }

        }
    }
}
