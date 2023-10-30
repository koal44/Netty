using Kaitai;
using Netryoshka.Models;
using PacketDotNet;
using SharpPcap;
using SharpPcap.WinDivert;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static Netryoshka.BasicPacket;

namespace Netryoshka.Services
{
    public class CaptureWindowsService : ICaptureService
    {
        private ILiveDevice? _device;
        private PacketArrivalEventHandler? _arrivalEventHandler;
        private CaptureStoppedEventHandler? _captureStoppedEventHandler;
        private readonly ILogger _logger;
        private readonly ISocketProcessMapperService _socketProcessMapper;
        private readonly CircularBuffer<BasicPacket> _capturedPackets; // stores all captured packets
        private BlockingCollection<RawCapture> _packetQueue; // where we process incoming packets
        private readonly int MaxPacketQueueCount = 10_000;
        private readonly List<string> _privilegedDeviceNames;
        private FilterData? _filterData;
        private bool _isDynamicFiltering;
        private PhysicalAddress _macAddress;
        private readonly FlowManager _flowManager;
        private bool _canUpdateWithProcessInfo;

        public Dictionary<string, string> VersionDict { get; private set; }
        public Dictionary<IPAddress, string> DnsCache { get; private set; }

        public CaptureWindowsService(ILogger logger, ISocketProcessMapperService socketProcessMapper, FlowManager packetCollectionService)
        {
            _logger = logger;
            _socketProcessMapper = socketProcessMapper;
            _flowManager = packetCollectionService;
            _packetQueue = new(new ConcurrentQueue<RawCapture>(), MaxPacketQueueCount);
            _capturedPackets = new(MaxPacketQueueCount);
            _isDynamicFiltering = false;
            _macAddress = PhysicalAddress.None;
            _canUpdateWithProcessInfo = false;

            // WinDivert is currently disabled. See the InitializeWinDivertAsync method for more details.
            _privilegedDeviceNames = new List<string>
            {
                // "WinDivert" // Disabled for now
            };
            VersionDict = new()
            {
                ["Pcap.SharpPcapVersion"] = Pcap.SharpPcapVersion.ToString(),
                ["Pcap.Version"] = Pcap.Version,
                ["Pcap.LibpcapVersion"] = Pcap.LibpcapVersion.ToString(),
            };
            DnsCache = new();
        }

        public List<string> GetDeviceNames()
        {
            var devices = CaptureDeviceList.Instance;
            var deviceNames = devices.Select(x =>
            {
                //if (x is LibPcapLiveDevice libPcapDevice 
                //    && !string.IsNullOrEmpty(libPcapDevice.Interface.FriendlyName))
                //{
                //    return libPcapDevice.Interface.FriendlyName;
                //}
                return x.Description;
            }).ToList();

            deviceNames.AddRange(_privilegedDeviceNames);

            return deviceNames;
        }

        public string? GetSelectedDeviceName()
        {
            return _device?.Description;
        }


        private static bool IsDynamicFiltering(FilterData filterData)
        {
            return (filterData.LocalProcessNames != null && filterData.LocalProcessNames.Any())
                || filterData.LocalPIDs != null && filterData.LocalPIDs.Any();
        }


        private async Task<ILiveDevice?> ConfigureStandardDevice(string deviceName, bool isPromiscuous, bool isDynamicFiltering)
        {
            var device = CaptureDeviceList.Instance.FirstOrDefault(x => x.Description.Contains(deviceName));

            if (isPromiscuous)
            {
                device?.Open(DeviceModes.Promiscuous, 1000);
            }
            else
            {
                device?.Open();
            }

            if (!isDynamicFiltering && device != null)
            {
                string filter = await BuildStaticFilterForStandardDevice(_filterData!.Value);
                device.Filter = filter;
            }

            _canUpdateWithProcessInfo = true;

            return device;
        }

