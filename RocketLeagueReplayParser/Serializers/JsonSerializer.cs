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

            public List<ActorStateProperty> Properties { get; set; }
        }

        public class ActorStateProperty
        {
            public string Name { get; set; }
            public List<object> Data { get; set; }
        }

        public string Serialize(Replay replay)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.RegisterConverters(new List<JavaScriptConverter>(){
                new ReplayJsonConverter(false, includeKeyFrames:false),
                new FrameJsonConverter(false),
                new MetadataPropertyConverter(false, (f) => replay.Frames[(int)f].Time),
                new MetadataPropertyDictionaryConverter(false, (f) => replay.Frames[(int)f].Time),
                new ActorStateJsonConverter(),
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
            serializer.RegisterConverters(new List<JavaScriptConverter>() { });

            //Dictionary<int, ActorStateJson> actorStates = new Dictionary<int, ActorStateJson>();
            var frameJson = new List<string>();
            foreach (var f in replay.Frames.Where(x => x.ActorStates.Count > 0))
            {
                List<UInt32> deletedActorStateIds = new List<UInt32>();
                List<ActorStateJson> newActorStates = new List<ActorStateJson>();
                List<ActorStateJson> updatedActorStates = new List<ActorStateJson>();

                Dictionary<int, ActorStateJson> actor = new Dictionary<int, ActorStateJson>();

                foreach (var a in f.ActorStates.Where(x => x.State == ActorStateState.Deleted))
                {
                    deletedActorStateIds.Add(a.Id);
                }

                foreach (var a in f.ActorStates.Where(x => x.State == ActorStateState.New))
                {
                    var actorState = new ActorStateJson();
                    actorState.Id = a.Id;
                    actorState.UnknownBit = a.Unknown1;
                    actorState.TypeName = a.TypeName;
                    actorState.ClassName = a.ClassName;
                    actorState.InitialPosition = a.Position;
                    actorState.Properties = new List<ActorStateProperty>();
                    newActorStates.Add(actorState);
                }

                foreach (var a in f.ActorStates.Where(x => x.State == ActorStateState.Existing))
                {
                    var actorState = new ActorStateJson();
                    actorState.Id = a.Id;
                    actorState.Properties = new List<ActorStateProperty>();

                    actorState.Properties = a.Properties.Select(p => new ActorStateProperty { Name = p.PropertyName, Data = p.Data }).ToList();

                    updatedActorStates.Add(actorState);
                }

                // Serializing at each frame to make sure we capture the state at each step.
                // Otherwise, since we're not cloning objects at each step, we'd serialize only the most recent set of data
                frameJson.Add(serializer.Serialize(new { Time = f.Time, DeletedActorIds = deletedActorStateIds, NewActors = newActorStates, UpdatedActors = updatedActorStates }));
            }
            return "[" + string.Join(",", frameJson) + "]";
        }
    }
}
