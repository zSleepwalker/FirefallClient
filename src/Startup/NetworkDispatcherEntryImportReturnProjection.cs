using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryImportReturnCapture(
    NetworkDispatcherEntryLockPreparationCapture ParentLockPreparationCapture,
    uint? FreshOperationTargetDWORD,
    bool? NormalReturnObserved,
    uint? FreshReturnedEAX);

public enum NetworkDispatcherEntryImportReturnCaptureState
{
    Unreached,
    Pending,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryImportReturnOutcome
{
    ParentLockPreparationNotAccepted,
    MissingFreshOperationTargetDWORD,
    NormalReturnNotObserved,
    CallDidNotReturnNormally,
    MissingFreshReturnedEAX,
    BoundedBeforeExistingLockGate
}

public readonly record struct NetworkDispatcherEntryImportReturnCall(
    uint RawFileOffset,
    uint IndirectCallCellVirtualAddress,
    uint? OperationTargetDWORD,
    bool InstructionReached,
    bool Attempted,
    bool? ReturnedNormally,
    bool Completed);

public readonly record struct NetworkDispatcherEntryImportReturnTargetRead(
    uint RawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value);

public sealed record NetworkDispatcherEntryImportReturnPlan(
    NetworkDispatcherEntryLockPreparationPlan ParentLockPreparationPlan,
    NetworkDispatcherEntryImportReturnCapture Capture,
    NetworkDispatcherEntryImportReturnOutcome Outcome,
    NetworkDispatcherEntryImportReturnTargetRead? TargetRead,
    NetworkDispatcherEntryImportReturnCall? Call,
    NetworkDispatcherEntryImportReturnCaptureState OperationTargetCaptureState,
    NetworkDispatcherEntryImportReturnCaptureState NormalReturnCaptureState,
    NetworkDispatcherEntryImportReturnCaptureState ReturnedEAXCaptureState,
    ImmutableArray<uint> CompletedInstructionOffsets,
    uint EntryRawFileOffset,
    uint? StopAtRawFileOffsetExclusive,
    uint? PreCallESP,
    uint? PreCallEBP,
    uint? PreCallEBX,
    uint? PreCallESI,
    uint? ExistingLockGateEntryEAX,
    NetworkIoCompletionClockGateFlags? FlagsAtStop,
    bool ExistingLockGateEntryBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    bool ProjectionEntered)
{
    public bool ExistingLockGateTESTReached => false;
    public bool ExistingLockGateProjected => false;
    public bool ExistingLockGateBranchProjected => false;
    public bool PostCallESPForwarded => false;
    public bool PostCallEBPForwarded => false;
    public bool PostCallEBXForwarded => false;
    public bool PostCallESIForwarded => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ImportABIInferred => false;
    public bool ImportMeaningInferred => false;
    public bool OperationTargetMeaningInferred => false;
    public bool OperationTargetIsRawFileOffset => false;
}

/// <summary>
/// Reprojects the indirect entry call and its normal return, stopping before
/// the existing lock-gate TEST at BBDDEB.
/// </summary>
public static class NetworkDispatcherEntryImportReturnProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDE5;
    public const uint IndirectCallRawFileOffset = 0x00BBDDE5;
    public const uint IndirectCallCellVirtualAddress = 0x01AC4284;
    public const uint ExistingLockGateEntryRawFileOffset = 0x00BBDDEB;
    public const uint IndirectCallInstructionByteLength = 6;
    public const byte OperationTargetDWORDWidth = sizeof(uint);

    public const string IndirectCallInstructionBytes = "FF158442AC01";
    public const string CapturedWindowBytes =
        "FF158442AC0185C0750753FF157842AC01FF4330";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryImportReturnPlan Project(
        NetworkDispatcherEntryImportReturnCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentLockPreparationCapture);

        NetworkDispatcherEntryLockPreparationPlan parent =
            NetworkDispatcherEntryLockPreparationProjection.Project(
                capture.ParentLockPreparationCapture);
        bool admitted =
            parent.ExactLockPreparationProjected
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.CurrentESPAtStop.HasValue
            && parent.EntryEBP.HasValue
            && parent.CurrentEBXAtStop.HasValue
            && parent.CurrentESIAtStop.HasValue;

        if (!admitted)
        {
            return new(
                parent,
                capture,
                NetworkDispatcherEntryImportReturnOutcome.ParentLockPreparationNotAccepted,
                null,
                null,
                StateForUnreachedParent(capture.FreshOperationTargetDWORD),
                StateForUnreachedParent(capture.NormalReturnObserved),
                StateForUnreachedParent(capture.FreshReturnedEAX),
                ImmutableArray<uint>.Empty,
                parent.StopAtRawFileOffsetExclusive,
                parent.StopAtRawFileOffsetExclusive,
                null,
                null,
                null,
                null,
                null,
                parent.FlagsAtStop,
                false,
                false,
                false,
                false);
        }

