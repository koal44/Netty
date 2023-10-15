using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Netryoshka.Json
{
    /// <summary>
    /// A JSON converter for deserializing into a dict with string keys and values of type <typeparamref name="T"/>. 
    /// Tailored for non-standard JSON, like Wireshark outputs with repeated keys or unusual patterns.
    /// </summary>
    /// <remarks>
    /// The converter expects the JSON to be structured as a series of key-instance pairs where each key 
    /// is represented by a property with a name matching <see cref="KeyPropertyName"/>, and each instance 
    /// is represented by a subsequent property with a name matching <see cref="ValuePropertyName"/>.
    /// Derived types should provide specific property names by overriding the respective virtual properties.
    /// </remarks>
    public class KeyedPairToDictConverter<T> : JsonConverter<Dictionary<string, T>?>
    {
        /// <summary>
        /// Deserializes the JSON into a dictionary with string keys and values of type <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="JsonException">Thrown if the JSON doesn't match the expected format.</exception>
        public override Dictionary<string, T> ReadJson(JsonReader reader, Type objectType, Dictionary<string, T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // the json is expected to be a series of key-instance pairs where the key is a
            // string and the instance is a json object of type T

            var dict = new Dictionary<string, T>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new JsonException($"Expected a property name token, found {reader.TokenType}.");
                if (reader.Value?.ToString() != KeyPropertyName)
                    throw new JsonException($"Expected a property name token matching '{KeyPropertyName}', but found '{reader.Value?.ToString()}' instead.");

                reader.Read();
                if (reader.TokenType != JsonToken.String)
                    throw new JsonException($"Expected a string token for the key, found {reader.TokenType} instead.");
                var key = reader.Value?.ToString()
                    ?? throw new JsonException("Null key encountered where a string was expected.");

                reader.Read();
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new JsonException($"Expected a property name token, found {reader.TokenType}.");
                if (reader.Value?.ToString() != ValuePropertyName)
                    throw new JsonException($"Expected a property name token matching '{ValuePropertyName}', but found '{reader.Value?.ToString()}' instead.");

                reader.Read();
                var value = serializer.Deserialize<T>(reader)
                    ?? throw new JsonException("Unexpected null instance.");

                dict.Add(key, value);
            }

            return dict;
        }

        public override void WriteJson(JsonWriter writer, Dictionary<string, T>? value, JsonSerializer serializer)
        {
            throw new NotSupportedException("Unnecessary because CanWrite is false. The type will skip the converter.");
        }

        public override bool CanWrite => false;

        public virtual string KeyPropertyName => throw new NotImplementedException();
        public virtual string ValuePropertyName => throw new NotImplementedException();
    }
}
