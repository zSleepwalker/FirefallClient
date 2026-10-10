namespace Firefall.Reconstruction.Startup;

public enum MatrixExitZoneReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode
}

public readonly record struct MatrixExitZoneReadResult(MatrixExitZoneReadOutcome Outcome, int BytesConsumed);

public readonly record struct MatrixExitZoneBranchSelection(
    int ConnectionState, bool InvokeSocketDestroyRequest, bool ClearSocketPointer,
    bool InvokeGlobalHook, bool SubmitExitZoneAck, bool SelectAckDiagnosticLog);

/// <summary>
/// Msg-40 is header-only. Captures known cleanup/state branches without running
/// world cleanup, socket lifetime operations, global hooks or runtime callbacks.
/// </summary>
public static class MatrixExitZoneNativeLayout
{
    public const ushort MessageCode = 40;
    public const uint HandlerFileOffset = 0x491230;
    public const uint FactoryCaseFileOffset = 0x33D757;
    public const uint ConstructorFileOffset = 0x33A4E0;
    public const int DescriptorAllocationSize = 0x28;
    public const int PacketSize = 1;
    public const uint CleanupFileOffset = 0x494B00;
    public const uint CurrentModeCleanupFileOffset = 0x64E210;
    public const uint CurrentModeCleanupRuntimeMessageId = 97;
    public const int RequiredDownstreamPointerOffset = 0x910;
    public const int DownstreamReleasedObjectOffset = 0xB4;
    public const int CurrentModePointerOffset = 0xB98;
    public const int ExitBehaviorByteOffset = 0xB94;
    public const uint GlobalHookFlagVirtualAddress = 0x1E4AC84;
    public const uint SocketDestroyRequestFileOffset = GameSocketDestroyNativeLayout.RequestMethodFileOffset;

    public static MatrixExitZoneReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixExitZoneReadOutcome.TruncatedHeader, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixExitZoneReadOutcome.NonCanonicalHeader, 0);
        if (header.Value != MessageCode)
            return new(MatrixExitZoneReadOutcome.UnexpectedMessageCode, 0);
        return new(MatrixExitZoneReadOutcome.Accepted, header.BytesConsumed);
    }

    public static int CaptureCleanupState(int connectionState, bool hasDownstreamObject, bool hasCurrentModeObject)
    {
        if (!hasDownstreamObject)
            throw new ArgumentException("Native cleanup requires connection +910.", nameof(hasDownstreamObject));
        if (connectionState is not (6 or 7))
            throw new ArgumentOutOfRangeException(nameof(connectionState), "Native cleanup requires connection state 6 or 7.");
        if (!hasCurrentModeObject)
            throw new ArgumentException("Native cleanup requires the current app-mode pointer.", nameof(hasCurrentModeObject));
        return 5;
    }

    /// <summary>The gate reads state after external cleanup returns; reentrancy is not guessed.</summary>
    public static MatrixExitZoneBranchSelection SelectAfterCleanup(int connectionStateAfterCleanup,
        byte exitBehaviorByte, bool hasTransport, bool globalHookEnabled, byte diagnosticFlags)
    {
        if (exitBehaviorByte != 0)
            return new(1, hasTransport, hasTransport, globalHookEnabled, false, false);
        bool submitAck = hasTransport && connectionStateAfterCleanup > 3;
        return new(4, false, false, false, submitAck, submitAck && (diagnosticFlags & 1) != 0);
    }
}
