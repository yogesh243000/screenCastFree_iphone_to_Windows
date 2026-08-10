using iPhoneMirror.Core.Models;

namespace iPhoneMirror.Core.Interfaces;

public interface IPairingService
{
    /// <summary>
    /// Ensures the connected device is paired/trusted, triggering the on-device "Trust This
    /// Computer?" dialog if needed. Returns <see cref="PairingOutcome.WaitingForUserTrust"/>
    /// rather than blocking indefinitely if the user hasn't responded within <paramref
    /// name="timeout"/> - the caller should show retry guidance and call again.
    /// </summary>
    Task<PairingOutcome> EnsurePairedAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
}
