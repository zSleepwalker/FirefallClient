using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRejectFieldLifecycleCapture(
    CapacityFinalCalleeLargeIndexedRejectGateCapture ParentCapture,
    uint? ChildEDX,
    uint? ChildFreshECXPlus10);

public enum CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
{
    ParentGateIncomplete,
    ParentUnsignedBelowTo8E76,
    ParentUnsignedBelowTo8E76WithUnusedChildCaptures,
    ChildMissingFreshECXPlus10,
    ChildMissingFreshECXPlus10AndEDX,
    ChildMissingEDX,
    ChildUnequalTo9275,
    ChildEqualToFallthrough9270
}

public enum CapacityFinalCalleeLargeRejectFieldLifecycleChildECXSource
{
    None,
    ParentEntryECX
}

public enum CapacityFinalCalleeLargeRejectFieldLifecycleChildEDXSource
{
    None,
    ExplicitIndependentLateCapture
}

public enum CapacityFinalCalleeLargeRejectFieldLifecycleFreshValueSource
{
    None,
    ExplicitIndependentMemoryEvidence
}

public readonly record struct CapacityFinalCalleeLargeRejectFieldLifecycleFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public sealed record CapacityFinalCalleeLargeRejectFieldLifecyclePlan(
    CapacityFinalCalleeLargeRejectFieldLifecycleCapture Capture,
    CapacityFinalCalleeLargeIndexedRejectGatePlan ParentGate,
    CapacityFinalCalleeLargeIndexedFieldGatePlan? ChildFieldGate,
    CapacityFinalCalleeLargeRejectFieldLifecycleOutcome Outcome)
{
    public bool ExactCleanHandoffToFieldGate { get; init; }
    public bool FieldGateReached => ChildFieldGate is not null;
    public CapacityFinalCalleeLargeRejectFieldLifecycleChildECXSource
        ChildECXSource { get; init; }
    public uint? ChildEntryECX { get; init; }
    public CapacityFinalCalleeLargeRejectFieldLifecycleChildEDXSource
        ChildEDXSource { get; init; }
    public uint? ChildEntryEDX { get; init; }
    public CapacityFinalCalleeLargeRejectFieldLifecycleFreshValueSource
        ChildFreshECXPlus10Source { get; init; }
    public uint? ChildFreshECXPlus10 { get; init; }
    public bool UnreachedChildEDXCaptureProvided { get; init; }
    public bool UnreachedChildFreshECXPlus10CaptureProvided { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateRead> OrderedReads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedRejectGateEvent> ParentEvents =>
        ParentGate.Events;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateEvent> ChildEvents =>
        ChildFieldGate?.Events
        ?? ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateEvent>.Empty;
    public CapacityFinalCalleeLargeRejectFieldLifecycleFlags? FlagsAtStop =>
        ChildFieldGate is { CompareReached: true } child
            ? ConvertFlags(child.CompareFlags)
            : ConvertFlags(ParentGate.CompareFlags);
    public bool FieldStoreReached => false;
    public bool MemoryWriteReached => false;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeLargeRejectFieldLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeLargeIndexedRejectGateFlags? flags) =>
        flags is CapacityFinalCalleeLargeIndexedRejectGateFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeRejectFieldLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeLargeIndexedFieldGateFlags? flags) =>
        flags is CapacityFinalCalleeLargeIndexedFieldGateFlags value
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
/// Joins the exact not-below edge at 926B to the indexed-field compare, without
/// extending either bounded branch into the later field-store instructions.
/// </summary>
public static class CapacityFinalCalleeLargeRejectFieldLifecycleProjection
{
    public const uint RejectGateEntryRawFileOffset =
        CapacityFinalCalleeLargeIndexedRejectGateProjection.EntryRawFileOffset;
    public const uint FieldGateEntryRawFileOffset =
        CapacityFinalCalleeLargeIndexedFieldGateProjection.EntryRawFileOffset;
    public const uint ParentBelowDestinationRawFileOffset =
        CapacityFinalCalleeLargeIndexedRejectGateProjection.BranchTargetRawFileOffset;
    public const uint ChildEqualDestinationRawFileOffset =
        CapacityFinalCalleeLargeIndexedFieldGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ChildUnequalDestinationRawFileOffset =
        CapacityFinalCalleeLargeIndexedFieldGateProjection.JnzDestinationRawFileOffset;

