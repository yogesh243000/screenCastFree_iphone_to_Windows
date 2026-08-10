namespace iPhoneMirror.Core.Models;

public enum ConnectionState
{
    Disconnected,
    Detecting,
    WaitingForTrust,
    Pairing,
    Connected,
    Reconnecting,
    Error
}
