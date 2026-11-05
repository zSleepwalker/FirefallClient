using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackImportReturnCapture(
    NetworkDispatcherEntryFallbackPushCapture ParentFallbackPushCapture,
    uint? FreshOperationTargetDWORD,
    bool? NormalReturnObserved);

public enum NetworkDispatcherEntryFallbackImportReturnCaptureState
{
    Unreached,
    Pending,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackImportReturnOutcome
{
    ParentFallbackPushNotAccepted,
    MissingFreshOperationTargetDWORD,
    NormalReturnNotObserved,
    CallDidNotReturnNormally,
    BoundedBeforeCounterIncrement
}

public readonly record struct NetworkDispatcherEntryFallbackImportReturnTargetRead(
    uint RawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value);

public readonly record struct NetworkDispatcherEntryFallbackImportReturnCall(
    uint RawFileOffset,
    uint IndirectCallCellVirtualAddress,
    uint? OperationTargetDWORD,
    bool InstructionReached,
    bool Attempted,
    bool? ReturnedNormally,
    bool Completed);

public sealed record NetworkDispatcherEntryFallbackImportReturnPlan(
    NetworkDispatcherEntryFallbackPushPlan ParentFallbackPushPlan,
    NetworkDispatcherEntryFallbackImportReturnCapture Capture,
    NetworkDispatcherEntryFallbackImportReturnOutcome Outcome,
    NetworkDispatcherEntryFallbackImportReturnTargetRead? TargetRead,
    NetworkDispatcherEntryFallbackImportReturnCall? Call,
    NetworkDispatcherEntryFallbackImportReturnCaptureState OperationTargetCaptureState,
    NetworkDispatcherEntryFallbackImportReturnCaptureState NormalReturnCaptureState,
    ImmutableArray<uint> CompletedInstructionOffsets,
    uint EntryRawFileOffset,
    uint? StopAtRawFileOffsetExclusive,
    uint? PreCallESP,
    uint? PreCallEBX,
    NetworkDispatcherLockGateFlags? FlagsAtStop,
    bool CounterIncrementBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    bool ProjectionEntered)
{
    public bool CounterIncrementProjected => false;
    public bool PostCallESPForwarded => false;
    public bool PostCallEBXForwarded => false;
    public bool PostCallESIForwarded => false;
    public bool PostCallEBPForwarded => false;
    public bool PostCallEAXForwarded => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ImportABIInferred => false;
    public bool ImportMeaningInferred => false;
    public bool OperationTargetMeaningInferred => false;
    public bool OperationTargetIsRawFileOffset => false;
}

/// <summary>
/// Reprojects the fallback import call and its normal return, stopping before
/// the counter increment at BBDDF6.
/// </summary>
public static class NetworkDispatcherEntryFallbackImportReturnProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDF0;
    public const uint IndirectCallRawFileOffset = 0x00BBDDF0;
    public const uint IndirectCallCellVirtualAddress = 0x01AC4278;
    public const uint CounterIncrementRawFileOffset = 0x00BBDDF6;
    public const uint IndirectCallInstructionByteLength = 6;
    public const byte OperationTargetDWORDWidth = sizeof(uint);

    public const string IndirectCallInstructionBytes = "FF157842AC01";
    public const string CapturedWindowBytes =
        "FF158442AC0185C0750753FF157842AC01FF4330";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackImportReturnPlan Project(
        NetworkDispatcherEntryFallbackImportReturnCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentFallbackPushCapture);

        NetworkDispatcherEntryFallbackPushPlan parent =
            NetworkDispatcherEntryFallbackPushProjection.Project(
                capture.ParentFallbackPushCapture);
        bool admitted =
            parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.Push is not null
            && parent.ZeroFallthroughEntered
            && !parent.FallbackImportCallEntered;

        if (!admitted)
        {
            return new(
                parent,
                capture,
                NetworkDispatcherEntryFallbackImportReturnOutcome.ParentFallbackPushNotAccepted,
                null,
                null,
                StateForUnreachedParent(capture.FreshOperationTargetDWORD),
                StateForUnreachedParent(capture.NormalReturnObserved),
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive ?? EntryRawFileOffset,
                parent.StopAtRawFileOffsetExclusive,
                null,
                null,
                parent.FlagsAtStop,
                false,
                false,
                false,
                false);
        }

