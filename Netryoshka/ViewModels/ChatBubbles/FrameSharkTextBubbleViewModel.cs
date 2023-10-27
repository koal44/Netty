using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netryoshka.ViewModels.ChatBubbles;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RequiresWireShark]
    [RegisterBubbleViewModel("FrameSharkText")]
    public partial class FrameSharkTextBubbleViewModel : BubbleViewModelBase
    {
        [ObservableProperty]
        private bool _isExpanded;
        [ObservableProperty]
        private string? _firstPartOfText;
        [ObservableProperty]
        private string? _restOfText;


        public FrameSharkTextBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                BodyContent = "BodyContent";
                FirstPartOfText = "FirstPartOfText";
                RestOfText = "RestOfText";
            }
        }


        public FrameSharkTextBubbleViewModel(BubbleData data)
            : base(data)
        {
            BodyContent = data.WireSharkData?.JsonString ?? string.Empty;
            var lines = BodyContent.Split('\n');
            FirstPartOfText = string.Join("\n", lines.Take(5));
            RestOfText = lines.Length > 5 ? string.Join("\n", lines.Skip(5)) : string.Empty;
            IsExpanded = false;
        }


        [RelayCommand]
        private void CopyText()
        {
            Clipboard.SetText(BodyContent);
        }

    }
}