        uint? operationTarget = capture.FreshOperationTargetDWORD;
        var targetRead = new NetworkDispatcherEntryImportReturnTargetRead(
            IndirectCallRawFileOffset,
            IndirectCallCellVirtualAddress,
            OperationTargetDWORDWidth,
            Attempted: true,
            Captured: operationTarget.HasValue,
            Value: operationTarget);
        NetworkDispatcherEntryImportReturnCall? call = operationTarget.HasValue
            ? new NetworkDispatcherEntryImportReturnCall(
                IndirectCallRawFileOffset,
                IndirectCallCellVirtualAddress,
                operationTarget,
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: capture.NormalReturnObserved,
                Completed: capture.NormalReturnObserved == true)
            : null;

        NetworkDispatcherEntryImportReturnCaptureState targetState =
            State(operationTarget);
        NetworkDispatcherEntryImportReturnCaptureState normalReturnState =
            operationTarget.HasValue
                ? capture.NormalReturnObserved.HasValue
                    ? NetworkDispatcherEntryImportReturnCaptureState.Captured
                    : NetworkDispatcherEntryImportReturnCaptureState.Pending
                : StateForUnused(capture.NormalReturnObserved);
        NetworkDispatcherEntryImportReturnCaptureState eaxState =
            operationTarget.HasValue && capture.NormalReturnObserved == true
                ? State(capture.FreshReturnedEAX)
                : StateForUnused(capture.FreshReturnedEAX);

        NetworkDispatcherEntryImportReturnOutcome outcome;
        ImmutableArray<uint> completedOffsets;
        uint stopAt;
        uint? gateEntryEAX = null;
        bool gateEntryReached = false;
        NetworkIoCompletionClockGateFlags? flagsAtStop = parent.FlagsAtStop;

        if (operationTarget is null)
        {
            outcome =
                NetworkDispatcherEntryImportReturnOutcome.MissingFreshOperationTargetDWORD;
            completedOffsets = ImmutableArray<uint>.Empty;
            stopAt = IndirectCallRawFileOffset;
        }
        else if (capture.NormalReturnObserved is null)
        {
            outcome = NetworkDispatcherEntryImportReturnOutcome.NormalReturnNotObserved;
            completedOffsets = ImmutableArray<uint>.Empty;
            stopAt = IndirectCallRawFileOffset;
        }
        else if (capture.NormalReturnObserved == false)
        {
            outcome = NetworkDispatcherEntryImportReturnOutcome.CallDidNotReturnNormally;
            completedOffsets = ImmutableArray<uint>.Empty;
            stopAt = IndirectCallRawFileOffset;
        }
        else
        {
            completedOffsets = ImmutableArray.Create(IndirectCallRawFileOffset);
            stopAt = ExistingLockGateEntryRawFileOffset;
            gateEntryReached = true;
            flagsAtStop = UnknownFlags;

            if (capture.FreshReturnedEAX is uint returnedEAX)
            {
                gateEntryEAX = returnedEAX;
                outcome =
                    NetworkDispatcherEntryImportReturnOutcome.BoundedBeforeExistingLockGate;
            }
            else
            {
                outcome = NetworkDispatcherEntryImportReturnOutcome.MissingFreshReturnedEAX;
            }
        }

        bool exactBoundedCompletion =
            gateEntryReached
            && parent.ExactLockPreparationProjected
            && stopAt == ExistingLockGateEntryRawFileOffset;
        bool exactCleanCompletion =
            exactBoundedCompletion
            && gateEntryEAX.HasValue
            && targetState == NetworkDispatcherEntryImportReturnCaptureState.Captured
            && normalReturnState == NetworkDispatcherEntryImportReturnCaptureState.Captured
            && eaxState == NetworkDispatcherEntryImportReturnCaptureState.Captured;

        return new(
            parent,
            capture,
            outcome,
            targetRead,
            call,
            targetState,
            normalReturnState,
            eaxState,
            completedOffsets,
            EntryRawFileOffset,
            stopAt,
            parent.CurrentESPAtStop,
            parent.EntryEBP,
            parent.CurrentEBXAtStop,
            parent.CurrentESIAtStop,
            gateEntryEAX,
            flagsAtStop,
            gateEntryReached,
            exactBoundedCompletion,
            exactCleanCompletion,
            true);
    }

    private static NetworkIoCompletionClockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);

    private static NetworkDispatcherEntryImportReturnCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryImportReturnCaptureState.Captured
            : NetworkDispatcherEntryImportReturnCaptureState.Missing;

    private static NetworkDispatcherEntryImportReturnCaptureState State(bool? value) =>
        value.HasValue
            ? NetworkDispatcherEntryImportReturnCaptureState.Captured
            : NetworkDispatcherEntryImportReturnCaptureState.Missing;

    private static NetworkDispatcherEntryImportReturnCaptureState StateForUnused<T>(T? value)
        where T : struct =>
        value.HasValue
            ? NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            : NetworkDispatcherEntryImportReturnCaptureState.Unreached;

    private static NetworkDispatcherEntryImportReturnCaptureState StateForUnreachedParent<T>(
        T? value)
        where T : struct =>
        value.HasValue
            ? NetworkDispatcherEntryImportReturnCaptureState.SuppliedUnused
            : NetworkDispatcherEntryImportReturnCaptureState.Unreached;
}
