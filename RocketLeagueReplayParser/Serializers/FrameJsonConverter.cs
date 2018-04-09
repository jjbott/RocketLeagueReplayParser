using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActorStateJson = RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson;
using Newtonsoft.Json;
using KellermanSoftware.CompareNetObjects;

namespace RocketLeagueReplayParser.Serializers
{ 
    public class FrameJsonConverter : JsonConverter
    {
        private readonly bool _raw;
        private readonly IDictionary<UInt32, RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson> _existingActorStates;

        private static CompareLogic DeepCompare = new CompareLogic();

        public FrameJsonConverter(bool raw)
        {
            _raw = raw;
            _existingActorStates = new Dictionary<UInt32, RocketLeagueReplayParser.Serializers.JsonSerializer.ActorStateJson>();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Frame);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            Frame frame = (Frame)value;

            if ( _raw)
            {
                SerializeRaw(writer, frame, serializer);
            }
            else
            {
                SerializePretty(writer, frame, serializer);
            }
        }

        private void SerializePretty(JsonWriter writer, Frame frame, Newtonsoft.Json.JsonSerializer serializer)
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
                    actorState.NameId = a.NameId;
                    actorState.UnknownBit = a.Unknown1;
                    actorState.TypeId = a.TypeId;
                    actorState.ClassId = a.ClassId;
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
                    ActorStateProperty existingProperty;
                    if (!existingActorState.Properties.TryGetValue(p.PropertyId, out existingProperty))
                    {
                        // new property
                        actorState.Properties[p.PropertyId] = p;
                        existingActorState.Properties[p.PropertyId] = p;
                    }
                    else
                    {
                        // Existing property.
                        var compareResult = DeepCompare.Compare(existingProperty, p);
                        if (!compareResult.AreEqual)  // Only keep if it is truly different
                        {
                            actorState.Properties.Add(p.PropertyId, p);

                            // replace the property in our main set of data, so we have it for the next frame
                            existingActorState.Properties[p.PropertyId] = p;
                        }
                    }
                }

                // Dont keep this state if we havent found any properties worth keeping
                if (actorState.Properties.Any())
                {
                    updatedActorStates[a.Id] = actorState;
                }
            }

            // If theres no property changes the frame is useless, so dont bother writing it
            if (deletedActorStateIds.Any() || updatedActorStates.Any())
            {
                writer.WriteStartObject();
                writer.WriteKeyValue("Time", frame.Time, serializer);
                writer.WriteKeyValue("Delta", frame.Delta, serializer);
                writer.WriteKeyValue("DeletedActorIds", deletedActorStateIds, serializer);
                writer.WriteKeyValue("ActorUpdates", updatedActorStates.Values, serializer);
                writer.WriteEndObject();
            }
        }

        private void SerializeRaw(JsonWriter writer, Frame frame, Newtonsoft.Json.JsonSerializer serializer)
        {
            List<UInt32> deletedActorStateIds = new List<UInt32>();
            List<ActorStateJson> newActorStates = new List<ActorStateJson>();
            List<ActorStateJson> updatedActorStates = new List<ActorStateJson>();

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.Deleted))
            {
                deletedActorStateIds.Add(a.Id);
            }

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.New))
            {
                var actorState = new ActorStateJson();
                actorState.Id = a.Id;
                actorState.NameId = a.NameId;
                actorState.UnknownBit = a.Unknown1;
                actorState.TypeId = a.TypeId;
                actorState.ClassId = a.ClassId;
                actorState.InitialPosition = a.Position;
                newActorStates.Add(actorState);
            }

            foreach (var a in frame.ActorStates.Where(x => x.State == ActorStateState.Existing))
            {
                var actorState = new ActorStateJson();
                actorState.Id = a.Id;

                foreach (var p in a.Properties)
                {
                    actorState.Properties[p.Key] = p.Value;
                }

                updatedActorStates.Add(actorState);
            }

            writer.WriteStartObject();
            writer.WriteKeyValue("Time", frame.Time, serializer);
            writer.WriteKeyValue("Delta", frame.Delta, serializer);
            writer.WriteKeyValue("DeletedActorIds", deletedActorStateIds, serializer);
            writer.WriteKeyValue("NewActors", newActorStates, serializer);
            writer.WriteKeyValue("UpdatedActors", updatedActorStates, serializer);
            writer.WriteEndObject();
        }

    }
}
