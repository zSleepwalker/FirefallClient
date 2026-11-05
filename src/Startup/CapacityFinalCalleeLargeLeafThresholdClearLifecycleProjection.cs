using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeLeafThresholdClearCapture(
    CapacityFinalCalleeLargeLeafThresholdGateCapture ParentThreshold,
    uint? FreshChildEbpMinus4);

public enum CapacityFinalCalleeLargeLeafThresholdClearOutcome
{
    ParentThresholdBlocked,
    UnsignedBelowBranchTarget,
    MissingChildFreshFrameMinus4,
    StoredZero
}

public enum CapacityFinalCalleeLargeLeafThresholdClearAccessKind
{
    Read,
    Compare,
    Write,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeLeafThresholdClearRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeLeafThresholdClearWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeLeafThresholdClearAccess(
    CapacityFinalCalleeLargeLeafThresholdClearAccessKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value,
    uint? Left,
    uint? Right,
    uint? Result,
    CapacityFinalCalleeLargeLeafThresholdGateFlags? Flags,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset);

public sealed record CapacityFinalCalleeLargeLeafThresholdClearPlan(
    CapacityFinalCalleeLargeLeafThresholdClearCapture Capture,
    CapacityFinalCalleeLargeLeafThresholdClearOutcome Outcome)
{
    public CapacityFinalCalleeLargeLeafThresholdGatePlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeLargeLeafClearStorePlan? ChildPlan { get; init; }
    public bool ChildReached { get; init; }
    public uint? ChildEntryEBP { get; init; }
    public uint? ParentCompareStoreAddress { get; init; }
    public uint? ParentCompareStoreValue { get; init; }
    public uint? ChildFreshReadAddress { get; init; }
    public uint? ChildFreshReadValue { get; init; }
    public CapacityFinalCalleeLargeLeafThresholdGateFlags? ParentCompareFlags { get; init; }
    public CapacityFinalCalleeLargeLeafThresholdGateFlags? FlagsAtStop { get; init; }
    public bool ChildMovPreservesParentCompareFlags { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeLeafThresholdClearRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeLeafThresholdClearRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeLeafThresholdClearWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeLeafThresholdClearWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeLeafThresholdClearAccess> Accesses
    {
        get;
        init;
    } = ImmutableArray<CapacityFinalCalleeLargeLeafThresholdClearAccess>.Empty;
    public bool UnreachedFreshChildCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the unsigned-not-below threshold fallthrough into the independent
/// fresh EBP-4 reload and zero store.
/// </summary>
public static class CapacityFinalCalleeLargeLeafThresholdClearLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeLeafThresholdGateProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeLeafClearStoreProjection.EntryRawFileOffset;
    public const uint BelowBranchTargetRawFileOffset =
        CapacityFinalCalleeLargeLeafThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint ChildBoundedStopRawFileOffset =
        CapacityFinalCalleeLargeLeafClearStoreProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeLargeLeafThresholdClearPlan Project(
        CapacityFinalCalleeLargeLeafThresholdClearCapture capture)
    {
        CapacityFinalCalleeLargeLeafThresholdGatePlan parent =
            CapacityFinalCalleeLargeLeafThresholdGateProjection.Project(
                capture.ParentThreshold);
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeLeafThresholdClearRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeLeafThresholdClearWrite>();
        var accesses =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeLeafThresholdClearAccess>();

        foreach (CapacityFinalCalleeLargeLeafThresholdGateEvent parentEvent in parent.Events)
        {
            Append(parentEvent.Kind switch
            {
                CapacityFinalCalleeLargeLeafThresholdGateEventKind.Read =>
                    CapacityFinalCalleeLargeLeafThresholdClearAccessKind.Read,
                CapacityFinalCalleeLargeLeafThresholdGateEventKind.Compare =>
                    CapacityFinalCalleeLargeLeafThresholdClearAccessKind.Compare,
                CapacityFinalCalleeLargeLeafThresholdGateEventKind.Write =>
                    CapacityFinalCalleeLargeLeafThresholdClearAccessKind.Write,
                _ => CapacityFinalCalleeLargeLeafThresholdClearAccessKind.Branch
            },
                parentEvent.RawFileOffset,
                parentEvent.Address,
                parentEvent.AddressCaptured,
                parentEvent.ValueCaptured,
                parentEvent.Value,
                parentEvent.Left,
                parentEvent.Right,
                parentEvent.Result,
                parentEvent.Flags,
                parentEvent.BranchTaken,
                parentEvent.SelectedDestinationRawFileOffset);
        }

        bool cleanBelowRoute =
            parent.ExactCleanAdmission
            && parent.BranchReached
            && parent.Outcome
                == CapacityFinalCalleeLargeLeafThresholdGateOutcome.UnsignedBelowBranchTarget
            && parent.BranchTaken == true
            && parent.BranchSelectedDestinationRawFileOffset
                == BelowBranchTargetRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == BelowBranchTargetRawFileOffset;
        bool cleanChildRoute =
            parent.ExactCleanAdmission
            && parent.BranchReached
            && parent.Outcome
                == CapacityFinalCalleeLargeLeafThresholdGateOutcome.UnsignedNotBelowFallthrough
            && parent.BranchTaken == false
            && parent.BranchSelectedDestinationRawFileOffset == ChildEntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset;

        CapacityFinalCalleeLargeLeafClearStorePlan? child = null;
        bool childReached = false;
        uint? childEntryEbp = null;
        CapacityFinalCalleeLargeLeafThresholdClearOutcome outcome;
        uint stopAt;
        bool exactCleanExitAdmitted = false;

        if (cleanChildRoute)
        {
            childReached = true;
            childEntryEbp = capture.ParentThreshold.EntryEBP;
            child = CapacityFinalCalleeLargeLeafClearStoreProjection.Project(new(
                EntryEBP: childEntryEbp,
                FreshFrameMinus4: capture.FreshChildEbpMinus4));

            foreach (CapacityFinalCalleeLargeLeafClearStoreEvent childEvent in child.Events)
            {
                Append(
                    childEvent.Kind == CapacityFinalCalleeLargeLeafClearStoreEventKind.Read
                        ? CapacityFinalCalleeLargeLeafThresholdClearAccessKind.Read
                        : CapacityFinalCalleeLargeLeafThresholdClearAccessKind.Write,
                    childEvent.RawFileOffset,
                    childEvent.Address,
                    childEvent.AddressCaptured,
                    childEvent.ValueCaptured,
                    childEvent.Value,
                    left: null,
                    right: null,
                    result: null,
                    flags: null,
                    branchTaken: null,
                    selectedDestinationRawFileOffset: null);
            }

            outcome = child.Outcome switch
            {
                CapacityFinalCalleeLargeLeafClearStoreOutcome.MissingFreshFrameMinus4 =>
                    CapacityFinalCalleeLargeLeafThresholdClearOutcome
                        .MissingChildFreshFrameMinus4,
                CapacityFinalCalleeLargeLeafClearStoreOutcome.StoredZero =>
                    CapacityFinalCalleeLargeLeafThresholdClearOutcome.StoredZero,
                _ => CapacityFinalCalleeLargeLeafThresholdClearOutcome.ParentThresholdBlocked
            };
            exactCleanExitAdmitted =
                child.Outcome == CapacityFinalCalleeLargeLeafClearStoreOutcome.StoredZero
                && child.ExactCleanExitAdmitted
                && child.StopAtRawFileOffsetExclusive == ChildBoundedStopRawFileOffset;
            stopAt = child.StopAtRawFileOffsetExclusive;
        }
        else if (cleanBelowRoute)
        {
            outcome =
                CapacityFinalCalleeLargeLeafThresholdClearOutcome.UnsignedBelowBranchTarget;
            exactCleanExitAdmitted = !capture.FreshChildEbpMinus4.HasValue;
            stopAt = BelowBranchTargetRawFileOffset;
        }
        else
        {
            outcome = CapacityFinalCalleeLargeLeafThresholdClearOutcome.ParentThresholdBlocked;
            stopAt = parent.StopAtRawFileOffsetExclusive;
        }

        return new(capture, outcome)
        {
            ParentPlan = parent,
            ChildPlan = child,
            ChildReached = childReached,
            ChildEntryEBP = childEntryEbp,
            ParentCompareStoreAddress = parent.StoreAddress,
            ParentCompareStoreValue = parent.StoreValue,
            ChildFreshReadAddress = child?.FreshFrameAddress,
            ChildFreshReadValue = child?.FreshFrameValue,
            ParentCompareFlags = parent.CompareFlags,
            FlagsAtStop = parent.FlagsAtStop,
            ChildMovPreservesParentCompareFlags =
                childReached && parent.CompareFlags.HasValue && child!.FlagsUnaffected,
            Reads = reads.ToImmutable(),
            Writes = writes.ToImmutable(),
            Accesses = accesses.ToImmutable(),
            UnreachedFreshChildCaptureProvided =
                !childReached && capture.FreshChildEbpMinus4.HasValue,
            ExactCleanExitAdmitted = exactCleanExitAdmitted,
            StopAtRawFileOffsetExclusive = stopAt
        };

        void Append(
            CapacityFinalCalleeLargeLeafThresholdClearAccessKind kind,
            uint rawFileOffset,
            uint? address,
            bool addressCaptured,
            bool valueCaptured,
            uint? value,
            uint? left,
            uint? right,
            uint? result,
            CapacityFinalCalleeLargeLeafThresholdGateFlags? flags,
            bool? branchTaken,
            uint? selectedDestinationRawFileOffset)
        {
            var access = new CapacityFinalCalleeLargeLeafThresholdClearAccess(
                kind,
                rawFileOffset,
                address,
                addressCaptured,
                valueCaptured,
                value,
                left,
                right,
                result,
                flags,
                branchTaken,
                selectedDestinationRawFileOffset);
            accesses.Add(access);
            if (kind == CapacityFinalCalleeLargeLeafThresholdClearAccessKind.Read)
            {
                reads.Add(new(
                    rawFileOffset, address, addressCaptured, valueCaptured, value));
            }
            else if (kind == CapacityFinalCalleeLargeLeafThresholdClearAccessKind.Write)
            {
                writes.Add(new(
                    rawFileOffset, address, addressCaptured, valueCaptured, value));
            }
        }
    }
}
