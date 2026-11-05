using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleCapture(
    CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture Parent,
    uint? LateChildFreshGlobalValue);

public enum CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
{
    ParentStopped,
    ParentNotClean,
    ParentOtherRouteWithUnusedLateChildFreshGlobalValue,
    EqualMissingDerivedEDI,
    EqualRootStoredAndJumped,
    UnequalMissingDerivedEDI,
    UnequalAtOrAboveUnsignedThresholdTo9354,
    UnequalBelowStopsBeforeOpaque9304,
    UnequalBelowStopsBeforeOpaque9304WithUnusedFreshGlobalValue,
    UnequalBelowMissingFreshGlobalValue,
    UnequalBelowZeroTestToBounded931B,
    UnequalBelowNonzeroTestToBounded9334
}

public readonly record struct CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public enum CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind
{
    ParentRead,
    ParentWrite,
    LateChildFreshGlobalRead
}

public readonly record struct CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffect(
    CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool ReadAttempted,
    bool WriteAttempted,
    bool Committed,
    bool ForwardedFromPriorWrite);

public sealed record CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan(
    CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleCapture Capture,
    CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan ParentLifecycle,
    CapacityFinalCalleeCommonIndexedMaskLifecyclePlan? CommonIndexedMaskLifecycle,
    CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeRightTagRootLifecycleRead> ParentReads =>
        ParentLifecycle.Reads;

    public ImmutableArray<CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite>
        ParentWrites => ParentLifecycle.OrderedWrites;

    public ImmutableArray<CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffect>
        CombinedOrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffect>
                .Empty;

    public uint? ParentDerivedEDI => ParentLifecycle.DerivedEDI;
    public uint? ParentSizeGateCount => ParentLifecycle.SizeGateCount;
    public bool ParentBelow92F6Admission =>
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleProjection
            .IsExactParentBelow92F6(ParentLifecycle);

    public uint? IndexedAddressECX =>
        CommonIndexedMaskLifecycle?.Capture.ParentCapture.EntryECX;

    public uint? IndexedAddressEDI =>
        CommonIndexedMaskLifecycle?.Capture.ParentCapture.EntryEDI;

    public uint? IndexedAddressEDX =>
        CommonIndexedMaskLifecycle?.ParentDerivedEDX;

    public bool ChildIndexAndEDIUseParentOperands =>
        ParentBelow92F6Admission
        && IndexedAddressECX == ParentSizeGateCount
        && IndexedAddressEDI == ParentDerivedEDI;

    public CapacityFinalCalleeCommonMaskSelectionGatePlan? ChildMaskSelectionGate =>
        CommonIndexedMaskLifecycle?.ChildMaskSelectionGate;

    public bool IndexedAddressEntryReached => CommonIndexedMaskLifecycle is not null;
    public bool IndexedLEAAttempted =>
        CommonIndexedMaskLifecycle?.ParentAddressGate.LEAReached == true;
    public bool Internal92F6Entered =>
        CommonIndexedMaskLifecycle is not null && IndexedLEAAttempted;
    public bool IndexedLEAIsAddressOnly =>
        CommonIndexedMaskLifecycle?.ParentAddressGate is
        {
            LEAReached: true,
            AddressOnlyLEA: true,
            MemoryReadPerformed: false
        };
    public bool Indexed930AMoveAttempted =>
        ChildMaskSelectionGate?.MoveEDIReached == true;
    public bool Internal930AEntered => Indexed930AMoveAttempted;

    public bool LateChildFreshGlobalReadAttempted =>
        CommonIndexedMaskLifecycle?.FreshGlobalReadReached == true;

    public uint? LateChildFreshGlobalReadAddress =>
        CommonIndexedMaskLifecycle?.FreshGlobalReadAddress;

    public bool LateChildFreshGlobalValueCaptured =>
        CommonIndexedMaskLifecycle?.FreshGlobalValueCaptured == true;

    public bool UnreachedLateChildFreshGlobalCaptureProvided =>
        Capture.LateChildFreshGlobalValue.HasValue
        && !LateChildFreshGlobalReadAttempted;

    public CapacityFinalCalleeCommonMaskSelectionGateFlags? ParentIndexedCompareFlags =>
        CommonIndexedMaskLifecycle?.ParentAddressGate.CompareFlags is
            CapacityFinalCalleeCommonIndexedAddressGateFlags flags
                ? new CapacityFinalCalleeCommonMaskSelectionGateFlags(
                    flags.Carry,
                    flags.Parity,
                    flags.Auxiliary,
                    flags.Zero,
                    flags.Sign,
                    flags.Overflow)
                : null;

    public CapacityFinalCalleeCommonMaskSelectionGateFlags? FlagsAtParent92F6 =>
        ParentLifecycle.FlagsAtStop is
            CapacityFinalCalleeLargeRightRootSuccessorLifecycleFlags flags
                ? new CapacityFinalCalleeCommonMaskSelectionGateFlags(
                    flags.Carry,
                    flags.Parity,
                    flags.Auxiliary,
                    flags.Zero,
                    flags.Sign,
                    flags.Overflow)
                : null;

    public CapacityFinalCalleeCommonMaskSelectionGateFlags? FlagsAtStop =>
        CommonIndexedMaskLifecycle?.FlagsAtStop
        ?? FlagsAtParent92F6
        ?? UnknownFlags;

    public bool ParentFlagsPreservedWithoutChild =>
        CommonIndexedMaskLifecycle is null
        && FlagsAtStop
            == (FlagsAtParent92F6 ?? UnknownFlags);

    public bool ParentFlagsPreservedThroughLEAAndMOV =>
        ParentBelow92F6Admission
        && IndexedLEAAttempted
        && CommonIndexedMaskLifecycle?.ParentAddressGate.MoveEAXReached == true
        && FlagsAtParent92F6.HasValue;

    public bool MissingFreshGlobalPreservesShiftFlags =>
        ChildMaskSelectionGate is
        {
            FreshGlobalReadReached: true,
            FreshGlobalValueCaptured: false,
            TestReached: false,
            ShiftReached: true,
            ShiftFlags: { } shiftFlags
        }
        && FlagsAtStop == shiftFlags;

    public bool OpaqueCallInstructionAtStop =>
        CommonIndexedMaskLifecycle?.OpaqueCallInstructionAtStop == true;

    public bool OpaqueCallEntered => false;
    public bool OuterZeroDestinationEntered => false;
    public bool OuterNonzeroDestinationEntered => false;
    public bool OuterCalleeEntered => false;

    public ImmutableArray<uint> IndexedMaskWrites =>
        ChildMaskSelectionGate?.Writes ?? ImmutableArray<uint>.Empty;

    public bool ExactCleanParentAdmission => ParentBelow92F6Admission;

    public bool ExactCleanAdmission
    {
        get
        {
            if (!ParentBelow92F6Admission
                || CommonIndexedMaskLifecycle is not
                    CapacityFinalCalleeCommonIndexedMaskLifecyclePlan child
                || child.UnreachedChildGlobalCaptureProvided
                || UnreachedLateChildFreshGlobalCaptureProvided
                || !ChildIndexAndEDIUseParentOperands)
            {
                return false;
            }

            if (child.Outcome
                == CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentLowEDIOpaqueStop)
            {
                return child.ParentAddressGate.Outcome
                        == CapacityFinalCalleeCommonIndexedAddressGateOutcome
                            .BelowUnsignedThresholdToCleanBoundedEdge
                    && child.ParentAddressGate.ExactCleanBoundedEdge
                    && child.ParentAddressGate.CompareLeft == ParentDerivedEDI
                    && child.ParentAddressGate.CompareRight
                        == CapacityFinalCalleeCommonIndexedAddressGateProjection
                            .CompareThreshold
                    && child.ParentAddressGate is
                    {
                        EntryECXCaptured: true,
                        LEAReached: true,
                        MoveEAXReached: true,
                        EntryEDICaptured: true,
                        CompareReached: true,
                        JnbReached: true,
                        JnbTaken: false,
                        JnbSelectedDestinationRawFileOffset:
                            CapacityFinalCalleeCommonIndexedAddressGateProjection
                                .FallthroughRawFileOffset,
                        OpaqueCallInstructionAtStop: true,
                        OpaqueCallEntered: false,
                        Writes.IsEmpty: true,
                        StopAtRawFileOffsetExclusive:
                            CapacityFinalCalleeCommonIndexedAddressGateProjection
                                .FallthroughRawFileOffset
                    }
                    && !child.ChildReached
                    && !child.FreshGlobalReadReached
                    && !OpaqueCallEntered
                    && StopAtRawFileOffsetExclusive
                        == CapacityFinalCalleeCommonIndexedAddressGateProjection
                            .FallthroughRawFileOffset;
            }

            return child.ExactCleanAdmission
                && ChildMaskSelectionGate is
                {
                    TestReached: true,
                    JnzReached: true,
                    ExactCleanBoundedDestination: true,
                    LaterOrOrStoreReached: false,
                    MemoryWritePerformed: false,
                    Writes.IsEmpty: true,
                    StopAtRawFileOffsetExclusive:
                        CapacityFinalCalleeCommonMaskSelectionGateProjection
                            .ZeroDestinationRawFileOffset
                        or CapacityFinalCalleeCommonMaskSelectionGateProjection
                            .NonzeroDestinationRawFileOffset
                }
                && StopAtRawFileOffsetExclusive
                    == ChildMaskSelectionGate.JnzSelectedDestinationRawFileOffset
                && !OpaqueCallEntered
                && !OuterZeroDestinationEntered
                && !OuterNonzeroDestinationEntered
                && !OuterCalleeEntered;
        }
    }

    public bool CleanBypass =>
        CommonIndexedMaskLifecycle is null
        && ParentLifecycle.ExactCleanCompletion
        && !UnreachedLateChildFreshGlobalCaptureProvided
        && ParentWrites.SequenceEqual(ParentLifecycle.OrderedWrites)
        && IndexedMaskWrites.IsEmpty;

    public bool ExactBoundedCompletion
    {
        get
        {
            if (!ParentLifecycle.ExactCleanCompletion)
            {
                return false;
            }

            if (CommonIndexedMaskLifecycle is null)
            {
                return true;
            }

            if (!ParentBelow92F6Admission || !ChildIndexAndEDIUseParentOperands)
            {
                return false;
            }

            if (CommonIndexedMaskLifecycle.Outcome
                == CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome
                    .ParentLowEDIOpaqueStop)
            {
                return CommonIndexedMaskLifecycle.ParentAddressGate is
                    {
                        Outcome:
                            CapacityFinalCalleeCommonIndexedAddressGateOutcome
                                .BelowUnsignedThresholdToCleanBoundedEdge,
                        EntryECXCaptured: true,
                        LEAReached: true,
                        MoveEAXReached: true,
                        EntryEDICaptured: true,
                        CompareReached: true,
                        JnbReached: true,
                        JnbTaken: false,
                        JnbSelectedDestinationRawFileOffset:
                            CapacityFinalCalleeCommonIndexedAddressGateProjection
                                .FallthroughRawFileOffset,
                        ExactCleanBoundedEdge: true,
                        OpaqueCallInstructionAtStop: true,
                        OpaqueCallEntered: false,
                        Writes.IsEmpty: true,
                        StopAtRawFileOffsetExclusive:
                            CapacityFinalCalleeCommonIndexedAddressGateProjection
                                .FallthroughRawFileOffset
                    }
                    && !CommonIndexedMaskLifecycle.ChildReached
                    && !CommonIndexedMaskLifecycle.FreshGlobalReadReached
                    && StopAtRawFileOffsetExclusive
                        == CapacityFinalCalleeCommonIndexedAddressGateProjection
                            .FallthroughRawFileOffset
                    && !OpaqueCallEntered;
            }

            return ChildMaskSelectionGate is
                {
                    TestReached: true,
                    JnzReached: true,
                    ExactCleanBoundedDestination: true,
                    JnzSelectedDestinationRawFileOffset:
                        CapacityFinalCalleeCommonMaskSelectionGateProjection
                            .ZeroDestinationRawFileOffset
                        or CapacityFinalCalleeCommonMaskSelectionGateProjection
                            .NonzeroDestinationRawFileOffset,
                    LaterOrOrStoreReached: false,
                    MemoryWritePerformed: false,
                    Writes.IsEmpty: true
                }
                && StopAtRawFileOffsetExclusive
                    == ChildMaskSelectionGate.JnzSelectedDestinationRawFileOffset
                && !OuterZeroDestinationEntered
                && !OuterNonzeroDestinationEntered
                && !OuterCalleeEntered;
        }
    }

    public bool ActualExactBoundedCompletion => ExactBoundedCompletion;

    public bool ExactCleanCompletion =>
        ExactBoundedCompletion
        && !UnreachedLateChildFreshGlobalCaptureProvided
        && CommonIndexedMaskLifecycle?.UnreachedChildGlobalCaptureProvided != true
        && (CommonIndexedMaskLifecycle is null
            ? CleanBypass
            : ExactCleanAdmission)
        && ParentWrites.SequenceEqual(ParentLifecycle.OrderedWrites)
        && IndexedMaskWrites.IsEmpty;

    public uint StopAtRawFileOffsetExclusive =>
        CommonIndexedMaskLifecycle?.StopAtRawFileOffsetExclusive
        ?? ParentLifecycle.StopAtRawFileOffsetExclusive;

    private static CapacityFinalCalleeCommonMaskSelectionGateFlags UnknownFlags =>
        new(null, null, null, null, null, null);
}

