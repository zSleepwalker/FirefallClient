using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeNodeSelectionReloadCapture(
    CapacityFinalCalleeLargeNodeFieldSelectionCapture FieldSelection,
    uint? FreshNodeDword);

public enum CapacityFinalCalleeLargeNodeSelectionReloadOutcome
{
    ParentSelectionBlocked,
    SecondFieldZeroToBoundedTarget,
    MissingChildFreshNodeDword,
    ChildReloadCompleted
}

public readonly record struct CapacityFinalCalleeLargeNodeSelectionReloadRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public sealed record CapacityFinalCalleeLargeNodeSelectionReloadPlan(
    CapacityFinalCalleeLargeNodeSelectionReloadCapture Capture,
    CapacityFinalCalleeLargeNodeSelectionReloadOutcome Outcome)
{
    public CapacityFinalCalleeLargeNodeFieldSelectionPlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeLargeNodeReloadPlan? ChildPlan { get; init; }
    public bool ChildReached { get; init; }
    public uint? ChildEntryECX { get; init; }
    public uint? ChildECXSourceRawFileOffset { get; init; }
    public uint? ChildFreshNodeAddress { get; init; }
    public bool ChildFreshNodeCaptured { get; init; }
    public uint? ChildFreshNodeDword { get; init; }
    public uint? ChildEAX { get; init; }
    public uint? ChildEDX { get; init; }
    public bool ChildJumpReached { get; init; }
    public uint? ChildJumpTargetRawFileOffset { get; init; }
    public CapacityFinalCalleeLargeNodeFieldSelectionFlags? ParentCompareFlagsAtStop { get; init; }
    public bool ParentCompareFlagsPreservedThroughChild { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeNodeSelectionReloadRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeNodeSelectionReloadRead>.Empty;
    public bool UnreachedFreshNodeCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the bounded 91F0 field selection with its 9205 fresh-node reload.
/// </summary>
public static class CapacityFinalCalleeLargeNodeSelectionReloadLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeNodeFieldSelectionProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeNodeReloadProjection.EntryRawFileOffset;
    public const uint ChildBoundedStopRawFileOffset =
        CapacityFinalCalleeLargeNodeReloadProjection.JumpTargetRawFileOffset;
    public const uint SecondFieldZeroStopRawFileOffset =
        CapacityFinalCalleeLargeNodeFieldSelectionProjection.Plus10ZeroDestinationRawFileOffset;

    public static CapacityFinalCalleeLargeNodeSelectionReloadPlan Project(
        CapacityFinalCalleeLargeNodeSelectionReloadCapture capture)
    {
        CapacityFinalCalleeLargeNodeFieldSelectionPlan parent =
            CapacityFinalCalleeLargeNodeFieldSelectionProjection.Project(capture.FieldSelection);
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeNodeSelectionReloadRead>();
        foreach (CapacityFinalCalleeLargeNodeFieldSelectionRead read in parent.Reads)
        {
            reads.Add(new(
                read.RawFileOffset,
                read.Address,
                AddressCaptured: read.Address.HasValue,
                ValueCaptured: read.ValueCaptured,
                Value: read.Value));
        }

        bool childRoute =
            parent.ExactCleanExitAdmitted
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
            && ((parent.Outcome
                    == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.FirstFieldNonzeroToTarget
                && parent.Plus14BranchReached
                && parent.Plus14BranchTaken == true
                && parent.Plus14BranchSelectedDestinationRawFileOffset == ChildEntryRawFileOffset
                && parent.Plus14LeaReached)
                || (parent.Outcome
                    == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldNonzeroToFallthrough
                && parent.Plus14BranchTaken == false
                && parent.Plus10BranchReached
                && parent.Plus10BranchTaken == false
                && parent.Plus10BranchSelectedDestinationRawFileOffset == ChildEntryRawFileOffset
                && parent.Plus10LeaReached));
        bool secondFieldZeroRoute =
            parent.ExactCleanExitAdmitted
            && parent.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldZeroToTarget
            && parent.Plus10BranchReached
            && parent.Plus10BranchTaken == true
            && parent.Plus10BranchSelectedDestinationRawFileOffset == SecondFieldZeroStopRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == SecondFieldZeroStopRawFileOffset;

        uint? childECX = null;
        uint? childECXSource = null;
        CapacityFinalCalleeLargeNodeReloadPlan? child = null;
        bool exactCleanExitAdmitted = false;
        CapacityFinalCalleeLargeNodeSelectionReloadOutcome outcome;
        uint stopAt;
        bool childReached = false;

        if (childRoute)
        {
            childReached = true;
            if (parent.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.FirstFieldNonzeroToTarget)
            {
                childECX = parent.Plus14LeaECXValue;
                childECXSource =
                    CapacityFinalCalleeLargeNodeFieldSelectionProjection.Plus14LeaRawFileOffset;
            }
            else
            {
                childECX = parent.Plus10LeaECXValue;
                childECXSource =
                    CapacityFinalCalleeLargeNodeFieldSelectionProjection.Plus10LeaRawFileOffset;
            }

            child = CapacityFinalCalleeLargeNodeReloadProjection.Project(new(
                EntryECX: childECX,
                FreshNodeDword: capture.FreshNodeDword));
            foreach (CapacityFinalCalleeLargeNodeReloadRead read in child.Reads)
            {
                reads.Add(new(
                    read.RawFileOffset,
                    read.Address,
                    read.AddressCaptured,
                    read.ValueCaptured,
                    read.Value));
            }

            exactCleanExitAdmitted = child.ExactCleanCompletion
                && child.JumpReached
                && child.JumpTargetRawFileOffset == ChildBoundedStopRawFileOffset
                && child.StopAtRawFileOffsetExclusive == ChildBoundedStopRawFileOffset;
            outcome = child.Outcome
                == CapacityFinalCalleeLargeNodeReloadOutcome.MissingFreshNodeDword
                    ? CapacityFinalCalleeLargeNodeSelectionReloadOutcome.MissingChildFreshNodeDword
                    : CapacityFinalCalleeLargeNodeSelectionReloadOutcome.ChildReloadCompleted;
            stopAt = child.StopAtRawFileOffsetExclusive;
        }
        else if (secondFieldZeroRoute)
        {
            outcome =
                CapacityFinalCalleeLargeNodeSelectionReloadOutcome.SecondFieldZeroToBoundedTarget;
            stopAt = SecondFieldZeroStopRawFileOffset;
            exactCleanExitAdmitted = !capture.FreshNodeDword.HasValue;
        }
        else
        {
            outcome = CapacityFinalCalleeLargeNodeSelectionReloadOutcome.ParentSelectionBlocked;
            stopAt = parent.StopAtRawFileOffsetExclusive;
        }

        return new(capture, outcome)
        {
            ParentPlan = parent,
            ChildPlan = child,
            ChildReached = childReached,
            ChildEntryECX = childECX,
            ChildECXSourceRawFileOffset = childECXSource,
            ChildFreshNodeAddress = child?.FreshNodeAddress,
            ChildFreshNodeCaptured = child?.FreshNodeCaptured ?? false,
            ChildFreshNodeDword = child?.FreshNodeDword,
            ChildEAX = child?.EAX,
            ChildEDX = child?.EDX,
            ChildJumpReached = child?.JumpReached ?? false,
            ChildJumpTargetRawFileOffset = child?.JumpTargetRawFileOffset,
            ParentCompareFlagsAtStop = parent.FlagsAtStop,
            ParentCompareFlagsPreservedThroughChild =
                childReached && parent.FlagsAtStop.HasValue,
            Reads = reads.ToImmutable(),
            UnreachedFreshNodeCaptureProvided =
                !childReached && capture.FreshNodeDword.HasValue,
            ExactCleanExitAdmitted = exactCleanExitAdmitted,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }
}
