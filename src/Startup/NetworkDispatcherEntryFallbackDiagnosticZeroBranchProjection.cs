using System.Collections.Immutable;
using System.Threading;

namespace Firefall.Reconstruction.Startup;

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchCapture(
    NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture ParentCapture,
    bool? OpaqueCallReturnedNormally = null,
    uint? FreshPostCallEAX = null,
    uint? FreshPostCallEBP = null,
    uint? FreshPostCallESI = null,
    uint? FreshFallthroughStackLoadEAX = null);

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
    MissingFreshPostCallEBP,
    MissingFreshPostCallEAX,
    MissingFreshPostCallESI,
    JzAtBBDE97TakenToBBDEA4,
    JzAtBBDE97NotTakenToBBDE99
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

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchStackStore(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    uint? EBPValue,
    uint? EAXValue,
    uint? StackAddress,
    uint? StoredValue,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter,
    bool Attempted,
    bool Reached,
    bool Captured,
    bool Committed,
    bool NativeMemoryWriteExecuted);

public enum NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination
{
    ECX,
    EAX
}

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaResult(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination DestinationRegister,
    uint? InputValue,
    uint? ResultValue,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    bool FlagsWritten);

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchCompare(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    uint EAXValue,
    uint ECXValue,
    uint SubtractionResult,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter,
    bool Attempted,
    bool Reached,
    bool Committed);

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchControlBoundary(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    int SignedRelativeDisplacement,
    uint NextInstructionRawFileOffset,
    uint BranchTargetRawFileOffset,
    uint BranchTargetCodeVirtualAddress,
    bool? ZeroCondition,
    bool? Taken,
    uint? SelectedDestinationRawFileOffset,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter,
    bool Reached,
    bool Attempted,
    bool BranchProjected);

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchDestinationBoundary(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    bool Reached,
    bool Attempted,
    bool InstructionExecuted);

