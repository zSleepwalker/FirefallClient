using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleCapture(
    CapacityFinalCalleeLargeSavedFrameCommonLifecycleCapture ParentCapture,
    uint? FreshGlobalDWORD);

public enum CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
{
    LargeSavedFrameCommonLifecycleIncomplete,
    LargeSavedFrameCommonLifecycleIncompleteWithUnreachedFreshGlobalDWORD,
    NonzeroTo9231,
    NonzeroTo9231WithUnreachedCaptures,
    ParentHandoffRejected,
    RootGateMissingFreshGlobalDWORD,
    RootGateEqualToFallthrough92D0,
    RootGateUnequalToBranch92EC
}

public enum CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind
{
    Read,
    Write
}

public enum CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage
{
    SavedFrameGate,
    CommonTailTagWrites,
    CommonTailRootGate
}

public readonly record struct CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccess(
    CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage Stage,
    CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan(
    CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleCapture Capture,
    CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan Parent,
    CapacityFinalCalleeCommonTailRootGatePlan? RootGate,
    CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome Outcome)
{
    public bool ExactCleanParentExit { get; init; }
    public bool RootGateReached => RootGate is not null;
    public bool ChildEntryESIRetained => RootGate?.EntryESICaptured == true;
    public bool ChildEntryESIDerived => ChildEntryESIRetained;
    public uint? ChildEntryESI => ChildEntryESIRetained
        ? RootGate?.Capture.EntryESI
        : null;
    public bool FreshGlobalReadReached => RootGate?.FreshGlobalReadReached == true;
    public uint? FreshGlobalAddress => RootGate?.FreshGlobalAddress;
    public bool FreshGlobalCaptured => RootGate?.FreshGlobalCaptured == true;
    public uint? FreshGlobalDWORD => RootGate?.FreshGlobalDWORD;
    public bool UnreachedFreshGlobalCaptureProvided =>
        !RootGateReached && Capture.FreshGlobalDWORD.HasValue;
    public CapacityFinalCalleeLargeSavedFrameCommonLifecycleFlags? ParentFlagsAtStop =>
        Parent.FlagsAtStop;
    public CapacityFinalCalleeCommonTailRootGateFlags? RootCompareFlags =>
        RootGate?.CompareFlags;
    public CapacityFinalCalleeLargeSavedFrameCommonLifecycleFlags? FlagsAtStop =>
        RootCompareFlags is { } rootFlags
            ? new(
                rootFlags.Carry,
                rootFlags.Parity,
                rootFlags.Auxiliary,
                rootFlags.Zero,
                rootFlags.Sign,
                rootFlags.Overflow)
            : ParentFlagsAtStop;
    public ImmutableArray<CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccess>
        OrderedMemoryAccesses { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccess>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleWrite>
        OrderedWrites { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleWrite>.Empty;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the saved-frame/common-tag lifecycle to the bounded CMP/JNZ at 92C8.
/// The child ESI comes only from the completed parent tag stores; the global
/// DWORD is a separate fresh read and is never inferred from those writes.
/// </summary>
public static class CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeSavedFrameGateProjection.EntryRawFileOffset;
    public const uint CommonTailCompletedBoundaryRawFileOffset =
        CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.CompletedBoundaryRawFileOffsetExclusive;
    public const uint RootCompareRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.CompareRawFileOffset;
    public const uint RootFallthroughRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset;
    public const uint RootBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset;
    public const uint FreshGlobalDWORDVirtualAddress =
        CapacityFinalCalleeCommonTailRootGateProjection.FreshGlobalDWORDVirtualAddress;
    public const uint NonzeroStopRawFileOffsetExclusive =
        CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.NonzeroDestinationRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailRootGateProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan Project(
        CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan parent =
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.Project(
                capture.ParentCapture);
        var accesses =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccess>();
        var writes =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleWrite>();

        foreach (CapacityFinalCalleeLargeSavedFrameGateRead read
            in parent.LargeSavedFrameGate.Reads)
        {
            accesses.Add(new(
                CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage.SavedFrameGate,
                CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind.Read,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value));
        }

        foreach (CapacityFinalCalleeLargeSavedFrameCommonLifecycleWrite write
            in parent.OrderedWrites)
        {
            accesses.Add(new(
                CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage
                    .CommonTailTagWrites,
                CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind.Write,
                write.RawFileOffset,
                write.Address,
                AddressCaptured: true,
                ValueCaptured: true,
                Value: write.Value));
            writes.Add(new(write.RawFileOffset, write.Address, write.Value));
        }

        if (IsNonzeroExit(parent))
        {
            bool unreachedCaptures =
                capture.FreshGlobalDWORD.HasValue
                || parent.UnreachedCallerEntryEDICaptureProvided
                || parent.UnreachedCallerEntryESICaptureProvided;
            return new(
                capture,
                parent,
                RootGate: null,
                unreachedCaptures
                    ? CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                        .NonzeroTo9231WithUnreachedCaptures
                    : CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                        .NonzeroTo9231)
            {
                ExactCleanParentExit =
                    parent.ExactCleanExitAdmitted && !unreachedCaptures,
                OrderedMemoryAccesses = accesses.ToImmutable(),
                OrderedWrites = writes.ToImmutable(),
                ExactCleanExitAdmitted =
                    parent.ExactCleanExitAdmitted && !unreachedCaptures,
                StopAtRawFileOffsetExclusive = NonzeroStopRawFileOffsetExclusive
            };
        }

        if (!IsExactCleanZeroExitToRoot(parent))
        {
            bool unreachedGlobal = capture.FreshGlobalDWORD.HasValue;
            return new(
                capture,
                parent,
                RootGate: null,
                unreachedGlobal
                    ? CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                        .LargeSavedFrameCommonLifecycleIncompleteWithUnreachedFreshGlobalDWORD
                    : CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                        .LargeSavedFrameCommonLifecycleIncomplete)
            {
                OrderedMemoryAccesses = accesses.ToImmutable(),
                OrderedWrites = writes.ToImmutable(),
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeCommonTailTagWritesPlan commonTail =
            parent.CommonTail!;
        uint retainedEntryESI = commonTail.Capture.EntryESI!.Value;
        CapacityFinalCalleeCommonTailRootGatePlan rootGate =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: retainedEntryESI,
                FreshGlobalDWORD: capture.FreshGlobalDWORD));

        if (rootGate.FreshGlobalReadReached
            && rootGate.FreshGlobalAddress is uint globalAddress)
        {
            accesses.Add(new(
                CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage
                    .CommonTailRootGate,
                CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind.Read,
                RootCompareRawFileOffset,
                globalAddress,
                AddressCaptured: true,
                ValueCaptured: rootGate.FreshGlobalCaptured,
                Value: rootGate.FreshGlobalDWORD));
        }

        bool exactRootExit = IsExactCleanRootExit(rootGate);
        CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome outcome =
            rootGate.Outcome switch
            {
                CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD =>
                    CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                        .RootGateMissingFreshGlobalDWORD,
                CapacityFinalCalleeCommonTailRootGateOutcome.EqualToFallthrough =>
                    CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                        .RootGateEqualToFallthrough92D0,
                CapacityFinalCalleeCommonTailRootGateOutcome.UnequalToBranchTarget =>
                    CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                        .RootGateUnequalToBranch92EC,
                _ => throw new InvalidOperationException(
                    "The common-tail root gate returned an unexpected outcome.")
            };

        return new(capture, parent, rootGate, outcome)
        {
            ExactCleanParentExit = true,
            OrderedMemoryAccesses = accesses.ToImmutable(),
            OrderedWrites = writes.ToImmutable(),
            ExactCleanExitAdmitted = exactRootExit,
            StopAtRawFileOffsetExclusive =
                rootGate.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsNonzeroExit(
        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome.NonzeroTo9231
        && plan.LargeSavedFrameGate.Outcome
            == CapacityFinalCalleeLargeSavedFrameGateOutcome.NonzeroToFallthrough
        && plan.LargeSavedFrameGate.FreshReadReached
        && plan.LargeSavedFrameGate.TestReached
        && plan.LargeSavedFrameGate.JzReached
        && plan.LargeSavedFrameGate.JzTaken == false
        && plan.LargeSavedFrameGate.JzSelectedDestinationRawFileOffset
            == NonzeroStopRawFileOffsetExclusive
        && plan.CommonTail is null
        && plan.StopAtRawFileOffsetExclusive == NonzeroStopRawFileOffsetExclusive;

    private static bool IsExactCleanZeroExitToRoot(
        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan plan)
    {
        CapacityFinalCalleeCommonTailTagWritesPlan? commonTail = plan.CommonTail;
        return plan.Outcome
                == CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome.Completed
            && plan.ExactCleanHandoffToCommonTail
            && plan.CommonTailReached
            && plan.ExactCleanExitAdmitted
            && plan.StopAtRawFileOffsetExclusive == CommonTailCompletedBoundaryRawFileOffset
            && commonTail is
            {
                Outcome: CapacityFinalCalleeCommonTailTagWritesOutcome.Completed,
                EntryEDICaptured: true,
                EntryESICaptured: true,
                FirstStoreReached: true,
                SecondStoreReached: true,
                ExactCleanExitAdmitted: true,
                UnreachedEntryESICaptureProvided: false,
                StopAtRawFileOffsetExclusive: var stopAt
            }
            && stopAt == CommonTailCompletedBoundaryRawFileOffset
            && commonTail.Writes.Length == 2
            && commonTail.Writes[0].RawFileOffset
                == CapacityFinalCalleeCommonTailTagWritesProjection
                    .FirstStoreRawFileOffset
            && commonTail.Writes[1].RawFileOffset
                == CapacityFinalCalleeCommonTailTagWritesProjection
                    .SecondStoreRawFileOffset
            && commonTail.Capture.EntryESI.HasValue;
    }

    private static bool IsExactCleanRootExit(
        CapacityFinalCalleeCommonTailRootGatePlan plan) =>
        (plan.Outcome is
            CapacityFinalCalleeCommonTailRootGateOutcome.EqualToFallthrough
            or CapacityFinalCalleeCommonTailRootGateOutcome.UnequalToBranchTarget)
        && plan.EntryESICaptured
        && plan.Capture.EntryESI.HasValue
        && plan.FreshGlobalReadReached
        && plan.FreshGlobalAddress == FreshGlobalDWORDVirtualAddress
        && plan.FreshGlobalCaptured
        && plan.FreshGlobalDWORD.HasValue
        && plan.CompareReached
        && plan.CompareResult.HasValue
        && plan.CompareFlags.HasValue
        && plan.JnzReached
        && plan.JnzTaken == (plan.CompareResult != 0)
        && plan.JnzSelectedDestinationRawFileOffset
            == (plan.JnzTaken == true
                ? RootBranchTargetRawFileOffset
                : RootFallthroughRawFileOffset)
        && plan.ExactCleanAdmission
        && plan.Writes.IsEmpty
        && !plan.OpaqueEntryReached
        && plan.StopAtRawFileOffsetExclusive
            == plan.JnzSelectedDestinationRawFileOffset;
}
