using System.Collections.Generic;
using System.Threading.Tasks;

namespace Netryoshka
{
    public interface ISocketProcessMapperService
    {
        List<TcpProcessRecord> GetAllTcpConnections();
        List<UdpProcessRecord> GetAllUdpConnections();
        Task<List<int>> GetLocalTcpPortsFromProcesses(HashSet<string> processNames, HashSet<int> processIDs);
    }
}
