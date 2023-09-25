using System;
using System.Net;

namespace Netryoshka
{
    public class SortableIPAddress : IPAddress, IComparable<SortableIPAddress>, IComparable
    {
        public SortableIPAddress(byte[] address) : base(address) { }
        public SortableIPAddress(long newAddress) : base(newAddress) { }
        public SortableIPAddress(ReadOnlySpan<byte> address) : base(address) { }
        public SortableIPAddress(byte[] address, long scopeid) : base(address, scopeid) { }
        public SortableIPAddress(ReadOnlySpan<byte> address, long scopeid) : base(address, scopeid) { }

        public int CompareTo(SortableIPAddress? other)
        {
            if (other == null)
                return 1;

            var result = AddressFamily.CompareTo(other.AddressFamily);
            if (result != 0)
                return result;

            var xBytes = GetAddressBytes();
            var yBytes = other.GetAddressBytes();

            var octets = Math.Min(xBytes.Length, yBytes.Length);
            for (var i = 0; i < octets; i++)
            {
                var octetResult = xBytes[i].CompareTo(yBytes[i]);
                if (octetResult != 0)
                    return octetResult;
            }
            return 0;
        }

        public int CompareTo(object? other)
        {
            // if other is null, the more specific overload should be called, so we don't worry about it here
            if (other is SortableIPAddress otherIp)
                return CompareTo(otherIp);
            throw new ArgumentException("Object is not a SortableIPAddress");
        }
    }
}
