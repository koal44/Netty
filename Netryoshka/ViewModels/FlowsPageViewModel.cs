﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Netryoshka.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Netryoshka.BasicPacket;

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
        [ObservableProperty]
        private ObservableCollection<BubbleData> _currentBubbleDataCollection;
        [ObservableProperty]
        private ObservableCollection<object> _currentItemViewModelCollecion;
        [ObservableProperty] 
        private FlowEndpoint? _selectedPivotEndpoint;
        [ObservableProperty] 
        private FlowEndpoint? _selectedOrbitEndpoint;
        private FlowKey? _currentFlowKey;
        [ObservableProperty]
        private string _pivotProcessInfo;
        [ObservableProperty]
        private FlowEndpointRole? _selectedBotRole;
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
        [ObservableProperty]
        private string? _keyLogFileName;
        [ObservableProperty]
        private IEnumerable<DeframeMethod> _deframeMethods;
        private CancellationTokenSource? _ctsSharkService;

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
            { "FrameNoShark", typeof(FrameNoSharkBubbleViewModel) },
            { "FrameSharkJson", typeof(FrameSharkJsonBubbleViewModel) },
            { "FrameSharkText", typeof(FrameSharkTextBubbleViewModel) },
        };

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
            _selectedNetworkLayer = NetworkLayer.Tcp;
            _selectedFrameDisplay = FrameDisplay.NoShark;
            _selectedTcpEncoding = TcpEncoding.Hex;
            _selectedDeframeMethod = null;
            _currentItemViewModelCollecion = new();
            _deframeMethods = Enum.GetValues(typeof(DeframeMethod)).Cast<DeframeMethod>();
            PropertyChanged += OnPropertyChanged;
            //CurrentBubbleDataCollection.CollectionChanged += OnCurrentBubbleDataCollectionChanged;

            // sets off a chain of side reactions which updates orbit, updating flowmessages.
            UpdatePivotEndpoints();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SelectedFrameDisplay):
                    UpdateWireSharkData();
                    break;
                case nameof(CurrentBubbleDataCollection):
                    UpdateBubbleItemsViewModels();
                    SelectedFrameDisplay = FrameDisplay.NoShark;
                    break;
                case nameof(SelectedOrbitEndpoint):
                    UpdateCurrentChatBubbles();
                    break;
                case nameof(SelectedPivotEndpoint):
                    UpdateOrbitEndpoints();
                    break;
                // these properties control the display of the chat bubbles
                case nameof(SelectedNetworkLayer):
                case nameof(SelectedTcpEncoding):
                case nameof(SelectedDeframeMethod):
                case nameof(MessagePrefixLength):
                case nameof(MessageTypeLength):
                    UpdateBubbleItemsViewModels();
                    break;
            }
        }

        private void UpdateWireSharkData()
        {
            // If a previous task is still running, cancel it
            _ctsSharkService?.Cancel();

            if (SelectedFrameDisplay == FrameDisplay.NoShark)
            {
                foreach (var bubbleData in CurrentBubbleDataCollection)
                {
                    bubbleData.WireSharkData = null;
                }
                UpdateBubbleItemsViewModels();
                return;
            }

            // Check the first bubble for WireSharkData, if exists then return early
            if (CurrentBubbleDataCollection.Count <= 0
                || CurrentBubbleDataCollection.First().WireSharkData != null)
            {
                UpdateBubbleItemsViewModels();
                return;
            }

            var packets = CurrentBubbleDataCollection.Select(bd => bd.BasicPacket).ToList();

            _ = Task.Run(async () =>
            {
                try
                {
                    _ctsSharkService = new CancellationTokenSource();
                    var sharkDataList = await _tSharkService.ConvertToWireSharkDataAsync(packets, _ctsSharkService.Token);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (sharkDataList.Count != CurrentBubbleDataCollection.Count)
                        {
                            _logger.Error("Mismatch between sharkDataList and BubbleDataCollection sizes.");
                            return;
                        }

                        for (int i = 0; i < CurrentBubbleDataCollection.Count; i++)
                        {
                            CurrentBubbleDataCollection[i].WireSharkData = sharkDataList[i];
                        }

                        UpdateBubbleItemsViewModels();
                    });
                }
                catch (OperationCanceledException)
                {
                    _logger.Info("Task was cancelled.");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in UpdateWireSharkData: {ex.Message}", ex);
                }
            });
        }

        private void UpdateBubbleItemsViewModels()
        {
            string key = SelectedNetworkLayer switch
            {
                NetworkLayer.App => $"App{(SelectedDeframeMethod.HasValue ? SelectedDeframeMethod : "Null")}",
                NetworkLayer.Frame => $"Frame{SelectedFrameDisplay}",
                NetworkLayer.Tcp => $"Tcp{SelectedTcpEncoding}",
                _ => $"{SelectedNetworkLayer}"
            };

            BubbleViewModelsByName.TryGetValue(key, out var viewModelType);
            if (viewModelType is null)
                throw new InvalidOperationException($"Could not select a suitable view model for key '{key}'");

            var newItemViewModelCollecion = new ObservableCollection<object>();
            foreach (var bubbleData in CurrentBubbleDataCollection)
            {
                var viewModel = Activator.CreateInstance(viewModelType, bubbleData)
                    ?? throw new InvalidOperationException($"Could not create a view model of type '{viewModelType}'");
                newItemViewModelCollecion.Add(viewModel);
            }
            CurrentItemViewModelCollecion = newItemViewModelCollecion;
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
            SelectedBotRole ??= PivotEndpoints.Any() ? FlowEndpointRole.Pivot : null;
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
            _currentFlowKey = null;

            if (SelectedPivotEndpoint != null && SelectedOrbitEndpoint != null)
            {
                _currentFlowKey = new FlowKey(SelectedPivotEndpoint, SelectedOrbitEndpoint);
            }

            UpdateCurrentChatBubbles();
            UpdatePivotProcessInfo();
        }

        private void UpdatePivotProcessInfo()
        {
            PivotProcessInfo = string.Empty;

            if (_currentFlowKey != null)
            {
                var packets = _allFlows[_currentFlowKey];
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

            if (_currentFlowKey != null)
            {
                var packets = _allFlows[_currentFlowKey];
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
        public void SelectBotRole(FlowEndpointRole endpointRole)
        {
            SelectedBotRole = endpointRole;
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
        public void ToggleFrameDisplay()
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

    }

    public class BubbleTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AppNullBubbleTemplate { get; set; } = null!;
        public DataTemplate AppHttpBubbleTemplate { get; set; } = null!;
        public DataTemplate AppHttpsBubbleTemplate { get; set; } = null!;
        public DataTemplate AppLengthPrefixBubbleTemplate { get; set; } = null!;
        public DataTemplate TcpHexBubbleTemplate { get; set; } = null!;
        public DataTemplate TcpAsciiBubbleTemplate { get; set; } = null!;
        public DataTemplate IpBubbleTemplate { get; set; } = null!;
        public DataTemplate EthernetBubbleTemplate { get; set; } = null!;
        public DataTemplate FrameNoSharkBubbleTemplate { get; set; } = null!;
        public DataTemplate FrameSharkJsonBubbleTemplate { get; set; } = null!;
        public DataTemplate FrameSharkTextBubbleTemplate { get; set; } = null!;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var template = item switch
            {
                EthernetBubbleViewModel => EthernetBubbleTemplate,
                FrameNoSharkBubbleViewModel => FrameNoSharkBubbleTemplate,
                FrameSharkJsonBubbleViewModel => FrameSharkJsonBubbleTemplate,
                FrameSharkTextBubbleViewModel => FrameSharkTextBubbleTemplate,
                TcpAsciiBubbleViewModel => TcpAsciiBubbleTemplate,
                TcpHexBubbleViewModel => TcpHexBubbleTemplate,
                _ => base.SelectTemplate(item, container),
            };
            return template;
        }
    }
}
