using Firefall.Reconstruction.Startup;

internal static class MatrixSynchronizationModeZeroDigestSlotsChecks
{
    public static void Run(Action<string, bool> check)
    {
        var failure = NativeAdapterAddressFingerprint.Compute(0, 5, Array.Empty<NativeAdapterAddressCandidate>());
        var skipped = MatrixSynchronizationModeZeroDigestSlots.Project(new(false, new byte[] { 0xFF }), failure);
        check("Native mode-zero optional fingerprint failures retain two zero slots and do not reject the selected branch",
            skipped is { Outcome: MatrixModeZeroDigestOutcome.CapturedSlotsReady, FirstCopied: false, AdapterCopied: false }
            && skipped.FirstSlot is { } firstZero && firstZero.Span.SequenceEqual(new byte[16])
            && skipped.AdapterSlot is { } adapterZero && adapterZero.Span.SequenceEqual(new byte[16]));
        byte[] firstBytes = Enumerable.Range(1, 16).Select(i => (byte)i).ToArray();
        var success = NativeAdapterAddressFingerprint.Compute(111, 0, Array.Empty<NativeAdapterAddressCandidate>());
        var copied = MatrixSynchronizationModeZeroDigestSlots.Project(new(true, firstBytes), success);
        firstBytes[0] = 0xFF;
        check("Native mode-zero separately copies the opaque first fingerprint and adapter MD5 rather than reusing stale temporary bytes",
            copied is { FirstCopied: true, AdapterCopied: true }
            && copied.FirstSlot is { } first && first.Span[0] == 1
            && copied.AdapterSlot is { } adapter && Convert.ToHexString(adapter.Span) == "D41D8CD98F00B204E9800998ECF8427E");
        var mixed = MatrixSynchronizationModeZeroDigestSlots.Project(new(true, new byte[16]), failure);
        check("Native mode-zero adapter failure does not copy the preceding helper's temporary digest",
            mixed is { FirstCopied: true, AdapterCopied: false }
            && mixed.AdapterSlot is { } mixedAdapter && mixedAdapter.Span.SequenceEqual(new byte[16]));
        var unresolvedAdapter = NativeAdapterAddressFingerprint.Compute(111, 0,
            new[] { new NativeAdapterAddressCandidate(6, 6, new byte[6], null) });
        check("Native mode-zero uncaptured fingerprint outcomes remain unresolved rather than treated as native failures",
            MatrixSynchronizationModeZeroDigestSlots.Project(new(null, ReadOnlyMemory<byte>.Empty), success)
                .Outcome == MatrixModeZeroDigestOutcome.UnresolvedFirstAttempt
            && MatrixSynchronizationModeZeroDigestSlots.Project(new(false, ReadOnlyMemory<byte>.Empty), unresolvedAdapter)
                .Outcome == MatrixModeZeroDigestOutcome.UnresolvedAdapterAttempt);
        var partial = MatrixSynchronizationModeZeroDigestSlots.Project(new(null, ReadOnlyMemory<byte>.Empty), success);
        check("Native mode-zero independent digest copies retain known adapter bytes even when the first attempt is uncaptured",
            partial is { FirstSlot: null, FirstCopied: null, AdapterCopied: true }
            && partial.AdapterSlot is { } known && success.Digest is { } successDigest
            && known.Span.SequenceEqual(successDigest.Span));
        bool malformed = false;
        try { MatrixSynchronizationModeZeroDigestSlots.Project(new(true, new byte[15]), success); }
        catch (ArgumentException) { malformed = true; }
        check("Native mode-zero successful digest captures require exactly sixteen bytes", malformed);
    }
}
