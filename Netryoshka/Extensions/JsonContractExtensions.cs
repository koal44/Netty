using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using static Netryoshka.Extensions.JsonReaderExtensions;

namespace Netryoshka.Extensions
{
    public static class JsonContractExtensions
    {
        public static bool IsReadOnlyOrFixedSize(this JsonContract contract)
        {
            // If the contract is of type JsonArrayContract, handle special cases
            if (contract is JsonArrayContract arrayContract)
            {
                var isArray = contract.CreatedType.IsArray
                    || (contract.CreatedType.IsGenericType
                        && contract.CreatedType.GetGenericTypeDefinition().FullName == "System.Linq.EmptyPartition`1");

                if (!isArray)
                {
                    // Check if it's mutable like List<T>
                    if (arrayContract.CreatedType.IsGenericType
                        && arrayContract.CreatedType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        return false;
                    }
                }
            }

            // If none of the special cases are met, assume it's read-only or fixed size.
            // This will also return true for JsonDictionaryContract and JsonPrimitiveContract
            return true;
        }

        public static void InvokeOnDeserialized(this JsonContract contract, object o, StreamingContext context)
        {
            if (contract.OnDeserializedCallbacks != null)
            {
                foreach (SerializationCallback callback in contract.OnDeserializedCallbacks)
                {
                    callback(o, context);
                }
            }
        }


        public static Type NonNullableUnderlyingType(this JsonContract contract)
        {
            var type = contract.GetType();
            var fieldInfo = type.GetField("NonNullableUnderlyingType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new Exception("Could not find field 'NonNullableUnderlyingType' in JsonContract");
            var fieldValue = fieldInfo.GetValue(contract)
                ?? throw new Exception(fieldInfo.Name + " is null");

            return (Type)fieldValue;
        }

        public static ReadType GetReadType(this JsonContract contract)
        {
            if (ReadTypeMap.TryGetValue(contract.CreatedType, out ReadType readType))
            {
                return readType;
            }

            return ReadType.Read;
        }

        private static readonly Dictionary<Type, ReadType> ReadTypeMap = new()
        {
            [typeof(byte[])] = ReadType.ReadAsBytes,
            [typeof(byte)] = ReadType.ReadAsInt32,
            [typeof(short)] = ReadType.ReadAsInt32,
            [typeof(int)] = ReadType.ReadAsInt32,
            [typeof(decimal)] = ReadType.ReadAsDecimal,
            [typeof(bool)] = ReadType.ReadAsBoolean,
            [typeof(string)] = ReadType.ReadAsString,
            [typeof(DateTime)] = ReadType.ReadAsDateTime,
            [typeof(float)] = ReadType.ReadAsDouble,
            [typeof(double)] = ReadType.ReadAsDouble,
            [typeof(long)] = ReadType.ReadAsInt64
        };


    }

}
