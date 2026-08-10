namespace iPhoneMirror.USB.Pymobiledevice3;

/// <summary>Matches the JSON shape of `python -m pymobiledevice3 usbmux list`.</summary>
internal sealed class UsbmuxDeviceDto
{
    public string? DeviceName { get; set; }
    public string? Identifier { get; set; }
    public string? ProductType { get; set; }
    public string? ProductVersion { get; set; }
    public string? ConnectionType { get; set; }
}
