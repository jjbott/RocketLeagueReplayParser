using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.NetworkStream
{
    public class TeamPaint
    {
        public byte TeamNumber { get; private set; }
        
        // Almost definitely the BlueTeam/OrangeTeam colors from CarColors in TAGame.upk
        public byte TeamColorId  { get; private set; }

        // Almost definitely the Accent colors from CarColors in TAGame.upk
        public byte CustomColorId  { get; private set; }

        // Finish Ids are in TAGame.upk in the ProductsDB content
        public UInt32 TeamFinishId { get; private set; }
        public UInt32 CustomFinishId { get; private set; }

        public static TeamPaint Deserialize(BitReader br)
        {
            var tp = new TeamPaint();

            tp.TeamNumber = br.ReadByte();
            tp.TeamColorId = br.ReadByte();
            tp.CustomColorId = br.ReadByte();
            tp.TeamFinishId = br.ReadUInt32();
            tp.CustomFinishId = br.ReadUInt32();

            return tp;
        }

        public override string ToString()
        {
            return string.Format("TeamNumber:{0}, TeamColorId:{1}, TeamFinishId:{2}, CustomColorId:{3}, CustomFinishId:{4}", TeamNumber, TeamColorId, TeamFinishId, CustomColorId, CustomFinishId);
        }
    }
}
