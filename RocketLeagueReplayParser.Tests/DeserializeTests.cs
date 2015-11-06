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
    public class DeserializeTests
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
        public void TestDeserialization(string filePath)
        {
            string log;
            var replay = Replay.Deserialize(filePath, out log);
            Console.WriteLine(log);
            //replay.ToObj(); 
            Console.WriteLine(replay.ToDebugString());
            //Console.WriteLine(replay.ToPositionJson());

            Assert.IsFalse(replay.Frames.Any(x => !x.Complete));
            Assert.IsFalse(replay.Frames.Any(x => x.ActorStates.Any(s=>!s.Complete)));
            Assert.IsFalse(replay.Frames.Any(x => x.ActorStates.Any(s => s.ForcedComplete)));
        }

        [TestCaseSource("ReplayFiles")]
        public void CreateJson(string filePath)
        {
            string log;
            var replay = Replay.Deserialize(filePath, out log);
            Console.WriteLine(replay.ToJson());
        }
    }
}
