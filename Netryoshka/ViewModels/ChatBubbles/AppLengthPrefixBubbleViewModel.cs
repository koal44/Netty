using Netryoshka.ViewModels.ChatBubbles;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("AppLengthPrefix")]
    public partial class AppLengthPrefixBubbleViewModel : BubbleViewModelBase
    {
        public AppLengthPrefixBubbleViewModel()
            : base()
        { }

        public AppLengthPrefixBubbleViewModel(BubbleData data)
            : base(data)
        {
            BodyContent = $"IP: {data.BasicPacket.SrcEndpoint.IpAddress} ⟶ {data.BasicPacket.DstEndpoint.IpAddress}";
        }
    }
}
