using System.Collections.Immutable;
using System.Threading;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackDiagnosticReturnCapture(
    NetworkDispatcherEntryFallbackDiagnosticPreparationCapture ParentCapture,
    bool? NormalReturnObserved);

public enum NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState
{
    Unreached,
    Pending,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackDiagnosticReturnOutcome
{
    ParentPreparationNotAccepted,
    NormalReturnNotObserved,
    CallDidNotReturnNormally,
    BoundedAfterDirectCall
}

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticReturnCallEvidence(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    int RelativeDisplacement,
    uint TargetRawFileOffset,
    uint TargetCodeVirtualAddress,
    bool InstructionReached,
    bool Attempted,
    bool? ReturnedNormally,
    bool Completed);

public sealed class NetworkDispatcherEntryFallbackDiagnosticReturnPlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackDiagnosticPreparationPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackDiagnosticReturnPlan(
        NetworkDispatcherEntryFallbackDiagnosticReturnCapture capture,
        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan parentPlan,
        NetworkDispatcherEntryFallbackDiagnosticReturnOutcome outcome,
        NetworkDispatcherEntryFallbackDiagnosticReturnCallEvidence? call,
        NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState normalReturnCaptureState,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        NetworkDispatcherLockGateFlags? flagsAtStop,
        bool callBoundaryReached,
        bool nextInstructionBoundaryReached,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        Call = call;
        NormalReturnCaptureState = normalReturnCaptureState;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        FlagsAtStop = flagsAtStop;
        CallBoundaryReached = callBoundaryReached;
        NextInstructionBoundaryReached = nextInstructionBoundaryReached;
        ProjectionEntered = projectionEntered;
        reprojectedParentPlan = new(
            () => NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.Project(
                Capture.ParentCapture),
            LazyThreadSafetyMode.ExecutionAndPublication);
        orderedSemanticPrefixPreserved = new(
            ComputeOrderedSemanticPrefixPreserved,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactBoundedCompletion = new(
            ComputeExactBoundedCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactCleanCompletion = new(
            ComputeExactCleanCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public NetworkDispatcherEntryFallbackDiagnosticReturnCapture Capture { get; }
    public NetworkDispatcherEntryFallbackDiagnosticPreparationPlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackDiagnosticReturnOutcome Outcome { get; }
    public NetworkDispatcherEntryFallbackDiagnosticReturnCallEvidence? Call { get; }
    public NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState NormalReturnCaptureState
    {
        get;
    }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint EntryRawFileOffset =>
        NetworkDispatcherEntryFallbackDiagnosticReturnProjection.EntryRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop { get; }
    public bool CallBoundaryReached { get; }
    public bool NextInstructionBoundaryReached { get; }
    public bool ProjectionEntered { get; }
    public bool FlagsInvalidatedOnNormalReturn =>
        Outcome == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome.BoundedAfterDirectCall;
    public bool PostCallFlagsForwarded => false;
    public bool PostCallRegistersForwarded => false;
    public bool PostCallESPForwarded => false;
    public bool PostCallEAXForwarded => false;
    public bool PostCallECXForwarded => false;
    public bool PostCallEBXForwarded => false;
    public bool PostCallESIForwarded => false;
    public bool PostCallEDIForwarded => false;
    public bool PostCallEBPForwarded => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeCallExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ParentInstructionsExecuted => false;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion() =>
        ProjectionEntered
        && ParentPlan.ExactCleanCompletion
        && Outcome == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome
            .BoundedAfterDirectCall
        && Call is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallRawFileOffset,
            InstructionReached: true,
            Attempted: true,
            ReturnedNormally: true,
            Completed: true
        }
        && CallBoundaryReached
        && NextInstructionBoundaryReached
        && CompletedInstructionOffsets.SequenceEqual(
            ParentPlan.CompletedInstructionOffsets.Add(
                NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallRawFileOffset))
        && CompletedInstructionOffsets[^1]
            == NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallRawFileOffset
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .NextInstructionRawFileOffset
        && FlagsAtStop == UnknownFlags;

    private bool ComputeExactCleanCompletion() =>
        ComputeExactBoundedCompletion()
        && OrderedSemanticPrefixPreserved
        && NormalReturnCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.Captured;

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan actual = ParentPlan;
        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan replay =
            reprojectedParentPlan.Value;
        if (actual.Capture != replay.Capture
            || actual.Outcome != replay.Outcome
            || !actual.Instructions.SequenceEqual(replay.Instructions)
            || !actual.CompletedInstructionOffsets.SequenceEqual(
                replay.CompletedInstructionOffsets)
            || actual.StopAtRawFileOffsetExclusive != replay.StopAtRawFileOffsetExclusive
            || actual.FinalESP != replay.FinalESP
            || actual.ECXAfterMove != replay.ECXAfterMove
            || actual.ParentFallthroughAccepted != replay.ParentFallthroughAccepted
            || actual.ProjectionEntered != replay.ProjectionEntered
            || actual.FlagsAtStop != replay.FlagsAtStop
            || actual.ExactBoundedCompletion != replay.ExactBoundedCompletion
            || actual.ExactCleanCompletion != replay.ExactCleanCompletion
            || actual.CallBoundaryReached != replay.CallBoundaryReached
            || !actual.OrderedSemanticPrefixPreserved)
        {
            return false;
        }

        if (!ProjectionEntered)
        {
            return Call is null
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                && StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive
                && FlagsAtStop == ParentPlan.FlagsAtStop
                && !CallBoundaryReached
                && !NextInstructionBoundaryReached;
        }

        bool normalReturnObserved = Capture.NormalReturnObserved == true;
        return CallBoundaryReached
            && Call is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallRawFileOffset,
                InstructionCodeVirtualAddress: var callCodeVirtualAddress,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                        .CallInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                        .CallInstructionBytes,
                RelativeDisplacement:
                    NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                        .CallRelativeDisplacement,
                TargetRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                        .TargetRawFileOffset,
                TargetCodeVirtualAddress: var targetCodeVirtualAddress,
                InstructionReached: true,
                Attempted: true
            }
            && callCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .CallCodeVirtualAddress
            && targetCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .TargetCodeVirtualAddress
            && Call.Value.ReturnedNormally == Capture.NormalReturnObserved
            && Call.Value.Completed == normalReturnObserved
            && (normalReturnObserved
                ? CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets.Add(
                        NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                            .CallRawFileOffset))
                    && StopAtRawFileOffsetExclusive
                        == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                            .NextInstructionRawFileOffset
                    && NextInstructionBoundaryReached
                    && FlagsAtStop == UnknownFlags
                : CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                    && StopAtRawFileOffsetExclusive
                        == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                            .CallRawFileOffset
                    && !NextInstructionBoundaryReached
                    && FlagsAtStop == ParentPlan.FlagsAtStop);
    }

