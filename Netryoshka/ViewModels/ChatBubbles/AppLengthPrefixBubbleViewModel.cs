namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RegisterBubbleViewModel("AppLengthPrefix")]
    public partial class AppLengthPrefixBubbleViewModel : BubbleViewModel
    {
        public AppLengthPrefixBubbleViewModel()
            : base()
        { }

        public AppLengthPrefixBubbleViewModel(BubbleData data)
            : base(data)
        {
        }
    }
}
