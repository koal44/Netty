using Netryoshka.Services;

namespace Netryoshka
{
    public class InteractionEndpoint
    {
        public FlowKey FlowKey { get; }
        public FlowEndpoint FlowEndpoint { get; }
        public TcpProcessRecord? ProcessInfo { get; set; }
        public string? DomainName { get; set; }

        public InteractionEndpoint(FlowKey flowKey, FlowEndpoint flowEndpoint, TcpProcessRecord? processInfo = null, string? domainName = null)
        {
            FlowKey = flowKey;
            FlowEndpoint = flowEndpoint;
            ProcessInfo = processInfo;
            DomainName = domainName;
        }
    }
}
