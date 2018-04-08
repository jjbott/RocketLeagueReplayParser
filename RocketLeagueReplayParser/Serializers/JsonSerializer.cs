using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using DeepEqual.Syntax;

namespace RocketLeagueReplayParser.Serializers
{
    public class JsonSerializer
    {

        public class ActorStateJson
        {
            public UInt32 Id { get; set; }
            public bool? UnknownBit { get; set; }
            public string TypeName { get; set; }
            public string ClassName { get; set; }

            public Vector3D InitialPosition { get; set; }

            public Dictionary<UInt32, ActorStateProperty> Properties { get; set; } = new Dictionary<UInt32, ActorStateProperty>();
        }

        public class ActorStateProperty
        {
            public UInt32 Id { get; set; }
            public string Name { get; set; }
            public object Data { get; set; }
        }

        public string Serialize(Replay replay)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.RegisterConverters(new List<JavaScriptConverter>(){
                new ReplayJsonConverter(false, includeKeyFrames:false),
                new FrameJsonConverter(false),
                new MetadataPropertyConverter(false, (f) => replay.Frames[(int)f].Time),
                new MetadataPropertyDictionaryConverter(false, (f) => replay.Frames[(int)f].Time),
                new ActorStateJsonConverter(false, true),
                new ClassNetCacheJsonConverter()});
            serializer.MaxJsonLength = 40 * 1024 * 1024;
            

            return serializer.Serialize(replay);
        }
        
        /// <summary>
        /// Output the replay as json with minimal post processing. 
        /// No removal of duplicate data, no joining of new and updated data. 
        /// </summary>
        /// <param name="replay"></param>
        /// <returns></returns>
        public string SerializeRaw(Replay replay)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.RegisterConverters(new List<JavaScriptConverter>(){
                new ReplayJsonConverter(true),
                new FrameJsonConverter(true),
                new MetadataPropertyConverter(true, (f) => replay.Frames[(int)f].Time),
                new MetadataPropertyDictionaryConverter(true, (f) => replay.Frames[(int)f].Time),
                new ActorStateJsonConverter(true, true),
                new ClassNetCacheJsonConverter()});
            serializer.MaxJsonLength = 40 * 1024 * 1024;


            return serializer.Serialize(replay);
        }
    }
}
