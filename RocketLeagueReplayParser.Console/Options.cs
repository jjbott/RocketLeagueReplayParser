using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.Console
{
    public class Options
    {
        [Option('r', "raw", Default = false, HelpText = "Output 'raw' JSON")]
        public bool Raw { get; set; }

        // Omitting long name, default --verbose
        [Option("fileoutput", HelpText = "Enable file output to current directory. By default writes to stdout")]
        public bool FileOutput { get; set; }

        [Option('d', "directory", Default = false, HelpText = "Directory mode: Parses all replay files in the input directory.")]
        public bool DirectoryMode { get; set; }

        [Value(0, Required = true, MetaName = "input", HelpText = "The replay file/directory to parse")]
        public string Input { get; set; }
    }
}
