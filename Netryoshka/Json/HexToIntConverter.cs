using Newtonsoft.Json;
using System;

namespace Netryoshka.Json
{
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
}
