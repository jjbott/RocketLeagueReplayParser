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
            public int Id { get; set; }
            public bool UnknownBit { get; set; }
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

        public class ActorStatePropertyConverter : JavaScriptConverter
        {
            public override IEnumerable<Type> SupportedTypes
            {
                get
                {
                    return new[] { typeof(ActorStateProperty) };
                }
            }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                ActorStateProperty p = (ActorStateProperty)obj;
                IDictionary<string, object> serialized = new Dictionary<string, object>();

                if ( p.Data.Count == 1 )
                {
                    serialized[p.Name] = p.Data[0];
                }
                else
                {
                    serialized[p.Name] = p.Data;
                }

                // Adding extra info in this case to convert to a flag. Is that weird? I dunno...
                // Conflicted between creating raw JSON, and creating "nice" JSON
                if (p.Name == "TAGame.CarComponent_TA:ReplicatedActive")
                {
                    serialized["TAGame.CarComponent_TA:Active"] = (Convert.ToInt32(p.Data[0]) % 2) != 0;
                }

                return serialized;
            }
        }

        public string Serialize(Replay replay)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.RegisterConverters(new List<JavaScriptConverter>(){new ActorStatePropertyConverter(), new FrameJsonConverter(false), new ReplayJsonConverter(false), new MetadataPropertyConverter()});
            serializer.MaxJsonLength = 20 * 1024 * 1024;
            /*
            Dictionary<int, ActorStateJson> actorStates = new Dictionary<int, ActorStateJson>();
            var frameJson = new List<string>();
            foreach (var f in replay.Frames.Where(x => x.ActorStates.Count > 0))
            {
                List<Int32> deletedActorStateIds = new List<int>();
                Dictionary<int, ActorStateJson> newActorStates = new Dictionary<int, ActorStateJson>();

                foreach (var a in f.ActorStates.Where(x => x.State == ActorStateState.Deleted))
                {
                    actorStates.Remove(a.Id);
                    deletedActorStateIds.Add(a.Id);
                }

                foreach (var a in f.ActorStates.Where(x => x.State == ActorStateState.New))
                {
                    // Skip anything thats not truly new. Dont want keyframes in our json (for now)
                    if (!actorStates.ContainsKey(a.Id))
                    {
                        var actorState = new ActorStateJson();
                        actorState.Id = a.Id;
                        actorState.UnknownBit = a.Unknown1;
                        actorState.TypeName = a.TypeName;
                        actorState.ClassName = a.ClassName;
                        actorState.InitialPosition = a.Position;
                        actorState.Properties = new List<ActorStateProperty>();

                        actorStates[a.Id] = actorState;
                    }
                }

                foreach (var a in f.ActorStates.Where(x => x.State == ActorStateState.Existing))
                {
                    var existingActorState = actorStates[a.Id];
                    ActorStateJson actorState = null;
                    if (newActorStates.ContainsKey(a.Id))
                    {
                        // new actor
                        actorState = newActorStates[a.Id];
                    }
                    else
                    {
                        // Existing actor. Start a new state object to track changes

                        actorState = new ActorStateJson();
                        actorState.Id = a.Id;
                        actorState.Properties = new List<ActorStateProperty>();

                        // Maybe skip some of the following for existing actors
                        //actorState.UnknownBit = existingActorState.UnknownBit;
                        //actorState.TypeName = existingActorState.TypeName;
                        actorState.ClassName = existingActorState.ClassName;
                        //actorState.InitialPosition = existingActorState.InitialPosition;
                    }

                    foreach (var p in a.Properties)
                    {
                        var property = new ActorStateProperty
                        {
                            Name = p.PropertyName,
                            Data = p.Data
                        };

                        var existingProperty = existingActorState.Properties.Where(ep => ep.Name == property.Name).FirstOrDefault();
                        if (existingProperty == null)
                        {
                            // new property
                            actorState.Properties.Add(property);
                            existingActorState.Properties.Add(property);
                        }
                        else
                        {
                            // Existing property.
                            if (property.Name == "TAGame.Ball_TA:HitTeamNum" // Keep "Event" properties.
                                || property.Name.Contains("Music") // Kind of guessing at some of these event properties. We'll see how they turn out.
                                || property.Name.Contains("Sound")
                                || property.Name.Contains("Event")
                                || !existingProperty.IsDeepEqual(property))  // Only keep if it is truly different
                            {
                                actorState.Properties.Add(property);

                                // replace the property in our main set of data, so we have it for the next frame
                                existingActorState.Properties = existingActorState.Properties.Where(ep => ep.Name != property.Name).ToList();
                                existingActorState.Properties.Add(property);
                            }
                        }
                    }

                    // Dont keep this state if we havent found any properties worth keeping
                    if (actorState.Properties.Any())
                    {
                        newActorStates[a.Id] = actorState;
                    }
                }

                // Serializing at each frame to make sure we capture the state at each step.
                // Otherwise, since we're not cloning objects at each step, we'd serialize only the most recent set of data
                if (deletedActorStateIds.Any() || newActorStates.Any())
                {
                    frameJson.Add(serializer.Serialize(new { Time = f.Time, DeletedActorIds = deletedActorStateIds, ActorUpdates = newActorStates.Values }));
                }
            }
            return "[" + string.Join(",", frameJson) + "]";*/

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
            serializer.RegisterConverters(new List<JavaScriptConverter>() { new ActorStatePropertyConverter() });

            //Dictionary<int, ActorStateJson> actorStates = new Dictionary<int, ActorStateJson>();
            var frameJson = new List<string>();
            foreach (var f in replay.Frames.Where(x => x.ActorStates.Count > 0))
            {
                List<Int32> deletedActorStateIds = new List<int>();
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
