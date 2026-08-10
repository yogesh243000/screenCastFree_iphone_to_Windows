namespace iPhoneMirror.Core.Models;

public sealed record DeviceStatus(
    ConnectionState State,
    string? DeviceName = null,
    string? IosVersion = null,
    string? Message = null);
