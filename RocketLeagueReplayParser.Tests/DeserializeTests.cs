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
    public class DeserializeTests
    {
        [TestCaseSource(typeof(ReplayFileSource), nameof(ReplayFileSource.ReplayFiles))]
        public void TestDeserialization(string filePath)
        {
            var replay = Replay.Deserialize(filePath);

#if DEBUG // Test will just crash if we're in release mode and theres a bad frame
            
            var badFrames = replay.Frames.Where(x => x.ActorStates?.Any(s => !s.Complete) ?? true);

            if ( badFrames.Any() )
            {
                foreach (var f in replay.Frames.Skip(Math.Max(replay.Frames.Count-2,0)).Take(2))
                {
                    Console.WriteLine(f.ToDebugString(replay.Objects, replay.Names));
                }

                Console.WriteLine(replay.ToDebugString());
            }


            Assert.IsFalse(replay.Frames.Any(x => x.Failed));
            Assert.IsFalse(replay.Frames.Any(x => !x.Complete));
            Assert.IsFalse(replay.Frames.Any(x => x.ActorStates.Any(s=>!s.Complete)));
            Assert.IsFalse(replay.Frames.Any(x => x.ActorStates.Any(s => s.ForcedComplete)));
#endif
        }

        [TestCaseSource(typeof(ReplayFileSource), nameof(ReplayFileSource.ReplayFiles))]
        public void TestHeaderDeserialization(string filePath)
        {
            var replay = Replay.DeserializeHeader(filePath);
            Assert.IsTrue(replay.Properties.Any());
        }

        [TestCaseSource(typeof(ReplayFileSource), nameof(ReplayFileSource.ReplayFiles))]
        public void CreatePrettyJson(string filePath)
        {
            var replay = Replay.Deserialize(filePath);
            var jsonSerializer = new Serializers.JsonSerializer();
            jsonSerializer.Serialize(replay);
        }

        [TestCaseSource(typeof(ReplayFileSource), nameof(ReplayFileSource.ReplayFiles))]
        public void CreateRawJson(string filePath)
        {
            var replay = Replay.Deserialize(filePath);
            var jsonSerializer = new Serializers.JsonSerializer();
            jsonSerializer.SerializeRaw(replay);
        }
    }
}
