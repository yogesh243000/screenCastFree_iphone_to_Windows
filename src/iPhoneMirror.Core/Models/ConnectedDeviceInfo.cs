namespace iPhoneMirror.Core.Models;

public sealed record ConnectedDeviceInfo(
    string Udid,
    string DeviceName,
    string ProductType,
    string ProductVersion);
