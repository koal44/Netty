using Netryoshka.Utils;
using Newtonsoft.Json.Serialization;
using System;
using System.Numerics;
using static Netryoshka.Utils.JsonUtil;

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

            return GetDefaultValue(propertyType);
        }


        public static object? GetDefaultValue(Type type)
        {
            if (!type.IsValueType)
            {
                return null;
            }

            switch (JsonUtil.GetTypeCode(type))
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
    }

    
}
