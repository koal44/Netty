using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Netryoshka.Json
{
    /// <summary>
    /// Handles deserialization of a JSON token that can be a single object, a string, or an array of objects.
    /// Transforms the token into a list of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize, which must have a parameterless ctor and potentially a 'FallbackString' property.</typeparam>
    /// <remarks>
    /// This converter is designed to work around Wireshark's JSON output that sometimes includes duplicate keys.
    /// If a string token is encountered, it is assigned to the 'FallbackString' property of a new instance of <typeparamref name="T"/>.
    /// </remarks>
    public class SingleToListConverter<T> : JsonConverter<List<T>?>
        where T: new()
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
        /// <exception cref="JsonException">Thrown when 'FallbackString' property is not found or is not writable.</exception>
        /// <remarks>
        /// This method serves as an unusual workaround for cases where the JSON data 
        /// from Wireshark returns a plain string instead of an expected JSON object.
        /// </remarks>
        public T FallbackDeserializeFromString(string? str)
        {
            if (str == null) throw new JsonException("A null string is not valid for FallbackString.");

            var instance = new T();
            var fallbackProperty = typeof(T).GetProperty("FallbackString")
                ?? throw new JsonException($"Type {typeof(T).Name} does not have a 'FallbackString' property.");

            if (!fallbackProperty.CanWrite || fallbackProperty.PropertyType != typeof(string))
            {
                throw new JsonException($"Type {typeof(T).Name} does not have a writable 'FallbackString' property of type string.");
            }

            fallbackProperty.SetValue(instance, str, null);
            return instance;
        }
    }
}
