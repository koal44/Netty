using Newtonsoft.Json;
using System;

namespace Netryoshka.Json
{
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
}
