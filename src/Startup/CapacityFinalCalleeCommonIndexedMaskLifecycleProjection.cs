namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedMaskLifecycleCapture(
    CapacityFinalCalleeCommonIndexedAddressGateCapture ParentCapture,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome
{
    ParentMissingEntryECX,
    ParentMissingEntryEDI,
    ParentLowEDIOpaqueStop,
    ChildMissingFreshGlobalValue,
    ChildZeroTestToBounded931B,
    ChildNonzeroTestToBounded9334
}

public sealed record CapacityFinalCalleeCommonIndexedMaskLifecyclePlan(
    CapacityFinalCalleeCommonIndexedMaskLifecycleCapture Capture,
    CapacityFinalCalleeCommonIndexedAddressGatePlan ParentAddressGate,
    CapacityFinalCalleeCommonMaskSelectionGatePlan? ChildMaskSelectionGate,
    CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome Outcome)
{
    public bool ChildReached => ChildMaskSelectionGate is not null;
    public bool ExactCleanHandoffToChild { get; init; }
    public bool ChildEntryECXDerivedFromReachedLEA { get; init; }
    public uint? ChildEntryECX => ChildMaskSelectionGate?.Capture.EntryECX;
    public uint? ParentDerivedEDX => ParentAddressGate.DerivedEDX;
    public uint? ParentDerivedEAX => ParentAddressGate.DerivedEAX;
    public bool FreshGlobalReadReached =>
        ChildMaskSelectionGate?.FreshGlobalReadReached == true;
    public uint? FreshGlobalReadAddress =>
        ChildMaskSelectionGate?.FreshGlobalReadAddress;
    public bool FreshGlobalValueCaptured =>
        ChildMaskSelectionGate?.FreshGlobalValueCaptured == true;
    public bool UnreachedChildGlobalCaptureProvided =>
        !ChildReached && Capture.FreshGlobalValue.HasValue;
    public CapacityFinalCalleeCommonMaskSelectionGateFlags? LatestChildFlags =>
        ChildMaskSelectionGate?.TestFlags
        ?? (ChildMaskSelectionGate?.ShiftCount == 0
            && ChildMaskSelectionGate?.TestReached != true
            ? ParentAddressGate.CompareFlags is { } parentFlags
                ? new(
                    parentFlags.Carry,
                    parentFlags.Parity,
                    parentFlags.Auxiliary,
                    parentFlags.Zero,
                    parentFlags.Sign,
                    parentFlags.Overflow)
                : ChildMaskSelectionGate?.ShiftFlags
            : ChildMaskSelectionGate?.ShiftFlags);
    public CapacityFinalCalleeCommonMaskSelectionGateFlags? FlagsAtStop =>
        ChildReached
            ? LatestChildFlags
            : ParentAddressGate.CompareFlags is { } parentFlags
                ? new(
                    parentFlags.Carry,
                    parentFlags.Parity,
                    parentFlags.Auxiliary,
                    parentFlags.Zero,
                    parentFlags.Sign,
                    parentFlags.Overflow)
                : null;
    public bool OpaqueCallInstructionAtStop =>
        ParentAddressGate.OpaqueCallInstructionAtStop;
    public bool OpaqueCallEntered => false;
    public bool OpaqueCallReturnObserved => false;
    public bool CalleeIdentityKnown => false;
    public bool ExactCleanAdmission { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the exact high-EDI edge at 930A to the common mask-selection gate.
/// The child's ECX is the original entry index that reached the parent's LEA;
/// the 1EDADC8 value is a separate fresh global capture.
/// </summary>
public static class CapacityFinalCalleeCommonIndexedMaskLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryVirtualAddress;
    public const uint ParentHandoffRawFileOffset =
        CapacityFinalCalleeCommonIndexedAddressGateProjection.BranchTargetRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.EntryRawFileOffset;
    public const uint FreshGlobalVirtualAddress =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.FreshGlobalVirtualAddress;
    public const uint ZeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.ZeroDestinationRawFileOffset;
    public const uint NonzeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonMaskSelectionGateProjection.NonzeroDestinationRawFileOffset;

    public static CapacityFinalCalleeCommonIndexedMaskLifecyclePlan Project(
        CapacityFinalCalleeCommonIndexedMaskLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonIndexedAddressGatePlan parent =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(
                capture.ParentCapture);

        if (parent.Outcome
            == CapacityFinalCalleeCommonIndexedAddressGateOutcome.MissingEntryECX)
        {
            return new(
                capture,
                parent,
                null,
                CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentMissingEntryECX)
            {
                StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
            };
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonIndexedAddressGateOutcome.MissingEntryEDI)
        {
            return new(
                capture,
                parent,
                null,
                CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentMissingEntryEDI)
            {
                StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
            };
        }

        if (!IsExactHighBranch(parent))
        {
            return new(
                capture,
                parent,
                null,
                CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentLowEDIOpaqueStop)
            {
                StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeCommonMaskSelectionGatePlan child =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(new(
                EntryECX: capture.ParentCapture.EntryECX,
                FreshGlobalValue: capture.FreshGlobalValue));
        CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome outcome =
            child.Outcome switch
            {
                CapacityFinalCalleeCommonMaskSelectionGateOutcome.MissingFreshGlobalValue =>
                    CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome
                        .ChildMissingFreshGlobalValue,
                CapacityFinalCalleeCommonMaskSelectionGateOutcome.ZeroTestResultToBounded931B =>
                    CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome
                        .ChildZeroTestToBounded931B,
                CapacityFinalCalleeCommonMaskSelectionGateOutcome
                    .NonzeroTestResultToBounded9334 =>
                    CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome
                        .ChildNonzeroTestToBounded9334,
                _ => throw new InvalidOperationException(
                    "The mask-selection child returned an unexpected outcome.")
            };

        bool exactChildStop =
            child.ExactCleanBoundedDestination
            && child.JnzSelectedDestinationRawFileOffset
                == child.StopAtRawFileOffsetExclusive
            && child.JnzSelectedDestinationRawFileOffset is
                ZeroDestinationRawFileOffset or NonzeroDestinationRawFileOffset
            && !child.LaterOrOrStoreReached
            && !child.MemoryWritePerformed
            && child.Writes.IsEmpty;

        return new(capture, parent, child, outcome)
        {
            ExactCleanHandoffToChild = true,
            ChildEntryECXDerivedFromReachedLEA =
                parent.LEAReached
                && parent.EntryECXCaptured
                && child.Capture.EntryECX == capture.ParentCapture.EntryECX,
            ExactCleanAdmission =
                exactChildStop
                && parent.LEAReached
                && parent.EntryECXCaptured
                && child.Capture.EntryECX == capture.ParentCapture.EntryECX
                && !child.UnreachedFreshGlobalCaptureProvided,
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactHighBranch(
        CapacityFinalCalleeCommonIndexedAddressGatePlan parent) =>
        parent.Outcome
            == CapacityFinalCalleeCommonIndexedAddressGateOutcome
                .AtOrAboveUnsignedThresholdToCleanBoundedEdge
        && parent.EntryECXCaptured
        && parent.LEAReached
        && parent.EntryEDICaptured
        && parent.CompareReached
        && parent.JnbReached
        && parent.JnbTaken == true
        && parent.JnbSelectedDestinationRawFileOffset == ParentHandoffRawFileOffset
        && parent.StopAtRawFileOffsetExclusive == ParentHandoffRawFileOffset
        && parent.ExactCleanBoundedEdge
        && !parent.OpaqueCallEntered
        && !parent.OpaqueCallReturnObserved
        && !parent.CalleeIdentityKnown;
}
