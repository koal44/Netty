using System;
using System.Text;
using Netryoshka.Utils;

namespace Netryoshka.Hack
{
    public static class HexFun
    {
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


        public static (string id, string accessKey) GetDataFromAccessKeyPacket(string tcpPayload)
        {
            var hex = tcpPayload;

            if (HexUtils.TryReadNBytesFromHex(hex, 2, 2, Endianness.Little, out uint messageType) && messageType == 1044)
            {
                if (HexUtils.TryReadNBytesFromHex(hex, 4, 4, Endianness.Little, out uint id))
                {
                    var accessKey = GetAccessKeyFromHexMessage(hex);
                    return (id: $"{id}", accessKey);
                }
            }

            return (id: "", accessKey: "");
        }

    }
}
