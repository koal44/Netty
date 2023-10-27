using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Netryoshka.Services;
using Netryoshka.ViewModels;
using Netryoshka.ViewModels.ChatBubbles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Netryoshka
{
    public partial class FlowsPageViewModel : ObservableRecipient, IFlowsPageViewModel
    {
        private readonly FlowManager _flowManager;
        private readonly ILogger _logger;
        private readonly TSharkService _tSharkService;
        private readonly ICaptureService _captureService;
        private readonly Dictionary<FlowKey, List<BasicPacket>> _allFlows;

        [ObservableProperty]
        private ObservableCollection<InteractionEndpoint> _pivotEndpoints;
        [ObservableProperty]
        private ObservableCollection<InteractionEndpoint> _orbitEndpoints;
        [ObservableProperty]
        private List<BubbleData> _currentBubbleDataList;
        [ObservableProperty]
        private ObservableCollection<object> _currentItemViewModelCollecion;
        [ObservableProperty] 
        private InteractionEndpoint? _selectedPivotEndpoint;
        [ObservableProperty]
        private InteractionEndpoint? _selectedOrbitEndpoint;
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
        private DeframeMethod _selectedDeframeMethod;
        [ObservableProperty]
        private int _messagePrefixLength;
        [ObservableProperty]
        private int _messageTypeLength;
        [ObservableProperty]
        private string? _keyLogFileName;
        [ObservableProperty]
        private IEnumerable<DeframeMethod> _deframeMethods;
        private CancellationTokenSource? _ctsSharkService;
        [ObservableProperty]
        private bool _canContentScroll;
        [ObservableProperty]
        private Type? _viewModelItemType;
        [ObservableProperty]
        private InteractionComboBoxMode _pivotCBDisplayMode;
        [ObservableProperty]
        private InteractionComboBoxMode _orbitCBDisplayMode;


        public FlowsPageViewModel(FlowManager flowManager, ILogger logger, TSharkService tSharkService, ICaptureService captureService)
        {
            _flowManager = flowManager;
            _logger = logger;
            _tSharkService = tSharkService;
            _captureService = captureService;

            _allFlows = _flowManager.GetAllFlows();
            _pivotEndpoints = new();
            _orbitEndpoints = new();
            _currentBubbleDataList = new();
            _pivotProcessInfo = string.Empty;
            _selectedNetworkLayer = NetworkLayer.Tcp;
            _selectedFrameDisplay = FrameDisplay.NoShark;
            _selectedTcpEncoding = TcpEncoding.Hex;
            _selectedDeframeMethod = DeframeMethod.LengthPrefix;
            _currentItemViewModelCollecion = new();
            _deframeMethods = Enum.GetValues(typeof(DeframeMethod)).Cast<DeframeMethod>();
            _canContentScroll = true;
            _pivotCBDisplayMode = InteractionComboBoxMode.ProcessInfo;
            _orbitCBDisplayMode = InteractionComboBoxMode.DomainName;

            PropertyChanged += OnPropertyChanged;

            // sets off a chain of side reactions which updates orbit, updating flowmessages.
            UpdatePivotEndpoints();
        }


        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModelItemType):
                    break;
                case nameof(SelectedPivotEndpoint):
                    UpdateOrbitEndpoints();
                    break;
                case nameof(SelectedOrbitEndpoint):
                    UpdateCurrentFlow();
                    break;
                case nameof(SelectedNetworkLayer):
                case nameof(SelectedFrameDisplay):
                case nameof(SelectedTcpEncoding):
                case nameof(SelectedDeframeMethod):
                case nameof(CurrentBubbleDataList):
                    UpdateBubbleItemsViewModels();
                    break;
                //case nameof(MessagePrefixLength):
                //case nameof(MessageTypeLength):
            }
        }


        private void UpdateWireSharkData()
        {
            // If a previous task is still running, cancel it
            _ctsSharkService?.Cancel();

            var packets = CurrentBubbleDataList.Select(bd => bd.BasicPacket).ToList();

            _ = Task.Run(async () =>
            {
                try
                {
                    _ctsSharkService = new CancellationTokenSource();
                    var sharkDataList = await _tSharkService.ConvertToWireSharkDataAsync(packets, _ctsSharkService.Token);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (sharkDataList.Count != CurrentBubbleDataList.Count)
                        {
                            _logger.Error("Mismatch between sharkDataList and BubbleDataCollection sizes.");
                            return;
                        }

                        for (int i = 0; i < CurrentBubbleDataList.Count; i++)
                        {
                            if (sharkDataList[i] == null)
                                _logger.Error($"sharkDataList[{i}] was null");
                            CurrentBubbleDataList[i].WireSharkData = sharkDataList[i];
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
                    _logger.Error($"Error in UpdateFrameData: {ex.Message}", ex);
                }
            });
        }


        private void UpdateBubbleItemsViewModels()
        {
            string key = SelectedNetworkLayer switch
            {
                NetworkLayer.App => $"App{SelectedDeframeMethod}",
                NetworkLayer.Frame => $"Frame{SelectedFrameDisplay}",
                NetworkLayer.Tcp => $"Tcp{SelectedTcpEncoding}",
                _ => $"{SelectedNetworkLayer}"
            };

            //BubbleViewModelsByName.TryGetValue(key, out var viewModelType);
            BubbleViewModelBase.RegisteredTypes.TryGetValue(key, out var viewModelType);
            if (viewModelType is null)
                throw new InvalidOperationException($"Could not select a suitable view model for key '{key}'");

            ViewModelItemType = viewModelType;

            var canScrollAttr = viewModelType.GetCustomAttribute<CanContentScrollAttribute>(false)
                ?? throw new InvalidOperationException($"Could not get CanContentScrollAttribute for type '{viewModelType}'");
            CanContentScroll = canScrollAttr.CanScroll;

            bool isWireSharkDependent = viewModelType.GetCustomAttributes(typeof(RequiresWireSharkAttribute), true).Any();

            // if it's a WireSharkViewModel and the first bubble doesn't have WireSharkData, then update it
            // UpdateWireSharkData() will call UpdateBubbleItemsViewModels() when it's done
            if (isWireSharkDependent
                && CurrentBubbleDataList.Count > 0 
                && CurrentBubbleDataList.First().WireSharkData == null)
            {
                UpdateWireSharkData();
                return;
            }

            var newItemViewModelCollecion = new ObservableCollection<object>();
            foreach (var bubbleData in CurrentBubbleDataList)
            {
                var viewModel = Activator.CreateInstance(viewModelType, bubbleData)
                    ?? throw new InvalidOperationException($"Could not create a view model of type '{viewModelType}'");
                newItemViewModelCollecion.Add(viewModel);
            }
            CurrentItemViewModelCollecion = newItemViewModelCollecion;
        }


        private void UpdatePivotEndpoints()
        {
            PivotEndpoints.Clear();

            var uniquePivots = new HashSet<InteractionEndpoint>();
            foreach (var flowKey in _allFlows.Keys)
            {
                if (flowKey == null) continue;

                var firstPacket = _allFlows[flowKey].FirstOrDefault();
                if (firstPacket == null)
                {
                    _logger.Error($"firstpacket was null for flow key: {flowKey}");
                    continue;
                }

                switch (firstPacket.Direction)
                {
                    case BasicPacket.BPDirection.Outgoing:
                        uniquePivots.Add(new InteractionEndpoint(flowKey, firstPacket.SrcEndpoint));
                        break;
                    case BasicPacket.BPDirection.Incoming:
                        uniquePivots.Add(new InteractionEndpoint(flowKey, firstPacket.DstEndpoint));
                        break;
                    default:
                        uniquePivots.Add(new InteractionEndpoint(flowKey, firstPacket.SrcEndpoint));
                        uniquePivots.Add(new InteractionEndpoint(flowKey, firstPacket.DstEndpoint));
                        break;
                }
            }

            foreach (var pivot in uniquePivots)
            {
                var packets = _allFlows[pivot.FlowKey];
                pivot.ProcessInfo = packets == null || packets.Count <= 0
                    ? null
                    : packets[0].ProcessInfo;
                pivot.DomainName = _captureService.GetDomainName(pivot.FlowEndpoint.IpAddress);

                PivotEndpoints.Add(pivot);
            }

            SelectedPivotEndpoint = PivotEndpoints.FirstOrDefault();
            SelectedBotRole ??= PivotEndpoints.Any() ? FlowEndpointRole.Pivot : null;
        }


        private void UpdateOrbitEndpoints()
        {
            SelectedOrbitEndpoint = null;
            OrbitEndpoints.Clear();

            if (SelectedPivotEndpoint != null)
            {
                foreach (var flowKey in _allFlows.Keys)
                {
                    var orbitEndpoint = SelectedPivotEndpoint.FlowEndpoint switch
                    {
                        var pivot when pivot.Equals(flowKey.Endpoint1) => flowKey.Endpoint2,
                        var pivot when pivot.Equals(flowKey.Endpoint2) => flowKey.Endpoint1,
                        _ => null
                    };

                    if (orbitEndpoint != null)
                    {
                        var orbit = new InteractionEndpoint(flowKey, orbitEndpoint)
                        {
                            DomainName = _captureService.GetDomainName(orbitEndpoint.IpAddress)
                        };
                        OrbitEndpoints.Add(orbit);
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
        }


        private void UpdateCurrentFlow()
        {
            _currentFlowKey = null;

            if (SelectedPivotEndpoint != null && SelectedOrbitEndpoint != null)
            {
                _currentFlowKey = new FlowKey(SelectedPivotEndpoint.FlowEndpoint, SelectedOrbitEndpoint.FlowEndpoint);
            }

            UpdatePivotProcessInfo();
            UpdateCurrentChatBubbles();
        }


        private void UpdatePivotProcessInfo()
        {
            var processInfo = SelectedPivotEndpoint?.ProcessInfo;
            var processName = processInfo?.ProcessName ?? "";
            var processId = processInfo?.ProcessId != null ? $"pid {processInfo.ProcessId}" : "";
            var processState = processInfo?.State?.StateDescription ?? "";
            var n = Environment.NewLine;

            PivotProcessInfo = $"{processName}{n}{processId}{n}{processState}".Trim();
        }


        private void UpdateCurrentChatBubbles()
        {
            if (_currentFlowKey != null)
            {
                var packets = _allFlows[_currentFlowKey];
                if (packets == null) return;

                DateTime? lastTimestamp = null;

                var newBubbleDataList = new List<BubbleData>();
                for (int index = 1; index < packets.Count; index++)
                {
                    var packet = packets[index];
                    var endPointRole = packet.FlowKey.Endpoint1 == SelectedPivotEndpoint?.FlowEndpoint
                            ? FlowEndpointRole.Pivot
                            : FlowEndpointRole.Orbit;
                    var packetInterval = lastTimestamp.HasValue
                            ? packet.Timestamp - lastTimestamp.Value
                            : (TimeSpan?)null;

                    newBubbleDataList.Add(new BubbleData(packet, endPointRole, packetInterval, index));

                    lastTimestamp = packet.Timestamp;
                }
                CurrentBubbleDataList = newBubbleDataList;
            }
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


    public enum InteractionComboBoxMode
    {
        ProcessInfo,
        DomainName,
        None
    }


    public class BubbleTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AppLengthPrefixBubbleTemplate { get; set; } = null!;
        public DataTemplate AppHttpBubbleTemplate { get; set; } = null!;
        public DataTemplate AppHttpsBubbleTemplate { get; set; } = null!;
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
                FrameNoSharkBubbleViewModel => FrameNoSharkBubbleTemplate,
                FrameSharkJsonBubbleViewModel => FrameSharkJsonBubbleTemplate,
                FrameSharkTextBubbleViewModel => FrameSharkTextBubbleTemplate,
                EthernetBubbleViewModel => EthernetBubbleTemplate,
                IpBubbleViewModel => IpBubbleTemplate,
                TcpAsciiBubbleViewModel => TcpAsciiBubbleTemplate,
                TcpHexBubbleViewModel => TcpHexBubbleTemplate,
                AppLengthPrefixBubbleViewModel => AppLengthPrefixBubbleTemplate,
                AppHttpBubbleViewModel => AppHttpBubbleTemplate,
                AppHttpsBubbleViewModel => AppHttpsBubbleTemplate,
                _ => base.SelectTemplate(item, container),
            };
            return template;
        }
    }
}
