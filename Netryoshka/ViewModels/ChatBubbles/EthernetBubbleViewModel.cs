using CommunityToolkit.Mvvm.ComponentModel;
using Netryoshka.DesignTime;
using Netryoshka.Extensions;
using System;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka
{
    public partial class EthernetBubbleViewModel : ObservableObject
    {

        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private string? _footerContent;

        public EthernetBubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var packet = DesignTimeData.GetPackets()[0];
                EndPointRole = FlowEndpointRole.Pivot;
                HeaderContent = null;
                BodyContent = GetEthernetContent(packet);
                FooterContent = TimeSpan.Zero.ToString("mm\\.ss\\.ffff");
            }
        }

        public EthernetBubbleViewModel(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            HeaderContent = null;
            BodyContent = GetEthernetContent(data.BasicPacket);
            FooterContent = data.PacketInterval?.ToString("mm\\.ss\\.ffff");
        }

        private static string? GetEthernetContent(BasicPacket packet)
        {
            string srcAddress = packet.SrcEndpoint.MacAddress.ToFormattedString(MacAddressFormat.ColonSeparated);
            string dstAddress = packet.DstEndpoint.MacAddress.ToFormattedString(MacAddressFormat.ColonSeparated);

            return @$"MAC: {srcAddress} ⟶ {dstAddress}";
        }
    }
}
