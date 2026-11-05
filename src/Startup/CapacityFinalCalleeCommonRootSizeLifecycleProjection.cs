using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonRootSizeLifecycleCapture(
    uint? EntryESI,
    uint? FreshGlobalDWORD,
    uint? CallerEDI);

public enum CapacityFinalCalleeCommonRootSizeLifecycleOutcome
{
    MissingEntryESIAndFreshGlobalDWORD,
    MissingEntryESI,
    MissingFreshGlobalDWORD,
    MissingParentWithUnusedCallerEDI,
    EqualToFallthrough92D0,
    EqualWithUnusedCallerEDI,
    UnequalMissingCallerEDI,
    BelowUnsignedThresholdToFallthrough92F6,
    AtOrAboveUnsignedThresholdToBranch9354,
    ParentNotClean
}

public readonly record struct CapacityFinalCalleeCommonRootSizeLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonRootSizeLifecyclePlan(
    CapacityFinalCalleeCommonRootSizeLifecycleCapture Capture,
    CapacityFinalCalleeCommonTailRootGatePlan Parent,
    CapacityFinalCalleeCommonTailSizeGatePlan? Child,
    CapacityFinalCalleeCommonRootSizeLifecycleOutcome Outcome)
{
    public bool ExactCleanParentAdmission { get; init; }
    public bool ExactCleanBranchToSizeGate { get; init; }
    public bool CallerEDICaptureReached => Child?.MoveReached == true;
    public bool CallerEDICaptureProvidedButUnreached =>
        Capture.CallerEDI.HasValue && !CallerEDICaptureReached;
    public bool CallerEDIRetained => CallerEDICaptureReached && Child?.EntryEDICaptured == true;
    public uint? RetainedCallerEDI =>
        CallerEDIRetained ? Child?.Capture.EntryEDI : null;
    public bool FreshGlobalReadReached => Parent.FreshGlobalReadReached;
    public uint? FreshGlobalAddress => Parent.FreshGlobalAddress;
    public bool FreshGlobalCaptured => Parent.FreshGlobalCaptured;
    public uint? FreshGlobalDWORD => Parent.FreshGlobalDWORD;
    public CapacityFinalCalleeCommonTailRootGateFlags? ParentCompareFlags =>
        Parent.CompareFlags;
    public CapacityFinalCalleeCommonTailSizeGateShiftFlags? ChildShiftFlags =>
        Child?.ShiftFlags;
    public CapacityFinalCalleeCommonTailSizeGateCompareFlags? ChildCompareFlags =>
        Child?.CompareFlags;
    public CapacityFinalCalleeCommonTailSizeGateCompareFlags? FlagsAtStop =>
        Child?.CompareFlags is { } childFlags
            ? childFlags
            : Parent.CompareFlags is { } parentFlags
                ? new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
                    parentFlags.Carry,
                    parentFlags.Parity,
                    parentFlags.Auxiliary,
                    parentFlags.Zero,
                    parentFlags.Sign,
                    parentFlags.Overflow)
                : null;
    public bool ParentCompareFlagsPreservedAtStop =>
        Parent.CompareFlags is { } parentFlags
        && FlagsAtStop is { } flags
        && flags == new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
            parentFlags.Carry,
            parentFlags.Parity,
            parentFlags.Auxiliary,
            parentFlags.Zero,
            parentFlags.Sign,
            parentFlags.Overflow);
    public bool ExactCleanExitAdmitted { get; init; }
    public bool TargetEntered => false;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the common-tail root compare at 92C8 to its clean unequal size gate,
