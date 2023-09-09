using System;
using System.Text;

namespace Netty.Utils
{
    public static class HexFun
    {

        public static string ReverseEndianness(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Invalid hex string length.");

            var reverseStringBuilder = new StringBuilder();
            for (int i = hex.Length - 2; i >= 0; i -= 2)
            {
                reverseStringBuilder.Append(hex.AsSpan(i, 2));
            }

            return reverseStringBuilder.ToString();
        }

        public static bool TryRead2Bytes(string hex, out int result)
        {
            result = 0;
            if (hex.Length < 8) return false;

            try
            {
                string part = hex.Substring(4, 4);
                var reversed = ReverseEndianness(part);
                result = Convert.ToUInt16(reversed, 16);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        public static bool TryRead4Bytes(string hex, out int result)
        {
            result = 0;
            if (hex.Length < 16) return false;

            try
            {
                string part = hex.Substring(8, 8);
                var reversed = ReverseEndianness(part);
                result = (int)Convert.ToUInt32(reversed, 16);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }


        public static int Read2Bytes(string hex)
        {
            string part = hex.Substring(4, 4);
            var reversed = ReverseEndianness(part);
            return Convert.ToUInt16(reversed, 16);
        }

        public static int Read4Bytes(string hex)
        {
            string part = hex.Substring(8, 8);
            var reversed = ReverseEndianness(part);
            return (int)Convert.ToUInt32(reversed, 16);
        }

        public static string GetAccessKeyFromHexMessage(string hex)
        {
            int startIndex = hex.ToLower().IndexOf("a001");
            if (startIndex == -1)
            {
                return string.Empty;
            }

            int currentIndex = startIndex + 4;
            var result = new StringBuilder();
            while (currentIndex + 2 <= hex.Length)
            {
                string byteString = hex.Substring(currentIndex, 2);

                if (byteString == "00")
                    break;

                result.Append(Convert.ToChar(Convert.ToUInt32(byteString, 16)));
                currentIndex += 2;
            }

            return result.ToString();
        }


    }
}
