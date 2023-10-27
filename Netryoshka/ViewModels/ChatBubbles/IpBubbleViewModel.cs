using Netryoshka.ViewModels.ChatBubbles;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("Ip")]
    public partial class IpBubbleViewModel : BubbleViewModelBase
    {
        public IpBubbleViewModel()
            : base()
        { }

        public IpBubbleViewModel(BubbleData data)
            : base(data)
        {
            BodyContent = $"IP: {data.BasicPacket.SrcEndpoint.IpAddress} ⟶ {data.BasicPacket.DstEndpoint.IpAddress}";
        }
    }
}
