using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture(
    CapacityFinalCalleeLargeRightTagRootLifecycleCapture Parent);

public enum CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
{
    ParentStopped,
    ParentStoppedWithUnusedFreshRootGlobalDWORD,
    ParentNotClean,
    EqualMissingDerivedEDI,
    EqualRootStoredAndJumped,
    UnequalMissingDerivedEDI,
    UnequalBelowUnsignedThresholdTo92F6,
    UnequalAtOrAboveUnsignedThresholdTo9354
}

public enum CapacityFinalCalleeLargeRightRootSuccessorLifecycleWriteKind
{
    ParentWrite,
    EqualRootGlobalStore
}

public readonly record struct CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite(
    CapacityFinalCalleeLargeRightRootSuccessorLifecycleWriteKind Kind,
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeLargeRightRootSuccessorLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeLargeRightRootSuccessorLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan(
    CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture Capture,
    CapacityFinalCalleeLargeRightTagRootLifecyclePlan ParentLifecycle,
    CapacityFinalCalleeCommonTailRootStorePlan? EqualRootStore,
    CapacityFinalCalleeCommonTailSizeGatePlan? UnequalSizeGate,
    CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeRightTagRootLifecycleRead> Reads =>
        ParentLifecycle.Reads;

    public ImmutableArray<CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite>
        OrderedWrites { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite>.Empty;

    public bool ExactCleanParentAdmission =>
        CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
            .IsExactRootBranch(ParentLifecycle, EqualRoute: true)
        || CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
            .IsExactRootBranch(ParentLifecycle, EqualRoute: false);

    public bool EqualRouteSelected =>
        ParentLifecycle.Outcome
            == CapacityFinalCalleeLargeRightTagRootLifecycleOutcome.EqualToFallthrough92D0
        && ParentLifecycle.RootJnzTaken == false
        && ParentLifecycle.RootJnzSelectedDestinationRawFileOffset
            == CapacityFinalCalleeLargeRightTagRootLifecycleProjection
                .FallthroughRawFileOffset;

    public bool UnequalRouteSelected =>
        ParentLifecycle.Outcome
            == CapacityFinalCalleeLargeRightTagRootLifecycleOutcome.UnequalToBranchTarget92EC
        && ParentLifecycle.RootJnzTaken == true
        && ParentLifecycle.RootJnzSelectedDestinationRawFileOffset
            == CapacityFinalCalleeLargeRightTagRootLifecycleProjection
                .BranchTargetRawFileOffset;

    public uint? DerivedEDI =>
        EqualRootStore?.Capture.EntryEDI
        ?? (UnequalSizeGate?.EntryEDICaptured == true
            ? UnequalSizeGate.Capture.EntryEDI
            : null);

    public bool ChildHelperEntered => EqualRootStore is not null || UnequalSizeGate is not null;
    public bool EqualRootStoreEntered => EqualRootStore is not null;
    public bool UnequalSizeGateEntered => UnequalSizeGate is not null;
    public bool EqualRootStoreCommitted => EqualRootStore?.StoreReached == true;
    public uint? EqualRootStoreAddress => EqualRootStore?.StoreAddress;
    public uint? EqualRootStoreValue => EqualRootStore?.StoreValue;
    public bool EqualRootStoreJumpReached => EqualRootStore?.JumpReached == true;
    public uint? EqualRootStoreJumpTargetRawFileOffset =>
        EqualRootStore?.JumpTargetRawFileOffset;
    public bool? SizeGateJnbTaken => UnequalSizeGate?.JnbTaken;
    public uint? SizeGateSelectedDestinationRawFileOffset =>
        UnequalSizeGate?.JnbSelectedDestinationRawFileOffset;
    public uint? SizeGateCount => UnequalSizeGate?.ShiftResult;
    public CapacityFinalCalleeCommonTailRootGateFlags? ParentRootCompareFlags =>
        ParentLifecycle.RootCompareFlags;

    public CapacityFinalCalleeLargeRightRootSuccessorLifecycleFlags? FlagsAtStop
    {
        get
        {
            if (UnequalSizeGate?.CompareFlags is { } sizeFlags)
            {
                return new(
                    sizeFlags.Carry,
                    sizeFlags.Parity,
                    sizeFlags.Auxiliary,
                    sizeFlags.Zero,
                    sizeFlags.Sign,
                    sizeFlags.Overflow);
            }

            if (ParentLifecycle.RootCompareFlags is { } rootFlags)
            {
                return Convert(rootFlags);
            }

            return ParentLifecycle.FlagsAtStop is { } parentFlags
                ? Convert(parentFlags)
                : null;
        }
    }

    public bool ParentRootFlagsPreservedUntilSizeShift =>
        UnequalSizeGate is
        {
            MoveReached: true,
            ShiftReached: true
        }
        && ParentRootCompareFlags.HasValue;

    public bool EqualRootFlagsPreservedThroughStoreAndJump =>
        EqualRootStore?.FlagsPreserved == true
        && ParentRootCompareFlags.HasValue
        && FlagsAtStop == Convert(ParentRootCompareFlags.Value);

    public bool EqualDestinationEntered => false;
    public bool UnequalFallthroughEntered => false;
    public bool UnequalBranchTargetEntered => false;
    public bool OpaqueCallEntered => false;

    public bool ExactCleanCompletion
    {
        get
        {
            if (!ExactCleanParentAdmission || !ParentLifecycle.ExactCleanCompletion)
            {
                return false;
            }

            if (EqualRouteSelected)
            {
                return UnequalSizeGate is null
                    && EqualRootStore is
                    {
                        Outcome: CapacityFinalCalleeCommonTailRootStoreOutcome.StoredAndJumped,
                        EntryEDICaptured: true,
                        StoreReached: true,
                        StoreAddress:
                            CapacityFinalCalleeCommonTailRootStoreProjection
                                .RootStorePEVirtualAddress,
                        StoreValue: { } storedValue,
                        JumpReached: true,
                        JumpTargetRawFileOffset:
                            CapacityFinalCalleeCommonTailRootStoreProjection
                                .JumpTargetRawFileOffset,
                        FlagsPreserved: true,
                        ExactBoundedExitAdmitted: true
                    }
                    && EqualRootStore.Capture.EntryEDI == DerivedEDI
                    && storedValue == DerivedEDI
                    && EqualRootStore.StopAtRawFileOffsetExclusive
                        == CapacityFinalCalleeCommonTailRootStoreProjection
                            .JumpTargetRawFileOffset
                    && EqualRootFlagsPreservedThroughStoreAndJump
                    && OrderedWrites.Length == ParentLifecycle.OrderedWrites.Length + 1
                    && OrderedWrites[^1]
                        == new CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite(
                            CapacityFinalCalleeLargeRightRootSuccessorLifecycleWriteKind
                                .EqualRootGlobalStore,
                            CapacityFinalCalleeCommonTailRootStoreProjection
                                .StoreRawFileOffset,
                            CapacityFinalCalleeCommonTailRootStoreProjection
                                .RootStorePEVirtualAddress,
                            storedValue)
                    && !EqualDestinationEntered;
            }

            if (UnequalRouteSelected)
            {
                CapacityFinalCalleeCommonTailSizeGatePlan? sizeGate = UnequalSizeGate;
                if (sizeGate is null
                    || EqualRootStore is not null
                    || !sizeGate.EntryEDICaptured
                    || !sizeGate.MoveReached
                    || !sizeGate.ShiftReached
                    || sizeGate.ShiftCount
                        != CapacityFinalCalleeCommonTailSizeGateProjection.ShiftCount
                    || sizeGate.ShiftInput != DerivedEDI
                    || sizeGate.ShiftResult is not uint count
                    || sizeGate.CompareLeft != count
                    || sizeGate.CompareRight
                        != CapacityFinalCalleeCommonTailSizeGateProjection.CompareThreshold
                    || sizeGate.CompareFlags is null
                    || !sizeGate.JnbReached
                    || !sizeGate.ExactCleanAdmission
                    || sizeGate.TargetEntered
                    || !sizeGate.Writes.IsEmpty
                    || sizeGate.StopAtRawFileOffsetExclusive
                        != sizeGate.JnbSelectedDestinationRawFileOffset
                    || OrderedWrites.Length != ParentLifecycle.OrderedWrites.Length)
                {
                    return false;
                }

                bool below = count < CapacityFinalCalleeCommonTailSizeGateProjection
                    .CompareThreshold
                    && sizeGate.Outcome
                        == CapacityFinalCalleeCommonTailSizeGateOutcome
                            .BelowUnsignedThresholdToFallthrough
                    && sizeGate.JnbTaken == false
                    && sizeGate.JnbSelectedDestinationRawFileOffset
                        == CapacityFinalCalleeCommonTailSizeGateProjection
                            .FallthroughRawFileOffset;
                bool atOrAbove = count >= CapacityFinalCalleeCommonTailSizeGateProjection
                    .CompareThreshold
                    && sizeGate.Outcome
                        == CapacityFinalCalleeCommonTailSizeGateOutcome
                            .AtOrAboveUnsignedThresholdToBranchTarget
                    && sizeGate.JnbTaken == true
                    && sizeGate.JnbSelectedDestinationRawFileOffset
                        == CapacityFinalCalleeCommonTailSizeGateProjection
                            .BranchTargetRawFileOffset;
                return (below || atOrAbove)
                    && ParentRootFlagsPreservedUntilSizeShift
                    && !UnequalFallthroughEntered
                    && !UnequalBranchTargetEntered;
            }

            return false;
        }
    }

    public uint StopAtRawFileOffsetExclusive =>
        EqualRootStore?.StopAtRawFileOffsetExclusive
        ?? UnequalSizeGate?.StopAtRawFileOffsetExclusive
        ?? ParentLifecycle.StopAtRawFileOffsetExclusive;

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleFlags Convert(
        CapacityFinalCalleeCommonTailRootGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleFlags Convert(
        CapacityFinalCalleeCommonTailTagWritesFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
}

/// <summary>
/// Joins the right-root comparison to its equal-only root store or unequal size gate.
/// </summary>
public static class CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeRightTagRootLifecycleProjection.EntryRawFileOffset;
    public const uint EqualRootStoreEntryRawFileOffset =
        CapacityFinalCalleeCommonTailRootStoreProjection.EntryRawFileOffset;
    public const uint UnequalSizeGateEntryRawFileOffset =
        CapacityFinalCalleeCommonTailSizeGateProjection.EntryRawFileOffset;
    public const uint EqualStopRawFileOffset =
        CapacityFinalCalleeCommonTailRootStoreProjection.JumpTargetRawFileOffset;
    public const uint UnequalBelowStopRawFileOffset =
        CapacityFinalCalleeCommonTailSizeGateProjection.FallthroughRawFileOffset;
    public const uint UnequalAtOrAboveStopRawFileOffset =
        CapacityFinalCalleeCommonTailSizeGateProjection.BranchTargetRawFileOffset;
    public const uint EqualRootStorePEVirtualAddress =
        CapacityFinalCalleeCommonTailRootStoreProjection.RootStorePEVirtualAddress;

    public static ImmutableArray<CapacityFinalCalleeLargeRightRootSuccessorLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan Project(
        CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeRightTagRootLifecyclePlan parent =
            CapacityFinalCalleeLargeRightTagRootLifecycleProjection.Project(capture.Parent);

        bool equalRootBranch = IsExactRootBranch(parent, EqualRoute: true);
        bool unequalRootBranch = IsExactRootBranch(parent, EqualRoute: false);
        if (!parent.ExactCleanCompletion
            || (!equalRootBranch && !unequalRootBranch))
        {
            CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome stoppedOutcome =
                parent.Outcome switch
                {
                    CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                        .ParentStoppedWithUnusedFreshRootGlobalDWORD =>
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .ParentStoppedWithUnusedFreshRootGlobalDWORD,
                    CapacityFinalCalleeLargeRightTagRootLifecycleOutcome.ParentStopped
                        or CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                            .MissingFreshRootGlobalDWORD =>
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .ParentStopped,
                    _ => CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                        .ParentNotClean
                };
            return new(capture, parent, null, null, stoppedOutcome)
            {
                OrderedWrites = ParentWrites(parent)
            };
        }

        uint? derivedEDI = capture.Parent.Parent.LateEntryEDI;
        if (equalRootBranch)
        {
            CapacityFinalCalleeCommonTailRootStorePlan store =
                CapacityFinalCalleeCommonTailRootStoreProjection.Project(new(derivedEDI));
            ImmutableArray<CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite> writes =
                ParentWrites(parent);
            foreach (CapacityFinalCalleeCommonTailRootStoreWrite write in store.Writes)
            {
                writes = writes.Add(new(
                    CapacityFinalCalleeLargeRightRootSuccessorLifecycleWriteKind
                        .EqualRootGlobalStore,
                    write.RawFileOffset,
                    write.Address,
                    write.Value));
            }

            return new(
                capture,
                parent,
                store,
                null,
                store.Outcome
                    == CapacityFinalCalleeCommonTailRootStoreOutcome.StoredAndJumped
                    ? CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                        .EqualRootStoredAndJumped
                    : CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                        .EqualMissingDerivedEDI)
            {
                OrderedWrites = writes
            };
        }

        if (unequalRootBranch)
        {
            CapacityFinalCalleeCommonTailSizeGatePlan sizeGate =
                CapacityFinalCalleeCommonTailSizeGateProjection.Project(new(derivedEDI));
            CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome outcome =
                sizeGate.Outcome switch
                {
                    CapacityFinalCalleeCommonTailSizeGateOutcome.MissingEntryEDI =>
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .UnequalMissingDerivedEDI,
                    CapacityFinalCalleeCommonTailSizeGateOutcome
                        .BelowUnsignedThresholdToFallthrough =>
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .UnequalBelowUnsignedThresholdTo92F6,
                    CapacityFinalCalleeCommonTailSizeGateOutcome
                        .AtOrAboveUnsignedThresholdToBranchTarget =>
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .UnequalAtOrAboveUnsignedThresholdTo9354,
                    _ => throw new InvalidOperationException(
                        "The exact unequal parent selected an unexpected size-gate outcome.")
                };
            return new(capture, parent, null, sizeGate, outcome)
            {
                OrderedWrites = ParentWrites(parent)
            };
        }

        return new(
            capture,
            parent,
            null,
            null,
            CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome.ParentNotClean)
        {
            OrderedWrites = ParentWrites(parent)
        };
    }

    private static ImmutableArray<CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite>
        ParentWrites(CapacityFinalCalleeLargeRightTagRootLifecyclePlan parent) =>
        parent.OrderedWrites
            .Select(write =>
                new CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite(
                    CapacityFinalCalleeLargeRightRootSuccessorLifecycleWriteKind.ParentWrite,
                    write.RawFileOffset,
                    write.Address,
                    write.Value))
            .ToImmutableArray();

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightRootSuccessorLifecycleInstructionEvidence>
        BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRightRootSuccessorLifecycleInstructionEvidence>();

        foreach (CapacityFinalCalleeLargeRightTagRootLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeLargeRightTagRootLifecycleProjection.Instructions)
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

        foreach (CapacityFinalCalleeCommonTailRootStoreInstructionEvidence instruction
            in CapacityFinalCalleeCommonTailRootStoreProjection.Instructions)
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

        foreach (CapacityFinalCalleeCommonTailSizeGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonTailSizeGateProjection.Instructions)
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

    internal static bool IsExactRootBranch(
        CapacityFinalCalleeLargeRightTagRootLifecyclePlan parent,
        bool EqualRoute)
    {
        CapacityFinalCalleeCommonTailRootGatePlan? root = parent.RootGate;
        uint expectedDestination = EqualRoute
            ? CapacityFinalCalleeLargeRightTagRootLifecycleProjection.FallthroughRawFileOffset
            : CapacityFinalCalleeLargeRightTagRootLifecycleProjection.BranchTargetRawFileOffset;
        return parent.ExactCleanCompletion
            && parent.ExactCleanParentHandoff
            && parent.Outcome
                == (EqualRoute
                    ? CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                        .EqualToFallthrough92D0
                    : CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                        .UnequalToBranchTarget92EC)
            && root is
            {
                EntryESICaptured: true,
                FreshGlobalReadReached: true,
                FreshGlobalCaptured: true,
                CompareReached: true,
                CompareResult: { } compareResult,
                CompareFlags: { } compareFlags,
                JnzReached: true,
                JnzTaken: { } taken,
                JnzSelectedDestinationRawFileOffset: { } destination,
                ExactCleanAdmission: true,
                OpaqueEntryReached: false
            }
            && root.Capture.EntryESI == parent.Capture.Parent.LateEntryESI
            && root.FreshGlobalDWORD == parent.Capture.FreshRootGlobalDWORD
            && (EqualRoute ? compareResult == 0 : compareResult != 0)
            && compareFlags.Zero == EqualRoute
            && root.JnzTaken == !EqualRoute
            && taken == !EqualRoute
            && destination == expectedDestination
            && root.StopAtRawFileOffsetExclusive == expectedDestination
            && parent.RootJnzSelectedDestinationRawFileOffset == expectedDestination
            && parent.RootJnzTaken == !EqualRoute
            && parent.RootBranchTargetEntered == false
            && parent.RootFallthroughEntered == false
            && parent.FlagsPreservedUntilRootCompare;
    }
}
