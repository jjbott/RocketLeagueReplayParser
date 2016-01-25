using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using DeepEqual.Syntax;
using ActorStateJson = RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson;
using ActorStateProperty = RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateProperty;

namespace RocketLeagueReplayParser.Serializers
{
    public class FrameJsonConverter : JavaScriptConverter
    {
        bool _raw;
        IDictionary<int, RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson> _existingActorStates;

        public FrameJsonConverter(bool raw)
        {
            _raw = raw;
            _existingActorStates = new Dictionary<int, RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson>();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new[] { typeof(Frame) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            Frame frame = (Frame)obj;

            if ( _raw)
            {
                return SerializeRaw(frame, serializer);
            }
            else
            {
                return SerializePretty(frame, serializer);
            }
        }

        private IDictionary<string, object> SerializePretty(Frame frame, JavaScriptSerializer serializer)
        {
            List<Int32> deletedActorStateIds = new List<int>();
            Dictionary<int, RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson> newActorStates = new Dictionary<int, RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson>();

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.Deleted))
            {
                _existingActorStates.Remove(a.Id);
                deletedActorStateIds.Add(a.Id);
            }

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.New))
            {
                // Skip anything thats not truly new. Dont want keyframes in our json (for now)
                if (!_existingActorStates.ContainsKey(a.Id))
                {
                    var actorState = new RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson();
                    actorState.Id = a.Id;
                    actorState.UnknownBit = a.Unknown1;
                    actorState.TypeName = a.TypeName;
                    actorState.ClassName = a.ClassName;
                    actorState.InitialPosition = a.Position;
                    actorState.Properties = new List<RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateProperty>();

                    _existingActorStates[a.Id] = actorState;
                }
            }

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.Existing))
            {
                var existingActorState = _existingActorStates[a.Id];
                RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson actorState = null;
                if (newActorStates.ContainsKey(a.Id))
                {
                    // new actor
                    actorState = newActorStates[a.Id];
                }
                else
                {
                    // Existing actor. Start a new state object to track changes

                    actorState = new RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson();
                    actorState.Id = a.Id;
                    actorState.Properties = new List<RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateProperty>();

                    // Maybe skip some of the following for existing actors
                    //actorState.UnknownBit = existingActorState.UnknownBit;
                    //actorState.TypeName = existingActorState.TypeName;
                    //actorState.ClassName = existingActorState.ClassName;
                    //actorState.InitialPosition = existingActorState.InitialPosition;
                }

                foreach (var p in a.Properties)
                {
                    var property = new RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateProperty
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

            if (deletedActorStateIds.Any() || newActorStates.Any())
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                result["Time"] = frame.Time;
                result["DeletedActorIds"] = deletedActorStateIds;
                result["ActorUpdates"] = newActorStates.Values;
                return result;
            }
            else
            {
                return null;
            }
        }

        private IDictionary<string, object> SerializeRaw(Frame frame, JavaScriptSerializer serializer)
        {
            List<Int32> deletedActorStateIds = new List<int>();
            List<ActorStateJson> newActorStates = new List<ActorStateJson>();
            List<ActorStateJson> updatedActorStates = new List<ActorStateJson>();

            Dictionary<int, ActorStateJson> actor = new Dictionary<int, ActorStateJson>();

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.Deleted))
            {
                deletedActorStateIds.Add(a.Id);
            }

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.New))
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

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.Existing))
            {
                var actorState = new ActorStateJson();
                actorState.Id = a.Id;
                actorState.Properties = new List<ActorStateProperty>();

                actorState.Properties = a.Properties.Select(p => new ActorStateProperty { Name = p.PropertyName, Data = p.Data }).ToList();

                updatedActorStates.Add(actorState);
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["Time"] = frame.Time;
            result["DeletedActorIds"] = deletedActorStateIds;
            result["NewActors"] = newActorStates;
            result["UpdatedActors"] = updatedActorStates;
            return result;
        }

    }
}
