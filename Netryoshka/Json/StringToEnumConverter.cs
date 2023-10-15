using Newtonsoft.Json;
using System;

namespace Netryoshka.Json
{
    public class StringToEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct
    {
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
            // Write the enum instance as a string for the different formats
        }
    }
}
