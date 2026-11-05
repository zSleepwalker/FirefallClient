using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightFieldTagLifecycleCapture(
    uint? EntryEDX,
    uint? FreshEDXPlus14,
    uint? FreshGlobalValue,
    uint? LateEntryEAX,
    uint? LateEntryEDI,
    uint? LateEntryESI);

public enum CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
{
    None,
    FreshZeroBypass92BD,
    ThresholdNotBelowPairedStores92BD
}

public enum CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome
{
    ParentStopped,
    ParentStoppedWithUnusedChildCaptures,
    MissingLateEntryEAXAtFirstStore,
    ChildStoppedAtMOV,
    ChildStoppedAtFirstTagStore,
    Completed
}

public enum CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind
{
    ParentFirstStore,
    ParentSecondStore,
    TagFirstStore,
    TagSecondStore
}

public readonly record struct CapacityFinalCalleeLargeRightFieldTagLifecycleWrite(
    CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind Kind,
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeLargeRightFieldTagLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeRightFieldTagLifecyclePlan(
    CapacityFinalCalleeLargeRightFieldTagLifecycleCapture Capture,
    CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan ParentLifecycle,
    CapacityFinalCalleeCommonTailTagWritesPlan? CommonTail,
    CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute ParentRoute,
    CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeRightFieldThresholdLifecycleRead> Reads =>
        ParentLifecycle.Reads;

    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome ParentOutcome =>
        ParentLifecycle.ParentOutcome;

    public bool ParentBranchReached => ParentLifecycle.ParentBranchReached;
    public bool? ParentBranchTaken => ParentLifecycle.ParentBranchTaken;
    public uint? ParentBranchSelectedDestinationRawFileOffset =>
        ParentLifecycle.ParentBranchSelectedDestinationRawFileOffset;
    public uint ParentStopAtRawFileOffsetExclusive =>
        ParentLifecycle.ParentStopAtRawFileOffsetExclusive;
    public bool ExactCleanParentHandoff =>
        ParentLifecycle.ExactCleanCompletion
        && ParentRoute != CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute.None;

    public bool LateEntryEAXCaptureProvided =>
        ParentLifecycle.LateEntryEAXCaptureProvided;
    public bool LateEntryEAXCaptureReached =>
        ParentLifecycle.LateEntryEAXCaptureReached;
    public bool UnusedLateEntryEAXCaptureProvided =>
        ParentLifecycle.UnusedLateEntryEAXCaptureProvided;

    public bool LateEntryEDICaptureProvided => Capture.LateEntryEDI.HasValue;
    public bool LateEntryEDICaptureReached => CommonTail is not null;
    public bool UnusedLateEntryEDICaptureProvided =>
        LateEntryEDICaptureProvided && !LateEntryEDICaptureReached;
    public bool LateEntryESICaptureProvided => Capture.LateEntryESI.HasValue;
    public bool LateEntryESICaptureReached =>
        CommonTail?.EntryESICaptured == true;
    public bool UnusedLateEntryESICaptureProvided =>
        LateEntryESICaptureProvided && !LateEntryESICaptureReached;

    public bool Child92BDEntered => CommonTail is not null;
    public bool ChildMOVAt92BDAttempted => Child92BDEntered;
    public bool ChildMOVAt92BDExecuted => CommonTail?.EAXLoadedFromEDI == true;
    public uint? ChildEAXBeforeOR => CommonTail?.EAXRetained;
    public uint? ChildEAXAfterOR => CommonTail?.EAXAfterOr;
    public bool ChildORReached => CommonTail?.OrInstructionReached == true;
    public bool FirstTagStoreAttempted =>
        CommonTail?.FirstStoreReached == true
        || CommonTail?.Outcome
            == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI;
    public bool FirstTagStoreCommitted =>
        CommonTail?.Writes.Any(write =>
            write.RawFileOffset
                == CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset)
            == true;
    public bool SecondTagStoreAttempted => CommonTail?.SecondStoreReached == true;
    public bool SecondTagStoreCommitted =>
        CommonTail?.Writes.Any(write =>
            write.RawFileOffset
                == CapacityFinalCalleeCommonTailTagWritesProjection.SecondStoreRawFileOffset)
            == true;
    public bool Final92C8Entered => false;
    public bool FinalBoundaryInstructionExecuted => false;

    public ImmutableArray<CapacityFinalCalleeLargeRightPairedStoresWrite> ParentWrites =>
        ParentLifecycle.Writes;
    public ImmutableArray<CapacityFinalCalleeCommonTailTagWritesWrite> TagWrites =>
        CommonTail?.Writes
        ?? ImmutableArray<CapacityFinalCalleeCommonTailTagWritesWrite>.Empty;

    public ImmutableArray<CapacityFinalCalleeLargeRightFieldTagLifecycleWrite>
        OrderedWrites { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeRightFieldTagLifecycleWrite>.Empty;

    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? ParentFlagsAtStop =>
        ParentLifecycle.ParentFlagsAtStop;
    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? ParentFlagsAtChildEntry =>
        Child92BDEntered ? ParentLifecycle.ParentFlagsAtStop : null;
    public CapacityFinalCalleeCommonTailTagWritesFlags? TagORFlags =>
        CommonTail?.OrFlags;
    public CapacityFinalCalleeCommonTailTagWritesFlags? FlagsAtStop =>
        CommonTail?.OrInstructionReached == true
            ? CommonTail.OrFlags
            : Convert(ParentLifecycle.ParentFlagsAtStop);

    public bool FlagsPreservedUntilTagOR =>
        (!Child92BDEntered || !ChildORReached)
            ? FlagsAtStop == Convert(ParentLifecycle.ParentFlagsAtStop)
            : ParentFlagsAtChildEntry == ParentLifecycle.ParentFlagsAtStop
                && FlagsAtStop == TagORFlags;

    public bool ExactCleanCompletion =>
        ExactCleanParentHandoff
        && CommonTail is
        {
            Outcome: CapacityFinalCalleeCommonTailTagWritesOutcome.Completed,
            ExactCleanExitAdmitted: true,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeCommonTailTagWritesProjection.BoundaryRawFileOffsetExclusive
        } commonTail
        && commonTail.Writes.Length == 2
        && FirstTagStoreCommitted
        && SecondTagStoreCommitted
        && !Final92C8Entered
        && !FinalBoundaryInstructionExecuted
        && !UnusedLateEntryEDICaptureProvided
        && !UnusedLateEntryESICaptureProvided;

    public uint StopAtRawFileOffsetExclusive =>
        CommonTail?.StopAtRawFileOffsetExclusive
        ?? ParentLifecycle.StopAtRawFileOffsetExclusive;

    private static CapacityFinalCalleeCommonTailTagWritesFlags? Convert(
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.AuxiliaryCarry,
                value.Zero, value.Sign, value.Overflow)
            : null;
}

/// <summary>
/// Joins the two exact right-field parent routes at 92BD to the common tag
/// MOV/OR/two-store child and stops before 92C8.
/// </summary>
public static class CapacityFinalCalleeLargeRightFieldTagLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeRightFieldStoresLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonTailTagWritesProjection.EntryRawFileOffset;
    public const uint FinalBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailTagWritesProjection.BoundaryRawFileOffsetExclusive;
    public const uint ZeroBypassRawFileOffset =
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection
            .RightFieldZeroDestinationRawFileOffset;
    public const uint PairedStoresEntryRawFileOffset =
        CapacityFinalCalleeLargeRightPairedStoresProjection.EntryRawFileOffset;
    public const uint UnsignedBelowDestinationRawFileOffset =
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection
            .ThresholdBranchTargetRawFileOffset;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        CapacityFinalCalleeLargeRightFieldStoresLifecycleProjection
            .CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedWindowBytes =
        CapacityFinalCalleeLargeRightFieldStoresLifecycleProjection.CapturedWindowBytes
        + CapacityFinalCalleeCommonTailTagWritesProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeLargeRightFieldTagLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeRightFieldTagLifecyclePlan Project(
        CapacityFinalCalleeLargeRightFieldTagLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan parent =
            CapacityFinalCalleeLargeRightFieldStoresLifecycleProjection.Project(new(
                capture.EntryEDX,
                capture.FreshEDXPlus14,
                capture.FreshGlobalValue,
                capture.LateEntryEAX));

        CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute parentRoute =
            ClassifyExactParentRoute(parent);
        var orderedWrites =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRightFieldTagLifecycleWrite>();
        foreach (CapacityFinalCalleeLargeRightPairedStoresWrite write in parent.Writes)
        {
            orderedWrites.Add(new(
                write.RawFileOffset
                    == CapacityFinalCalleeLargeRightPairedStoresProjection.FirstStoreRawFileOffset
                    ? CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.ParentFirstStore
                    : CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.ParentSecondStore,
                write.RawFileOffset,
                write.Address,
                write.Value));
        }

        if (parentRoute
            == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute.None)
        {
            bool unusedChildCapture =
                capture.LateEntryEDI.HasValue || capture.LateEntryESI.HasValue;
            return new(
                capture,
                parent,
                CommonTail: null,
                parentRoute,
                unusedChildCapture
                    || parent.UnusedLateEntryEAXCaptureProvided
                    ? CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome
                        .ParentStoppedWithUnusedChildCaptures
                    : parent.Outcome
                        == CapacityFinalCalleeLargeRightFieldStoresLifecycleOutcome
                            .MissingLateEntryEAXAtFirstStore
                        ? CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome
                            .MissingLateEntryEAXAtFirstStore
                        : CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome.ParentStopped)
            {
                OrderedWrites = orderedWrites.ToImmutable()
            };
        }

        CapacityFinalCalleeCommonTailTagWritesPlan commonTail =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(
                EntryEDI: capture.LateEntryEDI,
                EntryESI: capture.LateEntryESI));
        foreach (CapacityFinalCalleeCommonTailTagWritesWrite write in commonTail.Writes)
        {
            orderedWrites.Add(new(
                write.RawFileOffset
                    == CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset
                    ? CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.TagFirstStore
                    : CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.TagSecondStore,
                write.RawFileOffset,
                write.Address,
                write.Value));
        }

        CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome outcome =
            commonTail.Outcome switch
            {
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI =>
                    CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome.ChildStoppedAtMOV,
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI =>
                    CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome
                        .ChildStoppedAtFirstTagStore,
                CapacityFinalCalleeCommonTailTagWritesOutcome.Completed =>
                    CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome.Completed,
                _ => throw new InvalidOperationException(
                    "The common tag child returned an unexpected outcome.")
            };

        return new(capture, parent, commonTail, parentRoute, outcome)
        {
            OrderedWrites = orderedWrites.ToImmutable()
        };
    }

    private static CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
        ClassifyExactParentRoute(
            CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan parent)
    {
        bool cleanZeroBypass =
            parent.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome.ZeroTo92BD
            && parent.ExactCleanParentExitAdmitted
            && parent.ParentBranchReached
            && parent.ParentBranchTaken == true
            && parent.ParentBranchSelectedDestinationRawFileOffset == ZeroBypassRawFileOffset
            && parent.ParentStopAtRawFileOffsetExclusive == ZeroBypassRawFileOffset
            && parent.PairedStores is null
            && !parent.LateEntryEAXCaptureProvided;
        if (cleanZeroBypass)
        {
            return CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
                .FreshZeroBypass92BD;
        }

        bool cleanNotBelowWithPairedStores =
            parent.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && parent.ExactCleanParentExitAdmitted
            && parent.ParentBranchReached
            && parent.ParentBranchTaken == false
            && parent.ParentBranchSelectedDestinationRawFileOffset
                == PairedStoresEntryRawFileOffset
            && parent.ParentStopAtRawFileOffsetExclusive == PairedStoresEntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
            && parent.ExactCleanCompletion
            && parent.PairedStores is
            {
                Outcome: CapacityFinalCalleeLargeRightPairedStoresOutcome.Completed,
                ExactCleanCompletion: true,
                FirstStoreCommitted: true,
                SecondStoreCommitted: true,
                StopAtRawFileOffsetExclusive:
                    CapacityFinalCalleeLargeRightPairedStoresProjection
                        .BoundaryRawFileOffsetExclusive,
                BoundaryInstructionExecuted: false
            }
            && parent.Writes.Length == 2;
        return cleanNotBelowWithPairedStores
            ? CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
                .ThresholdNotBelowPairedStores92BD
            : CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute.None;
    }

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightFieldTagLifecycleInstructionEvidence>
        BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRightFieldTagLifecycleInstructionEvidence>();

        foreach (CapacityFinalCalleeLargeRightFieldStoresLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeLargeRightFieldStoresLifecycleProjection.Instructions)
        {
            instructions.Add(new(
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                instruction.Bytes,
                instruction.BranchTargetRawFileOffset,
                instruction.BranchTargetVirtualAddress));
        }

        foreach (CapacityFinalCalleeCommonTailTagWritesInstructionEvidence instruction
            in CapacityFinalCalleeCommonTailTagWritesProjection.Instructions)
        {
            instructions.Add(new(
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                instruction.Bytes));
        }

        return instructions.ToImmutable();
    }
}
