using System;
using System.Windows;
using System.Windows.Controls;

namespace Netryoshka
{
    public class ChatBubbleTemplateSelector : DataTemplateSelector
    {
        private FlowsPageViewModel? _parentViewModel;
        public FlowsPageViewModel? ParentViewModel { get; set; }
        public DataTemplate? AppHttpDataTemplate { get; set; }
        public DataTemplate? AppHttpsDataTemplate { get; set; }
        public DataTemplate? AppLengthPrefixDataTemplate { get; set; }
        public DataTemplate? TcpHexDataTemplate { get; set; }
        public DataTemplate? TcpAsciiDataTemplate { get; set; }
        public DataTemplate? IpDataTemplate { get; set; }
        public DataTemplate? EthDataTemplate { get; set; }
        public DataTemplate? FrameDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (ParentViewModel is null)
            {
                throw new InvalidOperationException("SelectTemplate has null ParentViewModel");
            }
            //if (item is not FlowChatBubbleViewModel viewModel)
            //{
            //    throw new InvalidOperationException("SelectTemplate received an unexpected item");
            //}

            var selectedTemplate = ParentViewModel.SelectedNetworkLayer switch
            {
                NetworkLayer.App => ParentViewModel.SelectedDeframeMethod switch
                {
                    DeframeMethod.Http => AppHttpDataTemplate,
                    DeframeMethod.Https => AppHttpsDataTemplate,
                    DeframeMethod.LengthPrefixed => AppLengthPrefixDataTemplate,
                    _ => null
                },
                NetworkLayer.Tcp => ParentViewModel.SelectedTcpEncoding switch
                {
                    TcpEncoding.Hex => TcpHexDataTemplate,
                    TcpEncoding.Ascii => TcpAsciiDataTemplate,
                    _ => null
                },
                NetworkLayer.Ip => IpDataTemplate,
                NetworkLayer.Eth => EthDataTemplate,
                NetworkLayer.Frame => FrameDataTemplate,
                _ => null
            };

            return selectedTemplate ?? throw new InvalidOperationException($"Could not select a suitable DataTemplate for NetworkLayer: {ParentViewModel.SelectedNetworkLayer}");
        }

    }
}
