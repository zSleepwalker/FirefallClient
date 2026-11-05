using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionCapture(
    uint? EntryECX,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionStop
{
    ParentStoppedWithoutClean931BExit,
    NonzeroTestBoundedAt9334,
    ChildStoppedWithoutCleanExit,
    CommonMaskOrStoreCompletedAt9323
}

public enum CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource
{
    ParentShift,
    ParentTest,
    ChildOr
}

public readonly record struct CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow,
    CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource Source);

public sealed record CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionPlan(
    CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionCapture Capture)
{
    public CapacityFinalCalleeCommonMaskSelectionGateCapture ParentCapture { get; init; }
    public CapacityFinalCalleeCommonMaskSelectionGatePlan Parent { get; init; } = null!;
    public bool ParentClean931BExitAdmitted { get; init; }
    public CapacityFinalCalleeCommonMaskOrStoreCapture? ChildCapture { get; init; }
    public CapacityFinalCalleeCommonMaskOrStorePlan? Child { get; init; }
    public bool ChildCleanExitAdmitted { get; init; }
    public bool ChildPerformedFreshGlobalRead => Child?.GlobalReadReached ?? false;
    public ImmutableArray<CapacityFinalCalleeCommonMaskOrStoreWrite> OrderedWrites { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonMaskOrStoreWrite>.Empty;
    public CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlags? ParentFlagsAtStop { get; init; }
    public CapacityFinalCalleeCommonMaskOrStoreFlags? ChildFlagsAtStop { get; init; }
    public CapacityFinalCalleeCommonMaskOrStoreFlags? FlagsAtStore { get; init; }
    public CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlags? FlagsAtStop { get; init; }
    public CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionStop Stop { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
    public bool Entered9323RingWrites => false;
}

public static class CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection
{
    public const uint ZeroTestDestinationRawFileOffset = 0x012C931B;
    public const uint NonzeroTestDestinationRawFileOffset = 0x012C9334;
    public const uint OrStoreBoundaryRawFileOffsetExclusive = 0x012C9323;

    public static CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionPlan Project(
        CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionCapture capture)
    {
        var parentCapture = new CapacityFinalCalleeCommonMaskSelectionGateCapture(
            capture.EntryECX,
            capture.FreshGlobalValue);
        var parent = CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(parentCapture);
        var parentFlags = MapParentFlags(parent.FlagsAtStop, parent);

        if (parent.Outcome ==
                CapacityFinalCalleeCommonMaskSelectionGateOutcome.NonzeroTestResultToBounded9334 &&
            parent.JnzReached &&
            parent.JnzTaken == true &&
            parent.JnzSelectedDestinationRawFileOffset == NonzeroTestDestinationRawFileOffset &&
            parent.StopAtRawFileOffsetExclusive == NonzeroTestDestinationRawFileOffset &&
            parent.ExactCleanBoundedDestination)
        {
            return new CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionPlan(capture)
            {
                ParentCapture = parentCapture,
                Parent = parent,
                ParentClean931BExitAdmitted = false,
                ParentFlagsAtStop = parentFlags,
                FlagsAtStop = parentFlags,
                Stop = CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionStop
                    .NonzeroTestBoundedAt9334,
                StopAtRawFileOffsetExclusive = NonzeroTestDestinationRawFileOffset
            };
        }

        bool exactZeroTestExit =
            parent.Outcome ==
                CapacityFinalCalleeCommonMaskSelectionGateOutcome.ZeroTestResultToBounded931B &&
            parent.ShiftReached &&
            parent.FreshGlobalReadReached &&
            parent.FreshGlobalValueCaptured &&
            parent.TestReached &&
            parent.JnzReached &&
            parent.JnzTaken == false &&
            parent.JnzSelectedDestinationRawFileOffset == ZeroTestDestinationRawFileOffset &&
            parent.StopAtRawFileOffsetExclusive == ZeroTestDestinationRawFileOffset &&
            parent.ExactCleanBoundedDestination &&
            parent.DestinationValueAfterShift.HasValue &&
            parent.Capture.FreshGlobalValue.HasValue;

        if (!exactZeroTestExit)
        {
            return new CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionPlan(capture)
            {
                ParentCapture = parentCapture,
                Parent = parent,
                ParentClean931BExitAdmitted = false,
                ParentFlagsAtStop = parentFlags,
                FlagsAtStop = parentFlags,
                Stop = CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionStop
                    .ParentStoppedWithoutClean931BExit,
                StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
            };
        }

        var childCapture = new CapacityFinalCalleeCommonMaskOrStoreCapture(
            parent.Capture.FreshGlobalValue,
            parent.DestinationValueAfterShift);
        var child = CapacityFinalCalleeCommonMaskOrStoreProjection.Project(childCapture);
        bool childCleanExit =
            child.Outcome == CapacityFinalCalleeCommonMaskOrStoreOutcome.Completed &&
            child.EntryECXCaptured &&
            child.EntryEDICaptured &&
            child.OrReached &&
            child.GlobalWriteReached &&
            child.GlobalWriteAddress == CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalVirtualAddress &&
            child.GlobalWriteValue == child.OrResult &&
            child.ExactCleanExitAdmitted &&
            child.StopAtRawFileOffsetExclusive ==
                CapacityFinalCalleeCommonMaskOrStoreProjection.BoundaryRawFileOffsetExclusive &&
            child.Writes.Length == 1;

        var childFlags = child.OrFlags;
        var finalFlags = childCleanExit
            ? MapChildFlags(childFlags)
            : parentFlags;

        return new CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionPlan(capture)
        {
            ParentCapture = parentCapture,
            Parent = parent,
            ParentClean931BExitAdmitted = true,
            ChildCapture = childCapture,
            Child = child,
            ChildCleanExitAdmitted = childCleanExit,
            OrderedWrites = childCleanExit
                ? child.Writes
                : ImmutableArray<CapacityFinalCalleeCommonMaskOrStoreWrite>.Empty,
            ParentFlagsAtStop = parentFlags,
            ChildFlagsAtStop = childFlags,
            FlagsAtStore = childCleanExit ? childFlags : null,
            FlagsAtStop = finalFlags,
            Stop = childCleanExit
                ? CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionStop
                    .CommonMaskOrStoreCompletedAt9323
                : CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionStop
                    .ChildStoppedWithoutCleanExit,
            StopAtRawFileOffsetExclusive = childCleanExit
                ? CapacityFinalCalleeCommonMaskOrStoreProjection.BoundaryRawFileOffsetExclusive
                : child.StopAtRawFileOffsetExclusive
        };
    }

    private static CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlags? MapParentFlags(
        CapacityFinalCalleeCommonMaskSelectionGateFlags? flags,
        CapacityFinalCalleeCommonMaskSelectionGatePlan parent)
    {
        if (flags is not CapacityFinalCalleeCommonMaskSelectionGateFlags value)
        {
            return null;
        }

        var source = parent.TestFlags.HasValue
            ? CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource.ParentTest
            : CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource.ParentShift;
        return new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow, source);
    }

    private static CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlags? MapChildFlags(
        CapacityFinalCalleeCommonMaskOrStoreFlags? flags)
    {
        if (flags is not CapacityFinalCalleeCommonMaskOrStoreFlags value)
        {
            return null;
        }

        return new(
            value.Carry,
            value.Parity,
            value.AuxiliaryCarry,
            value.Zero,
            value.Sign,
            value.Overflow,
            CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource.ChildOr);
    }
}
