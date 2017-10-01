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
                return Directory.EnumerateFiles(dir, "*.replay").OrderByDescending(x => File.GetCreationTime(x));
            }
        }

        [TestCaseSource("ReplayFiles")]
        public void TestDeserialization(string filePath)
        {
            var replay = Replay.Deserialize(filePath);

#if DEBUG // Test will just crash if we're in release mode and theres a bad frame
            
            var badFrames = replay.Frames.Where(x => x.ActorStates.Any(s => !s.Complete));
            foreach(var f in badFrames)
            {
                Console.WriteLine(f.ToDebugString(replay.Objects, replay.Names)); 
            }
            Assert.IsFalse(replay.Frames.Any(x => !x.Complete));
            Assert.IsFalse(replay.Frames.Any(x => x.ActorStates.Any(s=>!s.Complete)));
            Assert.IsFalse(replay.Frames.Any(x => x.ActorStates.Any(s => s.ForcedComplete)));
#endif
        }

        [TestCaseSource("ReplayFiles")]
        public void TestHeaderDeserialization(string filePath)
        {
            var replay = Replay.DeserializeHeader(filePath);
            Assert.IsTrue(replay.Properties.Any());
        }

        [TestCaseSource("ReplayFiles")]
        public void CreateJson(string filePath)
        {
            var replay = Replay.Deserialize(filePath);
            var jsonSerializer = new Serializers.JsonSerializer();
            Console.WriteLine(jsonSerializer.Serialize(replay));
        }

        [TestCaseSource("ReplayFiles")]
        public void CreateRawJson(string filePath)
        {
            var replay = Replay.Deserialize(filePath);
            var jsonSerializer = new Serializers.JsonSerializer();
            Console.WriteLine(jsonSerializer.SerializeRaw(replay));
        }

        [TestCaseSource("ReplayFiles")]
        public void CreatePositionJson(string filePath)
        {
            var replay = Replay.Deserialize(filePath);
            Console.WriteLine(replay.ToPositionJson());
        }

        [TestCaseSource("ReplayFiles")]
        public void CreateHeatMap(string filePath)
        {
            var replay = Replay.Deserialize(filePath);
            replay.ToHeatmap();
        }
    }
}
