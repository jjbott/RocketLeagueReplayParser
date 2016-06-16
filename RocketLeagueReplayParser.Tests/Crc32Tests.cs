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
    public class Crc32Tests
    {
        public IEnumerable<string> ReplayFiles
        {
            get
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Rocket League\TAGame\Demos\");
                return Directory.EnumerateFiles(dir, "*.replay").OrderByDescending(f => new FileInfo(f).CreationTime);
            }
        }

        [Test]
        [TestCaseSource("ReplayFiles")]
        public void ValidateReplayCrc(string file)
        {
            Assert.IsTrue(Replay.ValidateCrc(file, false));
        }
    }
}
