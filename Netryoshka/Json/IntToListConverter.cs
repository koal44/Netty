using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Netryoshka.Json
{
    /// <summary>
    /// Converts integer values sourced from a list of duplicate-keyed properties in JSON, 
    /// handling both direct integers and their string representations.
    /// </summary>
    public class IntToListConverter : JsonConverter<List<int>?>
    {
        public override List<int>? ReadJson(JsonReader reader, Type objectType, List<int>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);

            int value = jToken.Type switch
            {
                JTokenType.Integer => jToken.Value<int>(),
                JTokenType.String => int.TryParse(jToken.Value<string>(), out int result)
                        ? result
                        : throw new JsonException($"Failed to convert '{jToken.Value<string>()}' to integer."),
                _ => throw new JsonException($"Unexpected token type: {jToken.Type}")
            };

            var list = existingValue ?? new List<int>();
            list.Add(value);
            return list;
        }

        public override void WriteJson(JsonWriter writer, List<int>? value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
        }

        public override bool CanWrite => false;
    }
}
