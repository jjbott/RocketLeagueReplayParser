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
        bool _rawMode;
        bool _includeUnknowns;
        bool _includeMetadataProperties;
        bool _includeLevels;
        bool _includeKeyFrames;
        bool _includeFrames;
        bool _includeDebugStrings;
        bool _includeTickMarks;
        bool _includePackages;
        bool _includeObjects;
        bool _includeNames;
        bool _includeClassIndexes;
        bool _includeClassNetCaches;

        public ReplayJsonConverter(
            bool rawMode = false,
            bool includeUnknowns = false,
            bool includeMetadataProperties = true,
            bool includeLevels = false,
            bool includeKeyFrames = false,
            bool includeFrames = true,
            bool includeDebugStrings = false,
            bool includeTickMarks = true, // eh...
            bool includePackages = false,
            bool includeObjects = false,
            bool includeNames = false,
            bool includeClassIndexes = false,
            bool includeClassNetCaches = false
            )
        {
            _rawMode = rawMode;
            _includeUnknowns = includeUnknowns;
            _includeMetadataProperties = includeMetadataProperties;
            _includeLevels = includeLevels;
            _includeKeyFrames = includeKeyFrames;
            _includeFrames = includeFrames;
            _includeDebugStrings = includeDebugStrings;
            _includeTickMarks = includeTickMarks; 
            _includePackages = includePackages;
            _includeObjects = includeObjects;
            _includeNames = includeNames;
            _includeClassIndexes = includeClassIndexes;
            _includeClassNetCaches = includeClassNetCaches;
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

            if (_includeUnknowns)
            {
                result["Unknown1"] = replay.Unknown1;
                result["Unknown2"] = replay.Unknown2;
                result["Unknown3"] = replay.Unknown3;
                result["Unknown4"] = replay.Unknown4;
                result["Unknown5"] = replay.Unknown5;
            }

            if (_includeMetadataProperties)
            {
                result["Properties"] = replay.Properties;
            }

            if (_includeUnknowns)
            {
                result["Unknown7"] = replay.Unknown7;
            }

            if (_includeLevels)
            {
                result["Levels"] = replay.Levels;
            }

            if (_includeKeyFrames)
            {
                result["KeyFrames"] = replay.KeyFrames;
            }
            
            // The raw network stream is of no use to anyone
            //result["NetworkStreamLength"] = replay.NetworkStreamLength;
            //result["NetworkStream"] = replay.NetworkStream;

            if (_includeFrames)
            {
                result["Frames"] = replay.Frames;
            }

            if (_includeDebugStrings)
            {
                result["DebugStrings"] = replay.DebugStrings;
            }

            if (_includeTickMarks)
            {
                result["TickMarks"] = replay.TickMarks;
            }

            if (_includePackages)
            {
                result["Packages"] = replay.Packages;
            }

            if (_includeObjects)
            {
                result["Objects"] = replay.Objects;
            }

            if (_includeNames)
            {
                result["Names"] = replay.Names;
            }

            if (_includeClassIndexes)
            {
                result["ClassIndexes"] = replay.ClassIndexes;
            }

            if (_includeClassNetCaches)
            {
                result["ClassNetCaches"] = replay.ClassNetCaches;
            }

            return result;
        }
   
    }
}