        private async Task<ILiveDevice> InitializeDeviceAsync(string deviceName, FilterData filterdata, bool isPromiscuous)
        {
            _filterData = filterdata;
            _isDynamicFiltering = IsDynamicFiltering(filterdata);

            _device = deviceName == "WinDivert"
                ? await InitializeWinDivertAsync(filterdata, isPromiscuous)
                : await ConfigureStandardDevice(deviceName, isPromiscuous, _isDynamicFiltering);

            if (_device == null)
            {
                string errorMsg = $"No device matching the description: {deviceName}";
                _logger.Error(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            _captureStoppedEventHandler = new CaptureStoppedEventHandler(
                (object sender, CaptureStoppedEventStatus status) =>
                {
                    if (status != CaptureStoppedEventStatus.CompletedWithoutError)
                    {
                        _logger.Error("Error stopping capture");
                    }
                }
            );

            _device.OnCaptureStopped += _captureStoppedEventHandler;

            _macAddress = _device.MacAddress;

            return _device;
        }


        // WinDivert support has been temporarily disabled because it's not fully operational, 
        // requires admin privileges, and the filter can't be changed without stopping the capture.
        // It might be re-enabled in the future if there's a need for an alternative method 
        // for capturing PID at the socket layer or injecting/modifying packets.
#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable IDE0060 // Remove unused parameter
        private async Task<ILiveDevice> InitializeWinDivertAsync(FilterData filterdata, bool isPromiscuous)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            throw new NotSupportedException("WinDivert support is currently not available. Please use an alternative method.");

            var version = "2.2.2";
            var arch = IntPtr.Size == 8 ? "x64" : "x86";
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var driverPath = Path.Combine(baseDir, "lib", $"WinDivert-{version}-A\\", arch);
            var dllPath = Path.Combine(driverPath, "WinDivert.dll");

            // Check if the driver is there
            if (!File.Exists(dllPath))
            {
                _logger.Error($"Couldn't find '{dllPath}'");
                throw new Exception($"Couldn't find '{dllPath}'");
            }

            // Patch PATH env
            var oldPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string newPath = oldPath + Path.PathSeparator + driverPath;
            Environment.SetEnvironmentVariable("PATH", newPath);

            _device = new WinDivertDevice();
            string filter = await BuildWinDivertFilterAsync(filterdata);
            return _device;
        }
#pragma warning restore CS0162 // Unreachable code detected

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
        private async Task<string> BuildWinDivertFilterAsync(FilterData filterData)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1822 // Mark members as static
        {
            //return "";
            return await Task.FromResult("");
        }


        

        /// <summary>
        /// Builds a static filter for a standard device.
        /// </summary>
        /// <remarks>
        /// Note: This method may result in over-capturing, as it treats local and remote ports the same way.
        /// </remarks>
        /// <param name="filterData">The filter data.</param>
        /// <returns>The filter string.</returns>
        private static Task<string> BuildStaticFilterForStandardDevice(FilterData filterData)
        {
            // Error if process names or PIDs are present, as they indicate dynamic filtering
            if (filterData.LocalProcessNames.Any() || filterData.LocalPIDs.Any())
            {
                throw new InvalidOperationException("Process names and PIDs are not allowed when building a static filter for a standard device.");
            }

            if (filterData.CustomFilter != string.Empty)
            {
                return Task.FromResult(filterData.CustomFilter);
            }

            // Combine both remote and local ports
            var ports = filterData.RemotePorts.Concat(filterData.LocalPorts).ToList();

            // Ensure there are unique ports
            ports = ports.Distinct().ToList();

            // Start building the filter
            StringBuilder filterBuilder = new();

            if (ports.Any())
            {
                filterBuilder.Append('(');
                filterBuilder.Append(string.Join(" or ", ports.Select(port => $"tcp port {port}")));
                filterBuilder.Append(')');
            }

            // Add IP addresses to the filter
            if (filterData.RemoteIpAddresses.Any())
            {
                if (filterBuilder.Length > 0)
                    filterBuilder.Append(" and ");

                filterBuilder.Append('(');
                filterBuilder.Append(string.Join(" or ", filterData.RemoteIpAddresses.Select(ip => $"host {ip}")));
                filterBuilder.Append(')');
            }

            if (filterData.LogDNSLookups)
            {
                if (filterBuilder.Length > 0)
                    filterBuilder.Append(" or ");

                filterBuilder.Append("(udp port 53 or tcp port 53)");
            }

            return Task.FromResult(filterBuilder.ToString());
        }



        // producer-consumer pattern
        public async Task StartCaptureAsync(Action<BasicPacket> packetReceivedCallback, FilterData filterdata, string deviceName, bool isPromiscuous = false)
        {
            _flowManager.Clear();
            _capturedPackets.Clear();
            _packetQueue = new(new ConcurrentQueue<RawCapture>(), 10_000); // Max 10,000 packets in queue

            _device = await InitializeDeviceAsync(deviceName, filterdata, isPromiscuous);

            _arrivalEventHandler = (sender, e) =>
            {
                // Adding packet to the queue which should be processed in another thread
                _packetQueue.Add(e.GetPacket());
            };

            _device.OnPacketArrival += _arrivalEventHandler;
            
            _device.StartCapture();
            _ = Task.Run(() => ProcessPackets(packetReceivedCallback)); // Start the consumer task
        }


        private void ProcessPackets(Action<BasicPacket> packetReceivedCallback)
        {
            Dictionary<ushort, TcpProcessRecord> tcpConnectionsByLocalPortDict = new();

            // !! UDP connections are not supported in this implementation:

            try
            {
                foreach (var rawCap in _packetQueue.GetConsumingEnumerable())
                {
                    var basicPacket = RawCaptureToBasicPacket(rawCap);
                    if (basicPacket == null) { continue; }

                    basicPacket.Direction = _macAddress switch
                    {
                        var mac when mac.Equals(basicPacket.SrcEndpoint.MacAddress) 
                            => BPDirection.Outgoing,
                        var mac when mac.Equals(basicPacket.DstEndpoint.MacAddress) 
                            => BPDirection.Incoming,
                        _ 
                            => BPDirection.Unknown
                    };

                    // handle DNS lookups
                    if (basicPacket.Protocol == BPProtocol.UDP
                        && (basicPacket.SrcEndpoint.Port == 53 || basicPacket.DstEndpoint.Port == 53))
                    {
                        HandleDnsLookups(basicPacket);
                        continue; // TODO: And if the user wants to capture DNS lookups?
                    }

                    //basicPacket.BPDirection = _macAddress.Equals(basicPacket.MacEndpointAddresses?.SourceMacAddress)
                    //    ? BPDirection.Outgoing : _macAddress.Equals(basicPacket.MacEndpointAddresses?.DestinationMacAddress)
                    //    ? BPDirection.Incoming
                    //    : BPDirection.Other;

                    if (_canUpdateWithProcessInfo || _isDynamicFiltering)
                    {
                        var processRecord = GetTcpProcessRecord(basicPacket, tcpConnectionsByLocalPortDict);
                        if (processRecord == null)
                        {
                            // Mapping of local ports to process names, which takes the 1st process found.
                            // NOTE: It's possible that multiple processes share the same port. Such cases should be rare but the current design could lead to inaccurate process info.
                            tcpConnectionsByLocalPortDict = _socketProcessMapper
                                                                .GetAllTcpConnections()
                                                                .GroupBy(x => x.LocalPort)
                                                                .ToDictionary(g => g.Key, g => g.First());
                            processRecord = GetTcpProcessRecord(basicPacket, tcpConnectionsByLocalPortDict);
                        }

                        if (processRecord != null)
                        {
                            // Cross-reference successful; update basicPacket with pid and process name
                            basicPacket.ProcessInfo = processRecord;
                        }
                    }

                    if (_isDynamicFiltering)
                    {
                        if (!_filterData!.Value.ShouldKeepPacket(basicPacket))
                        {
                            continue;
                        }
                    }

                    _flowManager.AddPacket(basicPacket);
                    _capturedPackets.Add(basicPacket);
                    packetReceivedCallback(basicPacket);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info("Capture operation was canceled. This is expected during shutdown.");
            }
            catch (Exception e)
            {
                _logger.Error($"Unexpected exception in ProcessPackets.\n {e.Message}", e);
            }

        }

        private void HandleDnsLookups(BasicPacket basicPacket)
        {
            var dnsPacket = new DnsPacket(new KaitaiStream(basicPacket.Payload));
            if (dnsPacket.Answers.Count == 0) return;
            if (dnsPacket.Queries.Count == 0) throw new Exception("DNS packet has no queries.");

            var queryName = string.Join(".", dnsPacket.Queries[0].Name.Name.Select(label => label.Name));
            if (string.IsNullOrEmpty(queryName)) return;

            List<IPAddress> ipAddresses = new();
            foreach (var answer in dnsPacket.Answers)
            {
                if (answer is null) { continue; }

                switch (answer.Payload)
                {
                    case DnsPacket.DomainName payload:
                        // var domainName = string.Join(".", payload.Name.Select(label => label.Name));
                        break;
                    case DnsPacket.Address payload:
                        ipAddresses.Add(new IPAddress(payload.Ip));
                        break;
                    default:
                        break;
                }
            }

            if (ipAddresses.Count == 0) return;

            foreach (var ipAddress in ipAddresses)
            {
                DnsCache[ipAddress] = queryName;
            }
        }

        private static TcpProcessRecord? GetTcpProcessRecord(BasicPacket basicPacket, Dictionary<ushort, TcpProcessRecord> tcpConnectionsByLocalPortDict)
        {
            ushort? localPort = null;

            if (basicPacket.Direction == BPDirection.Unknown)
            {
                bool sourcePortFound = tcpConnectionsByLocalPortDict.ContainsKey(basicPacket.SrcEndpoint.Port);
                bool destPortFound = tcpConnectionsByLocalPortDict.ContainsKey(basicPacket.DstEndpoint.Port);

                if (sourcePortFound && destPortFound)
                {
                    return null; // Both ports found, so we can't know for sure which one is local
                }

                if (sourcePortFound)
                {
                    localPort = basicPacket.SrcEndpoint.Port;
                }
                else if (destPortFound)
                {
                    localPort = basicPacket.DstEndpoint.Port;
                }

                if (!localPort.HasValue)
                {
                    return null; // Neither port found, so return null
                }
            }
            else
            {
                localPort = basicPacket.Direction == BPDirection.Outgoing
                    ? basicPacket.SrcEndpoint.Port
                    : basicPacket.DstEndpoint.Port;
            }

            tcpConnectionsByLocalPortDict.TryGetValue(localPort.Value, out TcpProcessRecord? processRecord);
            return processRecord;
        }

        public void StopCapture()
        {
            if (_device != null) _device.OnPacketArrival -= _arrivalEventHandler;
            _packetQueue.CompleteAdding(); // No more items will be added

            if (_device == null) { return; }
            _device.StopCapture();
            _device.Close();
            _device.OnPacketArrival -= _arrivalEventHandler;
            _device.OnCaptureStopped -= _captureStoppedEventHandler;
            _device = null;
        }


        public IEnumerable<BasicPacket> GetPackets() => _capturedPackets.GetAll();

        public bool HasData => _capturedPackets.Any();

        public int CapturedPacketCount => _capturedPackets.Count;

        public void WritePacketsToFile(string fileName)
        {
            WritePacketsToFile(fileName, _capturedPackets.GetAll());
        }

        public void WritePacketsToFile(string fileName, IEnumerable<BasicPacket> packets)
        {
            using SharpPcap.LibPcap.CaptureFileWriterDevice pcapFileWriter = new(fileName, System.IO.FileMode.Create);
            pcapFileWriter.Open(LinkLayers.Ethernet);
            foreach (var packet in packets)
            {
                var rawCap = BasicPacketToRawCapture(packet);
                pcapFileWriter.Write(rawCap);
            }
            pcapFileWriter.Close();
        }


        public void LoadPacketsFromFile(Action<BasicPacket> packetReceivedCallback, string fileName)
        {
            _isDynamicFiltering = false;
            _canUpdateWithProcessInfo = false;

            _flowManager.Clear();
            _capturedPackets.Clear();
            _packetQueue = new(new ConcurrentQueue<RawCapture>(), MaxPacketQueueCount);

            var device = new SharpPcap.LibPcap.CaptureFileReaderDevice(fileName);
            device.Open();

            //_device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
            _arrivalEventHandler = (sender, e) =>
            {
                _packetQueue.Add(e.GetPacket());
            };
            device.OnPacketArrival += _arrivalEventHandler;

            _captureStoppedEventHandler = (sender, e) =>
            {
                device.OnPacketArrival -= _arrivalEventHandler;
                device.OnCaptureStopped -= _captureStoppedEventHandler;
            };
            device.OnCaptureStopped += _captureStoppedEventHandler;

            // Start capturing an 'INFINTE' number of packets
            // This method will return when EOF reached.
            device.Capture(); // should start its own thread

            // Close the pcap _device
            device.Close();

            Task.Run(() => ProcessPackets(packetReceivedCallback));
        }


        private static BasicPacket? RawCaptureToBasicPacket(RawCapture rawCapture)
        {
            // Parse the raw basicPacket data
            var packet = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

            if (packet is EthernetPacket ethPacket)
            {
                var sourceMacAddress = ethPacket.SourceHardwareAddress;
                var destinationMacAddress = ethPacket.DestinationHardwareAddress;

                // Check for IP Packet
                if (ethPacket.PayloadPacket is IPPacket ipPacket)
                {
                    IPAddress sourceAddress = ipPacket.SourceAddress;
                    IPAddress destinationAddress = ipPacket.DestinationAddress;

                    // var tcpPacket = parsedPacket.Extract<TcpPacket>();

                    // Check for TCP basicPacket
                    if (ipPacket.PayloadPacket is TcpPacket tcpPacket)
                    {
                        var tcpHeaders = new TcpHeaders(
                            tcpPacket.SequenceNumber,
                            tcpPacket.AcknowledgmentNumber,
                            tcpPacket.Acknowledgment,
                            tcpPacket.Synchronize,
                            tcpPacket.Finished,
                            tcpPacket.Reset,
                            tcpPacket.Push,
                            tcpPacket.Urgent,
                            tcpPacket.WindowSize,
                            tcpPacket.DataOffset,
                            tcpPacket.TotalPacketLength,
                            tcpPacket.Checksum
                        );

                        return new BasicPacket(
                            sourceAddress, tcpPacket.SourcePort,
                            destinationAddress, tcpPacket.DestinationPort,
                            tcpPacket.PayloadData, rawCapture.Timeval.Date,
                            BPProtocol.TCP, tcpHeaders, sourceMacAddress, destinationMacAddress
                        );
                    }
                    else if (ipPacket.PayloadPacket is UdpPacket udpPacket)
                    {
                        return new BasicPacket(sourceAddress, udpPacket.SourcePort,
                            destinationAddress, udpPacket.DestinationPort,
                            udpPacket.PayloadData, rawCapture.Timeval.Date,
                            BPProtocol.UDP, null, sourceMacAddress, destinationMacAddress
                        );
                    }
                    // Check for other protocols if necessary...
                }
            }

            // Handle other types of packets or throw exception as needed
            //throw new NotSupportedException("Packet type not supported");
            return null;
        }


        private static RawCapture BasicPacketToRawCapture(BasicPacket basicPacket)
        {
            if (basicPacket.Protocol != BPProtocol.TCP)
                throw new NotSupportedException("Only TCP packets are supported");

            // Use provided MAC addresses or spoof them
            var sourceMacAddress = basicPacket.SrcEndpoint.MacAddress;
            var destinationMacAddress = basicPacket.DstEndpoint.MacAddress;

            // Construct the TCP basicPacket
            var tcpPacket = new TcpPacket(basicPacket.SrcEndpoint.Port, basicPacket.DstEndpoint.Port);
            if (basicPacket.TcpHeadersData.HasValue)
            {
                var headers = basicPacket.TcpHeadersData.Value;
                tcpPacket.SequenceNumber = headers.SequenceNumber;
                tcpPacket.AcknowledgmentNumber = headers.AcknowledgmentNumber;
                tcpPacket.Acknowledgment = headers.IsAcknowledgment;
                tcpPacket.Synchronize = headers.IsSynchronize;
                tcpPacket.Finished = headers.IsFinal;
                tcpPacket.Reset = headers.IsReset;
                tcpPacket.Push = headers.IsPush;
                tcpPacket.Urgent = headers.IsUrgent;
                tcpPacket.WindowSize = headers.WindowSize;
                tcpPacket.Checksum = headers.Checksum;
                // other TCP fields?
            }
            tcpPacket.PayloadData = basicPacket.Payload;

            // Construct the IP basicPacket
            var ipPacket = new IPv4Packet(basicPacket.SrcEndpoint.IpAddress, basicPacket.DstEndpoint.IpAddress)
            {
                PayloadPacket = tcpPacket
            };

            // Construct the Ethernet basicPacket
            var ethPacket = new EthernetPacket(sourceMacAddress, destinationMacAddress, EthernetType.IPv4)
            {
                PayloadPacket = ipPacket
            };

            // Convert to RawCapture
            var linkLayerType = LinkLayers.Ethernet;
            var rawCapture = new RawCapture(linkLayerType, new PosixTimeval(basicPacket.Timestamp), ethPacket.Bytes);

            return rawCapture;
        }


        public bool RequiresAdminPrivileges(string deviceName)
        {
            return _privilegedDeviceNames.Contains(deviceName);
        }


        public bool IsValidFilter(string filter, string deviceName)
        {
            // if (string.IsNullOrEmpty(filter)) return true;
            if (deviceName == "WinDivert") return false; // Abandoning support for WinDivert

            var device = CaptureDeviceList.Instance.FirstOrDefault(d => d.Description.Contains(deviceName))
                ?? throw new ArgumentException($"Device with name {deviceName} not found.");

            try
            {
                device.Open();
                device.Filter = filter; // Will throw an exception if filter is invalid
                return true;
            }
            catch (PcapException)
            {
                return false;
            }
            finally
            {
                device.Close();
            }
        }

        public string? GetDomainName(IPAddress ipAddress)
        {
            return DnsCache.TryGetValue(ipAddress, out string? domainName)
                ? domainName
                : null;
        }
    }

}
