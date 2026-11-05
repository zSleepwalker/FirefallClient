using System.Collections.Immutable;
using System.Threading;

namespace Firefall.Reconstruction.Startup;

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchCapture(
    NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture ParentCapture,
    bool? OpaqueCallReturnedNormally = null);

public enum NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
{
    ParentPrefixNotAccepted,
    MissingBranchCondition,
    InconsistentBranchCondition,
    JzNotTakenToBBDE40,
    MissingOpaqueCallReturnStatus,
    OpaqueCallDidNotReturnNormally,
    CompletedAtNormalReturnBoundaryBBDE8C
}

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchJz(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    int SignedRelativeDisplacement,
    uint BranchTargetCodeVirtualAddress,
    uint BranchTargetRawFileOffset,
    uint SelectedDestinationRawFileOffset,
    uint TestedEAX,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter,
    bool Taken,
    bool Attempted,
    bool Reached,
    bool Committed);

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchLea(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    uint ESIValue,
    uint? EBXBefore,
    uint EBXAfter,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    bool FlagsWritten,
    bool NativeMemoryReadExecuted,
    bool NativeMemoryWriteExecuted);

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchCallBoundary(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    uint IndirectPointerAddress,
    bool Reached,
    bool CallExecuted,
    bool PointerReadExecuted,
    bool CalleeContinuationIdentified);

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchOpaqueCallReturn(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    uint IndirectPointerAddress,
    uint NextInstructionRawFileOffset,
    uint NextInstructionCodeVirtualAddress,
    byte NextInstructionByteLength,
    string NextInstructionBytes,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter,
    bool BoundaryReached,
    bool Attempted,
    bool? ReturnedNormally,
    bool Completed,
    bool NormalReturnBoundaryReached,
    bool PointerReadExecuted,
    bool TargetExecuted,
    bool NextInstructionExecuted);

