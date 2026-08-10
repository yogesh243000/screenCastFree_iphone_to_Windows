using System.Text.Json;
using iPhoneMirror.Core.Interfaces;
using iPhoneMirror.Core.Models;
using iPhoneMirror.USB.Pymobiledevice3;

namespace iPhoneMirror.USB.DeviceDiscovery;

public sealed class Pymobiledevice3DeviceDiscoveryService : IDeviceDiscoveryService
{
    public async Task<IReadOnlyList<ConnectedDeviceInfo>> ListDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await Pymobiledevice3ProcessRunner.RunAsync(["usbmux", "list"], cancellationToken);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"'pymobiledevice3 usbmux list' failed (exit {result.ExitCode}): {result.StandardError.Trim()}");
        }

        var dtos = JsonSerializer.Deserialize<List<UsbmuxDeviceDto>>(result.StandardOutput)
                   ?? [];

        return dtos
            .Where(d => d.Identifier is not null && d.DeviceName is not null)
            .Select(d => new ConnectedDeviceInfo(
                Udid: d.Identifier!,
                DeviceName: d.DeviceName!,
                ProductType: d.ProductType ?? "Unknown",
                ProductVersion: d.ProductVersion ?? "Unknown"))
            .ToList();
    }
}
