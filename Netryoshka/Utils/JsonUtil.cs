using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

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
        /// Deserializes a JSON stream from a JsonTextReader and combines duplicate keys within objects into arrays.
        /// </summary>
        /// <param name="reader">The JsonTextReader to read from.</param>
        /// <returns>A JToken representing the JSON data structure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the reader is null.</exception>
        public static JToken DeserializeAndCombineDuplicateKeys(JsonTextReader reader)
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
        /// <param name="reader">The JsonTextReader to read from.</param>
        /// <returns>A JObject.</returns>
        private static JObject DeserializeObject(JsonTextReader reader)
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
        /// <param name="reader">The JsonTextReader to read from.</param>
        /// <returns>A JArray.</returns>
        private static JArray DeserializeArray(JsonTextReader reader)
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
                return reader.Value is string hex
                     ? Convert.ToInt32(hex, 16)
                     : null;
            }

            public override void WriteJson(JsonWriter writer, int? numOpt, JsonSerializer serializer)
            {
                writer.WriteValue(numOpt is int num ? $"0x{num:X}" : null);
            }
        }

    }
}
