using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.Tests
{
    public static class ReplayFileSource
    {
        public static IEnumerable<TestCaseData> ReplayFiles
        {
            get
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Rocket League\TAGame\Demos\");
                return Directory.EnumerateFiles(dir, "*.replay").OrderByDescending(x => File.GetCreationTime(x))
                    // Ignore this replay. It crashes RL too
                    .Where(f => !f.Contains("B82DDB624C393A4A425E68AB40DC2450"))
                    .Select((f, i) => new TestCaseData(f).SetName($"{{m}}({i:D5}-{Path.GetFileNameWithoutExtension(f)})"));
            }
        }
    }
}
