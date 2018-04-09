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
    public class JsonTests
    {
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

        [Explicit]
        [TestCaseSource(typeof(ReplayFileSource), nameof(ReplayFileSource.ReplayFiles))]
        public void OpenSerializedJson(string filePath)
        {
            var jsonFilePath = Path.Combine(Path.GetTempPath(), "replay.json");

            // Prevent accidentially opening hundreds of replays
            if (File.Exists(jsonFilePath))
            {
                var lastWriteTime = File.GetLastWriteTime(jsonFilePath);
                Assert.IsTrue(lastWriteTime < (DateTime.Now - TimeSpan.FromSeconds(10)));
            }

            var replay = Replay.Deserialize(filePath);
            var jsonSerializer = new Serializers.JsonSerializer();
            File.WriteAllText(jsonFilePath, jsonSerializer.Serialize(replay, false, true));

            System.Diagnostics.Process.Start(jsonFilePath);
        }
    }
}
