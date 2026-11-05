using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedTagTraversalLifecycleCapture(
    CapacityFinalCalleeCommonIndexedCountClassificationCapture Parent,
    uint? LateEntryEDI,
    uint? FreshEDXPlus4,
    uint? FreshChildDWORD,
    uint? LateFreshChildPlus4);

public enum CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
{
    ParentBlocked,
    ParentTagGateStopped,
    ParentEqualBypassesTraversalTo9403,
    ParentUnequalHandoffNotExact,
    MissingFreshChildDWORDAt93F1,
    ChildZeroTo945C,
    MissingLateFreshChildPlus4At93F9,
    ChildMaskedEqualsEntryEDIAt9403,
    ChildMaskedDiffersFromEntryEDITo93E4
}

public enum CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
{
    ParentIndexedDWORDAt93C0,
    ParentTagDWORDAt93D8,
    FreshChildDWORDAt93F1,
    FreshChildPlus4DWORDAt93F9
}

public readonly record struct CapacityFinalCalleeCommonIndexedTagTraversalLifecycleRead(
    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value,
    bool ReadReached);

public enum CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase
{
    IndexedCount,
    ParentTagGate,
    TraversalIteration
}

public readonly record struct CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstruction(
    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase Phase,
    uint RawFileOffset);

public sealed record CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan(
    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleCapture Capture,
    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeCommonTraversalIterationPlan? TraversalPlan { get; init; }
    public CapacityFinalCalleeCommonTraversalIterationCapture? TraversalCapture { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonIndexedTagTraversalLifecycleRead>
        OrderedReads { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonIndexedTagTraversalLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstruction>.Empty;

    public CapacityFinalCalleeCommonIndexedCountClassificationPlan CountPlan =>
        ParentPlan.ParentPlan;
    public CapacityFinalCalleeCommonMaskedTagEqualityGatePlan? TagPlan =>
        ParentPlan.ChildPlan;
    public CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome? TagOutcome =>
        TagPlan?.Outcome;
    public bool ExactCountTagHandoffCompleted => ParentPlan.ExactParentHandoffCompleted;
    public bool ParentEqual9403Selected =>
        TagPlan is
        {
            JzTaken: true,
            JzSelectedDestinationRawFileOffset:
                CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                    .EqualDestinationRawFileOffset
        };
    public bool ParentEqual9403Entered => false;
    public bool ParentUnequal93E4Selected =>
        TagPlan is
        {
            JzTaken: false,
            JzSelectedDestinationRawFileOffset:
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
                    .TraversalEntryRawFileOffset
        };
    public bool ParentUnequalHandoffExact { get; init; }
    public bool TraversalEntry93E4Entered => TraversalPlan?.EntryMoveReached == true;
    public uint? ParentDerivedCountEAX => ParentPlan.ChildEntryEAX;
    public uint? ParentDerivedTagEDX => ParentPlan.ChildEntryEDX;
    public uint? ParentDerivedTagEDI => ParentPlan.ChildEntryEDI;
    public uint? TraversalEntryEBX => TraversalCapture?.EntryEBX;
    public uint? TraversalLateEntryEDX => TraversalCapture?.LateEntryEDX;
    public uint? TraversalEntryEDI => TraversalCapture?.EntryEDI;
    public bool TraversalOperandsAreParentDerived =>
        TraversalCapture is { } child
        && child.EntryEBX == TagPlan?.EBXValue
        && child.LateEntryEDX == ParentPlan.ChildEntryEDX
        && child.EntryEDI == ParentPlan.ChildEntryEDI;
    public bool FreshChildDWORDCaptureProvidedButUnused =>
        TraversalPlan is null
            ? Capture.FreshChildDWORD.HasValue
            : TraversalPlan.UnreachedFreshChildDWORDCaptureProvided;
    public bool LateFreshChildPlus4CaptureProvidedButUnused =>
        TraversalPlan is null
            ? Capture.LateFreshChildPlus4.HasValue
            : TraversalPlan.UnreachedLateFreshChildPlus4CaptureProvided;
    public bool LateEntryEDICaptureProvidedButUnused =>
        ParentPlan.LateEntryEDICaptureProvidedButUnused;
    public bool TraversalInheritedEDIAlreadyConsumed =>
        ParentUnequalHandoffExact
        && ParentPlan.LateEntryEDICaptureAdmitted
        && ParentPlan.ChildPlan?.EntryEDIOperandConsumed == true;
    public CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? ParentFlagsAtStop =>
        ParentPlan.FlagsAtStop;
    public CapacityFinalCalleeCommonTraversalIterationFlags? TraversalFlagsAtEntry =>
        ParentPlan.FlagsAtStop is { } flags
            ? CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
                .ToTraversalFlags(flags)
            : null;
    public CapacityFinalCalleeCommonTraversalIterationFlags? TraversalFlagsAfterEntryMove =>
        TraversalPlan?.EntryMoveReached == true ? TraversalFlagsAtEntry : null;
    public CapacityFinalCalleeCommonTraversalIterationFlags? FlagsAtStop =>
        TraversalPlan?.FlagsAtStop
        ?? (ParentPlan.FlagsAtStop is { } flags
            ? CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
                .ToTraversalFlags(flags)
            : null);
    public bool ChildZero945CSelected =>
        TraversalPlan is { } child
        && child.ChildZeroBranchTaken == true
        && child.ChildZeroBranchDestinationRawFileOffset
            == CapacityFinalCalleeCommonTraversalIterationProjection
                .ZeroChildBranchTargetRawFileOffset;
    public bool ChildZero945CEntered => false;
    public bool ChildLoopBack93E4Selected =>
        TraversalPlan is { } child
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
/// Composes the 93C0 indexed-count and 93D4 tag gates with at most one 93E4
/// traversal iteration. Each fresh memory operand is observed only at its
/// original instruction; no selected edge or loop-back destination is entered.
/// </summary>
public static class CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedCountClassificationProjection.EntryRawFileOffset;
    public const uint ParentTagEntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.ChildEntryRawFileOffset;
    public const uint TraversalEntryRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.EntryRawFileOffset;
    public const uint EqualBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.EqualDestinationRawFileOffset;
    public const uint ZeroChildBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTraversalIterationProjection.ZeroChildBranchTargetRawFileOffset;

    public static CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan Project(
        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan parent =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(new(
                capture.Parent,
                capture.LateEntryEDI,
                capture.FreshEDXPlus4));
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonIndexedTagTraversalLifecycleRead>();
        foreach (CapacityFinalCalleeCommonIndexedCountTagLifecycleRead read
            in parent.OrderedReads)
        {
            reads.Add(new(
                read.Stage switch
                {
                    CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage
                        .ParentIndexedDWORDAt93C0 =>
                        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
                            .ParentIndexedDWORDAt93C0,
                    _ =>
                        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
                            .ParentTagDWORDAt93D8
                },
                read.RawFileOffset,
                read.AbsoluteAddress,
                read.AbsoluteAddress.HasValue,
                read.ValueCaptured,
                read.Value,
                read.ReadReached));
        }

        var instructions =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstruction>();
        AddCountInstructions(parent.ParentPlan, instructions);
        foreach (CapacityFinalCalleeCommonMaskedTagEqualityGateEvent item
            in parent.ChildOrderedEvents)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase
                    .ParentTagGate,
                item.RawFileOffset));
        }

        if (parent.Outcome == CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ParentBlocked)
        {
            return Finish(
                capture,
                parent,
                traversal: null,
                traversalCapture: null,
                reads,
                instructions,
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome.ParentBlocked,
                parent.StopAtRawFileOffsetExclusive,
                parentUnequalHandoffExact: false,
                exactSingleIterationEvidence: false,
                exactCleanCompletion: false);
        }

        if (parent.ChildPlan?.Outcome
            == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.EqualToBounded9403)
        {
            bool exactEqual = IsExactEqualBoundary(parent)
                && !capture.FreshChildDWORD.HasValue
                && !capture.LateFreshChildPlus4.HasValue;
            return Finish(
                capture,
                parent,
                traversal: null,
                traversalCapture: null,
                reads,
                instructions,
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ParentEqualBypassesTraversalTo9403,
                parent.StopAtRawFileOffsetExclusive,
                parentUnequalHandoffExact: false,
                exactSingleIterationEvidence: false,
                exactCleanCompletion: exactEqual);
        }

        if (!IsExactUnequalHandoff(parent))
        {
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome outcome =
                parent.ChildPlan?.Outcome
                    == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome
                        .UnequalToFallThrough93E4
                    ? CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                        .ParentUnequalHandoffNotExact
                    : CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                        .ParentTagGateStopped;
            uint stopAt = outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ParentUnequalHandoffNotExact
                ? TraversalEntryRawFileOffset
                : parent.StopAtRawFileOffsetExclusive;
            return Finish(
                capture,
                parent,
                traversal: null,
                traversalCapture: null,
                reads,
                instructions,
                outcome,
                stopAt,
                parentUnequalHandoffExact: false,
                exactSingleIterationEvidence: false,
                exactCleanCompletion: false);
        }

        var traversalCapture = new CapacityFinalCalleeCommonTraversalIterationCapture(
            EntryEBX: parent.ChildPlan!.EBXValue,
            LateEntryEDX: parent.ChildEntryEDX,
            FreshChildDWORD: capture.FreshChildDWORD,
            LateFreshChildPlus4: capture.LateFreshChildPlus4,
            EntryEDI: parent.ChildEntryEDI);
        CapacityFinalCalleeCommonTraversalIterationPlan traversal =
            CapacityFinalCalleeCommonTraversalIterationProjection.Project(traversalCapture);
        foreach (CapacityFinalCalleeCommonTraversalIterationRead read in traversal.Reads)
        {
            reads.Add(new(
                read.RawFileOffset
                    == CapacityFinalCalleeCommonTraversalIterationProjection
                        .FreshChildReadRawFileOffset
                    ? CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
                        .FreshChildDWORDAt93F1
                    : CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
                        .FreshChildPlus4DWORDAt93F9,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value,
                ReadReached: true));
        }

        foreach (CapacityFinalCalleeCommonTraversalIterationEvent item in traversal.Events)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase
                    .TraversalIteration,
                item.RawFileOffset));
        }

        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome childOutcome =
            traversal.Outcome switch
            {
                CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MissingFreshChildDWORDAt93F1 =>
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                        .MissingFreshChildDWORDAt93F1,
                CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C =>
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                        .ChildZeroTo945C,
                CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MissingLateFreshChildPlus4At93F9 =>
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                        .MissingLateFreshChildPlus4At93F9,
                CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildEqualsEntryEDIAt9403 =>
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                        .ChildMaskedEqualsEntryEDIAt9403,
                CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4 =>
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                        .ChildMaskedDiffersFromEntryEDITo93E4,
                _ =>
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                        .ParentUnequalHandoffNotExact
            };
        bool exactSingleIteration = IsExactTraversalIteration(traversal, childOutcome);
        return Finish(
            capture,
            parent,
            traversal,
            traversalCapture,
            reads,
            instructions,
            childOutcome,
            traversal.StopAtRawFileOffsetExclusive,
            parentUnequalHandoffExact: true,
            exactSingleIteration,
            exactCleanCompletion: exactSingleIteration);
    }

    private static bool IsExactEqualBoundary(
        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan parent) =>
        parent.ExactParentHandoffCompleted
        && parent.Outcome
            == CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildEqualToBounded9403
        && parent.ChildBoundedTargetNotEntered
        && parent.ChildPlan is
        {
            ExactCleanBoundedDestination: true,
            MaskReached: true,
            ShiftReached: true,
            ShiftExecuted: true,
            CompareReached: true,
            JzReached: true,
            JzTaken: true,
            StopAtRawFileOffsetExclusive: EqualBoundaryRawFileOffsetExclusive
        }
        && parent.ChildPlan.JzSelectedDestinationRawFileOffset
            == EqualBoundaryRawFileOffsetExclusive
        && !parent.ChildPlan.EntryEAXCaptureProvidedButUnused
        && !parent.ChildPlan.EntryEDICaptureProvidedButUnused
        && !parent.ChildPlan.EntryEDXCaptureProvidedButUnused
        && !parent.ChildPlan.FreshEDXPlus4CaptureProvidedButUnused;

    private static bool IsExactUnequalHandoff(
        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan? tag = parent.ChildPlan;
        if (!parent.ExactParentHandoffCompleted
            || parent.Outcome
                != CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome
                    .ChildUnequalToFallThrough93E4
            || !parent.ChildBoundedTargetNotEntered
            || tag is null
            || !tag.ExactCleanBoundedDestination
            || !tag.MaskReached
            || !tag.ShiftReached
            || !tag.ShiftExecuted
            || !tag.CompareReached
            || !tag.JzReached
            || tag.JzTaken != false
            || tag.StopAtRawFileOffsetExclusive != TraversalEntryRawFileOffset
            || tag.JzSelectedDestinationRawFileOffset != TraversalEntryRawFileOffset
            || tag.CompareFlags?.Zero != false
            || tag.ReadAttempts is not
            [{
                RawFileOffset: CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                    .FreshEDXPlus4MoveRawFileOffset,
                AbsoluteAddress: not null,
                ReadReached: true,
                ValueCaptured: true,
                Value: not null
            }]
            || tag.EBXValue != tag.ShiftResult
            || tag.EBXValue is null
            || parent.ChildEntryEDX != parent.ParentPlan.FreshEDX
            || parent.ChildEntryEDI != parent.Capture.LateEntryEDI
            || parent.ChildEntryEAX
                != (parent.ParentPlan.Outcome
                    == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
                    ? parent.ParentPlan.XorEAXResult
                    : parent.ParentPlan.SubtractResult)
            || !tag.EntryEAXOperandConsumed
            || !tag.EntryEDIOperandConsumed
            || !tag.EntryEDXOperandConsumed
            || !tag.FreshEDXPlus4OperandConsumed
            || tag.EntryEAXCaptureProvidedButUnused
            || tag.EntryEDICaptureProvidedButUnused
            || tag.EntryEDXCaptureProvidedButUnused
            || tag.FreshEDXPlus4CaptureProvidedButUnused)
        {
            return false;
        }

        return parent.OrderedReads is
        [
            {
                Stage: CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage
                    .ParentIndexedDWORDAt93C0,
                ReadReached: true,
                ValueCaptured: true,
                Value: not null
            },
            {
                Stage: CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage
                    .ChildTagDWORDAt93D8,
                RawFileOffset: CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                    .FreshEDXPlus4MoveRawFileOffset,
                ReadReached: true,
                AbsoluteAddress: not null,
                ValueCaptured: true,
                Value: not null
            }
        ];
    }

    private static bool IsExactTraversalIteration(
        CapacityFinalCalleeCommonTraversalIterationPlan child,
        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome outcome)
    {
        if (outcome == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                .ChildZeroTo945C)
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
                && !child.UnreachedFreshChildDWORDCaptureProvided;
        }

        bool terminal =
            outcome is CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                .ChildMaskedEqualsEntryEDIAt9403
                or CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                .ChildMaskedDiffersFromEntryEDITo93E4;
        return terminal
            && child.ExactSingleIterationEvidence
            && child.CompareReached
            && child.LoopBranchReached
            && child.LoopBranchSelectedDestinationRawFileOffset
                == child.StopAtRawFileOffsetExclusive
            && child.LoopBranchTaken
                == (outcome
                    == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                        .ChildMaskedDiffersFromEntryEDITo93E4)
            && !child.TargetEntered
            && child.StopAtRawFileOffsetExclusive is
                EqualBoundaryRawFileOffsetExclusive
                or TraversalEntryRawFileOffset;
    }

    private static void AddCountInstructions(
        CapacityFinalCalleeCommonIndexedCountClassificationPlan count,
        ImmutableArray<CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstruction>.Builder
            instructions)
    {
        if (count.IndexedDWORDReadAttempted)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase.IndexedCount,
                CapacityFinalCalleeCommonIndexedCountClassificationProjection.FreshLoadRawFileOffset));
        }

        if (count.CompareInstructionReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase.IndexedCount,
                CapacityFinalCalleeCommonIndexedCountClassificationProjection.CompareRawFileOffset));
        }

        if (count.JnzReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase.IndexedCount,
                CapacityFinalCalleeCommonIndexedCountClassificationProjection.JnzRawFileOffset));
        }

        if (count.XorEAXReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase.IndexedCount,
                CapacityFinalCalleeCommonIndexedCountClassificationProjection.XorRawFileOffset));
        }

        if (count.JumpReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase.IndexedCount,
                CapacityFinalCalleeCommonIndexedCountClassificationProjection.JumpRawFileOffset));
        }

        if (count.ShiftECXReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase.IndexedCount,
                CapacityFinalCalleeCommonIndexedCountClassificationProjection.ShiftRawFileOffset));
        }

        if (count.MoveImmediateEAXReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase.IndexedCount,
                CapacityFinalCalleeCommonIndexedCountClassificationProjection
                    .MoveImmediateRawFileOffset));
        }

        if (count.SubtractReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase.IndexedCount,
                CapacityFinalCalleeCommonIndexedCountClassificationProjection
                    .SubtractRawFileOffset));
        }
    }

    private static CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan Finish(
        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleCapture capture,
        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan parent,
        CapacityFinalCalleeCommonTraversalIterationPlan? traversal,
        CapacityFinalCalleeCommonTraversalIterationCapture? traversalCapture,
        ImmutableArray<CapacityFinalCalleeCommonIndexedTagTraversalLifecycleRead>.Builder reads,
        ImmutableArray<CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstruction>.Builder
            instructions,
        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome outcome,
        uint stopAt,
        bool parentUnequalHandoffExact,
        bool exactSingleIterationEvidence,
        bool exactCleanCompletion) =>
        new(capture, outcome)
        {
            ParentPlan = parent,
            TraversalPlan = traversal,
            TraversalCapture = traversalCapture,
            OrderedReads = reads.ToImmutable(),
            OrderedInstructions = instructions.ToImmutable(),
            ParentUnequalHandoffExact = parentUnequalHandoffExact,
            ExactSingleIterationEvidence = exactSingleIterationEvidence,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };

    internal static CapacityFinalCalleeCommonTraversalIterationFlags ToTraversalFlags(
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);
}
