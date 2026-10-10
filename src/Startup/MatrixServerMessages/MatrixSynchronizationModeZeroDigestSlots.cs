namespace Firefall.Reconstruction.Startup;

public readonly record struct NativeFingerprintAttempt(bool? NativeSucceeded, ReadOnlyMemory<byte> CapturedOutput);

public enum MatrixModeZeroDigestOutcome
{
    CapturedSlotsReady,
    UnresolvedFirstAttempt,
    UnresolvedAdapterAttempt
}

public sealed record MatrixModeZeroDigestSlotsResult(
    MatrixModeZeroDigestOutcome Outcome, ReadOnlyMemory<byte>? FirstSlot,
    ReadOnlyMemory<byte>? AdapterSlot, bool? FirstCopied, bool? AdapterCopied)
{
    public ReadOnlyMemory<byte>? WindowsDriveSerialSlot => FirstSlot;
    public bool? WindowsDriveSerialCopied => FirstCopied;
}

/// <summary>Only the two conditional digest copies in the already-selected mode-zero branch; not native activation.</summary>
public static class MatrixSynchronizationModeZeroDigestSlots
{
    public const uint SelectorFileOffset = 0x124B7C0;
    public const uint FirstFingerprintMethodFileOffset = 0x127C730;
    public const uint AdapterCallsiteFileOffset = 0x124B93A;
    public const int FirstSlotOffset = 4;
    public const int AdapterSlotOffset = 0x14;
    public const int DigestLength = 16;

    public static MatrixModeZeroDigestSlotsResult ProjectCapturedFingerprints(NativeWindowsDriveFingerprintResult first,
        NativeAdapterFingerprintResult adapter)
    {
        ArgumentNullException.ThrowIfNull(first);
        return Project(new NativeFingerprintAttempt(first.NativeReturnValue,
            first.Digest ?? ReadOnlyMemory<byte>.Empty), adapter);
    }

    public static MatrixModeZeroDigestSlotsResult Project(NativeFingerprintAttempt first,
        NativeAdapterFingerprintResult adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        byte[]? firstSlot = first.NativeSucceeded == true
            ? CopyDigest(first.CapturedOutput, nameof(first))
            : first.NativeSucceeded == false ? new byte[DigestLength] : null;
        byte[]? adapterSlot = adapter.NativeReturnValue == true
            ? CopyDigest(adapter.Digest ?? throw new ArgumentException("Successful adapter capture requires its digest.", nameof(adapter)),
                nameof(adapter))
            : adapter.NativeReturnValue == false ? new byte[DigestLength] : null;
        var outcome = first.NativeSucceeded is null ? MatrixModeZeroDigestOutcome.UnresolvedFirstAttempt
            : adapter.NativeReturnValue is null ? MatrixModeZeroDigestOutcome.UnresolvedAdapterAttempt
            : MatrixModeZeroDigestOutcome.CapturedSlotsReady;
        ReadOnlyMemory<byte>? firstMemory = null, adapterMemory = null;
        if (firstSlot is not null)
            firstMemory = new(firstSlot);
        if (adapterSlot is not null)
            adapterMemory = new(adapterSlot);
        return new(outcome, firstMemory, adapterMemory,
            first.NativeSucceeded, adapter.NativeReturnValue);
    }

    private static byte[] CopyDigest(ReadOnlyMemory<byte> captured, string parameter)
    {
        if (captured.Length != DigestLength)
            throw new ArgumentException("Successful native fingerprint must contain exactly sixteen captured bytes.", parameter);
        return captured.ToArray();
    }
}
