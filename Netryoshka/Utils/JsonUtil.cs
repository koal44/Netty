using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace Netty.Utils
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



        public static JToken? DeserializeAndCombineDuplicates(JsonTextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            if (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read()) return null;
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                if (!reader.Read()) return null;

                var obj = new JObject();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    var propName = reader.Value?.ToString();
                    if (string.IsNullOrEmpty(propName)) return null;

                    if (!reader.Read()) return null;

                    var newValue = DeserializeAndCombineDuplicates(reader);
                    if (newValue == null) return null;

                    var existingValue = obj[propName];
                    if (existingValue == null)
                    {
                        obj.Add(new JProperty(propName, newValue));
                    }
                    else if (existingValue.Type == JTokenType.Array)
                    {
                        CombineWithArray(existingValue.Value<JArray>(), newValue);
                    }
                    else // Convert existing non-array property nullableDate to an array
                    {
                        if (existingValue.Parent is not JProperty prop) return null;

                        var array = new JArray();
                        prop.Value = array;
                        array.Add(existingValue);
                        CombineWithArray(array, newValue);
                    }

                    if (!reader.Read()) return null;
                }
                return obj;
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                if (!reader.Read()) return null;

                var array = new JArray();
                while (reader.TokenType != JsonToken.EndArray)
                {
                    var newValue = DeserializeAndCombineDuplicates(reader);
                    if (newValue == null) return null;

                    array.Add(newValue);
                    if (!reader.Read()) return null;
                }
                return array;
            }

            return new JValue(reader.Value);
        }

        // Helper function, also made null-safe
        private static void CombineWithArray(JArray? array, JToken? newValue)
        {
            if (array == null || newValue == null) return;

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
