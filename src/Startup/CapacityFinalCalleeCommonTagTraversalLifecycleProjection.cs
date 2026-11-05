using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTagTraversalLifecycleCapture(
    uint? EntryEAX,
    uint? EntryEDI,
    uint? EntryEDX,
    uint? FreshEDXPlus4,
    uint? FreshChildDWORD,
    uint? LateFreshChildPlus4);

public enum CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
{
    ParentGateStopped,
    ParentEqualBypassesChildTo9403,
    ParentUnequalHandoffNotExact,
    MissingFreshChildDWORDAt93F1,
    ChildZeroTo945C,
    MissingLateFreshChildPlus4At93F9,
    ChildMaskedEqualsEntryEDIAt9403,
    ChildMaskedDiffersFromEntryEDITo93E4
}

public enum CapacityFinalCalleeCommonTagTraversalLifecycleReadPhase
{
    ParentGate,
    ChildIteration
}

public readonly record struct CapacityFinalCalleeCommonTagTraversalLifecycleRead(
    CapacityFinalCalleeCommonTagTraversalLifecycleReadPhase Phase,
    uint? RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value,
    bool ReadReached);

public enum CapacityFinalCalleeCommonTagTraversalLifecycleInstructionPhase
{
    ParentGate,
    ChildIteration
}

public readonly record struct CapacityFinalCalleeCommonTagTraversalLifecycleInstruction(
    CapacityFinalCalleeCommonTagTraversalLifecycleInstructionPhase Phase,
    uint RawFileOffset);

