using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Netty
{
    public partial class MatryoshkaBoxViewModel : ObservableObject
    {
        public IFlowsPageViewModel? ParentViewModel { get; set; }

        [ObservableProperty]
        private string _currentLayerOnDisplay = string.Empty;

        public MatryoshkaBoxViewModel()
        {

        }

        [RelayCommand]
        public void SetNetworkLayer(object parameter)
        {
            if (parameter is not string tag) return;
            if (ParentViewModel is null) throw new InvalidOperationException("ParentViewModel must not be null");

            ParentViewModel.SelectedNetworkLayer = tag switch
            {
                "Frame" => NetworkLayer.Frame,
                "Eth" => NetworkLayer.Eth,
                "IP" => NetworkLayer.Ip,
                "TCP" => NetworkLayer.Tcp,
                "App" => NetworkLayer.App,
                _ => throw new NotImplementedException()
            };
        }

    }
}
