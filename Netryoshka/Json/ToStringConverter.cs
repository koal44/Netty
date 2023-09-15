using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Netryoshka.Json
{
    public class ToStringConverter : JsonConverter
    {
        private readonly HashSet<Type> _supportedTypes;

        public ToStringConverter(IEnumerable<Type> supportedTypes)
        {
            _supportedTypes = new HashSet<Type>(supportedTypes);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value}");
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("This converter is intended for one-way serialization. Deserialization back to the original object type is not supported.");
        }

        public override bool CanConvert(Type objectType) => _supportedTypes.Contains(objectType);
        public override bool CanRead => false;
    }



}
