using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeNodeSelectionLeafThresholdCapture(
    CapacityFinalCalleeLargeNodeFieldSelectionCapture ParentSelection,
    uint? FreshGlobalValue,
    uint? FreshEbpMinusC);

public enum CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
{
    ParentSelectionBlocked,
    ParentBoundedAt9205,
    MissingChildFreshGlobalValue,
    MissingChildEntryEDX,
    MissingChildEntryEBP,
    MissingChildFreshEbpMinusC,
    UnsignedBelowBranchTarget,
    UnsignedNotBelowFallthrough
}

public enum CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind
{
    Read,
    Write
}

public readonly record struct CapacityFinalCalleeLargeNodeSelectionLeafThresholdFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeNodeSelectionLeafThresholdRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeNodeSelectionLeafThresholdWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccess(
    CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public sealed record CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan(
    CapacityFinalCalleeLargeNodeSelectionLeafThresholdCapture Capture,
    CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome Outcome)
{
    public CapacityFinalCalleeLargeNodeFieldSelectionPlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeLargeLeafThresholdGatePlan? ChildPlan { get; init; }
    public bool ChildReached { get; init; }
    public uint? ChildEntryEDX { get; init; }
    public uint? ChildEntryEDXSourceRawFileOffset { get; init; }
    public uint? ChildEntryEBP { get; init; }
    public CapacityFinalCalleeLargeNodeSelectionLeafThresholdFlags? ParentCompareFlagsAtStop
    {
        get;
        init;
    }
    public CapacityFinalCalleeLargeNodeSelectionLeafThresholdFlags? ChildCompareFlagsAtStop
    {
        get;
        init;
    }
    public CapacityFinalCalleeLargeNodeSelectionLeafThresholdFlags? FlagsAtStop
    {
        get;
        init;
    }
    public bool ParentCompareFlagsPreservedWhenChildCompareMissing { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeNodeSelectionLeafThresholdRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeNodeSelectionLeafThresholdRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeNodeSelectionLeafThresholdWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeNodeSelectionLeafThresholdWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccess> Accesses
    {
        get;
        init;
    } = ImmutableArray<CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccess>.Empty;
    public bool UnreachedFreshGlobalCaptureProvided { get; init; }
    public bool UnreachedFreshFrameCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the clean 91F0 selection into the bounded 920B leaf-threshold gate.
/// </summary>
public static class CapacityFinalCalleeLargeNodeSelectionLeafThresholdLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeNodeFieldSelectionProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeLeafThresholdGateProjection.EntryRawFileOffset;
    public const uint ParentOtherRouteStopRawFileOffset =
        CapacityFinalCalleeLargeNodeFieldSelectionProjection.BoundaryRawFileOffsetExclusive;
    public const uint ChildEntryEDXSourceRawFileOffset =
        CapacityFinalCalleeLargeNodeFieldSelectionProjection.FrameReadRawFileOffset;
    public const uint ChildBoundedStopRawFileOffset =
        CapacityFinalCalleeLargeLeafThresholdGateProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan Project(
        CapacityFinalCalleeLargeNodeSelectionLeafThresholdCapture capture)
    {
        CapacityFinalCalleeLargeNodeFieldSelectionPlan parent =
            CapacityFinalCalleeLargeNodeFieldSelectionProjection.Project(capture.ParentSelection);
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeNodeSelectionLeafThresholdRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeNodeSelectionLeafThresholdWrite>();
        var accesses =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccess>();

        foreach (CapacityFinalCalleeLargeNodeFieldSelectionRead read in parent.Reads)
        {
            var composedRead = new CapacityFinalCalleeLargeNodeSelectionLeafThresholdRead(
                read.RawFileOffset,
                read.Address,
                read.Address.HasValue,
                read.ValueCaptured,
                read.Value);
            reads.Add(composedRead);
            accesses.Add(new(
                CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Read,
                composedRead.RawFileOffset,
                composedRead.Address,
                composedRead.AddressCaptured,
                composedRead.ValueCaptured,
                composedRead.Value));
        }

        bool childRoute =
            parent.ExactCleanExitAdmitted
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
            && parent.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldZeroToTarget
            && parent.Plus14BranchReached
            && parent.Plus14BranchTaken == false
            && parent.Plus10BranchReached
            && parent.Plus10BranchTaken == true
            && parent.Plus10LeaReached
            && parent.Plus10BranchSelectedDestinationRawFileOffset == ChildEntryRawFileOffset;
        bool parentOtherRoute =
            parent.ExactCleanExitAdmitted
            && parent.StopAtRawFileOffsetExclusive == ParentOtherRouteStopRawFileOffset
            && ((parent.Outcome
                    == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.FirstFieldNonzeroToTarget
                && parent.Plus14BranchReached
                && parent.Plus14BranchTaken == true
                && parent.Plus14LeaReached
                && !parent.Plus10CompareReached
                && !parent.Plus10BranchReached
                && parent.Plus14BranchSelectedDestinationRawFileOffset
                    == ParentOtherRouteStopRawFileOffset)
                || (parent.Outcome
                    == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldNonzeroToFallthrough
                && parent.Plus14BranchReached
                && parent.Plus14BranchTaken == false
                && parent.Plus10BranchReached
                && parent.Plus10BranchTaken == false
                && parent.Plus10LeaReached
                && parent.Plus10BranchSelectedDestinationRawFileOffset
                    == ParentOtherRouteStopRawFileOffset));

        CapacityFinalCalleeLargeLeafThresholdGatePlan? child = null;
        bool childReached = false;
        uint? childEDX = null;
        uint? childEBP = null;
        CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome outcome;
        uint stopAt;
        bool exactCleanExitAdmitted = false;

        if (childRoute)
        {
            childReached = true;
            childEDX = parent.FrameMinus4EDX;
            childEBP = childEDX.HasValue && capture.FreshGlobalValue.HasValue
                ? capture.ParentSelection.EntryEBP
                : null;
            child = CapacityFinalCalleeLargeLeafThresholdGateProjection.Project(new(
                EntryEDX: childEDX,
                FreshGlobalValue: capture.FreshGlobalValue,
                EntryEBP: childEBP,
                FreshEbpMinusC: capture.FreshEbpMinusC));

            foreach (CapacityFinalCalleeLargeLeafThresholdGateEvent gateEvent in child.Events)
            {
                if (gateEvent.Kind
                    == CapacityFinalCalleeLargeLeafThresholdGateEventKind.Read)
                {
                    var composedRead =
                        new CapacityFinalCalleeLargeNodeSelectionLeafThresholdRead(
                            gateEvent.RawFileOffset,
                            gateEvent.Address,
                            gateEvent.AddressCaptured,
                            gateEvent.ValueCaptured,
                            gateEvent.Value);
                    reads.Add(composedRead);
                    accesses.Add(new(
                        CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Read,
                        composedRead.RawFileOffset,
                        composedRead.Address,
                        composedRead.AddressCaptured,
                        composedRead.ValueCaptured,
                        composedRead.Value));
                }
                else if (gateEvent.Kind
                    == CapacityFinalCalleeLargeLeafThresholdGateEventKind.Write)
                {
                    var composedWrite =
                        new CapacityFinalCalleeLargeNodeSelectionLeafThresholdWrite(
                            gateEvent.RawFileOffset,
                            gateEvent.Address,
                            gateEvent.AddressCaptured,
                            gateEvent.ValueCaptured,
                            gateEvent.Value);
                    writes.Add(composedWrite);
                    accesses.Add(new(
                        CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Write,
                        composedWrite.RawFileOffset,
                        composedWrite.Address,
                        composedWrite.AddressCaptured,
                        composedWrite.ValueCaptured,
                        composedWrite.Value));
                }
            }

            exactCleanExitAdmitted =
                child.ExactCleanAdmission
                && child.BranchReached
                && child.StopAtRawFileOffsetExclusive
                    == child.BranchSelectedDestinationRawFileOffset;
            outcome = child.Outcome switch
            {
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingFreshGlobalValue =>
                    CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                        .MissingChildFreshGlobalValue,
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingEntryEDX =>
                    CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                        .MissingChildEntryEDX,
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingEntryEBP =>
                    CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                        .MissingChildEntryEBP,
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingFreshEbpMinusC =>
                    CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                        .MissingChildFreshEbpMinusC,
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.UnsignedBelowBranchTarget =>
                    CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                        .UnsignedBelowBranchTarget,
                CapacityFinalCalleeLargeLeafThresholdGateOutcome.UnsignedNotBelowFallthrough =>
                    CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                        .UnsignedNotBelowFallthrough,
                _ => throw new InvalidOperationException("Unhandled leaf threshold outcome.")
            };
            stopAt = child.StopAtRawFileOffsetExclusive;
        }
        else if (parentOtherRoute)
        {
            outcome =
                CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome.ParentBoundedAt9205;
            stopAt = ParentOtherRouteStopRawFileOffset;
            exactCleanExitAdmitted =
                !capture.FreshGlobalValue.HasValue
                && !capture.FreshEbpMinusC.HasValue;
        }
        else
        {
            outcome =
                CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome.ParentSelectionBlocked;
            stopAt = parent.StopAtRawFileOffsetExclusive;
        }

        CapacityFinalCalleeLargeNodeSelectionLeafThresholdFlags? parentFlags =
            ConvertParentFlags(parent.FlagsAtStop);
        CapacityFinalCalleeLargeNodeSelectionLeafThresholdFlags? childFlags =
            child?.CompareFlags is CapacityFinalCalleeLargeLeafThresholdGateFlags childCompare
                ? ConvertChildFlags(childCompare)
                : null;

        return new(capture, outcome)
        {
            ParentPlan = parent,
            ChildPlan = child,
            ChildReached = childReached,
            ChildEntryEDX = childEDX,
            ChildEntryEDXSourceRawFileOffset = childReached
                ? ChildEntryEDXSourceRawFileOffset
                : null,
            ChildEntryEBP = childEBP,
            ParentCompareFlagsAtStop = parentFlags,
            ChildCompareFlagsAtStop = childFlags,
            FlagsAtStop = childFlags ?? parentFlags,
            ParentCompareFlagsPreservedWhenChildCompareMissing =
                childReached && childFlags is null && parentFlags.HasValue,
            Reads = reads.ToImmutable(),
            Writes = writes.ToImmutable(),
            Accesses = accesses.ToImmutable(),
            UnreachedFreshGlobalCaptureProvided =
                !childReached && capture.FreshGlobalValue.HasValue,
            UnreachedFreshFrameCaptureProvided =
                !childReached
                    ? capture.FreshEbpMinusC.HasValue
                    : child?.UnreachedFreshFrameCaptureProvided ?? false,
            ExactCleanExitAdmitted = exactCleanExitAdmitted,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static CapacityFinalCalleeLargeNodeSelectionLeafThresholdFlags? ConvertParentFlags(
        CapacityFinalCalleeLargeNodeFieldSelectionFlags? flags) =>
        flags is CapacityFinalCalleeLargeNodeFieldSelectionFlags value
            ? new(value.Carry, value.Parity, value.AuxiliaryCarry, value.Zero, value.Sign, value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeNodeSelectionLeafThresholdFlags ConvertChildFlags(
        CapacityFinalCalleeLargeLeafThresholdGateFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);
}
