using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Netryoshka.Utils
{
    public static partial class HexUtils
    {
        /// <summary>
        /// Maps an integer (0-15) to char ('0' to 'f').
        /// </summary>
        /// <param name="n">The integer to convert. Must be between 0 and 15.</param>
        /// <returns>The character representation ('0' to 'f').</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the input is outside the range 0-15.</exception>
        public static char IntToHexChar(int n)
        {
            const int ASCII_ZERO = 48;
            const int ASCII_LOWERCASE_A = 97;

            if (n < 0 || n > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "Input must be a single digit hexadecimal number (0-15)");
            }

            if (n <= 9)
            {
                return (char)(n + ASCII_ZERO);
            }

            return (char)(n - 10 + ASCII_LOWERCASE_A);
        }

        /// <summary>
        /// Gets the integer value of a hex character.
        /// </summary>
        /// <param name="hexChar">The hex character to convert.</param>
        /// <returns>The integer value of the hex character.</returns>
        private static int HexCharToInt(char hexChar)
        {
            int val = hexChar;
            // Convert the hex character to its integer value.
            // ASCII values for '0' to '9' are 48-57.
            // ASCII values for 'A' to 'F' are 65-70.
            // ASCII values for 'a' to 'f' are 97-102.
            return val - (val < 58 ? 48 : val < 97 ? 55 : 87);
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array using optimized bitwise operations.
        /// </summary>
        /// <param name="hex">The hexadecimal string to convert.</param>
        /// <returns>A byte array representing the hexadecimal string.</returns>
        /// <exception cref="ArgumentException">Thrown when the input hex string has an odd length.</exception>
        public static byte[] HexToBytesOptimized(string hex)
        {
            // Check if the hex string has an odd number of characters, which is invalid.
            if (hex.Length % 2 == 1)
            {
                throw new ArgumentException("The hex string cannot have an odd number of digits", nameof(hex));
            }

            // Initialize the byte array. 
            // Using bit-shifting to divide the length of the string by 2.
            byte[] arr = new byte[hex.Length >> 1];

            // Iterate through each pair of characters in the hex string.
            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                // Convert each pair of hex characters to a byte using bitwise operations.
                // Shift the first hex character 4 bits to the left to make room for the second character,
                // and then use bitwise OR to combine them into a single byte.
                arr[i] = (byte)((HexCharToInt(hex[i << 1]) << 4) + HexCharToInt(hex[(i << 1) + 1]));
            }

            return arr;
        }


        /// <summary>
        /// Converts a hex string to a byte array.
        /// </summary>
        /// <param name="hex">The hex string to convert.</param>
        /// <returns>A byte array representing the hex string.</returns>
        /// <exception cref="ArgumentException">Thrown when the hex string has an odd number of digits.</exception>
        public static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException($"The hex string cannot have an odd number of digits: {hex}");
            }

            byte[] data = new byte[hex.Length / 2];
            for (int byteIndex = 0; byteIndex < data.Length; byteIndex++)
            {
                string byteValue = hex.Substring(byteIndex * 2, 2);
                data[byteIndex] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }


        public static string GenerateRandomHexString(int length)
        {
            if (length < 0) throw new ArgumentException("Length should be non-negative");

            byte[] buffer = new byte[length / 2];  // 2 hex digits represent one byte
            Random random = new();
            random.NextBytes(buffer);

            var sb = new StringBuilder();
            foreach (byte b in buffer)
            {
                sb.Append(b.ToString("X2"));  // Convert byte to 2-digit hex
            }

            // If odd, add one more random digit
            if (length % 2 != 0)
            {
                sb.Append(random.Next(0, 16).ToString("X"));
            }

            return sb.ToString();
        }


        /// <summary>
        /// Reverses the endianness of a hexadecimal string.
        /// </summary>
        /// <param name="hex">The hexadecimal string to reverse. Must have an even length.</param>
        /// <returns>The hexadecimal string with reversed endianness.</returns>
        /// <exception cref="ArgumentException">Thrown when the input string has an odd length or contains invalid characters.</exception>
        public static string ReverseHexEndianness(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Invalid hex string length. Length must be a multiple of 2.");

            if (!hex.All(IsValidHexChar))
                throw new ArgumentException("Invalid hex string. Contains non-hexadecimal characters.");

            var reverseStringBuilder = new StringBuilder(hex.Length);
            for (int i = hex.Length - 2; i >= 0; i -= 2)
            {
                reverseStringBuilder.Append(hex.AsSpan(i, 2));
            }

            return reverseStringBuilder.ToString();
        }


        public static bool IsValidHexChar(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'A' && c <= 'F') ||
                   (c >= 'a' && c <= 'f');
        }


        /// <summary>
        /// Tries to read a specified number of bytes from a hex string starting at a given byte offset.
        /// </summary>
        /// <param name="hex">The hex string to read from.</param>
        /// <param name="nBytes">The number of bytes to read (1-4).</param>
        /// <param name="byteOffset">The byte offset to start reading from.</param>
        /// <param name="endianness">The endianness of the hex string.</param>
        /// <param name="result">The integer value representing the read bytes.</param>
        /// <returns>True if the read was successful, otherwise false.</returns>
        public static bool TryReadNBytesFromHex(string hex, int nBytes, int byteOffset, Endianness endianness, out uint result)
        {
            result = 0;

            if (nBytes < 1 || nBytes > 4) return false;

            int offsetCharacters = byteOffset * 2;
            int nCharacters = nBytes * 2;

            if (hex.Length < offsetCharacters + nCharacters) return false;

            string hexSubString = hex.Substring(offsetCharacters, nCharacters);

            if (endianness == Endianness.Little)
            {
                hexSubString = ReverseHexEndianness(hexSubString);
            }

            if (!hexSubString.All(IsValidHexChar)) return false;

            try
            {
                result = Convert.ToUInt32(hexSubString, 16);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }


        //public static bool TryRead2Bytes(string hex, out int result)
        //{
        //    result = 0;
        //    if (hex.Length < 8) return false;

        //    try
        //    {
        //        string part = hex.Substring(4, 4);
        //        var reversed = ReverseHexEndianness(part);
        //        result = Convert.ToUInt16(reversed, 16);
        //        return true;
        //    }
        //    catch
        //    {
        //        result = 0;
        //        return false;
        //    }
        //}

        //public static bool TryRead4Bytes(string hex, out int result)
        //{
        //    result = 0;
        //    if (hex.Length < 16) return false;

        //    try
        //    {
        //        string part = hex.Substring(8, 8);
        //        var reversed = ReverseHexEndianness(part);
        //        result = (int)Convert.ToUInt32(reversed, 16);
        //        return true;
        //    }
        //    catch
        //    {
        //        result = 0;
        //        return false;
        //    }
        //}

    }
}
