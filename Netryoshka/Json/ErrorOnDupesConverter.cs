using Netryoshka.Extensions;
using Netryoshka.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Netryoshka.Json
{
    /// <summary>
    /// A JSON converter that checks for duplicate properties in the deserialization process.
    /// Certain properties that handle duplicate keys can be ignored by passing their types to the constructor.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    public class ErrorOnDupesConverter<T> : JsonConverter<T?>
    {
        private List<string>? _propertiesToConsider;
        private List<string>? _propertiesWithConverters;

        private List<string> PropertiesToConsider => _propertiesToConsider
            ??= typeof(T).GetProperties()
                .Select(prop => prop.GetCustomAttribute<JsonPropertyAttribute>())
                .Where(attr => attr != null && attr.PropertyName != null)
                .Select(attr => attr!.PropertyName!)
                .ToList();

        // assume that properties with converters are responsible for handling duplicate keys
        private List<string> PropertiesWithConverters => _propertiesWithConverters
            ??= typeof(T).GetProperties()
                .Where(prop => prop.GetCustomAttributes(typeof(JsonConverterAttribute), true).Any())
                .Select(prop => prop.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? prop.Name)
                .ToList();

        public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            //return serializer.Deserialize<T>(reader);
            //serializer.Populate(reader, instance);
            //return instance;

            T instance = existingValue ?? Create(serializer) ?? throw new JsonSerializationException("No object created.");

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException("Expected StartObject token.");
            }

            var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(typeof(T));

            var handledProperties = new HashSet<string>();

            var dynamicProperties = contract.Properties
                .Where(p => p?.PropertyName != null && p.PropertyName.StartsWith("REGEX_"))
                .ToList();

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.Comment) continue;

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new JsonSerializationException("Expected PropertyName token.");

                var propertyName = reader.Value?.ToString()
                    ?? throw new JsonSerializationException("PropertyName not found.");

                if (handledProperties.Contains(propertyName) && !PropertiesWithConverters.Contains(propertyName))
                {
                    throw new JsonSerializationException($"Duplicate property '{propertyName}' found.");
                }

                bool isDynamicProperty = false;

                JsonProperty? property = contract.Properties.GetClosestMatchProperty(propertyName);
                if (property == null)
                {
                    property = dynamicProperties.FirstOrDefault(p => p?.PropertyName != null && Regex.IsMatch(propertyName, p.PropertyName[6..]));
                    
                    if (property != null)
                    {
                        isDynamicProperty = true;
                        HandleDynamicProperty(propertyName, property, instance);
                    }
                }

                if (property == null || property.Ignored || (!isDynamicProperty && !PropertiesToConsider.Contains(propertyName)))
                {
                    reader.Skip();
                    continue;
                }

                handledProperties.Add(propertyName);

                var propertyContract = property.PropertyType == null
                    ? null
                    : serializer.ContractResolver.ResolveContract(property.PropertyType);

                JsonConverter? propertyConverter = serializer.GetConverter(propertyContract, property.Converter, contract, null);

                if (!reader.ReadForType(propertyContract, propertyConverter != null))
                {
                    throw new JsonSerializationException($"Unexpected end when setting {propertyName}'s value.");
                }

                JsonNetUtils.SetPropertyValue(property, propertyConverter, contract, null, reader, instance, serializer);
            }

            contract.InvokeOnDeserialized(instance, serializer.Context);

            return instance;
        }

        protected virtual void HandleDynamicProperty(string propertyName, JsonProperty property, T instance) { }

        public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
        {
            throw new Exception("Unnecessary because CanWrite is false.");
        }

        public override bool CanWrite => false;

        protected virtual T Create(JsonSerializer serializer)
        {
            // return Activator.CreateInstance<T>();

            if (serializer.ContractResolver.ResolveContract(typeof(T)) is not JsonObjectContract contract
                || contract.DefaultCreator == null)
                throw new JsonSerializationException($"No parameterless constructor defined for {typeof(T)}.");

            return (T)contract.DefaultCreator();
        }

        
    }
}
