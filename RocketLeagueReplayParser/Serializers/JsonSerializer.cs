using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace RocketLeagueReplayParser.Serializers
{
    public class JsonSerializer
    {

        public class ActorStateJson
        {
            public UInt32 Id { get; set; }
            public UInt32? NameId { get; set; }
            public bool? UnknownBit { get; set; }
            public UInt32? TypeId { get; set; }
            public int? ClassId { get; set; }

            public Vector3D InitialPosition { get; set; }

            public Dictionary<UInt32, ActorStateProperty> Properties { get; set; } = new Dictionary<UInt32, ActorStateProperty>();
        }
        
        /// <summary>
        /// "Pretty" Json serialization. 
        /// Includes post processing to remove duplicate data, and may exclude some data
        /// </summary>
        /// <param name="replay"></param>
        /// <param name="minimal">If true, reduces the size of the JSON, mainly by using ids instead of strings</param>
        /// <returns></returns>
        public string Serialize(Replay replay, bool minimal = false, bool indent = false)
        {
            return JsonConvert.SerializeObject(replay,
                indent ? Formatting.Indented : Formatting.None,
                new JsonConverter[]{
                    new ReplayJsonConverter(false, includeKeyFrames:false),
                    new FrameJsonConverter(false),
                    new MetadataPropertyConverter(false, (f) => replay.Frames[(int)f].Time),
                    new MetadataPropertyDictionaryConverter(false),
                    new ActorStateJsonConverter(false, minimal, replay.Objects),
                    new ClassIndexJsonConverter(false),
                    new FloatJsonConverter(),
                    new ClassNetCacheJsonConverter(),
                    new ActorStatePropertyJsonConverter(false, minimal, replay.Objects)
                });
        }
        
        /// <summary>
        /// Output the replay as json with minimal post processing. 
        /// No removal of duplicate data, no joining of new and updated data. 
        /// </summary>
        /// <param name="replay"></param>
        /// <param name="minimal">If true, reduces the size of the JSON, mainly by using ids instead of strings</param>
        /// <returns></returns>
        public string SerializeRaw(Replay replay, bool minimal = false, bool indent = false)
        {
            return JsonConvert.SerializeObject(replay, 
                indent ? Formatting.Indented : Formatting.None, 
                new JsonConverter[]{
                    new ReplayJsonConverter(true),
                    new FrameJsonConverter(true),
                    new MetadataPropertyConverter(true, (f) => replay.Frames[(int)f].Time),
                    new MetadataPropertyDictionaryConverter(true),
                    new ActorStateJsonConverter(true, minimal, replay.Objects),
                    new ClassIndexJsonConverter(true),
                    new FloatJsonConverter(),
                    new ClassNetCacheJsonConverter(),
                    new ActorStatePropertyJsonConverter(true, minimal, replay.Objects
                )});
        }
    }
}
