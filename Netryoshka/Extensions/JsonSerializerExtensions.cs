using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Netryoshka
{
    public static class JsonSerializerExtensions
    {
        public static JsonConverter? GetConverter(this JsonSerializer serializer, JsonContract? contract, JsonConverter? memberConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty)
        {
            JsonConverter? converter = null;

            if (memberConverter != null)
            {
                // member attribute converter
                converter = memberConverter;
            }
            else if (containerProperty?.ItemConverter != null)
            {
                converter = containerProperty.ItemConverter;
            }
            else if (containerContract?.ItemConverter != null)
            {
                converter = containerContract.ItemConverter;
            }
            else if (contract != null)
            {
                if (contract.Converter != null)
                {
                    // class attribute converter
                    converter = contract.Converter;
                }
                else if (serializer.GetMatchingConverter(contract.UnderlyingType) is JsonConverter matchingConverter)
                {
                    // passed in converters
                    converter = matchingConverter;
                }
                else if (contract.InternalConverter != null)
                {
                    // internally specified converter
                    converter = contract.InternalConverter;
                }
            }
            return converter;
        }

        public static JsonConverter? GetMatchingConverter(this JsonSerializer serializer, Type type)
        {
            if (serializer.Converters != null)
            {
                for (int i = 0; i < serializer.Converters.Count; i++)
                {
                    JsonConverter converter = serializer.Converters[i];

                    if (converter.CanConvert(type))
                    {
                        return converter;
                    }
                }
            }

            return null;
        }

        public static JsonContract GetContract(this JsonSerializer serializer, Type type)
        {
            return serializer.ContractResolver.ResolveContract(type);
        }
    }
}
