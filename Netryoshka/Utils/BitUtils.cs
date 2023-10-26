using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Netryoshka.Utils
{
    public static class BitUtils
    {
        public static string BytesToAscii(byte[] buffer, bool keepSpacing = true)
        {
            var sb = new StringBuilder();

            foreach (byte b in buffer)
            {
                char c = (char)b; // Directly cast byte to char

                if (keepSpacing)
                {
                    switch (c)
                    {
                        case '\n': sb.Append("\\n"); continue;
                        case '\r': sb.Append("\\r"); continue;
                        case '\t': sb.Append("\\t"); continue;
                    }
                }

                // ASCII controlStyle characters are from 0x00 to 0x1F
                //sb.Append(c >= 0x00 && c <= 0x1F ? '.' : c);

                // Only append printable ASCII characters, replace anything else with '.'
                sb.Append((c >= 0x20 && c <= 0x7E) ? c : '.');
            }

            return sb.ToString();
        }


        public static long DoubleToInt64Bits(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        public static double Int64BitsToDouble(long value)
        {
            return BitConverter.Int64BitsToDouble(value);
        }

        public static int SingleToInt32Bits(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        public static float Int32BitsToSingle(int value)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }

        public static bool ToBoolean(byte[] value, int startIndex)
        {
            return BitConverter.ToBoolean(value, startIndex);
        }

        public static char ToChar(byte[] buffer, int startIndex, Endianness endianness)
        {
            return endianness == Endianness.Big
                ? (char)BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ushort>(ref buffer[startIndex]))
                : (char)Unsafe.As<byte, ushort>(ref buffer[startIndex]);
        }

        public static double ToDouble(byte[] value, int startIndex, Endianness endianness)
        {
            return Int64BitsToDouble(ToInt64(value, startIndex, endianness));
        }

        public static float ToSingle(byte[] value, int startIndex, Endianness endianness)
        {
            return Int32BitsToSingle(ToInt32(value, startIndex, endianness));
        }

        public static ushort ToUInt16(byte[] buffer, int startIndex, Endianness endianness)
        {
            return endianness == Endianness.Big
                ? BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ushort>(ref buffer[startIndex]))
                : Unsafe.As<byte, ushort>(ref buffer[startIndex]);
        }

        public static uint ToUInt32(byte[] buffer, int startIndex, Endianness endianness)
        {
            return endianness == Endianness.Big
                ? BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, uint>(ref buffer[startIndex]))
                : Unsafe.As<byte, uint>(ref buffer[startIndex]);
        }

        public static ulong ToUInt64(byte[] buffer, int startIndex, Endianness endianness)
        {
            return endianness == Endianness.Big
                ? BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ulong>(ref buffer[startIndex]))
                : Unsafe.As<byte, ulong>(ref buffer[startIndex]);
        }

        public static short ToInt16(byte[] buffer, int startIndex, Endianness endianness)
        {
            return endianness == Endianness.Big
                ? BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, short>(ref buffer[startIndex]))
                : Unsafe.As<byte, short>(ref buffer[startIndex]);
        }

        public static int ToInt32(byte[] buffer, int startIndex, Endianness endianness)
        {
            return endianness == Endianness.Big
                ? BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, int>(ref buffer[startIndex]))
                : Unsafe.As<byte, int>(ref buffer[startIndex]);
        }

        public static long ToInt64(byte[] buffer, int startIndex, Endianness endianness)
        {
            return endianness == Endianness.Big
                ? BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, long>(ref buffer[startIndex]))
                : Unsafe.As<byte, long>(ref buffer[startIndex]);
        }

        public static string ToString(byte[] value)
        {
            return BitConverter.ToString(value);
        }

        public static string ToString(byte[] value, int startIndex)
        {
            return BitConverter.ToString(value, startIndex);
        }

        public static string ToString(byte[] value, int startIndex, int length)
        {
            return BitConverter.ToString(value, startIndex, length);
        }

        public static decimal ToDecimal(byte[] value, int startIndex, Endianness endianness)
        {
            int[] array = new int[4];
            for (int i = 0; i < 4; i++)
            {
                array[i] = ToInt32(value, startIndex + i * 4, endianness);
            }

            return new decimal(array);
        }

        public static byte[] GetBytes(decimal value, Endianness endianness)
        {
            byte[] array = new byte[16];
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < 4; i++)
            {
                CopyBytes(bits[i], array, i * 4, endianness);
            }

            return array;
        }

        public static void CopyBytes(decimal value, byte[] buffer, int index, Endianness endianness)
        {
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < 4; i++)
            {
                CopyBytes(bits[i], buffer, i * 4 + index, endianness);
            }
        }

        private static byte[] GetBytes(long value, int bytes, Endianness endianness)
        {
            byte[] array = new byte[bytes];
            CopyBytes(value, bytes, array, 0, endianness);
            return array;
        }

        public static byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(char value, Endianness endianness)
        {
            return GetBytes(value, 2, endianness);
        }

        public static byte[] GetBytes(double value, Endianness endianness)
        {
            return GetBytes(DoubleToInt64Bits(value), 8, endianness);
        }

        public static byte[] GetBytes(short value, Endianness endianness)
        {
            return GetBytes(value, 2, endianness);
        }

        public static byte[] GetBytes(int value, Endianness endianness)
        {
            return GetBytes(value, 4, endianness);
        }

        public static byte[] GetBytes(long value, Endianness endianness)
        {
            return GetBytes(value, 8, endianness);
        }

        public static byte[] GetBytes(float value, Endianness endianness)
        {
            return GetBytes(SingleToInt32Bits(value), 4, endianness);
        }

        public static byte[] GetBytes(ushort value, Endianness endianness)
        {
            return GetBytes(value, 2, endianness);
        }

        public static byte[] GetBytes(uint value, Endianness endianness)
        {
            return GetBytes(value, 4, endianness);
        }

        public static byte[] GetBytes(ulong value, Endianness endianness)
        {
            return GetBytes((long)value, 8, endianness);
        }

        private static void CopyBytes(long value, int bytes, byte[] buffer, int index, Endianness endianness)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (buffer.Length < index + bytes) throw new ArgumentOutOfRangeException(nameof(buffer));

            if (endianness == Endianness.Big)
            {
                int num = index + bytes - 1;
                for (int i = 0; i < bytes; i++)
                {
                    buffer[num - i] = (byte)(value & 0xFF);
                    value >>= 8;
                }
            }
            else
            {
                for (int i = 0; i < bytes; i++)
                {
                    buffer[i + index] = (byte)(value & 0xFF);
                    value >>= 8;
                }
            }
        }

        public static void CopyBytes(bool value, byte[] buffer, int index)
        {
            buffer[index] = (byte)(value ? 1u : 0u);
        }

        public static void CopyBytes(double value, byte[] buffer, int index, Endianness endianness)
        {
            CopyBytes(DoubleToInt64Bits(value), buffer, index, endianness);
        }

        public static void CopyBytes(short value, byte[] buffer, int index, Endianness endianness)
        {
            if (endianness == Endianness.Big)
                value = BinaryPrimitives.ReverseEndianness(value);
            Unsafe.WriteUnaligned(ref buffer[index], value);
        }

        public static void CopyBytes(int value, byte[] buffer, int index, Endianness endianness)
        {
            if (endianness == Endianness.Big)
                value = BinaryPrimitives.ReverseEndianness(value);
            Unsafe.WriteUnaligned(ref buffer[index], value);
        }

        public static void CopyBytes(long value, byte[] buffer, int index, Endianness endianness)
        {
            if (endianness == Endianness.Big)
                value = BinaryPrimitives.ReverseEndianness(value);
            Unsafe.WriteUnaligned(ref buffer[index], value);
        }

        public static void CopyBytes(ushort value, byte[] buffer, int index, Endianness endianness)
        {
            if (endianness == Endianness.Big)
                value = BinaryPrimitives.ReverseEndianness(value);
            Unsafe.WriteUnaligned(ref buffer[index], value);
        }

        public static void CopyBytes(uint value, byte[] buffer, int index, Endianness endianness)
        {
            if (endianness == Endianness.Big)
                value = BinaryPrimitives.ReverseEndianness(value);
            Unsafe.WriteUnaligned(ref buffer[index], value);
        }

        public static void CopyBytes(ulong value, byte[] buffer, int index, Endianness endianness)
        {
            if (endianness == Endianness.Big)
                value = BinaryPrimitives.ReverseEndianness(value);
            Unsafe.WriteUnaligned(ref buffer[index], value);
        }

        public static void CopyBytes(float value, byte[] buffer, int index, Endianness endianness)
        {
            CopyBytes(SingleToInt32Bits(value), 4, buffer, index, endianness);
        }

    }
}
