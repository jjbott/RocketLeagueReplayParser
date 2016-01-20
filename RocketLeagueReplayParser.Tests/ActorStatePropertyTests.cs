using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RocketLeagueReplayParser.NetworkStream;
using System.IO;

namespace RocketLeagueReplayParser.Tests
{
    [TestFixture]
    public class ActorStatePropertyTests
    {
        public IEnumerable<string> ReplayFiles
        {
            get
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Rocket League\TAGame\Demos\");
                return Directory.EnumerateFiles(dir, "*.replay");
            }
        }


        [TestCaseSource("ReplayFiles")]
        public void ClientLoadoutTest(string filePath)
        {
            string log;
            var r = Replay.Deserialize(filePath, out log);

            foreach (var p in r.Frames.Where(f => f.ActorStates != null).SelectMany(x => x.ActorStates).Where(s => s.Properties != null).SelectMany(s => s.Properties).Where(p => p.PropertyName == "TAGame.PRI_TA:ClientLoadout"))
            {
                var cl = (ClientLoadout)p.Data[0];
                Assert.IsTrue(cl.Unknown1 == 10);
                Assert.IsTrue(cl.Unknown2 == 0);
            }
        }
    }
}