public sealed class NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan(
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCapture capture,
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parentPlan,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome outcome,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState esiCaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState dwordAtA8CaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState dwordAtA4CaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState branchConditionCaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState targetESICaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
            normalReturnCaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchJz? jz,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchLea? addressCalculation,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCallBoundary? callBoundary,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchOpaqueCallReturn? opaqueCallReturn,
        ImmutableArray<uint> completedInstructionOffsets,
        bool parentContinuationReached,
        bool projectionEntered,
        bool suffixAdmitted,
        uint? stopAtRawFileOffsetExclusive,
        uint? lastReachedRawFileOffset,
        NetworkDispatcherLockGateFlags? flagsAtStop)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        PostCallESICaptureState = esiCaptureState;
        DWORDAtA8CaptureState = dwordAtA8CaptureState;
        DWORDAtA4CaptureState = dwordAtA4CaptureState;
        BranchConditionCaptureState = branchConditionCaptureState;
        TargetESICaptureState = targetESICaptureState;
        NormalReturnCaptureState = normalReturnCaptureState;
        Jz = jz;
        AddressCalculation = addressCalculation;
        CallBoundary = callBoundary;
        OpaqueCallReturn = opaqueCallReturn;
        CompletedInstructionOffsets = completedInstructionOffsets;
        ParentContinuationReached = parentContinuationReached;
        ProjectionEntered = projectionEntered;
        SuffixAdmitted = suffixAdmitted;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        FlagsAtStop = flagsAtStop;
        reprojectedParentPlan = new(
            () => NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.Project(
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

    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCapture Capture { get; }
    public NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome Outcome { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        PostCallESICaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        DWORDAtA8CaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        DWORDAtA4CaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        BranchConditionCaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        TargetESICaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        NormalReturnCaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchJz? Jz { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchLea? AddressCalculation { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCallBoundary? CallBoundary { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchOpaqueCallReturn?
        OpaqueCallReturn { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint EntryRawFileOffset =>
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.BranchRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public uint? LastReachedRawFileOffset { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop { get; }
    public bool ParentContinuationReached { get; }
    public bool ProjectionEntered { get; }
    public bool SuffixAdmitted { get; }
    public bool BranchProjected => Jz.HasValue;
    public bool JzTaken => Jz is { Taken: true };
    public bool JzNotTaken => Jz is { Taken: false };
    public bool TargetPathEntered => AddressCalculation.HasValue;
    public uint? BranchTargetRawFileOffset =>
        ParentContinuationReached
            ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .BranchTargetRawFileOffset
            : null;
    public uint? BranchTargetCodeVirtualAddress =>
        ParentContinuationReached
            ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .BranchTargetCodeVirtualAddress
            : null;
    public uint? SelectedDestinationRawFileOffset =>
        Jz is { } branch ? branch.SelectedDestinationRawFileOffset : null;
    public uint? PreCallESI => ParentPlan.ESI;
    public uint? PreCallEAX => ParentPlan.EAX;
    public uint? ESI => OpaqueCallReturn.HasValue ? null : ParentPlan.ESI;
    public uint? EAX => OpaqueCallReturn.HasValue ? null : ParentPlan.EAX;
    public uint? EBX =>
        OpaqueCallReturn.HasValue
            ? null
            : AddressCalculation is { } calculation
                ? calculation.EBXAfter
                : ParentPlan.EBX;
    public uint? EBXBeforeCall =>
        AddressCalculation is { } calculation
            ? calculation.EBXAfter
            : null;
    public uint? EBXAtCallBoundary =>
        AddressCalculation is { } calculation
            ? calculation.EBXAfter
            : null;
    public bool CallBoundaryReached => CallBoundary is { Reached: true };
    public bool OpaqueCallAttempted => OpaqueCallReturn is { Attempted: true };
    public bool OpaqueCallCompleted => OpaqueCallReturn is { Completed: true };
    public bool NormalReturnBoundaryReached =>
        OpaqueCallReturn is { NormalReturnBoundaryReached: true };
    public bool? OpaqueCallReturnedNormally => OpaqueCallReturn?.ReturnedNormally;
    public bool PostCallFlagsForwarded => false;
    public bool PostCallRegistersForwarded => false;
    public uint? PostCallEBX => null;
    public uint? PostCallEAX => null;
    public bool BranchExecuted => false;
    public bool TargetInstructionExecuted => false;
    public bool CallExecuted => false;
    public bool OpaqueCellReadExecuted => false;
    public bool CalleeContinuationIdentified => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ParentInstructionsExecuted => false;
    public bool BranchConditionFlagsPreserved =>
        ParentContinuationReached
        && ParentPlan.FlagsAtStop.HasValue
        && Jz is { } branch
        && branch.FlagsBefore == ParentPlan.FlagsAtStop
        && branch.FlagsAfter == ParentPlan.FlagsAtStop
        && (!AddressCalculation.HasValue
            || AddressCalculation.Value.FlagsBefore == ParentPlan.FlagsAtStop
                && AddressCalculation.Value.FlagsAfter == ParentPlan.FlagsAtStop
                && !AddressCalculation.Value.FlagsWritten);
    public bool ConditionFlagsPreserved =>
        BranchConditionFlagsPreserved
        && FlagsAtStop == ParentPlan.FlagsAtStop;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion()
    {
        if (!ParentContinuationReached)
        {
            return ParentPlan.ExactBoundedCompletion
                && Jz is null
                && AddressCalculation is null
                && CallBoundary is null
                && OpaqueCallReturn is null
                && NormalReturnCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.OpaqueCallReturnedNormally)
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                && StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive
                && LastReachedRawFileOffset == LastCompletedParentInstruction()
                && FlagsAtStop == ParentPlan.FlagsAtStop;
        }

        if (Jz is not {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .BranchRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .BranchInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .BranchInstructionBytes,
                SignedRelativeDisplacement:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .SignedRelativeDisplacement,
                BranchTargetRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .BranchTargetRawFileOffset,
                Attempted: true,
                Reached: true,
                Committed: true
            }
            || !BranchConditionFlagsPreserved
            || Jz.Value.TestedEAX != ParentPlan.EAX
            || Jz.Value.InstructionCodeVirtualAddress
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .BranchCodeVirtualAddress
            || Jz.Value.BranchTargetCodeVirtualAddress
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .BranchTargetCodeVirtualAddress
            || Jz.Value.Taken != (ParentPlan.FlagsAtStop?.Zero == true)
            || Jz.Value.Taken != (ParentPlan.EAX == 0))
        {
            return false;
        }

        ImmutableArray<uint> expectedOffsets =
            ParentPlan.CompletedInstructionOffsets.Add(
                NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .BranchRawFileOffset);
        if (!Jz.Value.Taken)
        {
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .JzNotTakenToBBDE40
                && Jz.Value.SelectedDestinationRawFileOffset
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .FallThroughRawFileOffset
                && AddressCalculation is null
                && CallBoundary is null
                && OpaqueCallReturn is null
                && NormalReturnCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.OpaqueCallReturnedNormally)
                && CompletedInstructionOffsets.SequenceEqual(expectedOffsets)
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .FallThroughRawFileOffset
                && LastReachedRawFileOffset
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .BranchRawFileOffset
                && FlagsAtStop == ParentPlan.FlagsAtStop;
        }

        uint expectedEBX = unchecked(ParentPlan.ESI!.Value
            + NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .ESIToOpaqueCellDisplacement);
        expectedOffsets = expectedOffsets.Add(
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .LeaEBXRawFileOffset);
        expectedOffsets = expectedOffsets.Add(
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .CallBoundaryRawFileOffset);
        return Outcome
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                    .CompletedAtNormalReturnBoundaryBBDE8C
            && SuffixAdmitted
            && AddressCalculation is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaEBXRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaEBXInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaEBXInstructionBytes,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsWritten: false,
                NativeMemoryReadExecuted: false,
                NativeMemoryWriteExecuted: false
            }
            && AddressCalculation.Value.InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .LeaEBXCodeVirtualAddress
            && AddressCalculation.Value.ESIValue == ParentPlan.ESI
            && AddressCalculation.Value.EBXBefore == ParentPlan.EBX
            && AddressCalculation.Value.EBXAfter == expectedEBX
            && AddressCalculation.Value.FlagsBefore == ParentPlan.FlagsAtStop
            && AddressCalculation.Value.FlagsAfter == ParentPlan.FlagsAtStop
            && CallBoundary is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryInstructionBytes,
                IndirectPointerAddress:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .OpaqueCellPointerVirtualAddress,
                Reached: true,
                CallExecuted: false,
                PointerReadExecuted: false,
                CalleeContinuationIdentified: false
            }
            && CallBoundary.Value.InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryCodeVirtualAddress
            && NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && OpaqueCallReturnedNormally == true
            && OpaqueCallReturn is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryInstructionBytes,
                IndirectPointerAddress:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .OpaqueCellPointerVirtualAddress,
                NextInstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .NormalReturnRawFileOffset,
                NextInstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .NormalReturnInstructionByteLength,
                NextInstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .NormalReturnInstructionBytes,
                BoundaryReached: true,
                Attempted: true,
                ReturnedNormally: true,
                Completed: true,
                NormalReturnBoundaryReached: true,
                PointerReadExecuted: false,
                TargetExecuted: false,
                NextInstructionExecuted: false
            }
            && OpaqueCallReturn.Value.InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryCodeVirtualAddress
            && OpaqueCallReturn.Value.NextInstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NormalReturnCodeVirtualAddress
            && OpaqueCallReturn.Value.FlagsBefore == ParentPlan.FlagsAtStop
            && OpaqueCallReturn.Value.FlagsAfter
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .UnknownFlags
            && CompletedInstructionOffsets.SequenceEqual(expectedOffsets)
            && StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NormalReturnRawFileOffset
            && LastReachedRawFileOffset
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryRawFileOffset
            && FlagsAtStop
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .UnknownFlags;
    }

    private bool ComputeExactCleanCompletion() =>
        SuffixAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                .CompletedAtNormalReturnBoundaryBBDE8C
        && ExactBoundedCompletion
        && BranchConditionCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && TargetESICaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && NormalReturnCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && OpaqueCallReturnedNormally == true
        && BranchConditionFlagsPreserved
        && CallBoundaryReached
        && !BranchExecuted
        && !TargetInstructionExecuted
        && !CallExecuted
        && !OpaqueCellReadExecuted
        && !CalleeContinuationIdentified
        && !NativeInstructionsExecuted
        && !NativeMemoryReadsExecuted
        && !NativeMemoryWritesExecuted
        && !ParentInstructionsExecuted
        && OrderedSemanticPrefixPreserved;

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan replay =
            reprojectedParentPlan.Value;
        if (!ReprojectedParentMatches(replay))
        {
            return false;
        }

        if (!ParentContinuationReached)
        {
            return Jz is null
                && AddressCalculation is null
                && CallBoundary is null
                && OpaqueCallReturn is null
                && NormalReturnCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.OpaqueCallReturnedNormally)
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                && StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive
                && FlagsAtStop == ParentPlan.FlagsAtStop;
        }

        if (Jz is not { } branch
            || !CompletedInstructionOffsets.Take(
                ParentPlan.CompletedInstructionOffsets.Length)
                .SequenceEqual(ParentPlan.CompletedInstructionOffsets)
            || branch.InstructionRawFileOffset
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .BranchRawFileOffset
            || branch.FlagsBefore != ParentPlan.FlagsAtStop
            || branch.FlagsAfter != ParentPlan.FlagsAtStop)
        {
            return false;
        }

        if (!branch.Taken)
        {
            return AddressCalculation is null
                && CallBoundary is null
                && OpaqueCallReturn is null
                && NormalReturnCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.OpaqueCallReturnedNormally)
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets.Add(
                        NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                            .BranchRawFileOffset))
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .FallThroughRawFileOffset;
        }

        if (AddressCalculation is not { } lea
            || lea.InstructionRawFileOffset
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .LeaEBXRawFileOffset
            || lea.InstructionCodeVirtualAddress
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .LeaEBXCodeVirtualAddress
            || lea.InstructionByteLength
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .LeaEBXInstructionByteLength
            || lea.InstructionBytes
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .LeaEBXInstructionBytes
            || !lea.Attempted
            || !lea.Reached
            || !lea.Committed
            || lea.FlagsWritten
            || lea.NativeMemoryReadExecuted
            || lea.NativeMemoryWriteExecuted
            || lea.ESIValue != ParentPlan.ESI
            || lea.EBXBefore != ParentPlan.EBX
            || lea.FlagsBefore != ParentPlan.FlagsAtStop
            || lea.FlagsAfter != ParentPlan.FlagsAtStop)
        {
            return false;
        }

        ImmutableArray<uint> targetPathOffsets = ParentPlan.CompletedInstructionOffsets
            .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .BranchRawFileOffset)
            .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .LeaEBXRawFileOffset);
        if (lea.EBXAfter != unchecked(ParentPlan.ESI!.Value
                + NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .ESIToOpaqueCellDisplacement)
            || CallBoundary is not {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryInstructionBytes,
                IndirectPointerAddress:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .OpaqueCellPointerVirtualAddress,
                Reached: true,
                CallExecuted: false,
                PointerReadExecuted: false,
                CalleeContinuationIdentified: false
            }
            || CallBoundary.Value.InstructionCodeVirtualAddress
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryCodeVirtualAddress
            || OpaqueCallReturn is not { } call
            || call.InstructionRawFileOffset
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryRawFileOffset
            || call.InstructionCodeVirtualAddress
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryCodeVirtualAddress
            || call.InstructionByteLength
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryInstructionByteLength
            || call.InstructionBytes
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryInstructionBytes
            || call.IndirectPointerAddress
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .OpaqueCellPointerVirtualAddress
            || call.NextInstructionRawFileOffset
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NormalReturnRawFileOffset
            || call.NextInstructionCodeVirtualAddress
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NormalReturnCodeVirtualAddress
            || call.NextInstructionByteLength
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NormalReturnInstructionByteLength
            || call.NextInstructionBytes
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NormalReturnInstructionBytes
            || !call.BoundaryReached
            || !call.Attempted
            || call.PointerReadExecuted
            || call.TargetExecuted
            || call.NextInstructionExecuted
            || call.FlagsBefore != ParentPlan.FlagsAtStop)
        {
            return false;
        }

        if (Capture.OpaqueCallReturnedNormally == true)
        {
            return NormalReturnCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
                && call.ReturnedNormally == true
                && call.Completed
                && call.NormalReturnBoundaryReached
                && call.FlagsAfter
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .UnknownFlags
                && CompletedInstructionOffsets.SequenceEqual(targetPathOffsets.Add(
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryRawFileOffset))
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .NormalReturnRawFileOffset
                && LastReachedRawFileOffset
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryRawFileOffset
                && FlagsAtStop
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .UnknownFlags;
        }

        return NormalReturnCaptureState
                == (Capture.OpaqueCallReturnedNormally.HasValue
                    ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
                    : NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing)
            && call.ReturnedNormally == Capture.OpaqueCallReturnedNormally
            && !call.Completed
            && !call.NormalReturnBoundaryReached
            && call.FlagsAfter == ParentPlan.FlagsAtStop
            && Outcome
                == (Capture.OpaqueCallReturnedNormally == false
                    ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .OpaqueCallDidNotReturnNormally
                    : NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .MissingOpaqueCallReturnStatus)
            && CompletedInstructionOffsets.SequenceEqual(targetPathOffsets)
            && StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryRawFileOffset
            && LastReachedRawFileOffset
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .LeaEBXRawFileOffset
            && FlagsAtStop == ParentPlan.FlagsAtStop;
    }

    private bool ReprojectedParentMatches(
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan replay) =>
        replay.Capture == ParentPlan.Capture
        && replay.Outcome == ParentPlan.Outcome
        && replay.EsiCaptureState == ParentPlan.EsiCaptureState
        && replay.DWORDAtA8CaptureState == ParentPlan.DWORDAtA8CaptureState
        && replay.DWORDAtA4CaptureState == ParentPlan.DWORDAtA4CaptureState
        && replay.Reads.SequenceEqual(ParentPlan.Reads)
        && replay.Instructions.SequenceEqual(ParentPlan.Instructions)
        && replay.CompletedInstructionOffsets.SequenceEqual(
            ParentPlan.CompletedInstructionOffsets)
        && replay.StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive
        && replay.ESI == ParentPlan.ESI
        && replay.EAX == ParentPlan.EAX
        && replay.EBX == ParentPlan.EBX
        && replay.FlagsAtStop == ParentPlan.FlagsAtStop
        && replay.BranchBoundaryReached == ParentPlan.BranchBoundaryReached
        && replay.ProjectionEntered == ParentPlan.ProjectionEntered;

    private uint? LastCompletedParentInstruction() =>
        ParentPlan.CompletedInstructionOffsets.IsEmpty
            ? null
            : ParentPlan.CompletedInstructionOffsets[^1];
}

