using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonRootStoreLifecycleCapture(
    uint? EntryESI,
    uint? FreshGlobalDWORD,
    uint? CallerEDI);

public enum CapacityFinalCalleeCommonRootStoreLifecycleOutcome
{
    MissingEntryESIAndFreshGlobalDWORD,
    MissingEntryESI,
    MissingFreshGlobalDWORD,
    MissingParentWithUnusedCallerEDI,
    UnequalToBranchTarget,
    UnequalWithUnusedCallerEDI,
    MissingCallerEDI,
    StoredAndJumped,
    ParentNotClean
}

public enum CapacityFinalCalleeCommonRootStoreLifecycleAccessKind
{
    Read,
    Write
}

public readonly record struct CapacityFinalCalleeCommonRootStoreLifecycleAccess(
    CapacityFinalCalleeCommonRootStoreLifecycleAccessKind Kind,
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct CapacityFinalCalleeCommonRootStoreLifecycleWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeCommonRootStoreLifecyclePlan(
    CapacityFinalCalleeCommonRootStoreLifecycleCapture Capture,
    CapacityFinalCalleeCommonTailRootGatePlan Parent,
    CapacityFinalCalleeCommonTailRootStorePlan? Child,
    CapacityFinalCalleeCommonRootStoreLifecycleOutcome Outcome)
{
    public bool ExactCleanParentAdmission { get; init; }
    public bool CallerEDICaptureReached { get; init; }
    public bool CallerEDICaptureProvidedButUnreached { get; init; }
    public bool FlagsPreservedThroughChild { get; init; }
    public CapacityFinalCalleeCommonTailRootGateFlags? FlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonRootStoreLifecycleAccess>
        OrderedMemoryAccesses { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonRootStoreLifecycleAccess>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonRootStoreLifecycleWrite> Writes
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonRootStoreLifecycleWrite>.Empty;
    public bool ExactCleanExitAdmitted { get; init; }
    public bool EpilogueEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the common-tail root comparison to its equal-only store and bounded exit.
/// </summary>
public static class CapacityFinalCalleeCommonRootStoreLifecycleProjection
{
    public const uint GlobalReadRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.CompareRawFileOffset;
    public const uint GlobalReadVirtualAddress =
        CapacityFinalCalleeCommonTailRootGateProjection.FreshGlobalDWORDVirtualAddress;
    public const uint EqualStoreRawFileOffset =
        CapacityFinalCalleeCommonTailRootStoreProjection.StoreRawFileOffset;
    public const uint EqualStoreAddress =
        CapacityFinalCalleeCommonTailRootStoreProjection.RootStorePEVirtualAddress;
    public const uint UnequalStopRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset;
    public const uint CompletedStopRawFileOffset =
        CapacityFinalCalleeCommonTailRootStoreProjection.JumpTargetRawFileOffset;

    public static CapacityFinalCalleeCommonRootStoreLifecyclePlan Project(
        CapacityFinalCalleeCommonRootStoreLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonTailRootGatePlan parent =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                capture.EntryESI,
                capture.FreshGlobalDWORD));

        var accesses =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonRootStoreLifecycleAccess>();
        if (parent.FreshGlobalReadReached && parent.FreshGlobalAddress is uint readAddress)
        {
            accesses.Add(new(
                CapacityFinalCalleeCommonRootStoreLifecycleAccessKind.Read,
                GlobalReadRawFileOffset,
                readAddress,
                parent.FreshGlobalCaptured ? parent.FreshGlobalDWORD : null));
        }

        if (IsExactCleanParentUnequal(parent))
        {
            bool unusedCallerEDI = capture.CallerEDI.HasValue;
            return new(
                capture,
                parent,
                null,
                unusedCallerEDI
                    ? CapacityFinalCalleeCommonRootStoreLifecycleOutcome
                        .UnequalWithUnusedCallerEDI
                    : CapacityFinalCalleeCommonRootStoreLifecycleOutcome.UnequalToBranchTarget)
            {
                ExactCleanParentAdmission = true,
                CallerEDICaptureProvidedButUnreached = unusedCallerEDI,
                FlagsAtStop = parent.CompareFlags,
                OrderedMemoryAccesses = accesses.ToImmutable(),
                ExactCleanExitAdmitted = !unusedCallerEDI,
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
        }

        if (IsExactMissingParent(parent))
        {
            bool unusedCallerEDI = capture.CallerEDI.HasValue;
            return new(
                capture,
                parent,
                null,
                unusedCallerEDI
                    ? CapacityFinalCalleeCommonRootStoreLifecycleOutcome
                        .MissingParentWithUnusedCallerEDI
                    : parent.Outcome switch
                    {
                        CapacityFinalCalleeCommonTailRootGateOutcome
                            .MissingEntryESIAndFreshGlobalDWORD =>
                            CapacityFinalCalleeCommonRootStoreLifecycleOutcome
                                .MissingEntryESIAndFreshGlobalDWORD,
                        CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI =>
                            CapacityFinalCalleeCommonRootStoreLifecycleOutcome.MissingEntryESI,
                        CapacityFinalCalleeCommonTailRootGateOutcome
                            .MissingFreshGlobalDWORD =>
                            CapacityFinalCalleeCommonRootStoreLifecycleOutcome
                                .MissingFreshGlobalDWORD,
                        _ => throw new InvalidOperationException()
                    })
            {
                CallerEDICaptureProvidedButUnreached = unusedCallerEDI,
                OrderedMemoryAccesses = accesses.ToImmutable(),
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
        }

        if (!IsExactCleanParentEqual(parent))
        {
            return new(
                capture,
                parent,
                null,
                CapacityFinalCalleeCommonRootStoreLifecycleOutcome.ParentNotClean)
            {
                CallerEDICaptureProvidedButUnreached = capture.CallerEDI.HasValue,
                OrderedMemoryAccesses = accesses.ToImmutable(),
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeCommonTailRootStorePlan child =
            CapacityFinalCalleeCommonTailRootStoreProjection.Project(new(capture.CallerEDI));
        foreach (CapacityFinalCalleeCommonTailRootStoreWrite write in child.Writes)
        {
            accesses.Add(new(
                CapacityFinalCalleeCommonRootStoreLifecycleAccessKind.Write,
                write.RawFileOffset,
                write.Address,
                write.Value));
        }

        bool childStoredAndJumped =
            child.Outcome
                == CapacityFinalCalleeCommonTailRootStoreOutcome.StoredAndJumped
                && child.EntryEDICaptured
                && child.StoreReached
                && child.StoreAddress == EqualStoreAddress
            && child.StoreValue == capture.CallerEDI
            && child.Writes.Length == 1
            && child.Writes[0].RawFileOffset == EqualStoreRawFileOffset
            && child.Writes[0].Address == EqualStoreAddress
            && child.Writes[0].Value == capture.CallerEDI
            && child.JumpReached
            && child.JumpTargetRawFileOffset == CompletedStopRawFileOffset
            && child.JumpTargetVirtualAddress
                == CapacityFinalCalleeCommonTailRootStoreProjection
                    .JumpTargetVirtualAddress
            && child.FlagsPreserved
            && child.ExactBoundedExitAdmitted
            && child.StopAtRawFileOffsetExclusive == CompletedStopRawFileOffset
            && !child.EpilogueEntered;
        if (child.Outcome
                == CapacityFinalCalleeCommonTailRootStoreOutcome.StoredAndJumped
            && !childStoredAndJumped)
        {
            throw new InvalidOperationException(
                "The common root store did not reach its exact bounded 9441 exit.");
        }

        ImmutableArray<CapacityFinalCalleeCommonRootStoreLifecycleWrite> writes =
            child.Writes.Select(write =>
                new CapacityFinalCalleeCommonRootStoreLifecycleWrite(
                    write.RawFileOffset,
                    write.Address,
                    write.Value))
                .ToImmutableArray();
        return new(
            capture,
            parent,
            child,
            childStoredAndJumped
                ? CapacityFinalCalleeCommonRootStoreLifecycleOutcome.StoredAndJumped
                : CapacityFinalCalleeCommonRootStoreLifecycleOutcome.MissingCallerEDI)
        {
            ExactCleanParentAdmission = true,
            CallerEDICaptureReached = true,
            FlagsPreservedThroughChild =
                childStoredAndJumped && child.FlagsPreserved,
            FlagsAtStop = parent.CompareFlags,
            OrderedMemoryAccesses = accesses.ToImmutable(),
            Writes = writes,
            ExactCleanExitAdmitted = childStoredAndJumped
                && child.FlagsPreserved
                && parent.CompareFlags.HasValue,
            StopAtRawFileOffsetExclusive =
                child.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactCleanParentEqual(
        CapacityFinalCalleeCommonTailRootGatePlan plan) =>
        plan.Outcome == CapacityFinalCalleeCommonTailRootGateOutcome.EqualToFallthrough
        && plan.EntryESICaptured
        && plan.FreshGlobalReadReached
        && plan.FreshGlobalCaptured
        && plan.FreshGlobalDWORD.HasValue
        && plan.CompareReached
        && plan.CompareResult == 0
        && plan.CompareFlags is { Zero: true }
        && plan.JnzReached
        && plan.JnzTaken == false
        && plan.JnzSelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset
        && plan.ExactCleanAdmission
        && plan.Writes.IsEmpty
        && !plan.OpaqueEntryReached
        && plan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset;

    private static bool IsExactCleanParentUnequal(
        CapacityFinalCalleeCommonTailRootGatePlan plan) =>
        plan.Outcome == CapacityFinalCalleeCommonTailRootGateOutcome.UnequalToBranchTarget
        && plan.EntryESICaptured
        && plan.FreshGlobalReadReached
        && plan.FreshGlobalCaptured
        && plan.FreshGlobalDWORD.HasValue
        && plan.CompareReached
        && plan.CompareResult.HasValue
        && plan.CompareResult != 0
        && plan.CompareFlags is { Zero: false }
        && plan.JnzReached
        && plan.JnzTaken == true
        && plan.JnzSelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset
        && plan.ExactCleanAdmission
        && plan.Writes.IsEmpty
        && !plan.OpaqueEntryReached
        && plan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset;

    private static bool IsExactMissingParent(
        CapacityFinalCalleeCommonTailRootGatePlan plan)
    {
        bool missingOperands =
            plan.Outcome is
                CapacityFinalCalleeCommonTailRootGateOutcome
                    .MissingEntryESIAndFreshGlobalDWORD
                or CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI
                or CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD;
        bool missingEntryESI =
            plan.Outcome is
                CapacityFinalCalleeCommonTailRootGateOutcome
                    .MissingEntryESIAndFreshGlobalDWORD
                or CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI;
        bool missingGlobal =
            plan.Outcome is
                CapacityFinalCalleeCommonTailRootGateOutcome
                    .MissingEntryESIAndFreshGlobalDWORD
                or CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD;
        return missingOperands
            && plan.FreshGlobalReadReached
            && plan.FreshGlobalAddress == GlobalReadVirtualAddress
            && plan.EntryESICaptured == plan.Capture.EntryESI.HasValue
            && plan.FreshGlobalCaptured == plan.Capture.FreshGlobalDWORD.HasValue
            && !plan.CompareReached
            && !plan.JnzReached
            && !plan.ExactCleanAdmission
            && plan.Writes.IsEmpty
            && !plan.OpaqueEntryReached
            && plan.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailRootGateProjection.CompareRawFileOffset
            && (!missingEntryESI || plan.Capture.EntryESI is null)
            && (!missingGlobal || plan.Capture.FreshGlobalDWORD is null);
    }
}
