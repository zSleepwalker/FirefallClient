using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleCapture(
    uint? EntryEAX,
    uint? EntryECX,
    uint? EntryEDX,
    uint? FreshEDXPlus10,
    uint? ChildFreshGlobalValue);

public enum CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
{
    MissingBothParentFirstStoreOperands,
    MissingParentEntryEAX,
    MissingParentEntryECX,
    MissingParentEntryEDX,
    MissingParentFreshEDXPlus10,
    ParentZeroTo92A4,
    MissingChildFreshGlobalValue,
    ChildUnsignedBelowTo8E76,
    ChildUnsignedNotBelowTo929E,
    ParentLifecycleIncomplete
}

public readonly record struct CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public enum CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlagsSource
{
    ParentTest,
    ChildCompare
}

public enum CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
{
    Write,
    Read
}

public readonly record struct CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperation(
    uint RawFileOffset,
    CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind Kind,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Committed);

public sealed record CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan(
    CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleCapture Capture,
    CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeLargeBacklinkNextFieldGatePlan ParentGate { get; init; } = null!;
    public CapacityFinalCalleeLargeNextFieldThresholdGatePlan? ChildGate { get; init; }
    public bool ChildReached => ChildGate is not null;
    public bool NoChildReached => !ChildReached;
    public bool ParentStoredEntryECXCaptured => Capture.EntryECX.HasValue;
    public uint? ParentStoredEntryECX => Capture.EntryECX;
    public bool ParentNonzeroExactHandoff { get; init; }
    public bool ParentFreshECXCaptured => ParentGate.FreshReadValueCaptured;
    public uint? ParentFreshECX => ParentGate.FreshECX;
    public bool ChildECXDerivedFromParentFreshECX { get; init; }
    public uint? ChildEntryECX { get; init; }
    public bool ChildEntryECXDiffersFromParentStoredEntryECX =>
        ChildECXDerivedFromParentFreshECX
        && ChildEntryECX != ParentStoredEntryECX;
    public bool ChildFreshGlobalReadReached => ChildGate?.GlobalReadReached == true;
    public uint? ChildFreshGlobalAddress => ChildGate?.GlobalAddress;
    public bool ChildFreshGlobalValueCaptured => ChildGate?.FreshGlobalValueCaptured == true;
    public uint? ChildFreshGlobalValue => ChildGate?.FreshGlobalValue;
    public bool UnreachedParentEntryEDXCaptureProvided =>
        ParentGate.UnreachedEntryEDXCaptureProvided;
    public bool UnreachedParentFreshEDXPlus10CaptureProvided =>
        ParentGate.UnreachedFreshEDXPlus10CaptureProvided;
    public bool UnusableParentFreshEDXPlus10CaptureProvided =>
        ParentGate.UnusableFreshEDXPlus10CaptureProvided;
    public bool UnreachedChildFreshGlobalValueCaptureProvided { get; init; }
    public bool UnreachedChildLateCaptureProvided =>
        UnreachedChildFreshGlobalValueCaptureProvided;
    public bool FirstStoreAttempted => ParentGate.FirstStoreAttempted;
    public uint? FirstStoreAddress => ParentGate.FirstStoreAddress;
    public bool FirstStoreAddressCaptured => ParentGate.FirstStoreAddressCaptured;
    public uint? FirstStoreValue => ParentGate.FirstStoreValue;
    public bool FirstStoreValueCaptured => ParentGate.FirstStoreValueCaptured;
    public bool FirstStoreCommitted => ParentGate.FirstStoreCommitted;
    public bool FreshReadReached => ParentGate.FreshReadReached;
    public uint? FreshReadAddress => ParentGate.FreshReadAddress;
    public bool FreshReadAddressCaptured => ParentGate.FreshReadAddressCaptured;
    public bool ParentTestReached => ParentGate.TestReached;
    public CapacityFinalCalleeLargeBacklinkNextFieldGateFlags? ParentTestFlags =>
        ParentGate.TestFlags;
    public bool ChildCompareReached => ChildGate?.CompareReached == true;
    public uint? ChildCompareLeft => ChildGate?.CompareLeft;
    public uint? ChildCompareRight => ChildGate?.CompareRight;
    public uint? ChildCompareResult => ChildGate?.CompareResult;
    public CapacityFinalCalleeLargeNextFieldThresholdGateFlags? ChildCompareFlags =>
        ChildGate?.CompareFlags;
    public bool ChildBranchReached => ChildGate?.BranchReached == true;
    public bool? ChildBranchTaken => ChildGate?.BranchTaken;
    public uint? ChildBranchSelectedDestinationRawFileOffset =>
        ChildGate?.BranchSelectedDestinationRawFileOffset;
    public CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlags? FlagsAtStop =>
        ChildGate is { CompareReached: true, CompareFlags: { } childFlags }
            ? Convert(childFlags)
            : ParentGate.TestFlags is { } parentFlags
                ? Convert(parentFlags)
                : null;
    public CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlagsSource?
        FlagsAtStopSource =>
            ChildGate?.CompareReached == true
                ? CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlagsSource.ChildCompare
                : ParentGate.TestReached
                    ? CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlagsSource.ParentTest
                    : null;
    public bool ParentTestFlagsRemainUntilChildCompare =>
        ParentGate.TestReached && ChildGate?.CompareReached != true;
    public ImmutableArray<CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperation>
        MemoryOperations { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperation>.Empty;
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public bool ChildPairedStoreReached => false;
    public bool ChildDestinationEntered => false;
    public bool ControlFlowBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlags Convert(
        CapacityFinalCalleeLargeBacklinkNextFieldGateFlags flags) =>
        new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlags Convert(
        CapacityFinalCalleeLargeNextFieldThresholdGateFlags flags) =>
        new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign,
            flags.Overflow);
}