public sealed record CapacityFinalCalleeCommonTagTraversalLifecyclePlan(
    CapacityFinalCalleeCommonTagTraversalLifecycleCapture Capture,
    CapacityFinalCalleeCommonTagTraversalLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonMaskedTagEqualityGatePlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeCommonTraversalIterationPlan? ChildPlan { get; init; }
    public CapacityFinalCalleeCommonTraversalIterationCapture? ChildCapture { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonTagTraversalLifecycleRead> OrderedReads
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonTagTraversalLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonTagTraversalLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonTagTraversalLifecycleInstruction>.Empty;

    public CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public uint? ParentShiftedEBX => ParentPlan.ShiftResult;
    public uint? ParentDestinationRawFileOffset =>
        ParentPlan.JzSelectedDestinationRawFileOffset;
    public uint ParentStopAtRawFileOffsetExclusive =>
        ParentPlan.StopAtRawFileOffsetExclusive;
    public bool ParentEqual9403Selected =>
        ParentPlan.JzTaken == true
        && ParentPlan.JzSelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                .EqualDestinationRawFileOffset;
    public bool ParentEqual9403Entered => false;
    public bool ParentUnequal93E4Selected =>
        ParentPlan.JzTaken == false
        && ParentPlan.JzSelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                .FallThroughDestinationRawFileOffset;
    public bool ParentUnequalHandoffExact { get; init; }
    public bool Internal93E4HandoffEntered => ChildPlan?.EntryMoveReached == true;
    public uint? ChildEntryEBX => ChildCapture?.EntryEBX;
    public uint? ChildLateEntryEDX => ChildCapture?.LateEntryEDX;
    public uint? ChildEntryEDI => ChildCapture?.EntryEDI;
    public bool ChildEntryOperandsAreParentDerived =>
        ChildCapture is { } child
        && child.EntryEBX == ParentPlan.EBXValue
        && child.LateEntryEDX == Capture.EntryEDX
        && child.EntryEDI == Capture.EntryEDI;
    public bool FreshChildDWORDCaptureProvidedButUnused =>
        ChildPlan is null && Capture.FreshChildDWORD.HasValue;
    public bool LateFreshChildPlus4CaptureProvidedButUnused =>
        ChildPlan is null
            ? Capture.LateFreshChildPlus4.HasValue
            : ChildPlan.UnreachedLateFreshChildPlus4CaptureProvided;
    public CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? ParentFlagsAtStop =>
        ParentPlan.FlagsAtStop;
    public CapacityFinalCalleeCommonTraversalIterationFlags? ChildFlagsAtEntry =>
        ParentPlan.FlagsAtStop is { } flags
            ? new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow)
            : null;
    public bool ChildEntryMovePreservesIncomingFlags =>
        ChildPlan?.EntryMoveReached == true;
    public CapacityFinalCalleeCommonTraversalIterationFlags? ChildFlagsAfterEntryMove =>
        ChildEntryMovePreservesIncomingFlags ? ChildFlagsAtEntry : null;
    public CapacityFinalCalleeCommonTraversalIterationFlags? FlagsAtStop =>
        ChildPlan?.FlagsAtStop
        ?? (ParentPlan.FlagsAtStop is { } flags
            ? new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow)
            : null);
    public bool ChildZero945CSelected =>
        ChildPlan is { } child
        && child.ChildZeroBranchTaken == true
        && child.ChildZeroBranchDestinationRawFileOffset
            == CapacityFinalCalleeCommonTraversalIterationProjection
                .ZeroChildBranchTargetRawFileOffset;
    public bool ChildZero945CEntered => false;
    public bool ChildLoopBack93E4Selected =>
        ChildPlan is { } child
        && child.LoopBranchTaken == true
        && child.LoopBranchSelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonTraversalIterationProjection
                .LoopBranchTargetRawFileOffset;
    public bool ChildLoopBack93E4Entered => false;
    public bool ChildBoundary9403Entered => false;
    public bool ExactSingleIterationEvidence { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool TargetEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the masked-tag gate with at most one traversal iteration. Child EBX
/// and EDI and the child pointer are inherited from the parent gate; late memory
/// reads remain independently captured.
/// </summary>
public static class CapacityFinalCalleeCommonTagTraversalLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.EntryRawFileOffset;
    public const uint EqualBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.EqualDestinationRawFileOffset;
    public const uint ZeroChildBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTraversalIterationProjection.ZeroChildBranchTargetRawFileOffset;

    public static CapacityFinalCalleeCommonTagTraversalLifecyclePlan Project(
        CapacityFinalCalleeCommonTagTraversalLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan parent =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(new(
                capture.EntryEAX,
                capture.EntryEDI,
                capture.EntryEDX,
                capture.FreshEDXPlus4));
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonTagTraversalLifecycleRead>();
        foreach (CapacityFinalCalleeCommonMaskedTagEqualityGateReadAttempt read
            in parent.ReadAttempts)
        {
            reads.Add(new(
                CapacityFinalCalleeCommonTagTraversalLifecycleReadPhase.ParentGate,
                read.RawFileOffset,
                read.AbsoluteAddress,
                read.AbsoluteAddress.HasValue,
                read.ValueCaptured,
                read.Value,
                read.ReadReached));
        }

        var instructions =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonTagTraversalLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonMaskedTagEqualityGateEvent item
            in parent.OrderedEvents)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonTagTraversalLifecycleInstructionPhase.ParentGate,
                item.RawFileOffset));
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.EqualToBounded9403)
        {
            bool cleanEqual = parent.ExactCleanBoundedDestination
                && !capture.FreshChildDWORD.HasValue
                && !capture.LateFreshChildPlus4.HasValue;
            return Finish(
                capture,
                parent,
                child: null,
                childCapture: null,
                reads,
                instructions,
                CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ParentEqualBypassesChildTo9403,
                stopAt: parent.StopAtRawFileOffsetExclusive,
                parentUnequalHandoffExact: false,
                exactSingleIterationEvidence: false,
                exactCleanCompletion: cleanEqual);
        }

        if (!IsExactUnequalHandoff(parent))
        {
            return Finish(
                capture,
                parent,
                child: null,
                childCapture: null,
                reads,
                instructions,
                CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ParentGateStopped,
                stopAt: parent.StopAtRawFileOffsetExclusive,
                parentUnequalHandoffExact: false,
                exactSingleIterationEvidence: false,
                exactCleanCompletion: false);
        }

        var childCapture = new CapacityFinalCalleeCommonTraversalIterationCapture(
            EntryEBX: parent.EBXValue,
            LateEntryEDX: capture.EntryEDX,
            FreshChildDWORD: capture.FreshChildDWORD,
            LateFreshChildPlus4: capture.LateFreshChildPlus4,
            EntryEDI: capture.EntryEDI);
        CapacityFinalCalleeCommonTraversalIterationPlan child =
            CapacityFinalCalleeCommonTraversalIterationProjection.Project(childCapture);
        foreach (CapacityFinalCalleeCommonTraversalIterationRead read in child.Reads)
        {
            reads.Add(new(
                CapacityFinalCalleeCommonTagTraversalLifecycleReadPhase.ChildIteration,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value,
                ReadReached: true));
        }

        foreach (CapacityFinalCalleeCommonTraversalIterationEvent item in child.Events)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonTagTraversalLifecycleInstructionPhase.ChildIteration,
                item.RawFileOffset));
        }

        CapacityFinalCalleeCommonTagTraversalLifecycleOutcome outcome = child.Outcome switch
        {
            CapacityFinalCalleeCommonTraversalIterationOutcome.MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C =>
                CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ChildZeroTo945C,
            CapacityFinalCalleeCommonTraversalIterationOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonTraversalIterationOutcome
                .MaskedChildEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403,
            CapacityFinalCalleeCommonTraversalIterationOutcome
                .MaskedChildDiffersFromEntryEDITo93E4 =>
                CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4,
            _ => CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                .ParentUnequalHandoffNotExact
        };
        bool exactSingleIteration = IsExactChildIteration(child, outcome)
            && (outcome
                    != CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ChildZeroTo945C
                || !capture.LateFreshChildPlus4.HasValue);
        bool exactCleanCompletion = exactSingleIteration;
        return Finish(
            capture,
            parent,
            child,
            childCapture,
            reads,
            instructions,
            outcome,
            child.StopAtRawFileOffsetExclusive,
            parentUnequalHandoffExact: true,
            exactSingleIteration,
            exactCleanCompletion);
    }

    public static bool IsExactUnequalHandoff(
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan parent) =>
        parent.Outcome
            == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.UnequalToFallThrough93E4
        && parent.ExactCleanBoundedDestination
        && parent.JzReached
        && parent.JzTaken == false
        && parent.JzSelectedDestinationRawFileOffset
            == ChildEntryRawFileOffset
        && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
        && parent.CompareReached
        && parent.CompareFlags?.Zero == false
        && parent.EntryEAXOperandConsumed
        && parent.EntryEDIOperandConsumed
        && parent.EntryEDXOperandConsumed
        && parent.FreshEDXPlus4OperandConsumed
        && parent.ShiftExecuted
        && parent.MaskReached
        && parent.ShiftReached
        && parent.EBXValue == parent.ShiftResult
        && parent.EBXValue.HasValue
        && parent.FreshReadReached
        && parent.ReadAttempts is
            [{
                RawFileOffset: CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                    .FreshEDXPlus4MoveRawFileOffset,
                AbsoluteAddress: not null,
                ReadReached: true,
                ValueCaptured: true,
                Value: not null
            }]
        && !parent.EntryEAXCaptureProvidedButUnused
        && !parent.EntryEDICaptureProvidedButUnused
        && !parent.EntryEDXCaptureProvidedButUnused
        && !parent.FreshEDXPlus4CaptureProvidedButUnused;

    private static bool IsExactChildIteration(
        CapacityFinalCalleeCommonTraversalIterationPlan child,
        CapacityFinalCalleeCommonTagTraversalLifecycleOutcome outcome)
    {
        bool exactTerminal =
            outcome is CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ChildZeroTo945C
                or CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403
                or CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4;
        if (!exactTerminal)
        {
            return false;
        }

        if (outcome
            == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ChildZeroTo945C)
        {
            return child.EntryMoveReached
                && child.EntryEBXCaptured
                && child.ChildAddressLeaReached
                && child.DoubleEBXReached
                && child.ShiftEAXReached
                && child.AddFourReached
                && child.LateEntryEDXCaptured
                && child.FreshChildReadReached
                && child.FreshChildDWORDCaptured
                && child.FreshChildDWORD == 0
                && child.ChildZeroBranchReached
                && child.ChildZeroBranchTaken == true
                && child.ChildZeroBranchDestinationRawFileOffset
                    == ZeroChildBoundaryRawFileOffsetExclusive
                && !child.MoveChildToEDXReached
                && !child.FreshChildPlus4ReadReached
                && !child.CompareReached
                && !child.TargetEntered
                && !child.UnreachedLateEntryEDXCaptureProvided
                && !child.UnreachedLateFreshChildPlus4CaptureProvided
                && child.StopAtRawFileOffsetExclusive
                    == ZeroChildBoundaryRawFileOffsetExclusive
                && child.UnreachedFreshChildDWORDCaptureProvided == false;
        }

        return child.ExactSingleIterationEvidence
            && child.CompareReached
            && child.LoopBranchReached
            && child.LoopBranchSelectedDestinationRawFileOffset
                == child.StopAtRawFileOffsetExclusive
            && child.LoopBranchTaken
                == (outcome == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4)
            && !child.TargetEntered
            && child.StopAtRawFileOffsetExclusive is
                EqualBoundaryRawFileOffsetExclusive
                or ChildEntryRawFileOffset;
    }

    private static CapacityFinalCalleeCommonTagTraversalLifecyclePlan Finish(
        CapacityFinalCalleeCommonTagTraversalLifecycleCapture capture,
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan parent,
        CapacityFinalCalleeCommonTraversalIterationPlan? child,
        CapacityFinalCalleeCommonTraversalIterationCapture? childCapture,
        ImmutableArray<CapacityFinalCalleeCommonTagTraversalLifecycleRead>.Builder reads,
        ImmutableArray<CapacityFinalCalleeCommonTagTraversalLifecycleInstruction>.Builder
            instructions,
        CapacityFinalCalleeCommonTagTraversalLifecycleOutcome outcome,
        uint stopAt,
        bool parentUnequalHandoffExact,
        bool exactSingleIterationEvidence,
        bool exactCleanCompletion) =>
        new(capture, outcome)
        {
            ParentPlan = parent,
            ChildPlan = child,
            ChildCapture = childCapture,
            OrderedReads = reads.ToImmutable(),
            OrderedInstructions = instructions.ToImmutable(),
            ParentUnequalHandoffExact = parentUnequalHandoffExact,
            ExactSingleIterationEvidence = exactSingleIterationEvidence,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
