using System.Net;
using System.Net.NetworkInformation;

namespace Netryoshka
{
    /// <summary>
    /// This class provides access an IPv4 TCP connection addresses and ports and its
    /// associated Process IDs and names.
    /// </summary>
    public record TcpProcessRecord(
        IPAddress LocalAddress,
        IPAddress RemoteAddress,
        ushort LocalPort,
        ushort RemotePort,
        int ProcessId,
        ITcpProcessState State,
        //MibTcpState State,
        string? ProcessName)
    {
        public override string ToString()
        {
            return $"{ProcessName}, {ProcessId}, {LocalPort}";
        }
    }

    public interface ITcpProcessState
    {
        string StateDescription { get; }
    }




}
