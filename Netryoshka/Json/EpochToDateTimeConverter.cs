using Newtonsoft.Json;
using System;

namespace Netryoshka.Json
{
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
}
