using iPhoneMirror.App.Commands;
using iPhoneMirror.Core.Models;

namespace iPhoneMirror.App.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private ConnectionState _state = ConnectionState.Disconnected;
    private string _statusText = "iPhone Not Connected";
    private string _fpsText = "--";
    private string _resolutionText = "--";
    private string _latencyText = "--";
    private string _usbText = "Disconnected";

    public MainViewModel()
    {
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

    private void OnDetect()
    {
        // Device discovery lands in Milestone 2 (iPhoneMirror.USB). For now this only
        // proves the UI/command wiring works end to end without faking a device state.
        State = ConnectionState.WaitingForTrust;
        StatusText = "Detect not yet implemented (Milestone 2) - simulating waiting-for-trust state";
    }

    private void OnConnect()
    {
        State = ConnectionState.Error;
        StatusText = "Connect not yet implemented (Milestone 4)";
    }
}
