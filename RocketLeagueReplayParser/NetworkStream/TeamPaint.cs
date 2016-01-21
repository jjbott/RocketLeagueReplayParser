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
        public byte TeamColorId  { get; private set; }
        public Int32 TeamFinishId  { get; private set; }
        public byte CustomColorId  { get; private set; }
        public Int32 CustomFinishId { get; private set; }

        public static TeamPaint Deserialize(BitReader br)
        {
            var tp = new TeamPaint();

            tp.TeamNumber = br.ReadByte();
            tp.TeamColorId = br.ReadByte();
            tp.TeamFinishId = br.ReadInt32();
            tp.CustomColorId = br.ReadByte();
            tp.CustomFinishId = br.ReadInt32();

            return tp;
        }

        public override string ToString()
        {
            return string.Format("TeamNumber:{0}, TeamColorId:{1}, TeamFinishId:{2}, CustomColorId:{3}, CustomFinishId:{4}", TeamNumber, TeamColorId, TeamFinishId, CustomColorId, CustomFinishId);
        }
    }
}
