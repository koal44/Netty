using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netryoshka.Hack;
using Netryoshka.Models;
using Netryoshka.Services;
using Netryoshka.Views;
using Netryoshka.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Netryoshka.ViewModels
{
    public partial class CapturePageViewModel : ObservableObject
	{
        public bool AccessKeyHack { get; private set; } = false;

        private readonly ICaptureService _captureService;
		private readonly ILogger _logger;
		private readonly IContentDialogService _contentDialogService;
        private readonly ISocketProcessMapperService _socketProcessMapperService;
        private readonly IFileDialogService _fileDialogService;

        private readonly DispatcherTimer _timer;
        private readonly List<string> _capturedTextBuffer = new();

        [ObservableProperty]
        private ObservableCollection<string> _deviceNames;
        [ObservableProperty]
        private string? _selectedDeviceName;
        [ObservableProperty]
        private bool _isCapturing;
        [ObservableProperty]
        private CircularBuffer<string> _capturedTextCollection;

        [ObservableProperty]
        private string _customFilter;
        [ObservableProperty]
        private string _remotePorts;
        [ObservableProperty]
        private string _remoteIPAddresses;
        [ObservableProperty]
        private string _localPorts;
        [ObservableProperty]
        private string _localPIDs;
        [ObservableProperty]
        private string _localProcessNames;

        public CapturePageViewModel(ICaptureService captureService, ILogger logger, IContentDialogService contentDialogService, ISocketProcessMapperService socketProcessMapperService, IFileDialogService fileDialogService)
		{
			_captureService = captureService;
            _logger = logger;
			_contentDialogService = contentDialogService;
            _socketProcessMapperService = socketProcessMapperService;
            _fileDialogService = fileDialogService;

            _deviceNames = new ObservableCollection<string>();
            _selectedDeviceName = "Wi-Fi";
            _isCapturing = false;
            _remotePorts = "443";
			_remoteIPAddresses = "";
			_localPorts = "";
			_localPIDs = "";
			_localProcessNames = "";
			_customFilter = "";
            _capturedTextCollection = new CircularBuffer<string>(1000);

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1),
                IsEnabled = false,
            };
            _timer.Tick += (sender, e) => FlushCapturedTextToCollection();

            PropertyChanged += OnPropertyChanged;
        }

        [RelayCommand]
		private void LoadNetworkDevices()
		{
			var devices = _captureService.GetDeviceNames();
			DeviceNames = new ObservableCollection<string>(devices);
			SelectedDeviceName = string.IsNullOrEmpty(SelectedDeviceName)
				? null
				: devices.FirstOrDefault(x => x.Contains(SelectedDeviceName));
		}

        [RelayCommand]
        private async Task StartOrStopCapturingAsync()
		{
            if (IsCapturing)
			{
				StopCapturing();
			}
			else
			{
				await StartCapturingAsync();
			}
		}

		private async Task StartCapturingAsync()
		{
            LoadNetworkDevices();

            // can't proceed forward without a capture device.
            if (string.IsNullOrEmpty(SelectedDeviceName))
            {
                await ShowInvalidCaptureDeviceDialog();
				return;
            }

            // validate input
            if (!TryParseIntString(RemotePorts, out var remotePorts))
            {
                ShowInvalidCaptureInputResponse(nameof(RemotePorts));
                return;
            }

            if (!TryParseIPString(RemoteIPAddresses, out var remoteIPAddresses))
            {
                ShowInvalidCaptureInputResponse(nameof(RemoteIPAddresses));
                return;
            }

            if (!TryParseIntString(LocalPorts, out var localPorts))
            {
                ShowInvalidCaptureInputResponse(nameof(LocalPorts));
                return;
            }

            if (!TryParseIntString(LocalPIDs, out var localPIDs))
            {
                ShowInvalidCaptureInputResponse(nameof(LocalPIDs));
                return;
            }

            if (!TryParseProcessNames(LocalProcessNames, out var localProcessNames))
            {
                ShowInvalidCaptureInputResponse(nameof(LocalProcessNames));
                return;
            }

            if (!CanContinueWithSelectedDevice(SelectedDeviceName)) 
			{
				await ShowAdminRequiredDialog();
                return; 
			}

            if (!_captureService.IsValidFilter(CustomFilter, SelectedDeviceName))
            {
                await ShowInvalidFilterDialog();
                return;
            }

            IsCapturing = true;
            CapturedTextCollection.Clear();

            var filterData = new FilterData(remotePorts: remotePorts, remoteIPAddresses: remoteIPAddresses, localPorts: localPorts, localPIDs: localPIDs, localProcessNames: localProcessNames, CustomFilter, logDnsLookups: true);
            await _captureService.StartCaptureAsync(HandlePacketCapture, filterData, SelectedDeviceName ?? "Wi-Fi", false);
		}


        public bool CanContinueWithSelectedDevice(string deviceName)
        {
            if (_captureService.RequiresAdminPrivileges(deviceName))
            {
                return IsRunningAdminMode();
            }
            return true;
        }

        private static bool IsRunningAdminMode()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private async Task ShowInvalidCaptureDeviceDialog()
        {
            var dialogOptions = new SimpleContentDialogCreateOptions()
            {
                Title = "You must first select a capture device",
                Content = $"{CustomFilter}",
                CloseButtonText = "Ok",
            };

            _ = await _contentDialogService.ShowSimpleDialogAsync(dialogOptions);
        }

        private async Task ShowInvalidFilterDialog()
        {
            var dialogOptions = new SimpleContentDialogCreateOptions()
            {
                Title = "Invalid Filter",
                Content = $"{CustomFilter}",
                CloseButtonText = "Ok",
            };

            _ = await _contentDialogService.ShowSimpleDialogAsync(dialogOptions);
        }

        private async Task ShowAdminRequiredDialog()
        {
            var dialogOptions = new SimpleContentDialogCreateOptions()
            {
                Title = "Admin rights are needed for the selected device",
                Content = "Please restart with admin privileges to continue.",
                CloseButtonText = "Ok",
            };

            _ = await _contentDialogService.ShowSimpleDialogAsync(dialogOptions);
        }

        private void ShowInvalidCaptureInputResponse(string propName) 
		{
            var prop = GetType().GetProperty(propName);
			if (prop == null)
			{
				_logger.Error($"{MethodBase.GetCurrentMethod()?.Name}: failed to get property for {propName}");
				return;
			}
            _logger.Error($"Failed to parse {propName}: '{prop.GetValue(this)}'");
		}

        private static bool TryParseIntString(string csvString, out HashSet<int> outList)
        {
            outList = new HashSet<int>();

            if (string.IsNullOrWhiteSpace(csvString)) return true;

            var numberStrings = csvString.Split(new char[] { ',', ';' });

            foreach (var numberString in numberStrings)
            {
                if (!int.TryParse(numberString.Trim(), out int number))
                {
                    outList.Clear();
                    return false;
                }

                outList.Add(number);
            }

            return true;
        }

        private static bool TryParseIPString(string csvString, out HashSet<IPAddress> outList)
        {
            outList = new HashSet<IPAddress>();

            if (string.IsNullOrWhiteSpace(csvString)) return true;

            var ipStrings = csvString.Split(new char[] { ',', ';' });

            foreach (var ipString in ipStrings)
            {
                if (!IPAddress.TryParse(ipString.Trim(), out var ipAddress))
                {
                    outList.Clear();
                    return false;
                }

                outList.Add(ipAddress!);
            }

            return true;
        }

        private static bool TryParseProcessNames(string csvString, out HashSet<string> outList)
        {
            outList = new HashSet<string>();

            if (string.IsNullOrWhiteSpace(csvString)) return true;

            var processNames = csvString.Split(new char[] { ',', ';' });

            foreach (var processName in processNames)
            {
                outList.Add(processName.Trim());
            }

            return true;
        }


        private void HandlePacketCapture(BasicPacket packet)
		{
			if (packet.Payload.Length > 0)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
                    if (AccessKeyHack)
                    {
                        var (id, key) = HexFun.GetDataFromAccessKeyPacket(Convert.ToHexString(packet.Payload));
                        if (id != "" && key != "")
                        {
                            AddCapturedTextToBuffer($"{id}{Environment.NewLine}{Environment.NewLine}{key}");
                        }
                        return;
                    }

                    AddCapturedTextToBuffer(Convert.ToHexString(packet.Payload));
                });
			}
		}

		private void StopCapturing()
		{
			IsCapturing = false;
			_captureService.StopCapture();
		}


		[RelayCommand]
		private async Task OnShowSavePCapDialog()
		{
			var dialogOptions = new SimpleContentDialogCreateOptions()
			{
				Title = "Save captured packets?",
				Content = "",
				CloseButtonText = "Cancel",
			};

			if (IsCapturing)
			{
				dialogOptions.Content = "Please stop capturing before saving.";
			}
			else if (!_captureService.HasData)
			{
				dialogOptions.Content = "There's no data to save";
			}
			else
			{
				dialogOptions.Content = $"Captured {_captureService.CapturedPacketCount} packets";
				dialogOptions.PrimaryButtonText = "Save";
			}

			var userSelectionResult = await _contentDialogService.ShowSimpleDialogAsync(dialogOptions);

			switch (userSelectionResult)
			{
				case ContentDialogResult.Primary: // User Selected Save
					await SaveCapturedPacketsWizard();
					break;
				case ContentDialogResult.Secondary: // inapplicable
				case ContentDialogResult.None: // User Canceled
				default:
					break;
			}
		}


        private async Task SaveCapturedPacketsWizard()
        {
            var fileName = $"CapturedPackets.{DateTime.Now:yyyy_MM_dd_HH_mm_ss}";

            await _fileDialogService.ShowSaveDialogAndExecuteAction(
                "Pcap Files|*.pcap|All Files|*.*",
                ".pcap",
                "Save captured packets",
                fileName,
                async (string f) => await Task.Run(() => _captureService.WritePacketsToFile(f))
            );
        }
       

		[RelayCommand]
		private async Task OnShowLoadPCapDialog()
		{
			var dialogOptions = new SimpleContentDialogCreateOptions()
			{
				Title = "Load captured packets?",
				Content = "",
				CloseButtonText = "Cancel",
			};

			if (IsCapturing)
			{
				dialogOptions.Content = "Please stop capturing before loading.";
			}
			else
			{
				dialogOptions.Content = $"Load PCAP from file?";
				dialogOptions.PrimaryButtonText = "Load";
			}

			var userSelectionResult = await _contentDialogService.ShowSimpleDialogAsync(dialogOptions);

			switch (userSelectionResult)
			{
				case ContentDialogResult.Primary: // User Selected Load
					await LoadCapturedPacketsWizard();
					break;
				case ContentDialogResult.Secondary: // inapplicable
				case ContentDialogResult.None: // User Canceled
				default:
					break;
			}
		}

        private async Task LoadCapturedPacketsWizard()
        {
            await _fileDialogService.ShowOpenDialogAndExecuteAction(
                "Pcap Files|*.pcap|All Files|*.*",
                ".pcap",
                "Load captured packets",
                async fileName => { await Task.Run(() =>
                    _captureService.LoadPacketsFromFile(HandlePacketCapture, fileName));
                }
            );
        }


        [RelayCommand]
        private async Task ShowKeylogDialog()
        {
            var sslKeyLogFile = Environment.GetEnvironmentVariable("SSLKEYLOGFILE", EnvironmentVariableTarget.User);
            //var mitmProxySslKeyLogFile = Environment.GetEnvironmentVariable("MITMPROXY_SSLKEYLOGFILE");

            var dialogOptions = new SimpleContentDialogCreateOptions()
            {
                Title = "Set sslkeylog?",
                Content = @$"SSLKEYLOGFILE: {sslKeyLogFile}

Capture https traffic from MitmProxy, Firefox or Chrome. Restart those apps.",
                PrimaryButtonText = "Set Env",
                SecondaryButtonText = "Unset Env",
                CloseButtonText = "Cancel",
            };

            var userSelectionResult = await _contentDialogService.ShowSimpleDialogAsync(dialogOptions);

            switch (userSelectionResult)
            {
                case ContentDialogResult.Primary: // Set Env
                    await SetKeyLogFileWizard("SSLKEYLOGFILE");
                    break;
                case ContentDialogResult.Secondary: // Unset
                    Environment.SetEnvironmentVariable("SSLKEYLOGFILE", null, EnvironmentVariableTarget.User);
                    break;
                case ContentDialogResult.None: // Cancel
                    break;
                default:
                    break;
            }
        }

        private async Task SetKeyLogFileWizard(string envVariable)
        {
            await _fileDialogService.ShowSaveDialogAndExecuteAction(
                "Ssl Keys|*.txt",
                ".txt",
                "Set SslKey location",
                envVariable.ToLower(),
                async filePath => {
                    if (!File.Exists(filePath)) File.Create(filePath).Dispose();
                    Environment.SetEnvironmentVariable(envVariable, filePath, EnvironmentVariableTarget.User);
                    await Task.CompletedTask;
                }
            );

            //var sslKeyLogFile = Environment.GetEnvironmentVariable("SSLKEYLOGFILE", EnvironmentVariableTarget.User);
        }
       

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CustomFilter):
                    if (CustomFilter != "") RemotePorts = RemoteIPAddresses = LocalPorts = LocalPIDs = LocalProcessNames = "";
                    break;
                case nameof(RemotePorts):
                    if (RemotePorts != "") CustomFilter = LocalPIDs = LocalProcessNames = "";
                    break;
                case nameof(RemoteIPAddresses):
                    if (RemoteIPAddresses != "") CustomFilter = LocalPIDs = LocalProcessNames = "";
                    break;
                case nameof(LocalPorts):
                    if (LocalPorts != "") CustomFilter = LocalPIDs = LocalProcessNames = "";
                    break;
                case nameof(LocalPIDs):
                    if (LocalPIDs != "") CustomFilter = RemotePorts = RemoteIPAddresses = LocalPorts = "";
                    break;
                case nameof(LocalProcessNames):
                    if (LocalProcessNames != "") CustomFilter = RemotePorts = RemoteIPAddresses = LocalPorts = "";
                    break;
            }
        }


        [RelayCommand]
        public async Task OpenProcessSocketDialog()
        {
            List<TcpProcessRecord> tcpRecords = _socketProcessMapperService.GetAllTcpConnections();
            var viewModelRecords = tcpRecords.Select(r => new TcpProcessRecordViewModel(r)).ToList();
            var recordsWindow = new ProcessPortsDialog(_contentDialogService.GetContentPresenter());
            recordsWindow.dataGrid.ItemsSource = viewModelRecords;

            var dialogResult = await recordsWindow.ShowAsync();
            if (dialogResult == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
				if (recordsWindow.SelectedPropertyName != null && recordsWindow.SelectedValue != null)
				{
                    CustomFilter = "";
                    switch (recordsWindow.SelectedPropertyName)
                    {
                        case "RemotePort":
                            RemotePorts = recordsWindow.SelectedValue;
                            break;
                        case "RemoteAddress":
                            RemoteIPAddresses = recordsWindow.SelectedValue;
                            break;
                        case "LocalPort":
                            LocalPorts = recordsWindow.SelectedValue;
                            break;
                        case "ProcessId":
                            LocalPIDs = recordsWindow.SelectedValue;
                            break;
                        case "ProcessName":
                            LocalProcessNames = recordsWindow.SelectedValue;
                            break;
						default:
							_logger.Error($"Error in result from OpenProcessSocketDialog(). Missing case for '{recordsWindow.SelectedPropertyName}'");
							break;
                    }
                }
            }
        }


        [RelayCommand]
        public async Task OpenFilterEditorDialog(FilterEditorDialog content)
        {
			var dialog = new SimpleContentDialogCreateOptions()
				{
					Title = "Custom Filter",
					Content = content,
					PrimaryButtonText = "Update",
					CloseButtonText = "Cancel",
				};

            var dialogResult = await _contentDialogService.ShowSimpleDialogAsync(dialog);

			if (content.CustomFilter == null || content.CustomFilter.Length == 0) { return; }

            switch (dialogResult)
            {
                case ContentDialogResult.Primary: // User Selected Update
					CustomFilter = content.CustomFilter;
                    break;
                case ContentDialogResult.Secondary: // inapplicable
                case ContentDialogResult.None: // User Canceled
                default:
                    break;
            }
        }

        public void AddCapturedTextToBuffer(string newText)
        {
            _capturedTextBuffer.Add(newText);
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        private void FlushCapturedTextToCollection()
        {
            if (!IsCapturing)
            {
                _timer.Stop();
                _capturedTextBuffer.Clear();
                return;
            }

            var tempData = new CircularBuffer<string>(CapturedTextCollection, CapturedTextCollection.Capacity);
            tempData.AddRange(_capturedTextBuffer);
            CapturedTextCollection = tempData;
            _capturedTextBuffer.Clear();
            _timer.Stop();
        }

    }
    
}
