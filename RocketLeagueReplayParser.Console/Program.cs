using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any())
            {
                var result = Parser.Default.ParseArguments<Options>(args);
                result.MapResult(
                    options => Process(options),
                    _ => 1);
            }
            else
            {
                Parser.Default.ParseArguments<Options>(new string[] { "--help" });
            }
        }

        private static int Process(Options o)
        {
            var serializer = new Serializers.JsonSerializer();
            var inputFiles = new List<string>();

            if (o.DirectoryMode)
            {
                if ( !Directory.Exists(o.Input) )
                {
                    System.Console.Error.WriteLine("Directory does not exist");
                    return 1;
                }

                inputFiles.AddRange(Directory.EnumerateFiles(o.Input, "*.replay"));

                if (!inputFiles.Any())
                {
                    System.Console.Error.WriteLine("No replay files found in the specified directory");
                    return 1;
                }
            }
            else
            {
                inputFiles.Add(o.Input);
            }

            foreach (var file in inputFiles)
            {
                if (!File.Exists(file))
                {
                    System.Console.Error.WriteLine(string.Format("Specified replay file {0} does not exist", file));
                    return 1;
                }

                if (o.FileOutput)
                {
                    System.Console.Write("Processing replay " + file + "...");
                }

                var replay = Replay.Deserialize(file);

                string json;
                if (o.Raw)
                {
                    json = serializer.SerializeRaw(replay);
                }
                else
                {
                    json = serializer.Serialize(replay);
                }

                if (o.FileOutput)
                {
                    var filename = Path.GetFileNameWithoutExtension(file) + ".json";
                    File.WriteAllText(filename, json);
                    System.Console.WriteLine("Complete!");
                }
                else
                {
                    System.Console.Write(json);
                }
            }

            return 0;
        }
    }
}
