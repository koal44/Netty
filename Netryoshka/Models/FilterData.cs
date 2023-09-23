using System.Collections.Generic;
using System.Net;
using static Netryoshka.BasicPacket;

namespace Netryoshka
{
    public readonly struct FilterData
    {
        public HashSet<int> RemotePorts { get; }
        public HashSet<IPAddress> RemoteIpAddresses { get; }
        public HashSet<int> LocalPorts { get; }
        public HashSet<int> LocalPIDs { get; }
        public HashSet<string> LocalProcessNames { get; }
        public string CustomFilter { get; }
        public bool LogDNSLookups { get; }

        public FilterData(
            HashSet<int>? remotePorts = null,
            HashSet<IPAddress>? remoteIPAddresses = null,
            HashSet<int>? localPorts = null,
            HashSet<int>? localPIDs = null,
            HashSet<string>? localProcessNames = null,
            string? customFilter = null,
            bool? logDnsLookups = null)
        {
            RemotePorts = remotePorts ?? new HashSet<int>();
            RemoteIpAddresses = remoteIPAddresses ?? new HashSet<IPAddress>();
            LocalPorts = localPorts ?? new HashSet<int>();
            LocalPIDs = localPIDs ?? new HashSet<int>();
            LocalProcessNames = localProcessNames ?? new HashSet<string>();
            CustomFilter = customFilter ?? string.Empty;
            LogDNSLookups = logDnsLookups ?? false;
        }

        public bool ShouldKeepPacket(BasicPacket packet)
        {
            if (CustomFilter != string.Empty) return true;
            if (RemotePorts.Contains(packet.DstEndpoint.Port) && IsOutgoing(packet.Direction)) return true;
            if (RemotePorts.Contains(packet.SrcEndpoint.Port) && IsIncoming(packet.Direction)) return true;
            if (RemoteIpAddresses.Contains(packet.DstEndpoint.IpAddress) && IsOutgoing(packet.Direction)) return true;
            if (RemoteIpAddresses.Contains(packet.SrcEndpoint.IpAddress) && IsIncoming(packet.Direction)) return true;
            if (LocalPorts.Contains(packet.DstEndpoint.Port) && IsIncoming(packet.Direction)) return true;
            if (LocalPorts.Contains(packet.SrcEndpoint.Port) && IsOutgoing(packet.Direction)) return true;
            if (packet.ProcessInfo?.ProcessId is int processId && LocalPIDs.Contains(processId)) return true;
            if (packet.ProcessInfo?.ProcessName is string processName && !string.IsNullOrEmpty(processName) && LocalProcessNames.Contains(processName)) return true;

            return false;
        }

        private static bool IsOutgoing(BPDirection direction)
        {
            return direction == BPDirection.Outgoing || direction == BPDirection.Unknown;
        }

        private static bool IsIncoming(BPDirection direction)
        {
            return direction == BPDirection.Incoming || direction == BPDirection.Unknown;
        }


    }

}
