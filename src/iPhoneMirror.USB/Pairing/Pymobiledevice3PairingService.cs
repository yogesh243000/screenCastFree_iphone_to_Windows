using System.Diagnostics;
using iPhoneMirror.Core.Interfaces;
using iPhoneMirror.Core.Models;
using iPhoneMirror.USB.Pymobiledevice3;

namespace iPhoneMirror.USB.Pairing;

public sealed class Pymobiledevice3PairingService : IPairingService
{
    public async Task<PairingOutcome> EnsurePairedAsync(
        TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        ProcessResult result;
        try
        {
            result = await Pymobiledevice3ProcessRunner.RunAsync(["lockdown", "pair"], linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            // The device hasn't responded to the Trust dialog yet - not a failure, just not
            // done. The caller is expected to retry (e.g. after showing "tap Trust" guidance).
            return PairingOutcome.WaitingForUserTrust;
        }
        // A cancellation caused by the caller's own token (not our timeout) propagates as-is.

        if (result.ExitCode == 0)
        {
            return PairingOutcome.Paired;
        }

        Debug.WriteLine($"lockdown pair failed (exit {result.ExitCode}): {result.StandardError}");

        return result.StandardError.Contains("not connected", StringComparison.OrdinalIgnoreCase)
            ? PairingOutcome.NoDeviceConnected
            : PairingOutcome.Failed;
    }
}
