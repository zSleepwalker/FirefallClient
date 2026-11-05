using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonDualThresholdRingLifecycleCapture(
    uint? FreshGlobalValue,
    uint? ParentEntryEDX,
    uint? FreshEDXPlus8,
    uint? LateEntryESI);

public enum CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
{
    MissingFreshGlobalValue,
    MissingParentEntryEDX,
    MissingFreshEDXPlus8,
    FirstUnsignedBelowBypassesChild,
    SecondUnsignedBelowBypassesChild,
    ParentHandoffNotExact,
    MissingLateEntryESIForFirstWrite,
    ChildHandoffNotExact,
    CompletedToBoundaryBeforeDec
}

public enum CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffectKind
{
    FreshRead,
    ChildWriteAttempt
}

public readonly record struct CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffect(
    CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Attempted,
    bool Committed);

public readonly record struct CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Committed)
{
    public bool Attempted => true;
}

public sealed record CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan(
    CapacityFinalCalleeCommonDualThresholdRingLifecycleCapture Capture,
    CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonDualPointerThresholdPlan ParentPlan { get; init; } = null!;
    public CapacityFinalCalleeCommonRingInsertionPlan? ChildPlan { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffect>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite> Writes
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite>.Empty;

    public CapacityFinalCalleeCommonDualPointerThresholdOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public bool? ParentFirstJBTaken => ParentPlan.FirstJBTaken;
    public bool? ParentSecondJBTaken => ParentPlan.SecondJBTaken;
    public uint? ParentDestinationRawFileOffset =>
        ParentPlan.SecondJBSelectedDestinationRawFileOffset
        ?? ParentPlan.FirstJBSelectedDestinationRawFileOffset;
    public uint ParentStopAtRawFileOffsetExclusive =>
        ParentPlan.StopAtRawFileOffsetExclusive;
    public bool ParentExactFallthroughHandoff =>
        CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection
            .IsExactFallthroughHandoff(ParentPlan);
    public uint? ChildEntryEAX => ChildPlan?.Capture.EntryEAX;
    public uint? ChildEntryESI => ChildPlan?.Capture.EntryESI;
    public uint? ChildLateEntryEDX =>
        ChildPlan is { LateEntryEDXReadReached: true } child
            ? child.Capture.LateEntryEDX
            : null;
    public bool ChildLateEntryEDXReadReached =>
        ChildPlan?.LateEntryEDXReadReached == true;
    public bool ParentFallthroughInstructionEntered => ChildPlan is not null;
    public bool LateEntryESIProvidedButUnused =>
        ChildPlan is null && Capture.LateEntryESI.HasValue;
    public bool ExplicitDirtyExit =>
        (Outcome
            is CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .FirstUnsignedBelowBypassesChild
                or CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .SecondUnsignedBelowBypassesChild)
        && LateEntryESIProvidedButUnused;
    public CapacityFinalCalleeCommonDualPointerThresholdFlags? FlagsAtStop =>
        ParentPlan.FlagsAtStop;
    public bool ChildStoresPreserveParentFlags =>
        ChildPlan is null || ChildPlan.MovInstructionsPreserveFlags;
    public bool ChildBoundaryReached => ChildPlan?.BoundaryReached == true;
    public bool ChildBoundaryInstructionEntered =>
        ChildPlan?.BoundaryInstructionEntered == true;
    public uint ChildBoundaryRawFileOffsetExclusive =>
        CapacityFinalCalleeCommonRingInsertionProjection.BoundaryRawFileOffsetExclusive;
    public bool DecAtBoundaryEntered => false;
    public bool ExactCleanCompletion { get; init; }
    public bool DestinationEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the exact 941C dual-threshold fallthrough with the five ring stores,
/// deriving child EAX from the fresh EDX+8 read and deferring parent EDX use to store two.
/// </summary>
public static class CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonDualPointerThresholdProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonRingInsertionProjection.EntryRawFileOffset;
    public const uint CompletionRawFileOffset =
        CapacityFinalCalleeCommonRingInsertionProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan Project(
        CapacityFinalCalleeCommonDualThresholdRingLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonDualPointerThresholdPlan parent =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                capture.FreshGlobalValue,
                capture.ParentEntryEDX,
                capture.FreshEDXPlus8));
        ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffect>.Builder
            effects = ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffect>();

        foreach (CapacityFinalCalleeCommonDualPointerThresholdRead read in parent.Reads)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffectKind.FreshRead,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.Value,
                read.ValueCaptured,
                Attempted: true,
                Committed: false));
        }

        if (!IsExactFallthroughHandoff(parent))
        {
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome parentOutcome =
                parent.Outcome switch
                {
                    CapacityFinalCalleeCommonDualPointerThresholdOutcome.MissingFreshGlobalValue =>
                        CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                            .MissingFreshGlobalValue,
                    CapacityFinalCalleeCommonDualPointerThresholdOutcome.MissingEntryEDX =>
                        CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                            .MissingParentEntryEDX,
                    CapacityFinalCalleeCommonDualPointerThresholdOutcome.MissingFreshEDXPlus8 =>
                        CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                            .MissingFreshEDXPlus8,
                    CapacityFinalCalleeCommonDualPointerThresholdOutcome
                        .FirstUnsignedBelowBranchTarget =>
                        CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                            .FirstUnsignedBelowBypassesChild,
                    CapacityFinalCalleeCommonDualPointerThresholdOutcome
                        .SecondUnsignedBelowBranchTarget =>
                        CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                            .SecondUnsignedBelowBypassesChild,
                    _ => CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                        .ParentHandoffNotExact
                };
            return Finish(
                capture,
                parent,
                child: null,
                parentOutcome,
                effects,
                ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite>.Empty,
                stopAt: parent.StopAtRawFileOffsetExclusive,
                exactCleanCompletion: false);
        }

        CapacityFinalCalleeCommonRingInsertionPlan child =
            CapacityFinalCalleeCommonRingInsertionProjection.Project(new(
                EntryEAX: parent.FreshEDXPlus8,
                EntryESI: capture.LateEntryESI,
                LateEntryEDX: capture.ParentEntryEDX));
        ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite> writes =
            child.Writes
                .Select(write => new CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite(
                    write.RawFileOffset,
                    write.Address,
                    write.AddressCaptured,
                    write.Value,
                    write.ValueCaptured,
                    write.Committed))
                .ToImmutableArray();
        foreach (CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite write in writes)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffectKind
                    .ChildWriteAttempt,
                write.RawFileOffset,
                write.Address,
                write.AddressCaptured,
                write.Value,
                write.ValueCaptured,
                write.Attempted,
                write.Committed));
        }

        CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome outcome =
            child.Outcome switch
            {
                CapacityFinalCalleeCommonRingInsertionOutcome.MissingEntryESIForFirstWrite =>
                    CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                        .MissingLateEntryESIForFirstWrite,
                CapacityFinalCalleeCommonRingInsertionOutcome.CompletedToBoundaryBeforeDec =>
                    CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                        .CompletedToBoundaryBeforeDec,
                _ => CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .ChildHandoffNotExact
            };
        bool exactCleanCompletion =
            outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec
            && IsExactFallthroughHandoff(parent)
            && child.Outcome
                == CapacityFinalCalleeCommonRingInsertionOutcome
                    .CompletedToBoundaryBeforeDec
            && child.Writes.Length == 5
            && child.Writes.All(write => write.Committed)
            && child.StopAtRawFileOffsetExclusive == CompletionRawFileOffset
            && child.BoundaryReached
            && !child.BoundaryInstructionEntered
            && !child.DestinationEntered
            && child.LateEntryEDXReadReached
            && !child.UnreachedLateEntryEDXCaptureProvided
            && child.MovInstructionsPreserveFlags;

        return Finish(
            capture,
            parent,
            child,
            outcome,
            effects,
            writes,
            child.StopAtRawFileOffsetExclusive,
            exactCleanCompletion);
    }

    public static bool IsExactFallthroughHandoff(
        CapacityFinalCalleeCommonDualPointerThresholdPlan parent) =>
        parent.ExactCleanAdmission
        && parent.Outcome
            == CapacityFinalCalleeCommonDualPointerThresholdOutcome
                .BothUnsignedNotBelowFallthrough
        && parent.FirstJBTaken == false
        && parent.SecondJBTaken == false
        && parent.FirstCompareReached
        && parent.FirstJBReached
        && parent.SecondCompareReached
        && parent.SecondJBReached
        && parent.FreshGlobalReadReached
        && parent.FreshEDXPlus8ReadReached
        && !parent.TargetEntered
        && !parent.UnreachedFreshGlobalCaptureProvided
        && !parent.UnreachedEntryEDXCaptureProvided
        && !parent.UnreachedFreshEDXPlus8CaptureProvided
        && parent.FirstJBSelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonDualPointerThresholdProjection
                .SecondCompareRawFileOffset
        && parent.SecondJBSelectedDestinationRawFileOffset == ChildEntryRawFileOffset
        && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
        && parent.FreshGlobalCaptured
        && parent.EntryEDXCaptured
        && parent.FreshEDXPlus8Captured;

    private static CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan Finish(
        CapacityFinalCalleeCommonDualThresholdRingLifecycleCapture capture,
        CapacityFinalCalleeCommonDualPointerThresholdPlan parent,
        CapacityFinalCalleeCommonRingInsertionPlan? child,
        CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome outcome,
        ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffect>.Builder
            effects,
        ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite> writes,
        uint stopAt,
        bool exactCleanCompletion) =>
        new(capture, outcome)
        {
            ParentPlan = parent,
            ChildPlan = child,
            OrderedMemoryEffects = effects.ToImmutable(),
            Writes = writes,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
