using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonOrRingLifecycleCapture(
    uint? ParentEntryECX,
    uint? ParentEntryEDI,
    uint? ChildEntryEDX,
    uint? ChildEntryEAX,
    uint? ChildEntryESI);

public enum CapacityFinalCalleeCommonOrRingLifecycleOutcome
{
    MissingParentECX,
    MissingParentEDI,
    MissingBothParentOperands,
    ParentHandoffNotVerified,
    MissingChildEDXForFirstWrite,
    MissingChildESIForFirstWrite,
    MissingChildEAXForSecondWrite,
    CompletedTo012C9441
}

public enum CapacityFinalCalleeCommonOrRingLifecycleStageKind
{
    ParentOrStore,
    ChildRingWrites
}

public readonly record struct CapacityFinalCalleeCommonOrRingLifecycleWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value,
    bool IsParentGlobalWrite)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public sealed record CapacityFinalCalleeCommonOrRingLifecycleStage(
    CapacityFinalCalleeCommonOrRingLifecycleStageKind Kind,
    ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleWrite> Writes,
    uint StopAtRawFileOffsetExclusive,
    CapacityFinalCalleeCommonMaskOrStoreFlags? FlagsAtStop,
    bool ExactCleanStop);

public sealed record CapacityFinalCalleeCommonOrRingLifecyclePlan(
    CapacityFinalCalleeCommonOrRingLifecycleCapture Capture,
    CapacityFinalCalleeCommonOrRingLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonMaskOrStorePlan? ParentPlan { get; init; }
    public CapacityFinalCalleeCommonRingWritesPlan? ChildPlan { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleStage> Stages { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleStage>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleWrite>.Empty;
    public bool ParentExactCleanHandoffVerified { get; init; }
    public bool ChildEntryEAXCaptureWithheld { get; init; }
    public bool ExternalUnusedChildEntryEAXCaptureProvided { get; init; }
    public bool GlobalWriteReached { get; init; }
    public uint? GlobalWriteAddress { get; init; }
    public uint? GlobalWriteValue { get; init; }
    public CapacityFinalCalleeCommonMaskOrStoreFlags? FlagsAtStop { get; init; }
    public bool ChildFlagsUnaffected { get; init; }
    public bool BranchReached { get; init; }
    public uint? BranchTargetRawFileOffset { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool DestinationEntered { get; init; }
    public bool EpilogueEntered { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the exact common OR/global-store handoff at 9323 with the four ring writes.
/// </summary>
public static class CapacityFinalCalleeCommonOrRingLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonMaskOrStoreProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonRingWritesProjection.EntryRawFileOffset;
    public const uint CompletionRawFileOffset =
        CapacityFinalCalleeCommonRingWritesProjection.BranchTargetRawFileOffset;

    public static CapacityFinalCalleeCommonOrRingLifecyclePlan Project(
        CapacityFinalCalleeCommonOrRingLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonMaskOrStorePlan parent =
            CapacityFinalCalleeCommonMaskOrStoreProjection.Project(new(
                capture.ParentEntryECX,
                capture.ParentEntryEDI));
        bool parentHandoffVerified = IsExactParentHandoff(parent);
        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleStage>.Builder stages =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonOrRingLifecycleStage>();

        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleWrite> parentWrites =
            parent.Writes
                .Select(write => new CapacityFinalCalleeCommonOrRingLifecycleWrite(
                    write.RawFileOffset,
                    write.Address,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Value: write.Value,
                    IsParentGlobalWrite: true))
                .ToImmutableArray();
        stages.Add(new(
            CapacityFinalCalleeCommonOrRingLifecycleStageKind.ParentOrStore,
            parentWrites,
            parent.StopAtRawFileOffsetExclusive,
            parent.FlagsAtStop,
            parentHandoffVerified));

        if (!parentHandoffVerified)
        {
            return Finish(
                capture,
                parent,
                child: null,
                outcome: parent.Outcome switch
                {
                    CapacityFinalCalleeCommonMaskOrStoreOutcome.MissingEntryECX =>
                        CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingParentECX,
                    CapacityFinalCalleeCommonMaskOrStoreOutcome.MissingEntryEDI =>
                        CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingParentEDI,
                    CapacityFinalCalleeCommonMaskOrStoreOutcome.MissingBothEntryOperands =>
                        CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingBothParentOperands,
                    _ => CapacityFinalCalleeCommonOrRingLifecycleOutcome.ParentHandoffNotVerified
                },
                stages,
                parentHandoffVerified,
                childEntryEAXCaptureWithheld: false,
                externalUnusedChildEntryEAXCaptureProvided: capture.ChildEntryEAX.HasValue,
                parentWrites,
                childWrites: ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleWrite>.Empty,
                stopAt: parent.StopAtRawFileOffsetExclusive);
        }

        bool firstStoreOperandsKnown =
            capture.ChildEntryEDX.HasValue && capture.ChildEntryESI.HasValue;
        uint? childEAX = firstStoreOperandsKnown ? capture.ChildEntryEAX : null;
        CapacityFinalCalleeCommonRingWritesPlan child =
            CapacityFinalCalleeCommonRingWritesProjection.Project(new(
                capture.ChildEntryEDX,
                childEAX,
                capture.ChildEntryESI));
        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleWrite> childWrites =
            child.Writes
                .Select(write => new CapacityFinalCalleeCommonOrRingLifecycleWrite(
                    write.RawFileOffset,
                    write.Address,
                    write.AddressCaptured,
                    write.ValueCaptured,
                    write.Value,
                    IsParentGlobalWrite: false))
                .ToImmutableArray();
        stages.Add(new(
            CapacityFinalCalleeCommonOrRingLifecycleStageKind.ChildRingWrites,
            childWrites,
            child.StopAtRawFileOffsetExclusive,
            parent.FlagsAtStop,
            child.ExactCleanCompletion
                && child.StopAtRawFileOffsetExclusive == CompletionRawFileOffset));

        return Finish(
            capture,
            parent,
            child,
            child.Outcome switch
            {
                CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryEDXForFirstWrite =>
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingChildEDXForFirstWrite,
                CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryESIForFirstWrite =>
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingChildESIForFirstWrite,
                CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryEAXForSecondWrite =>
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingChildEAXForSecondWrite,
                _ => CapacityFinalCalleeCommonOrRingLifecycleOutcome.CompletedTo012C9441
            },
            stages,
            parentHandoffVerified,
            childEntryEAXCaptureWithheld:
                capture.ChildEntryEAX.HasValue && !firstStoreOperandsKnown,
            externalUnusedChildEntryEAXCaptureProvided:
                capture.ChildEntryEAX.HasValue && !child.EntryEAXReadReached,
            parentWrites,
            childWrites,
            child.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeCommonOrRingLifecyclePlan Finish(
        CapacityFinalCalleeCommonOrRingLifecycleCapture capture,
        CapacityFinalCalleeCommonMaskOrStorePlan parent,
        CapacityFinalCalleeCommonRingWritesPlan? child,
        CapacityFinalCalleeCommonOrRingLifecycleOutcome outcome,
        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleStage>.Builder stages,
        bool parentHandoffVerified,
        bool childEntryEAXCaptureWithheld,
        bool externalUnusedChildEntryEAXCaptureProvided,
        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleWrite> parentWrites,
        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleWrite> childWrites,
        uint stopAt)
    {
        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleStage> immutableStages =
            stages.ToImmutable();
        ImmutableArray<CapacityFinalCalleeCommonOrRingLifecycleWrite> writes =
            parentWrites.AddRange(childWrites);
        bool branchReached = child?.BranchReached == true;
        bool exactCleanCompletion =
            parentHandoffVerified
            && child is not null
            && child.ExactCleanCompletion
            && child.StopAtRawFileOffsetExclusive == CompletionRawFileOffset
            && branchReached
            && child.BranchTargetRawFileOffset == CompletionRawFileOffset
            && !externalUnusedChildEntryEAXCaptureProvided
            && !child.DestinationEntered;

        return new(capture, outcome)
        {
            ParentPlan = parent,
            ChildPlan = child,
            Stages = immutableStages,
            Writes = writes,
            ParentExactCleanHandoffVerified = parentHandoffVerified,
            ChildEntryEAXCaptureWithheld = childEntryEAXCaptureWithheld,
            ExternalUnusedChildEntryEAXCaptureProvided =
                externalUnusedChildEntryEAXCaptureProvided,
            GlobalWriteReached = parent.GlobalWriteReached,
            GlobalWriteAddress = parent.GlobalWriteAddress,
            GlobalWriteValue = parent.GlobalWriteValue,
            FlagsAtStop = parent.FlagsAtStop,
            ChildFlagsUnaffected = child?.FlagsUnaffected == true,
            BranchReached = branchReached,
            BranchTargetRawFileOffset = child?.BranchTargetRawFileOffset,
            ExactCleanCompletion = exactCleanCompletion,
            DestinationEntered = child?.DestinationEntered == true,
            EpilogueEntered = false,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static bool IsExactParentHandoff(
        CapacityFinalCalleeCommonMaskOrStorePlan parent) =>
        parent.ExactCleanExitAdmitted
        && parent.Outcome == CapacityFinalCalleeCommonMaskOrStoreOutcome.Completed
        && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
        && parent.GlobalWriteReached
        && parent.GlobalWriteAddress
            == CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalVirtualAddress
        && parent.GlobalWriteValue == parent.OrResult
        && parent.Writes is
            [{
                RawFileOffset: CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalStoreRawFileOffset,
                Address: CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalVirtualAddress
            }]
        && parent.Writes[0].Value == parent.OrResult;
}
