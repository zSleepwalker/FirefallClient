using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackPushCapture(
    NetworkDispatcherEntryReturnedTestBranchCapture ParentReturnedTestBranchCapture,
    uint? FreshPostCallESP,
    uint? FreshPostCallEBX);

public enum NetworkDispatcherEntryFallbackPushCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackPushOutcome
{
    EntryTestBranchNotAccepted,
    NonzeroTestBranchRejected,
    MissingFreshPostCallESP,
    MissingFreshPostCallEBX,
    PushProjected
}

public readonly record struct NetworkDispatcherEntryFallbackPushEffect(
    uint RawFileOffset,
    uint StackAddress,
    uint Value,
    bool Attempted,
    bool Committed);

public sealed record NetworkDispatcherEntryFallbackPushPlan(
    NetworkDispatcherEntryReturnedTestBranchPlan ParentTestBranchPlan,
    NetworkDispatcherEntryFallbackPushCapture Capture,
    NetworkDispatcherEntryFallbackPushOutcome Outcome,
    NetworkDispatcherEntryFallbackPushCaptureState FreshPostCallESPCaptureState,
    NetworkDispatcherEntryFallbackPushCaptureState FreshPostCallEBXCaptureState,
    ImmutableArray<uint> CompletedInstructionOffsets,
    uint EntryRawFileOffset,
    uint? StopAtRawFileOffsetExclusive,
    NetworkDispatcherLockGateFlags? FlagsAtTestEntry,
    NetworkDispatcherLockGateFlags? FlagsAtStop,
    bool ZeroFallthroughEntered,
    NetworkDispatcherEntryFallbackPushEffect? Push,
    bool FallbackImportCallEntered,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    bool ProjectionEntered)
{
    public NetworkDispatcherLockGateFlags? FlagsBeforePush =>
        ZeroFallthroughEntered ? ParentTestBranchPlan.FlagsAtStop : null;
    public bool PushInstructionExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool PreCallESPForwarded => false;
    public bool PreCallESILockBaseUsed => false;
    public bool PostCallESPForwarded => false;
    public bool PostCallEBXForwarded => false;
    public bool FallbackImportCallProjected => false;
    public bool FallbackImportTargetRead => false;
    public bool FallbackImportReturnObserved => false;
}