    private static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);
}

/// <summary>
/// Reprojects the direct diagnostic call's observed return and stops before BBDE2B.
/// </summary>
public static class NetworkDispatcherEntryFallbackDiagnosticReturnProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE26;
    public const uint CallRawFileOffset = 0x00BBDE26;
    public const uint NextInstructionRawFileOffset = 0x00BBDE2B;
    public const string NextInstructionBytes = "8B86A8000000";
    public const uint TargetRawFileOffset = 0x012C2030;
    public const byte CallInstructionByteLength = 5;
    public const uint RawToCodeVirtualAddressDelta = 0x00400C00;
    public const int CallRelativeDisplacement = 0x00704205;
    public const byte CallOpcode = 0xE8;
    public const string CallInstructionBytes = "E805427000";
    public const string TargetEntryInstructionBytes = "558BEC";
    public const string CapturedPreparationAndCallBytes =
        "683C4FBB016A01B98085E601E805427000";
    public const bool CallIsDirect = true;
    public const bool BytesIndependentlyVerified = true;

    public static uint EntryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(EntryRawFileOffset);
    public static uint CallCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(CallRawFileOffset);
    public static uint TargetCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(TargetRawFileOffset);
    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackDiagnosticReturnPlan Project(
        NetworkDispatcherEntryFallbackDiagnosticReturnCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentCapture);

        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan parent =
            NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.Project(
                capture.ParentCapture);
        bool admitted =
            parent.ExactCleanCompletion
            && parent.ParentFallthroughAccepted
            && parent.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome
                    .ProjectedThreeInstructionPrefix
            && parent.CallBoundaryReached
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.FinalESP.HasValue
            && parent.ECXAfterMove
                == NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                    .EcxImmediateValue;

        if (!admitted)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticReturnOutcome
                    .ParentPreparationNotAccepted,
                null,
                StateForUnreachedParent(capture.NormalReturnObserved),
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive,
                parent.FlagsAtStop,
                callBoundaryReached: false,
                nextInstructionBoundaryReached: false,
                projectionEntered: false);
        }

        var call = new NetworkDispatcherEntryFallbackDiagnosticReturnCallEvidence(
            CallRawFileOffset,
            CallCodeVirtualAddress,
            CallInstructionByteLength,
            CallInstructionBytes,
            CallRelativeDisplacement,
            TargetRawFileOffset,
            TargetCodeVirtualAddress,
            InstructionReached: true,
            Attempted: true,
            ReturnedNormally: capture.NormalReturnObserved,
            Completed: capture.NormalReturnObserved == true);

        if (capture.NormalReturnObserved is null)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticReturnOutcome.NormalReturnNotObserved,
                call,
                NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.Pending,
                parent.CompletedInstructionOffsets,
                CallRawFileOffset,
                parent.FlagsAtStop,
                callBoundaryReached: true,
                nextInstructionBoundaryReached: false,
                projectionEntered: true);
        }

        if (capture.NormalReturnObserved == false)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticReturnOutcome
                    .CallDidNotReturnNormally,
                call,
                NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.Captured,
                parent.CompletedInstructionOffsets,
                CallRawFileOffset,
                parent.FlagsAtStop,
                callBoundaryReached: true,
                nextInstructionBoundaryReached: false,
                projectionEntered: true);
        }

        return new(
            capture,
            parent,
            NetworkDispatcherEntryFallbackDiagnosticReturnOutcome.BoundedAfterDirectCall,
            call,
            NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.Captured,
            parent.CompletedInstructionOffsets.Add(CallRawFileOffset),
            NextInstructionRawFileOffset,
            UnknownFlags,
            callBoundaryReached: true,
            nextInstructionBoundaryReached: true,
            projectionEntered: true);
    }

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + RawToCodeVirtualAddressDelta);

    private static NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState
        StateForUnreachedParent(bool? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.Unreached;

    private static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);
}
