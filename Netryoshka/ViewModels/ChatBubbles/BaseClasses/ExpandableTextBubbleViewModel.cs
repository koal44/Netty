using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows;

namespace Netryoshka.ViewModels
{
    public partial class ExpandableTextBubbleViewModel : BubbleViewModel
    {
        [ObservableProperty]
        private bool _isExpanded;
        [ObservableProperty]
        private string? _bodyContent;
        //[ObservableProperty]
        //private string? _firstPartOfText;
        //[ObservableProperty]
        //private string? _restOfText;


        public ExpandableTextBubbleViewModel()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                IsExpanded = false;
                BodyContent = "BodyContent";
                //FirstPartOfText = "FirstPartOfText";
                //RestOfText = "RestOfText";
            }
        }


        public ExpandableTextBubbleViewModel(BubbleData data)
            : base(data)
        {
            IsExpanded = false;
        }


        //[RelayCommand]
        //protected virtual void CopyText()
        //{
        //    Clipboard.SetText(BodyContent);
        //}

        [RelayCommand]
        private void CopyBody()
        {
            Clipboard.SetText(BodyContent);
        }
    }
}
