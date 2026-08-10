using iPhoneMirror.App.ViewModels;
using iPhoneMirror.Core.Models;
using iPhoneMirror.Tests.Fakes;

namespace iPhoneMirror.Tests;

public class MainViewModelTests
{
    [Fact]
    public async Task Detect_NoDeviceFound_StaysDisconnectedWithGuidance()
    {
        var vm = new MainViewModel(new FakeDeviceDiscoveryService(), new FakePairingService(PairingOutcome.Paired));

        await vm.DetectAsync();

        Assert.Equal(ConnectionState.Disconnected, vm.State);
        Assert.Contains("No iPhone detected", vm.StatusText);
        Assert.Equal("Disconnected", vm.UsbText);
    }

    [Fact]
    public async Task Detect_DeviceFound_MovesToWaitingForTrust()
    {
        var device = new ConnectedDeviceInfo("udid-1", "Test iPhone", "iPhone17,2", "26.6");
        var vm = new MainViewModel(
            new FakeDeviceDiscoveryService([device]), new FakePairingService(PairingOutcome.Paired));

        await vm.DetectAsync();

        Assert.Equal(ConnectionState.WaitingForTrust, vm.State);
        Assert.Contains("Test iPhone", vm.StatusText);
        Assert.Equal("Connected", vm.UsbText);
    }

    [Fact]
    public async Task Detect_ToolingMissing_ShowsSetupProblemNotStackTrace()
    {
        var vm = new MainViewModel(
            new FakeDeviceDiscoveryService(throws: new FileNotFoundException("pymobiledevice3 venv missing")),
            new FakePairingService(PairingOutcome.Paired));

        await vm.DetectAsync();

        Assert.Equal(ConnectionState.Error, vm.State);
        Assert.Contains("Setup problem", vm.StatusText);
    }

    [Fact]
    public async Task Connect_Paired_MovesToConnected()
    {
        var vm = new MainViewModel(new FakeDeviceDiscoveryService(), new FakePairingService(PairingOutcome.Paired));

        await vm.ConnectAsync();

        Assert.Equal(ConnectionState.Connected, vm.State);
        Assert.Contains("iPhone connected", vm.StatusText);
    }

    [Fact]
    public async Task Connect_WaitingForUserTrust_StaysRetryableWithGuidance()
    {
        var vm = new MainViewModel(
            new FakeDeviceDiscoveryService(), new FakePairingService(PairingOutcome.WaitingForUserTrust));

        await vm.ConnectAsync();

        Assert.Equal(ConnectionState.WaitingForTrust, vm.State);
        Assert.Contains("Tap \"Trust\"", vm.StatusText);
        Assert.True(vm.ConnectCommand.CanExecute(null));
    }

    [Fact]
    public async Task Connect_DeviceDisconnectedMidPairing_ReturnsToDisconnected()
    {
        var vm = new MainViewModel(
            new FakeDeviceDiscoveryService(), new FakePairingService(PairingOutcome.NoDeviceConnected));

        await vm.ConnectAsync();

        Assert.Equal(ConnectionState.Disconnected, vm.State);
        Assert.Equal("Disconnected", vm.UsbText);
    }

    [Fact]
    public async Task Connect_Failed_AllowsRetry()
    {
        var vm = new MainViewModel(new FakeDeviceDiscoveryService(), new FakePairingService(PairingOutcome.Failed));

        await vm.ConnectAsync();

        Assert.Equal(ConnectionState.WaitingForTrust, vm.State);
        Assert.Contains("Pairing failed", vm.StatusText);
        Assert.True(vm.ConnectCommand.CanExecute(null));
    }

    [Fact]
    public void DetectCommand_DisabledWhileNotDisconnected()
    {
        var vm = new MainViewModel(new FakeDeviceDiscoveryService(), new FakePairingService(PairingOutcome.Paired));

        Assert.True(vm.DetectCommand.CanExecute(null));
        Assert.False(vm.ConnectCommand.CanExecute(null));
    }
}
