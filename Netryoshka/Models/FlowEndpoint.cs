using System.Net;
using System.Net.NetworkInformation;

namespace Netty
{
    public record FlowEndpoint(IPAddress IpAddress, ushort Port, PhysicalAddress MacAddress)
    {
        public override string ToString() => $"{IpAddress} [{Port}] [{MacAddress}]";
    }

}