/// while bounding the equal path at 92D0 and both size paths at their branch edges.
/// </summary>
public static class CapacityFinalCalleeCommonRootSizeLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeCommonTailRootGateProjection.EntryVirtualAddress;
    public const uint ParentBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailRootGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonTailSizeGateProjection.EntryRawFileOffset;
    public const uint ChildBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailSizeGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint EqualFallthroughRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset;
    public const uint UnequalBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset;
    public const uint SizeFallthroughRawFileOffset =
        CapacityFinalCalleeCommonTailSizeGateProjection.FallthroughRawFileOffset;
    public const uint SizeBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonTailSizeGateProjection.BranchTargetRawFileOffset;
    public const uint FreshGlobalDWORDVirtualAddress =
        CapacityFinalCalleeCommonTailRootGateProjection.FreshGlobalDWORDVirtualAddress;

    public const string CapturedWindowBytes =
        "3B 35 DC AD ED 01 75 1C 8B CF C1 E9 03 83 F9 20 73 5E";

    public static ImmutableArray<CapacityFinalCalleeCommonRootSizeLifecycleInstructionEvidence>
        Instructions { get; } =
            ImmutableArray.Create(
                Instruction(
                    "Compare entry ESI with the independent fresh global DWORD",
                    CapacityFinalCalleeCommonTailRootGateProjection.CompareRawFileOffset,
                    "3B 35 DC AD ED 01"),
                Instruction(
                    "Branch to 92EC when the root comparison is unequal",
                    CapacityFinalCalleeCommonTailRootGateProjection.JnzRawFileOffset,
                    "75 1C",
                    CapacityFinalCalleeCommonTailRootGateProjection
                        .BranchTargetRawFileOffset),
                Instruction(
                    "Move the independently captured caller EDI into ECX on the unequal path",
                    CapacityFinalCalleeCommonTailSizeGateProjection.MoveRawFileOffset,
                    "8B CF"),
                Instruction(
                    "Shift ECX right by three",
                    CapacityFinalCalleeCommonTailSizeGateProjection.ShiftRawFileOffset,
                    "C1 E9 03"),
                Instruction(
                    "Compare shifted ECX with unsigned threshold 20h",
                    CapacityFinalCalleeCommonTailSizeGateProjection.CompareRawFileOffset,
                    "83 F9 20"),
                Instruction(
                    "Branch to 9354 when unsigned ECX is at least 20h",
                    CapacityFinalCalleeCommonTailSizeGateProjection.JnbRawFileOffset,
                    "73 5E",
                    CapacityFinalCalleeCommonTailSizeGateProjection
                        .BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonRootSizeLifecyclePlan Project(
        CapacityFinalCalleeCommonRootSizeLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonTailRootGatePlan parent =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                capture.EntryESI,
                capture.FreshGlobalDWORD));

        if (IsExactCleanParentEqual(parent))
        {
            bool unusedCallerEDI = capture.CallerEDI.HasValue;
            return new(
                capture,
                parent,
                Child: null,
                unusedCallerEDI
                    ? CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                        .EqualWithUnusedCallerEDI
                    : CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                        .EqualToFallthrough92D0)
            {
                ExactCleanParentAdmission = true,
                ExactCleanExitAdmitted = !unusedCallerEDI,
                StopAtRawFileOffsetExclusive =
                    CapacityFinalCalleeCommonTailRootGateProjection
                        .FallthroughRawFileOffset
            };
        }

        if (IsExactCleanParentUnequal(parent))
        {
            CapacityFinalCalleeCommonTailSizeGatePlan child =
                CapacityFinalCalleeCommonTailSizeGateProjection.Project(new(
                    capture.CallerEDI));
            bool exactBranch = parent.JnzTaken == true
                && parent.JnzSelectedDestinationRawFileOffset
                    == CapacityFinalCalleeCommonTailRootGateProjection
                        .BranchTargetRawFileOffset
                && parent.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeCommonTailRootGateProjection
                        .BranchTargetRawFileOffset;

            if (child.Outcome
                    == CapacityFinalCalleeCommonTailSizeGateOutcome.MissingEntryEDI)
            {
                bool exactMissingEDIStop = exactBranch
                    && !child.EntryEDICaptured
                    && !child.MoveReached
                    && !child.ShiftReached
                    && !child.CompareReached
                    && !child.JnbReached
                    && !child.ExactCleanAdmission
                    && !child.TargetEntered
                    && child.Writes.IsEmpty
                    && child.StopAtRawFileOffsetExclusive
                        == CapacityFinalCalleeCommonTailSizeGateProjection
                            .MoveRawFileOffset;
                if (!exactMissingEDIStop)
                {
                    return ParentNotClean(capture, parent);
                }

                return new(
                    capture,
                    parent,
                    child,
                    CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                        .UnequalMissingCallerEDI)
                {
                    ExactCleanParentAdmission = true,
                    ExactCleanBranchToSizeGate = true,
                    StopAtRawFileOffsetExclusive =
                        child.StopAtRawFileOffsetExclusive
                };
            }

            bool below = IsExactCleanSizeStop(
                child,
                CapacityFinalCalleeCommonTailSizeGateOutcome
                    .BelowUnsignedThresholdToFallthrough,
                expectedTaken: false,
                CapacityFinalCalleeCommonTailSizeGateProjection
                    .FallthroughRawFileOffset);
            bool atOrAbove = IsExactCleanSizeStop(
                child,
                CapacityFinalCalleeCommonTailSizeGateOutcome
                    .AtOrAboveUnsignedThresholdToBranchTarget,
                expectedTaken: true,
                CapacityFinalCalleeCommonTailSizeGateProjection
                    .BranchTargetRawFileOffset);
            if (!exactBranch || (!below && !atOrAbove))
            {
                return ParentNotClean(capture, parent);
            }

            return new(
                capture,
                parent,
                child,
                below
                    ? CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                        .BelowUnsignedThresholdToFallthrough92F6
                    : CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                        .AtOrAboveUnsignedThresholdToBranch9354)
            {
                ExactCleanParentAdmission = true,
                ExactCleanBranchToSizeGate = true,
                ExactCleanExitAdmitted = true,
                StopAtRawFileOffsetExclusive =
                    child.StopAtRawFileOffsetExclusive
            };
        }

        if (IsExactMissingParent(parent))
        {
            bool unusedCallerEDI = capture.CallerEDI.HasValue;
            CapacityFinalCalleeCommonRootSizeLifecycleOutcome outcome =
                unusedCallerEDI
                    ? CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                        .MissingParentWithUnusedCallerEDI
                    : parent.Outcome switch
                    {
                        CapacityFinalCalleeCommonTailRootGateOutcome
                            .MissingEntryESIAndFreshGlobalDWORD =>
                            CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                                .MissingEntryESIAndFreshGlobalDWORD,
                        CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI =>
                            CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                                .MissingEntryESI,
                        CapacityFinalCalleeCommonTailRootGateOutcome
                            .MissingFreshGlobalDWORD =>
                            CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                                .MissingFreshGlobalDWORD,
                        _ => throw new InvalidOperationException(
                            "The common root gate returned an unexpected missing outcome.")
                    };
            return new(capture, parent, null, outcome)
            {
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
        }

        return ParentNotClean(capture, parent);
    }

    private static CapacityFinalCalleeCommonRootSizeLifecyclePlan ParentNotClean(
        CapacityFinalCalleeCommonRootSizeLifecycleCapture capture,
        CapacityFinalCalleeCommonTailRootGatePlan parent) =>
        new(
            capture,
            parent,
            Child: null,
            CapacityFinalCalleeCommonRootSizeLifecycleOutcome.ParentNotClean)
        {
            StopAtRawFileOffsetExclusive =
                parent.StopAtRawFileOffsetExclusive
        };

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
        CapacityFinalCalleeCommonTailRootGatePlan plan) =>
        plan.Outcome is
            CapacityFinalCalleeCommonTailRootGateOutcome
                .MissingEntryESIAndFreshGlobalDWORD
            or CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI
            or CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD
        && plan.FreshGlobalReadReached
        && plan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonTailRootGateProjection.CompareRawFileOffset
        && !plan.CompareReached
        && !plan.JnzReached
        && !plan.ExactCleanAdmission
        && plan.Writes.IsEmpty
        && !plan.OpaqueEntryReached;

    private static bool IsExactCleanSizeStop(
        CapacityFinalCalleeCommonTailSizeGatePlan plan,
        CapacityFinalCalleeCommonTailSizeGateOutcome expectedOutcome,
        bool expectedTaken,
        uint expectedDestination) =>
        plan.Outcome == expectedOutcome
        && plan.EntryEDICaptured
        && plan.MoveReached
        && plan.ShiftReached
        && plan.ShiftInput.HasValue
        && plan.ShiftCount
            == CapacityFinalCalleeCommonTailSizeGateProjection.ShiftCount
        && plan.ShiftResult.HasValue
        && plan.ShiftFlags.HasValue
        && plan.CompareReached
        && plan.CompareLeft == plan.ShiftResult
        && plan.CompareRight
            == CapacityFinalCalleeCommonTailSizeGateProjection.CompareThreshold
        && plan.CompareResult.HasValue
        && plan.CompareFlags.HasValue
        && plan.JnbReached
        && plan.JnbTaken == expectedTaken
        && plan.JnbSelectedDestinationRawFileOffset == expectedDestination
        && plan.ExactCleanAdmission
        && !plan.TargetEntered
        && plan.Writes.IsEmpty
        && plan.StopAtRawFileOffsetExclusive == expectedDestination;

    private static CapacityFinalCalleeCommonRootSizeLifecycleInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}
