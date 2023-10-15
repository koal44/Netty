using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Netryoshka
{
    public static class JsonReaderExtensions
    {
        public static bool MoveToContent(this JsonReader reader)
        {
            JsonToken t = reader.TokenType;
            while (t == JsonToken.None || t == JsonToken.Comment)
            {
                if (!reader.Read())
                {
                    return false;
                }

                t = reader.TokenType;
            }

            return true;
        }

        public static bool ReadAndMoveToContent(this JsonReader reader)
        {
            return reader.Read() && reader.MoveToContent();
        }


        public static bool ReadForType(this JsonReader reader, JsonContract? contract, bool hasConverter)
        {
            // don't read properties with converters as a specific value
            // the value might be a string which will then get converted which will error if read as date for example
            if (hasConverter)
            {
                return reader.Read();
            }

            ReadType t = GetReadType(contract);

            switch (t)
            {
                case ReadType.Read:
                    return reader.ReadAndMoveToContent();
                case ReadType.ReadAsInt32:
                    reader.ReadAsInt32();
                    break;
                case ReadType.ReadAsInt64:
                    bool result = reader.ReadAndMoveToContent();
                    if (reader.TokenType == JsonToken.Undefined)
                    {
                        throw new JsonReaderException($"An undefined token is not a valid {contract?.UnderlyingType ?? typeof(long)}.");
                    }
                    return result;
                case ReadType.ReadAsDecimal:
                    reader.ReadAsDecimal();
                    break;
                case ReadType.ReadAsDouble:
                    reader.ReadAsDouble();
                    break;
                case ReadType.ReadAsBytes:
                    reader.ReadAsBytes();
                    break;
                case ReadType.ReadAsBoolean:
                    reader.ReadAsBoolean();
                    break;
                case ReadType.ReadAsString:
                    reader.ReadAsString();
                    break;
                case ReadType.ReadAsDateTime:
                    reader.ReadAsDateTime();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (reader.TokenType != JsonToken.None);
        }

        private enum ReadType
        {
            Read,
            ReadAsInt32,
            ReadAsInt64,
            ReadAsBytes,
            ReadAsString,
            ReadAsDecimal,
            ReadAsDateTime,
            ReadAsDouble,
            ReadAsBoolean
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

        private static ReadType GetReadType(JsonContract? contract)
        {
            if (contract == null)
            {
                return ReadType.Read;
            }

            if (ReadTypeMap.TryGetValue(contract.CreatedType, out ReadType readType))
            {
                return readType;
            }

            return ReadType.Read;
        }

        private enum JsonContractType
        {
            None = 0,
            Object = 1,
            Array = 2,
            Primitive = 3,
            String = 4,
            Dictionary = 5,
            Dynamic = 6,
            Serializable = 7,
            Linq = 8
        }

    }

}