/// <summary>
/// Composes the exact nonzero backlink path at 9292 with its independent global
/// threshold comparison, without forwarding or coalescing any memory access.
/// </summary>
public static class CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeNextFieldThresholdGateProjection.EntryRawFileOffset;
    public const uint ParentZeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.JzDestinationRawFileOffset;
    public const uint ChildBelowDestinationRawFileOffset =
        CapacityFinalCalleeLargeNextFieldThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint ChildNotBelowDestinationRawFileOffset =
        CapacityFinalCalleeLargeNextFieldThresholdGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeLargeNextFieldThresholdGateProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan Project(
        CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan parent =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                capture.EntryEAX,
                capture.EntryECX,
                capture.EntryEDX,
                capture.FreshEDXPlus10));

        bool parentNonzeroAtExactHandoff =
            parent.Outcome
                == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.NonzeroTo9292
            && parent.ExactCleanCompletion
            && parent.TestReached
            && parent.JzReached
            && parent.JzTaken == false
            && parent.JzSelectedDestinationRawFileOffset == ChildEntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
            && parent.FreshReadValueCaptured
            && parent.FreshECX.HasValue;

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan Finish(
            CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeLargeNextFieldThresholdGatePlan? child = null,
            bool controlFlowBoundedCompletion = false,
            bool exactCleanCompletion = false) =>
            new(capture, outcome)
            {
                ParentGate = parent,
                ChildGate = child,
                ParentNonzeroExactHandoff = parentNonzeroAtExactHandoff,
                ChildECXDerivedFromParentFreshECX =
                    child is not null && parent.FreshReadValueCaptured
                    && child.Capture.EntryECX == parent.FreshECX,
                ChildEntryECX = child?.Capture.EntryECX,
                UnreachedChildFreshGlobalValueCaptureProvided =
                    child is null && capture.ChildFreshGlobalValue.HasValue,
                MemoryOperations = BuildMemoryOperations(parent, child),
                ReachedInstructions = BuildReachedInstructions(parent, child),
                ControlFlowBoundedCompletion = controlFlowBoundedCompletion,
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!parentNonzeroAtExactHandoff)
        {
            if (parent.Outcome
                == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.ZeroTo92A4
                && parent.ExactCleanCompletion
                && parent.JzReached
                && parent.JzTaken == true
                && parent.JzSelectedDestinationRawFileOffset
                    == ParentZeroDestinationRawFileOffset
                && parent.StopAtRawFileOffsetExclusive
                    == ParentZeroDestinationRawFileOffset)
            {
                bool noUnreachedParentCapture =
                    !parent.UnreachedEntryEDXCaptureProvided
                    && !parent.UnreachedFreshEDXPlus10CaptureProvided
                    && !parent.UnusableFreshEDXPlus10CaptureProvided;
                bool noUnreachedChildCapture = !capture.ChildFreshGlobalValue.HasValue;
                return Finish(
                    CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                        .ParentZeroTo92A4,
                    ParentZeroDestinationRawFileOffset,
                    controlFlowBoundedCompletion: true,
                    exactCleanCompletion: noUnreachedParentCapture
                        && noUnreachedChildCapture);
            }

            return Finish(
                MapParentOutcome(parent.Outcome),
                parent.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeLargeNextFieldThresholdGatePlan child =
            CapacityFinalCalleeLargeNextFieldThresholdGateProjection.Project(new(
                EntryECX: parent.FreshECX,
                FreshGlobalValue: capture.ChildFreshGlobalValue));
        if (child.Outcome
            == CapacityFinalCalleeLargeNextFieldThresholdGateOutcome.MissingFreshGlobalValue)
        {
            return Finish(
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingChildFreshGlobalValue,
                child.StopAtRawFileOffsetExclusive,
                child);
        }

        if (!child.CompareReached || !child.BranchReached
            || !child.ExactCleanCompletion)
        {
            return Finish(
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .ParentLifecycleIncomplete,
                child.StopAtRawFileOffsetExclusive,
                child);
        }

        bool below = child.BranchTaken == true
            && child.BranchSelectedDestinationRawFileOffset == ChildBelowDestinationRawFileOffset
            && child.Outcome
                == CapacityFinalCalleeLargeNextFieldThresholdGateOutcome
                    .UnsignedBelowBranchTarget;
        bool notBelow = child.BranchTaken == false
            && child.BranchSelectedDestinationRawFileOffset
                == ChildNotBelowDestinationRawFileOffset
            && child.Outcome
                == CapacityFinalCalleeLargeNextFieldThresholdGateOutcome
                    .UnsignedNotBelowFallthrough;
        bool exactChildStop =
            child.StopAtRawFileOffsetExclusive
                == child.BranchSelectedDestinationRawFileOffset
            && !child.MemoryWriteReached
            && child.EntryECXCaptured
            && child.CompareLeft == parent.FreshECX
            && child.CompareRight == capture.ChildFreshGlobalValue;
        if (!exactChildStop || (!below && !notBelow))
        {
            return Finish(
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .ParentLifecycleIncomplete,
                child.StopAtRawFileOffsetExclusive,
                child);
        }

        return Finish(
            below
                ? CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .ChildUnsignedBelowTo8E76
                : CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .ChildUnsignedNotBelowTo929E,
            child.StopAtRawFileOffsetExclusive,
            child,
            controlFlowBoundedCompletion: true,
            exactCleanCompletion:
                !parent.UnreachedEntryEDXCaptureProvided
                && !parent.UnreachedFreshEDXPlus10CaptureProvided
                && !parent.UnusableFreshEDXPlus10CaptureProvided
                && child.EntryECXCaptured);
    }

    private static ImmutableArray<
        CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperation>
        BuildMemoryOperations(
            CapacityFinalCalleeLargeBacklinkNextFieldGatePlan parent,
            CapacityFinalCalleeLargeNextFieldThresholdGatePlan? child)
    {
        var operations =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperation>();
        foreach (CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperation operation
            in parent.MemoryOperations)
        {
            operations.Add(new(
                operation.RawFileOffset,
                operation.Kind
                    == CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind.Write
                        ? CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
                            .Write
                        : CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
                            .Read,
                operation.Address,
                operation.AddressCaptured,
                operation.Value,
                operation.ValueCaptured,
                operation.Committed));
        }

        if (child is not null)
        {
            foreach (CapacityFinalCalleeLargeNextFieldThresholdGateRead read in child.Reads)
            {
                operations.Add(new(
                    read.RawFileOffset,
                    CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
                        .Read,
                    read.Address,
                    read.AddressCaptured,
                    read.Value,
                    read.ValueCaptured,
                    Committed: false));
            }
        }

        return operations.ToImmutable();
    }

    private static ImmutableArray<uint> BuildReachedInstructions(
        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan parent,
        CapacityFinalCalleeLargeNextFieldThresholdGatePlan? child)
    {
        var instructions = ImmutableArray.CreateBuilder<uint>();
        instructions.AddRange(parent.ReachedInstructions);
        if (child?.CompareInstructionReached == true)
        {
            instructions.Add(
                CapacityFinalCalleeLargeNextFieldThresholdGateProjection.CompareRawFileOffset);
        }

        if (child?.BranchReached == true)
        {
            instructions.Add(
                CapacityFinalCalleeLargeNextFieldThresholdGateProjection.BranchRawFileOffset);
        }

        return instructions.ToImmutable();
    }

    private static CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
        MapParentOutcome(CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.MissingBothFirstStoreOperands =>
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingBothParentFirstStoreOperands,
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.MissingEntryEAXForFirstStore =>
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingParentEntryEAX,
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.MissingEntryECXForFirstStore =>
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingParentEntryECX,
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.MissingEntryEDXForFreshRead =>
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingParentEntryEDX,
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.MissingFreshEDXPlus10 =>
                CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingParentFreshEDXPlus10,
            _ => CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                .ParentLifecycleIncomplete
        };
}