/// <summary>
/// Reprojects the taken TEST/JZ path through the opaque call's normal return
/// boundary, stopping before the instruction at BBDE8C.
/// </summary>
public static class NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
{
    public const uint BranchRawFileOffset =
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.BranchRawFileOffset;
    public const uint FallThroughRawFileOffset =
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.BranchNextRawFileOffset;
    public const uint BranchTargetRawFileOffset =
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.BranchTargetRawFileOffset;
    public const uint LeaEBXRawFileOffset = 0x00BBDE83;
    public const uint CallBoundaryRawFileOffset = 0x00BBDE86;
    public const uint NormalReturnRawFileOffset = 0x00BBDE8C;
    public const uint NormalReturnInstructionBoundaryRawFileOffset =
        NormalReturnRawFileOffset;
    public const uint OpaqueCallTargetSlotVirtualAddress = 0x01AC4D24;
    public const uint OpaqueCallTargetSlotRawFileOffset = 0x016C3924;
    public const uint ESIToOpaqueCellDisplacement = 0x6C;
    public const uint OpaqueCellPointerVirtualAddress =
        OpaqueCallTargetSlotVirtualAddress;
    public const byte BranchInstructionByteLength =
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
            .BranchInstructionByteLength;
    public const int SignedRelativeDisplacement = 0x43;
    public const byte LeaEBXInstructionByteLength = 3;
    public const byte CallBoundaryInstructionByteLength = 6;
    public const byte NormalReturnInstructionByteLength = 3;
    public const string BranchInstructionBytes =
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
            .BranchInstructionBytes;
    public const string LeaEBXInstructionBytes = "8D5E6C";
    public const string CallBoundaryInstructionBytes = "FF15244DAC01";
    public const string NormalReturnInstructionBytes = "8945F0";
    public const string OpaqueCallTargetSlotImportName = "timeGetTime";
    public const string CoordinatorProvidedBytes = "8D5E6CFF15244DAC018945F0";
    public const bool BytesIndependentlyVerified = true;
    public const bool OpaqueCallTargetAddressResolved = false;

