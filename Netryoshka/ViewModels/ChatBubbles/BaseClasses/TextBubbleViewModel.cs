using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    public partial class TextBubbleViewModel : BubbleViewModel
    {
        [ObservableProperty]
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;


        public TextBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                HeaderContent = "Header";
                BodyContent = "Body";
            }
        }


        public TextBubbleViewModel(BubbleData data)
        : base(data)
        {
        }


        [RelayCommand]
        private void CopyHeader()
        {
            Clipboard.SetText(HeaderContent);
        }


        [RelayCommand]
        private void CopyBody()
        {
            Clipboard.SetText(BodyContent);
        }
    }
}
