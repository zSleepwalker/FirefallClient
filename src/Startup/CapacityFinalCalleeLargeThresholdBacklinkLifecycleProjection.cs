using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeThresholdBacklinkLifecycleCapture(
    uint? EntryEDX,
    uint? EntryEBX,
    uint? EntryEBP,
    uint? FreshEdxPlus8,
    uint? FreshEbpMinus10,
    uint? FreshFrameMinus14,
    uint? FreshBacklink);

public enum CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
{
    MissingThresholdEntryEDX,
    MissingThresholdFreshEdxPlus8,
    MissingThresholdEntryEBX,
    MissingThresholdEntryEBP,
    MissingThresholdFreshEbpMinus10,
    ThresholdUnsignedBelowBranchTarget,
    ThresholdUnsignedNotBelowFallthrough,
    ThresholdBelowWithRejectedBacklinkCaptures,
    MissingBacklinkFreshFrameMinus14,
    MissingBacklinkFreshBacklink,
    BacklinkEqualToFallthrough,
    BacklinkUnequalToTarget
}

public enum CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage
{
    Threshold,
    BacklinkGate
}

public enum CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind
{
    Read,
    Compare,
    Write,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeThresholdBacklinkLifecycleFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeThresholdBacklinkLifecycleRead(
    CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeThresholdBacklinkLifecycleWrite(
    CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeThresholdBacklinkLifecycleEvent(
    CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage Stage,
    CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeThresholdBacklinkLifecycleFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public readonly record struct CapacityFinalCalleeLargeThresholdBacklinkLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan(
    CapacityFinalCalleeLargeThresholdBacklinkLifecycleCapture Capture,
    CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeLargeFrameThresholdPlan Threshold { get; init; } = null!;
    public CapacityFinalCalleeLargeFrameBacklinkGatePlan? BacklinkGate { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleEvent>.Empty;
    public bool BacklinkGateReached { get; init; }
    public bool EntryEBPRetainedForBacklinkGate { get; init; }
    public uint? BacklinkGateEntryEBP { get; init; }
    public bool EntryEDXRetainedForBacklinkGate { get; init; }
    public uint? BacklinkGateEntryEDX { get; init; }
    public bool UnreachedFreshFrameMinus14CaptureProvided { get; init; }
    public bool UnreachedFreshBacklinkCaptureProvided { get; init; }
    public bool ThresholdCleanExitAdmitted { get; init; }
    public bool BacklinkCleanExitAdmitted { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public CapacityFinalCalleeLargeThresholdBacklinkLifecycleFlags? BranchFlags { get; init; }
    public CapacityFinalCalleeLargeThresholdBacklinkLifecycleFlags? FlagsAtStop =>
        BacklinkGate?.CompareFlags is { } backlinkFlags
            ? new(backlinkFlags.Carry, backlinkFlags.Parity, backlinkFlags.Auxiliary,
                backlinkFlags.Zero, backlinkFlags.Sign, backlinkFlags.Overflow)
            : Threshold.CompareFlags is { } thresholdFlags
                ? new(thresholdFlags.Carry, thresholdFlags.Parity, thresholdFlags.Auxiliary,
                    thresholdFlags.Zero, thresholdFlags.Sign, thresholdFlags.Overflow)
                : null;
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the clean not-below threshold fallthrough at 91B9 into the fresh
/// frame-minus-14 and backlink gate, without reusing the threshold store value.
/// </summary>
public static class CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeFrameThresholdProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress = 0x016C9DA8;
    public const uint BacklinkGateEntryRawFileOffset =
        CapacityFinalCalleeLargeFrameBacklinkGateProjection.EntryRawFileOffset;
    public const uint ThresholdBelowDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameThresholdProjection.BranchTargetRawFileOffset;
    public const uint BacklinkUnequalDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameBacklinkGateProjection.NonzeroDestinationRawFileOffset;
    public const uint BacklinkEqualDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameBacklinkGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint BoundaryRawFileOffsetExclusive =
        BacklinkEqualDestinationRawFileOffset;

    public const string CapturedWindowBytes =
        CapacityFinalCalleeLargeFrameThresholdProjection.CapturedWindowBytes
        + " "
        + CapacityFinalCalleeLargeFrameBacklinkGateProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan Project(
        CapacityFinalCalleeLargeThresholdBacklinkLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeFrameThresholdPlan threshold =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                capture.EntryEDX,
                capture.EntryEBX,
                capture.EntryEBP,
                capture.FreshEdxPlus8,
                capture.FreshEbpMinus10));

        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeThresholdBacklinkLifecycleRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeThresholdBacklinkLifecycleWrite>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeThresholdBacklinkLifecycleEvent>();
        AppendThreshold(threshold, reads, writes, events);

        bool lateFrameMinus14 = capture.FreshFrameMinus14.HasValue;
        bool lateBacklink = capture.FreshBacklink.HasValue;
        bool backlinkGateReached = false;
        bool ebpRetained = false;
        uint? backlinkEntryEBP = null;
        bool edxRetained = false;
        uint? backlinkEntryEDX = null;
        bool thresholdCleanExitAdmitted = threshold.ExactCleanAdmission;
        bool backlinkCleanExitAdmitted = false;
        CapacityFinalCalleeLargeFrameBacklinkGatePlan? backlinkGate = null;
        bool branchReached = threshold.BranchReached;
        bool? branchTaken = threshold.BranchTaken;
        CapacityFinalCalleeLargeThresholdBacklinkLifecycleFlags? branchFlags =
            threshold.BranchFlags is { } thresholdFlags
                ? Convert(thresholdFlags)
                : null;
        uint? branchDestination = threshold.BranchSelectedDestinationRawFileOffset;

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan Finish(
            CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                Threshold = threshold,
                BacklinkGate = backlinkGate,
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                Events = events.ToImmutable(),
                BacklinkGateReached = backlinkGateReached,
                EntryEBPRetainedForBacklinkGate = ebpRetained,
                BacklinkGateEntryEBP = backlinkEntryEBP,
                EntryEDXRetainedForBacklinkGate = edxRetained,
                BacklinkGateEntryEDX = backlinkEntryEDX,
                UnreachedFreshFrameMinus14CaptureProvided = lateFrameMinus14,
                UnreachedFreshBacklinkCaptureProvided = lateBacklink,
                ThresholdCleanExitAdmitted = thresholdCleanExitAdmitted,
                BacklinkCleanExitAdmitted = backlinkCleanExitAdmitted,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                BranchReached = branchReached,
                BranchTaken = branchTaken,
                BranchFlags = branchFlags,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                StopAtRawFileOffsetExclusive = stopAt
            };

        switch (threshold.Outcome)
        {
            case CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEDX:
                return Finish(
                    CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                        .MissingThresholdEntryEDX,
                    threshold.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEdxPlus8:
                return Finish(
                    CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                        .MissingThresholdFreshEdxPlus8,
                    threshold.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEBX:
                return Finish(
                    CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                        .MissingThresholdEntryEBX,
                    threshold.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEBP:
                return Finish(
                    CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                        .MissingThresholdEntryEBP,
                    threshold.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEbpMinus10:
                return Finish(
                    CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                        .MissingThresholdFreshEbpMinus10,
                    threshold.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameThresholdOutcome.UnsignedBelowBranchTarget:
            {
                bool rejectedChildCaptures = lateFrameMinus14 || lateBacklink;
                lateFrameMinus14 = rejectedChildCaptures && lateFrameMinus14;
                lateBacklink = rejectedChildCaptures && lateBacklink;
                return Finish(
                    rejectedChildCaptures
                        ? CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                            .ThresholdBelowWithRejectedBacklinkCaptures
                        : CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                            .ThresholdUnsignedBelowBranchTarget,
                    threshold.StopAtRawFileOffsetExclusive,
                    exactCleanExitAdmitted:
                        threshold.ExactCleanAdmission && !rejectedChildCaptures);
            }
            case CapacityFinalCalleeLargeFrameThresholdOutcome.UnsignedNotBelowFallthrough:
                if (!threshold.ExactCleanAdmission
                    || threshold.BranchTaken != false
                    || threshold.BranchSelectedDestinationRawFileOffset
                        != BacklinkGateEntryRawFileOffset)
                {
                    return Finish(
                        CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                            .ThresholdUnsignedNotBelowFallthrough,
                        threshold.StopAtRawFileOffsetExclusive);
                }

                backlinkGateReached = true;
                lateFrameMinus14 = false;
                lateBacklink = false;
                backlinkGate = CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(new(
                    EntryEBP: threshold.EntryEBPCaptured
                        ? capture.EntryEBP
                        : null,
                    FreshFrameMinus14: capture.FreshFrameMinus14,
                    EntryEDX: capture.FreshFrameMinus14.HasValue
                        && threshold.EntryEDXCaptured
                            ? capture.EntryEDX
                            : null,
                    FreshBacklink: capture.FreshBacklink));
                lateFrameMinus14 =
                    backlinkGate.UnreachedFreshFrameMinus14CaptureProvided;
                lateBacklink = backlinkGate.UnreachedFreshBacklinkCaptureProvided;
                ebpRetained = backlinkGate.FrameMinus14ReadReached
                    && threshold.EntryEBPCaptured;
                backlinkEntryEBP = ebpRetained ? capture.EntryEBP : null;
                edxRetained = backlinkGate.FrameMinus14Captured
                    && threshold.EntryEDXCaptured;
                backlinkEntryEDX = edxRetained ? capture.EntryEDX : null;
                AppendBacklinkGate(backlinkGate, reads, writes, events);
                branchTaken = backlinkGate.JnzTaken ?? threshold.BranchTaken;
                branchFlags = backlinkGate.CompareFlags is { } backlinkFlags
                    ? Convert(backlinkFlags)
                    : branchFlags;
                branchDestination =
                    backlinkGate.JnzSelectedDestinationRawFileOffset
                    ?? threshold.BranchSelectedDestinationRawFileOffset;
                backlinkCleanExitAdmitted = backlinkGate.ExactCleanExitAdmitted;

                return Finish(
                    MapBacklinkOutcome(backlinkGate.Outcome),
                    backlinkGate.StopAtRawFileOffsetExclusive,
                    exactCleanExitAdmitted:
                        threshold.ExactCleanAdmission
                        && backlinkGate.ExactCleanExitAdmitted);
            default:
                throw new InvalidOperationException("Unhandled threshold outcome.");
        }
    }

    private static CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome MapBacklinkOutcome(
        CapacityFinalCalleeLargeFrameBacklinkGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingFreshFrameMinus14 =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .MissingBacklinkFreshFrameMinus14,
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingFreshBacklink =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .MissingBacklinkFreshBacklink,
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome.EqualToFallthrough =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .BacklinkEqualToFallthrough,
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome.UnequalToTarget =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .BacklinkUnequalToTarget,
            _ => throw new InvalidOperationException(
                "The backlink gate can only follow a clean threshold fallthrough.")
        };

    private static void AppendThreshold(
        CapacityFinalCalleeLargeFrameThresholdPlan threshold,
        ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleRead>.Builder reads,
        ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleWrite>.Builder writes,
        ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleEvent>.Builder events)
    {
        foreach (CapacityFinalCalleeLargeFrameThresholdRead read in threshold.Reads)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage.Threshold,
                read.RawFileOffset,
                read.Address,
                read.Address.HasValue,
                read.ValueCaptured,
                read.Value));
        }

        foreach (CapacityFinalCalleeLargeFrameThresholdWrite write in threshold.Writes)
        {
            writes.Add(new(
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage.Threshold,
                write.RawFileOffset,
                write.Address,
                write.AddressCaptured,
                write.ValueCaptured,
                write.Value));
        }

        foreach (CapacityFinalCalleeLargeFrameThresholdEvent item in threshold.Events)
        {
            events.Add(new(
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage.Threshold,
                Convert(item.Kind),
                item.RawFileOffset,
                item.Address,
                item.AddressCaptured,
                item.ValueCaptured,
                item.Value,
                item.Left,
                item.Right,
                item.Result,
                item.Flags is { } flags ? Convert(flags) : null,
                item.BranchTaken,
                item.SelectedDestinationRawFileOffset));
        }
    }

    private static void AppendBacklinkGate(
        CapacityFinalCalleeLargeFrameBacklinkGatePlan backlinkGate,
        ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleRead>.Builder reads,
        ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleWrite>.Builder writes,
        ImmutableArray<CapacityFinalCalleeLargeThresholdBacklinkLifecycleEvent>.Builder events)
    {
        foreach (CapacityFinalCalleeLargeFrameBacklinkGateRead read in backlinkGate.Reads)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage.BacklinkGate,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value));
        }

        foreach (CapacityFinalCalleeLargeFrameBacklinkGateEvent item in backlinkGate.Events)
        {
            events.Add(new(
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage.BacklinkGate,
                Convert(item.Kind),
                item.RawFileOffset,
                item.Address,
                item.AddressCaptured,
                item.ValueCaptured,
                item.Value,
                item.Left,
                item.Right,
                item.Result,
                item.Flags is { } flags ? Convert(flags) : null,
                item.BranchTaken,
                item.SelectedDestinationRawFileOffset));
        }
    }

    private static CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind Convert(
        CapacityFinalCalleeLargeFrameThresholdEventKind kind) =>
        kind switch
        {
            CapacityFinalCalleeLargeFrameThresholdEventKind.Read =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind.Read,
            CapacityFinalCalleeLargeFrameThresholdEventKind.Compare =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind.Compare,
            CapacityFinalCalleeLargeFrameThresholdEventKind.Write =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind.Write,
            CapacityFinalCalleeLargeFrameThresholdEventKind.Branch =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind.Branch,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind Convert(
        CapacityFinalCalleeLargeFrameBacklinkGateEventKind kind) =>
        kind switch
        {
            CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Read =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind.Read,
            CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Compare =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind.Compare,
            CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Branch =>
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleEventKind.Branch,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static CapacityFinalCalleeLargeThresholdBacklinkLifecycleFlags Convert(
        CapacityFinalCalleeLargeFrameThresholdFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);

    private static CapacityFinalCalleeLargeThresholdBacklinkLifecycleFlags Convert(
        CapacityFinalCalleeLargeFrameBacklinkGateFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);

    private static ImmutableArray<
        CapacityFinalCalleeLargeThresholdBacklinkLifecycleInstructionEvidence>
        BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleInstructionEvidence>();
        foreach (CapacityFinalCalleeLargeFrameThresholdInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameThresholdProjection.Instructions)
        {
            instructions.Add(new(
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                instruction.Bytes,
                instruction.BranchTargetRawFileOffset,
                instruction.BranchTargetVirtualAddress));
        }

        foreach (CapacityFinalCalleeLargeFrameBacklinkGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameBacklinkGateProjection.Instructions)
        {
            instructions.Add(new(
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                instruction.Bytes,
                instruction.BranchTargetRawFileOffset,
                instruction.BranchTargetVirtualAddress));
        }

        return instructions.ToImmutable();
    }
}
