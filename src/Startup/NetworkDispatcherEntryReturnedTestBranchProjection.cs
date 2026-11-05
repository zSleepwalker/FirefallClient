using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryReturnedTestBranchCapture(
    NetworkDispatcherEntryImportReturnCapture ParentEntryImportReturnCapture);

public enum NetworkDispatcherEntryReturnedTestBranchOutcome
{
    EntryReturnPrefixNotAccepted,
    MissingFreshReturnedEAX,
    TestBranchProjected
}

public sealed record NetworkDispatcherEntryReturnedTestBranchPlan(
    NetworkDispatcherEntryImportReturnPlan ParentEntryImportReturnPlan,
    NetworkDispatcherEntryReturnedTestBranchCapture Capture,
    NetworkDispatcherEntryReturnedTestBranchOutcome Outcome,
    NetworkDispatcherLockGatePlan? ExistingLockGateTestPlan,
    ImmutableArray<uint> CompletedInstructionOffsets,
    uint EntryRawFileOffset,
    uint? StopAtRawFileOffsetExclusive,
    NetworkDispatcherLockGateFlags? FlagsAtTestEntry,
    NetworkDispatcherLockGateFlags? FlagsAtStop,
    bool EntryImportReturnBoundaryReached,
    bool TestReached,
    bool? TestBranchTaken,
    bool BranchInstructionReached,
    uint? SelectedDestinationRawFileOffset,
    bool TargetInstructionEntered,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    bool ProjectionEntered)
{
    public bool PostCallESPForwarded => false;
    public bool PostCallEBPForwarded => false;
    public bool PostCallEBXForwarded => false;
    public bool PostCallESIForwarded => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ExistingLockGateCounterReadProjected =>
        ExistingLockGateTestPlan?.Reads.IsEmpty == false;
    public bool ExistingLockGateCounterWriteProjected =>
        ExistingLockGateTestPlan?.Writes.IsEmpty == false;
    public bool ExistingLockGatePushProjected =>
        ExistingLockGateTestPlan?.Pushes.IsEmpty == false;
    public bool ExistingLockGateImportCallProjected =>
        ExistingLockGateTestPlan?.OpaqueCalls.IsEmpty == false;
}

/// <summary>
/// Reprojects the returned EAX TEST/JNZ prefix and stops before either target.
/// </summary>
public static class NetworkDispatcherEntryReturnedTestBranchProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDE5;
    public const uint TestRawFileOffset = 0x00BBDDEB;
    public const uint BranchRawFileOffset = 0x00BBDDED;
    public const uint ZeroFallthroughRawFileOffset = 0x00BBDDEF;
    public const uint NonzeroTargetRawFileOffset = 0x00BBDDF6;
    public const uint TargetCounterIncrementRawFileOffset = 0x00BBDDF6;
    public const uint TargetLockPushRawFileOffset = 0x00BBDDEF;
    public const string TestAndBranchBytes = "85C07507";
    public const string CapturedWindowBytes =
        "FF158442AC0185C0750753FF157842AC01FF4330";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryReturnedTestBranchPlan Project(
        NetworkDispatcherEntryReturnedTestBranchCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentEntryImportReturnCapture);

        NetworkDispatcherEntryImportReturnPlan parent =
            NetworkDispatcherEntryImportReturnProjection.Project(
                capture.ParentEntryImportReturnCapture);
        NetworkDispatcherLockGateFlags? flagsAtStop = ConvertFlags(parent.FlagsAtStop);

        if (!parent.ExactCleanCompletion
            || parent.ExistingLockGateEntryEAX is not uint returnedEAX)
        {
            NetworkDispatcherEntryReturnedTestBranchOutcome outcome =
                parent.Outcome
                    == NetworkDispatcherEntryImportReturnOutcome.MissingFreshReturnedEAX
                    ? NetworkDispatcherEntryReturnedTestBranchOutcome.MissingFreshReturnedEAX
                    : NetworkDispatcherEntryReturnedTestBranchOutcome.EntryReturnPrefixNotAccepted;

            return new(
                parent,
                capture,
                outcome,
                null,
                parent.CompletedInstructionOffsets,
                parent.EntryRawFileOffset,
                parent.StopAtRawFileOffsetExclusive,
                null,
                flagsAtStop,
                parent.ExistingLockGateEntryBoundaryReached,
                TestReached: false,
                TestBranchTaken: null,
                BranchInstructionReached: false,
                SelectedDestinationRawFileOffset: null,
                TargetInstructionEntered: false,
                ExactBoundedCompletion: false,
                ExactCleanCompletion: false,
                ProjectionEntered: false);
        }

        NetworkDispatcherLockGatePlan gate =
            NetworkDispatcherLockGateProjection.Project(
                new NetworkDispatcherLockGateCapture(
                    ResultEAX: returnedEAX,
                    EntryESIReceiver: null,
                    FreshCounterValue: null,
                    EntryEBP: null,
                    FreshArgumentByte: null,
                    OpaqueImportReturnedNormally: null));
        bool branchTaken = gate.TestBranchTaken
            ?? throw new InvalidOperationException("The fresh returned EAX TEST was not projected.");
        uint destination = branchTaken
            ? NonzeroTargetRawFileOffset
            : ZeroFallthroughRawFileOffset;
        bool exactBoundedCompletion =
            gate.TestReached
            && gate.StopAtRawFileOffsetExclusive == destination
            && destination is ZeroFallthroughRawFileOffset or NonzeroTargetRawFileOffset
            && gate.Reads.IsEmpty
            && gate.Writes.IsEmpty
            && gate.Pushes.IsEmpty
            && gate.OpaqueCalls.IsEmpty;
        bool exactCleanCompletion =
            exactBoundedCompletion
            && parent.ExactCleanCompletion
            && gate.TestFlags.HasValue
            && !gate.UnreachedReceiverCaptureProvided
            && !gate.UnreachedCounterCaptureProvided
            && !gate.UnreachedEntryEBPCaptureProvided
            && !gate.UnreachedArgumentByteCaptureProvided
            && !gate.UnreachedOpaqueReturnStatusProvided;
        ImmutableArray<uint> completedOffsets =
            parent.CompletedInstructionOffsets.Add(TestRawFileOffset).Add(BranchRawFileOffset);

        return new(
            parent,
            capture,
            NetworkDispatcherEntryReturnedTestBranchOutcome.TestBranchProjected,
            gate,
            completedOffsets,
            EntryRawFileOffset,
            destination,
            new NetworkDispatcherLockGateFlags(
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null),
            gate.TestFlags,
            EntryImportReturnBoundaryReached: true,
            TestReached: gate.TestReached,
            TestBranchTaken: branchTaken,
            BranchInstructionReached: gate.TestReached,
            SelectedDestinationRawFileOffset: destination,
            TargetInstructionEntered: false,
            ExactBoundedCompletion: exactBoundedCompletion,
            ExactCleanCompletion: exactCleanCompletion,
            ProjectionEntered: true);
    }

    private static NetworkDispatcherLockGateFlags? ConvertFlags(
        NetworkIoCompletionClockGateFlags? flags) =>
        flags is NetworkIoCompletionClockGateFlags value
            ? new NetworkDispatcherLockGateFlags(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;
}
