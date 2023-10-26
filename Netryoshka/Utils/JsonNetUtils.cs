using Netryoshka.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace Netryoshka.Utils
{
    public static class JsonNetUtils
    {
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


        public static bool IsPrimitiveToken(JsonToken token)
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


        public static bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
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
                    return MathUtils.ApproxEquals(Convert.ToDouble(objA, CultureInfo.CurrentCulture), Convert.ToDouble(objB, CultureInfo.CurrentCulture));
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

        private static readonly Dictionary<Type, PrimitiveTypeCode> TypeCodeMap = new()
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


        public static object? GetDefaultValue(Type type)
        {
            if (!type.IsValueType)
            {
                return null;
            }

            switch (GetTypeCode(type))
            {
                case PrimitiveTypeCode.Boolean:
                    return false;
                case PrimitiveTypeCode.Char:
                case PrimitiveTypeCode.SByte:
                case PrimitiveTypeCode.Byte:
                case PrimitiveTypeCode.Int16:
                case PrimitiveTypeCode.UInt16:
                case PrimitiveTypeCode.Int32:
                case PrimitiveTypeCode.UInt32:
                    return 0;
                case PrimitiveTypeCode.Int64:
                case PrimitiveTypeCode.UInt64:
                    return 0L;
                case PrimitiveTypeCode.Single:
                    return 0f;
                case PrimitiveTypeCode.Double:
                    return 0.0;
                case PrimitiveTypeCode.Decimal:
                    return 0m;
                case PrimitiveTypeCode.DateTime:
                    return new DateTime();
                case PrimitiveTypeCode.BigInteger:
                    return new BigInteger();
                case PrimitiveTypeCode.Guid:
                    return new Guid();
                case PrimitiveTypeCode.DateTimeOffset:
                    return new DateTimeOffset();
            }

            if (ReflectionUtils.IsNullable(type))
            {
                return null;
            }

            // possibly use IL initobj for perf here?
            return Activator.CreateInstance(type);
        }


        public static bool SetPropertyValue(JsonProperty property, JsonConverter? propertyConverter, JsonContainerContract? containerContract, JsonProperty? containerProperty, JsonReader reader, object target, JsonSerializer serializer)
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

                    useExistingValue = !propertyContract.UnderlyingType.IsValueType
                        && !propertyContract.IsReadOnlyOrFixedSize();
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

        private static bool ShouldSetPropertyValue(JsonProperty property, JsonObjectContract? contract, object? value, JsonSerializer serializer)
        {
            var resolvedNullHandling = property.NullValueHandling
                ?? contract?.ItemNullValueHandling
                ?? serializer.NullValueHandling;

            if (value == null && resolvedNullHandling == NullValueHandling.Ignore)
            {
                return false;
            }

            if (JsonNetUtils.HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
                && !JsonNetUtils.HasFlag(property.DefaultValueHandling.GetValueOrDefault(serializer.DefaultValueHandling), DefaultValueHandling.Populate)
                && JsonNetUtils.ValueEquals(value, property.GetResolvedDefaultValue()))
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
