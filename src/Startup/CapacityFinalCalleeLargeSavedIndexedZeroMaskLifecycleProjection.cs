using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleCapture(
    CapacityFinalCalleeLargeSavedIndexedStoreGateCapture ParentCapture,
    uint? ChildEntryEBP,
    uint? ChildFreshEDXPlus1C,
    uint? ChildFreshEBPMinus8);

public enum CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
{
    ParentGateIncomplete,
    ParentNonzeroTo927C,
    ParentNonzeroTo927CWithUnusedChildCaptures,
    ChildMissingFreshEDXPlus1C,
    ChildMissingEntryEBP,
    ChildMissingFreshEBPMinus8,
    ChildCompletedTo925B
}

public enum CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage
{
    SavedIndexedStoreGate,
    SavedMaskShift
}

public enum CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEDXSource
{
    None,
    ParentOriginalEntryEDXAtReachedZeroGate
}

public enum CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEBPSource
{
    None,
    ExplicitIndependentCaptureAtReachedConsumer
}

public enum CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshIndexSource
{
    None,
    IndependentChildEDXPlus1CRead
}

public enum CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshFrameSource
{
    None,
    IndependentChildEBPMinus8Read
}

public enum CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource
{
    None,
    ParentTest,
    ChildShift
}

public readonly record struct CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleRead(
    CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleWrite(
    CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public sealed record CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan(
    CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleCapture Capture,
    CapacityFinalCalleeLargeSavedIndexedStoreGatePlan ParentPlan,
    CapacityFinalCalleeLargeSavedMaskShiftPlan? ChildPlan,
    CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome Outcome)
{
    public bool ExactCleanHandoffToChild { get; init; }
    public bool ChildReached => ChildPlan is not null;
    public CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEDXSource
        ChildEntryEDXSource { get; init; }
    public uint? ChildEntryEDX { get; init; }
    public CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEBPSource
        ChildEntryEBPSource { get; init; }
    public uint? ChildEntryEBP { get; init; }
    public CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshIndexSource
        ChildFreshIndexSource { get; init; }
    public uint? ChildFreshEDXPlus1C { get; init; }
    public CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshFrameSource
        ChildFreshFrameSource { get; init; }
    public uint? ChildFreshEBPMinus8 { get; init; }
    public bool UnreachedChildEntryEBPCaptureProvided { get; init; }
    public bool UnreachedChildFreshEDXPlus1CCaptureProvided { get; init; }
    public bool UnreachedChildFreshEBPMinus8CaptureProvided { get; init; }
    public bool AnyUnreachedChildCaptureProvided =>
        UnreachedChildEntryEBPCaptureProvided
        || UnreachedChildFreshEDXPlus1CCaptureProvided
        || UnreachedChildFreshEBPMinus8CaptureProvided;
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleRead>
        OrderedReads { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleWrite>
        OrderedWrites { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleWrite>.Empty;
    public CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags? ParentTestFlags =>
        ConvertFlags(ParentPlan.TestFlags);
    public CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags? ChildShiftFlags =>
        ConvertFlags(ChildPlan?.ShiftFlags);
    public bool ParentFlagsRestoredForZeroCount { get; init; }
    public bool ChildShiftFlagsReplaceParentTestFlags { get; init; }
    public CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags? FinalFlagsAtStop
        { get; init; }
    public CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags? FlagsAtStop =>
        FinalFlagsAtStop;
    public CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource
        FlagsAtStopSource { get; init; }
    public bool GlobalAndReached => false;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeLargeSavedIndexedStoreGateFlags? flags) =>
        flags is CapacityFinalCalleeLargeSavedIndexedStoreGateFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeLargeSavedMaskShiftFlags? flags) =>
        flags is CapacityFinalCalleeLargeSavedMaskShiftFlags value
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
/// Joins only the exact zero-EAX edge at 9251 to the independent saved-mask
/// reload and shift, stopping before the global AND at 925B.
/// </summary>
public static class CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleProjection
{
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeSavedMaskShiftProjection.EntryRawFileOffset;
    public const uint ParentNonzeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.JnzDestinationRawFileOffset;
    public const uint ParentZeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint BoundedStopRawFileOffsetExclusive =
        CapacityFinalCalleeLargeSavedMaskShiftProjection.BoundedStopRawFileOffsetExclusive;

    public static CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan Project(
        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan parent =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(
                capture.ParentCapture);

        ImmutableArray<CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleRead>
            ParentReads() =>
                parent.Reads.Select(read =>
                    new CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleRead(
                        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage
                            .SavedIndexedStoreGate,
                        read.RawFileOffset,
                        read.Address,
                        read.AddressCaptured,
                        read.ValueCaptured,
                        read.Value))
                    .ToImmutableArray();

        ImmutableArray<CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleWrite>
            ParentWrites() =>
                parent.Writes.Select(write =>
                    new CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleWrite(
                        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage
                            .SavedIndexedStoreGate,
                        write.RawFileOffset,
                        write.Address,
                        write.AddressCaptured,
                        write.ValueCaptured,
                        write.Value))
                    .ToImmutableArray();

        bool anyChildCaptureProvided =
            capture.ChildEntryEBP.HasValue
            || capture.ChildFreshEDXPlus1C.HasValue
            || capture.ChildFreshEBPMinus8.HasValue;

        if (!IsExactParentZeroExit(parent))
        {
            bool exactParentNonzeroExit = IsExactParentNonzeroExit(parent);
            return new(
                capture,
                parent,
                ChildPlan: null,
                exactParentNonzeroExit && anyChildCaptureProvided
                    ? CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                        .ParentNonzeroTo927CWithUnusedChildCaptures
                    : exactParentNonzeroExit
                        ? CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                            .ParentNonzeroTo927C
                        : CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                            .ParentGateIncomplete)
            {
                UnreachedChildEntryEBPCaptureProvided =
                    capture.ChildEntryEBP.HasValue,
                UnreachedChildFreshEDXPlus1CCaptureProvided =
                    capture.ChildFreshEDXPlus1C.HasValue,
                UnreachedChildFreshEBPMinus8CaptureProvided =
                    capture.ChildFreshEBPMinus8.HasValue,
                ReachedInstructions = parent.ReachedInstructions,
                OrderedReads = ParentReads(),
                OrderedWrites = ParentWrites(),
                FinalFlagsAtStop = ConvertFlags(parent.FlagsAtStop),
                FlagsAtStopSource = parent.TestFlags.HasValue
                    ? CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource
                        .ParentTest
                    : CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource.None,
                ExactCleanExitAdmitted =
                    exactParentNonzeroExit && !anyChildCaptureProvided,
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
        }

        uint parentOriginalEDX = capture.ParentCapture.EntryEDX!.Value;
        bool childIndexCaptured = capture.ChildFreshEDXPlus1C.HasValue;
        CapacityFinalCalleeLargeSavedMaskShiftPlan child =
            CapacityFinalCalleeLargeSavedMaskShiftProjection.Project(new(
                EntryEDX: parentOriginalEDX,
                FreshEDXPlus1C: capture.ChildFreshEDXPlus1C,
                EntryEBP: childIndexCaptured ? capture.ChildEntryEBP : null,
                FreshEBPMinus8: capture.ChildFreshEBPMinus8));

        var reads =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleRead>();
        reads.AddRange(ParentReads());
        foreach (CapacityFinalCalleeLargeSavedMaskShiftRead read in child.OrderedReads)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage.SavedMaskShift,
                read.RawFileOffset,
                read.Address,
                read.Address.HasValue,
                read.ValueCaptured,
                read.Value));
        }

        bool childShiftReached = child.ShiftResult.HasValue;
        bool zeroShiftCount = childShiftReached && child.ShiftCountMasked == 0;
        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags? finalFlags =
            zeroShiftCount
                ? ConvertFlags(parent.TestFlags)
                : childShiftReached
                    ? ConvertFlags(child.ShiftFlags)
                    : ConvertFlags(parent.TestFlags);
        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource flagsSource =
            zeroShiftCount || !childShiftReached
                ? parent.TestFlags.HasValue
                    ? CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource
                        .ParentTest
                    : CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource.None
                : CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource
                    .ChildShift;

        bool childExactBoundedStop =
            child.Outcome
                == CapacityFinalCalleeLargeSavedMaskShiftOutcome.BoundedBeforeGlobalAnd
            && child.ExactCleanExitAdmitted
            && child.StopAtRawFileOffsetExclusive == BoundedStopRawFileOffsetExclusive
            && child.NotResult.HasValue
            && child.NotReached
            && !child.GlobalAndReached;

        return new(
            capture,
            parent,
            child,
            MapChildOutcome(child.Outcome))
        {
            ExactCleanHandoffToChild = true,
            ChildEntryEDXSource = child.FreshEDXPlus1CReadReached
                ? CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEDXSource
                    .ParentOriginalEntryEDXAtReachedZeroGate
                : CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEDXSource.None,
            ChildEntryEDX = child.FreshEDXPlus1CReadReached ? parentOriginalEDX : null,
            ChildEntryEBPSource =
                child.EntryEBPConsumerReached && capture.ChildEntryEBP.HasValue
                ? CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEBPSource
                    .ExplicitIndependentCaptureAtReachedConsumer
                : CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEBPSource.None,
            ChildEntryEBP = child.EntryEBPConsumerReached
                ? capture.ChildEntryEBP
                : null,
            ChildFreshIndexSource = child.FreshEDXPlus1CCaptured
                ? CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshIndexSource
                    .IndependentChildEDXPlus1CRead
                : CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshIndexSource.None,
            ChildFreshEDXPlus1C =
                child.FreshEDXPlus1CCaptured ? child.Capture.FreshEDXPlus1C : null,
            ChildFreshFrameSource = child.FreshEBPMinus8Captured
                ? CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshFrameSource
                    .IndependentChildEBPMinus8Read
                : CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshFrameSource.None,
            ChildFreshEBPMinus8 =
                child.FreshEBPMinus8Captured ? child.Capture.FreshEBPMinus8 : null,
            UnreachedChildFreshEDXPlus1CCaptureProvided = false,
            UnreachedChildEntryEBPCaptureProvided =
                !child.EntryEBPConsumerReached && capture.ChildEntryEBP.HasValue,
            UnreachedChildFreshEBPMinus8CaptureProvided =
                !child.FreshEBPMinus8ReadReached && capture.ChildFreshEBPMinus8.HasValue,
            ReachedInstructions =
                parent.ReachedInstructions.AddRange(
                    (child.FreshEDXPlus1CReadReached
                        ? new[]
                        {
                            CapacityFinalCalleeLargeSavedMaskShiftProjection
                                .EntryRawFileOffset
                        }
                        : Array.Empty<uint>())
                    .Concat(
                        child.EntryEBPConsumerReached
                            ? new[]
                            {
                                CapacityFinalCalleeLargeSavedMaskShiftProjection
                                    .EBPMinus8ReadRawFileOffset
                            }
                            : Array.Empty<uint>())
                    .Concat(
                        child.ShiftResult.HasValue
                            ? new[]
                            {
                                CapacityFinalCalleeLargeSavedMaskShiftProjection
                                    .ShiftRawFileOffset,
                                CapacityFinalCalleeLargeSavedMaskShiftProjection
                                    .NotRawFileOffset
                            }
                            : Array.Empty<uint>())),
            OrderedReads = reads.ToImmutable(),
            OrderedWrites = ParentWrites(),
            ParentFlagsRestoredForZeroCount = zeroShiftCount,
            ChildShiftFlagsReplaceParentTestFlags =
                childShiftReached && !zeroShiftCount,
            FinalFlagsAtStop = finalFlags,
            FlagsAtStopSource = flagsSource,
            ExactCleanExitAdmitted =
                childExactBoundedStop && !anyChildCaptureProvidedAfterConsumption(
                    capture, child),
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactParentZeroExit(
        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome.ZeroToFallthrough9251
        && plan.ExactCleanCompletion
        && plan.EntryEDXCaptured
        && plan.FreshIndexReadReached
        && plan.FreshEDXPlus1CCaptured
        && plan.StoreInstructionReached
        && plan.StoreValueCaptured
        && plan.StoreValue == 0
        && plan.TestReached
        && plan.JnzReached
        && plan.JnzTaken == false
        && plan.JnzSelectedDestinationRawFileOffset == ParentZeroDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
        && !plan.UnreachedFreshEDXPlus1CCaptureProvided
        && !plan.UnreachedEntryEAXCaptureProvided;

    private static bool IsExactParentNonzeroExit(
        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan plan) =>
        plan.Outcome == CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome.NonzeroTo927C
        && plan.ExactCleanCompletion
        && plan.EntryEDXCaptured
        && plan.FreshIndexReadReached
        && plan.FreshEDXPlus1CCaptured
        && plan.StoreInstructionReached
        && plan.StoreValueCaptured
        && plan.TestReached
        && plan.JnzReached
        && plan.JnzTaken == true
        && plan.JnzSelectedDestinationRawFileOffset
            == ParentNonzeroDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == ParentNonzeroDestinationRawFileOffset
        && !plan.UnreachedFreshEDXPlus1CCaptureProvided
        && !plan.UnreachedEntryEAXCaptureProvided;

    private static bool anyChildCaptureProvidedAfterConsumption(
        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleCapture capture,
        CapacityFinalCalleeLargeSavedMaskShiftPlan child) =>
        (!child.FreshEDXPlus1CReadReached && capture.ChildFreshEDXPlus1C.HasValue)
        || (!child.EntryEBPConsumerReached && capture.ChildEntryEBP.HasValue)
        || (!child.FreshEBPMinus8ReadReached && capture.ChildFreshEBPMinus8.HasValue);

    private static CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeLargeSavedIndexedStoreGateFlags? flags) =>
        flags is CapacityFinalCalleeLargeSavedIndexedStoreGateFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeLargeSavedMaskShiftFlags? flags) =>
        flags is CapacityFinalCalleeLargeSavedMaskShiftFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
        MapChildOutcome(CapacityFinalCalleeLargeSavedMaskShiftOutcome outcome) =>
            outcome switch
            {
                CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingFreshEDXPlus1C =>
                    CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                        .ChildMissingFreshEDXPlus1C,
                CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingEntryEBP =>
                    CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                        .ChildMissingEntryEBP,
                CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingFreshEBPMinus8 =>
                    CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                        .ChildMissingFreshEBPMinus8,
                CapacityFinalCalleeLargeSavedMaskShiftOutcome.BoundedBeforeGlobalAnd =>
                    CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                        .ChildCompletedTo925B,
                CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingEntryEDX =>
                    throw new InvalidOperationException(
                        "The child EDX is derived from the completed parent gate."),
                _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
            };
}
