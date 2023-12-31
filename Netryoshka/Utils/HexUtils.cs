﻿using System;
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
            if (length % 2 != 0) throw new ArgumentException("Hex string lengths need to be even");

            byte[] buffer = new byte[length / 2];  // 2 hex digits represent one byte
            Random random = new();
            random.NextBytes(buffer);

            var sb = new StringBuilder();
            foreach (byte b in buffer)
            {
                sb.Append(b.ToString("X2"));  // Convert byte to 2-digit hex
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


        /// <summary>
        /// Converts a byte array to its hexadecimal string representation.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <param name="textCase">The text case for the hexadecimal string.</param>
        /// <param name="separator">The separator character to use between bytes.</param>
        /// <returns>A hexadecimal string representation of the byte array.</returns>
        public static string BytesToHex(byte[] bytes, TextCase textCase, char? separator = null)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0) return string.Empty;

            int length = (bytes.Length * 2) + (separator.HasValue ? bytes.Length - 1 : 0);
            const int MaxLength = 2621440; // 5MB = 5 * 1024 * 1024 bytes / 2 bytes per char

            if (length > MaxLength)
            {
                throw new ArgumentOutOfRangeException($"The length of the byte array is too large to convert to a hex string. The maximum character length for the resulting string is {MaxLength}.");
            }

            return string.Create(length, (bytes, textCase, separator), static (result, args) =>
            {
                var (byteArray, caseArg, sepArg) = args;
                int byteIndex = 0;
                int charIndex = 0;

                byte currentByte = byteArray[byteIndex++];
                result[charIndex++] = NibbleToHexChar(currentByte >> 4, caseArg);
                result[charIndex++] = NibbleToHexChar(currentByte, caseArg);

                while (byteIndex < byteArray.Length)
                {
                    currentByte = byteArray[byteIndex++];
                    if (sepArg.HasValue)
                    {
                        result[charIndex++] = sepArg.Value;
                    }
                    result[charIndex++] = NibbleToHexChar(currentByte >> 4, caseArg);
                    result[charIndex++] = NibbleToHexChar(currentByte, caseArg);
                }
            });
        }


        /// <summary>
        /// Converts the least significant 4 bits (rightmost nibble) of an 32-bit int to its 4-bit hexadecimal character representation.
        /// </summary>
        /// <param name="value">The 32-bit int containing the nibble to convert.</param>
        /// <param name="textCase">Specifies the text casing for the hexadecimal letter when the nibble is 10 or above.</param>
        /// <returns>A char representing the hexadecimal value of the least significant 4 bits in the int.</returns>
        public static char NibbleToHexChar(int value, TextCase textCase)
        {
            int digit = value & 0xF; // Take the least significant 4 bits (ie rightmost nibble)
            if (digit < 10)
            {
                return (char)('0' + digit);
            }
            else
            {
                return (char)((textCase == TextCase.Upper ? 'A' : 'a') + digit - 10);
            }
        }

    }
}
