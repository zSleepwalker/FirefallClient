namespace Firefall.Reconstruction.RedHanded;

public readonly record struct DebugExceptionArguments(
    uint SourceByteCount,
    uint SourceAddress,
    uint MessageByteCount,
    uint MessageAddress);

public enum PrivateExceptionMessageKind
{
    NotPrivateCode,
    NoMessage,
    TooLong,
    RemoteBytes,
    BadMemory
}

public readonly record struct PrivateExceptionMessageDecision(
    PrivateExceptionMessageKind Kind,
    bool ReadsRemoteMemory,
    uint RemoteByteCount);

public readonly record struct DebugSnapshotTransition(
    bool CaptureMetadata,
    uint ResultingState,
    uint ResultingTick,
    bool TerminateMonitoredProcess);

/// <summary>Original-client raw offsets and RedHanded PE VAs for the exception handoff.</summary>
public static class DebugExceptionHandoffNativeLayout
{
    public const int ClientRaiseHelperFileOffset = 0x12C3470;
    public const int ClientFatalRaiseCallFileOffset = 0x12C35CF;
    public const int ClientAssertRaiseCallFileOffset = 0x12C36DF;
    public const int ClientFormattedAssertRaiseCallFileOffset = 0x12C3972;
    public const uint ClientRaiseExceptionImportVa = 0x1AC41B0;
    public const uint ContinuableSnapshotExceptionCode = 0x012ED502;
    public const uint ArgumentCount = 4;
    public const int ArgumentByteCount = 16;
    public const int MessageLengthRecordOffset = 0x1C;
    public const int MessageAddressRecordOffset = 0x20;
    public const uint MessageReaderVa = 0x4088A0;
    public const uint MessageReadCallVa = 0x4089AA;
    public const uint DumpThrottleVa = 0x409850;
    public const uint DumpThrottleMilliseconds = 2000;
    public const uint RipEventCode = 9;
    public const uint RipMetadataCallVa = 0x409A86;
    public const uint RipTerminateCallVa = 0x409AA1;
    public const string NoMessageMarker = "RED5_HALT <<NOMSG>>";
    public const string TooLongMessageMarker = "RED5_HALT <<MSGTOOLONG>>";
    public const string BadMemoryMessageMarker = "RED5_HALT <<BADMEM>>";

    /// <summary>Lengths are supplied native strlen results, excluding NUL; pointers remain opaque.</summary>
    public static DebugExceptionArguments CaptureArguments(
        uint sourceByteCount, uint sourceAddress, uint messageByteCount, uint messageAddress) =>
        new(sourceByteCount, sourceAddress, messageByteCount, messageAddress);

    public static uint CaptureRaiseFlags(bool continuable) => continuable ? 0u : 1u;

    public static bool CaptureDumpEligibility(uint state, uint previousTick, uint observedTick) =>
        state == 0 || unchecked(observedTick - previousTick) > DumpThrottleMilliseconds;

    /// <summary>Separate clock observations preserve the native check-then-update call order.</summary>
    public static DebugSnapshotTransition CaptureFatalTransition(
        uint previousState, uint previousTick, uint eligibilityTick, uint stateUpdateTick) =>
        new(CaptureDumpEligibility(previousState, previousTick, eligibilityTick), 3, stateUpdateTick, true);

    public static DebugSnapshotTransition CaptureSnapshotTransition(
        uint previousState, uint previousTick, uint stateUpdateTick)
    {
        bool capture = previousState == 0 || previousState == 2;
        uint state = capture ? 1u : previousState;
        return new(capture, state, capture ? stateUpdateTick : previousTick, state == 3);
    }

    public static DebugSnapshotTransition CaptureRipTransition(uint previousState, uint previousTick) =>
        new(previousState == 0 || previousState == 2, previousState, previousTick, true);

    public static PrivateExceptionMessageDecision CaptureMessage(
        uint exceptionCode, uint parameterCount, uint messageByteCount, bool readSucceeded)
    {
        if (unchecked(exceptionCode - CrashMetadataNativeLayout.EmbeddedSourceExceptionCode) > 2)
            return new(PrivateExceptionMessageKind.NotPrivateCode, false, 0);
        if (parameterCount < ArgumentCount)
            return new(PrivateExceptionMessageKind.NoMessage, false, 0);
        if (messageByteCount > CrashMetadataNativeLayout.MaximumSourceByteCount)
            return new(PrivateExceptionMessageKind.TooLong, false, 0);
        return new(readSucceeded ? PrivateExceptionMessageKind.RemoteBytes
                : PrivateExceptionMessageKind.BadMemory,
            true, messageByteCount);
    }
}
