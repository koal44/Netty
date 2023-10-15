using Netryoshka.Utils;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using static Netryoshka.Utils.JsonUtil;

namespace Netryoshka
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

        public static bool IsNullable(this JsonContract contract)
        {
            return ReflectionUtils.IsNullable(contract.UnderlyingType);
        }

        public static bool IsConvertable(this JsonContract contract)
        {
            return typeof(IConvertible).IsAssignableFrom(contract.CreatedType);
        }

        public static bool IsEnum(this JsonContract contract)
        {
            return contract.CreatedType.IsEnum;
        }

        public static PrimitiveTypeCode GetTypeCode(this JsonContract contract)
        {
            return JsonUtil.GetTypeCode(contract.UnderlyingType);
        }
    }

}
