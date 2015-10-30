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
                return Directory.EnumerateFiles(dir, "*.replay");
            }
        }

        [Test]
        [TestCaseSource("ReplayFiles")]
        public void Crc32Test(string file)
        {
            var bytes = File.ReadAllBytes(file);//@"D:\MyData\CodeProjects\RocketLeagueReplayParser\RocketLeagueReplayParserWeb\replays\9baaa252-d585-4627-b726-f790e9c3f1fb-2s.replay");

            var targetCrc = BitConverter.ToUInt32(bytes, 4);
            Console.WriteLine("Target crc: " + targetCrc.ToString("x8"));
            var altTargetCrc = BitConverter.ToUInt32(new byte[] { bytes[7], bytes[6], bytes[5], bytes[4] }, 0);
            Console.WriteLine("Alt Target crc: " + altTargetCrc.ToString("x8"));

            var startPosition = 8;
            for (var s = startPosition; s < bytes.Length; s++)
            {
                //Console.WriteLine("Checking index: " + s.ToString());
                UInt32 crc = 0;
                for (var e = s; e < bytes.Length; e++)
                {
                    crc = Crc32.Compute(new byte[] { bytes[e] }, 1, crc);
                    if (crc == targetCrc)
                    {
                        Console.WriteLine(string.Format("Matching CRC found! Start index {0}, end index {1}", s, e));
                    }

                    if (crc == altTargetCrc)
                    {
                        Console.WriteLine(string.Format("Matching Alt CRC found! Start index {0}, end index {1}", s, e));
                    }
                }
            }

            /*
            UInt32 crc = Crc32.Compute(new byte[] { 0}, 1, 0);
            crc = Crc32.Compute(new byte[] { 0 }, 1, crc);
            crc = Crc32.Compute(new byte[] { 0 }, 1, crc);
            crc = Crc32.Compute(new byte[] { 0 }, 1, crc);
            Console.WriteLine(crc.ToString("x8"));
             * */
        }

        [Test]
        public void Crc32Test2()
        {
            UInt32 crc = Crc32.Compute_Deprecated(new byte[] { 0 }, 1, 0);
            crc = Crc32.Compute_Deprecated(new byte[] { 0 }, 1, crc);
            crc = Crc32.Compute_Deprecated(new byte[] { 0 }, 1, crc);
            crc = Crc32.Compute_Deprecated(new byte[] { 0 }, 1, crc);
            Console.WriteLine(crc.ToString("x8"));

        }


    }
}