/// <summary>
/// Reprojects only the zero-TEST fallthrough PUSH EBX and stops before its import call.
/// </summary>
public static class NetworkDispatcherEntryFallbackPushProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDEF;
    public const uint PushRawFileOffset = 0x00BBDDEF;
    public const uint FallbackImportCallRawFileOffset = 0x00BBDDF0;
    public const byte PushInstructionByteLength = 1;
    public const uint StackPointerDecrement = sizeof(uint);
    public const string PushInstructionBytes = "53";
    public const string CapturedWindowBytes =
        "FF158442AC0185C0750753FF157842AC01FF4330";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackPushPlan Project(
        NetworkDispatcherEntryFallbackPushCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentReturnedTestBranchCapture);

        NetworkDispatcherEntryReturnedTestBranchPlan parent =
            NetworkDispatcherEntryReturnedTestBranchProjection.Project(
                capture.ParentReturnedTestBranchCapture);
        bool zeroFallthroughEntered =
            parent.ExactCleanCompletion
            && parent.TestReached
            && parent.BranchInstructionReached
            && parent.TestBranchTaken == false
            && parent.SelectedDestinationRawFileOffset == EntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset;

        if (!zeroFallthroughEntered)
        {
            bool nonzeroBranchRejected =
                parent.ExactCleanCompletion
                && parent.TestBranchTaken == true
                && parent.SelectedDestinationRawFileOffset
                    == NetworkDispatcherEntryReturnedTestBranchProjection
                        .NonzeroTargetRawFileOffset;
            return new(
                parent,
                capture,
                nonzeroBranchRejected
                    ? NetworkDispatcherEntryFallbackPushOutcome.NonzeroTestBranchRejected
                    : NetworkDispatcherEntryFallbackPushOutcome.EntryTestBranchNotAccepted,
                StateForUnused(capture.FreshPostCallESP),
                StateForUnused(capture.FreshPostCallEBX),
                parent.CompletedInstructionOffsets,
                EntryRawFileOffset,
                parent.StopAtRawFileOffsetExclusive,
                parent.FlagsAtTestEntry,
                parent.FlagsAtStop,
                false,
                null,
                FallbackImportCallEntered: false,
                ExactBoundedCompletion: false,
                ExactCleanCompletion: false,
                ProjectionEntered: false);
        }

        NetworkDispatcherEntryFallbackPushCaptureState espState =
            State(capture.FreshPostCallESP);
        if (capture.FreshPostCallESP is not uint freshPostCallESP)
        {
            return StopBeforePush(
                parent,
                capture,
                NetworkDispatcherEntryFallbackPushOutcome.MissingFreshPostCallESP,
                espState,
                StateForUnused(capture.FreshPostCallEBX));
        }

        NetworkDispatcherEntryFallbackPushCaptureState ebxState =
            State(capture.FreshPostCallEBX);
        if (capture.FreshPostCallEBX is not uint freshPostCallEBX)
        {
            return StopBeforePush(
                parent,
                capture,
                NetworkDispatcherEntryFallbackPushOutcome.MissingFreshPostCallEBX,
                espState,
                ebxState);
        }

        uint stackAddress = unchecked(freshPostCallESP - StackPointerDecrement);
        var push = new NetworkDispatcherEntryFallbackPushEffect(
            PushRawFileOffset,
            stackAddress,
            freshPostCallEBX,
            Attempted: true,
            Committed: true);
        ImmutableArray<uint> completedOffsets =
            parent.CompletedInstructionOffsets.Add(PushRawFileOffset);
        bool exactBoundedCompletion =
            parent.ExactCleanCompletion
            && completedOffsets[^1] == PushRawFileOffset
            && FallbackImportCallRawFileOffset
                == PushRawFileOffset + PushInstructionByteLength;

        return new(
            parent,
            capture,
            NetworkDispatcherEntryFallbackPushOutcome.PushProjected,
            espState,
            ebxState,
            completedOffsets,
            EntryRawFileOffset,
            FallbackImportCallRawFileOffset,
            parent.FlagsAtTestEntry,
            parent.FlagsAtStop,
            true,
            push,
            FallbackImportCallEntered: false,
            ExactBoundedCompletion: exactBoundedCompletion,
            ExactCleanCompletion: exactBoundedCompletion,
            ProjectionEntered: true);
    }

    private static NetworkDispatcherEntryFallbackPushPlan StopBeforePush(
        NetworkDispatcherEntryReturnedTestBranchPlan parent,
        NetworkDispatcherEntryFallbackPushCapture capture,
        NetworkDispatcherEntryFallbackPushOutcome outcome,
        NetworkDispatcherEntryFallbackPushCaptureState espState,
        NetworkDispatcherEntryFallbackPushCaptureState ebxState) =>
        new(
            parent,
            capture,
            outcome,
            espState,
            ebxState,
            parent.CompletedInstructionOffsets,
            EntryRawFileOffset,
            PushRawFileOffset,
            parent.FlagsAtTestEntry,
            parent.FlagsAtStop,
            true,
            null,
            FallbackImportCallEntered: false,
            ExactBoundedCompletion: false,
            ExactCleanCompletion: false,
            ProjectionEntered: true);

    private static NetworkDispatcherEntryFallbackPushCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackPushCaptureState.Captured
            : NetworkDispatcherEntryFallbackPushCaptureState.Missing;

    private static NetworkDispatcherEntryFallbackPushCaptureState StateForUnused(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackPushCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackPushCaptureState.Unreached;
}
