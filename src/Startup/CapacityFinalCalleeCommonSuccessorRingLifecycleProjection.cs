using System;
using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonSuccessorRingLifecycleCapture(
    uint? ParentEntryEDX,
    uint? ParentFreshEDXPlus8,
    uint? ParentFreshGlobal1EDADD8,
    uint? ChildEntryESI);

public enum CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
{
    ParentMissingEntryEDX,
    ParentMissingFreshEDXPlus8,
    ParentMissingFreshGlobal1EDADD8,
    ParentUnsignedBelowBranchTarget,
    ParentHandoffNotVerified,
    ChildMissingESIForFirstWrite,
    CompletedTo012C9441
}

public enum CapacityFinalCalleeCommonSuccessorRingLifecycleStageKind
{
    ParentSuccessorThreshold,
    ChildRingWrites
}

public readonly record struct CapacityFinalCalleeCommonSuccessorRingLifecycleWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public sealed record CapacityFinalCalleeCommonSuccessorRingLifecycleStage(
    CapacityFinalCalleeCommonSuccessorRingLifecycleStageKind Kind,
    ImmutableArray<CapacityFinalCalleeCommonSuccessorRingLifecycleWrite> Writes,
    uint StopAtRawFileOffsetExclusive,
    CapacityFinalCalleeCommonSuccessorThresholdGateFlags? FlagsAtStop,
    bool ExactCleanStop);

