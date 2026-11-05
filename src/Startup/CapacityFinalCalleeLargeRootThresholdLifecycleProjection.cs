using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRootThresholdLifecycleCapture(
    uint? EntryEDX,
    uint? EntryEBP,
    uint? FreshEdxPlusC,
    uint? FreshEdxPlus18,
    uint? EntryEBX,
    uint? FreshEdxPlus8,
    uint? FreshEbpMinus10);

public enum CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
{
    MissingRootEntryEDX,
    MissingRootFreshEdxPlusC,
    MissingRootFreshEdxPlus18,
    MissingRootEntryEBPForStore,
    RootEqualToTarget,
    RootEqualWithRejectedThresholdCaptures,
    RootUnequalContinuationRejected,
    MissingThresholdFreshEdxPlus8,
    MissingThresholdEntryEBX,
    MissingThresholdEntryEBP,
    MissingThresholdFreshEbpMinus10,
    ThresholdUnsignedBelowBranchTarget,
    ThresholdUnsignedNotBelowFallthrough
}

public enum CapacityFinalCalleeLargeRootThresholdLifecycleStage
{
    RootGate,
    Threshold
}

public enum CapacityFinalCalleeLargeRootThresholdLifecycleEventKind
{
    Read,
    Compare,
    Write,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeRootThresholdLifecycleFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeRootThresholdLifecycleRead(
    CapacityFinalCalleeLargeRootThresholdLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeRootThresholdLifecycleWrite(
    CapacityFinalCalleeLargeRootThresholdLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeRootThresholdLifecycleEvent(
    CapacityFinalCalleeLargeRootThresholdLifecycleStage Stage,
    CapacityFinalCalleeLargeRootThresholdLifecycleEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeRootThresholdLifecycleFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public readonly record struct CapacityFinalCalleeLargeRootThresholdLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeRootThresholdLifecyclePlan(
    CapacityFinalCalleeLargeRootThresholdLifecycleCapture Capture,
    CapacityFinalCalleeLargeRootThresholdLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeLargeFrameRootGatePlan RootGate { get; init; } = null!;
    public CapacityFinalCalleeLargeFrameThresholdPlan? Threshold { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleEvent>.Empty;
    public bool ThresholdReached { get; init; }
    public bool EntryEDXRetainedForThreshold { get; init; }
    public uint? ThresholdEntryEDX { get; init; }
    public bool EntryEBPRetainedForThreshold { get; init; }
    public uint? ThresholdEntryEBP { get; init; }
    public bool UnreachedEntryEBXCaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlus8CaptureProvided { get; init; }
    public bool UnreachedFreshEbpMinus10CaptureProvided { get; init; }
    public bool RootCleanExitAdmitted { get; init; }
    public bool ThresholdCleanExitAdmitted { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public CapacityFinalCalleeLargeRootThresholdLifecycleFlags? BranchFlags { get; init; }
    public CapacityFinalCalleeLargeRootThresholdLifecycleFlags? FlagsAtStop { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the large-frame root gate's clean unequal fallthrough into the
/// threshold gate while preserving the original caller EDX and EBP roots.
/// </summary>
public static class CapacityFinalCalleeLargeRootThresholdLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C919B;
    public const uint EntryVirtualAddress = 0x016C9D9B;
    public const uint RootEqualDestinationRawFileOffset = 0x012C91D6;
    public const uint ThresholdEntryRawFileOffset = 0x012C91A8;
    public const uint ThresholdBranchTargetRawFileOffset = 0x012C8E76;
    public const uint ThresholdFallthroughRawFileOffset = 0x012C91B9;
    public const uint BoundaryRawFileOffsetExclusive = ThresholdFallthroughRawFileOffset;

    public const string CapturedWindowBytes =
        "8B 42 0C 8B 4A 18 89 4D F0 3B C2 74 2E "
        + "8B 4A 08 3B CB 89 4D EC 8B 4D F0 0F 82 BD FC FF FF";

    public static ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeRootThresholdLifecyclePlan Project(
        CapacityFinalCalleeLargeRootThresholdLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeFrameRootGatePlan root =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(new(
                capture.EntryEDX,
                capture.EntryEBP,
                capture.FreshEdxPlusC,
                capture.FreshEdxPlus18));

        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRootThresholdLifecycleRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRootThresholdLifecycleWrite>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRootThresholdLifecycleEvent>();
        AppendRoot(root, reads, writes, events);

        bool lateEntryEBX = capture.EntryEBX.HasValue;
        bool lateFreshEdxPlus8 = capture.FreshEdxPlus8.HasValue;
        bool lateFreshEbpMinus10 = capture.FreshEbpMinus10.HasValue;
        bool thresholdReached = false;
        bool thresholdCleanExitAdmitted = false;
        CapacityFinalCalleeLargeFrameThresholdPlan? thresholdPlan = null;
        CapacityFinalCalleeLargeRootThresholdLifecycleFlags? rootFlagsAtStop =
            root.CompareFlags is { } rootCompareFlags ? Convert(rootCompareFlags) : null;
        bool? branchTaken = root.BranchTaken;
        CapacityFinalCalleeLargeRootThresholdLifecycleFlags? branchFlags = rootFlagsAtStop;
        CapacityFinalCalleeLargeRootThresholdLifecycleFlags? flagsAtStop = rootFlagsAtStop;
        uint? branchDestination = root.BranchSelectedDestinationRawFileOffset;

        CapacityFinalCalleeLargeRootThresholdLifecyclePlan Finish(
            CapacityFinalCalleeLargeRootThresholdLifecycleOutcome outcome,
            uint stopAt,
            bool rootCleanExitAdmitted = false,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                RootGate = root,
                Threshold = thresholdPlan,
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                Events = events.ToImmutable(),
                ThresholdReached = thresholdReached,
                EntryEDXRetainedForThreshold = thresholdReached,
                ThresholdEntryEDX = thresholdReached ? capture.EntryEDX : null,
                EntryEBPRetainedForThreshold = thresholdPlan?.EntryEBPCaptured == true,
                ThresholdEntryEBP = thresholdPlan?.EntryEBPCaptured == true
                    ? capture.EntryEBP
                    : null,
                UnreachedEntryEBXCaptureProvided = lateEntryEBX,
                UnreachedFreshEdxPlus8CaptureProvided = lateFreshEdxPlus8,
                UnreachedFreshEbpMinus10CaptureProvided = lateFreshEbpMinus10,
                RootCleanExitAdmitted = rootCleanExitAdmitted,
                ThresholdCleanExitAdmitted = thresholdCleanExitAdmitted,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                BranchReached = branchDestination.HasValue,
                BranchTaken = branchTaken,
                BranchFlags = branchFlags,
                FlagsAtStop = flagsAtStop,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                StopAtRawFileOffsetExclusive = stopAt
            };

        switch (root.Outcome)
        {
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEDX:
                return Finish(
                    CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingRootEntryEDX,
                    root.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlusC:
                return Finish(
                    CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingRootFreshEdxPlusC,
                    root.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlus18:
                return Finish(
                    CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingRootFreshEdxPlus18,
                    root.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEBPForStore:
                return Finish(
                    CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingRootEntryEBPForStore,
                    root.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameRootGateOutcome.EqualToTarget:
            {
                bool hasThresholdCaptures =
                    capture.EntryEBX.HasValue
                    || capture.FreshEdxPlus8.HasValue
                    || capture.FreshEbpMinus10.HasValue;
                bool exactRootExit = root.ExactCleanAdmission && !hasThresholdCaptures
                    && root.BranchReached && root.BranchTaken == true
                    && root.BranchSelectedDestinationRawFileOffset == RootEqualDestinationRawFileOffset
                    && root.StopAtRawFileOffsetExclusive == RootEqualDestinationRawFileOffset;
                return Finish(
                    hasThresholdCaptures
                        ? CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
                            .RootEqualWithRejectedThresholdCaptures
                        : CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.RootEqualToTarget,
                    root.StopAtRawFileOffsetExclusive,
                    rootCleanExitAdmitted: exactRootExit,
                    exactCleanExitAdmitted: exactRootExit);
            }
            case CapacityFinalCalleeLargeFrameRootGateOutcome.UnequalToFallthrough:
                if (!root.ExactCleanAdmission
                    || !root.BranchReached
                    || root.StopAtRawFileOffsetExclusive != ThresholdEntryRawFileOffset
                    || root.BranchTaken != false
                    || root.BranchSelectedDestinationRawFileOffset
                        != ThresholdEntryRawFileOffset)
                {
                    return Finish(
                        CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
                            .RootUnequalContinuationRejected,
                        root.StopAtRawFileOffsetExclusive);
                }

                thresholdReached = true;
                lateEntryEBX = false;
                lateFreshEdxPlus8 = false;
                lateFreshEbpMinus10 = false;
                thresholdPlan = CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                    EntryEDX: capture.EntryEDX,
                    EntryEBX: capture.EntryEBX,
                    EntryEBP: capture.FreshEdxPlus8.HasValue && capture.EntryEBX.HasValue
                        ? capture.EntryEBP
                        : null,
                    FreshEdxPlus8: capture.FreshEdxPlus8,
                    FreshEbpMinus10: capture.FreshEbpMinus10));
                lateEntryEBX = thresholdPlan.UnreachedEntryEBXCaptureProvided;
                lateFreshEdxPlus8 =
                    thresholdPlan.UnreachedFreshEdxPlus8CaptureProvided;
                lateFreshEbpMinus10 =
                    thresholdPlan.UnreachedFreshEbpMinus10CaptureProvided;
                AppendThreshold(thresholdPlan, reads, writes, events);
                branchTaken = thresholdPlan.BranchTaken ?? root.BranchTaken;
                branchFlags = thresholdPlan.BranchFlags is { } thresholdFlags
                    ? Convert(thresholdFlags)
                    : branchFlags;
                flagsAtStop = thresholdPlan.CompareFlags is { } thresholdCompareFlags
                    ? Convert(thresholdCompareFlags)
                    : rootFlagsAtStop;
                branchDestination =
                    thresholdPlan.BranchSelectedDestinationRawFileOffset
                    ?? root.BranchSelectedDestinationRawFileOffset;
                thresholdCleanExitAdmitted = thresholdPlan.ExactCleanAdmission;

                return Finish(
                    MapOutcome(thresholdPlan.Outcome),
                    thresholdPlan.StopAtRawFileOffsetExclusive,
                    rootCleanExitAdmitted: true,
                    exactCleanExitAdmitted:
                        root.ExactCleanAdmission && thresholdPlan.ExactCleanAdmission);
            default:
                throw new InvalidOperationException("Unhandled root-gate outcome.");
        }
    }

    private static CapacityFinalCalleeLargeRootThresholdLifecycleOutcome MapOutcome(
        CapacityFinalCalleeLargeFrameThresholdOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEdxPlus8 =>
                CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingThresholdFreshEdxPlus8,
            CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEBX =>
                CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingThresholdEntryEBX,
            CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEBP =>
                CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingThresholdEntryEBP,
            CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEbpMinus10 =>
                CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingThresholdFreshEbpMinus10,
            CapacityFinalCalleeLargeFrameThresholdOutcome.UnsignedBelowBranchTarget =>
                CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
                    .ThresholdUnsignedBelowBranchTarget,
            CapacityFinalCalleeLargeFrameThresholdOutcome.UnsignedNotBelowFallthrough =>
                CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
                    .ThresholdUnsignedNotBelowFallthrough,
            _ => throw new InvalidOperationException(
                "The composed threshold must inherit EDX and EBP from the root gate.")
        };

    private static void AppendRoot(
        CapacityFinalCalleeLargeFrameRootGatePlan root,
        ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleRead>.Builder reads,
        ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleWrite>.Builder writes,
        ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleEvent>.Builder events)
    {
        foreach (CapacityFinalCalleeLargeFrameRootGateRead read in root.Reads)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeRootThresholdLifecycleStage.RootGate,
                read.RawFileOffset,
                read.Address,
                read.Address.HasValue,
                read.ValueCaptured,
                read.Value));
        }

        foreach (CapacityFinalCalleeLargeFrameRootGateWrite write in root.Writes)
        {
            writes.Add(new(
                CapacityFinalCalleeLargeRootThresholdLifecycleStage.RootGate,
                write.RawFileOffset,
                write.Address,
                write.AddressCaptured,
                write.ValueCaptured,
                write.Value));
        }

        foreach (CapacityFinalCalleeLargeFrameRootGateEvent item in root.Events)
        {
            events.Add(new(
                CapacityFinalCalleeLargeRootThresholdLifecycleStage.RootGate,
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

    private static void AppendThreshold(
        CapacityFinalCalleeLargeFrameThresholdPlan threshold,
        ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleRead>.Builder reads,
        ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleWrite>.Builder writes,
        ImmutableArray<CapacityFinalCalleeLargeRootThresholdLifecycleEvent>.Builder events)
    {
        foreach (CapacityFinalCalleeLargeFrameThresholdRead read in threshold.Reads)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeRootThresholdLifecycleStage.Threshold,
                read.RawFileOffset,
                read.Address,
                read.Address.HasValue,
                read.ValueCaptured,
                read.Value));
        }

        foreach (CapacityFinalCalleeLargeFrameThresholdWrite write in threshold.Writes)
        {
            writes.Add(new(
                CapacityFinalCalleeLargeRootThresholdLifecycleStage.Threshold,
                write.RawFileOffset,
                write.Address,
                write.AddressCaptured,
                write.ValueCaptured,
                write.Value));
        }

        foreach (CapacityFinalCalleeLargeFrameThresholdEvent item in threshold.Events)
        {
            events.Add(new(
                CapacityFinalCalleeLargeRootThresholdLifecycleStage.Threshold,
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

    private static CapacityFinalCalleeLargeRootThresholdLifecycleEventKind Convert(
        CapacityFinalCalleeLargeFrameRootGateEventKind kind) =>
        kind switch
        {
            CapacityFinalCalleeLargeFrameRootGateEventKind.Read =>
                CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Read,
            CapacityFinalCalleeLargeFrameRootGateEventKind.Compare =>
                CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Compare,
            CapacityFinalCalleeLargeFrameRootGateEventKind.Write =>
                CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Write,
            CapacityFinalCalleeLargeFrameRootGateEventKind.Branch =>
                CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Branch,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static CapacityFinalCalleeLargeRootThresholdLifecycleEventKind Convert(
        CapacityFinalCalleeLargeFrameThresholdEventKind kind) =>
        kind switch
        {
            CapacityFinalCalleeLargeFrameThresholdEventKind.Read =>
                CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Read,
            CapacityFinalCalleeLargeFrameThresholdEventKind.Compare =>
                CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Compare,
            CapacityFinalCalleeLargeFrameThresholdEventKind.Write =>
                CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Write,
            CapacityFinalCalleeLargeFrameThresholdEventKind.Branch =>
                CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Branch,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static CapacityFinalCalleeLargeRootThresholdLifecycleFlags Convert(
        CapacityFinalCalleeLargeFrameRootGateCompareFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);

    private static CapacityFinalCalleeLargeRootThresholdLifecycleFlags Convert(
        CapacityFinalCalleeLargeFrameThresholdFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);

    private static ImmutableArray<
        CapacityFinalCalleeLargeRootThresholdLifecycleInstructionEvidence> BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRootThresholdLifecycleInstructionEvidence>();
        foreach (CapacityFinalCalleeLargeFrameRootGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameRootGateProjection.Instructions)
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

        return instructions.ToImmutable();
    }
}
