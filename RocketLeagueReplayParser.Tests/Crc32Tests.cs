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
        [Test]
        [TestCaseSource(typeof(ReplayFileSource), nameof(ReplayFileSource.ReplayFiles))]
        public void ValidateReplayCrc(string file)
        {
            Assert.IsTrue(Replay.ValidateCrc(file, false));
        }
    }
}
