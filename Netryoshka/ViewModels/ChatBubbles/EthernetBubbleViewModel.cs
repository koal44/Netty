using Netryoshka.Extensions;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("Eth")]
    public partial class EthernetBubbleViewModel : TextBubbleViewModel
    {
        public EthernetBubbleViewModel()
            : base()
        { }

        public EthernetBubbleViewModel(BubbleData data)
            : base(data)
        {
            BodyContent = GetEthernetContent(data.BasicPacket);
        }


        private static string? GetEthernetContent(BasicPacket packet)
        {
            string srcAddress = packet.SrcEndpoint.MacAddress.ToFormattedString(MacAddressFormat.ColonSeparated);
            string dstAddress = packet.DstEndpoint.MacAddress.ToFormattedString(MacAddressFormat.ColonSeparated);

            return @$"MAC: {srcAddress} ⟶ {dstAddress}";
        }
    }
}
