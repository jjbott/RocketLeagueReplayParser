using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RocketLeagueReplayParser.Serializers
{
    public class ReplayJsonConverter : JavaScriptConverter
    {
        bool _raw;

        public ReplayJsonConverter(bool raw)
        {
            _raw = raw;
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new[] { typeof(Replay) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            Replay replay = (Replay)obj;

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["Unknown1"] = replay.Unknown1;
            result["Unknown2"] = replay.Unknown2;
            result["Unknown3"] = replay.Unknown3;
            result["Unknown4"] = replay.Unknown4;
            result["Unknown5"] = replay.Unknown5;
            result["Properties"] = replay.Properties;
            //result["LengthOfRemainingData"] = replay.LengthOfRemainingData;
            result["Unknown7"] = replay.Unknown7;
            //result["LevelLength"] = replay.LevelLength;
            result["Levels"] = replay.Levels;
            //result["KeyFrameLength"] = replay.KeyFrameLength;
            result["KeyFrames"] = replay.KeyFrames;
            
            // The raw network stream is of no use to anyone
            //result["NetworkStreamLength"] = replay.NetworkStreamLength;
            //result["NetworkStream"] = replay.NetworkStream;
            
            result["Frames"] = replay.Frames;
            //result["DebugStringLength"] = replay.DebugStringLength;
            result["DebugStrings"] = replay.DebugStrings;
            //result["TickMarkLength"] = replay.TickMarkLength;
            result["TickMarks"] = replay.TickMarks;
            //result["PackagesLength"] = replay.PackagesLength;
            result["Packages"] = replay.Packages;
            //result["ObjectLength"] = replay.ObjectLength;
            result["Objects"] = replay.Objects;
            //result["NamesLength"] = replay.NamesLength;
            result["Names"] = replay.Names;
            //result["ClassIndexLength"] = replay.ClassIndexLength;
            result["ClassIndexes"] = replay.ClassIndexes;
            //result["ClassNetCacheLength"] = replay.ClassNetCacheLength;
            result["ClassNetCaches"] = replay.ClassNetCaches;
            return result;
        }
   
    }
}
