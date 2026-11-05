using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedIndexRejectLifecycleCapture(
    CapacityFinalCalleeLargeSavedIndexGateCapture SavedIndexGateCapture,
    uint? ChildEntryECX);

public enum CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
{
    ParentGateIncomplete,
    ParentEqualTo9243,
    ParentEqualTo9243WithUnusedChildECX,
    MissingChildEntryECX,
    ChildUnsignedBelowToTarget,
    ChildUnsignedNotBelowToFallthrough
}

public enum CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildECXSource
{
    None,
    ExplicitIndependentCapture
}

public enum CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildEBXSource
{
    None,
    ParentEBXAfterFreshGlobalReload
}

public readonly record struct CapacityFinalCalleeLargeSavedIndexRejectLifecycleFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public sealed record CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan(
    CapacityFinalCalleeLargeSavedIndexRejectLifecycleCapture Capture,
    CapacityFinalCalleeLargeSavedIndexGatePlan SavedIndexGate,
    CapacityFinalCalleeLargeIndexedRejectGatePlan? IndexedRejectGate,
    CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome Outcome)
{
    public bool ExactCleanHandoffToRejectGate { get; init; }
    public bool RejectGateReached => IndexedRejectGate is not null;
    public CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildECXSource
        ChildEntryECXSource { get; init; }
    public uint? ChildEntryECX { get; init; }
    public CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildEBXSource
        ChildEntryEBXSource { get; init; }
    public uint? ChildEntryEBX { get; init; }
    public bool UnreachedChildEntryECXCaptureProvided { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedIndexGateRead> OrderedReads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeSavedIndexGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedRejectGateEvent> ChildEvents =>
        IndexedRejectGate?.Events
        ?? ImmutableArray<CapacityFinalCalleeLargeIndexedRejectGateEvent>.Empty;
    public CapacityFinalCalleeLargeSavedIndexRejectLifecycleFlags? FlagsAtStop =>
        IndexedRejectGate?.CompareFlags is { } childFlags
            ? ConvertFlags(childFlags)
            : ConvertFlags(SavedIndexGate.CompareFlags);
    public bool ChildMemoryReadsReached => false;
    public bool MemoryWritesReached => false;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeLargeSavedIndexRejectLifecycleFlags ConvertFlags(
        CapacityFinalCalleeLargeIndexedRejectGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeLargeSavedIndexRejectLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeLargeSavedIndexGateFlags? flags) =>
        flags is CapacityFinalCalleeLargeSavedIndexGateFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;
}

/// <summary>
/// Joins only the exact unequal saved-index edge at 9263 to its register CMP/JB gate.
/// </summary>
public static class CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection
{
    public const uint RejectGateEntryRawFileOffset =
        CapacityFinalCalleeLargeIndexedRejectGateProjection.EntryRawFileOffset;
    public const uint ParentEqualDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ChildBelowDestinationRawFileOffset =
        CapacityFinalCalleeLargeIndexedRejectGateProjection.BranchTargetRawFileOffset;
    public const uint ChildNotBelowDestinationRawFileOffset =
        CapacityFinalCalleeLargeIndexedRejectGateProjection.FallthroughRawFileOffset;

    public static CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan Project(
        CapacityFinalCalleeLargeSavedIndexRejectLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeSavedIndexGatePlan parent =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(
                capture.SavedIndexGateCapture);

        ImmutableArray<uint> ParentInstructions() => parent.ReachedInstructions;

        bool childCaptureProvided = capture.ChildEntryECX.HasValue;
        if (!IsExactParentUnequalExit(parent))
        {
            bool exactParentEqualExit = IsExactParentEqualExit(parent);
            return new(
                capture,
                parent,
                null,
                exactParentEqualExit
                    ? childCaptureProvided
                        ? CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                            .ParentEqualTo9243WithUnusedChildECX
                        : CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                            .ParentEqualTo9243
                    : CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                        .ParentGateIncomplete)
            {
                UnreachedChildEntryECXCaptureProvided = childCaptureProvided,
                ReachedInstructions = ParentInstructions(),
                OrderedReads = parent.Reads,
                ExactCleanExitAdmitted = exactParentEqualExit && !childCaptureProvided,
                StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
            };
        }

        uint childEntryEBX = parent.EBXAfterFreshGlobalReload!.Value;
        CapacityFinalCalleeLargeIndexedRejectGatePlan child =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: capture.ChildEntryECX,
                EntryEBX: childEntryEBX));

        return new(
            capture,
            parent,
            child,
            MapChildOutcome(child.Outcome))
        {
            ExactCleanHandoffToRejectGate = true,
            ChildEntryECXSource = capture.ChildEntryECX.HasValue
                ? CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildECXSource
                    .ExplicitIndependentCapture
                : CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildECXSource.None,
            ChildEntryECX = child.EntryECXCaptured ? capture.ChildEntryECX : null,
            ChildEntryEBXSource = child.EntryEBXCaptured
                ? CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildEBXSource
                    .ParentEBXAfterFreshGlobalReload
                : CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildEBXSource.None,
            ChildEntryEBX = child.EntryEBXCaptured ? childEntryEBX : null,
            ReachedInstructions =
                parent.ReachedInstructions.AddRange(child.ReachedInstructions),
            OrderedReads = parent.Reads,
            ExactCleanExitAdmitted =
                IsExactChildExit(child) && parent.ExactCleanCompletion,
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactParentEqualExit(
        CapacityFinalCalleeLargeSavedIndexGatePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeLargeSavedIndexGateOutcome.EqualToFallthrough9243
        && plan.ExactCleanCompletion
        && plan.EntryEDXCaptured
        && plan.FreshEDXPlus1CReadReached
        && plan.FreshEDXPlus1CCaptured
        && plan.CompareReached
        && plan.FreshIndexedDwordCaptured
        && plan.FreshGlobalReadReached
        && plan.FreshGlobalValueCaptured
        && plan.JnzReached
        && plan.JnzTaken == false
        && plan.JnzSelectedDestinationRawFileOffset
            == ParentEqualDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == ParentEqualDestinationRawFileOffset
        && !plan.UnreachedFreshIndexedDwordCaptureProvided
        && !plan.UnreachedFreshGlobalValueCaptureProvided;

    private static bool IsExactParentUnequalExit(
        CapacityFinalCalleeLargeSavedIndexGatePlan plan) =>
        plan.Outcome == CapacityFinalCalleeLargeSavedIndexGateOutcome.JnzTo9263
        && plan.ExactCleanCompletion
        && plan.EntryEDXCaptured
        && plan.FreshEDXPlus1CReadReached
        && plan.FreshEDXPlus1CCaptured
        && plan.CompareReached
        && plan.FreshIndexedDwordCaptured
        && plan.FreshGlobalReadReached
        && plan.FreshGlobalValueCaptured
        && plan.EBXAfterFreshGlobalReload.HasValue
        && plan.JnzReached
        && plan.JnzTaken == true
        && plan.JnzSelectedDestinationRawFileOffset
            == RejectGateEntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == RejectGateEntryRawFileOffset
        && !plan.UnreachedFreshIndexedDwordCaptureProvided
        && !plan.UnreachedFreshGlobalValueCaptureProvided;

    private static bool IsExactChildExit(
        CapacityFinalCalleeLargeIndexedRejectGatePlan plan) =>
        plan.ExactCleanExitAdmitted
        && plan.CompareReached
        && ((plan.Outcome == CapacityFinalCalleeLargeIndexedRejectGateOutcome.UnsignedBelowToTarget
                && plan.BranchTaken == true)
            || (plan.Outcome == CapacityFinalCalleeLargeIndexedRejectGateOutcome.UnsignedNotBelowToFallthrough
                && plan.BranchTaken == false))
        && plan.BranchReached
        && plan.BranchSelectedDestinationRawFileOffset
            == (plan.BranchTaken == true
                ? ChildBelowDestinationRawFileOffset
                : ChildNotBelowDestinationRawFileOffset)
        && plan.StopAtRawFileOffsetExclusive
            == plan.BranchSelectedDestinationRawFileOffset;

    private static CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
        MapChildOutcome(CapacityFinalCalleeLargeIndexedRejectGateOutcome outcome) =>
            outcome switch
            {
                CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryECX =>
                    CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                        .MissingChildEntryECX,
                CapacityFinalCalleeLargeIndexedRejectGateOutcome.UnsignedBelowToTarget =>
                    CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                        .ChildUnsignedBelowToTarget,
                CapacityFinalCalleeLargeIndexedRejectGateOutcome
                    .UnsignedNotBelowToFallthrough =>
                    CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                        .ChildUnsignedNotBelowToFallthrough,
                CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryEBX
                    or CapacityFinalCalleeLargeIndexedRejectGateOutcome
                        .MissingEntryECXAndEBX =>
                    throw new InvalidOperationException(
                        "The child EBX is always derived from the completed parent reload."),
                _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
            };
}