public enum NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffectKind
{
    SFENCE,
    LFENCE,
    StackScalarLoad,
    FieldScalarStore
}

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffect(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffectKind Kind,
    uint? OperandAddress,
    uint? Value,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter,
    bool Reached,
    bool Attempted,
    bool Committed,
    bool NativeMemoryReadExecuted,
    bool NativeMemoryWriteExecuted);

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughContinuation(
    ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffect> Effects,
    ImmutableArray<uint> CompletedInstructionOffsets,
    uint StopAtRawFileOffsetExclusive,
    uint LastReachedRawFileOffset,
    NetworkDispatcherLockGateFlags FlagsAtStop,
    bool ExactBoundedCompletion);

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
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
            postOpaqueCallEAXCaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
            postOpaqueCallEBPCaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
            postOpaqueCallESICaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
            controlBoundaryConditionCaptureState,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchJz? jz,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchLea? addressCalculation,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCallBoundary? callBoundary,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchOpaqueCallReturn? opaqueCallReturn,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchStackStore? stackStore,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaResult? leaECX,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaResult? leaEAX,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCompare? compare,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchControlBoundary?
            nextControlBoundary,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchDestinationBoundary?
            destinationInstructionBoundary,
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughContinuation?
            fallthroughContinuation,
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
        PostOpaqueCallEAXCaptureState = postOpaqueCallEAXCaptureState;
        PostOpaqueCallEBPCaptureState = postOpaqueCallEBPCaptureState;
        PostOpaqueCallESICaptureState = postOpaqueCallESICaptureState;
        ControlBoundaryConditionCaptureState = controlBoundaryConditionCaptureState;
        Jz = jz;
        AddressCalculation = addressCalculation;
        CallBoundary = callBoundary;
        OpaqueCallReturn = opaqueCallReturn;
        StackStore = stackStore;
        LeaECX = leaECX;
        LeaEAX = leaEAX;
        Compare = compare;
        NextControlBoundary = nextControlBoundary;
        DestinationInstructionBoundary = destinationInstructionBoundary;
        FallthroughContinuation = fallthroughContinuation;
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
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        PostOpaqueCallEAXCaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        PostOpaqueCallEBPCaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        PostOpaqueCallESICaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        ControlBoundaryConditionCaptureState { get; }
    public uint? FreshPostCallEAX => Capture.FreshPostCallEAX;
    public uint? FreshPostCallEBP => Capture.FreshPostCallEBP;
    public uint? FreshPostCallESI => Capture.FreshPostCallESI;
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchJz? Jz { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchLea? AddressCalculation { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCallBoundary? CallBoundary { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchOpaqueCallReturn?
        OpaqueCallReturn { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchStackStore? StackStore { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaResult? LeaECX { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaResult? LeaEAX { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchCompare? Compare { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchControlBoundary?
        NextControlBoundary { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchDestinationBoundary?
        DestinationInstructionBoundary { get; }
    public NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughContinuation?
        FallthroughContinuation { get; }
    public bool StackStoreCommitted => StackStore is { Committed: true };
    public bool ImmediateSuffixCompleted =>
        NextControlBoundary is { Reached: true, Attempted: true, BranchProjected: true }
        && (NextControlBoundary.Value.Taken == true
            ? DestinationInstructionBoundary is { Reached: true, Attempted: false }
            : FallthroughContinuation is { ExactBoundedCompletion: true })
        && Compare is { Committed: true };
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
    public bool ControlBoundaryFlagsPreserved =>
        Compare is { } comparison
        && NextControlBoundary is { } branch
        && branch.FlagsBefore == comparison.FlagsAfter
        && branch.FlagsAfter == comparison.FlagsAfter
        && DestinationInstructionBoundary is { Reached: true, Attempted: false };
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
                && StackStore is null
                && LeaECX is null
                && LeaEAX is null
                && Compare is null
                && NextControlBoundary is null
                && DestinationInstructionBoundary is null
                && ControlBoundaryConditionCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached
                && NormalReturnCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.OpaqueCallReturnedNormally)
                && PostOpaqueCallEAXCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallEAX)
                && PostOpaqueCallEBPCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallEBP)
                && PostOpaqueCallESICaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallESI)
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
                && StackStore is null
                && LeaECX is null
                && LeaEAX is null
                && Compare is null
                && NextControlBoundary is null
                && DestinationInstructionBoundary is null
                && ControlBoundaryConditionCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached
                && NormalReturnCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.OpaqueCallReturnedNormally)
                && PostOpaqueCallEAXCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallEAX)
                && PostOpaqueCallEBPCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallEBP)
                && PostOpaqueCallESICaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallESI)
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
        expectedOffsets = expectedOffsets.Add(
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .StorePostCallEAXRawFileOffset)
            .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.LeaECXRawFileOffset)
            .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.LeaEAXRawFileOffset)
            .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.CompareRawFileOffset)
            .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .NextControlBoundaryRawFileOffset);
        if (Capture.FreshPostCallEBP is not uint postCallEBP
            || Capture.FreshPostCallESI is not uint postCallESI
            || Capture.FreshPostCallEAX is not uint postCallEAX)
        {
            return false;
        }

        uint expectedStackAddress = unchecked(postCallEBP
            - NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .StackStoreDisplacement);
        uint expectedECX = unchecked(postCallESI
            + NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .ESIToECXFieldDisplacement);
        NetworkDispatcherLockGateFlags expectedCompareFlags =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .ComputeCompareFlags(expectedStackAddress, expectedECX);
        bool expectedTaken = expectedCompareFlags.Zero == true;
        uint expectedDestination = expectedTaken
            ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .BranchTargetDestinationInstructionRawFileOffset
            : NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .FallThroughDestinationInstructionRawFileOffset;
        if (Outcome != (expectedTaken
                ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                    .JzAtBBDE97TakenToBBDEA4
                : NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                    .JzAtBBDE97NotTakenToBBDE99)
            || ControlBoundaryConditionCaptureState
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            || !SuffixAdmitted)
        {
            return false;
        }
        return SuffixAdmitted
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
            && PostOpaqueCallEAXCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && PostOpaqueCallEBPCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && PostOpaqueCallESICaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && StackStore is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .StorePostCallEAXRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .StorePostCallEAXInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .StorePostCallEAXInstructionBytes,
                Attempted: true,
                Reached: true,
                Captured: true,
                Committed: true,
                NativeMemoryWriteExecuted: false
            }
            && StackStore.Value.InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .StorePostCallEAXCodeVirtualAddress
            && StackStore.Value.EBPValue == postCallEBP
            && StackStore.Value.EAXValue == postCallEAX
            && StackStore.Value.StackAddress == expectedStackAddress
            && StackStore.Value.StoredValue == postCallEAX
            && StackStore.Value.FlagsBefore
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.UnknownFlags
            && StackStore.Value.FlagsAfter == StackStore.Value.FlagsBefore
            && LeaECX is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaECXRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaECXInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaECXInstructionBytes,
                DestinationRegister:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination.ECX,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsWritten: false
            }
            && LeaECX.Value.InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .LeaECXCodeVirtualAddress
            && LeaECX.Value.InputValue == postCallESI
            && LeaECX.Value.ResultValue == expectedECX
            && LeaECX.Value.FlagsBefore
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.UnknownFlags
            && LeaECX.Value.FlagsAfter == LeaECX.Value.FlagsBefore
            && LeaEAX is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaEAXRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaEAXInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaEAXInstructionBytes,
                DestinationRegister:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination.EAX,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsWritten: false
            }
            && LeaEAX.Value.InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .LeaEAXCodeVirtualAddress
            && LeaEAX.Value.InputValue == postCallEBP
            && LeaEAX.Value.ResultValue == expectedStackAddress
            && LeaEAX.Value.FlagsBefore == LeaECX.Value.FlagsAfter
            && LeaEAX.Value.FlagsAfter == LeaEAX.Value.FlagsBefore
            && Compare is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CompareRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CompareInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CompareInstructionBytes,
                Attempted: true,
                Reached: true,
                Committed: true
            }
            && Compare.Value.InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CompareCodeVirtualAddress
            && Compare.Value.EAXValue == expectedStackAddress
            && Compare.Value.ECXValue == expectedECX
            && Compare.Value.SubtractionResult
                == unchecked(expectedStackAddress - expectedECX)
            && Compare.Value.FlagsBefore == LeaEAX.Value.FlagsAfter
            && Compare.Value.FlagsAfter == expectedCompareFlags
            && NextControlBoundary is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .NextControlBoundaryRawFileOffset,
                InstructionByteLength:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .NextControlBoundaryInstructionByteLength,
                InstructionBytes:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .NextControlBoundaryInstructionBytes,
                SignedRelativeDisplacement:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .NextControlBoundaryRelativeDisplacement,
                NextInstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ControlBoundaryFallThroughRawFileOffset,
                BranchTargetRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ControlBoundaryTargetRawFileOffset,
                ZeroCondition: var zeroCondition,
                Taken: var taken,
                SelectedDestinationRawFileOffset: var selectedDestination,
                Reached: true,
                Attempted: true,
                BranchProjected: true
            }
            && NextControlBoundary.Value.InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NextControlBoundaryCodeVirtualAddress
            && NextControlBoundary.Value.BranchTargetCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .RawFileOffsetToCodeVirtualAddress(
                        NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                            .ControlBoundaryTargetRawFileOffset)
            && NextControlBoundary.Value.ZeroCondition == expectedCompareFlags.Zero
            && zeroCondition == expectedCompareFlags.Zero
            && taken == expectedTaken
            && selectedDestination == expectedDestination
            && NextControlBoundary.Value.FlagsBefore == expectedCompareFlags
            && NextControlBoundary.Value.FlagsAfter == expectedCompareFlags
            && DestinationInstructionBoundary is {
                Reached: true,
                Attempted: false,
                InstructionExecuted: false
            }
            && DestinationInstructionBoundary.Value.InstructionRawFileOffset
                == expectedDestination
            && DestinationInstructionBoundary.Value.InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .RawFileOffsetToCodeVirtualAddress(expectedDestination)
            && DestinationInstructionBoundary.Value.InstructionByteLength
                == (expectedTaken
                    ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .BranchTargetDestinationInstructionByteLength
                    : NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .FallThroughDestinationInstructionByteLength)
            && DestinationInstructionBoundary.Value.InstructionBytes
                == (expectedTaken
                    ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .BranchTargetDestinationInstructionBytes
                    : NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .FallThroughDestinationInstructionBytes)
            && CompletedInstructionOffsets.SequenceEqual(expectedOffsets)
            && StopAtRawFileOffsetExclusive
                == expectedDestination
            && LastReachedRawFileOffset
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NextControlBoundaryRawFileOffset
            && FlagsAtStop == expectedCompareFlags
            && ControlBoundaryFlagsPreserved;
    }

    private bool ComputeExactCleanCompletion() =>
        SuffixAdmitted
        && ParentPlan.ExactCleanCompletion
        && (Outcome
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                    .JzAtBBDE97TakenToBBDEA4
            || Outcome
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                    .JzAtBBDE97NotTakenToBBDE99)
        && ExactBoundedCompletion
        && BranchConditionCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && TargetESICaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && NormalReturnCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && PostOpaqueCallEAXCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && PostOpaqueCallEBPCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && PostOpaqueCallESICaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && OpaqueCallReturnedNormally == true
        && StackStoreCommitted
        && ImmediateSuffixCompleted
        && BranchConditionFlagsPreserved
        && ControlBoundaryConditionCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
        && ControlBoundaryFlagsPreserved
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
                && StackStore is null
                && LeaECX is null
                && LeaEAX is null
                && Compare is null
                && NextControlBoundary is null
                && NormalReturnCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.OpaqueCallReturnedNormally)
                && PostOpaqueCallEAXCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallEAX)
                && PostOpaqueCallEBPCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallEBP)
                && PostOpaqueCallESICaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallESI)
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
                && StackStore is null
                && LeaECX is null
                && LeaEAX is null
                && Compare is null
                && NextControlBoundary is null
                && NormalReturnCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.OpaqueCallReturnedNormally)
                && PostOpaqueCallEAXCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallEAX)
                && PostOpaqueCallEBPCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallEBP)
                && PostOpaqueCallESICaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallESI)
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
                && ComputeOrderedImmediateSuffix(targetPathOffsets.Add(
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryRawFileOffset));
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

    private bool ComputeOrderedImmediateSuffix(ImmutableArray<uint> callOffsets)
    {
        NetworkDispatcherLockGateFlags unknownFlags =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.UnknownFlags;
        if (StackStore is not { } store
            || store.InstructionRawFileOffset
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .StorePostCallEAXRawFileOffset
            || store.InstructionCodeVirtualAddress
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .StorePostCallEAXCodeVirtualAddress
            || store.InstructionByteLength
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .StorePostCallEAXInstructionByteLength
            || store.InstructionBytes
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .StorePostCallEAXInstructionBytes
            || !store.Attempted
            || !store.Reached
            || store.NativeMemoryWriteExecuted
            || store.FlagsBefore != unknownFlags
            || store.FlagsAfter != unknownFlags)
        {
            return false;
        }

        ImmutableArray<uint> beforeStoreOffsets = callOffsets;
        if (Capture.FreshPostCallEBP is not uint ebp)
        {
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .MissingFreshPostCallEBP
                && PostOpaqueCallEBPCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing
                && PostOpaqueCallEAXCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallEAX)
                && PostOpaqueCallESICaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallESI)
                && store.EBPValue is null
                && store.EAXValue is null
                && store.StackAddress is null
                && store.StoredValue is null
                && !store.Captured
                && !store.Committed
                && LeaECX is null
                && LeaEAX is null
                && Compare is null
                && NextControlBoundary is null
                && CompletedInstructionOffsets.SequenceEqual(beforeStoreOffsets)
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .StorePostCallEAXRawFileOffset
                && LastReachedRawFileOffset
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryRawFileOffset
                && FlagsAtStop == unknownFlags;
        }

        uint stackAddress = unchecked(ebp
            - NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .StackStoreDisplacement);
        if (Capture.FreshPostCallEAX is not uint postCallEAX)
        {
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .MissingFreshPostCallEAX
                && PostOpaqueCallEBPCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
                && PostOpaqueCallEAXCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing
                && PostOpaqueCallESICaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .ReturnStatusUnusedOrUnreached(Capture.FreshPostCallESI)
                && store.EBPValue == ebp
                && store.EAXValue is null
                && store.StackAddress == stackAddress
                && store.StoredValue is null
                && !store.Captured
                && !store.Committed
                && LeaECX is null
                && LeaEAX is null
                && Compare is null
                && NextControlBoundary is null
                && CompletedInstructionOffsets.SequenceEqual(beforeStoreOffsets)
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .StorePostCallEAXRawFileOffset
                && LastReachedRawFileOffset
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CallBoundaryRawFileOffset
                && FlagsAtStop == unknownFlags;
        }

        ImmutableArray<uint> afterStoreOffsets = beforeStoreOffsets.Add(
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .StorePostCallEAXRawFileOffset);
        if (!store.Captured
            || !store.Committed
            || store.EBPValue != ebp
            || store.EAXValue != postCallEAX
            || store.StackAddress != stackAddress
            || store.StoredValue != postCallEAX
            || PostOpaqueCallEBPCaptureState
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            || PostOpaqueCallEAXCaptureState
                != NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured)
        {
            return false;
        }

        if (Capture.FreshPostCallESI is not uint esi)
        {
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .MissingFreshPostCallESI
                && PostOpaqueCallESICaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing
                && LeaECX is {
                    DestinationRegister:
                        NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination.ECX,
                    Attempted: true,
                    Reached: true,
                    Committed: false,
                    InputValue: null,
                    ResultValue: null,
                    FlagsWritten: false
                }
                && LeaECX.Value.InstructionRawFileOffset
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaECXRawFileOffset
                && LeaECX.Value.FlagsBefore == unknownFlags
                && LeaECX.Value.FlagsAfter == unknownFlags
                && LeaEAX is null
                && Compare is null
                && NextControlBoundary is null
                && CompletedInstructionOffsets.SequenceEqual(afterStoreOffsets)
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaECXRawFileOffset
                && LastReachedRawFileOffset
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .StorePostCallEAXRawFileOffset
                && FlagsAtStop == unknownFlags;
        }

        uint ecxValue = unchecked(esi
            + NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .ESIToECXFieldDisplacement);
        return PostOpaqueCallESICaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && (Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .JzAtBBDE97TakenToBBDEA4
                || Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                        .JzAtBBDE97NotTakenToBBDE99)
            && ExactBoundedCompletion
            && LeaECX is {
                DestinationRegister:
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination.ECX,
                InputValue: var ecxInput,
                ResultValue: var ecxResult,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsWritten: false
            }
            && ecxInput == esi
            && ecxResult == ecxValue
            && LeaEAX is { Committed: true, DestinationRegister:
                NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination.EAX }
            && Compare is { Committed: true }
            && NextControlBoundary is { Reached: true, Attempted: true, BranchProjected: true }
            && DestinationInstructionBoundary is { Reached: true, Attempted: false }
            && CompletedInstructionOffsets.SequenceEqual(
                afterStoreOffsets
                    .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaECXRawFileOffset)
                    .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .LeaEAXRawFileOffset)
                    .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .CompareRawFileOffset)
                    .Add(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .NextControlBoundaryRawFileOffset))
            && StopAtRawFileOffsetExclusive
                == DestinationInstructionBoundary.Value.InstructionRawFileOffset
            && LastReachedRawFileOffset
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NextControlBoundaryRawFileOffset;
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
    public const uint StorePostCallEAXRawFileOffset = 0x00BBDE8C;
    public const uint LeaECXRawFileOffset = 0x00BBDE8F;
    public const uint LeaEAXRawFileOffset = 0x00BBDE92;
    public const uint CompareRawFileOffset = 0x00BBDE95;
    public const uint NextControlBoundaryRawFileOffset = 0x00BBDE97;
    public const uint ControlBoundaryFallThroughRawFileOffset = 0x00BBDE99;
    public const uint ControlBoundaryTargetRawFileOffset = 0x00BBDEA4;
    public const uint FallThroughDestinationInstructionRawFileOffset =
        ControlBoundaryFallThroughRawFileOffset;
    public const uint BranchTargetDestinationInstructionRawFileOffset =
        ControlBoundaryTargetRawFileOffset;
    public const uint FallthroughLFENCERawFileOffset = 0x00BBDE9C;
    public const uint FallthroughStackScalarLoadRawFileOffset = 0x00BBDE9F;
    public const uint FallthroughFieldScalarStoreRawFileOffset = 0x00BBDEA2;
    public const uint FallthroughJoinBoundaryRawFileOffset = 0x00BBDEA4;
    public const uint StackStoreDisplacement = 0x10;
    public const uint ESIToECXFieldDisplacement = 0x68;
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
    public const byte StorePostCallEAXInstructionByteLength = 3;
    public const byte LeaECXInstructionByteLength = 3;
    public const byte LeaEAXInstructionByteLength = 3;
    public const byte CompareInstructionByteLength = 2;
    public const byte NextControlBoundaryInstructionByteLength = 2;
    public const byte FallThroughDestinationInstructionByteLength = 3;
    public const byte BranchTargetDestinationInstructionByteLength = 2;
    public const byte FallthroughLFENCEInstructionByteLength = 3;
    public const byte FallthroughStackScalarLoadInstructionByteLength = 3;
    public const byte FallthroughFieldScalarStoreInstructionByteLength = 2;
    public const int NextControlBoundaryRelativeDisplacement = 0x0B;
    public const string BranchInstructionBytes =
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
            .BranchInstructionBytes;
    public const string LeaEBXInstructionBytes = "8D5E6C";
    public const string CallBoundaryInstructionBytes = "FF15244DAC01";
    public const string NormalReturnInstructionBytes = "8945F0";
    public const string StorePostCallEAXInstructionBytes = "8945F0";
    public const string LeaECXInstructionBytes = "8D4E68";
    public const string LeaEAXInstructionBytes = "8D45F0";
    public const string CompareInstructionBytes = "3BC1";
    public const string NextControlBoundaryInstructionBytes = "740B";
    public const string FallThroughDestinationInstructionBytes = "0FAEF8";
    public const string BranchTargetDestinationInstructionBytes = "8B01";
    public const string ControlBoundaryAndDestinationAnchorBytes =
        "740B0FAEF80FAEE88B45F089018B01";
    public const string FallthroughLFENCEInstructionBytes = "0FAEE8";
    public const string FallthroughStackScalarLoadInstructionBytes = "8B45F0";
    public const string FallthroughFieldScalarStoreInstructionBytes = "8901";
    public const string FallthroughWindowBytes = "0FAEF80FAEE88B45F08901";
    public const string OpaqueCallTargetSlotImportName = "timeGetTime";
    public const string ImmediateSuffixBytes = "8945F08D4E688D45F03BC1";
    public const string CoordinatorProvidedBytes =
        "8D5E6CFF15244DAC018945F08D4E688D45F03BC1740B";
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
    public static uint StorePostCallEAXCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(StorePostCallEAXRawFileOffset);
    public static uint LeaECXCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(LeaECXRawFileOffset);
    public static uint LeaEAXCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(LeaEAXRawFileOffset);
    public static uint CompareCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(CompareRawFileOffset);
    public static uint NextControlBoundaryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(NextControlBoundaryRawFileOffset);
    public static uint FallThroughDestinationInstructionCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(FallThroughDestinationInstructionRawFileOffset);
    public static uint BranchTargetDestinationInstructionCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(BranchTargetDestinationInstructionRawFileOffset);
    public static uint FallthroughLFENCECodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(FallthroughLFENCERawFileOffset);
    public static uint FallthroughStackScalarLoadCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(FallthroughStackScalarLoadRawFileOffset);
    public static uint FallthroughFieldScalarStoreCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(FallthroughFieldScalarStoreRawFileOffset);
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
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState postCallEAXState =
            ReturnStatusUnusedOrUnreached(capture.FreshPostCallEAX);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState postCallEBPState =
            ReturnStatusUnusedOrUnreached(capture.FreshPostCallEBP);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState postCallESIState =
            ReturnStatusUnusedOrUnreached(capture.FreshPostCallESI);
        var completedOffsets =
            ImmutableArray.CreateBuilder<uint>();
        completedOffsets.AddRange(parentPlan.CompletedInstructionOffsets);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchJz? jz = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchLea? lea = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCallBoundary? callBoundary = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchOpaqueCallReturn? opaqueCallReturn =
            null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchStackStore? stackStore = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaResult? leaECX = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaResult? leaEAX = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCompare? compare = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchControlBoundary?
            nextControlBoundary = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchDestinationBoundary?
            destinationInstructionBoundary = null;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
            controlBoundaryConditionCaptureState =
                NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached;
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughContinuation?
            fallthroughContinuation = null;
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
                            stopAt = NormalReturnRawFileOffset;
                            lastReached = CallBoundaryRawFileOffset;
                            flagsAtStop = UnknownFlags;

                            postCallEBPState = StateForCapturedValue(capture.FreshPostCallEBP);
                            if (capture.FreshPostCallEBP is not uint postCallEBP)
                            {
                                postCallEAXState =
                                    ReturnStatusUnusedOrUnreached(capture.FreshPostCallEAX);
                                postCallESIState =
                                    ReturnStatusUnusedOrUnreached(capture.FreshPostCallESI);
                                stackStore = new(
                                    StorePostCallEAXRawFileOffset,
                                    StorePostCallEAXCodeVirtualAddress,
                                    StorePostCallEAXInstructionByteLength,
                                    StorePostCallEAXInstructionBytes,
                                    null,
                                    null,
                                    null,
                                    null,
                                    UnknownFlags,
                                    UnknownFlags,
                                    Attempted: true,
                                    Reached: true,
                                    Captured: false,
                                    Committed: false,
                                    NativeMemoryWriteExecuted: false);
                                outcome =
                                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                                        .MissingFreshPostCallEBP;
                            }
                            else
                            {
                                uint stackAddress = unchecked(postCallEBP
                                    - StackStoreDisplacement);
                                if (capture.FreshPostCallEAX is not uint postCallEAX)
                                {
                                    postCallEAXState =
                                        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
                                            .Missing;
                                    postCallESIState =
                                        ReturnStatusUnusedOrUnreached(capture.FreshPostCallESI);
                                    stackStore = new(
                                        StorePostCallEAXRawFileOffset,
                                        StorePostCallEAXCodeVirtualAddress,
                                        StorePostCallEAXInstructionByteLength,
                                        StorePostCallEAXInstructionBytes,
                                        postCallEBP,
                                        null,
                                        stackAddress,
                                        null,
                                        UnknownFlags,
                                        UnknownFlags,
                                        Attempted: true,
                                        Reached: true,
                                        Captured: false,
                                        Committed: false,
                                        NativeMemoryWriteExecuted: false);
                                    outcome =
                                        NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                                            .MissingFreshPostCallEAX;
                                }
                                else
                                {
                                    postCallEAXState =
                                        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
                                            .Captured;
                                    stackStore = new(
                                        StorePostCallEAXRawFileOffset,
                                        StorePostCallEAXCodeVirtualAddress,
                                        StorePostCallEAXInstructionByteLength,
                                        StorePostCallEAXInstructionBytes,
                                        postCallEBP,
                                        postCallEAX,
                                        stackAddress,
                                        postCallEAX,
                                        UnknownFlags,
                                        UnknownFlags,
                                        Attempted: true,
                                        Reached: true,
                                        Captured: true,
                                        Committed: true,
                                        NativeMemoryWriteExecuted: false);
                                    completedOffsets.Add(StorePostCallEAXRawFileOffset);
                                    lastReached = StorePostCallEAXRawFileOffset;
                                    stopAt = LeaECXRawFileOffset;

                                    postCallESIState =
                                        StateForCapturedValue(capture.FreshPostCallESI);
                                    if (capture.FreshPostCallESI is not uint postCallESI)
                                    {
                                        leaECX = new(
                                            LeaECXRawFileOffset,
                                            LeaECXCodeVirtualAddress,
                                            LeaECXInstructionByteLength,
                                            LeaECXInstructionBytes,
                                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination
                                                .ECX,
                                            null,
                                            null,
                                            UnknownFlags,
                                            UnknownFlags,
                                            Attempted: true,
                                            Reached: true,
                                            Committed: false,
                                            FlagsWritten: false);
                                        outcome =
                                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                                                .MissingFreshPostCallESI;
                                    }
                                    else
                                    {
                                        uint ecxValue = unchecked(postCallESI
                                            + ESIToECXFieldDisplacement);
                                        postCallESIState =
                                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
                                                .Captured;
                                        leaECX = new(
                                            LeaECXRawFileOffset,
                                            LeaECXCodeVirtualAddress,
                                            LeaECXInstructionByteLength,
                                            LeaECXInstructionBytes,
                                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination
                                                .ECX,
                                            postCallESI,
                                            ecxValue,
                                            UnknownFlags,
                                            UnknownFlags,
                                            Attempted: true,
                                            Reached: true,
                                            Committed: true,
                                            FlagsWritten: false);
                                        completedOffsets.Add(LeaECXRawFileOffset);
                                        lastReached = LeaECXRawFileOffset;
                                        stopAt = LeaEAXRawFileOffset;

                                        leaEAX = new(
                                            LeaEAXRawFileOffset,
                                            LeaEAXCodeVirtualAddress,
                                            LeaEAXInstructionByteLength,
                                            LeaEAXInstructionBytes,
                                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchLeaDestination
                                                .EAX,
                                            postCallEBP,
                                            stackAddress,
                                            UnknownFlags,
                                            UnknownFlags,
                                            Attempted: true,
                                            Reached: true,
                                            Committed: true,
                                            FlagsWritten: false);
                                        completedOffsets.Add(LeaEAXRawFileOffset);
                                        lastReached = LeaEAXRawFileOffset;
                                        stopAt = CompareRawFileOffset;

                                        NetworkDispatcherLockGateFlags cmpFlags =
                                            ComputeCompareFlags(stackAddress, ecxValue);
                                        uint subtraction = unchecked(stackAddress - ecxValue);
                                        compare = new(
                                            CompareRawFileOffset,
                                            CompareCodeVirtualAddress,
                                            CompareInstructionByteLength,
                                            CompareInstructionBytes,
                                            stackAddress,
                                            ecxValue,
                                            subtraction,
                                            UnknownFlags,
                                            cmpFlags,
                                            Attempted: true,
                                            Reached: true,
                                            Committed: true);
                                        completedOffsets.Add(CompareRawFileOffset);
                                        lastReached = CompareRawFileOffset;
                                        nextControlBoundary = new(
                                            NextControlBoundaryRawFileOffset,
                                            NextControlBoundaryCodeVirtualAddress,
                                            NextControlBoundaryInstructionByteLength,
                                            NextControlBoundaryInstructionBytes,
                                            NextControlBoundaryRelativeDisplacement,
                                            ControlBoundaryFallThroughRawFileOffset,
                                            ControlBoundaryTargetRawFileOffset,
                                            RawFileOffsetToCodeVirtualAddress(
                                                ControlBoundaryTargetRawFileOffset),
                                            cmpFlags.Zero,
                                            cmpFlags.Zero,
                                            cmpFlags.Zero == true
                                                ? ControlBoundaryTargetRawFileOffset
                                                : ControlBoundaryFallThroughRawFileOffset,
                                            cmpFlags,
                                            cmpFlags,
                                            Reached: true,
                                            Attempted: true,
                                            BranchProjected: true);
                                        completedOffsets.Add(NextControlBoundaryRawFileOffset);
                                        lastReached = NextControlBoundaryRawFileOffset;
                                        controlBoundaryConditionCaptureState =
                                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
                                                .Captured;
                                        bool secondJzTaken = cmpFlags.Zero == true;
                                        uint selectedControlBoundaryDestination = secondJzTaken
                                            ? BranchTargetDestinationInstructionRawFileOffset
                                            : FallThroughDestinationInstructionRawFileOffset;
                                        destinationInstructionBoundary = new(
                                            selectedControlBoundaryDestination,
                                            RawFileOffsetToCodeVirtualAddress(
                                                selectedControlBoundaryDestination),
                                            secondJzTaken
                                                ? BranchTargetDestinationInstructionByteLength
                                                : FallThroughDestinationInstructionByteLength,
                                            secondJzTaken
                                                ? BranchTargetDestinationInstructionBytes
                                                : FallThroughDestinationInstructionBytes,
                                            Reached: true,
                                            Attempted: false,
                                            InstructionExecuted: false);
                                        outcome = secondJzTaken
                                            ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                                                .JzAtBBDE97TakenToBBDEA4
                                            : NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                                                .JzAtBBDE97NotTakenToBBDE99;
                                        stopAt = selectedControlBoundaryDestination;
                                        flagsAtStop = cmpFlags;
                                    }
                                }
                            }
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

        if (nextControlBoundary is { Taken: false }
            && compare is { } fallthroughCompare
            && stackStore is { Committed: true, StackAddress: uint savedStackAddress }
            && leaECX is { ResultValue: uint fieldAddress })
        {
            NetworkDispatcherLockGateFlags fallthroughFlags =
                fallthroughCompare.FlagsAfter;
            uint? reloadedValue = capture.FreshFallthroughStackLoadEAX;
            ImmutableArray<uint> fallthroughOffsets =
                completedOffsets.ToImmutable()
                    .Add(FallThroughDestinationInstructionRawFileOffset)
                    .Add(FallthroughLFENCERawFileOffset);
            ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffect>
                fallthroughEffects =
                    ImmutableArray.Create(
                        new NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffect(
                            FallThroughDestinationInstructionRawFileOffset,
                            FallThroughDestinationInstructionCodeVirtualAddress,
                            FallThroughDestinationInstructionByteLength,
                            FallThroughDestinationInstructionBytes,
                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffectKind
                                .SFENCE,
                            null,
                            null,
                            fallthroughFlags,
                            fallthroughFlags,
                            Reached: true,
                            Attempted: false,
                            Committed: false,
                            NativeMemoryReadExecuted: false,
                            NativeMemoryWriteExecuted: false),
                        new NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffect(
                            FallthroughLFENCERawFileOffset,
                            FallthroughLFENCECodeVirtualAddress,
                            FallthroughLFENCEInstructionByteLength,
                            FallthroughLFENCEInstructionBytes,
                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffectKind
                                .LFENCE,
                            null,
                            null,
                            fallthroughFlags,
                            fallthroughFlags,
                            Reached: true,
                            Attempted: false,
                            Committed: false,
                            NativeMemoryReadExecuted: false,
                            NativeMemoryWriteExecuted: false),
                        new NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffect(
                            FallthroughStackScalarLoadRawFileOffset,
                            FallthroughStackScalarLoadCodeVirtualAddress,
                            FallthroughStackScalarLoadInstructionByteLength,
                            FallthroughStackScalarLoadInstructionBytes,
                            NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffectKind
                                .StackScalarLoad,
                            savedStackAddress,
                            reloadedValue,
                            fallthroughFlags,
                            fallthroughFlags,
                            Reached: true,
                            Attempted: true,
                            Committed: reloadedValue.HasValue,
                            NativeMemoryReadExecuted: false,
                            NativeMemoryWriteExecuted: false));
            uint continuationStop;
            uint continuationLastReached;
            bool exactFallthroughCompletion;
            if (reloadedValue is uint loadedValue)
            {
                fallthroughEffects = fallthroughEffects.Add(
                    new NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffect(
                        FallthroughFieldScalarStoreRawFileOffset,
                        FallthroughFieldScalarStoreCodeVirtualAddress,
                        FallthroughFieldScalarStoreInstructionByteLength,
                        FallthroughFieldScalarStoreInstructionBytes,
                        NetworkDispatcherEntryFallbackDiagnosticZeroBranchFallthroughEffectKind
                            .FieldScalarStore,
                        fieldAddress,
                        loadedValue,
                        fallthroughFlags,
                        fallthroughFlags,
                        Reached: true,
                        Attempted: true,
                        Committed: true,
                        NativeMemoryReadExecuted: false,
                        NativeMemoryWriteExecuted: false));
                fallthroughOffsets = fallthroughOffsets
                    .Add(FallthroughStackScalarLoadRawFileOffset)
                    .Add(FallthroughFieldScalarStoreRawFileOffset);
                continuationStop = FallthroughJoinBoundaryRawFileOffset;
                continuationLastReached = FallthroughFieldScalarStoreRawFileOffset;
                exactFallthroughCompletion = true;
            }
            else
            {
                continuationStop = FallthroughStackScalarLoadRawFileOffset;
                continuationLastReached = FallthroughLFENCERawFileOffset;
                exactFallthroughCompletion = false;
            }

            fallthroughContinuation = new(
                fallthroughEffects,
                fallthroughOffsets,
                continuationStop,
                continuationLastReached,
                fallthroughFlags,
                ExactBoundedCompletion: exactFallthroughCompletion);
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
            postCallEAXState,
            postCallEBPState,
            postCallESIState,
            controlBoundaryConditionCaptureState,
            jz,
            lea,
            callBoundary,
            opaqueCallReturn,
            stackStore,
            leaECX,
            leaEAX,
            compare,
            nextControlBoundary,
            destinationInstructionBoundary,
            fallthroughContinuation,
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

    internal static NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        ReturnStatusUnusedOrUnreached(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState
        StateForCapturedValue(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            : NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing;

    internal static NetworkDispatcherLockGateFlags ComputeCompareFlags(uint left, uint right)
    {
        uint result = unchecked(left - right);
        bool evenParity =
            (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0;
        return new(
            Carry: left < right,
            Parity: evenParity,
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    internal static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);

    private static uint? LastCompletedParentInstruction(
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parent) =>
        parent.CompletedInstructionOffsets.IsEmpty
            ? null
            : parent.CompletedInstructionOffsets[^1];
}
