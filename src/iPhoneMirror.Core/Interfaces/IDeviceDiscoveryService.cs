using iPhoneMirror.Core.Models;

namespace iPhoneMirror.Core.Interfaces;

public interface IDeviceDiscoveryService
{
    Task<IReadOnlyList<ConnectedDeviceInfo>> ListDevicesAsync(CancellationToken cancellationToken = default);
}