public sealed record CapacityFinalCalleeCommonSuccessorRingLifecyclePlan(
    CapacityFinalCalleeCommonSuccessorRingLifecycleCapture Capture,
    CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonSuccessorThresholdGateCapture ParentCapture { get; init; }
    public CapacityFinalCalleeCommonSuccessorThresholdGatePlan Parent { get; init; } = null!;
    public CapacityFinalCalleeCommonRingWritesCapture? ChildCapture { get; init; }
    public CapacityFinalCalleeCommonRingWritesPlan? Child { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonSuccessorRingLifecycleStage> Stages { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonSuccessorRingLifecycleStage>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonSuccessorRingLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonSuccessorRingLifecycleWrite>.Empty;
    public bool ParentExactBranchHandoff { get; init; }
    public uint? DerivedChildEntryEDX { get; init; }
    public uint? DerivedChildEntryEAX { get; init; }
    public bool DerivedChildEntryEAXCaptured { get; init; }
    public bool DerivedChildEntryEAXUnused { get; init; }
    public bool ExternalUnusedChildEntryEAXCaptureProvided => false;
    public bool UnreachedChildESICaptureProvided { get; init; }
    public CapacityFinalCalleeCommonSuccessorThresholdGateFlags? FlagsAtStop { get; init; }
    public bool ChildFlagsUnaffected { get; init; }
    public bool BranchReached { get; init; }
    public uint? BranchTargetRawFileOffset { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool DestinationEntered { get; init; }
    public bool EpilogueEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the exact unsigned not-below edge at 9343 to the shared four-write ring.
/// The child's EDX is retained from the gate entry and its EAX comes from the
/// gate's fresh EDX+8 read; only the late ESI input is independently captured.
/// </summary>
public static class CapacityFinalCalleeCommonSuccessorRingLifecycleProjection
{
    public const uint ParentEntryRawFileOffset =
        CapacityFinalCalleeCommonSuccessorThresholdGateProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonAlternateRingWritesProjection.EntryRawFileOffset;
    public const uint BelowDestinationRawFileOffset =
        CapacityFinalCalleeCommonSuccessorThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint ChildFallthroughRawFileOffset =
        CapacityFinalCalleeCommonSuccessorThresholdGateProjection.FallthroughRawFileOffset;
    public const uint CompletionRawFileOffset =
        CapacityFinalCalleeCommonAlternateRingWritesProjection.BranchTargetRawFileOffset;

    public static CapacityFinalCalleeCommonSuccessorRingLifecyclePlan Project(
        CapacityFinalCalleeCommonSuccessorRingLifecycleCapture capture)
    {
        var parentCapture = new CapacityFinalCalleeCommonSuccessorThresholdGateCapture(
            capture.ParentEntryEDX,
            capture.ParentFreshEDXPlus8,
            capture.ParentFreshGlobal1EDADD8);
        CapacityFinalCalleeCommonSuccessorThresholdGatePlan parent =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(parentCapture);

        ImmutableArray<CapacityFinalCalleeCommonSuccessorRingLifecycleStage>.Builder stages =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonSuccessorRingLifecycleStage>();
        stages.Add(new(
            CapacityFinalCalleeCommonSuccessorRingLifecycleStageKind.ParentSuccessorThreshold,
            ImmutableArray<CapacityFinalCalleeCommonSuccessorRingLifecycleWrite>.Empty,
            parent.StopAtRawFileOffsetExclusive,
            parent.CompareFlags,
            (IsExactBelowBranch(parent) || IsExactNotBelowBranch(parent))
                && !parent.UnreachedFreshEdxPlus8CaptureProvided
                && !parent.UnreachedFreshGlobalCaptureProvided));

        bool exactParentBelowBranch =
            IsExactBelowBranch(parent)
            && !parent.UnreachedFreshEdxPlus8CaptureProvided
            && !parent.UnreachedFreshGlobalCaptureProvided;
        bool exactParentNotBelowBranch =
            IsExactNotBelowBranch(parent)
            && !parent.UnreachedFreshEdxPlus8CaptureProvided
            && !parent.UnreachedFreshGlobalCaptureProvided;
        bool unreachedChildESI =
            !exactParentNotBelowBranch && capture.ChildEntryESI.HasValue;
        uint? derivedChildEDX = exactParentNotBelowBranch
            ? parentCapture.EntryEDX
            : null;
        uint? derivedChildEAX = exactParentNotBelowBranch
            ? parent.FreshEdxPlus8
            : null;
        bool derivedChildEAXCaptured = derivedChildEAX.HasValue;

        if (!exactParentNotBelowBranch)
        {
            CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome outcome =
                parent.Outcome switch
                {
                    CapacityFinalCalleeCommonSuccessorThresholdGateOutcome.MissingEntryEDX =>
                        CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                            .ParentMissingEntryEDX,
                    CapacityFinalCalleeCommonSuccessorThresholdGateOutcome.MissingFreshEdxPlus8 =>
                        CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                            .ParentMissingFreshEDXPlus8,
                    CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                        .MissingFreshGlobal1EDADD8 =>
                        CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                            .ParentMissingFreshGlobal1EDADD8,
                    CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                        .UnsignedBelowBranchTarget =>
                        CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                            .ParentUnsignedBelowBranchTarget,
                    _ => CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                        .ParentHandoffNotVerified
                };
            bool exactCleanStop = exactParentBelowBranch && !unreachedChildESI;

            return new(capture, outcome)
            {
                ParentCapture = parentCapture,
                Parent = parent,
                Stages = stages.ToImmutable(),
                ParentExactBranchHandoff = exactParentNotBelowBranch,
                DerivedChildEntryEDX = derivedChildEDX,
                DerivedChildEntryEAX = derivedChildEAX,
                DerivedChildEntryEAXCaptured = derivedChildEAXCaptured,
                DerivedChildEntryEAXUnused = derivedChildEAXCaptured,
                UnreachedChildESICaptureProvided = unreachedChildESI,
                FlagsAtStop = parent.CompareFlags,
                ExactCleanCompletion = exactCleanStop,
                StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
            };
        }

        var childCapture = new CapacityFinalCalleeCommonRingWritesCapture(
            derivedChildEDX,
            capture.ChildEntryESI.HasValue ? derivedChildEAX : null,
            capture.ChildEntryESI);
        CapacityFinalCalleeCommonRingWritesPlan child =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.Project(childCapture);
        ImmutableArray<CapacityFinalCalleeCommonSuccessorRingLifecycleWrite> childWrites =
            child.Writes
                .Select(write => new CapacityFinalCalleeCommonSuccessorRingLifecycleWrite(
                    write.RawFileOffset,
                    write.Address,
                    write.AddressCaptured,
                    write.ValueCaptured,
                    write.Value))
                .ToImmutableArray();
        bool childComplete =
            child.Outcome == CapacityFinalCalleeCommonRingWritesOutcome.CompletedTo012C9441
            && child.ExactCleanCompletion
            && child.BranchReached
            && child.BranchTargetRawFileOffset == CompletionRawFileOffset
            && child.StopAtRawFileOffsetExclusive == CompletionRawFileOffset
            && !child.DestinationEntered;
        bool derivedEAXUnused = !child.EntryEAXReadReached && derivedChildEAXCaptured;
        stages.Add(new(
            CapacityFinalCalleeCommonSuccessorRingLifecycleStageKind.ChildRingWrites,
            childWrites,
            child.StopAtRawFileOffsetExclusive,
            parent.CompareFlags,
            childComplete));

        return new(capture, child.Outcome switch
        {
            CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryESIForFirstWrite =>
                CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ChildMissingESIForFirstWrite,
            CapacityFinalCalleeCommonRingWritesOutcome.CompletedTo012C9441 =>
                CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .CompletedTo012C9441,
            _ => throw new InvalidOperationException(
                "The ring-write projection returned an unexpected outcome.")
        })
        {
            ParentCapture = parentCapture,
            Parent = parent,
            ChildCapture = childCapture,
            Child = child,
            Stages = stages.ToImmutable(),
            Writes = childWrites,
            ParentExactBranchHandoff = exactParentNotBelowBranch,
            DerivedChildEntryEDX = derivedChildEDX,
            DerivedChildEntryEAX = derivedChildEAX,
            DerivedChildEntryEAXCaptured = derivedChildEAXCaptured,
            DerivedChildEntryEAXUnused = derivedEAXUnused,
            UnreachedChildESICaptureProvided = false,
            FlagsAtStop = parent.CompareFlags,
            ChildFlagsUnaffected = child.FlagsUnaffected,
            BranchReached = child.BranchReached,
            BranchTargetRawFileOffset = child.BranchTargetRawFileOffset,
            ExactCleanCompletion = exactParentNotBelowBranch
                && childComplete
                && !child.UnreachedEntryEAXCaptureProvided
                && !unreachedChildESI,
            DestinationEntered = child.DestinationEntered,
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactBelowBranch(
        CapacityFinalCalleeCommonSuccessorThresholdGatePlan parent) =>
        parent.ExactCleanAdmission
        && parent.Outcome
            == CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                .UnsignedBelowBranchTarget
        && parent.JBReached
        && parent.JBTaken == true
        && parent.JBSelectedDestinationRawFileOffset
            == BelowDestinationRawFileOffset
        && parent.StopAtRawFileOffsetExclusive == BelowDestinationRawFileOffset;

    private static bool IsExactNotBelowBranch(
        CapacityFinalCalleeCommonSuccessorThresholdGatePlan parent) =>
        parent.ExactCleanAdmission
        && parent.Outcome
            == CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                .UnsignedNotBelowFallthrough
        && parent.EntryEDXCaptured
        && parent.FreshEdxPlus8ReadReached
        && parent.FreshEdxPlus8Captured
        && parent.FreshGlobalReadReached
        && parent.FreshGlobalCaptured
        && parent.CompareReached
        && parent.CompareFlags.HasValue
        && parent.JBReached
        && parent.JBTaken == false
        && parent.JBSelectedDestinationRawFileOffset
            == ChildFallthroughRawFileOffset
        && parent.StopAtRawFileOffsetExclusive == ChildFallthroughRawFileOffset;
}
