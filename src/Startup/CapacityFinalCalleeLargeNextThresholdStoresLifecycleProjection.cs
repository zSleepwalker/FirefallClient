using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeNextThresholdStoresLifecycleCapture(
    uint? EntryECX,
    uint? FreshGlobalValue,
    uint? ChildEntryEAX);

public enum CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
{
    MissingBothParentCompareOperands,
    MissingParentEntryECX,
    MissingParentFreshGlobalValue,
    ParentUnsignedBelowTo8E76,
    ChildMissingFirstStoreAddress,
    ChildMissingFirstStoreValue,
    ChildCompleted,
    ParentLifecycleIncomplete
}

public readonly record struct CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperation(
    uint RawFileOffset,
    CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperationKind Kind,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Committed);

public enum CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperationKind
{
    Read,
    Write
}

public sealed record CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan(
    CapacityFinalCalleeLargeNextThresholdStoresLifecycleCapture Capture,
    CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeLargeNextFieldThresholdGatePlan ParentGate { get; init; } = null!;
    public CapacityFinalCalleeLargeNextPairedStoresPlan? ChildStores { get; init; }
    public bool ChildReached => ChildStores is not null;
    public bool NoChildReached => !ChildReached;
    public bool ChildEntryECXDerivedFromParentEntryECX { get; init; }
    public uint? ChildEntryECX { get; init; }
    public bool UnreachedChildEntryEAXCaptureProvided { get; init; }
    public bool ChildEntryEAXCaptured => ChildStores?.EntryEAXCaptured == true;
    public uint? ChildEntryEAX => ChildStores?.Capture.EntryEAX;
    public bool ParentCompareReached => ParentGate.CompareReached;
    public uint? ParentCompareLeft => ParentGate.CompareLeft;
    public uint? ParentCompareRight => ParentGate.CompareRight;
    public CapacityFinalCalleeLargeNextFieldThresholdGateFlags? ParentCompareFlags =>
        ParentGate.CompareFlags;
    public bool ParentBranchReached => ParentGate.BranchReached;
    public bool? ParentBranchTaken => ParentGate.BranchTaken;
    public uint? ParentBranchSelectedDestinationRawFileOffset =>
        ParentGate.BranchSelectedDestinationRawFileOffset;
    public bool ParentExactNotBelowHandoff { get; init; }
    public CapacityFinalCalleeLargeNextFieldThresholdGateFlags? FlagsAtStop =>
        ParentGate.CompareFlags;
    public bool FlagsUnaffectedByChildStores =>
        ChildStores is { FlagsUnaffected: true };
    public bool ParentCompareFlagsRemainThroughChildStores =>
        ChildStores is { FlagsUnaffected: true } && ParentGate.CompareFlags.HasValue;
    public ImmutableArray<CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperation>
        MemoryOperations { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperation>.Empty;
    public ImmutableArray<uint> ReachedInstructions { get; init; } = ImmutableArray<uint>.Empty;
    public bool BoundaryInstructionExecuted => false;
    public bool FreshReadReachedAfterStores => false;
    public bool ChildDestinationEntered => false;
    public bool ControlFlowBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the exact not-below threshold handoff at 929E with its two ordered stores.
/// </summary>
public static class CapacityFinalCalleeLargeNextThresholdStoresLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeNextFieldThresholdGateProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeNextPairedStoresProjection.EntryRawFileOffset;
    public const uint ParentBelowDestinationRawFileOffset =
        CapacityFinalCalleeLargeNextFieldThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint ParentNotBelowDestinationRawFileOffset =
        CapacityFinalCalleeLargeNextFieldThresholdGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeLargeNextPairedStoresProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan Project(
        CapacityFinalCalleeLargeNextThresholdStoresLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeNextFieldThresholdGatePlan parent =
            CapacityFinalCalleeLargeNextFieldThresholdGateProjection.Project(new(
                capture.EntryECX,
                capture.FreshGlobalValue));

        bool exactNotBelowHandoff =
            parent.Outcome
                == CapacityFinalCalleeLargeNextFieldThresholdGateOutcome
                    .UnsignedNotBelowFallthrough
            && parent.ExactCleanCompletion
            && parent.CompareReached
            && parent.BranchReached
            && parent.BranchTaken == false
            && parent.BranchSelectedDestinationRawFileOffset
                == ParentNotBelowDestinationRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
            && parent.CompareLeft == capture.EntryECX
            && parent.CompareRight == capture.FreshGlobalValue
            && parent.CompareFlags.HasValue;

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan Finish(
            CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeLargeNextPairedStoresPlan? childStores = null,
            bool controlFlowBoundedCompletion = false,
            bool exactCleanCompletion = false) =>
            new(capture, outcome)
            {
                ParentGate = parent,
                ChildStores = childStores,
                ChildEntryECXDerivedFromParentEntryECX =
                    childStores is not null
                    && childStores.Capture.EntryECX == capture.EntryECX,
                ChildEntryECX = childStores?.Capture.EntryECX,
                UnreachedChildEntryEAXCaptureProvided =
                    childStores is null && capture.ChildEntryEAX.HasValue,
                ParentExactNotBelowHandoff = exactNotBelowHandoff,
                MemoryOperations = BuildMemoryOperations(parent, childStores),
                ReachedInstructions = BuildReachedInstructions(parent, childStores),
                ControlFlowBoundedCompletion = controlFlowBoundedCompletion,
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!exactNotBelowHandoff)
        {
            if (parent.Outcome
                    == CapacityFinalCalleeLargeNextFieldThresholdGateOutcome
                        .UnsignedBelowBranchTarget
                && parent.ExactCleanCompletion
                && parent.BranchReached
                && parent.BranchTaken == true
                && parent.BranchSelectedDestinationRawFileOffset
                    == ParentBelowDestinationRawFileOffset
                && parent.StopAtRawFileOffsetExclusive
                    == ParentBelowDestinationRawFileOffset)
            {
                bool noUnreachedChildCapture = !capture.ChildEntryEAX.HasValue;
                return Finish(
                    CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                        .ParentUnsignedBelowTo8E76,
                    ParentBelowDestinationRawFileOffset,
                    controlFlowBoundedCompletion: true,
                    exactCleanCompletion: noUnreachedChildCapture);
            }

            return Finish(MapParentOutcome(parent.Outcome), parent.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeLargeNextPairedStoresPlan child =
            CapacityFinalCalleeLargeNextPairedStoresProjection.Project(new(
                EntryEAX: capture.ChildEntryEAX,
                EntryECX: capture.EntryECX));

        if (child.Outcome
            == CapacityFinalCalleeLargeNextPairedStoresOutcome.MissingFirstStoreAddress)
        {
            return Finish(
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .ChildMissingFirstStoreAddress,
                child.StopAtRawFileOffsetExclusive,
                child);
        }

        if (child.Outcome
            == CapacityFinalCalleeLargeNextPairedStoresOutcome.MissingFirstStoreValue)
        {
            return Finish(
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .ChildMissingFirstStoreValue,
                child.StopAtRawFileOffsetExclusive,
                child);
        }

        bool exactStores =
            child.Outcome == CapacityFinalCalleeLargeNextPairedStoresOutcome.Completed
            && child.ExactCleanCompletion
            && child.FirstStoreCommitted
            && child.SecondStoreReached
            && child.SecondStoreCommitted
            && child.Writes.Length == 2
            && child.Writes[0].RawFileOffset
                == CapacityFinalCalleeLargeNextPairedStoresProjection.FirstStoreRawFileOffset
            && child.Writes[1].RawFileOffset
                == CapacityFinalCalleeLargeNextPairedStoresProjection.SecondStoreRawFileOffset
            && child.StopAtRawFileOffsetExclusive == BoundaryRawFileOffsetExclusive
            && !child.BoundaryInstructionExecuted
            && !child.FreshReadReached
            && child.Capture.EntryECX == capture.EntryECX
            && child.Capture.EntryEAX == capture.ChildEntryEAX;
        if (!exactStores)
        {
            return Finish(
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .ParentLifecycleIncomplete,
                child.StopAtRawFileOffsetExclusive,
                child);
        }

        return Finish(
            CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome.ChildCompleted,
            BoundaryRawFileOffsetExclusive,
            child,
            controlFlowBoundedCompletion: true,
            exactCleanCompletion: true);
    }

    private static ImmutableArray<
        CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperation>
        BuildMemoryOperations(
            CapacityFinalCalleeLargeNextFieldThresholdGatePlan parent,
            CapacityFinalCalleeLargeNextPairedStoresPlan? child)
    {
        var operations =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperation>();
        foreach (CapacityFinalCalleeLargeNextFieldThresholdGateRead read in parent.Reads)
        {
            operations.Add(new(
                read.RawFileOffset,
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperationKind.Read,
                read.Address,
                read.AddressCaptured,
                read.Value,
                read.ValueCaptured,
                Committed: false));
        }

        if (child is not null && child.FirstStoreReached)
        {
            operations.Add(new(
                CapacityFinalCalleeLargeNextPairedStoresProjection.FirstStoreRawFileOffset,
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperationKind.Write,
                child.FirstStoreAddress,
                child.FirstStoreAddress.HasValue,
                child.FirstStoreValue,
                child.FirstStoreValue.HasValue,
                child.FirstStoreCommitted));

            if (child.SecondStoreReached)
            {
                operations.Add(new(
                    CapacityFinalCalleeLargeNextPairedStoresProjection.SecondStoreRawFileOffset,
                    CapacityFinalCalleeLargeNextThresholdStoresLifecycleMemoryOperationKind.Write,
                    child.SecondStoreAddress,
                    child.SecondStoreAddress.HasValue,
                    child.SecondStoreValue,
                    child.SecondStoreValue.HasValue,
                    child.SecondStoreCommitted));
            }
        }

        return operations.ToImmutable();
    }

    private static ImmutableArray<uint> BuildReachedInstructions(
        CapacityFinalCalleeLargeNextFieldThresholdGatePlan parent,
        CapacityFinalCalleeLargeNextPairedStoresPlan? child)
    {
        var instructions = ImmutableArray.CreateBuilder<uint>();
        if (parent.CompareInstructionReached)
        {
            instructions.Add(
                CapacityFinalCalleeLargeNextFieldThresholdGateProjection.CompareRawFileOffset);
        }

        if (parent.BranchReached)
        {
            instructions.Add(
                CapacityFinalCalleeLargeNextFieldThresholdGateProjection.BranchRawFileOffset);
        }

        if (child?.FirstStoreReached == true)
        {
            instructions.Add(
                CapacityFinalCalleeLargeNextPairedStoresProjection.FirstStoreRawFileOffset);
        }

        if (child?.SecondStoreReached == true)
        {
            instructions.Add(
                CapacityFinalCalleeLargeNextPairedStoresProjection.SecondStoreRawFileOffset);
        }

        return instructions.ToImmutable();
    }

    private static CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
        MapParentOutcome(CapacityFinalCalleeLargeNextFieldThresholdGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeLargeNextFieldThresholdGateOutcome.MissingBothCompareOperands =>
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .MissingBothParentCompareOperands,
            CapacityFinalCalleeLargeNextFieldThresholdGateOutcome.MissingEntryECX =>
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .MissingParentEntryECX,
            CapacityFinalCalleeLargeNextFieldThresholdGateOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .MissingParentFreshGlobalValue,
            _ => CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                .ParentLifecycleIncomplete
        };
}
