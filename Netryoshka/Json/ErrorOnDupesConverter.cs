using Netryoshka.Extensions;
using Netryoshka.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Netryoshka.Json
{
    /// <summary>
    /// A JSON converter that checks for duplicate properties in the deserialization process.
    /// Certain properties that handle duplicate keys can be ignored by passing their types to the constructor.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    public class ErrorOnDupesConverter<T> : JsonConverter<T?>
    {
        private readonly HashSet<Type> _ignoredConverters;
        private List<string>? _propertiesToConsider;
        private List<string>? _propertiesToIgnore;

        public ErrorOnDupesConverter()
        {
            _ignoredConverters = new HashSet<Type>
                {
                    typeof(KeyedPairToDictConverter<T>),
                    typeof(SingleToListConverter<T>),
                    typeof(IntToListConverter),
                };
        }

        private List<string> PropertiesToConsider => _propertiesToConsider
            ??= typeof(T).GetProperties()
                .Select(prop => prop.GetCustomAttribute<JsonPropertyAttribute>())
                .Where(attr => attr != null && attr.PropertyName != null)
                .Select(attr => attr!.PropertyName!)
                .ToList();

        private List<string> PropertiesToIgnore => _propertiesToIgnore
            ??= typeof(T).GetProperties()
                .Where(prop => prop.GetCustomAttributes(typeof(JsonConverterAttribute), true)
                    .Cast<JsonConverterAttribute>()
                    .Any(attr => _ignoredConverters.Contains(attr.ConverterType)))
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

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.Comment) continue;

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new JsonSerializationException("Expected PropertyName token.");

                var propertyName = reader.Value?.ToString()
                    ?? throw new JsonSerializationException("PropertyName not found.");

                if (handledProperties.Contains(propertyName) && !PropertiesToIgnore.Contains(propertyName))
                {
                    throw new JsonSerializationException($"Duplicate property '{propertyName}' found.");
                }

                if (PropertiesToConsider.Contains(propertyName))
                {
                    handledProperties.Add(propertyName);
                }
                else
                {
                    reader.Skip();
                    continue;
                }

                JsonProperty property = contract.Properties.GetClosestMatchProperty(propertyName)
                    ?? throw new JsonSerializationException($"Couldn't find json property for '{propertyName}'.");

                if (property == null || property.Ignored)
                {
                    reader.Read();
                    continue;
                }

                var propertyContract = property.PropertyType == null
                    ? null
                    : serializer.ContractResolver.ResolveContract(property.PropertyType);

                JsonConverter? propertyConverter = serializer.GetConverter(propertyContract, property.Converter, contract, null);

                if (!reader.ReadForType(propertyContract, propertyConverter != null))
                {
                    throw new JsonSerializationException($"Unexpected end when setting {propertyName}'s value.");
                }

                _ = SetPropertyValue(property, propertyConverter, contract, null, reader, instance, serializer);

            }

            return instance;

        }

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

        private static bool SetPropertyValue(JsonProperty property, JsonConverter? propertyConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty, JsonReader reader, object target, JsonSerializer serializer)
        {
            bool skipSettingProperty = CalculatePropertyDetails(property, ref propertyConverter, containerContract,
                containerProperty, reader, target, serializer, out bool useExistingValue, out object? currentValue,
                out JsonContract? propertyContract, out bool gottenCurrentValue, out bool ignoredValue);

            if (skipSettingProperty)
            {
                // Don't set extension data if the value was ignored
                // e.g. a null with NullValueHandling should not go in ExtensionData
                if (ignoredValue)
                {
                    return true;
                }

                return false;
            }

            object? value;

            if (propertyConverter != null && propertyConverter.CanRead)
            {
                if (!gottenCurrentValue && property.Readable)
                {
                    currentValue = property.ValueProvider!.GetValue(target);
                }

                //var propertySerializer = new JsonSerializerProxy(new JsonSerializerInternalReader(serializer));
                var propertySerializer = serializer;
                value = propertyConverter.ReadJson(reader, property.PropertyType!, currentValue, propertySerializer);
            }
            else
            {
                //var propertySerializer = new JsonSerializerInternalReader(serializer);
                //value = propertySerializer.Deserialize(reader, property.PropertyType!, false);

                //value = CreateValueInternal(reader, property.PropertyType, propertyContract, property, containerContract, containerProperty, (useExistingValue) ? currentValue : null, serializer);

                value = CreateValueUsingInternalReader(reader, property.PropertyType, propertyContract, property, containerContract, containerProperty, (useExistingValue) ? currentValue : null, serializer);
            }

            // always set the value if useExistingValue is false,
            // otherwise also set it if CreateValue returns a new value compared to the currentValue
            // this could happen because of a JsonConverter against the type
            if ((!useExistingValue || value != currentValue)
                && ShouldSetPropertyValue(property, containerContract as JsonObjectContract, value, serializer))
            {
                property.ValueProvider!.SetValue(target, value);
                property.SetIsSpecified?.Invoke(target, true);

                return true;
            }

            // the value wasn't set be JSON was populated onto the existing value
            return useExistingValue;
        }

        private static readonly Type? InternalReaderType = typeof(JsonSerializer).Assembly.GetType("Newtonsoft.Json.Serialization.JsonSerializerInternalReader");
        private static readonly MethodInfo? CreateValueInternalMethod = InternalReaderType?.GetMethod("CreateValueInternal", BindingFlags.NonPublic | BindingFlags.Instance);

        private static object? CreateValueUsingInternalReader(JsonReader reader, Type? propertyType, JsonContract? propertyContract, JsonProperty? jsonProperty, JsonContainerContract? containerContract, JsonProperty? containerProperty, object? existingValue, JsonSerializer jsonSerializer)
        {
            if (InternalReaderType == null || CreateValueInternalMethod == null)
            {
                throw new InvalidOperationException("Could not find internal type or method for 'JsonSerializerInternalReader'.");
            }

            // Create an instance of JsonSerializerInternalReader
            object internalReaderInstance = Activator.CreateInstance(InternalReaderType, new object[] { jsonSerializer })
                ?? throw new Exception("Could not create instance of internal type.");

            // Prepare the arguments for CreateValueInternal
            object?[] args = {
                reader,
                propertyType,
                propertyContract,
                jsonProperty,
                containerContract,
                containerProperty,
                existingValue
            };

            // Invoke CreateValueInternal method
            object? result = CreateValueInternalMethod.Invoke(internalReaderInstance, args);

            return result;
        }

        private static bool CalculatePropertyDetails(JsonProperty property, ref JsonConverter? propertyConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty, JsonReader reader, object target, JsonSerializer serializer, out bool useExistingValue, out object? currentValue, out JsonContract? propertyContract, out bool gottenCurrentValue, out bool ignoredValue)
        {
            currentValue = null;
            useExistingValue = false;
            propertyContract = null;
            gottenCurrentValue = false;
            ignoredValue = false;

            if (property.Ignored)
            {
                return true;
            }

            JsonToken tokenType = reader.TokenType;

            propertyContract = property.PropertyType == null
                                ? null
                                : serializer.ContractResolver.ResolveContract(property.PropertyType);

            ObjectCreationHandling objectCreationHandling =
                property.ObjectCreationHandling.GetValueOrDefault(serializer.ObjectCreationHandling);

            if ((objectCreationHandling != ObjectCreationHandling.Replace)
                && (tokenType == JsonToken.StartArray || tokenType == JsonToken.StartObject || propertyConverter != null)
                && property.Readable)
            //&& propertyContract?.ContractType != JsonContractType.Linq)
            // linq is not relevant to wireshark deserialization
            {
                currentValue = property.ValueProvider!.GetValue(target);
                gottenCurrentValue = true;

                if (currentValue != null)
                {
                    propertyContract = serializer.GetContract(currentValue.GetType());

                    useExistingValue = !propertyContract.IsReadOnlyOrFixedSize()
                        && !propertyContract.UnderlyingType.IsValueType;
                }
            }

            if (!property.Writable && !useExistingValue)
            {
                return true;
            }

            // test tokenType here because null might not be convertible to some types, e.g. ignoring null when applied to DateTime
            var resolvedNullValueHandling = property.NullValueHandling
                ?? (containerContract as JsonObjectContract)?.ItemNullValueHandling
                ?? serializer.NullValueHandling;

            if (tokenType == JsonToken.Null && resolvedNullValueHandling == NullValueHandling.Ignore)
            {
                ignoredValue = true;
                return true;
            }

            // test tokenType here because default value might not be convertible to actual type, e.g. default of "" for DateTime
            if (JsonUtils.HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
                && !JsonUtils.HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Populate)
                && JsonUtils.IsPrimitiveToken(tokenType)
                && JsonUtils.ValueEquals(reader.Value, property.GetResolvedDefaultValue()))
            {
                ignoredValue = true;
                return true;
            }

            if (currentValue != null
                && serializer.GetContract(currentValue.GetType()) != propertyContract)
            {
                propertyConverter = serializer.GetConverter(propertyContract, property.Converter, containerContract, containerProperty);
            }

            return false;
        }

        private static bool ShouldSetPropertyValue(JsonProperty property, JsonObjectContract? contract, object? value, JsonSerializer serializer)
        {
            var resolvedNullHandling = property.NullValueHandling
                ?? contract?.ItemNullValueHandling
                ?? serializer.NullValueHandling;

            if (value == null && resolvedNullHandling == NullValueHandling.Ignore)
            {
                return false;
            }

            if (JsonUtils.HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
                && !JsonUtils.HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Populate)
                && JsonUtils.ValueEquals(value, property.GetResolvedDefaultValue()))
            {
                return false;
            }

            if (!property.Writable)
            {
                return false;
            }

            return true;
        }
    }
}
