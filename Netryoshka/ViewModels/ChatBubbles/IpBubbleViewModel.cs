namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("Ip")]
    public partial class IpBubbleViewModel : TextBubbleViewModel
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