    public static uint BranchCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(BranchRawFileOffset);
    public static uint BranchTargetCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(BranchTargetRawFileOffset);
    public static uint LeaEBXCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(LeaEBXRawFileOffset);
    public static uint CallBoundaryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(CallBoundaryRawFileOffset);
    public static uint NormalReturnCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(NormalReturnRawFileOffset);
    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesOpaqueCellCall => false;

    public static NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan Project(
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture.ParentCapture);

        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parentPlan =
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.Project(
                capture.ParentCapture);
        bool parentContinuationReached =
            ParentAccepted(parentPlan);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState esiState =
            FromParentState(parentPlan.EsiCaptureState);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState dwordAtA8State =
            FromParentState(parentPlan.DWORDAtA8CaptureState);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState dwordAtA4State =
            FromParentState(parentPlan.DWORDAtA4CaptureState);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState branchConditionState =
            !parentContinuationReached
                ? ConditionUnusedOrUnreached(parentPlan)
                : NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState targetESIState =
            parentPlan.ESI.HasValue
                ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.SuppliedUnused
                : NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState normalReturnState =
            ReturnStatusUnusedOrUnreached(capture.OpaqueCallReturnedNormally);
        var completedOffsets =
            ImmutableArray.CreateBuilder<uint>();
        completedOffsets.AddRange(parentPlan.CompletedInstructionOffsets);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchJz? jz = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchLea? lea = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCallBoundary? callBoundary = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchOpaqueCallReturn? opaqueCallReturn =
            null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome outcome =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome.ParentPrefixNotAccepted;
        bool projectionEntered = false;
        bool suffixAdmitted = false;
        uint? stopAt = parentPlan.StopAtRawFileOffsetExclusive;
        uint? lastReached = LastCompletedParentInstruction(parentPlan);
        NetworkDispatcherLockGateFlags? flagsAtStop = parentPlan.FlagsAtStop;

