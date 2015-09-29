using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser
{
    public class Level
    {
        public string Name { get; private set; }

        public static Level Deserialize(BinaryReader bs)
        {
            var level = new Level();
            level.Name = bs.ReadAsciiString();
            return level;
        }
    }
}
