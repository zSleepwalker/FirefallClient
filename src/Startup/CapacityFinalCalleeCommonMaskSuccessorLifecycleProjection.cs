using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonMaskSuccessorLifecycleCapture(
    uint? ParentEntryECX,
    uint? ParentFreshGlobal1EDADC8,
    uint? ChildEntryEDX,
    uint? FreshEDXPlus8,
    uint? FreshGlobal1EDADD8);

public enum CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
{
    ParentMissingEntryECX,
    ParentMissingFreshGlobal1EDADC8,
    ParentZeroTestToBounded931B,
    ChildMissingEntryEDX,
    ChildMissingFreshEDXPlus8,
    ChildMissingFreshGlobal1EDADD8,
    ChildUnsignedBelowBranchTarget,
    ChildUnsignedNotBelowFallthrough
}

public enum CapacityFinalCalleeCommonMaskSuccessorLifecycleFlagSource
{
    ParentShift,
    ParentTest,
    ChildCompare
}

public readonly record struct CapacityFinalCalleeCommonMaskSuccessorLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow,
    CapacityFinalCalleeCommonMaskSuccessorLifecycleFlagSource Source);

public readonly record struct CapacityFinalCalleeCommonMaskSuccessorLifecycleRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public sealed record CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan(
    CapacityFinalCalleeCommonMaskSuccessorLifecycleCapture Capture,
    CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonMaskSelectionGateCapture ParentCapture { get; init; }
    public CapacityFinalCalleeCommonMaskSelectionGatePlan Parent { get; init; } = null!;
    public CapacityFinalCalleeCommonMaskSuccessorLifecycleRead? ParentGlobalRead { get; init; }
    public CapacityFinalCalleeCommonSuccessorThresholdGateCapture? ChildCapture { get; init; }
    public CapacityFinalCalleeCommonSuccessorThresholdGatePlan? Child { get; init; }
    public bool ChildReached => Child is not null;
    public bool ParentNonzeroHandoffAdmitted { get; init; }
    public bool UnreachedChildCaptureProvided { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonMaskSuccessorLifecycleRead> OrderedReads { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonMaskSuccessorLifecycleRead>.Empty;
    public CapacityFinalCalleeCommonMaskSuccessorLifecycleFlags? ParentFlagsAtStop { get; init; }
    public CapacityFinalCalleeCommonMaskSuccessorLifecycleFlags? ChildFlagsAtStop { get; init; }
    public CapacityFinalCalleeCommonMaskSuccessorLifecycleFlags? FlagsAtStop { get; init; }
    public uint? SelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public bool TargetEntered => false;
    public bool FutureWritesReached => false;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the exact nonzero common-mask edge to the independently captured
/// successor threshold gate. Both gates stop at their selected branch edge.
/// </summary>
public static class CapacityFinalCalleeCommonMaskSuccessorLifecycleProjection
{
    public const uint ParentEntryRawFileOffset =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.EntryRawFileOffset;
    public const uint ParentZeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.ZeroDestinationRawFileOffset;
    public const uint ParentNonzeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.NonzeroDestinationRawFileOffset;
    public const uint ParentFreshGlobalVirtualAddress =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.FreshGlobalVirtualAddress;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonSuccessorThresholdGateProjection.EntryRawFileOffset;
    public const uint ChildFreshGlobalVirtualAddress =
        CapacityFinalCalleeCommonSuccessorThresholdGateProjection.FreshGlobalVirtualAddress;
    public const uint ChildBelowDestinationRawFileOffset =
        CapacityFinalCalleeCommonSuccessorThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint ChildFallthroughDestinationRawFileOffset =
        CapacityFinalCalleeCommonSuccessorThresholdGateProjection.FallthroughRawFileOffset;

    public static CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan Project(
        CapacityFinalCalleeCommonMaskSuccessorLifecycleCapture capture)
    {
        var parentCapture = new CapacityFinalCalleeCommonMaskSelectionGateCapture(
            capture.ParentEntryECX,
            capture.ParentFreshGlobal1EDADC8);
        CapacityFinalCalleeCommonMaskSelectionGatePlan parent =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(parentCapture);

        CapacityFinalCalleeCommonMaskSuccessorLifecycleRead? parentRead =
            parent.FreshGlobalReadReached
                ? new(
                    CapacityFinalCalleeCommonMaskSelectionGateProjection
                        .FreshGlobalMoveRawFileOffset,
                    ParentFreshGlobalVirtualAddress,
                    AddressCaptured: true,
                    ValueCaptured: parent.FreshGlobalValueCaptured,
                    Value: parentCapture.FreshGlobalValue)
                : null;
        CapacityFinalCalleeCommonMaskSuccessorLifecycleFlags? parentFlags =
            MapParentFlags(parent);
        bool anyChildCaptureProvided =
            capture.ChildEntryEDX.HasValue
            || capture.FreshEDXPlus8.HasValue
            || capture.FreshGlobal1EDADD8.HasValue;

        if (parent.Outcome
            == CapacityFinalCalleeCommonMaskSelectionGateOutcome.MissingEntryECX)
        {
            return FinishParentStop(
                CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome.ParentMissingEntryECX);
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonMaskSelectionGateOutcome.MissingFreshGlobalValue)
        {
            return FinishParentStop(
                CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                    .ParentMissingFreshGlobal1EDADC8);
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonMaskSelectionGateOutcome.ZeroTestResultToBounded931B)
        {
            bool exactParentZeroExit =
                parent.JnzReached
                && parent.JnzTaken == false
                && parent.JnzSelectedDestinationRawFileOffset
                    == ParentZeroDestinationRawFileOffset
                && parent.StopAtRawFileOffsetExclusive
                    == ParentZeroDestinationRawFileOffset
                && parent.ExactCleanBoundedDestination;

            return new(
                capture,
                CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                    .ParentZeroTestToBounded931B)
            {
                ParentCapture = parentCapture,
                Parent = parent,
                ParentGlobalRead = parentRead,
                ParentFlagsAtStop = parentFlags,
                FlagsAtStop = parentFlags,
                UnreachedChildCaptureProvided = anyChildCaptureProvided,
                SelectedDestinationRawFileOffset =
                    ParentZeroDestinationRawFileOffset,
                ExactCleanAdmission = exactParentZeroExit && !anyChildCaptureProvided,
                StopAtRawFileOffsetExclusive =
                    ParentZeroDestinationRawFileOffset,
                OrderedReads = parentRead is { } read
                    ? ImmutableArray.Create(read)
                    : ImmutableArray<CapacityFinalCalleeCommonMaskSuccessorLifecycleRead>
                        .Empty
            };
        }

        if (!IsExactParentNonzeroExit(parent))
        {
            return FinishParentStop(
                CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                    .ParentMissingFreshGlobal1EDADC8);
        }

        var childCapture = new CapacityFinalCalleeCommonSuccessorThresholdGateCapture(
            capture.ChildEntryEDX,
            capture.FreshEDXPlus8,
            capture.FreshGlobal1EDADD8);
        CapacityFinalCalleeCommonSuccessorThresholdGatePlan child =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(childCapture);
        var orderedReads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonMaskSuccessorLifecycleRead>();
        if (parentRead is { } reachedParentRead)
        {
            orderedReads.Add(reachedParentRead);
        }

        orderedReads.AddRange(child.Reads.Select(read =>
            new CapacityFinalCalleeCommonMaskSuccessorLifecycleRead(
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value)));

        CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome childOutcome =
            child.Outcome switch
            {
                CapacityFinalCalleeCommonSuccessorThresholdGateOutcome.MissingEntryEDX =>
                    CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                        .ChildMissingEntryEDX,
                CapacityFinalCalleeCommonSuccessorThresholdGateOutcome.MissingFreshEdxPlus8 =>
                    CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                        .ChildMissingFreshEDXPlus8,
                CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                    .MissingFreshGlobal1EDADD8 =>
                    CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                        .ChildMissingFreshGlobal1EDADD8,
                CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                    .UnsignedBelowBranchTarget =>
                    CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                        .ChildUnsignedBelowBranchTarget,
                CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                    .UnsignedNotBelowFallthrough =>
                    CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                        .ChildUnsignedNotBelowFallthrough,
                _ => throw new InvalidOperationException(
                    "The successor threshold gate returned an unexpected outcome.")
            };

        CapacityFinalCalleeCommonMaskSuccessorLifecycleFlags? childFlags =
            MapChildFlags(child.CompareFlags);
        bool childBranchSelected =
            child.JBReached
            && child.JBSelectedDestinationRawFileOffset
                == child.StopAtRawFileOffsetExclusive
            && child.JBSelectedDestinationRawFileOffset is
                ChildBelowDestinationRawFileOffset or ChildFallthroughDestinationRawFileOffset
            && child.ExactCleanAdmission
            && child.Writes.IsEmpty
            && !child.TargetEntered;
        bool exactCleanAdmission =
            childBranchSelected
            && !child.UnreachedFreshEdxPlus8CaptureProvided
            && !child.UnreachedFreshGlobalCaptureProvided;

        return new(capture, childOutcome)
        {
            ParentCapture = parentCapture,
            Parent = parent,
            ParentGlobalRead = parentRead,
            ChildCapture = childCapture,
            Child = child,
            ParentNonzeroHandoffAdmitted = true,
            ParentFlagsAtStop = parentFlags,
            ChildFlagsAtStop = childFlags,
            FlagsAtStop = childFlags ?? parentFlags,
            UnreachedChildCaptureProvided =
                child.UnreachedFreshEdxPlus8CaptureProvided
                || child.UnreachedFreshGlobalCaptureProvided,
            OrderedReads = orderedReads.ToImmutable(),
            SelectedDestinationRawFileOffset =
                child.JBSelectedDestinationRawFileOffset,
            ExactCleanAdmission = exactCleanAdmission,
            StopAtRawFileOffsetExclusive =
                child.StopAtRawFileOffsetExclusive
        };

        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan FinishParentStop(
            CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome outcome) =>
            new(capture, outcome)
            {
                ParentCapture = parentCapture,
                Parent = parent,
                ParentGlobalRead = parentRead,
                ParentFlagsAtStop = parentFlags,
                FlagsAtStop = parentFlags,
                UnreachedChildCaptureProvided = anyChildCaptureProvided,
                OrderedReads = parentRead is { } read
                    ? ImmutableArray.Create(read)
                    : ImmutableArray<CapacityFinalCalleeCommonMaskSuccessorLifecycleRead>
                        .Empty,
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
    }

    private static bool IsExactParentNonzeroExit(
        CapacityFinalCalleeCommonMaskSelectionGatePlan parent) =>
        parent.Outcome
            == CapacityFinalCalleeCommonMaskSelectionGateOutcome
                .NonzeroTestResultToBounded9334
        && parent.ShiftReached
        && parent.FreshGlobalReadReached
        && parent.FreshGlobalValueCaptured
        && parent.TestReached
        && parent.JnzReached
        && parent.JnzTaken == true
        && parent.JnzSelectedDestinationRawFileOffset
            == ParentNonzeroDestinationRawFileOffset
        && parent.StopAtRawFileOffsetExclusive
            == ParentNonzeroDestinationRawFileOffset
        && parent.ExactCleanBoundedDestination
        && parent.DestinationValueAfterShift.HasValue;

    private static CapacityFinalCalleeCommonMaskSuccessorLifecycleFlags? MapParentFlags(
        CapacityFinalCalleeCommonMaskSelectionGatePlan parent)
    {
        if (parent.FlagsAtStop is not CapacityFinalCalleeCommonMaskSelectionGateFlags flags)
        {
            return null;
        }

        CapacityFinalCalleeCommonMaskSuccessorLifecycleFlagSource source =
            parent.TestFlags.HasValue
                ? CapacityFinalCalleeCommonMaskSuccessorLifecycleFlagSource.ParentTest
                : CapacityFinalCalleeCommonMaskSuccessorLifecycleFlagSource.ParentShift;
        return new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow,
            source);
    }

    private static CapacityFinalCalleeCommonMaskSuccessorLifecycleFlags? MapChildFlags(
        CapacityFinalCalleeCommonSuccessorThresholdGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonSuccessorThresholdGateFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow,
                CapacityFinalCalleeCommonMaskSuccessorLifecycleFlagSource.ChildCompare)
            : null;
}
