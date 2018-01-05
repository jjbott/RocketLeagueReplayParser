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
        private readonly bool _rawMode;
        private readonly bool _includeReplayMetadata;
        private readonly bool _includeMetadataProperties;
        private readonly bool _includeLevels;
        private readonly bool _includeKeyFrames;
        private readonly bool _includeFrames;
        private readonly bool _includeDebugStrings;
        private readonly bool _includeTickMarks;
        private readonly bool _includePackages;
        private readonly bool _includeObjects;
        private readonly bool _includeNames;
        private readonly bool _includeClassIndexes;
        private readonly bool _includeClassNetCaches;

        public ReplayJsonConverter(
            bool rawMode,
            bool includeReplayHeader = true,
            bool includeMetadataProperties = true,
            bool includeLevels = true,
            bool includeKeyFrames = true,
            bool includeFrames = true,
            bool includeDebugStrings = false,
            bool includeTickMarks = true, // eh...
            bool includePackages = true,
            bool includeObjects = true,
            bool includeNames = true,
            bool includeClassIndexes = true,
            bool includeClassNetCaches = true
            )
        {
            _rawMode = rawMode;
            _includeReplayMetadata = includeReplayHeader;
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

            if (_includeReplayMetadata)
            {
                result["Part1Length"] = replay.Part1Length;
                result["Part1Crc"] = replay.Part1Crc.ToString("X");
                result["EngineVersion"] = replay.EngineVersion;
                result["LicenseeVersion"] = replay.LicenseeVersion;
                result["ReplayClass"] = replay.TAGame_Replay_Soccar_TA;
            }

            if (_includeMetadataProperties)
            {
                result["Properties"] = replay.Properties;
            }

            if (_includeReplayMetadata)
            {
                result["Part2Crc"] = replay.Part2Crc.ToString("X");
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
                if (_rawMode)
                {
                    result["Frames"] = replay.Frames;
                }
                else
                {
                    // Frame serializer will produce null frames. Filter those out
                    // Round-tripping the frames so we end up with objects.
                    // Otherwise we'll end up with a serialized list of strings.
                    // Yeah it sucks but I havent thought of something better yet.
                    result["Frames"] = replay.Frames
                        .Select(x => serializer.DeserializeObject(serializer.Serialize(x)))
                        .Where(x => x != null);
                }
            }

            if (_includeDebugStrings)
            {
                result["DebugStrings"] = replay.DebugStrings;
            }

            if (_includeTickMarks)
            {
                if (_rawMode)
                {
                    result["TickMarks"] = replay.TickMarks;
                }
                else
                {
                    // In "pretty" mode we'll be removing frames that dont add useful info.
                    // So, the frame index may not line up correctly.
                    // Replace with time info, since thats all we need anyways.
                    result["TickMarks"] = replay.TickMarks.Select(x => new { Type = x.Type, Time = replay.Frames[Math.Max(0, x.Frame)].Time });
                }
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
