using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueReplayParser.Serializers
{
    public static class JsonWriterExtensions
    {
        public static void WriteKeyValue(this JsonWriter writer, string key, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WritePropertyName(key);
            serializer.Serialize(writer, value);
        }
    }
}
