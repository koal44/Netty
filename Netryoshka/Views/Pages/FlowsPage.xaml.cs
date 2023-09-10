using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Netryoshka
{
    public partial class FlowsPage : Page
    {
        public FlowsPageViewModel ViewModel { get; }
        public Dictionary<string, DataTemplate> FlowChatBubbleTemplatesByName { get; }

        public FlowsPage(FlowsPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();

            FlowChatBubbleTemplatesByName = new Dictionary<string, DataTemplate>
            {
                { "AppNull", (DataTemplate)FindResource("AppNullDataTemplate") },
                { "AppHttp", (DataTemplate)FindResource("AppHttpDataTemplate") },
                { "AppHttps", (DataTemplate)FindResource("AppHttpsDataTemplate") },
                { "AppLengthPrefix", (DataTemplate)FindResource("AppLengthPrefixDataTemplate") },
                { "TcpHex", (DataTemplate)FindResource("TcpHexDataTemplate") },
                { "TcpAscii", (DataTemplate)FindResource("TcpAsciiDataTemplate") },
                { "Ip", (DataTemplate)FindResource("IpDataTemplate") },
                { "Eth", (DataTemplate)FindResource("EthDataTemplate") },
                { "Frame", (DataTemplate)FindResource("FrameDataTemplate") }
            };

            RefreshChatBubbleTemplates();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedNetworkLayer):
                case nameof(ViewModel.SelectedTcpEncoding):
                case nameof(ViewModel.SelectedDeframeMethod):
                case nameof(ViewModel.MessagePrefixLength):
                case nameof(ViewModel.MessageTypeLength):
                    RefreshChatBubbleTemplates();
                    break;
            }
        }

        private void RefreshChatBubbleTemplates()
        {
            string key = ViewModel.SelectedNetworkLayer switch
            {
                NetworkLayer.App => $"App{(ViewModel.SelectedDeframeMethod.HasValue ? ViewModel.SelectedDeframeMethod : "Null")}",
                NetworkLayer.Tcp => $"Tcp{ViewModel.SelectedTcpEncoding}",
                _ => $"{ViewModel.SelectedNetworkLayer}"
            };

            FlowChatBubbleTemplatesByName.TryGetValue(key, out var template);
            if (template is null)
                throw new InvalidOperationException($"Could not select a suitable Template for key '{key}'");

            if (FlowChatListBox.ItemsSource is not ObservableCollection<BubbleData> flowChatBubbles)
                throw new InvalidOperationException("Unexpected FlowChatListBox.ItemsSource type.");

            //if (FlowChatListBox.ItemsSource is not ObservableCollection<IFlowChatBubbleViewModel> flowChatBubbles)
            //    throw new InvalidOperationException(
            //$"Unexpected FlowChatListBox.ItemsSource type. Expected: ObservableCollection<BubbleData>, " +
            //$"Actual: {FlowChatListBox.ItemsSource?.GetType().FullName ?? "null"}");

            foreach (var item in flowChatBubbles)
            {
                item.Template = template;
            }
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.ParseAsHttpAsync();
        }

    }




}
