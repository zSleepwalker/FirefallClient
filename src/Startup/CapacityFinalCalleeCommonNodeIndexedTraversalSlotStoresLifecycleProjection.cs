using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleCapture(
    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleCapture TraversalCapture);

public enum CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
{
    ParentStopped,
    ParentChildZeroHandoffNotExactAt945C,
    MissingDerivedChildSlotAddressAt945C,
    MissingFreshThresholdValueAt945C,
    ChildZeroUnsignedBelowBranchTarget,
    ChildZeroNotBelowHandoffNotExactAt9468,
    MissingDerivedChildSlotAddressAt9468,
    MissingInheritedNodeInitializerAt9468,
    MissingOriginalParentIndexedPointerAt946A,
    CompletedToCleanupJump,
    ParentOtherBoundedExit
}

public enum CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind
{
    ParentWrite,
    ParentFreshGlobalRead,
    ParentFreshIndexedDWORDRead,
    ParentFreshTagDWORDRead,
    FreshChildDWORDRead,
    FreshChildPlus4Read,
    ThresholdFreshGlobalRead,
    StoreWrite
}

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleMemoryEffect(
    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Attempted,
    bool Committed,
    bool ForwardedFromPriorWrite);

public enum CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase
{
    ParentTraversal,
    ChildIteration,
    ZeroSlotThreshold,
    StoreTail
}

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstruction(
    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase Phase,
    uint RawFileOffset);

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan(
    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan ParentLifecycle
        { get; init; } = null!;
    public CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? ThresholdPlan =>
        ParentLifecycle.ThresholdPlan;
    public CapacityFinalCalleeCommonZeroSlotStoreTailPlan? StoreTail { get; init; }
    public CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan TraversalPlan =>
        ParentLifecycle.TraversalPlan;
    public ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleRead>
        OrderedReads => ParentLifecycle.OrderedReads;
    public ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleMemoryEffect>
                .Empty;
    public ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstruction>
                .Empty;
    public uint? ChildFieldAddressAt93EE => ParentLifecycle.ChildFieldAddressAt93EE;
    public uint? ParentDerivedEntryEDX => ParentLifecycle.ParentDerivedEntryEDX;
    public uint? ThresholdEntryECX => ParentLifecycle.ThresholdEntryECX;
    public uint? StoreTailEntryECX => StoreTail?.Capture.EntryECX;
    public uint? StoreTailEntryESI => StoreTail?.Capture.EntryESI;
    public uint? StoreTailLateEntryEDX => StoreTail?.Capture.LateEntryEDX;
    public uint? InheritedNodeInitializerESI =>
        Capture.TraversalCapture.Parent.Parent.Parent.EntryESI;
    public bool StoreTailEntryECXIsDerivedChildSlotAddress =>
        StoreTailEntryECX == ChildFieldAddressAt93EE
        && ChildFieldAddressAt93EE.HasValue;
    public bool StoreTailEntryESIIsInheritedNodeInitializer =>
        StoreTailEntryESI == InheritedNodeInitializerESI
        && InheritedNodeInitializerESI.HasValue;
    public bool StoreTailLateEntryEDXIsOriginalDerivedParentPointer =>
        StoreTailLateEntryEDX == ParentDerivedEntryEDX
        && ParentDerivedEntryEDX.HasValue;
    public bool StoreTailLateEntryEDXIsDerivedChildIndexEBX =>
        StoreTailLateEntryEDX == ParentLifecycle.TraversalPlan.DerivedEntryEBX;
    public bool StoreTailLateEntryEDXIsZeroChildValue =>
        StoreTailLateEntryEDX == ParentLifecycle.TraversalPlan.IterationPlan?.FreshChildDWORD;
    public bool ExactParentZeroHandoff => ParentLifecycle.ExactParentZeroHandoff;
    public bool ChildZero945CSelected => ParentLifecycle.ChildZero945CSelected;
    public bool Internal945CEntered => ParentLifecycle.Internal945CEntered;
    public bool ThresholdCompareReached => ParentLifecycle.ThresholdCompareReached;
    public bool? ThresholdBranchTaken => ParentLifecycle.ThresholdBranchTaken;
    public uint? ThresholdBranchSelectedDestinationRawFileOffset =>
        ParentLifecycle.ThresholdBranchSelectedDestinationRawFileOffset;
    public bool StoreTailEntryReached => StoreTail is not null;
    public bool FirstStoreAttempted => StoreTail?.FirstStoreAttempted == true;
    public bool FirstStoreCommitted => StoreTail?.FirstStoreCommitted == true;
    public bool SecondStoreAttempted => StoreTail?.SecondStoreAttempted == true;
    public bool SecondStoreCommitted => StoreTail?.SecondStoreCommitted == true;
    public bool ThirdStoreAttempted => StoreTail?.ThirdStoreAttempted == true;
    public bool ThirdStoreCommitted => StoreTail?.ThirdStoreCommitted == true;
    public bool FourthStoreAttempted => StoreTail?.FourthStoreAttempted == true;
    public bool FourthStoreCommitted => StoreTail?.FourthStoreCommitted == true;
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite> Writes =>
        StoreTail?.Writes
        ?? ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite>.Empty;
    public CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleFlags? FlagsAtStop
    {
        get
        {
            if (StoreTail?.FlagsAtStop is { } storeFlags)
                return new(
                    storeFlags.Carry,
                    storeFlags.Parity,
                    storeFlags.AuxiliaryCarry,
                    storeFlags.Zero,
                    storeFlags.Sign,
                    storeFlags.Overflow);
            if (ThresholdPlan?.CompareFlags is not { } flags)
            {
                if (ParentLifecycle.FlagsAtStop is not { } parentFlags)
                    return null;
                return new(
                    parentFlags.Carry,
                    parentFlags.Parity,
                    parentFlags.Auxiliary,
                    parentFlags.Zero,
                    parentFlags.Sign,
                    parentFlags.Overflow);
            }
            return new(
                flags.Carry,
                flags.Parity,
                flags.AuxiliaryCarry,
                flags.Zero,
                flags.Sign,
                flags.Overflow);
        }
    }
    public bool Internal9468Entered => FirstStoreAttempted;
    public bool Outer93ACEntered => false;
    public bool Outer9403Entered => false;
    public bool Outer93E4Entered => false;
    public bool Outer9468Entered => false;
    public bool Outer8E76Entered => false;
    public bool CleanupJumpReached => StoreTail?.JumpReached == true;
    public bool Outer942FEntered => false;
    public bool ThresholdCompareFlagsPreservedThroughStores =>
        StoreTail is { FlagsPreserved: true }
        && ThresholdPlan?.CompareFlags is { } flags
        && StoreTail.FlagsAtStop == new CapacityFinalCalleeCommonZeroSlotStoreTailFlags(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Adds the 9468 store tail only after the exact clean indexed zero-child,
/// unsigned-not-below threshold fallthrough.
/// </summary>
public static class CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleProjection.EntryRawFileOffset;
    public const uint ThresholdEntryRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.EntryRawFileOffset;
    public const uint StoreTailEntryRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotStoreTailProjection.EntryRawFileOffset;
    public const uint BelowBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint CleanupJumpTargetRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpTargetRawFileOffset;
    public const uint CleanupJumpBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonZeroSlotStoreTailProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleProjection.Project(
                capture.TraversalCapture);
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleMemoryEffect>();
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind.ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind.ParentWrite,
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind.ParentFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind.ParentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind.ParentFreshIndexedDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind.ParentFreshIndexedDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind.ParentFreshTagDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind.ParentFreshTagDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind.FreshChildDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind.FreshChildDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind.FreshChildPlus4Read =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind.FreshChildPlus4Read,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind.ThresholdFreshGlobalRead
                },
                effect.RawFileOffset,
                effect.Address,
                effect.AddressCaptured,
                effect.Value,
                effect.ValueCaptured,
                effect.Attempted,
                effect.Committed,
                effect.ForwardedFromPriorWrite));
        }

        foreach (CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstruction instruction
            in parent.OrderedInstructions)
        {
            instructions.Add(new(
                instruction.Phase switch
                {
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstructionPhase.ChildIteration =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase.ChildIteration,
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstructionPhase.ZeroSlotThreshold =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase.ZeroSlotThreshold,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase.ParentTraversal
                },
                instruction.RawFileOffset));
        }

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan? storeTail = null;
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome outcome =
            MapParentOutcome(parent);
        uint stopAt = parent.StopAtRawFileOffsetExclusive;
        bool exactBelowRoute = IsExactBelowRoute(parent);
        bool exactStoreRoute = IsExactStoreTailHandoff(parent);

        if (exactBelowRoute)
        {
            outcome =
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget;
        }
        else if (exactStoreRoute)
        {
            CapacityFinalCalleeCommonZeroSlotThresholdGatePlan threshold =
                parent.ThresholdPlan!;
            storeTail = CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(
                EntryECX: threshold.CompareLeft,
                EntryESI: capture.TraversalCapture.Parent.Parent.Parent.EntryESI,
                LateEntryEDX: parent.ParentDerivedEntryEDX,
                IncomingFlags: ToStoreTailFlags(threshold.CompareFlags)));
            outcome = storeTail.Outcome switch
            {
                CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingEntryECXForFirstStore =>
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                        .MissingDerivedChildSlotAddressAt9468,
                CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingEntryESIForFirstStore =>
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                        .MissingInheritedNodeInitializerAt9468,
                CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingLateEntryEDXForSecondStore =>
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                        .MissingOriginalParentIndexedPointerAt946A,
                _ =>
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                        .CompletedToCleanupJump
            };
            foreach (CapacityFinalCalleeCommonZeroSlotStoreTailWrite write in storeTail.Writes)
            {
                effects.Add(new(
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind.StoreWrite,
                    write.RawFileOffset,
                    write.Address,
                    write.AddressCaptured,
                    write.Value,
                    write.ValueCaptured,
                    write.Attempted,
                    write.Committed,
                    false));
                instructions.Add(new(
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase.StoreTail,
                    write.RawFileOffset));
            }

            if (storeTail.JumpReached)
            {
                instructions.Add(new(
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase.StoreTail,
                    CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpRawFileOffset));
            }

            stopAt = storeTail.StopAtRawFileOffsetExclusive;
        }
        else if (parent.ThresholdPlan is not null
            && parent.ChildZero945CSelected
            && parent.ThresholdPlan.BranchTaken == false)
        {
            outcome =
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroNotBelowHandoffNotExactAt9468;
        }
        else if (parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C)
        {
            outcome =
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingFreshThresholdValueAt945C;
        }
        else if (parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C)
        {
            outcome =
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingDerivedChildSlotAddressAt945C;
        }
        else if (parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C)
        {
            outcome =
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .ParentChildZeroHandoffNotExactAt945C;
        }

        bool exactStoreCompletion = exactStoreRoute
            && storeTail is
            {
                Outcome: CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget,
                ExactBoundedExitAdmitted: true,
                JumpReached: true,
                JumpTargetRawFileOffset: CleanupJumpTargetRawFileOffset,
                StopAtRawFileOffsetExclusive: CleanupJumpTargetRawFileOffset,
                BoundaryReached: true,
                BoundaryInstructionEntered: false,
                JumpTargetEntered: false,
                Writes.Length: 4
            }
            && storeTail.Writes.All(write => write.Committed);
        bool exactBounded = storeTail is null
            ? parent.ExactBoundedCompletion
            : exactStoreCompletion;
        bool exactClean = storeTail is null
            ? parent.ExactCleanCompletion
            : exactStoreCompletion;

        return new(capture, outcome)
        {
            ParentLifecycle = parent,
            StoreTail = storeTail,
            OrderedMemoryEffects = effects.ToImmutable(),
            OrderedInstructions = instructions.ToImmutable(),
            ExactBoundedCompletion = exactBounded,
            ExactCleanCompletion = exactClean,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static bool IsExactBelowRoute(
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? threshold = parent.ThresholdPlan;
        return parent.ExactParentZeroHandoff
            && parent.Internal945CEntered
            && threshold is
            {
                ExactCleanCompletion: true,
                CompareInstructionReached: true,
                CompareReached: true,
                BranchReached: true,
                BranchTaken: true,
                CompareFlags: { Carry: true },
                BranchSelectedDestinationRawFileOffset: BelowBranchTargetRawFileOffset,
                StopAtRawFileOffsetExclusive: BelowBranchTargetRawFileOffset,
                TargetEntered: false
            }
            && threshold.BranchFlags == threshold.CompareFlags;
    }

    private static bool IsExactStoreTailHandoff(
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonTraversalIterationPlan? iteration = parent.IterationPlan;
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? threshold = parent.ThresholdPlan;
        return parent.ExactParentZeroHandoff
            && parent.ExactCleanCompletion
            && parent.ChildZero945CSelected
            && parent.Internal945CEntered
            && !parent.ChildZero945CBoundaryTargetEntered
            && parent.ParentDerivedEntryEDX.HasValue
            && parent.TraversalPlan.EntryEDXDerivedFromParentFreshIndexedLoad
            && iteration is
            {
                FreshChildDWORD: 0,
                LateEntryEDXCaptured: true,
                MoveChildToEDXReached: false,
                FreshChildPlus4ReadReached: false,
                StopAtRawFileOffsetExclusive:
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleProjection
                        .ZeroChildBoundaryRawFileOffsetExclusive
            }
            && iteration.Capture.LateEntryEDX == parent.ParentDerivedEntryEDX
            && parent.ThresholdEntryECXIsDerivedFromChildFieldAddress
            && parent.ChildFieldAddressAt93EE.HasValue
            && parent.ThresholdEntryECX == parent.ChildFieldAddressAt93EE
            && threshold is
            {
                ExactCleanCompletion: true,
                CompareInstructionReached: true,
                GlobalReadReached: true,
                FreshGlobalValueCaptured: true,
                EntryECXCaptured: true,
                CompareReached: true,
                CompareLeft: not null,
                CompareRight: not null,
                CompareFlags: { Carry: false },
                TargetEntered: false,
                BranchReached: true,
                BranchTaken: false,
                BranchSelectedDestinationRawFileOffset: StoreTailEntryRawFileOffset,
                StopAtRawFileOffsetExclusive: StoreTailEntryRawFileOffset
            }
            && threshold.CompareLeft == parent.ChildFieldAddressAt93EE
            && threshold.BranchFlags == threshold.CompareFlags;
    }

    private static CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
        MapParentOutcome(
            CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan parent) =>
        parent.Outcome switch
        {
            CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome.ParentZeroToBounded93AC
            or CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome.ParentEqualToBounded9403
            or CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome.MaskedChildEqualsEntryEDIAt9403
            or CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome.MaskedChildDiffersFromEntryEDITo93E4 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .ParentOtherBoundedExit,
            _ =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome.ParentStopped
        };

    private static CapacityFinalCalleeCommonZeroSlotStoreTailFlags? ToStoreTailFlags(
        CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonZeroSlotThresholdGateFlags known
            ? new(
                known.Carry,
                known.Parity,
                known.AuxiliaryCarry,
                known.Zero,
                known.Sign,
                known.Overflow)
            : null;
}
