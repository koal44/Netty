using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Netryoshka.Utils
{
    public static class Util
    {

        private static readonly Random _random = new();




        /// <summary>
        /// Joins an array of strings with a given separator, ignoring any null or empty strings.
        /// </summary>
        /// <param name="separator">The string to use as a separator.</param>
        /// <param name="items">An array of strings to join.</param>
        /// <returns>A string containing all the array elements separated by the separator character.</returns>
        public static string StringJoin(string separator, params string[] items)
        {
            if (items == null)
            {
                return string.Empty;
            }

            var filteredItems = items.Where(item => !string.IsNullOrEmpty(item));
            return string.Join(separator, filteredItems);
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
                arr[i] = (byte)((GetHexValue(hex[i << 1]) << 4) + GetHexValue(hex[(i << 1) + 1]));
            }

            return arr;
        }

        /// <summary>
        /// Gets the integer value of a hex character.
        /// </summary>
        /// <param name="hexChar">The hex character to convert.</param>
        /// <returns>The integer value of the hex character.</returns>
        private static int GetHexValue(char hexChar)
        {
            int val = hexChar;
            // Convert the hex character to its integer value.
            // ASCII values for '0' to '9' are 48-57.
            // ASCII values for 'A' to 'F' are 65-70.
            // ASCII values for 'a' to 'f' are 97-102.
            return val - (val < 58 ? 48 : val < 97 ? 55 : 87);
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
            _random.NextBytes(buffer);

            var sb = new StringBuilder();
            foreach (byte b in buffer)
            {
                sb.Append(b.ToString("X2"));  // Convert byte to 2-digit hex
            }

            // If odd, add one more _random digit
            if (length % 2 != 0)
            {
                sb.Append(_random.Next(0, 16).ToString("X"));
            }

            return sb.ToString();
        }

        public static string BytesToAscii(byte[] buffer, bool keepSpacing = true)
        {
            StringBuilder sb = new StringBuilder();

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

                // ASCII control characters are from 0x00 to 0x1F
                //sb.Append(c >= 0x00 && c <= 0x1F ? '.' : c);

                // Only append printable ASCII characters, replace anything else with '.'
                sb.Append((c >= 0x20 && c <= 0x7E) ? c : '.');
            }

            return sb.ToString();
        }



        //public static string BytesToEscapedString(byte[] buffer, bool keepSpacing = true, Encoding? encoding = null)
        //{
        //    encoding ??= Encoding.ASCII;
        //    string str = encoding.GetString(buffer);
        //    StringBuilder sb = new StringBuilder();

        //    foreach (char c in str)
        //    {
        //        if (keepSpacing)
        //        {
        //            switch (c)
        //            {
        //                case '\n': sb.Append("\\n"); continue;
        //                case '\r': sb.Append("\\r"); continue;
        //                case '\t': sb.Append("\\t"); continue;
        //            }
        //        }

        //        // ASCII control characters are from 0x00 to 0x1F
        //        sb.Append(c >= 0x00 && c <= 0x1F
        //            ? '.'
        //            : c);
        //    }

        //    return sb.ToString();
        //}

        /* if ((b >= 32 && b <= 126) || b == 9 || b == 10)  // ASCII printable characters + tab and newline
            {
                sb.Append((char)b);
            }
            else
            {
                sb.Append('.');
            }*/


    }

}