        if (parentContinuationReached)
        {
            if (parentPlan.EAX is not uint eax
                || parentPlan.FlagsAtStop is not NetworkDispatcherLockGateFlags flags
                || flags.Zero is not bool zero)
            {
                outcome =
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .MissingBranchCondition;
                branchConditionState =
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing;
                stopAt = BranchRawFileOffset;
            }
            else if ((eax == 0) != zero)
            {
                outcome =
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .InconsistentBranchCondition;
                branchConditionState =
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing;
                stopAt = BranchRawFileOffset;
            }
            else
            {
                branchConditionState =
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured;
                bool taken = zero;
                uint selectedDestination = taken
                    ? BranchTargetRawFileOffset
                    : FallThroughRawFileOffset;
                jz = new(
                    BranchRawFileOffset,
                    BranchCodeVirtualAddress,
                    BranchInstructionByteLength,
                    BranchInstructionBytes,
                    SignedRelativeDisplacement,
                    BranchTargetCodeVirtualAddress,
                    BranchTargetRawFileOffset,
                    selectedDestination,
                    eax,
                    flags,
                    flags,
                    taken,
                    Attempted: true,
                    Reached: true,
                    Committed: true);
                completedOffsets.Add(BranchRawFileOffset);
                projectionEntered = true;
                stopAt = selectedDestination;
                lastReached = BranchRawFileOffset;

                if (!taken)
                {
                    outcome =
                        NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                            .JzNotTakenToBBDE40;
                }
                else
                {
                    if (parentPlan.ESI is uint esi)
                    {
                        uint ebx = unchecked(esi + ESIToOpaqueCellDisplacement);
                        lea = new(
                            LeaEBXRawFileOffset,
                            LeaEBXCodeVirtualAddress,
                            LeaEBXInstructionByteLength,
                            LeaEBXInstructionBytes,
                            esi,
                            parentPlan.EBX,
                            ebx,
                            flags,
                            flags,
                            Attempted: true,
                            Reached: true,
                            Committed: true,
                            FlagsWritten: false,
                            NativeMemoryReadExecuted: false,
                            NativeMemoryWriteExecuted: false);
                        callBoundary = new(
                            CallBoundaryRawFileOffset,
                            CallBoundaryCodeVirtualAddress,
                            CallBoundaryInstructionByteLength,
                            CallBoundaryInstructionBytes,
                            OpaqueCellPointerVirtualAddress,
                            Reached: true,
                            CallExecuted: false,
                            PointerReadExecuted: false,
                            CalleeContinuationIdentified: false);
                        completedOffsets.Add(LeaEBXRawFileOffset);
                        targetESIState =
                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
                                .Captured;
                        suffixAdmitted = true;
                        stopAt = CallBoundaryRawFileOffset;
                        lastReached = LeaEBXRawFileOffset;
                        normalReturnState =
                            capture.OpaqueCallReturnedNormally.HasValue
                                ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
                                    .Captured
                                : NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
                                    .Missing;
                        NetworkDispatcherLockGateFlags flagsAfterCall =
                            capture.OpaqueCallReturnedNormally == true
                                ? UnknownFlags
                                : flags;
                        opaqueCallReturn = new(
                            CallBoundaryRawFileOffset,
                            CallBoundaryCodeVirtualAddress,
                            CallBoundaryInstructionByteLength,
                            CallBoundaryInstructionBytes,
                            OpaqueCallTargetSlotVirtualAddress,
                            NormalReturnRawFileOffset,
                            NormalReturnCodeVirtualAddress,
                            NormalReturnInstructionByteLength,
                            NormalReturnInstructionBytes,
                            flags,
                            flagsAfterCall,
                            BoundaryReached: true,
                            Attempted: true,
                            ReturnedNormally: capture.OpaqueCallReturnedNormally,
                            Completed: capture.OpaqueCallReturnedNormally == true,
                            NormalReturnBoundaryReached:
                                capture.OpaqueCallReturnedNormally == true,
                            PointerReadExecuted: false,
                            TargetExecuted: false,
                            NextInstructionExecuted: false);

                        if (capture.OpaqueCallReturnedNormally == true)
                        {
                            completedOffsets.Add(CallBoundaryRawFileOffset);
                            outcome =
                                NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                                    .CompletedAtNormalReturnBoundaryBBDE8C;
                            stopAt = NormalReturnRawFileOffset;
                            lastReached = CallBoundaryRawFileOffset;
                            flagsAtStop = UnknownFlags;
                        }
                        else
                        {
                            outcome = capture.OpaqueCallReturnedNormally == false
                                ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                                    .OpaqueCallDidNotReturnNormally
                                : NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                                    .MissingOpaqueCallReturnStatus;
                        }
                    }
                    else
                    {
                        outcome =
                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                                .MissingBranchCondition;
                        branchConditionState =
                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
                                .Missing;
                        stopAt = LeaEBXRawFileOffset;
                    }
                }
            }
        }

