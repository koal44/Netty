using Netryoshka.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace Netryoshka.Utils
{
    public static class JsonUtil
    {
        public class CustomTraceWriter : ITraceWriter
        {
            public TraceLevel LevelFilter => TraceLevel.Verbose;

            public void Trace(TraceLevel level, string message, Exception? ex)
            {
                var exText = ex != null ? $"Exception: {ex.Message}" : string.Empty;
                var lvlText = $"Level: {level}";
                var msgText = $"Message: {message}";

                var components = new[] { lvlText, msgText, exText }
                                 .Where(s => !string.IsNullOrEmpty(s));

                Debug.WriteLine(string.Join(", ", components));
            }
        }

        public static T? DeserializeObject<T>(string json, JsonSerializerSettings serializerSettings, JsonLoadSettings loadSettings)
        {
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(serializerSettings);

            using JsonTextReader reader = new(new StringReader(json));
            var jtoken = JToken.ReadFrom(reader, loadSettings);
            return jtoken.ToObject<T>(jsonSerializer);
        }


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


        /// <summary>
        /// Deserializes a JSON stream from a JsonReader and combines duplicate keys within objects into arrays.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <returns>A JToken representing the JSON data structure.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the reader is null.</exception>
        public static JToken DeserializeAndCombineDuplicateKeys(JsonReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            // Make sure the reader is advanced before starting deserialization
            if (reader.TokenType == JsonToken.None && !reader.Read())
                throw new InvalidOperationException("Could not read from JsonTextReader.");

            return reader.TokenType switch
            {
                JsonToken.StartObject => DeserializeObject(reader),
                JsonToken.StartArray => DeserializeArray(reader),
                _ => new JValue(reader.Value)
            };
        }

        /// <summary>
        /// Deserialize JSON object from a reader.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <returns>A JObject.</returns>
        private static JObject DeserializeObject(JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Reader must be at the start of an object", nameof(reader));

            var obj = new JObject();
            while (true)
            {
                if (!reader.Read())
                    throw new InvalidOperationException("JSON appears to be truncated.");

                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new InvalidOperationException("Expected property name but got " + reader.TokenType);

                var propName = reader.Value?.ToString()
                    ?? throw new InvalidOperationException("Unexpected null property name.");

                if (!reader.Read())
                    throw new InvalidOperationException("JSON appears to be truncated.");

                var propValue = DeserializeAndCombineDuplicateKeys(reader);
                CombinePropertyValue(obj, propName, propValue);
            }

            return obj;
        }

        /// <summary>
        /// Deserialize JSON array from a reader.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <returns>A JArray.</returns>
        private static JArray DeserializeArray(JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartArray)
                throw new ArgumentException("Reader must be at the start of an array", nameof(reader));

            if (!reader.Read()) throw new InvalidOperationException("JSON appears to be truncated.");

            var array = new JArray();
            while (reader.TokenType != JsonToken.EndArray)
            {
                var newValue = DeserializeAndCombineDuplicateKeys(reader);
                array.Add(newValue);

                if (!reader.Read()) throw new InvalidOperationException("JSON appears to be truncated.");
            }

            return array;
        }

        private static void CombinePropertyValue(JObject obj, string propName, JToken newValue)
        {
            var existingValue = obj[propName];
            if (existingValue == null)
            {
                obj.Add(new JProperty(propName, newValue));
            }
            else if (existingValue.Type == JTokenType.Array)
            {
                var existingArray = existingValue.Value<JArray>()
                    ?? throw new InvalidOperationException("Expected a JArray but got null.");
                CombineWithArray(existingArray, newValue);
            }
            else if (existingValue.Parent is JProperty prop)
            {
                var array = new JArray(existingValue);
                prop.Value = array;
                CombineWithArray(array, newValue);
            }
        }

        private static void CombineWithArray(JArray array, JToken newValue)
        {
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

        public class StringToEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct
        {
            //public enum NumericStringFormat
            //{
            //    Decimal,
            //    Hex
            //}

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

        /// <summary>
        /// Converts integer values sourced from a list of duplicate-keyed properties in JSON, 
        /// handling both direct integers and their string representations.
        /// </summary>
        public class IntToListConverter : JsonConverter<List<int>?>
        {
            public override List<int>? ReadJson(JsonReader reader, Type objectType, List<int>? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var jToken = JToken.ReadFrom(reader);

                int value = jToken.Type switch
                {
                    JTokenType.Integer => jToken.Value<int>(),
                    JTokenType.String => int.TryParse(jToken.Value<string>(), out int result)
                            ? result
                            : throw new JsonException($"Failed to convert '{jToken.Value<string>()}' to integer."),
                    _ => throw new JsonException($"Unexpected token type: {jToken.Type}")
                };

                var list = existingValue ?? new List<int>();
                list.Add(value);
                return list;
            }

            public override void WriteJson(JsonWriter writer, List<int>? value, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
            }

            public override bool CanWrite => false;
        }


        /// <summary>
        /// Handles deserialization of a JSON token that can either be a single object or a string.
        /// Transforms the token into a list of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <remarks>
        /// The need for this converter arose from Wireshark's use of duplicate keys within JSON objects,
        /// a scenario not directly supported by Newtonsoft.Json's library.
        /// In cases where the token is a string, a custom handler in the derived class is necessary.
        /// </remarks>
        public class SingleToListConverter<T> : JsonConverter<List<T>?>
        {
            public override List<T> ReadJson(JsonReader reader, Type objectType, List<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var jToken = JToken.ReadFrom(reader);

                if (jToken.Type == JTokenType.Array)
                {
                    if (existingValue != null) throw new JsonException("Unexpected array token within existing list.");
                    return jToken.ToObject<List<T>>(serializer)
                        ?? throw new JsonException("Unexpected null instance.");
                }

                var list = existingValue ?? new List<T>();

                T? value = jToken.Type switch
                {
                    JTokenType.Object => jToken.ToObject<T>(serializer),
                    JTokenType.String => HandleStringToken(jToken.Value<string>()),
                    _ => throw new JsonException($"Unexpected token type: {jToken.Type}")
                };

                if (value is T nonNullValue)
                {
                    list.Add(nonNullValue);
                }

                return list;
            }

            public override void WriteJson(JsonWriter writer, List<T>? value, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
            }

            public override bool CanWrite => false;

            /// <summary>
            /// Handles a JSON string token and returns an object of type T.
            /// </summary>
            /// <param name="str">The JSON string token.</param>
            /// <returns>An object of type T that represents the given string token.</returns>
            /// <remarks>
            /// This method serves as an unusual workaround for cases where the JSON data 
            /// from Wireshark returns a plain string instead of an expected JSON object.
            /// </remarks>
            public virtual T HandleStringToken(string? str)
            {
                throw new NotImplementedException("Handling of string tokens is not implemented in the base class.");
            }
        }

        /// <summary>
        /// A JSON converter for deserializing into a dict with string keys and values of type <typeparamref name="T"/>. 
        /// Tailored for non-standard JSON, like Wireshark outputs with repeated keys or unusual patterns.
        /// </summary>
        /// <remarks>
        /// The converter expects the JSON to be structured as a series of key-instance pairs where each key 
        /// is represented by a property with a name matching <see cref="KeyPropertyName"/>, and each instance 
        /// is represented by a subsequent property with a name matching <see cref="ValuePropertyName"/>.
        /// Derived types should provide specific property names by overriding the respective virtual properties.
        /// </remarks>
        public class KeyedPairToDictConverter<T> : JsonConverter<Dictionary<string, T>?>
        {
            /// <summary>
            /// Deserializes the JSON into a dictionary with string keys and values of type <typeparamref name="T"/>.
            /// </summary>
            /// <exception cref="JsonException">Thrown if the JSON doesn't match the expected format.</exception>
            public override Dictionary<string, T> ReadJson(JsonReader reader, Type objectType, Dictionary<string, T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                // the json is expected to be a series of key-instance pairs where the key is a
                // string and the instance is a json object of type T

                var dict = new Dictionary<string, T>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndObject)
                        break;
                    
                    if (reader.TokenType != JsonToken.PropertyName)
                        throw new JsonException($"Expected a property name token, found {reader.TokenType}.");
                    if (reader.Value?.ToString() != KeyPropertyName)
                        throw new JsonException($"Expected a property name token matching '{KeyPropertyName}', but found '{reader.Value?.ToString()}' instead.");

                    reader.Read();
                    if (reader.TokenType != JsonToken.String)
                        throw new JsonException($"Expected a string token for the key, found {reader.TokenType} instead.");
                    var key = reader.Value?.ToString()
                        ?? throw new JsonException("Null key encountered where a string was expected.");

                    reader.Read();
                    if (reader.TokenType != JsonToken.PropertyName)
                        throw new JsonException($"Expected a property name token, found {reader.TokenType}.");
                    if (reader.Value?.ToString() != ValuePropertyName)
                        throw new JsonException($"Expected a property name token matching '{ValuePropertyName}', but found '{reader.Value?.ToString()}' instead.");

                    reader.Read();
                    var value = serializer.Deserialize<T>(reader)
                        ?? throw new JsonException("Unexpected null instance.");

                    dict.Add(key, value);
                }

                return dict;
            }

            public override void WriteJson(JsonWriter writer, Dictionary<string, T>? value, JsonSerializer serializer)
            {
                throw new NotSupportedException("Unnecessary because CanWrite is false. The type will skip the converter.");
            }

            public override bool CanWrite => false;

            public virtual string KeyPropertyName => throw new NotImplementedException();
            public virtual string ValuePropertyName => throw new NotImplementedException();
        }


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


            //public T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
            //{
            //    //return serializer.Deserialize<T>(reader);

            //    T value = Create(serializer)
            //        ?? throw new JsonSerializationException("No object created.");
            //    //serializer.Populate(reader, instance);
            //    //return instance;

            //    var jsonObject = JObjectLoad(reader, new CustomLoadSettings(PropertiesToConsider, PropertiesToIgnore));

            //    //JObject.Load() advances the reader, so we need to reset it
            //    using (var jsonReader = jsonObject.CreateReader())
            //    {
            //        serializer.Populate(jsonReader, value);
            //    }
            //    return value;
            //}


            //public T? ReadJson3(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
            //{
            //    if (reader is not JsonTextReader jsonReader)
            //        throw new JsonException("Expected a JsonTextReader.");

            //    T instance = Create(serializer)
            //        ?? throw new JsonSerializationException("No object created.");

            //    var seenKeys = new HashSet<string>();



            //    while (jsonReader.Read())
            //    {
            //        if (jsonReader.TokenType == JsonToken.PropertyName)
            //        {
            //            var key = jsonReader.Value?.ToString();
            //            if (!string.IsNullOrEmpty(key) && !seenKeys.Add(key))
            //            {
            //                throw new JsonException($"Duplicate property found in JSON: {key}");
            //            }
            //        }
            //    }


            //    // Deserialize
            //    serializer.Populate(jsonReader, instance);
            //    return instance;


            //    //return jsonObject.ToObject<T>(serializer); // problems with infinite recursion

            //    // If no duplicates, proceed with the standard deserialization


            //    // JObject.load() advances the reader, so we need to reset it
            //    //using (var jsonReader = jsonObject.CreateReader())
            //    //{
            //    //    serializer.Populate(jsonReader, instance);
            //    //}
            //    //return instance;
            //}

            //public T? ReadJson2(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
            //{
            //    var jsonObject = JObject.JObjectLoad(reader);
            //    var seenProperties = new HashSet<string>();

            //    foreach (var property in jsonObject.Properties())
            //    {
            //        if (!PropertiesToConsider.Contains(property.Name)) continue;

            //        if (PropertiesToIgnore.Contains(property.Name)) continue;

            //        if (!seenProperties.Add(property.Name))
            //        {
            //            throw new JsonException($"Duplicate property found in JSON: {property.Name}");
            //        }
            //    }

            //    //return jsonObject.ToObject<T>(serializer); // problems with infinite recursion

            //    // If no duplicates, proceed with the standard deserialization
            //    T instance = Create(serializer)
            //        ?? throw new JsonSerializationException("No object created.");

            //    // JObject.load() advances the reader, so we need to reset it
            //    using (var jsonReader = jsonObject.CreateReader())
            //    {
            //        serializer.Populate(jsonReader, instance);
            //    }
            //    return instance;
            //}


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

        public static JObject JObjectLoad(JsonReader reader, JsonLoadSettings? settings = null)
        {
            if (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                {
                    throw new JsonReaderException("Error reading JObject from JsonReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonReaderException($"Error reading JObject from JsonReader. Current JsonReader item is not an object: {reader.TokenType}");
            }

            var o = new JObject();
            o.SetLineInfo(reader as IJsonLineInfo, settings);

            o.ReadTokenFrom(reader, settings);

            return o;
        }

        public class CustomLoadSettings : JsonLoadSettings
        {
            public List<string> PropertiesToConsider { get; set; } = new List<string>();
            public List<string> PropertiesToIgnore { get; set; } = new List<string>();

            public CustomLoadSettings() { }

            public CustomLoadSettings(List<string> propertiesToConsider, List<string> propertiesToIgnore)
            {
                PropertiesToConsider = propertiesToConsider ?? throw new ArgumentNullException(nameof(propertiesToConsider));
                PropertiesToIgnore = propertiesToIgnore ?? throw new ArgumentNullException(nameof(propertiesToIgnore));
            }
        }


        private static bool SetPropertyValue(JsonProperty property, JsonConverter? propertyConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty, JsonReader reader, object target, JsonSerializer serializer)
        {
            bool skipSettingProperty = CalculatePropertyDetails(
                property,
                ref propertyConverter,
                containerContract,
                containerProperty,
                reader,
                target,
                serializer,
                out bool useExistingValue,
                out object? currentValue,
                out JsonContract? propertyContract,
                out bool gottenCurrentValue,
                out bool ignoredValue);

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

        private static Type? InternalReaderType = typeof(JsonSerializer).Assembly.GetType("Newtonsoft.Json.Serialization.JsonSerializerInternalReader");
        private static MethodInfo? CreateValueInternalMethod = InternalReaderType?.GetMethod("CreateValueInternal", BindingFlags.NonPublic | BindingFlags.Instance);

        public static object? CreateValueUsingInternalReader(
            JsonReader reader,
            Type? propertyType,
            JsonContract? propertyContract,
            JsonProperty? jsonProperty,
            JsonContainerContract? containerContract,
            JsonProperty? containerProperty,
            object? existingValue,
            JsonSerializer jsonSerializer)
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

        private static bool CalculatePropertyDetails(
            JsonProperty property,
            ref JsonConverter? propertyConverter,
            JsonContainerContract? containerContract,
            JsonProperty? containerProperty,
            JsonReader reader,
            object target,
            JsonSerializer serializer,
            out bool useExistingValue,
            out object? currentValue,
            out JsonContract? propertyContract,
            out bool gottenCurrentValue,
            out bool ignoredValue)
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
            if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
                && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Populate)
                && IsPrimitiveToken(tokenType)
                && ValueEquals(reader.Value, property.GetResolvedDefaultValue()))
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


        private static bool IsPrimitiveToken(JsonToken token)
        {
            return token switch
            {
                JsonToken.Integer or
                JsonToken.Float or
                JsonToken.String or
                JsonToken.Boolean or
                JsonToken.Undefined or
                JsonToken.Null or
                JsonToken.Date or
                JsonToken.Bytes => true,
                _ => false,
            };
        }

        private static object? CreateValueInternal(JsonReader reader, Type? objectType, JsonContract? contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerMember, object? existingValue, JsonSerializer serializer)
        {
            // linq not relevant for wireshark deserialization
            //if (contract != null && contract.ContractType == JsonContractType.Linq)
            //{
            //    return CreateJToken(reader, contract);
            //}

            do
            {
                switch (reader.TokenType)
                {
                    // populate a typed object or generic dictionary/array
                    // depending upon whether an objectType was supplied
                    case JsonToken.StartObject:
                        //return CreateObject(reader, objectType, contract, member, containerContract, containerMember, existingValue);
                    case JsonToken.StartArray:
                        //return CreateList(reader, objectType, contract, member, existingValue, null);
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Boolean:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.String:
                        string s = (string)reader.Value!;

                        // string that needs to be returned as a byte array should be base 64 decoded
                        if (objectType == typeof(byte[]))
                        {
                            return Convert.FromBase64String(s);
                        }

                        // convert empty string to null automatically for nullable types
                        if (CoerceEmptyStringToNull(objectType, contract, s))
                        {
                            return null;
                        }

                        return EnsureType(reader, s, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.StartConstructor:
                        string constructorName = reader.Value!.ToString()!;

                        return EnsureType(reader, constructorName, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                        return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.Raw:
                        return new JRaw((string?)reader.Value);
                    case JsonToken.Comment:
                        // ignore
                        break;
                    default:
                        throw new JsonSerializationException($"Unexpected token while deserializing object: {reader.TokenType}");
                }
            } while (reader.Read());

            throw new JsonSerializationException("Unexpected end when deserializing object.");
        }

        private static object? EnsureType(JsonReader reader, object? value, CultureInfo culture, JsonContract? contract, Type? targetType)
        {
            if (targetType == null)
            {
                return value;
            }

            if (contract == null) throw new JsonException("Contract is null.");

            Type? valueType = value?.GetType();

            // type of value and type of target don't match
            // attempt to convert value's type to target's type
            if (valueType != targetType)
            {
                if (value == null && contract.IsNullable())
                {
                    return null;
                }

                try
                {
                    if (contract.IsConvertable())
                    {
                        JsonPrimitiveContract primitiveContract = (JsonPrimitiveContract)contract;

                        if (contract.IsEnum())
                        {
                            if (value is string s)
                            {
                                return Enum.Parse(contract.CreatedType, s, true);
                            }
                            if (IsInteger(primitiveContract.GetTypeCode()))
                            {
                                return Enum.ToObject(contract.CreatedType, value!);
                            }
                        }
                        else if (contract.CreatedType == typeof(DateTime))
                        {
                            throw new Exception("Grumpy coder says: build your own damn datetime converter.");
                            // use DateTimeUtils because Convert.ChangeType does not set DateTime.Kind correctly
                            //if (value is string s && DateTimeUtils.TryParseDateTime(s, reader.DateTimeZoneHandling, reader.DateFormatString, reader.Culture, out DateTime dt))
                            //{
                            //    return DateTimeUtils.EnsureDateTime(dt, reader.DateTimeZoneHandling);
                            //}
                        }

                        if (value is BigInteger integer)
                        {
                            return FromBigInteger(integer, contract.CreatedType);
                        }

                        // this won't work when converting to a custom IConvertible
                        return Convert.ChangeType(value, contract.CreatedType, culture);
                    }

                    return ConvertOrCast(value, culture, contract.CreatedType);
                }
                catch (Exception)
                {
                    throw new JsonSerializationException($"Error converting value {ToString(value)} to type '{targetType}'.");
                }
            }

            return value;
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

            if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
                && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Populate)
                && ValueEquals(value, property.GetResolvedDefaultValue()))
            {
                return false;
            }

            if (!property.Writable)
            {
                return false;
            }

            return true;
        }

        private static bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
        {
            return ((value & flag) == flag);
        }

        public static bool ValueEquals(object? objA, object? objB)
        {
            if (objA == objB)
            {
                return true;
            }
            if (objA == null || objB == null)
            {
                return false;
            }

            // comparing an Int32 and Int64 both of the same value returns false
            // make types the same then compare
            if (objA.GetType() != objB.GetType())
            {
                if (IsInteger(objA) && IsInteger(objB))
                {
                    return Convert.ToDecimal(objA, CultureInfo.CurrentCulture).Equals(Convert.ToDecimal(objB, CultureInfo.CurrentCulture));
                }
                else if ((objA is double || objA is float || objA is decimal) && (objB is double || objB is float || objB is decimal))
                {
                    return ApproxEquals(Convert.ToDouble(objA, CultureInfo.CurrentCulture), Convert.ToDouble(objB, CultureInfo.CurrentCulture));
                }
                else
                {
                    return false;
                }
            }

            return objA.Equals(objB);
        }

        public static bool IsInteger(object value)
        {
            return GetTypeCode(value.GetType()) switch
            {
                PrimitiveTypeCode.SByte or 
                PrimitiveTypeCode.Byte or 
                PrimitiveTypeCode.Int16 or 
                PrimitiveTypeCode.UInt16 or 
                PrimitiveTypeCode.Int32 or 
                PrimitiveTypeCode.UInt32 or 
                PrimitiveTypeCode.Int64 or 
                PrimitiveTypeCode.UInt64 => true,
                _ => false,
            };
        }

        public static PrimitiveTypeCode GetTypeCode(Type t)
        {
            return GetTypeCode(t, out _);
        }

        private static PrimitiveTypeCode GetTypeCode(Type t, out bool isEnum)
        {
            if (TypeCodeMap.TryGetValue(t, out PrimitiveTypeCode typeCode))
            {
                isEnum = false;
                return typeCode;
            }

            if (t.IsEnum)
            {
                isEnum = true;
                return GetTypeCode(Enum.GetUnderlyingType(t));
            }

            // performance?
            if (ReflectionUtils.IsNullableType(t))
            {
                Type nonNullable = Nullable.GetUnderlyingType(t)!;
                if (nonNullable.IsEnum)
                {
                    Type nullableUnderlyingType = typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(nonNullable));
                    isEnum = true;
                    return GetTypeCode(nullableUnderlyingType);
                }
            }

            isEnum = false;
            return PrimitiveTypeCode.Object;
        }

        private static readonly Dictionary<Type, PrimitiveTypeCode> TypeCodeMap =
            new()
            {
                { typeof(char), PrimitiveTypeCode.Char },
                { typeof(char?), PrimitiveTypeCode.CharNullable },
                { typeof(bool), PrimitiveTypeCode.Boolean },
                { typeof(bool?), PrimitiveTypeCode.BooleanNullable },
                { typeof(sbyte), PrimitiveTypeCode.SByte },
                { typeof(sbyte?), PrimitiveTypeCode.SByteNullable },
                { typeof(short), PrimitiveTypeCode.Int16 },
                { typeof(short?), PrimitiveTypeCode.Int16Nullable },
                { typeof(ushort), PrimitiveTypeCode.UInt16 },
                { typeof(ushort?), PrimitiveTypeCode.UInt16Nullable },
                { typeof(int), PrimitiveTypeCode.Int32 },
                { typeof(int?), PrimitiveTypeCode.Int32Nullable },
                { typeof(byte), PrimitiveTypeCode.Byte },
                { typeof(byte?), PrimitiveTypeCode.ByteNullable },
                { typeof(uint), PrimitiveTypeCode.UInt32 },
                { typeof(uint?), PrimitiveTypeCode.UInt32Nullable },
                { typeof(long), PrimitiveTypeCode.Int64 },
                { typeof(long?), PrimitiveTypeCode.Int64Nullable },
                { typeof(ulong), PrimitiveTypeCode.UInt64 },
                { typeof(ulong?), PrimitiveTypeCode.UInt64Nullable },
                { typeof(float), PrimitiveTypeCode.Single },
                { typeof(float?), PrimitiveTypeCode.SingleNullable },
                { typeof(double), PrimitiveTypeCode.Double },
                { typeof(double?), PrimitiveTypeCode.DoubleNullable },
                { typeof(DateTime), PrimitiveTypeCode.DateTime },
                { typeof(DateTime?), PrimitiveTypeCode.DateTimeNullable },
                { typeof(DateTimeOffset), PrimitiveTypeCode.DateTimeOffset },
                { typeof(DateTimeOffset?), PrimitiveTypeCode.DateTimeOffsetNullable },
                { typeof(decimal), PrimitiveTypeCode.Decimal },
                { typeof(decimal?), PrimitiveTypeCode.DecimalNullable },
                { typeof(Guid), PrimitiveTypeCode.Guid },
                { typeof(Guid?), PrimitiveTypeCode.GuidNullable },
                { typeof(TimeSpan), PrimitiveTypeCode.TimeSpan },
                { typeof(TimeSpan?), PrimitiveTypeCode.TimeSpanNullable },
                { typeof(BigInteger), PrimitiveTypeCode.BigInteger },
                { typeof(BigInteger?), PrimitiveTypeCode.BigIntegerNullable },
                { typeof(Uri), PrimitiveTypeCode.Uri },
                { typeof(string), PrimitiveTypeCode.String },
                { typeof(byte[]), PrimitiveTypeCode.Bytes },
                //{ typeof(DBNull), PrimitiveTypeCode.DBNull }
            };

        public enum PrimitiveTypeCode
        {
            Empty = 0,
            Object = 1,
            Char = 2,
            CharNullable = 3,
            Boolean = 4,
            BooleanNullable = 5,
            SByte = 6,
            SByteNullable = 7,
            Int16 = 8,
            Int16Nullable = 9,
            UInt16 = 10,
            UInt16Nullable = 11,
            Int32 = 12,
            Int32Nullable = 13,
            Byte = 14,
            ByteNullable = 15,
            UInt32 = 16,
            UInt32Nullable = 17,
            Int64 = 18,
            Int64Nullable = 19,
            UInt64 = 20,
            UInt64Nullable = 21,
            Single = 22,
            SingleNullable = 23,
            Double = 24,
            DoubleNullable = 25,
            DateTime = 26,
            DateTimeNullable = 27,
            DateTimeOffset = 28,
            DateTimeOffsetNullable = 29,
            Decimal = 30,
            DecimalNullable = 31,
            Guid = 32,
            GuidNullable = 33,
            TimeSpan = 34,
            TimeSpanNullable = 35,
            BigInteger = 36,
            BigIntegerNullable = 37,
            Uri = 38,
            String = 39,
            Bytes = 40,
            DBNull = 41
        }

        public static bool ApproxEquals(double d1, double d2)
        {
            const double epsilon = 2.2204460492503131E-16;

            if (d1 == d2)
            {
                return true;
            }

            double tolerance = ((Math.Abs(d1) + Math.Abs(d2)) + 10.0) * epsilon;
            double difference = d1 - d2;

            return (-tolerance < difference && tolerance > difference);
        }

        public static object FromBigInteger(BigInteger i, Type targetType)
        {
            if (targetType == typeof(decimal)) return (decimal)i;
            if (targetType == typeof(double)) return (double)i;
            if (targetType == typeof(float)) return (float)i;
            if (targetType == typeof(ulong)) return (ulong)i;
            if (targetType == typeof(bool)) return i != 0;

            try
            {
                return System.Convert.ChangeType((long)i, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Can not convert from BigInteger to {targetType}.");
            }
        }

        internal static BigInteger ToBigInteger(object value)
        {
            if (value is BigInteger integer) return integer;
            if (value is string s) return BigInteger.Parse(s, CultureInfo.InvariantCulture);
            if (value is float f) return new BigInteger(f);
            if (value is double d) return new BigInteger(d);
            if (value is decimal @decimal) return new BigInteger(@decimal);
            if (value is int i) return new BigInteger(i);
            if (value is long l) return new BigInteger(l);
            if (value is uint u) return new BigInteger(u);
            if (value is ulong @ulong) return new BigInteger(@ulong);
            if (value is byte[] bytes) return new BigInteger(bytes);

            throw new InvalidCastException($"Cannot convert {value.GetType()} to BigInteger.");
        }

        public static object? ConvertOrCast(object? initialValue, CultureInfo culture, Type targetType)
        {
            if (targetType == typeof(object))
            {
                return initialValue;
            }

            if (initialValue == null && ReflectionUtils.IsNullable(targetType))
            {
                return null;
            }

            if (TryConvert(initialValue, culture, targetType, out object? convertedValue))
            {
                return convertedValue;
            }

            return EnsureTypeAssignable(initialValue, initialValue?.GetType()!, targetType);
        }

        private static object? EnsureTypeAssignable(object? value, Type initialType, Type targetType)
        {
            if (value != null)
            {
                Type valueType = value.GetType();

                if (targetType.IsAssignableFrom(valueType))
                {
                    return value;
                }


                // we won't use custom implicit or explict casts for wireshark deserialization.
                //Func<object?, object?>? castConverter = CastConverters.Get(new StructMultiKey<Type, Type>(valueType, targetType));
                //if (castConverter != null)
                //{
                //    return castConverter(value);
                //}
            }
            else
            {
                if (ReflectionUtils.IsNullable(targetType))
                {
                    return null;
                }
            }

            throw new ArgumentException($"Could not cast or convert from {initialType?.ToString() ?? "{null}"} to {targetType}.");
        }


        private static bool TryConvert(object? initialValue, CultureInfo culture, Type targetType, out object? value)
        {
            try
            {
                if (TryConvertInternal(initialValue, culture, targetType, out value))
                {
                    return true;
                }

                value = null;
                return false;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        private static bool TryConvertInternal(object? initialValue, CultureInfo culture, Type targetType, out object? value)
        {
            if (initialValue == null)
            {
                throw new ArgumentNullException(nameof(initialValue));
            }

            if (ReflectionUtils.IsNullableType(targetType))
            {
                targetType = Nullable.GetUnderlyingType(targetType)!;
            }

            Type initialType = initialValue.GetType();

            if (targetType == initialType)
            {
                value = initialValue;
                return true;
            }

            // use Convert.ChangeType if both types are IConvertible
            if (IsConvertible(initialValue.GetType()) && IsConvertible(targetType))
            {
                if (targetType.IsEnum)
                {
                    if (initialValue is string)
                    {
                        value = Enum.Parse(targetType, initialValue.ToString()!, true);
                        return true;
                    }
                    else if (IsInteger(initialValue))
                    {
                        value = Enum.ToObject(targetType, initialValue);
                        return true;
                    }
                }

                value = System.Convert.ChangeType(initialValue, targetType, culture);
                return true;
            }

            if (initialValue is DateTime dt && targetType == typeof(DateTimeOffset))
            {
                value = new DateTimeOffset(dt);
                return true;
            }

            if (initialValue is byte[] bytes && targetType == typeof(Guid))
            {
                value = new Guid(bytes);
                return true;
            }

            if (initialValue is Guid guid && targetType == typeof(byte[]))
            {
                value = guid.ToByteArray();
                return true;
            }

            if (initialValue is string s)
            {
                if (targetType == typeof(Guid))
                {
                    value = new Guid(s);
                    return true;
                }
                if (targetType == typeof(Uri))
                {
                    value = new Uri(s, UriKind.RelativeOrAbsolute);
                    return true;
                }
                if (targetType == typeof(TimeSpan))
                {
                    value = TimeSpan.Parse(s, CultureInfo.InvariantCulture);
                    return true;
                }
                if (targetType == typeof(byte[]))
                {
                    value = System.Convert.FromBase64String(s);
                    return true;
                }
                if (targetType == typeof(Version))
                {
                    if (Version.TryParse(s, out Version? result)) 
                    {
                        value = result;
                        return true;
                    }
                    value = null;
                    return false;
                }
                if (typeof(Type).IsAssignableFrom(targetType))
                {
                    value = Type.GetType(s, true);
                    return true;
                }
                if (targetType == typeof(DateOnly))
                {
                    value = DateOnly.ParseExact(s, "yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);
                    return true;
                }
                if (targetType == typeof(TimeOnly))
                {
                    value = TimeOnly.ParseExact(s, "HH':'mm':'ss.FFFFFFF", CultureInfo.InvariantCulture);
                    return true;
                }
            }

            if (targetType == typeof(BigInteger))
            {
                value = ToBigInteger(initialValue);
                return true;
            }
            if (initialValue is BigInteger integer)
            {
                value = FromBigInteger(integer, targetType);
                return true;
            }

            if (targetType.IsInterface || targetType.IsGenericTypeDefinition || targetType.IsAbstract)
            {
                value = null;
                return false;
            }

            value = null;
            return false;
        }

        public static bool IsConvertible(Type t)
        {
            return typeof(IConvertible).IsAssignableFrom(t);
        }

        private static bool CoerceEmptyStringToNull(Type? objectType, JsonContract? contract, string s)
        {
            return string.IsNullOrEmpty(s)
                && objectType != null 
                && objectType != typeof(string) 
                && objectType != typeof(object) 
                && contract != null 
                && contract.IsNullable();
        }

        public static string ToString(object? value)
        {
            if (value == null) return "{null}";
            return (value is string s) ? @"""" + s + @"""" : value!.ToString()!;
        }

    }
}
