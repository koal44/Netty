using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Netryoshka.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using static Netryoshka.BasicPacket;
using System.ComponentModel;

namespace Netryoshka
{
    public partial class FlowsPageViewModel : ObservableRecipient, IFlowsPageViewModel
    {

        private readonly FlowManager _flowManager;
        private readonly ILogger _logger;
        private readonly TSharkService _tSharkService;
        private readonly Dictionary<FlowKey, List<BasicPacket>> _allFlows;

        [ObservableProperty]
        private ObservableCollection<FlowEndpoint> _pivotEndpoints;

        [ObservableProperty]
        private ObservableCollection<FlowEndpoint> _orbitEndpoints;

        private ObservableCollection<BubbleData> _currentBubbleDataCollection;

        public ObservableCollection<BubbleData> CurrentBubbleDataCollection
        {
            get => _currentBubbleDataCollection;
            set
            {
                SetProperty(ref _currentBubbleDataCollection, value);
                UpdateBubbleItemsViewModels();
            }
        }

        [ObservableProperty]
        private ObservableCollection<object> _currentItemViewModelCollecion;

        private FlowEndpoint? _selectedPivotEndpoint;
        public FlowEndpoint? SelectedPivotEndpoint
        {
            get => _selectedPivotEndpoint;
            set
            {
                SetProperty(ref _selectedPivotEndpoint, value);
                UpdateOrbitEndpoints();
            }
        }

        private FlowEndpoint? _selectedOrbitEndpoint;
        public FlowEndpoint? SelectedOrbitEndpoint
        {
            get => _selectedOrbitEndpoint;
            set
            {
                SetProperty(ref _selectedOrbitEndpoint, value);
                //UpdateCurrentChatBubbles();
            }
        }

        private FlowKey? CurrentFlowKey { get; set; }

        [ObservableProperty]
        private string _pivotProcessInfo;

        private FlowEndpoint? _selectedFrameEndpoint;
        public FlowEndpoint? SelectedFrameEndpoint
        {
            get => _selectedFrameEndpoint;
            set
            {
                SetProperty(ref _selectedFrameEndpoint, value);
                IsSeriousBotSelected = SelectedPivotEndpoint?.Equals(_selectedFrameEndpoint) == true;
                IsDitzyBotSelected = SelectedOrbitEndpoint?.Equals(_selectedFrameEndpoint) == true;
            }
        }

        [ObservableProperty]
        private bool _isSeriousBotSelected;
        [ObservableProperty]
        private bool _isDitzyBotSelected;
        
        // these properties control the display of the chat bubbles
        [ObservableProperty]
        private NetworkLayer _selectedNetworkLayer;
        [ObservableProperty]
        private FrameDisplay _selectedFrameDisplay;
        [ObservableProperty]
        private TcpEncoding _selectedTcpEncoding;
        [ObservableProperty]
        private DeframeMethod? _selectedDeframeMethod;
        [ObservableProperty]
        private int _messagePrefixLength;
        [ObservableProperty]
        private int _messageTypeLength;

        public static IEnumerable<DeframeMethod> DeframeMethods => Enum.GetValues(typeof(DeframeMethod)).Cast<DeframeMethod>();
        [ObservableProperty]
        private string? _keyLogFileName;

        public FlowsPageViewModel(FlowManager flowManager, ILogger logger, TSharkService tSharkService)
        {
            _flowManager = flowManager;
            _logger = logger;
            _tSharkService = tSharkService;
            _allFlows = _flowManager.GetAllFlows();
            _pivotEndpoints = new();
            _orbitEndpoints = new();
            _currentBubbleDataCollection = new();
            _pivotProcessInfo = string.Empty;
            _isSeriousBotSelected = false;
            _isDitzyBotSelected = false;
            _selectedNetworkLayer = NetworkLayer.Tcp;
            _selectedFrameDisplay = FrameDisplay.NoShark;
            _selectedTcpEncoding = TcpEncoding.Hex;
            _selectedDeframeMethod = null;
            _currentItemViewModelCollecion = new();
            PropertyChanged += OnDisplayControlsPropertyChanged;

            // sets off a chain of side reactions which updates orbit, updating flowmessages.
            UpdatePivotEndpoints();


            //CurrentBubbleDataCollection.CollectionChanged += OnCurrentFlowChatBubblesChanged;
        }

