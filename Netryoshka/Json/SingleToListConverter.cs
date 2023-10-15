using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Netryoshka.Json
{
    /// <summary>
    /// Handles deserialization of a JSON token that can either be a single object or a string.
    /// Transforms the token into a list of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <remarks>
    /// The need for this converter arose from Wireshark's use of duplicate keys within JSON objects,
    /// a scenario not directly supported by Newtonsoft.Json's library.
    /// In cases where the token is a string, a custom handler in the derived class is necessary.
    /// </remarks>
    public class SingleToListConverter<T> : JsonConverter<List<T>?>
    {
        public override List<T> ReadJson(JsonReader reader, Type objectType, List<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);

            if (jToken.Type == JTokenType.Array)
            {
                if (existingValue != null) throw new JsonException("Unexpected array token within existing list.");
                return jToken.ToObject<List<T>>(serializer)
                    ?? throw new JsonException("Unexpected null instance.");
            }

            var list = existingValue ?? new List<T>();

            T? value = jToken.Type switch
            {
                JTokenType.Object => jToken.ToObject<T>(serializer),
                JTokenType.String => HandleStringToken(jToken.Value<string>()),
                _ => throw new JsonException($"Unexpected token type: {jToken.Type}")
            };

            if (value is T nonNullValue)
            {
                list.Add(nonNullValue);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, List<T>? value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
        }

        public override bool CanWrite => false;

        /// <summary>
        /// Handles a JSON string token and returns an object of type T.
        /// </summary>
        /// <param name="str">The JSON string token.</param>
        /// <returns>An object of type T that represents the given string token.</returns>
        /// <remarks>
        /// This method serves as an unusual workaround for cases where the JSON data 
        /// from Wireshark returns a plain string instead of an expected JSON object.
        /// </remarks>
        public virtual T HandleStringToken(string? str)
        {
            throw new NotImplementedException("Handling of string tokens is not implemented in the base class.");
        }
    }
}
