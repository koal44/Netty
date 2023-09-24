using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Netryoshka
{
    public interface IFlowsPageViewModel
    {
        ObservableCollection<InteractionEndpoint> PivotEndpoints { get; }
        ObservableCollection<InteractionEndpoint> OrbitEndpoints { get; }
        List<BubbleData> CurrentBubbleDataList { get; }
        InteractionEndpoint? SelectedPivotEndpoint { get; set; }
        InteractionEndpoint? SelectedOrbitEndpoint { get; set; }
        FlowEndpointRole? SelectedBotRole { get; set; }
        string PivotProcessInfo { get; }
        TcpEncoding SelectedTcpEncoding { get; set; }
        NetworkLayer SelectedNetworkLayer { get; set; }
        DeframeMethod SelectedDeframeMethod { get; set; }
        int MessagePrefixLength { get; set; }
        int MessageTypeLength { get; set; }

        event PropertyChangedEventHandler? PropertyChanged;
    }

}