        return new(
            capture,
            parentPlan,
            outcome,
            esiState,
            dwordAtA8State,
            dwordAtA4State,
            branchConditionState,
            targetESIState,
            normalReturnState,
            jz,
            lea,
            callBoundary,
            opaqueCallReturn,
            completedOffsets.ToImmutable(),
            parentContinuationReached,
            projectionEntered,
            suffixAdmitted,
            stopAt,
            lastReached,
            flagsAtStop);
    }

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
            .RawFileOffsetToCodeVirtualAddress(rawFileOffset);

    public static uint CalculateRelativeTargetCodeVirtualAddress(
        uint instructionCodeVirtualAddress,
        int signedRelativeDisplacement) =>
        unchecked(instructionCodeVirtualAddress
            + BranchInstructionByteLength
            + (uint)signedRelativeDisplacement);

    private static bool ParentAccepted(
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parent) =>
        parent.ExactBoundedCompletion
        && parent.ExactCleanCompletion
        && parent.Outcome
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome.BoundedBeforeJz
        && parent.ProjectionEntered
        && parent.BranchBoundaryReached
        && parent.StopAtRawFileOffsetExclusive == BranchRawFileOffset
        && !parent.ConditionalBranchProjected
        && !parent.Instructions.IsEmpty
        && parent.Instructions[^1] is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .TestEaxRawFileOffset,
            Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .TestEaxEax,
            Reached: true,
            Committed: true
        };

    private static NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        FromParentState(
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState state) =>
        state switch
        {
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Missing =>
                NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured =>
                NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.SuppliedUnused =>
                NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.SuppliedUnused,
            _ => NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached
        };

    private static NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        ConditionUnusedOrUnreached(
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parent) =>
        parent.EAX.HasValue && parent.FlagsAtStop?.Zero.HasValue == true
            ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached;

    internal static NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        ReturnStatusUnusedOrUnreached(bool? returnedNormally) =>
        returnedNormally.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached;

    internal static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);

    private static uint? LastCompletedParentInstruction(
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parent) =>
        parent.CompletedInstructionOffsets.IsEmpty
            ? null
            : parent.CompletedInstructionOffsets[^1];
}
