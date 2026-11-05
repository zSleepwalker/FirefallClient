using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightFieldThresholdLifecycleCapture(
    uint? EntryEDX,
    uint? FreshEDXPlus14,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
{
    MissingEntryEDXForFreshRead,
    MissingFreshEDXPlus14,
    MissingParentPrerequisiteWithUnusedFreshGlobalValue,
    ZeroTo92BD,
    ZeroTo92BDWithUnusedFreshGlobalValue,
    MissingFreshGlobalValue,
    UnsignedBelowBranchTarget,
    UnsignedNotBelowFallthrough
}

public enum CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage
{
    RightFieldGate,
    ThresholdGate
}

public readonly record struct CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeRightFieldThresholdLifecycleRead(
    CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeRightFieldThresholdLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan(
    CapacityFinalCalleeLargeRightFieldThresholdLifecycleCapture Capture,
    CapacityFinalCalleeLargeRightFieldGatePlan RightFieldGate,
    CapacityFinalCalleeLargeRightFieldThresholdGatePlan? ThresholdGate,
    CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeRightFieldThresholdLifecycleRead> Reads
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeRightFieldThresholdLifecycleRead>.Empty;
    public bool ThresholdGateReached => ThresholdGate is not null;
    public bool UnusedFreshGlobalValueCaptureProvided =>
        Capture.FreshGlobalValue.HasValue && !ThresholdGateReached;
    public bool ExactCleanParentAdmission => RightFieldGate.ExactCleanCompletion;
    public bool ExactCleanBranchToThresholdGate { get; init; }
    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? ParentTestFlags =>
        RightFieldGate.TestFlags is { } flags ? Convert(flags) : null;
    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? ThresholdCompareFlags =>
        ThresholdGate?.CompareFlags is { } flags ? Convert(flags) : null;
    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? FlagsAtStop =>
        ThresholdCompareFlags ?? ParentTestFlags;
    public bool ParentTestFlagsPreservedAtStop =>
        ParentTestFlags is { } parentFlags && FlagsAtStop == parentFlags;
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? BranchFlags { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public bool RightFieldBranchTargetEntered => false;
    public bool RightFieldFallthroughEntered => ThresholdGateReached;
    public bool ThresholdBranchTargetEntered => false;
    public bool ThresholdFallthroughEntered => false;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags Convert(
        CapacityFinalCalleeLargeRightFieldGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags Convert(
        CapacityFinalCalleeLargeRightFieldThresholdGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
}

/// <summary>
/// Composes the fresh right-field TEST/JZ into the independent global CMP/JB
/// only on the exact nonzero fallthrough, forwarding the loaded ECX value.
/// </summary>
public static class CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeRightFieldGateProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeLargeRightFieldGateProjection.EntryVirtualAddress;
    public const uint ThresholdEntryRawFileOffset =
        CapacityFinalCalleeLargeRightFieldThresholdGateProjection.EntryRawFileOffset;
    public const uint RightFieldZeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeRightFieldGateProjection.JzTargetRawFileOffset;
    public const uint ThresholdBranchTargetRawFileOffset =
        CapacityFinalCalleeLargeRightFieldThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint ThresholdFallthroughRawFileOffset =
        CapacityFinalCalleeLargeRightFieldThresholdGateProjection.FallthroughRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeLargeRightFieldThresholdGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint FreshGlobalValueVirtualAddress =
        CapacityFinalCalleeLargeRightFieldThresholdGateProjection.GlobalValueVirtualAddress;

    public const string CapturedWindowBytes =
        CapacityFinalCalleeLargeRightFieldGateProjection.CapturedWindowBytes
        + CapacityFinalCalleeLargeRightFieldThresholdGateProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeLargeRightFieldThresholdLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan Project(
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeRightFieldGatePlan parent =
            CapacityFinalCalleeLargeRightFieldGateProjection.Project(new(
                capture.EntryEDX,
                capture.FreshEDXPlus14));

        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRightFieldThresholdLifecycleRead>();
        foreach (CapacityFinalCalleeLargeRightFieldGateReadAttempt read in parent.ReadAttempts)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.RightFieldGate,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value));
        }

        bool branchReached = parent.JzReached;
        bool? branchTaken = parent.JzTaken;
        var branchFlags = parent.TestFlags is { } parentFlags
            ? Convert(parentFlags)
            : (CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags?)null;
        uint? branchDestination = parent.JzSelectedDestinationRawFileOffset;

        CapacityFinalCalleeLargeRightFieldThresholdGatePlan? threshold = null;
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome outcome;
        bool exactBranch = false;
        bool exactCleanExit = false;
        uint stopAt = parent.StopAtRawFileOffsetExclusive;

        switch (parent.Outcome)
        {
            case CapacityFinalCalleeLargeRightFieldGateOutcome.MissingEntryEDXForFreshRead:
            case CapacityFinalCalleeLargeRightFieldGateOutcome.MissingFreshEDXPlus14:
                outcome = capture.FreshGlobalValue.HasValue
                    ? CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                        .MissingParentPrerequisiteWithUnusedFreshGlobalValue
                    : parent.Outcome
                        == CapacityFinalCalleeLargeRightFieldGateOutcome
                            .MissingEntryEDXForFreshRead
                        ? CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                            .MissingEntryEDXForFreshRead
                        : CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                            .MissingFreshEDXPlus14;
                break;

            case CapacityFinalCalleeLargeRightFieldGateOutcome.ZeroTo92BD:
            {
                bool exactParent = parent.ExactCleanCompletion
                    && parent.JzReached
                    && parent.JzTaken == true
                    && parent.JzSelectedDestinationRawFileOffset
                        == RightFieldZeroDestinationRawFileOffset
                    && parent.StopAtRawFileOffsetExclusive
                        == RightFieldZeroDestinationRawFileOffset;
                bool unusedGlobal = capture.FreshGlobalValue.HasValue;
                outcome = unusedGlobal
                    ? CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                        .ZeroTo92BDWithUnusedFreshGlobalValue
                    : CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome.ZeroTo92BD;
                exactCleanExit = exactParent && !unusedGlobal;
                break;
            }

            case CapacityFinalCalleeLargeRightFieldGateOutcome.NonzeroTo92AB:
            {
                exactBranch = parent.ExactCleanCompletion
                    && parent.JzReached
                    && parent.JzTaken == false
                    && parent.JzSelectedDestinationRawFileOffset
                        == ThresholdEntryRawFileOffset
                    && parent.StopAtRawFileOffsetExclusive == ThresholdEntryRawFileOffset;
                if (!exactBranch)
                {
                    outcome = capture.FreshGlobalValue.HasValue
                        ? CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                            .MissingParentPrerequisiteWithUnusedFreshGlobalValue
                        : CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                            .MissingFreshEDXPlus14;
                    break;
                }

                threshold = CapacityFinalCalleeLargeRightFieldThresholdGateProjection.Project(new(
                    EntryECX: parent.FreshECX,
                    FreshGlobalValue: capture.FreshGlobalValue));
                foreach (CapacityFinalCalleeLargeRightFieldThresholdGateRead read
                    in threshold.Reads)
                {
                    reads.Add(new(
                        CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.ThresholdGate,
                        read.RawFileOffset,
                        read.Address,
                        read.AddressCaptured,
                        read.ValueCaptured,
                        read.Value));
                }

                branchReached = threshold.BranchReached || branchReached;
                if (threshold.BranchReached)
                {
                    branchTaken = threshold.BranchTaken;
                    branchFlags = threshold.BranchFlags is { } thresholdFlags
                        ? Convert(thresholdFlags)
                        : branchFlags;
                    branchDestination =
                        threshold.BranchSelectedDestinationRawFileOffset
                        ?? branchDestination;
                }

                stopAt = threshold.StopAtRawFileOffsetExclusive;
                exactCleanExit = threshold.ExactCleanCompletion
                    && threshold.CompareReached
                    && threshold.CompareLeft == parent.FreshECX
                    && threshold.CompareRight == capture.FreshGlobalValue
                    && threshold.BranchReached
                    && threshold.BranchTaken == threshold.CompareFlags?.Carry
                    && threshold.BranchSelectedDestinationRawFileOffset
                        == (threshold.BranchTaken == true
                            ? ThresholdBranchTargetRawFileOffset
                            : ThresholdFallthroughRawFileOffset)
                    && threshold.StopAtRawFileOffsetExclusive
                        == threshold.BranchSelectedDestinationRawFileOffset;
                outcome = threshold.Outcome switch
                {
                    CapacityFinalCalleeLargeRightFieldThresholdGateOutcome
                        .MissingFreshGlobalValue =>
                        CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                            .MissingFreshGlobalValue,
                    CapacityFinalCalleeLargeRightFieldThresholdGateOutcome
                        .UnsignedBelowBranchTarget =>
                        CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                            .UnsignedBelowBranchTarget,
                    CapacityFinalCalleeLargeRightFieldThresholdGateOutcome
                        .UnsignedNotBelowFallthrough =>
                        CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                            .UnsignedNotBelowFallthrough,
                    _ => throw new InvalidOperationException(
                        "A nonzero right-field handoff must provide entry ECX.")
                };
                break;
            }

            default:
                throw new InvalidOperationException("Unhandled right-field gate outcome.");
        }

        return new(capture, parent, threshold, outcome)
        {
            Reads = reads.ToImmutable(),
            ExactCleanBranchToThresholdGate = exactBranch,
            BranchReached = branchReached,
            BranchTaken = branchTaken,
            BranchFlags = branchFlags,
            BranchSelectedDestinationRawFileOffset = branchDestination,
            ExactCleanExitAdmitted = exactCleanExit,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags Convert(
        CapacityFinalCalleeLargeRightFieldGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags Convert(
        CapacityFinalCalleeLargeRightFieldThresholdGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleInstructionEvidence> BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRightFieldThresholdLifecycleInstructionEvidence>();
        foreach (CapacityFinalCalleeLargeRightFieldGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeRightFieldGateProjection.Instructions)
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

        foreach (CapacityFinalCalleeLargeRightFieldThresholdGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeRightFieldThresholdGateProjection.Instructions)
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
