using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Netryoshka
{
    public interface IFlowsPageViewModel
    {
        ObservableCollection<FlowEndpoint> PivotEndpoints { get; }
        ObservableCollection<FlowEndpoint> OrbitEndpoints { get; }
        ObservableCollection<BubbleData> CurrentBubbleDataCollection { get; }

        FlowEndpoint? SelectedPivotEndpoint { get; set; }
        FlowEndpoint? SelectedOrbitEndpoint { get; set; }
        FlowEndpointRole? SelectedBotRole { get; set; }
        string PivotProcessInfo { get; }
        TcpEncoding SelectedTcpEncoding { get; set; }
        NetworkLayer SelectedNetworkLayer { get; set; }
        DeframeMethod? SelectedDeframeMethod { get; set; }
        int MessagePrefixLength { get; set; }
        int MessageTypeLength { get; set; }

        event PropertyChangedEventHandler? PropertyChanged;
    }

}
