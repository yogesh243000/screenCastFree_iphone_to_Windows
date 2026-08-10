using iPhoneMirror.Core.Interfaces;
using iPhoneMirror.Core.Models;

namespace iPhoneMirror.Tests.Fakes;

public sealed class FakePairingService : IPairingService
{
    private readonly PairingOutcome _outcome;

    public FakePairingService(PairingOutcome outcome)
    {
        _outcome = outcome;
    }

    public Task<PairingOutcome> EnsurePairedAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        => Task.FromResult(_outcome);
}
