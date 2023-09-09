using System.Collections.Generic;
using System.Net;

namespace Proton.Services
{
    public class PacketCollectionService
    {
        private readonly Dictionary<FlowKey, List<BasicPacket>> _flows;
        private const int MaxFlowCount = 10_000;

        public PacketCollectionService()
        {
            _flows = new Dictionary<FlowKey, List<BasicPacket>>();
        }

        public void AddPacket(BasicPacket packet)
        {
            // If we're at the max, just return and don't add the packet.
            if (_flows.Count >= MaxFlowCount)
            {
                return;
            }

            var key = new FlowKey(
                new FlowEndpoint(packet.SrcEndpoint.IpAddress, packet.SrcEndpoint.Port),
                new FlowEndpoint(packet.DstEndpoint.IpAddress, packet.DstEndpoint.Port));

            if (!_flows.TryGetValue(key, out var packetList))
            {
                packetList = new List<BasicPacket>();
                _flows[key] = packetList;
            }

            packetList.Add(packet);
        }

        public void Clear() => _flows.Clear();

        public Dictionary<FlowKey, List<BasicPacket>> GetAllFlows()
        {
            return _flows;
        }

        public List<BasicPacket> GetAllFlowsByEndpoint(FlowEndpoint endpoint)
        {
            var result = new List<BasicPacket>();

            foreach (var key in _flows.Keys)
            {
                if (key.Endpoint1.Equals(endpoint) || key.Endpoint2.Equals(endpoint))
                {
                    result.AddRange(_flows[key]);
                }
            }

            return result;
        }

    }

    public record FlowEndpoint(IPAddress IpAddress, ushort Port);

    public class FlowKey
    {
        public FlowEndpoint Endpoint1 { get; }
        public FlowEndpoint Endpoint2 { get; }

        public FlowKey(FlowEndpoint endpoint1, FlowEndpoint endpoint2)
        {
            Endpoint1 = endpoint1;
            Endpoint2 = endpoint2;
        }

        public override bool Equals(object? obj)
        {
            if (obj is FlowKey other)
            {
                // (1111:22,3333:44) & (5555:66,7777:88) 
                return Endpoint1.Equals(other.Endpoint1) && Endpoint2.Equals(other.Endpoint2) ||
                       Endpoint1.Equals(other.Endpoint2) && Endpoint2.Equals(other.Endpoint1);
            }
            return false;
        }

        //public override int GetHashCode() => HashCode.Combine(Endpoint1, Endpoint2);

        public override int GetHashCode()
        {
            int hash1 = Endpoint1.GetHashCode();
            int hash2 = Endpoint2.GetHashCode();

            return hash1 ^ hash2; // XOR operation ensures order doesn't matter
        }


        public override string ToString() => $"{Endpoint1.IpAddress}:{Endpoint1.Port} <--> {Endpoint2.IpAddress}:{Endpoint2.Port}";
    }

}