        private void OnDisplayControlsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SelectedNetworkLayer):
                case nameof(SelectedTcpEncoding):
                case nameof(SelectedDeframeMethod):
                case nameof(MessagePrefixLength):
                case nameof(MessageTypeLength):
                    UpdateBubbleItemsViewModels();
                    break;
            }
        }

        private readonly Dictionary<string, Type> BubbleViewModelsByName = new()
        {
            //{ "AppNull", typeof(AppNullBubbleViewModel) },
            //{ "AppHttp", typeof(AppHttpBubbleViewModel) },
            //{ "AppHttps", typeof(AppHttpsBubbleViewModel) },
            //{ "AppLengthPrefix", typeof(AppLengthPrefixBubbleViewModel) },
            { "TcpHex", typeof(TcpHexBubbleViewModel) },
            { "TcpAscii", typeof(TcpAsciiBubbleViewModel) },
            //{ "Ip", typeof(IpBubbleViewModel) },
            { "Eth", typeof(EthernetBubbleViewModel) },
            { "Frame", typeof(FrameBubbleViewModel) }
        };

        private void UpdateBubbleItemsViewModels()
        {
            string key = SelectedNetworkLayer switch
            {
                NetworkLayer.App => $"App{(SelectedDeframeMethod.HasValue ? SelectedDeframeMethod : "Null")}",
                NetworkLayer.Tcp => $"Tcp{SelectedTcpEncoding}",
                _ => $"{SelectedNetworkLayer}"
            };

            BubbleViewModelsByName.TryGetValue(key, out var viewModelType);
            if (viewModelType is null)
                throw new InvalidOperationException($"Could not select a suitable view model for key '{key}'");

            CurrentItemViewModelCollecion.Clear();
            foreach (var bubbleData in CurrentBubbleDataCollection)
            {
                var viewModel = Activator.CreateInstance(viewModelType, bubbleData)
                    ?? throw new InvalidOperationException($"Could not create a view model of type '{viewModelType}'");
                CurrentItemViewModelCollecion.Add(viewModel);
            }
        }

        private void UpdatePivotEndpoints()
        {
            SelectedPivotEndpoint = null;
            PivotEndpoints.Clear();

            var uniquePivots = new HashSet<FlowEndpoint>();
            foreach (var flowKey in _allFlows.Keys)
            {
                var firstPacket = _allFlows[flowKey].FirstOrDefault();
                if (firstPacket == null)
                {
                    _logger.Warn($"First firstpacket was null for flow key: {flowKey}");
                    continue;
                }

                switch (firstPacket.Direction)
                {
                    case BPDirection.Outgoing:
                        uniquePivots.Add(firstPacket.SrcEndpoint);
                        break;
                    case BPDirection.Incoming:
                        uniquePivots.Add(firstPacket.DstEndpoint);
                        break;
                    default:
                        uniquePivots.Add(firstPacket.SrcEndpoint);
                        uniquePivots.Add(firstPacket.DstEndpoint);
                        break;
                }
            }

            foreach (var pivot in uniquePivots)
            {
                PivotEndpoints.Add(pivot);
            }

            SelectedPivotEndpoint = PivotEndpoints.FirstOrDefault();
            if (SelectedFrameEndpoint == null && PivotEndpoints.Count > 0)
            {
                SelectedFrameEndpoint = PivotEndpoints.First();
            }
            UpdateOrbitEndpoints();
        }

        private void UpdateOrbitEndpoints()
        {
            SelectedOrbitEndpoint = null;
            OrbitEndpoints.Clear();

            if (SelectedPivotEndpoint != null)
            {
                foreach (var flowKey in _allFlows.Keys)
                {
                    var orbitEndpoint = SelectedPivotEndpoint switch
                    {
                        var pivot when pivot.Equals(flowKey.Endpoint1) => flowKey.Endpoint2,
                        var pivot when pivot.Equals(flowKey.Endpoint2) => flowKey.Endpoint1,
                        _ => null
                    };

                    if (orbitEndpoint != null)
                    {
                        OrbitEndpoints.Add(orbitEndpoint);
                    }
                }

                if (OrbitEndpoints.Any())
                {
                    SelectedOrbitEndpoint = OrbitEndpoints.First();
                }
                else
                {
                    _logger.Error("OrbitEndpoints is empty. FlowManager should have both endpoints.");
                }
            }

            UpdateCurrentFlow();
        }

        private void UpdateCurrentFlow()
        {
            CurrentFlowKey = null;

            if (SelectedPivotEndpoint != null && SelectedOrbitEndpoint != null)
            {
                CurrentFlowKey = new FlowKey(SelectedPivotEndpoint, SelectedOrbitEndpoint);
            }

            UpdateCurrentChatBubbles();
            UpdatePivotProcessInfo();
        }

        private void UpdatePivotProcessInfo()
        {
            PivotProcessInfo = string.Empty;

            if (CurrentFlowKey != null)
            {
                var packets = _allFlows[CurrentFlowKey];
                if (packets == null || packets.Count <= 0) return; // shouldn't happen

                var firstpacket = packets[0];

                var processInfo = firstpacket.ProcessInfo;
                var processName = processInfo?.ProcessName ?? "";
                var processId = processInfo?.ProcessId != null ? $"pid {processInfo.ProcessId}" : "";
                var processState = processInfo?.State?.StateDescription ?? "";

                var n = Environment.NewLine;
                PivotProcessInfo = $"{processName}{n}{processId}{n}{processState}".Trim();
            }

        }

        private void UpdateCurrentChatBubbles()
        {
            CurrentBubbleDataCollection.Clear();

            if (CurrentFlowKey != null)
            {
                var packets = _allFlows[CurrentFlowKey];
                if (packets == null) return;

                DateTime? lastTimestamp = null;

                foreach (var packet in packets)
                {
                    var endPointRole = packet.FlowKey.Endpoint1 == SelectedPivotEndpoint
                            ? FlowEndpointRole.Pivot
                            : FlowEndpointRole.Orbit;
                    var packetInterval = lastTimestamp.HasValue
                            ? packet.Timestamp - lastTimestamp.Value
                            : (TimeSpan?)null;

                    CurrentBubbleDataCollection.Add(new BubbleData(packet, endPointRole, packetInterval));

                    lastTimestamp = packet.Timestamp;
                }
            }
            UpdateBubbleItemsViewModels();
        }

        [RelayCommand]
        public void SelectFrameEndpoint(FlowEndpointRole endpointRole)
        {
            SelectedFrameEndpoint = endpointRole switch
            {
                FlowEndpointRole.Pivot => SelectedPivotEndpoint,
                FlowEndpointRole.Orbit => SelectedOrbitEndpoint,
                _ => null
            };
        }

        [RelayCommand]
        public void ToggleTcpDisplayMode()
        {
            var values = Enum.GetValues<TcpEncoding>().ToArray();
            var currentIndex = Array.IndexOf(values, SelectedTcpEncoding);
            var nextIndex = (currentIndex + 1) % values.Length;

            SelectedTcpEncoding = values[nextIndex];
        }

        [RelayCommand]
        public void ToggleEthernetDisplayMode()
        {
            var values = Enum.GetValues<FrameDisplay>().ToArray();
            var currentIndex = Array.IndexOf(values, SelectedFrameDisplay);
            var nextIndex = (currentIndex + 1) % values.Length;

            SelectedFrameDisplay = values[nextIndex];
        }



        [RelayCommand]
        private void LoadKeyLogFileWizard()
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Keylog Files|*.txt|All Files|*.*",
                DefaultExt = ".txt",
                Title = "Use keylogfile?"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                KeyLogFileName = openFileDialog.FileName;
            }
        }




        private async Task UpdateTSharkPacketsAsync()
        {
            var packets = CurrentBubbleDataCollection.Select(bd => bd.BasicPacket).ToList();
            var sharkPacket = await _tSharkService.ParseHttpPacketsAsync(packets);
        }
    }

    public class BubbleTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AppNullBubbleTemplate { get; set; }
        public DataTemplate AppHttpBubbleTemplate { get; set; }
        public DataTemplate AppHttpsBubbleTemplate { get; set; }
        public DataTemplate AppLengthPrefixBubbleTemplate { get; set; }
        public DataTemplate TcpHexBubbleTemplate { get; set; }
        public DataTemplate TcpAsciiBubbleTemplate { get; set; }
        public DataTemplate IpBubbleTemplate { get; set; }
        public DataTemplate EthernetBubbleTemplate { get; set; }
        public DataTemplate FrameBubbleTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                EthernetBubbleViewModel => EthernetBubbleTemplate,
                FrameBubbleViewModel => FrameBubbleTemplate,
                TcpAsciiBubbleViewModel => TcpAsciiBubbleTemplate,
                TcpHexBubbleViewModel => TcpHexBubbleTemplate,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
