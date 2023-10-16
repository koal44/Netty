using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Netryoshka.Json
{
    public class StringToListConverter : JsonConverter<List<string>?>
    {
        public override List<string>? ReadJson(JsonReader reader, Type objectType, List<string>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);

            string value = jToken.Type switch
            {
                JTokenType.String => jToken.Value<string>() ?? throw new JsonException("Unexpected null string."),
                _ => throw new JsonException($"Unexpected token type: {jToken.Type}")
            };

            var list = existingValue ?? new List<string>();
            list.Add(value);
            return list;
        }

        public override void WriteJson(JsonWriter writer, List<string>? value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
        }

        public override bool CanWrite => false;
    }
}
