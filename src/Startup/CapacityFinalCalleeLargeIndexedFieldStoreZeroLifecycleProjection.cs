using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleCapture(
    CapacityFinalCalleeLargeIndexedFieldStoreLifecycleCapture ParentCapture);

public enum CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome
{
    ParentLifecycleIncomplete,
    ParentStoreValueMissing,
    ZeroTo92BD,
    NonzeroTo927C
}

public enum CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind
{
    Compare,
    ConditionalBranch,
    Store,
    UnconditionalJump,
    Test,
    ConditionalZeroBranch
}

public readonly record struct CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEvent(
    CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind Kind,
    uint RawFileOffset,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeIndexedFieldGateFlags? ParentFlags = null,
    CapacityFinalCalleeLargePostStoreZeroGateFlags? TestFlags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null,
    uint? Address = null,
    uint? Value = null,
    bool? Committed = null);

public readonly record struct CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan(
    CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleCapture Capture,
    CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome Outcome,
    CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan ParentLifecycle)
{
    public CapacityFinalCalleeLargeIndexedFieldGatePlan ParentGate =>
        ParentLifecycle.ParentGate;
    public CapacityFinalCalleeLargePostStoreZeroGatePlan? PostStoreZeroGate { get; init; }
    public bool ChildGateEntered => PostStoreZeroGate is not null;
    public bool ChildEAXDerivedFromParentStore =>
        PostStoreZeroGate is not null
        && ParentLifecycle.StoreValueCaptured
        && PostStoreZeroGate.Capture.EntryEAX == ParentLifecycle.StoreValue;
    public uint? ChildEAX =>
        ChildGateEntered ? PostStoreZeroGate!.Capture.EntryEAX : null;
    public CapacityFinalCalleeLargeIndexedFieldGateFlags? ParentFlagsAtStore =>
        ParentLifecycle.FlagsAtStop;
    public CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleFlags? FlagsAtStop =>
        PostStoreZeroGate?.FlagsAtStop is { } childFlags
            ? new(childFlags.Carry, childFlags.Parity, childFlags.AuxiliaryCarry,
                childFlags.Zero, childFlags.Sign, childFlags.Overflow)
            : ParentLifecycle.FlagsAtStop is { } parentFlags
                ? new(parentFlags.Carry, parentFlags.Parity, parentFlags.AuxiliaryCarry,
                    parentFlags.Zero, parentFlags.Sign, parentFlags.Overflow)
                : null;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateRead> Reads =>
        ParentLifecycle.Reads;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldStoreLifecycleWrite> Writes =>
        ParentLifecycle.Writes;
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEvent> Events
    {
        get; init;
    } = ImmutableArray<CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEvent>.Empty;
    public bool ExactBoundedCompletion { get; init; }
    public bool ContinuationEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the indexed-field stores through 9278 with the following TEST/JZ, stopping at its destination.
/// </summary>
public static class CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.EntryVirtualAddress;
    public const uint PostStoreZeroGateRawFileOffset =
        CapacityFinalCalleeLargePostStoreZeroGateProjection.EntryRawFileOffset;
    public const uint ContinuationBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeLargePostStoreZeroGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ZeroBranchTargetRawFileOffset =
        CapacityFinalCalleeLargePostStoreZeroGateProjection.BranchTargetRawFileOffset;
    public const uint NonzeroFallthroughRawFileOffset =
        CapacityFinalCalleeLargePostStoreZeroGateProjection.FallthroughRawFileOffset;

    public const string CapturedWindowBytes =
        CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.CapturedWindowBytes
        + " "
        + CapacityFinalCalleeLargePostStoreZeroGateProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleInstructionEvidence>
        Instructions { get; } =
        CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Instructions
            .Select(instruction => new
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleInstructionEvidence(
                    instruction.Observation,
                    instruction.RawFileOffset,
                    instruction.FlatIdaAddress,
                    instruction.VirtualAddress,
                    instruction.Bytes,
                    instruction.BranchTargetRawFileOffset,
                    instruction.BranchTargetVirtualAddress))
            .Concat(
                CapacityFinalCalleeLargePostStoreZeroGateProjection.Instructions
                    .Select(instruction => new
                        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleInstructionEvidence(
                            instruction.Observation,
                            instruction.RawFileOffset,
                            instruction.FlatIdaAddress,
                            instruction.VirtualAddress,
                            instruction.Bytes,
                            instruction.BranchTargetRawFileOffset,
                            instruction.BranchTargetVirtualAddress)))
            .ToImmutableArray();

    public static CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan Project(
        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan parent =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(
                capture.ParentCapture);
        var reached = ImmutableArray.CreateBuilder<uint>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEvent>();

        foreach (CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEvent parentEvent
            in parent.Events)
        {
            CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind kind =
                parentEvent.Kind switch
                {
                    CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.Compare =>
                        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind.Compare,
                    CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.ConditionalBranch =>
                        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind
                            .ConditionalBranch,
                    CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.Store =>
                        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind.Store,
                    CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.UnconditionalJump =>
                        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind
                            .UnconditionalJump,
                    _ => throw new InvalidOperationException("Unknown parent lifecycle event.")
                };
            events.Add(new(
                kind,
                parentEvent.RawFileOffset,
                parentEvent.Left,
                parentEvent.Right,
                parentEvent.Result,
                ParentFlags: parentEvent.Flags,
                BranchTaken: parentEvent.BranchTaken,
                SelectedDestinationRawFileOffset:
                    parentEvent.SelectedDestinationRawFileOffset,
                Address: parentEvent.Address,
                Value: parentEvent.Value,
                Committed: parentEvent.Committed));
        }

        reached.AddRange(parent.ReachedInstructions);

        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan Finish(
            CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeLargePostStoreZeroGatePlan? child = null,
            bool exactBoundedCompletion = false)
        {
            if (child is not null)
            {
                foreach (uint instruction in child.ReachedInstructions)
                {
                    reached.Add(instruction);
                }

                if (child.TestReached)
                {
                    events.Add(new(
                        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind.Test,
                        CapacityFinalCalleeLargePostStoreZeroGateProjection.TestRawFileOffset,
                        Left: child.TestResult,
                        Right: child.TestResult,
                        Result: child.TestResult,
                        TestFlags: child.TestFlags));
                }

                if (child.JZReached)
                {
                    events.Add(new(
                        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind
                            .ConditionalZeroBranch,
                        CapacityFinalCalleeLargePostStoreZeroGateProjection.JZRawFileOffset,
                        TestFlags: child.TestFlags,
                        BranchTaken: child.JZTaken,
                        SelectedDestinationRawFileOffset:
                            child.SelectedDestinationRawFileOffset));
                }
            }

            return new(capture, outcome, parent)
            {
                PostStoreZeroGate = child,
                ReachedInstructions = reached.ToImmutable(),
                Events = events.ToImmutable(),
                ExactBoundedCompletion = exactBoundedCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };
        }

        bool equalPath =
            parent.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome
                    .EqualStoreToContinuation;
        bool unequalPath =
            parent.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome
                    .UnequalStoreToContinuation;
        bool validStorePath = equalPath || unequalPath;

        if (!parent.StoreValueCaptured && parent.StoreInstructionReached)
        {
            return Finish(
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome
                    .ParentStoreValueMissing,
                parent.StopAtRawFileOffsetExclusive);
        }

        if (!validStorePath
            || !parent.ExactCleanCompletion
            || parent.StopAtRawFileOffsetExclusive != PostStoreZeroGateRawFileOffset
            || parent.ContinuationEntered
            || parent.UnreachedEntryEAXCaptureProvided
            || (equalPath
                ? !parent.FinalJumpReached
                    || parent.FinalJumpDestinationRawFileOffset != PostStoreZeroGateRawFileOffset
                : parent.FinalJumpReached || parent.FinalJumpDestinationRawFileOffset.HasValue)
            || !parent.StoreValueCaptured
            || !parent.StoreValue.HasValue)
        {
            return Finish(
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome
                    .ParentLifecycleIncomplete,
                parent.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeLargePostStoreZeroGatePlan child =
            CapacityFinalCalleeLargePostStoreZeroGateProjection.Project(
                new(EntryEAX: parent.StoreValue!.Value));
        if (!child.ExactBoundedBranchCompletion
            || !((child.Outcome == CapacityFinalCalleeLargePostStoreZeroGateOutcome.ZeroTo92BD
                    && child.JZTaken == true
                    && child.SelectedDestinationRawFileOffset == ZeroBranchTargetRawFileOffset)
                || (child.Outcome == CapacityFinalCalleeLargePostStoreZeroGateOutcome.NonzeroTo927C
                    && child.JZTaken == false
                    && child.SelectedDestinationRawFileOffset == NonzeroFallthroughRawFileOffset))
            || !child.TestReached
            || !child.JZReached
            || child.StopAtRawFileOffsetExclusive
                != child.SelectedDestinationRawFileOffset
            || child.DestinationEntered
            || child.ReachedInstructions.Length != 2
            || child.ReachedInstructions[0] != PostStoreZeroGateRawFileOffset
            || child.ReachedInstructions[1]
                != CapacityFinalCalleeLargePostStoreZeroGateProjection.JZRawFileOffset)
        {
            return Finish(
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome
                    .ParentLifecycleIncomplete,
                child.StopAtRawFileOffsetExclusive,
                child);
        }

        return Finish(
            child.Outcome
                == CapacityFinalCalleeLargePostStoreZeroGateOutcome.ZeroTo92BD
                ? CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome.ZeroTo92BD
                : CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome.NonzeroTo927C,
            child.StopAtRawFileOffsetExclusive,
            child,
            exactBoundedCompletion: true);
    }
}
