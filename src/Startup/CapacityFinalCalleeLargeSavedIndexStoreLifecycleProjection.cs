using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedIndexStoreLifecycleCapture(
    CapacityFinalCalleeLargeSavedIndexGateCapture SavedIndexGateCapture,
    uint? ChildFreshEDXPlus1C,
    uint? ChildEntryEAX);

public enum CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
{
    ParentGateIncomplete,
    ParentUnequalTo9263,
    ParentUnequalTo9263Contaminated,
    MissingChildEntryEDX,
    MissingChildFreshEDXPlus1C,
    MissingChildEntryEAXForStore,
    ChildZeroToFallthrough9251,
    ChildNonzeroTo927C
}

public enum CapacityFinalCalleeLargeSavedIndexStoreLifecycleStage
{
    SavedIndexGate,
    SavedIndexedStoreGate
}

public enum CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEDXSource
{
    None,
    ParentOriginalEntryEDXFromReachedFirstLoad
}

public enum CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildFreshIndexSource
{
    None,
    IndependentChildEDXPlus1CRead
}

public enum CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEAXSource
{
    None,
    ExplicitIndependentCaptureAtReachedStore
}

public readonly record struct CapacityFinalCalleeLargeSavedIndexStoreLifecycleFlags(
    bool Carry,
    bool Parity,
    bool? Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeSavedIndexStoreLifecycleRead(
    CapacityFinalCalleeLargeSavedIndexStoreLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSavedIndexStoreLifecycleWrite(
    CapacityFinalCalleeLargeSavedIndexStoreLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public sealed record CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan(
    CapacityFinalCalleeLargeSavedIndexStoreLifecycleCapture Capture,
    CapacityFinalCalleeLargeSavedIndexGatePlan SavedIndexGate,
    CapacityFinalCalleeLargeSavedIndexedStoreGatePlan? SavedIndexedStoreGate,
    CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome Outcome)
{
    public bool ExactCleanHandoffToStoreGate { get; init; }
    public bool StoreGateReached => SavedIndexedStoreGate is not null;
    public CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEDXSource
        ChildEntryEDXSource { get; init; }
    public uint? ChildEntryEDX { get; init; }
    public CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildFreshIndexSource
        ChildFreshIndexSource { get; init; }
    public uint? ChildFreshEDXPlus1C { get; init; }
    public CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEAXSource
        ChildEntryEAXSource { get; init; }
    public bool ChildEntryEAXCaptured =>
        SavedIndexedStoreGate?.StoreValueCaptured == true;
    public bool UnreachedChildFreshEDXPlus1CCaptureProvided { get; init; }
    public bool UnreachedChildEntryEAXCaptureProvided { get; init; }
    public bool ChildEntryEAXCaptureRejectedAtParentExit { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedIndexStoreLifecycleRead>
        OrderedReads { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedIndexStoreLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedIndexStoreLifecycleWrite>
        OrderedWrites { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedIndexStoreLifecycleWrite>.Empty;
    public CapacityFinalCalleeLargeSavedIndexStoreLifecycleFlags? FlagsAtStop =>
        SavedIndexedStoreGate?.TestFlags is
            CapacityFinalCalleeLargeSavedIndexedStoreGateFlags childFlags
                ? ConvertFlags(childFlags)
                : ConvertFlags(SavedIndexGate.CompareFlags);
    public bool FuturePathsReached => false;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeLargeSavedIndexStoreLifecycleFlags
        ConvertFlags(CapacityFinalCalleeLargeSavedIndexedStoreGateFlags flags) =>
            new(
                flags.Carry,
                flags.Parity,
                flags.AuxiliaryCarry,
                flags.Zero,
                flags.Sign,
                flags.Overflow);

    private static CapacityFinalCalleeLargeSavedIndexStoreLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeLargeSavedIndexGateFlags? flags) =>
            flags is CapacityFinalCalleeLargeSavedIndexGateFlags value
                ? new(
                    value.Carry,
                    value.Parity,
                    value.AuxiliaryCarry,
                    value.Zero,
                    value.Sign,
                    value.Overflow)
                : null;
}

/// <summary>
/// Joins only the exact 9243 equality edge to its fresh indexed store and EAX
/// TEST/JNZ gate, stopping at the selected branch boundary.
/// </summary>
public static class CapacityFinalCalleeLargeSavedIndexStoreLifecycleProjection
{
    public const uint StoreGateEntryRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.EntryRawFileOffset;
    public const uint ParentUnequalDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexGateProjection.JnzDestinationRawFileOffset;
    public const uint ParentEqualDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ChildZeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ChildNonzeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.JnzDestinationRawFileOffset;

    public static CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan Project(
        CapacityFinalCalleeLargeSavedIndexStoreLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeSavedIndexGatePlan parent =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(
                capture.SavedIndexGateCapture);

        ImmutableArray<CapacityFinalCalleeLargeSavedIndexStoreLifecycleRead>
            ParentReads() =>
                parent.Reads.Select(read =>
                    new CapacityFinalCalleeLargeSavedIndexStoreLifecycleRead(
                        CapacityFinalCalleeLargeSavedIndexStoreLifecycleStage
                            .SavedIndexGate,
                        read.RawFileOffset,
                        read.Address,
                        read.AddressCaptured,
                        read.ValueCaptured,
                        read.Value))
                    .ToImmutableArray();

        bool anyChildCaptureProvided =
            capture.ChildFreshEDXPlus1C.HasValue
            || capture.ChildEntryEAX.HasValue;

        if (!IsExactParentEqualityExit(parent))
        {
            bool exactParentUnequalExit = IsExactParentUnequalExit(parent);
            return new(
                capture,
                parent,
                null,
                exactParentUnequalExit
                    ? anyChildCaptureProvided
                        ? CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                            .ParentUnequalTo9263Contaminated
                        : CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                            .ParentUnequalTo9263
                    : CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                        .ParentGateIncomplete)
            {
                UnreachedChildFreshEDXPlus1CCaptureProvided =
                    capture.ChildFreshEDXPlus1C.HasValue,
                UnreachedChildEntryEAXCaptureProvided =
                    capture.ChildEntryEAX.HasValue,
                ChildEntryEAXCaptureRejectedAtParentExit =
                    exactParentUnequalExit && capture.ChildEntryEAX.HasValue,
                ReachedInstructions = parent.ReachedInstructions,
                OrderedReads = ParentReads(),
                ExactCleanExitAdmitted =
                    exactParentUnequalExit && !anyChildCaptureProvided,
                StopAtRawFileOffsetExclusive =
                    parent.StopAtRawFileOffsetExclusive
            };
        }

        uint parentEntryEDX = capture.SavedIndexGateCapture.EntryEDX!.Value;
        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan child =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: parentEntryEDX,
                FreshEDXPlus1C: capture.ChildFreshEDXPlus1C,
                EntryEAX: capture.ChildEntryEAX));

        var reads =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedIndexStoreLifecycleRead>();
        reads.AddRange(ParentReads());
        foreach (CapacityFinalCalleeLargeSavedIndexedStoreGateRead read in child.Reads)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeSavedIndexStoreLifecycleStage
                    .SavedIndexedStoreGate,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value));
        }

        var writes =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedIndexStoreLifecycleWrite>();
        foreach (CapacityFinalCalleeLargeSavedIndexedStoreGateWrite write in child.Writes)
        {
            writes.Add(new(
                CapacityFinalCalleeLargeSavedIndexStoreLifecycleStage
                    .SavedIndexedStoreGate,
                write.RawFileOffset,
                write.Address,
                write.AddressCaptured,
                write.ValueCaptured,
                write.Value));
        }

        return new(capture, parent, child, MapChildOutcome(child.Outcome))
        {
            ExactCleanHandoffToStoreGate = true,
            ChildEntryEDXSource = child.FreshIndexReadReached
                && child.EntryEDXCaptured
                    ? CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEDXSource
                        .ParentOriginalEntryEDXFromReachedFirstLoad
                    : CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEDXSource.None,
            ChildEntryEDX = child.EntryEDXCaptured ? parentEntryEDX : null,
            ChildFreshIndexSource = child.FreshEDXPlus1CCaptured
                ? CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildFreshIndexSource
                    .IndependentChildEDXPlus1CRead
                : CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildFreshIndexSource.None,
            ChildFreshEDXPlus1C =
                child.FreshEDXPlus1CCaptured ? child.FreshIndex : null,
            ChildEntryEAXSource = child.StoreValueCaptured
                ? CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEAXSource
                    .ExplicitIndependentCaptureAtReachedStore
                : CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEAXSource.None,
            UnreachedChildFreshEDXPlus1CCaptureProvided =
                !child.FreshIndexReadReached && capture.ChildFreshEDXPlus1C.HasValue,
            UnreachedChildEntryEAXCaptureProvided =
                !child.StoreInstructionReached && capture.ChildEntryEAX.HasValue,
            ReachedInstructions =
                parent.ReachedInstructions.AddRange(child.ReachedInstructions),
            OrderedReads = reads.ToImmutable(),
            OrderedWrites = writes.ToImmutable(),
            ExactCleanExitAdmitted =
                IsExactChildExit(child) && parent.ExactCleanCompletion,
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactParentEqualityExit(
        CapacityFinalCalleeLargeSavedIndexGatePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeLargeSavedIndexGateOutcome.EqualToFallthrough9243
        && plan.ExactCleanCompletion
        && plan.EntryEDXCaptured
        && plan.FreshEDXPlus1CReadReached
        && plan.FreshEDXPlus1CCaptured
        && plan.CompareReached
        && plan.FreshIndexedDwordCaptured
        && plan.FreshGlobalReadReached
        && plan.FreshGlobalValueCaptured
        && plan.JnzReached
        && plan.JnzTaken == false
        && plan.JnzSelectedDestinationRawFileOffset
            == StoreGateEntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == StoreGateEntryRawFileOffset
        && !plan.UnreachedFreshIndexedDwordCaptureProvided
        && !plan.UnreachedFreshGlobalValueCaptureProvided;

    private static bool IsExactParentUnequalExit(
        CapacityFinalCalleeLargeSavedIndexGatePlan plan) =>
        plan.Outcome == CapacityFinalCalleeLargeSavedIndexGateOutcome.JnzTo9263
        && plan.ExactCleanCompletion
        && plan.EntryEDXCaptured
        && plan.FreshEDXPlus1CReadReached
        && plan.FreshEDXPlus1CCaptured
        && plan.CompareReached
        && plan.FreshIndexedDwordCaptured
        && plan.FreshGlobalReadReached
        && plan.FreshGlobalValueCaptured
        && plan.JnzReached
        && plan.JnzTaken == true
        && plan.JnzSelectedDestinationRawFileOffset
            == ParentUnequalDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == ParentUnequalDestinationRawFileOffset
        && !plan.UnreachedFreshIndexedDwordCaptureProvided
        && !plan.UnreachedFreshGlobalValueCaptureProvided;

    private static bool IsExactChildExit(
        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan plan) =>
        plan.ExactCleanCompletion
        && plan.JnzReached
        && plan.JnzSelectedDestinationRawFileOffset
            == (plan.JnzTaken == true
                ? ChildNonzeroDestinationRawFileOffset
                : ChildZeroDestinationRawFileOffset)
        && plan.StopAtRawFileOffsetExclusive
            == plan.JnzSelectedDestinationRawFileOffset
        && !plan.UnreachedFreshEDXPlus1CCaptureProvided
        && !plan.UnreachedEntryEAXCaptureProvided;

    private static CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
        MapChildOutcome(CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome outcome) =>
            outcome switch
            {
                CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
                    .MissingEntryEDXForFreshRead =>
                    CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                        .MissingChildEntryEDX,
                CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
                    .MissingFreshEDXPlus1C =>
                    CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                        .MissingChildFreshEDXPlus1C,
                CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
                    .MissingEntryEAXForStore =>
                    CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                        .MissingChildEntryEAXForStore,
                CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
                    .ZeroToFallthrough9251 =>
                    CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                        .ChildZeroToFallthrough9251,
                CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome.NonzeroTo927C =>
                    CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                        .ChildNonzeroTo927C,
                _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
            };
}
