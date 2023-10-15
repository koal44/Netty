using Newtonsoft.Json.Serialization;

namespace Netryoshka.Extensions
{
    public static class JsonPropertyExtensions
    {
        public static object? GetResolvedDefaultValue(this JsonProperty jsonProperty)
        {
            var propertyType = jsonProperty.PropertyType;
            if (propertyType == null)
            {
                return null;
            }

            if (jsonProperty.DefaultValue != null)
            {
                return jsonProperty.DefaultValue;
            }

            return JsonUtils.GetDefaultValue(propertyType);
        }


    }
}