        uint? operationTarget = capture.FreshOperationTargetDWORD;
        var targetRead = new NetworkDispatcherEntryFallbackImportReturnTargetRead(
            IndirectCallRawFileOffset,
            IndirectCallCellVirtualAddress,
            OperationTargetDWORDWidth,
            Attempted: true,
            Captured: operationTarget.HasValue,
            Value: operationTarget);
        NetworkDispatcherEntryFallbackImportReturnCall? call = operationTarget.HasValue
            ? new NetworkDispatcherEntryFallbackImportReturnCall(
                IndirectCallRawFileOffset,
                IndirectCallCellVirtualAddress,
                operationTarget,
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: capture.NormalReturnObserved,
                Completed: capture.NormalReturnObserved == true)
            : null;

        NetworkDispatcherEntryFallbackImportReturnCaptureState targetState =
            State(operationTarget);
        NetworkDispatcherEntryFallbackImportReturnCaptureState normalReturnState =
            operationTarget.HasValue
                ? capture.NormalReturnObserved.HasValue
                    ? NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
                    : NetworkDispatcherEntryFallbackImportReturnCaptureState.Pending
                : StateForUnused(capture.NormalReturnObserved);

        NetworkDispatcherEntryFallbackImportReturnOutcome outcome;
        ImmutableArray<uint> completedOffsets;
        uint stopAt;
        bool counterIncrementReached = false;
        NetworkDispatcherLockGateFlags? flagsAtStop = parent.FlagsAtStop;

        if (operationTarget is null)
        {
            outcome =
                NetworkDispatcherEntryFallbackImportReturnOutcome.MissingFreshOperationTargetDWORD;
            completedOffsets = parent.CompletedInstructionOffsets;
            stopAt = IndirectCallRawFileOffset;
        }
        else if (capture.NormalReturnObserved is null)
        {
            outcome = NetworkDispatcherEntryFallbackImportReturnOutcome.NormalReturnNotObserved;
            completedOffsets = parent.CompletedInstructionOffsets;
            stopAt = IndirectCallRawFileOffset;
        }
        else if (capture.NormalReturnObserved == false)
        {
            outcome = NetworkDispatcherEntryFallbackImportReturnOutcome.CallDidNotReturnNormally;
            completedOffsets = parent.CompletedInstructionOffsets;
            stopAt = IndirectCallRawFileOffset;
        }
        else
        {
            outcome =
                NetworkDispatcherEntryFallbackImportReturnOutcome.BoundedBeforeCounterIncrement;
            completedOffsets = parent.CompletedInstructionOffsets.Add(
                IndirectCallRawFileOffset);
            stopAt = CounterIncrementRawFileOffset;
            counterIncrementReached = true;
            flagsAtStop = UnknownFlags;
        }

        bool exactBoundedCompletion =
            counterIncrementReached
            && parent.ExactCleanCompletion
            && stopAt == CounterIncrementRawFileOffset;
        bool exactCleanCompletion =
            exactBoundedCompletion
            && targetState == NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
            && normalReturnState == NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured;

        return new(
            parent,
            capture,
            outcome,
            targetRead,
            call,
            targetState,
            normalReturnState,
            completedOffsets,
            EntryRawFileOffset,
            stopAt,
            parent.Push?.StackAddress,
            parent.Push?.Value,
            flagsAtStop,
            counterIncrementReached,
            exactBoundedCompletion,
            exactCleanCompletion,
            true);
    }

    private static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);

    private static NetworkDispatcherEntryFallbackImportReturnCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
            : NetworkDispatcherEntryFallbackImportReturnCaptureState.Missing;

    private static NetworkDispatcherEntryFallbackImportReturnCaptureState State(bool? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackImportReturnCaptureState.Captured
            : NetworkDispatcherEntryFallbackImportReturnCaptureState.Missing;

    private static NetworkDispatcherEntryFallbackImportReturnCaptureState StateForUnused<T>(
        T? value)
        where T : struct =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackImportReturnCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackImportReturnCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackImportReturnCaptureState StateForUnreachedParent<T>(
        T? value)
        where T : struct =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackImportReturnCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackImportReturnCaptureState.Unreached;
}
