using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netryoshka.DesignTime;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Netryoshka
{
    public partial class FrameSharkTextBubbleViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _bubbleScale = 0.8;
        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _footerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private bool _isExpanded;
        [ObservableProperty]
        private string? _firstPartOfText;
        [ObservableProperty]
        private string? _restOfText;

        public FrameSharkTextBubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                BodyContent = "BodyContent";
                FirstPartOfText = "FirstPartOfText";
                RestOfText = "RestOfText";
                EndPointRole = FlowEndpointRole.Pivot;
                FooterContent = TimeSpan.Zero.ToString("mm\\.ss\\.ffff");
            }
        }

        public FrameSharkTextBubbleViewModel(BubbleData data)
        {
            BodyContent = data.WireSharkData!.JsonString;
            var lines = BodyContent.Split('\n');
            FirstPartOfText = string.Join("\n", lines.Take(5));
            RestOfText = lines.Length > 5 ? string.Join("\n", lines.Skip(5)) : string.Empty;
            IsExpanded = false;
            EndPointRole = data.EndPointRole;
            FooterContent = data.PacketInterval?.ToString("mm\\.ss\\.ffff");
        }


        [RelayCommand]
        private void CopyText()
        {
            Clipboard.SetText(BodyContent);
        }

    }
}
