using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Netryoshka.DesignTime
{
    public class FlowsPageDesignViewModel
    {
#pragma warning disable CS0067 // The event 'FlowsPageDesignViewModel.PropertyChanged' is never used
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067 // The event 'FlowsPageDesignViewModel.PropertyChanged' is never used
        public ObservableCollection<BubbleData> CurrentFlowChatBubbles { get; set; }
        public ObservableCollection<FlowEndpoint> PivotEndpoints { get; set; }
        public ObservableCollection<FlowEndpoint> OrbitEndpoints { get; set; }
        public FlowEndpoint? SelectedPivotEndpoint { get; set; }
        public FlowEndpoint? SelectedOrbitEndpoint { get; set; }
        public FlowEndpointRole? SelectedBotRole { get; set; }
        public string PivotProcessInfo { get; set; }
        public TcpEncoding SelectedTcpEncoding { get; set; }
        public NetworkLayer SelectedNetworkLayer { get; set; }
        public DeframeMethod SelectedDeframeMethod { get; set; }
        public int MessagePrefixLength { get; set; }
        public int MessageTypeLength { get; set; }
        public string KeyLogFileName { get; set; }

        public static FlowsPageDesignViewModel Instance { get; } = new FlowsPageDesignViewModel();

        public FlowsPageDesignViewModel()
        {
            CurrentFlowChatBubbles = new ObservableCollection<BubbleData>();
            PivotEndpoints = new ObservableCollection<FlowEndpoint>();
            OrbitEndpoints = new ObservableCollection<FlowEndpoint>();
            PivotProcessInfo = "SystemService";
            SelectedNetworkLayer = NetworkLayer.Frame;
            SelectedTcpEncoding = TcpEncoding.Hex;
            SelectedDeframeMethod = DeframeMethod.Tls;
            KeyLogFileName = "path/to/keylogfile.txt";

            DateTime? lastTimestamp = null;

            var packets = DesignTimeData.GetPackets();
            for (int index = 1; index < packets.Count; index++)
            {
                var packet = packets[index];
                var role = packet.Direction == BasicPacket.BPDirection.Incoming
                            ? FlowEndpointRole.Pivot
                            : FlowEndpointRole.Orbit;
                var packetInterval = lastTimestamp.HasValue
                            ? packet.Timestamp - lastTimestamp.Value
                            : TimeSpan.Zero;

                CurrentFlowChatBubbles.Add(
                    new BubbleData(packet, role, packetInterval, index)
                );

                lastTimestamp = packet.Timestamp;
            }
        }
    }
}