    public const string CapturedWindowBytes =
        "3B CB 0F 82 0B FC FF FF 39 51 10 75 05";

    public static ImmutableArray<CapacityFinalCalleeLargeIndexedRejectGateInstructionEvidence>
        Instructions { get; } =
        CapacityFinalCalleeLargeIndexedRejectGateProjection.Instructions
            .AddRange(CapacityFinalCalleeLargeIndexedFieldGateProjection.Instructions
                .Select(instruction =>
                    new CapacityFinalCalleeLargeIndexedRejectGateInstructionEvidence(
                        instruction.Observation,
                        instruction.RawFileOffset,
                        instruction.FlatIdaAddress,
                        instruction.VirtualAddress,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress)));

    public static CapacityFinalCalleeLargeRejectFieldLifecyclePlan Project(
        CapacityFinalCalleeLargeRejectFieldLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeIndexedRejectGatePlan parent =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(
                capture.ParentCapture);

        bool IsExactParentBelowExit() =>
            parent.Outcome
                == CapacityFinalCalleeLargeIndexedRejectGateOutcome.UnsignedBelowToTarget
            && parent.ExactCleanExitAdmitted
            && parent.EntryECXCaptured
            && parent.EntryEBXCaptured
            && parent.CompareReached
            && parent.BranchReached
            && parent.BranchTaken == true
            && parent.BranchSelectedDestinationRawFileOffset
                == ParentBelowDestinationRawFileOffset
            && parent.StopAtRawFileOffsetExclusive
                == ParentBelowDestinationRawFileOffset;

        bool IsExactParentNotBelowExit() =>
            parent.Outcome
                == CapacityFinalCalleeLargeIndexedRejectGateOutcome
                    .UnsignedNotBelowToFallthrough
            && parent.ExactCleanExitAdmitted
            && parent.EntryECXCaptured
            && parent.EntryEBXCaptured
            && parent.CompareReached
            && parent.BranchReached
            && parent.BranchTaken == false
            && parent.BranchSelectedDestinationRawFileOffset
                == FieldGateEntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive
                == FieldGateEntryRawFileOffset;

        bool childEDXProvided = capture.ChildEDX.HasValue;
        bool childFreshProvided = capture.ChildFreshECXPlus10.HasValue;
        bool parentBelow = IsExactParentBelowExit();
        if (!IsExactParentNotBelowExit())
        {
            bool unusedLaterCaptures = childEDXProvided || childFreshProvided;
            return new(
                capture,
                parent,
                null,
                parentBelow
                    ? unusedLaterCaptures
                        ? CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                            .ParentUnsignedBelowTo8E76WithUnusedChildCaptures
                        : CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                            .ParentUnsignedBelowTo8E76
                    : CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                        .ParentGateIncomplete)
            {
                UnreachedChildEDXCaptureProvided = childEDXProvided,
                UnreachedChildFreshECXPlus10CaptureProvided = childFreshProvided,
                ReachedInstructions = parent.ReachedInstructions,
                ExactCleanExitAdmitted = parentBelow && !unusedLaterCaptures,
                StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
            };
        }

        if (parent.CompareLeft is not uint retainedParentEntryECX)
        {
            throw new InvalidOperationException(
                "The completed parent compare must retain its entry ECX.");
        }

        CapacityFinalCalleeLargeIndexedFieldGatePlan child =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: retainedParentEntryECX,
                EntryEDX: capture.ChildEDX,
                FreshECXPlus10: capture.ChildFreshECXPlus10));

        bool exactChildExit = IsExactChildExit(child);
        return new(
            capture,
            parent,
            child,
            MapChildOutcome(child.Outcome))
        {
            ExactCleanHandoffToFieldGate = true,
            ChildECXSource =
                CapacityFinalCalleeLargeRejectFieldLifecycleChildECXSource.ParentEntryECX,
            ChildEntryECX = retainedParentEntryECX,
            ChildEDXSource = child.EntryEDXCaptured
                ? CapacityFinalCalleeLargeRejectFieldLifecycleChildEDXSource
                    .ExplicitIndependentLateCapture
                : CapacityFinalCalleeLargeRejectFieldLifecycleChildEDXSource.None,
            ChildEntryEDX = child.EntryEDXCaptured ? capture.ChildEDX : null,
            ChildFreshECXPlus10Source = child.FreshECXPlus10Captured
                ? CapacityFinalCalleeLargeRejectFieldLifecycleFreshValueSource
                    .ExplicitIndependentMemoryEvidence
                : CapacityFinalCalleeLargeRejectFieldLifecycleFreshValueSource.None,
            ChildFreshECXPlus10 =
                child.FreshECXPlus10Captured ? capture.ChildFreshECXPlus10 : null,
            ReachedInstructions =
                parent.ReachedInstructions.AddRange(child.ReachedInstructions),
            OrderedReads = child.Reads,
            ExactCleanExitAdmitted = exactChildExit,
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactChildExit(
        CapacityFinalCalleeLargeIndexedFieldGatePlan plan) =>
        plan.ExactCleanCompletion
        && plan.CompareInstructionReached
        && plan.EntryECXCaptured
        && plan.FreshECXPlus10ReadReached
        && plan.FreshECXPlus10Captured
        && plan.EntryEDXCaptured
        && plan.CompareReached
        && plan.BranchReached
        && plan.BranchTaken is bool branchTaken
        && (branchTaken
            ? plan.Outcome == CapacityFinalCalleeLargeIndexedFieldGateOutcome.UnequalToJnz9275
            : plan.Outcome == CapacityFinalCalleeLargeIndexedFieldGateOutcome.EqualToFallthrough9270)
        && !plan.UnreachedFreshECXPlus10CaptureProvided
        && !plan.UnreachedEntryEDXCaptureProvided
        && plan.BranchSelectedDestinationRawFileOffset
            == (branchTaken
                ? ChildUnequalDestinationRawFileOffset
                : ChildEqualDestinationRawFileOffset)
        && plan.StopAtRawFileOffsetExclusive
            == plan.BranchSelectedDestinationRawFileOffset
        && plan.MemoryWriteReached == false;

    private static CapacityFinalCalleeLargeRejectFieldLifecycleOutcome MapChildOutcome(
        CapacityFinalCalleeLargeIndexedFieldGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingFreshECXPlus10 =>
                CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                    .ChildMissingFreshECXPlus10,
            CapacityFinalCalleeLargeIndexedFieldGateOutcome
                .MissingFreshECXPlus10AndEntryEDX =>
                CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                    .ChildMissingFreshECXPlus10AndEDX,
            CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryEDX =>
                CapacityFinalCalleeLargeRejectFieldLifecycleOutcome.ChildMissingEDX,
            CapacityFinalCalleeLargeIndexedFieldGateOutcome.UnequalToJnz9275 =>
                CapacityFinalCalleeLargeRejectFieldLifecycleOutcome.ChildUnequalTo9275,
            CapacityFinalCalleeLargeIndexedFieldGateOutcome.EqualToFallthrough9270 =>
                CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                    .ChildEqualToFallthrough9270,
            CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryECX =>
                throw new InvalidOperationException(
                    "Child ECX is derived from the completed parent entry compare."),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };
}
