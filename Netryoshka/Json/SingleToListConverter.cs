using Newtonsoft.Json;
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
        where T : IFallbackString, new()
    {
        public override List<T>? ReadJson(JsonReader reader, Type objectType, List<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = existingValue ?? new List<T>();

            switch (reader.TokenType)
            {
                case JsonToken.StartArray:
                    reader.Read(); // consume the StartArray token

                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        var arrayValue = serializer.Deserialize<T>(reader)
                            ?? throw new JsonException($"Error converting token at path {reader.Path}");
                        list.Add(arrayValue);
                        reader.Read(); // Move to the next array item or EndArray
                    }
                    break;

                case JsonToken.String:
                    var stringVal = reader.Value as string;
                    var handledVal = FallbackDeserializeFromString(stringVal)
                        ?? throw new JsonException($"Unexpected null instance from string '{stringVal}' at path {reader.Path}");
                    list.Add(handledVal);
                    break;

                case JsonToken.StartObject:
                    var objVal = serializer.Deserialize<T>(reader)
                        ?? throw new JsonException($"Failed to deserialize an object of type {objectType.Name} at path {reader.Path}");
                    list.Add(objVal);
                    break;

                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType} at path {reader.Path}");
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
        public T FallbackDeserializeFromString(string? str)
        {
            return new T { FallbackString = str };
        }
    }

    public interface IFallbackString
    {
        string? FallbackString { get; set; }
    }
}