/// <summary>
/// Joins the exact below-threshold 92F6 edge to the indexed mask gate without
/// repeating the size gate or entering either bounded destination.
/// </summary>
public static class CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection.EntryRawFileOffset;
    public const uint IndexedAddressEntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryRawFileOffset;
    public const uint MaskSelectionEntryRawFileOffset =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.EntryRawFileOffset;
    public const uint OpaqueCallRawFileOffset =
        CapacityFinalCalleeCommonIndexedAddressGateProjection.OpaqueCallRawFileOffset;
    public const uint ZeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.ZeroDestinationRawFileOffset;
    public const uint NonzeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.NonzeroDestinationRawFileOffset;
    public const uint OuterCalleeEntryRawFileOffset = 0x012C8E76;
    public const uint FreshGlobalVirtualAddress =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.FreshGlobalVirtualAddress;

    public const string CapturedIndexedAddressWindowBytes =
        CapacityFinalCalleeCommonIndexedAddressGateProjection.CapturedWindowBytes;
    public const string CapturedMaskSelectionWindowBytes =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.CapturedWindowBytes;

    public static ImmutableArray<
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan Project(
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan parent =
            CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection.Project(
                capture.Parent);

        if (!IsExactParentBelow92F6(parent))
        {
            CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome outcome =
                capture.LateChildFreshGlobalValue.HasValue
                    ? CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                        .ParentOtherRouteWithUnusedLateChildFreshGlobalValue
                    : parent.Outcome switch
                    {
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .ParentStopped
                        or CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .ParentStoppedWithUnusedFreshRootGlobalDWORD =>
                            CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                                .ParentStopped,
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .EqualMissingDerivedEDI =>
                            CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                                .EqualMissingDerivedEDI,
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .EqualRootStoredAndJumped =>
                            CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                                .EqualRootStoredAndJumped,
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .UnequalMissingDerivedEDI =>
                            CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                                .UnequalMissingDerivedEDI,
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                            .UnequalAtOrAboveUnsignedThresholdTo9354 =>
                            CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                                .UnequalAtOrAboveUnsignedThresholdTo9354,
                        _ => CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                            .ParentNotClean
                    };
            return new(capture, parent, null, outcome)
            {
                CombinedOrderedMemoryEffects =
                    BuildCombinedOrderedMemoryEffects(parent, child: null)
            };
        }

        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan child =
            CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.Project(new(
                new(
                    EntryECX: parent.SizeGateCount,
                    EntryEDI: parent.DerivedEDI),
                FreshGlobalValue: capture.LateChildFreshGlobalValue));
        ImmutableArray<CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffect>
            orderedMemoryEffects = BuildCombinedOrderedMemoryEffects(parent, child);

        CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome childOutcome =
            child.Outcome switch
            {
                CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentLowEDIOpaqueStop =>
                    capture.LateChildFreshGlobalValue.HasValue
                        ? CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                            .UnequalBelowStopsBeforeOpaque9304WithUnusedFreshGlobalValue
                        : CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                            .UnequalBelowStopsBeforeOpaque9304,
                CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ChildMissingFreshGlobalValue =>
                    CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                        .UnequalBelowMissingFreshGlobalValue,
                CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ChildZeroTestToBounded931B =>
                    CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                        .UnequalBelowZeroTestToBounded931B,
                CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ChildNonzeroTestToBounded9334 =>
                    CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                        .UnequalBelowNonzeroTestToBounded9334,
                _ => CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome.ParentNotClean
            };

        return new(capture, parent, child, childOutcome)
        {
            CombinedOrderedMemoryEffects = orderedMemoryEffects
        };
    }

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        CapacityFinalCalleeCommonIndexedAddressGateProjection
            .RawFileOffsetToVirtualAddress(rawFileOffset);

    internal static bool IsExactParentBelow92F6(
        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonTailSizeGatePlan? gate = parent.UnequalSizeGate;
        return parent.Outcome
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .UnequalBelowUnsignedThresholdTo92F6
            && parent.ExactCleanParentAdmission
            && parent.UnequalRouteSelected
            && parent.ExactCleanCompletion
            && gate is
            {
                EntryEDICaptured: true,
                MoveReached: true,
                ShiftReached: true,
                ShiftInput: { } input,
                ShiftCount: CapacityFinalCalleeCommonTailSizeGateProjection.ShiftCount,
                ShiftResult: { } count,
                CompareReached: true,
                CompareLeft: { } comparedCount,
                CompareRight:
                    CapacityFinalCalleeCommonTailSizeGateProjection.CompareThreshold,
                CompareFlags: not null,
                JnbReached: true,
                JnbTaken: false,
                JnbSelectedDestinationRawFileOffset:
                    CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryRawFileOffset,
                ExactCleanAdmission: true,
                TargetEntered: false
            }
            && parent.DerivedEDI is uint originalEDI
            && gate.Capture.EntryEDI == originalEDI
            && input == originalEDI
            && count <= 31
            && count == (originalEDI >> 3)
            && comparedCount == count
            && !parent.EqualRootStoreEntered
            && !parent.UnequalFallthroughEntered
            && !parent.UnequalBranchTargetEntered
            && parent.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryRawFileOffset
            && gate.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryRawFileOffset;
    }

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffect>
        BuildCombinedOrderedMemoryEffects(
            CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan parent,
            CapacityFinalCalleeCommonIndexedMaskLifecyclePlan? child)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffect>();

        foreach (CapacityFinalCalleeLargeRightTagRootLifecycleRead read in parent.Reads)
        {
            effects.Add(new(
                CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind.ParentRead,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.Value,
                read.ValueCaptured,
                ReadAttempted: true,
                WriteAttempted: false,
                Committed: false,
                ForwardedFromPriorWrite: false));
        }

        foreach (CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite write
            in parent.OrderedWrites)
        {
            effects.Add(new(
                CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind.ParentWrite,
                write.RawFileOffset,
                write.Address,
                AddressCaptured: true,
                Value: write.Value,
                ValueCaptured: true,
                ReadAttempted: false,
                WriteAttempted: true,
                Committed: true,
                ForwardedFromPriorWrite: false));
        }

        ImmutableArray<CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffect>
            orderedParentEffects = effects
                .OrderBy(effect => effect.RawFileOffset)
                .ToImmutableArray();

        if (child?.FreshGlobalReadReached == true)
        {
            return orderedParentEffects.Add(new(
                CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind
                    .LateChildFreshGlobalRead,
                CapacityFinalCalleeCommonMaskSelectionGateProjection
                    .FreshGlobalMoveRawFileOffset,
                CapacityFinalCalleeCommonMaskSelectionGateProjection
                    .FreshGlobalVirtualAddress,
                AddressCaptured: true,
                Value: child.Capture.FreshGlobalValue,
                ValueCaptured: child.FreshGlobalValueCaptured,
                ReadAttempted: true,
                WriteAttempted: false,
                Committed: false,
                ForwardedFromPriorWrite: false));
        }

        return orderedParentEffects;
    }

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleInstructionEvidence>
        BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleInstructionEvidence>();

        foreach (CapacityFinalCalleeLargeRightRootSuccessorLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection.Instructions)
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

        foreach (CapacityFinalCalleeCommonIndexedAddressGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonIndexedAddressGateProjection.Instructions)
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

        foreach (CapacityFinalCalleeCommonMaskSelectionGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonMaskSelectionGateProjection.Instructions)
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
