using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netryoshka.Services;
using Netryoshka.Views;
using Netryoshka.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Netryoshka.ViewModels
{
    public partial class CapturePageViewModel : ObservableObject
	{
		private readonly ICaptureService _captureService;
		private readonly ILogger _logger;
		private readonly IContentDialogService _contentDialogService;
        private readonly ISocketProcessMapperService _socketProcessMapperService;
        private readonly IFileDialogService _fileDialogService;

        public CapturePageViewModel(ICaptureService captureService, ILogger logger, IContentDialogService contentDialogService, ISocketProcessMapperService socketProcessMapperService, IFileDialogService fileDialogService)
		{
			LoadNetworkDevicesCommand = new SimpleRelayCommand(_ => LoadNetworkDevices());
			_captureService = captureService;
            _logger = logger;
			_contentDialogService = contentDialogService;
            _socketProcessMapperService = socketProcessMapperService;
            _fileDialogService = fileDialogService;
            _selectedDeviceName = "Wi-Fi";

            _remotePorts = "5991";
			_remoteIPAddresses = "";
			_localPorts = "";
			_localPIDs = "";
			_localProcessNames = "";
			_customFilter = "";

            _deviceNames = new ObservableCollection<string>();

            StartOrStopCapturingCommand = new RelayCommand(async () => await StartOrStopCapturingAsync(), () => true);
            //OpenProcessSocketDialogCommand = new RelayCommand(async () => await OpenProcessSocketDialog(), () => true);


        }


		//public event PropertyChangedEventHandler? PropertyChanged;
		//public DataStreamRepository dataStreamManager = DataStreamRepository.Instance;



		private ObservableCollection<string> _deviceNames;
		public ObservableCollection<string> DeviceNames
		{
			get => _deviceNames;
			set => SetProperty(ref _deviceNames, value);
		}

		[ObservableProperty]
		private string? _selectedDeviceName;



		public ICommand LoadNetworkDevicesCommand { get; }
		private void LoadNetworkDevices()
		{
			var devices = _captureService.GetDeviceNames();
			DeviceNames = new ObservableCollection<string>(devices);
			SelectedDeviceName = string.IsNullOrEmpty(SelectedDeviceName)
				? null
				: devices.FirstOrDefault(x => x.Contains(SelectedDeviceName));
		}



		private bool _isCapturing;
		public bool IsCapturing
		{
			get => _isCapturing;
			set => SetProperty(ref _isCapturing, value);
		}


		public ICommand StartOrStopCapturingCommand { get; }
        //public RelayCommand OpenProcessSocketDialogCommand { get; private set; }

        //public ICommand StartCapturingCommand { get; }
        //public ICommand StopCapturingCommand { get; }


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
			OnSendInstructionsToViewToClearCaptureData();

            var filterData = new FilterData(remotePorts: remotePorts, remoteIPAddresses: remoteIPAddresses, localPorts: localPorts, localPIDs: localPIDs, localProcessNames: localProcessNames, CustomFilter);
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

            // userSelectionResult 
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

            // userSelectionResult 
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

            // userSelectionResult 
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
					OnTransmitCaptureDataToView(Convert.ToHexString(packet.Payload));
				});
			}
		}

		private void StopCapturing()
		{
			IsCapturing = false;
			_captureService.StopCapture();
		}

		public class TransmitCapturedDataEventArgs : EventArgs
		{
			public string PacketString { get; }

			public TransmitCapturedDataEventArgs(string captureData)
			{
				PacketString = captureData;
			}
		}

		public event EventHandler<TransmitCapturedDataEventArgs>? TransmitCaptureDataToView;
		private void OnTransmitCaptureDataToView(string packet)
		{
			TransmitCaptureDataToView?.Invoke(this, new TransmitCapturedDataEventArgs(packet));
		}


		public event EventHandler? SendInstructionsToViewToClearCaptureData;
		private void OnSendInstructionsToViewToClearCaptureData()
		{
			SendInstructionsToViewToClearCaptureData?.Invoke(this, EventArgs.Empty);
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

			//object content = new TextBlock() { Text = "foo" };
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

			//_ = userSelectionResult switch
			//{
			//    ContentDialogResult.Primary => "User saved their work",
			//    ContentDialogResult.Secondary => "User did not save their work",
			//    _ => "User cancelled the dialog"
			//};
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



        private string _customFilter;
        public string CustomFilter
        {
            get => _customFilter;
            set
            {
				if (value != "")
				{
                    RemotePorts = "";
                    RemoteIPAddresses = "";
                    LocalPorts = "";
                    LocalPIDs = "";
                    LocalProcessNames = "";
                }

                _customFilter = value;
                OnPropertyChanged(nameof(CustomFilter));
            }
        }

        private string _remotePorts;
        public string RemotePorts
        {
            get => _remotePorts;
            set
            {
				if (value != "")
				{
					CustomFilter = "";
                    LocalPIDs = "";
                    LocalProcessNames = "";
                }
                _remotePorts = value;
                OnPropertyChanged(nameof(RemotePorts));
            }
        }

        private string _remoteIPAddresses;
        public string RemoteIPAddresses
        {
            get => _remoteIPAddresses;
            set
            {
                if (value != "")
                {
                    CustomFilter = "";
                    LocalPIDs = "";
                    LocalProcessNames = "";
                }
                _remoteIPAddresses = value;
                OnPropertyChanged(nameof(RemoteIPAddresses));
            }
        }

        private string _localPorts;
        public string LocalPorts
        {
            get => _localPorts;
            set
            {
                if (value != "")
                {
                    CustomFilter = "";
                    LocalPIDs = "";
                    LocalProcessNames = "";
                }
                _localPorts = value;
                OnPropertyChanged(nameof(LocalPorts));
            }
        }

        private string _localPIDs;
        public string LocalPIDs
        {
            get => _localPIDs;
            set
            {
                if (value != "")
                {
                    CustomFilter = "";
                    RemotePorts = "";
                    RemoteIPAddresses = "";
                    LocalPorts = "";
                }
                _localPIDs = value;
                OnPropertyChanged(nameof(LocalPIDs));
            }
        }

        private string _localProcessNames;
        public string LocalProcessNames
        {
            get => _localProcessNames;
            set
            {
                if (value != "")
                {
                    CustomFilter = "";
                    RemotePorts = "";
                    RemoteIPAddresses = "";
                    LocalPorts = "";
                }
                _localProcessNames = value;
                OnPropertyChanged(nameof(LocalProcessNames));
            }
        }




        [RelayCommand]
        public async Task OpenProcessSocketDialog()
        {
            List<TcpProcessRecord> tcpRecords = _socketProcessMapperService.GetAllTcpConnections();

            var recordsWindow = new ProcessPortsDialog(_contentDialogService.GetContentPresenter());
            recordsWindow.dataGrid.ItemsSource = tcpRecords;
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
					//RemotePorts = "";
					//RemoteIpAddresses = "";
					//LocalPorts = "";
					//LocalPIDs = "";
					//LocalProcessNames = "";
                    break;
                case ContentDialogResult.Secondary: // inapplicable
                case ContentDialogResult.None: // User Canceled
                default:
                    break;
            }
        }



    }


    
}
