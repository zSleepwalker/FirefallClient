using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonZeroSlotStoresLifecycleCapture(
    uint? ParentEntryECX,
    uint? FreshGlobalValue,
    uint? LateEntryESI,
    uint? LateEntryEDX);

public enum CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
{
    ThresholdCompareOperandsMissing,
    UnsignedBelowBranchTarget,
    MissingLateEntryESIForFirstStore,
    MissingLateEntryEDXForSecondStore,
    CompletedToCleanupJump
}

public sealed record CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan(
    CapacityFinalCalleeCommonZeroSlotStoresLifecycleCapture Capture,
    CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonZeroSlotThresholdGatePlan ThresholdGate { get; init; } =
        null!;
    public CapacityFinalCalleeCommonZeroSlotStoreTailPlan? StoreTail { get; init; }
    public bool ChildReached { get; init; }
    public uint? ChildEntryECX { get; init; }
    public CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? ParentCompareFlags =>
        ThresholdGate.CompareFlags;
    public CapacityFinalCalleeCommonZeroSlotStoreTailFlags? ChildFlags { get; init; }
    public bool ChildFlagsDerivedFromParentCompare =>
        ChildReached && ParentCompareFlags.HasValue && ChildFlags.HasValue;
    public bool ChildFlagsUserProvided => false;
    public bool ParentCompareFlagsPreservedThroughChild =>
        ChildReached
        && ChildFlagsDerivedFromParentCompare
        && StoreTail is { FlagsPreserved: true }
        && StoreTail.FlagsAtStop == ChildFlags;
    public bool LateEntryESICaptureVisible => Capture.LateEntryESI.HasValue;
    public bool LateEntryEDXCaptureVisible => Capture.LateEntryEDX.HasValue;
    public bool LateEntryESIUnusedBecauseThresholdBypassed { get; init; }
    public bool LateEntryEDXUnusedBecauseThresholdBypassed { get; init; }
    public bool LateEntryEDXUnusedBecauseFirstStoreBlocked =>
        StoreTail?.LateEntryEDXUnusedBecauseFirstStoreBlocked == true;
    public bool StoreTailEntryReached => ChildReached;
    public bool StoreTailEntryInstructionEntered =>
        StoreTail?.FirstStoreAttempted == true;
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
    public bool CleanupJumpReached => StoreTail?.JumpReached == true;
    public uint? CleanupJumpTargetRawFileOffset => StoreTail?.JumpTargetRawFileOffset;
    public uint? CleanupJumpTargetVirtualAddress => StoreTail?.JumpTargetVirtualAddress;
    public bool CleanupJumpTargetEntered => StoreTail?.JumpTargetEntered == true;
    public CapacityFinalCalleeCommonZeroSlotStoreTailFlags? FlagsAtStop { get; init; }
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion =>
        ExactBoundedCompletion
        && !LateEntryESIUnusedBecauseThresholdBypassed
        && !LateEntryEDXUnusedBecauseThresholdBypassed
        && StoreTail?.UnreachedLateEntryEDXCaptureProvided != true;
    public bool DestinationEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the post-RET zero-slot CMP/JB into the ordered zero-slot stores and cleanup jump.
/// </summary>
public static class CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.EntryRawFileOffset;
    public const uint ThresholdGateBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint StoreTailEntryRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotStoreTailProjection.EntryRawFileOffset;
    public const uint BelowBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint CleanupJumpTargetRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpTargetRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonZeroSlotStoreTailProjection.BoundaryRawFileOffsetExclusive;
    public const string CapturedWindowBytes =
        "3B 0D D8 AD ED 01 0F 82 0E FA FF FF 89 31 89 56 18 89 76 0C 89 76 08 EB BA";

    public static CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan Project(
        CapacityFinalCalleeCommonZeroSlotStoresLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan parent =
            CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.Project(new(
                EntryECX: capture.ParentEntryECX,
                FreshGlobalValue: capture.FreshGlobalValue));

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan Finish(
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome outcome,
            bool childReached,
            CapacityFinalCalleeCommonZeroSlotStoreTailPlan? child,
            uint stopAt,
            bool exactBoundedCompletion = false) =>
            new(capture, outcome)
            {
                ThresholdGate = parent,
                StoreTail = child,
                ChildReached = childReached,
                ChildEntryECX = childReached ? parent.CompareLeft : null,
                ChildFlags = childReached ? ToStoreTailFlags(parent.CompareFlags) : null,
                LateEntryESIUnusedBecauseThresholdBypassed =
                    !childReached && capture.LateEntryESI.HasValue,
                LateEntryEDXUnusedBecauseThresholdBypassed =
                    !childReached && capture.LateEntryEDX.HasValue,
                FlagsAtStop = child?.FlagsAtStop ?? ToStoreTailFlags(parent.FlagsAtStop),
                ExactBoundedCompletion = exactBoundedCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        bool cleanBelowRoute =
            parent.ExactCleanCompletion
            && parent.BranchReached
            && parent.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                    .UnsignedBelowBranchTarget
            && parent.BranchTaken == true
            && parent.BranchSelectedDestinationRawFileOffset == BelowBranchTargetRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == BelowBranchTargetRawFileOffset;
        if (cleanBelowRoute)
        {
            return Finish(
                CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome.UnsignedBelowBranchTarget,
                childReached: false,
                child: null,
                stopAt: BelowBranchTargetRawFileOffset,
                exactBoundedCompletion: true);
        }

        bool cleanChildRoute =
            parent.ExactCleanCompletion
            && parent.CompareInstructionReached
            && parent.CompareReached
            && parent.GlobalReadReached
            && parent.FreshGlobalValueCaptured
            && parent.EntryECXCaptured
            && parent.CompareFlags.HasValue
            && !parent.TargetEntered
            && parent.BranchReached
            && parent.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                    .UnsignedNotBelowFallthrough
            && parent.BranchTaken == false
            && parent.BranchSelectedDestinationRawFileOffset == StoreTailEntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == StoreTailEntryRawFileOffset;
        if (!cleanChildRoute)
        {
            return Finish(
                CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                    .ThresholdCompareOperandsMissing,
                childReached: false,
                child: null,
                stopAt: parent.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan child =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(
                EntryECX: parent.CompareLeft,
                EntryESI: capture.LateEntryESI,
                LateEntryEDX: capture.LateEntryEDX,
                IncomingFlags: ToStoreTailFlags(parent.CompareFlags)));

        return child.Outcome switch
        {
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingEntryESIForFirstStore =>
                Finish(
                    CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                        .MissingLateEntryESIForFirstStore,
                    childReached: true,
                    child: child,
                    stopAt: child.StopAtRawFileOffsetExclusive),
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome
                .MissingLateEntryEDXForSecondStore =>
                Finish(
                    CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                        .MissingLateEntryEDXForSecondStore,
                    childReached: true,
                    child: child,
                    stopAt: child.StopAtRawFileOffsetExclusive),
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget =>
                Finish(
                    CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                        .CompletedToCleanupJump,
                    childReached: true,
                    child: child,
                    stopAt: child.StopAtRawFileOffsetExclusive,
                    exactBoundedCompletion: child.ExactBoundedExitAdmitted
                        && child.Writes.Length == 4
                        && child.Writes.All(write => write.Committed)
                        && child.JumpReached
                        && child.JumpTargetRawFileOffset == CleanupJumpTargetRawFileOffset
                        && child.StopAtRawFileOffsetExclusive == CleanupJumpTargetRawFileOffset
                        && !child.JumpTargetEntered
                        && !child.UnreachedLateEntryEDXCaptureProvided),
            _ => throw new InvalidOperationException(
                "The zero-slot store tail cannot lack its derived ECX destination.")
        };
    }

    private static CapacityFinalCalleeCommonZeroSlotStoreTailFlags? ToStoreTailFlags(
        CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonZeroSlotThresholdGateFlags known
            ? new(
                Carry: known.Carry,
                Parity: known.Parity,
                AuxiliaryCarry: known.AuxiliaryCarry,
                Zero: known.Zero,
                Sign: known.Sign,
                Overflow: known.Overflow)
            : null;
}
