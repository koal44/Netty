using System.Net;

namespace Netryoshka.ViewModels
{
    public class TcpProcessRecordViewModel
    {
        private readonly TcpProcessRecord _record;

        public TcpProcessRecordViewModel(TcpProcessRecord record)
        {
            _record = record;
            SortableLocalAddress = new SortableIPAddress(_record.LocalAddress.GetAddressBytes());
            SortableRemoteAddress = new SortableIPAddress(_record.RemoteAddress.GetAddressBytes());
        }

        public IPAddress LocalAddress => _record.LocalAddress;
        public IPAddress RemoteAddress => _record.RemoteAddress;
        public ushort LocalPort => _record.LocalPort;
        public ushort RemotePort => _record.RemotePort;
        public int ProcessId => _record.ProcessId;
        public ITcpProcessState State => _record.State;
        public string? ProcessName => _record.ProcessName;

        public SortableIPAddress SortableLocalAddress { get; }
        public SortableIPAddress SortableRemoteAddress { get; }
    }

}
