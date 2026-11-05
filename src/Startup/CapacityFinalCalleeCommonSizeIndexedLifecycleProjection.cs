using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonSizeIndexedLifecycleCapture(
    uint? EntryEDI);

public enum CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
{
    MissingEntryEDI,
    ParentSizeGateAtOrAboveThresholdToBounded9354,
    ChildAddressGateBelowThresholdToOpaque9304,
    ChildAddressGateAtOrAboveThresholdToBounded930A
}

public readonly record struct CapacityFinalCalleeCommonSizeIndexedLifecycleCompareFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public sealed record CapacityFinalCalleeCommonSizeIndexedLifecyclePlan(
    CapacityFinalCalleeCommonSizeIndexedLifecycleCapture Capture,
    CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonTailSizeGatePlan ParentSizeGate { get; init; } = null!;
    public CapacityFinalCalleeCommonIndexedAddressGatePlan? ChildAddressGate { get; init; }
    public bool ChildGateReached => ChildAddressGate is not null;
    public uint? ChildECX => ChildAddressGate?.Capture.EntryECX;
    public uint? ChildEDIAtCompare =>
        ChildAddressGate?.CompareReached == true
            ? ChildAddressGate.CompareLeft
            : null;
    public uint? DerivedAddress =>
        ChildAddressGate?.MoveEAXReached == true
            ? ChildAddressGate.DerivedEAX
            : null;
    public CapacityFinalCalleeCommonTailSizeGateCompareFlags? ParentCompareFlags =>
        ParentSizeGate?.CompareFlags;
    public CapacityFinalCalleeCommonIndexedAddressGateFlags? ChildCompareFlags =>
        ChildAddressGate?.CompareFlags;
    public CapacityFinalCalleeCommonSizeIndexedLifecycleCompareFlags? LastCompareFlags
    {
        get
        {
            if (ChildAddressGate?.CompareFlags is
                CapacityFinalCalleeCommonIndexedAddressGateFlags childFlags)
            {
                return new(
                    childFlags.Carry,
                    childFlags.Parity,
                    childFlags.Auxiliary,
                    childFlags.Zero,
                    childFlags.Sign,
                    childFlags.Overflow);
            }

            if (ParentSizeGate?.CompareFlags is
                CapacityFinalCalleeCommonTailSizeGateCompareFlags parentFlags)
            {
                return new(
                    parentFlags.Carry,
                    parentFlags.Parity,
                    parentFlags.Auxiliary,
                    parentFlags.Zero,
                    parentFlags.Sign,
                    parentFlags.Overflow);
            }

            return null;
        }
    }
    public bool ParentExactCleanAdmission => ParentSizeGate?.ExactCleanAdmission == true;
    public bool ChildExactCleanBoundedEdge =>
        ChildAddressGate?.ExactCleanBoundedEdge == true;
    public bool ExactCleanOutcome =>
        ParentExactCleanAdmission &&
        (ChildAddressGate is null || ChildExactCleanBoundedEdge);
    public bool OpaqueCallInstructionAtStop =>
        ChildAddressGate?.OpaqueCallInstructionAtStop == true;
    public bool OpaqueCallEntered => false;
    public bool MemoryReadPerformed => false;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the bounded 92EC size gate with the 92F6 indexed-address gate,
/// without entering the opaque call or reading or writing memory.
/// </summary>
public static class CapacityFinalCalleeCommonSizeIndexedLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonTailSizeGateProjection.EntryRawFileOffset;
    public const uint ParentBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonTailSizeGateProjection.BranchTargetRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryRawFileOffset;
    public const uint ChildOpaqueCallRawFileOffset =
        CapacityFinalCalleeCommonIndexedAddressGateProjection.OpaqueCallRawFileOffset;
    public const uint ChildBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonIndexedAddressGateProjection.BranchTargetRawFileOffset;

    public static CapacityFinalCalleeCommonSizeIndexedLifecyclePlan Project(
        CapacityFinalCalleeCommonSizeIndexedLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonTailSizeGatePlan parent =
            CapacityFinalCalleeCommonTailSizeGateProjection.Project(
                new CapacityFinalCalleeCommonTailSizeGateCapture(capture.EntryEDI));

        if (capture.EntryEDI is null)
        {
            return Finish(
                CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome.MissingEntryEDI,
                parent,
                child: null,
                parent.StopAtRawFileOffsetExclusive);
        }

        if (parent.ExactCleanAdmission
            && parent.JnbTaken == true
            && parent.JnbSelectedDestinationRawFileOffset == ParentBranchTargetRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == ParentBranchTargetRawFileOffset)
        {
            return Finish(
                CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
                    .ParentSizeGateAtOrAboveThresholdToBounded9354,
                parent,
                child: null,
                parent.StopAtRawFileOffsetExclusive);
        }

        if (!parent.ExactCleanAdmission
            || parent.JnbTaken != false
            || parent.JnbSelectedDestinationRawFileOffset != ChildEntryRawFileOffset
            || parent.StopAtRawFileOffsetExclusive != ChildEntryRawFileOffset
            || parent.ShiftResult is not uint childEcx)
        {
            throw new InvalidOperationException("The evidenced size gate must select an exact bounded exit.");
        }
        CapacityFinalCalleeCommonIndexedAddressGatePlan child =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(
                new CapacityFinalCalleeCommonIndexedAddressGateCapture(
                    childEcx,
                    capture.EntryEDI));

        CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome outcome =
            child.JnbTaken == true
                ? CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
                    .ChildAddressGateAtOrAboveThresholdToBounded930A
                : CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
                    .ChildAddressGateBelowThresholdToOpaque9304;

        return Finish(
            outcome,
            parent,
            child,
            child.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeCommonSizeIndexedLifecyclePlan Finish(
        CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome outcome,
        CapacityFinalCalleeCommonTailSizeGatePlan parent,
        CapacityFinalCalleeCommonIndexedAddressGatePlan? child,
        uint stopAt) =>
        new(new CapacityFinalCalleeCommonSizeIndexedLifecycleCapture(
                parent.Capture.EntryEDI),
            outcome)
        {
            ParentSizeGate = parent,
            ChildAddressGate = child,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
