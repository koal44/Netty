using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Netryoshka.Utils
{
    public static class JsonUtil
    {
        public static bool JObjToBool(JToken? token)
        {
            string? value = token?.ToString();
            if (value == null) return false;

            if (value != "0" && value != "1")
            {
                throw new JsonSerializationException($"Unexpected nullableDate {value} when attempting to deserialize to boolean.");
            }

            return value == "1";
        }

        public static T? JObjToValue<T>(JToken? token) where T : struct
        {
            return token == null ? (T?)null : token.Value<T>();
        }

        public static string? JObjToString(JToken? token)
        {
            return token?.Value<string>();
        }


        /// <summary>
        /// Deserializes a JSON stream from a JsonReader and combines duplicate keys within objects into arrays.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <returns>A JToken representing the JSON data structure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the reader is null.</exception>
        public static JToken DeserializeAndCombineDuplicateKeys(JsonReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            // Make sure the reader is advanced before starting deserialization
            if (reader.TokenType == JsonToken.None && !reader.Read())
                throw new InvalidOperationException("Could not read from JsonTextReader.");

            return reader.TokenType switch
            {
                JsonToken.StartObject => DeserializeObject(reader),
                JsonToken.StartArray => DeserializeArray(reader),
                _ => new JValue(reader.Value)
            };
        }

        /// <summary>
        /// Deserialize JSON object from a reader.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <returns>A JObject.</returns>
        private static JObject DeserializeObject(JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Reader must be at the start of an object", nameof(reader));

            var obj = new JObject();
            while (true)
            {
                if (!reader.Read())
                    throw new InvalidOperationException("JSON appears to be truncated.");

                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new InvalidOperationException("Expected property name but got " + reader.TokenType);

                var propName = reader.Value?.ToString()
                    ?? throw new InvalidOperationException("Unexpected null property name.");

                if (!reader.Read())
                    throw new InvalidOperationException("JSON appears to be truncated.");

                var propValue = DeserializeAndCombineDuplicateKeys(reader);
                CombinePropertyValue(obj, propName, propValue);
            }

            return obj;
        }

        /// <summary>
        /// Deserialize JSON array from a reader.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <returns>A JArray.</returns>
        private static JArray DeserializeArray(JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartArray)
                throw new ArgumentException("Reader must be at the start of an array", nameof(reader));

            if (!reader.Read()) throw new InvalidOperationException("JSON appears to be truncated.");

            var array = new JArray();
            while (reader.TokenType != JsonToken.EndArray)
            {
                var newValue = DeserializeAndCombineDuplicateKeys(reader);
                array.Add(newValue);

                if (!reader.Read()) throw new InvalidOperationException("JSON appears to be truncated.");
            }

            return array;
        }

        private static void CombinePropertyValue(JObject obj, string propName, JToken newValue)
        {
            var existingValue = obj[propName];
            if (existingValue == null)
            {
                obj.Add(new JProperty(propName, newValue));
            }
            else if (existingValue.Type == JTokenType.Array)
            {
                var existingArray = existingValue.Value<JArray>()
                    ?? throw new InvalidOperationException("Expected a JArray but got null.");
                CombineWithArray(existingArray, newValue);
            }
            else if (existingValue.Parent is JProperty prop)
            {
                var array = new JArray(existingValue);
                prop.Value = array;
                CombineWithArray(array, newValue);
            }
        }

        private static void CombineWithArray(JArray array, JToken newValue)
        {
            if (newValue.Type == JTokenType.Array)
            {
                foreach (var child in newValue.Children())
                {
                    array.Add(child);
                }
            }
            else
            {
                array.Add(newValue);
            }
        }


        /// <summary>
        /// A custom JSON converter that converts string "0" or "1" to boolean false or true, respectively.
        /// This class is specific to Newtonsoft.Json.
        /// </summary>
        public class BitToBoolConverter : JsonConverter<bool>
        {
            /// <summary>
            /// Deserializes the JSON string nullableDate to a boolean.
            /// </summary>
            public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                // Read the JSON nullableDate and convert it to a string
                var value = reader.Value?.ToString();

                // Check if the string is either "0" or "1"
                if (value == null || (value != "0" && value != "1"))
                {
                    throw new JsonSerializationException($"Unexpected nullableDate {value} when attempting to deserialize to boolean.");
                }

                // Return true if the string is "1", false otherwise
                return value.Equals("1");
            }

            /// <summary>
            /// Serializes the boolean nullableDate to a JSON string "0" or "1".
            /// </summary>
            public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
            {
                // Convert the boolean to its string representation "0" or "1"
                writer.WriteValue(value ? "1" : "0");
            }
        }


        public class EpochToDateTimeConverter : JsonConverter<DateTime?>
        {
            private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public override DateTime? ReadJson(JsonReader reader, Type objectType, DateTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return double.TryParse(reader.Value?.ToString(), out var epochTime)
                    ? UnixEpoch.AddSeconds(epochTime).ToLocalTime()
                    : null;
            }

            public override void WriteJson(JsonWriter writer, DateTime? dateTimeOpt, JsonSerializer serializer)
            {
                writer.WriteValue(dateTimeOpt is DateTime dt ? (dt - UnixEpoch).TotalSeconds.ToString() : null);
            }

        }


        public class HexToIntConverter : JsonConverter<int?>
        {
            public override int? ReadJson(JsonReader reader, Type objectType, int? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.Value is not string hex)
                    throw new JsonException($"Failed to convert \"{reader.Value}\" to int. Expected a hex string.");

                if (string.IsNullOrWhiteSpace(hex)) return null;

                try
                {
                    return Convert.ToInt32(hex, 16);
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException)
                {
                    throw new JsonException($"Failed to convert \"{hex}\" to integer.", ex);
                }
            }

            public override void WriteJson(JsonWriter writer, int? numOpt, JsonSerializer serializer)
            {
                writer.WriteValue(numOpt is int num ? $"0x{num:X}" : null);
            }
        }

        public class StringToEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct
        {
            //public enum NumericStringFormat
            //{
            //    Decimal,
            //    Hex
            //}

            public override TEnum ReadJson(JsonReader reader, Type objectType, TEnum existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType != JsonToken.String)
                    throw new JsonException($"Expected a string token, found {reader.TokenType}.");

                string enumString = reader.Value?.ToString() ?? string.Empty;

                try
                {
                    return (enumString.Length > 2 ? enumString[..2] : "") switch
                    {
                        "0x" => (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt32(enumString[2..], 16)),
                        "0o" => (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt32(enumString[2..], 8)),
                        "0b" => (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt32(enumString[2..], 2)),
                        _ => Enum.TryParse(enumString, true, out TEnum result)
                            ? result
                            : throw new JsonException($"Failed to convert '{enumString}' to {typeof(TEnum).Name}.")
                    };
                }
                catch (Exception)
                {
                    throw new JsonException($"Failed to convert '{enumString}' to {typeof(TEnum).Name}.");
                }

            }

            public override bool CanWrite => false;

            public override void WriteJson(JsonWriter writer, TEnum value, JsonSerializer serializer)
            {
                // Write the enum value as a string for the different formats
            }
        }


        /// <summary>
        /// Handles deserialization of a JSON token that can be an array, a single object, or a string.
        /// Transforms the token into a list of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <remarks>
        /// The need for this converter arose from Wireshark's use of duplicate keys within JSON objects,
        /// a scenario not directly supported by Newtonsoft.Json's library.
        /// A string token will require coding a custom handler in the derived class.
        /// </remarks>
        public class AdaptiveJsonArrayConverter<T> : JsonConverter<List<T>?>
        {
            public override List<T>? ReadJson(JsonReader reader, Type objectType, List<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var jToken = JToken.ReadFrom(reader);

                return jToken.Type switch
                {
                    JTokenType.Array => jToken.ToObject<List<T>>(serializer)
                        ?? new List<T>(),
                    JTokenType.Object => jToken.ToObject<T>(serializer) is { } singleObject
                        ? new List<T> { singleObject }
                        : new List<T>(),
                    JTokenType.String => new List<T> { HandleStringToken(jToken.Value<string>()) },
                    _ => throw new JsonException($"Unexpected token type: {jToken.Type}")
                };
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


        public class AdaptiveJsonDictionaryConverter<T> : JsonConverter<Dictionary<string, T>?>
        {
            public override Dictionary<string, T> ReadJson(JsonReader reader, Type objectType, Dictionary<string, T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                // the json is expected to be a series of key-value pairs where the key is a
                // string and the value is a json object of type T

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
                        ?? throw new JsonException("Unexpected null value.");

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

        public class CustomTraceWriter : ITraceWriter
        {
            public TraceLevel LevelFilter => TraceLevel.Verbose;

            public void Trace(TraceLevel level, string message, Exception? ex)
            {
                var exText = ex != null ? $"Exception: {ex.Message}" : string.Empty;
                var lvlText = $"Level: {level}";
                var msgText = $"Message: {message}";

                var components = new[] { lvlText, msgText, exText }
                                 .Where(s => !string.IsNullOrEmpty(s));

                Debug.WriteLine(string.Join(", ", components));
            }
        }

    }
}
