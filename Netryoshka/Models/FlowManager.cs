using System.Collections.Generic;

namespace Netty.Services
{
    public class FlowManager
    {
        private readonly Dictionary<FlowKey, List<BasicPacket>> _flows;
        private const int MaxFlowCount = 10_000;

        public FlowManager()
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
                new FlowEndpoint(packet.SrcEndpoint.IpAddress, packet.SrcEndpoint.Port, packet.SrcEndpoint.MacAddress),
                new FlowEndpoint(packet.DstEndpoint.IpAddress, packet.DstEndpoint.Port, packet.DstEndpoint.MacAddress));

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

}
