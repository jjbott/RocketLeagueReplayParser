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

                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Rocket League\TAGame\Demos");
                return Directory.EnumerateFiles(dir);
            }
        }

        [TestCaseSource("ReplayFiles")]
        public void TestDeserialization(string filePath)
        {
            var replay = Replay.Deserialize(filePath);
            Console.WriteLine(replay.Unknown5);
            foreach(var prop in replay.Properties)
            {
                Console.WriteLine(prop.ToString());
            }

        }
    }
}
