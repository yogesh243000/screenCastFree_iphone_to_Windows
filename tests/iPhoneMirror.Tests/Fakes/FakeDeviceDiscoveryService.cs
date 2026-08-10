using iPhoneMirror.Core.Interfaces;
using iPhoneMirror.Core.Models;

namespace iPhoneMirror.Tests.Fakes;

public sealed class FakeDeviceDiscoveryService : IDeviceDiscoveryService
{
    private readonly IReadOnlyList<ConnectedDeviceInfo> _devices;
    private readonly Exception? _throws;

    public FakeDeviceDiscoveryService(IReadOnlyList<ConnectedDeviceInfo>? devices = null, Exception? throws = null)
    {
        _devices = devices ?? [];
        _throws = throws;
    }

    public Task<IReadOnlyList<ConnectedDeviceInfo>> ListDevicesAsync(CancellationToken cancellationToken = default)
    {
        if (_throws is not null)
        {
            throw _throws;
        }

        return Task.FromResult(_devices);
    }
}
