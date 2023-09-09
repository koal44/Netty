using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Netty.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Netty.BasicPacket;

namespace Netty
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
        private ObservableCollection<IFlowChatBubbleViewModel> _currentFlowChatBubbles;

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
                UpdateChatBubbles();
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

        
        [ObservableProperty]
        private NetworkLayer _selectedNetworkLayer;
        [ObservableProperty]
        private TcpEncoding _selectedTcpEncoding;
        [ObservableProperty]
        private DeframeMethod? _selectedDeframeMethod;

        public static IEnumerable<DeframeMethod> DeframeMethods 
            => Enum.GetValues(typeof(DeframeMethod)).Cast<DeframeMethod>();

        [ObservableProperty]
        private int _messagePrefixLength;
        [ObservableProperty]
        private int _messageTypeLength;

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
            _currentFlowChatBubbles = new();
            _pivotProcessInfo = string.Empty;
            _isSeriousBotSelected = false;
            _isDitzyBotSelected = false;
            _selectedNetworkLayer = NetworkLayer.Tcp;
            _selectedTcpEncoding = TcpEncoding.Hex;

            // sets off a chain of side reactions which updates orbit, updating flowmessages.
            UpdatePivotEndpoints();
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

            UpdateChatBubbles();
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

        private void UpdateChatBubbles()
        {
            CurrentFlowChatBubbles.Clear();

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
                            : TimeSpan.Zero;

                    CurrentFlowChatBubbles.Add(new FlowChatBubbleViewModel(packet, endPointRole, this, packetInterval));

                    lastTimestamp = packet.Timestamp;
                }
            }
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
        public void ToggleDataDisplayMode()
        {
            var values = Enum.GetValues<TcpEncoding>().ToArray();
            var currentIndex = Array.IndexOf(values, SelectedTcpEncoding);
            var nextIndex = (currentIndex + 1) % values.Length;

            SelectedTcpEncoding = values[nextIndex];
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



        internal async Task ParseAsHttpAsync()
        {
            CurrentFlowChatBubbles.Clear();

            if (CurrentFlowKey != null)
            {
                var packets = _allFlows[CurrentFlowKey];
                if (packets == null) return;

                //DateTime? lastTimestamp = null;

                _ = await _tSharkService.ParseHttpPacketsAsync(packets);

            }
        }






    }


}
