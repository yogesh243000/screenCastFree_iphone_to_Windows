using System.IO;
using iPhoneMirror.App.Commands;
using iPhoneMirror.Core.Interfaces;
using iPhoneMirror.Core.Models;
using iPhoneMirror.USB.DeviceDiscovery;

namespace iPhoneMirror.App.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IDeviceDiscoveryService _deviceDiscoveryService;

    private ConnectionState _state = ConnectionState.Disconnected;
    private string _statusText = "iPhone Not Connected";
    private string _fpsText = "--";
    private string _resolutionText = "--";
    private string _latencyText = "--";
    private string _usbText = "Disconnected";

    public MainViewModel() : this(new Pymobiledevice3DeviceDiscoveryService())
    {
    }

    public MainViewModel(IDeviceDiscoveryService deviceDiscoveryService)
    {
        _deviceDiscoveryService = deviceDiscoveryService;
        DetectCommand = new RelayCommand(OnDetect, () => _state == ConnectionState.Disconnected);
        ConnectCommand = new RelayCommand(OnConnect, () => _state == ConnectionState.WaitingForTrust);
    }

    public ConnectionState State
    {
        get => _state;
        private set
        {
            if (SetField(ref _state, value))
            {
                DetectCommand.RaiseCanExecuteChanged();
                ConnectCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public string FpsText
    {
        get => _fpsText;
        private set => SetField(ref _fpsText, value);
    }

    public string ResolutionText
    {
        get => _resolutionText;
        private set => SetField(ref _resolutionText, value);
    }

    public string LatencyText
    {
        get => _latencyText;
        private set => SetField(ref _latencyText, value);
    }

    public string UsbText
    {
        get => _usbText;
        private set => SetField(ref _usbText, value);
    }

    public RelayCommand DetectCommand { get; }

    public RelayCommand ConnectCommand { get; }

    private async void OnDetect()
    {
        State = ConnectionState.Detecting;
        StatusText = "Looking for iPhone...";

        try
        {
            var devices = await _deviceDiscoveryService.ListDevicesAsync();
            var device = devices.FirstOrDefault();

            if (device is null)
            {
                State = ConnectionState.Disconnected;
                UsbText = "Disconnected";
                StatusText = "No iPhone detected.\n\nPlease:\n" +
                             "• Unlock your iPhone\n" +
                             "• Connect using a USB data cable\n" +
                             "• Tap Trust if prompted\n" +
                             "• Make sure the Apple device drivers are available";
                return;
            }

            UsbText = "Connected";
            State = ConnectionState.WaitingForTrust;
            StatusText = $"iPhone detected: {device.DeviceName}\n" +
                         $"{device.ProductType}, iOS {device.ProductVersion}\n\n" +
                         "Click Connect to start pairing.";
        }
        catch (FileNotFoundException ex)
        {
            State = ConnectionState.Error;
            UsbText = "Disconnected";
            StatusText = $"Setup problem: {ex.Message}";
        }
        catch (Exception ex)
        {
            State = ConnectionState.Error;
            UsbText = "Disconnected";
            StatusText = "Could not check for an iPhone. See logs for details.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private void OnConnect()
    {
        State = ConnectionState.Error;
        StatusText = "Connect not yet implemented (Milestone 3: pairing)";
    }
}
