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
        IDictionary<UInt32, RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson> _existingActorStates;

        public FrameJsonConverter(bool raw)
        {
            _raw = raw;
            _existingActorStates = new Dictionary<UInt32, RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson>();
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
            List<UInt32> deletedActorStateIds = new List<UInt32>();
            Dictionary<UInt32, ActorStateJson> updatedActorStates = new Dictionary<UInt32, ActorStateJson>();

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.Deleted))
            {
                _existingActorStates.Remove(a.Id);
                deletedActorStateIds.Add(a.Id);
            }

            // It doesn't make a lot of sense to keep new and updated actor data separate
            // since every "new" actor has a corresponding "update" with the actual properties.
            // So for the "pretty" JSON I'm putting them all in the "ActorUpdates" property.
            // New actors will have type, class, and initial position data in addition to all of their properties.
            
            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.New))
            {
                // Skip anything thats not truly new. Dont want keyframes in our json (for now)
                // (If there are property changes for a "new" actor we already have, they'll be caught below)
                if (!_existingActorStates.ContainsKey(a.Id))
                {
                    var actorState = new RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson();
                    actorState.Id = a.Id;
                    actorState.UnknownBit = a.Unknown1;
                    actorState.TypeName = a.TypeName;
                    actorState.ClassName = a.ClassName;
                    actorState.InitialPosition = a.Position;

                    _existingActorStates[a.Id] = actorState;
                    updatedActorStates[a.Id] = actorState;
                }
            }

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.Existing))
            {
                var existingActorState = _existingActorStates[a.Id];
                RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson actorState = null;
                if (updatedActorStates.ContainsKey(a.Id))
                {
                    // Actor that's new as of this frame. Use the object we created above.

                    actorState = updatedActorStates[a.Id];
                }
                else
                {
                    // Existing actor. Start a new state object to track changes

                    actorState = new RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson();
                    actorState.Id = a.Id;

                    // Maybe skip some of the following for existing actors
                    //actorState.UnknownBit = existingActorState.UnknownBit;
                    //actorState.TypeName = existingActorState.TypeName;
                    //actorState.ClassName = existingActorState.ClassName;
                    //actorState.InitialPosition = existingActorState.InitialPosition;
                }

                foreach (var p in a.Properties.Values)
                {
                    var property = new RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateProperty
                    {
                        Id = p.PropertyId,
                        Name = p.PropertyName,
                        Data = p.Data
                    };

                    ActorStateProperty existingProperty;
                    if (!existingActorState.Properties.TryGetValue(property.Id, out existingProperty))
                    {
                        // new property

                        actorState.Properties[property.Id] = property;
                        existingActorState.Properties[property.Id] = property;
                    }
                    else
                    {
                        // Existing property.
                        if (/* property.Name == "TAGame.Ball_TA:HitTeamNum" // Keep "Event" properties. TODO: Check if keyframes have this no matter what
                            || property.Name.Contains("Music") // Kind of guessing at some of these event properties. We'll see how they turn out.
                            || property.Name.Contains("Sound")
                            || property.Name.Contains("Event")
                            || */ !existingProperty.IsDeepEqual(property))  // Only keep if it is truly different
                        {
                            actorState.Properties.Add(property.Id, property);

                            // replace the property in our main set of data, so we have it for the next frame
                            existingActorState.Properties[property.Id] = property;
                        }
                    }
                }

                // Dont keep this state if we havent found any properties worth keeping
                if (actorState.Properties.Any())
                {
                    updatedActorStates[a.Id] = actorState;
                }
            }

            if (deletedActorStateIds.Any() || updatedActorStates.Any())
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                result["Time"] = frame.Time;
                result["Delta"] = frame.Delta;
                result["DeletedActorIds"] = deletedActorStateIds;
                result["ActorUpdates"] = updatedActorStates.Values;
                return result;
            }
            else
            {
                return null;
            }
        }

        private IDictionary<string, object> SerializeRaw(Frame frame, JavaScriptSerializer serializer)
        {
            List<UInt32> deletedActorStateIds = new List<UInt32>();
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
                newActorStates.Add(actorState);
            }

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.Existing))
            {
                var actorState = new ActorStateJson();
                actorState.Id = a.Id;

                foreach (var p in a.Properties)
                {
                    actorState.Properties[p.Key] = new ActorStateProperty { Id = p.Value.PropertyId, Name = p.Value.PropertyName, Data = p.Value.Data };
                }

                updatedActorStates.Add(actorState);
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["Time"] = frame.Time;
            result["Delta"] = frame.Delta;
            result["DeletedActorIds"] = deletedActorStateIds;
            result["NewActors"] = newActorStates;
            result["UpdatedActors"] = updatedActorStates;
            return result;
        }

    }
}
