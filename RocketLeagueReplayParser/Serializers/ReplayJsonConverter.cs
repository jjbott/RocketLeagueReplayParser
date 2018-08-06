using Newtonsoft.Json;
using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.Serializers
{
    public class ReplayJsonConverter : JsonConverter
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

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Replay);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            Replay replay = (Replay)value;

            writer.WriteStartObject();

            if (_includeReplayMetadata)
            {
                writer.WriteKeyValue("Part1Length", replay.Part1Length, serializer);
                writer.WriteKeyValue("Part1Crc", replay.Part1Crc.ToString("X"), serializer);
                writer.WriteKeyValue("EngineVersion", replay.EngineVersion, serializer);
                writer.WriteKeyValue("LicenseeVersion", replay.LicenseeVersion, serializer);
                if (replay.EngineVersion >= 868 && replay.LicenseeVersion >= 18)
                {
                    writer.WriteKeyValue("NetVersion", replay.NetVersion, serializer);
                }
                writer.WriteKeyValue("ReplayClass", replay.TAGame_Replay_Soccar_TA, serializer);
            }

            if (_includeMetadataProperties)
            {
                writer.WriteKeyValue("Properties", replay.Properties, serializer);
            }

            if (_includeReplayMetadata)
            {
                writer.WriteKeyValue("Part2Crc", replay.Part2Crc.ToString("X"), serializer);
            }

            if (_includeLevels)
            {
                writer.WriteKeyValue("Levels", replay.Levels, serializer);
            }

            if (_includeKeyFrames)
            {
                writer.WriteKeyValue("KeyFrames", replay.KeyFrames, serializer);
            }

            // The raw network stream is of no use to anyone ever
            //writer.WriteKeyValue("NetworkStreamLength", replay.NetworkStreamLength, serializer);
            //writer.WriteKeyValue("NetworkStream", replay.NetworkStream, serializer);

            if (_includeFrames)
            {
                writer.WriteKeyValue("Frames", replay.Frames, serializer);
            }

            if (_includeDebugStrings)
            {
                writer.WriteKeyValue("DebugStrings", replay.DebugStrings, serializer);
            }

            if (_includeTickMarks)
            {
                if (_rawMode)
                {
                    writer.WriteKeyValue("TickMarks", replay.TickMarks, serializer);
                }
                else
                {
                    // In "pretty" mode we'll be removing frames that dont add useful info.
                    // So, the frame index may not line up correctly.
                    // Replace with time info, since thats all we need anyways.
                    var tickmarkTimes = replay.TickMarks.Select(x => new { Type = x.Type, Time = replay.Frames[Math.Max(0, x.Frame)].Time });
                    writer.WriteKeyValue("TickMarks", tickmarkTimes, serializer);
                }
            }

            if (_includePackages)
            {
                writer.WriteKeyValue("Packages", replay.Packages, serializer);
            }

            if (_includeObjects)
            {
                writer.WriteKeyValue("Objects", replay.Objects, serializer);
            }

            if (_includeNames)
            {
                writer.WriteKeyValue("Names", replay.Names, serializer);
            }

            if (_includeClassIndexes)
            {
                writer.WriteKeyValue("ClassIndexes", replay.ClassIndexes, serializer);
            }

            if (_includeClassNetCaches)
            {
                writer.WriteKeyValue("ClassNetCaches", replay.ClassNetCaches, serializer);
            }

            writer.WriteEndObject();
        }
   
    }
}
