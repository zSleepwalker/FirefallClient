using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedMaskAndLifecycleCapture(
    CapacityFinalCalleeLargeSavedMaskShiftCapture ParentCapture,
    uint? FreshGlobalDword);

public enum CapacityFinalCalleeLargeSavedMaskAndLifecycleOutcome
{
    ParentIncomplete,
    ChildMissingFreshGlobalDword,
    ChildCompletedTo92BD
}

public enum CapacityFinalCalleeLargeSavedMaskAndLifecycleStage
{
    ParentSavedMask,
    ChildGlobalAnd
}

public enum CapacityFinalCalleeLargeSavedMaskAndLifecycleAccessKind
{
    Read,
    Write
}

public readonly record struct CapacityFinalCalleeLargeSavedMaskAndLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeLargeSavedMaskAndLifecycleRead(
    CapacityFinalCalleeLargeSavedMaskAndLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSavedMaskAndLifecycleWrite(
    CapacityFinalCalleeLargeSavedMaskAndLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSavedMaskAndLifecycleAccess(
    CapacityFinalCalleeLargeSavedMaskAndLifecycleStage Stage,
    CapacityFinalCalleeLargeSavedMaskAndLifecycleAccessKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public sealed record CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan(
    CapacityFinalCalleeLargeSavedMaskAndLifecycleCapture Capture,
    CapacityFinalCalleeLargeSavedMaskShiftPlan ParentPlan,
    CapacityFinalCalleeLargeMaskAndStorePlan? ChildPlan,
    CapacityFinalCalleeLargeSavedMaskAndLifecycleOutcome Outcome)
{
    public bool ChildReached { get; init; }
    public uint? ChildEAX { get; init; }
    public uint? ChildEAXSourceRawFileOffset { get; init; }
    public uint? ChildFreshGlobalAddress { get; init; }
    public bool ChildFreshGlobalReadReached { get; init; }
    public bool ChildFreshGlobalCaptured { get; init; }
    public uint? ChildFreshGlobalDword { get; init; }
    public bool UnreachedFreshGlobalDwordCaptureProvided { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeSavedMaskAndLifecycleRead>
        OrderedReads { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedMaskAndLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedMaskAndLifecycleWrite>
        OrderedWrites { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedMaskAndLifecycleWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedMaskAndLifecycleAccess>
        OrderedMemoryAccesses { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedMaskAndLifecycleAccess>.Empty;
    public CapacityFinalCalleeLargeSavedMaskShiftFlags? ParentFlagsAtStop =>
        ParentPlan.FlagsAtStop;
    public CapacityFinalCalleeLargeSavedMaskAndLifecycleFlags? FlagsAtStop { get; init; }
    public bool ParentFlagsPreservedThroughChild { get; init; }
    public bool ChildAndFlagsReplacedParentFlags { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool CommonTailEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the bounded saved-mask shift/NOT with its independent global AND.
/// </summary>
public static class CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeSavedMaskShiftProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeMaskAndStoreProjection.EntryRawFileOffset;
    public const uint GlobalVirtualAddress =
        CapacityFinalCalleeLargeMaskAndStoreProjection.GlobalVirtualAddress;
    public const uint CompletedStopRawFileOffsetExclusive =
        CapacityFinalCalleeLargeMaskAndStoreProjection.JumpTargetRawFileOffset;

    public static CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan Project(
        CapacityFinalCalleeLargeSavedMaskAndLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeSavedMaskShiftPlan parent =
            CapacityFinalCalleeLargeSavedMaskShiftProjection.Project(
                capture.ParentCapture);
        var reads =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedMaskAndLifecycleRead>();
        var writes =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedMaskAndLifecycleWrite>();
        var accesses =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedMaskAndLifecycleAccess>();

        foreach (CapacityFinalCalleeLargeSavedMaskShiftRead read in parent.OrderedReads)
        {
            var aggregate = new CapacityFinalCalleeLargeSavedMaskAndLifecycleRead(
                CapacityFinalCalleeLargeSavedMaskAndLifecycleStage.ParentSavedMask,
                read.RawFileOffset,
                read.Address,
                AddressCaptured: read.Address.HasValue,
                ValueCaptured: read.ValueCaptured,
                Value: read.Value);
            reads.Add(aggregate);
            accesses.Add(new(
                aggregate.Stage,
                CapacityFinalCalleeLargeSavedMaskAndLifecycleAccessKind.Read,
                aggregate.RawFileOffset,
                aggregate.Address,
                aggregate.AddressCaptured,
                aggregate.ValueCaptured,
                aggregate.Value));
        }

        bool exactParentHandoff =
            parent.Outcome
                == CapacityFinalCalleeLargeSavedMaskShiftOutcome.BoundedBeforeGlobalAnd
            && parent.ExactCleanExitAdmitted
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
            && parent.NotResult.HasValue;

        if (!exactParentHandoff)
        {
            return new(
                capture,
                parent,
                ChildPlan: null,
                Outcome: CapacityFinalCalleeLargeSavedMaskAndLifecycleOutcome.ParentIncomplete)
            {
                OrderedReads = reads.ToImmutable(),
                OrderedMemoryAccesses = accesses.ToImmutable(),
                FlagsAtStop = ConvertFlags(parent.FlagsAtStop),
                UnreachedFreshGlobalDwordCaptureProvided =
                    capture.FreshGlobalDword.HasValue,
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
        }

        uint childEax = parent.NotResult!.Value;
        CapacityFinalCalleeLargeMaskAndStorePlan child =
            CapacityFinalCalleeLargeMaskAndStoreProjection.Project(new(
                EntryEAX: childEax,
                FreshGlobalDword: capture.FreshGlobalDword));

        foreach (CapacityFinalCalleeLargeMaskAndStoreRead read in child.Reads)
        {
            var aggregate = new CapacityFinalCalleeLargeSavedMaskAndLifecycleRead(
                CapacityFinalCalleeLargeSavedMaskAndLifecycleStage.ChildGlobalAnd,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value);
            reads.Add(aggregate);
            accesses.Add(new(
                aggregate.Stage,
                CapacityFinalCalleeLargeSavedMaskAndLifecycleAccessKind.Read,
                aggregate.RawFileOffset,
                aggregate.Address,
                aggregate.AddressCaptured,
                aggregate.ValueCaptured,
                aggregate.Value));
        }

        foreach (CapacityFinalCalleeLargeMaskAndStoreWrite write in child.Writes)
        {
            var aggregate = new CapacityFinalCalleeLargeSavedMaskAndLifecycleWrite(
                CapacityFinalCalleeLargeSavedMaskAndLifecycleStage.ChildGlobalAnd,
                write.RawFileOffset,
                write.Address,
                write.AddressCaptured,
                write.ValueCaptured,
                write.Value);
            writes.Add(aggregate);
            accesses.Add(new(
                aggregate.Stage,
                CapacityFinalCalleeLargeSavedMaskAndLifecycleAccessKind.Write,
                aggregate.RawFileOffset,
                aggregate.Address,
                aggregate.AddressCaptured,
                aggregate.ValueCaptured,
                aggregate.Value));
        }

        bool childAndExecuted = child.AndExecuted;
        CapacityFinalCalleeLargeSavedMaskAndLifecycleOutcome outcome =
            child.Outcome == CapacityFinalCalleeLargeMaskAndStoreOutcome.Completed
                ? CapacityFinalCalleeLargeSavedMaskAndLifecycleOutcome
                    .ChildCompletedTo92BD
                : CapacityFinalCalleeLargeSavedMaskAndLifecycleOutcome
                    .ChildMissingFreshGlobalDword;

        return new(capture, parent, child, outcome)
        {
            ChildReached = true,
            ChildEAX = childEax,
            ChildEAXSourceRawFileOffset =
                CapacityFinalCalleeLargeSavedMaskShiftProjection.NotRawFileOffset,
            ChildFreshGlobalAddress = child.GlobalReadAddress,
            ChildFreshGlobalReadReached = child.GlobalReadReached,
            ChildFreshGlobalCaptured = child.FreshGlobalDwordCaptured,
            ChildFreshGlobalDword = child.GlobalReadValue,
            OrderedReads = reads.ToImmutable(),
            OrderedWrites = writes.ToImmutable(),
            OrderedMemoryAccesses = accesses.ToImmutable(),
            FlagsAtStop = childAndExecuted
                ? ConvertFlags(child.AndFlags)
                : ConvertFlags(parent.FlagsAtStop),
            ParentFlagsPreservedThroughChild = !childAndExecuted,
            ChildAndFlagsReplacedParentFlags = childAndExecuted,
            ExactCleanExitAdmitted =
                child.Outcome == CapacityFinalCalleeLargeMaskAndStoreOutcome.Completed
                && child.ExactStopAtJumpTarget
                && child.JumpReached
                && !child.CommonTailEntered
                && child.JumpTargetRawFileOffset
                    == CompletedStopRawFileOffsetExclusive
                && child.StopAtRawFileOffsetExclusive
                    == CompletedStopRawFileOffsetExclusive,
            StopAtRawFileOffsetExclusive =
                child.StopAtRawFileOffsetExclusive
        };
    }

    private static CapacityFinalCalleeLargeSavedMaskAndLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeLargeSavedMaskShiftFlags? flags) =>
        flags is CapacityFinalCalleeLargeSavedMaskShiftFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeSavedMaskAndLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeLargeMaskAndStoreFlags? flags) =>
        flags is CapacityFinalCalleeLargeMaskAndStoreFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;
}
