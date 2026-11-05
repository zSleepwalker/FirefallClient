using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedFrameIndexedLifecycleCapture(
    CapacityFinalCalleeLargeSavedFrameGateCapture LargeSavedFrameGateCapture,
    uint? ChildEntryEDX,
    uint? FreshEDXPlus1C,
    uint? FreshIndexedDword,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
{
    LargeSavedFrameGateIncomplete,
    ParentZeroTo92BD,
    ParentZeroTo92BDContaminated,
    MissingChildEntryEDX,
    MissingChildFreshEDXPlus1C,
    MissingChildFreshIndexedDword,
    MissingChildFreshGlobalValue,
    ChildEqualToFallthrough9243,
    ChildUnequalToBranch9263
}

public enum CapacityFinalCalleeLargeSavedFrameIndexedLifecycleReadStage
{
    LargeSavedFrameGate,
    LargeSavedIndexGate
}

public enum CapacityFinalCalleeLargeSavedFrameIndexedLifecycleChildEDXSource
{
    None,
    ExplicitIndependentCapture
}

public readonly record struct CapacityFinalCalleeLargeSavedFrameIndexedLifecycleFlags(
    bool Carry,
    bool Parity,
    bool? Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeSavedFrameIndexedLifecycleRead(
    CapacityFinalCalleeLargeSavedFrameIndexedLifecycleReadStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public sealed record CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan(
    CapacityFinalCalleeLargeSavedFrameIndexedLifecycleCapture Capture,
    CapacityFinalCalleeLargeSavedFrameGatePlan LargeSavedFrameGate,
    CapacityFinalCalleeLargeSavedIndexGatePlan? LargeSavedIndexGate,
    CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome Outcome)
{
    public bool ExactCleanHandoffToIndexedGate { get; init; }
    public bool IndexedGateReached => LargeSavedIndexGate is not null;
    public CapacityFinalCalleeLargeSavedFrameIndexedLifecycleChildEDXSource
        ChildEntryEDXSource { get; init; }
    public uint? ChildEntryEDX { get; init; }
    public bool ChildEntryEDXCaptured =>
        LargeSavedIndexGate?.EntryEDXCaptured == true;
    public bool UnreachedChildEntryEDXCaptureProvided { get; init; }
    public bool UnreachedFreshEDXPlus1CCaptureProvided { get; init; }
    public bool UnreachedFreshIndexedDwordCaptureProvided { get; init; }
    public bool UnreachedFreshGlobalValueCaptureProvided { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeSavedFrameIndexedLifecycleRead>
        OrderedReads { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedFrameIndexedLifecycleRead>.Empty;
    public CapacityFinalCalleeLargeSavedFrameIndexedLifecycleFlags? FlagsAtStop =>
        LargeSavedIndexGate?.CompareFlags is { } childFlags
            ? ConvertFlags(childFlags)
            : ConvertFlags(LargeSavedFrameGate.TestFlags);
    public bool FutureStoresReached => false;
    public bool EpilogueReached => false;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeLargeSavedFrameIndexedLifecycleFlags
        ConvertFlags(CapacityFinalCalleeLargeSavedIndexGateFlags flags) =>
            new(
                flags.Carry,
                flags.Parity,
                flags.AuxiliaryCarry,
                flags.Zero,
                flags.Sign,
                flags.Overflow);

    private static CapacityFinalCalleeLargeSavedFrameIndexedLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeLargeSavedFrameGateFlags? flags) =>
            flags is CapacityFinalCalleeLargeSavedFrameGateFlags value
                ? new(
                    value.Carry,
                    value.Parity,
                    value.Auxiliary,
                    value.Zero,
                    value.Sign,
                    value.Overflow)
                : null;
}

/// <summary>
/// Joins only the exact nonzero 9231 exit of the saved-frame gate to the
/// indexed gate. The child's EDX and its three memory values are independent
/// captures; none is inferred from the parent's ECX or saved-frame read.
/// </summary>
public static class CapacityFinalCalleeLargeSavedFrameIndexedLifecycleProjection
{
    public const uint IndexedGateEntryRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexGateProjection.EntryRawFileOffset;
    public const uint ParentZeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedFrameGateProjection.ZeroDestinationRawFileOffset;
    public const uint ParentNonzeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedFrameGateProjection.NonzeroDestinationRawFileOffset;
    public const uint ChildEqualDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ChildUnequalDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexGateProjection.JnzDestinationRawFileOffset;

    public static CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan Project(
        CapacityFinalCalleeLargeSavedFrameIndexedLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeSavedFrameGatePlan parent =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(
                capture.LargeSavedFrameGateCapture);

        ImmutableArray<CapacityFinalCalleeLargeSavedFrameIndexedLifecycleRead>
            ParentReads() =>
                parent.Reads.Select(read =>
                    new CapacityFinalCalleeLargeSavedFrameIndexedLifecycleRead(
                        CapacityFinalCalleeLargeSavedFrameIndexedLifecycleReadStage
                            .LargeSavedFrameGate,
                        read.RawFileOffset,
                        read.Address,
                        read.AddressCaptured,
                        read.ValueCaptured,
                        read.Value))
                    .ToImmutableArray();

        bool anyChildCaptureProvided =
            capture.ChildEntryEDX.HasValue
            || capture.FreshEDXPlus1C.HasValue
            || capture.FreshIndexedDword.HasValue
            || capture.FreshGlobalValue.HasValue;

        if (!IsExactParentNonzeroExit(parent))
        {
            bool exactParentZeroExit = IsExactParentZeroExit(parent);
            return new(
                capture,
                parent,
                null,
                exactParentZeroExit
                    ? anyChildCaptureProvided
                        ? CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                            .ParentZeroTo92BDContaminated
                        : CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                            .ParentZeroTo92BD
                    : CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                        .LargeSavedFrameGateIncomplete)
            {
                UnreachedChildEntryEDXCaptureProvided =
                    capture.ChildEntryEDX.HasValue,
                UnreachedFreshEDXPlus1CCaptureProvided =
                    capture.FreshEDXPlus1C.HasValue,
                UnreachedFreshIndexedDwordCaptureProvided =
                    capture.FreshIndexedDword.HasValue,
                UnreachedFreshGlobalValueCaptureProvided =
                    capture.FreshGlobalValue.HasValue,
                OrderedReads = ParentReads(),
                ExactCleanExitAdmitted =
                    exactParentZeroExit && !anyChildCaptureProvided,
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeLargeSavedIndexGatePlan child =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: capture.ChildEntryEDX,
                FreshEDXPlus1C: capture.FreshEDXPlus1C,
                FreshIndexedDword: capture.FreshIndexedDword,
                FreshGlobalValue: capture.FreshGlobalValue));

        var reads =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedFrameIndexedLifecycleRead>();
        reads.AddRange(ParentReads());
        foreach (CapacityFinalCalleeLargeSavedIndexGateRead read in child.Reads)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeSavedFrameIndexedLifecycleReadStage
                    .LargeSavedIndexGate,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value));
        }

        return new(capture, parent, child, MapChildOutcome(child.Outcome))
        {
            ExactCleanHandoffToIndexedGate = true,
            ChildEntryEDXSource = child.EntryEDXCaptured
                ? CapacityFinalCalleeLargeSavedFrameIndexedLifecycleChildEDXSource
                    .ExplicitIndependentCapture
                : CapacityFinalCalleeLargeSavedFrameIndexedLifecycleChildEDXSource.None,
            ChildEntryEDX = child.EntryEDXCaptured ? capture.ChildEntryEDX : null,
            UnreachedChildEntryEDXCaptureProvided =
                !child.EntryEDXCaptured && capture.ChildEntryEDX.HasValue,
            UnreachedFreshEDXPlus1CCaptureProvided =
                !child.FreshEDXPlus1CCaptured && capture.FreshEDXPlus1C.HasValue,
            UnreachedFreshIndexedDwordCaptureProvided =
                child.UnreachedFreshIndexedDwordCaptureProvided,
            UnreachedFreshGlobalValueCaptureProvided =
                child.UnreachedFreshGlobalValueCaptureProvided,
            OrderedReads = reads.ToImmutable(),
            ExactCleanExitAdmitted =
                IsExactChildExit(child)
                && parent.ExactCleanExitAdmitted,
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactParentNonzeroExit(
        CapacityFinalCalleeLargeSavedFrameGatePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeLargeSavedFrameGateOutcome.NonzeroToFallthrough
        && plan.ExactCleanExitAdmitted
        && plan.FreshReadReached
        && plan.TestReached
        && plan.JzReached
        && plan.JzTaken == false
        && plan.JzSelectedDestinationRawFileOffset
            == IndexedGateEntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == IndexedGateEntryRawFileOffset
        && !plan.UnreachedFreshFrameMinus10CaptureProvided;

    private static bool IsExactParentZeroExit(
        CapacityFinalCalleeLargeSavedFrameGatePlan plan) =>
        plan.Outcome == CapacityFinalCalleeLargeSavedFrameGateOutcome.ZeroToTarget
        && plan.ExactCleanExitAdmitted
        && plan.FreshReadReached
        && plan.TestReached
        && plan.JzReached
        && plan.JzTaken == true
        && plan.JzSelectedDestinationRawFileOffset
            == ParentZeroDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == ParentZeroDestinationRawFileOffset
        && !plan.UnreachedFreshFrameMinus10CaptureProvided;

    private static bool IsExactChildExit(
        CapacityFinalCalleeLargeSavedIndexGatePlan plan) =>
        plan.ExactCleanCompletion
        && plan.JnzReached
        && plan.JnzSelectedDestinationRawFileOffset
            == (plan.JnzTaken == true
                ? ChildUnequalDestinationRawFileOffset
                : ChildEqualDestinationRawFileOffset)
        && plan.StopAtRawFileOffsetExclusive
            == plan.JnzSelectedDestinationRawFileOffset
        && !plan.UnreachedFreshIndexedDwordCaptureProvided
        && !plan.UnreachedFreshGlobalValueCaptureProvided;

    private static CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
        MapChildOutcome(CapacityFinalCalleeLargeSavedIndexGateOutcome outcome) =>
            outcome switch
            {
                CapacityFinalCalleeLargeSavedIndexGateOutcome
                    .MissingEntryEDXForFreshRead =>
                    CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                        .MissingChildEntryEDX,
                CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingFreshEDXPlus1C =>
                    CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                        .MissingChildFreshEDXPlus1C,
                CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingFreshIndexedDword =>
                    CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                        .MissingChildFreshIndexedDword,
                CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingFreshGlobalValue =>
                    CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                        .MissingChildFreshGlobalValue,
                CapacityFinalCalleeLargeSavedIndexGateOutcome.EqualToFallthrough9243 =>
                    CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                        .ChildEqualToFallthrough9243,
                CapacityFinalCalleeLargeSavedIndexGateOutcome.JnzTo9263 =>
                    CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                        .ChildUnequalToBranch9263,
                _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
            };
}
