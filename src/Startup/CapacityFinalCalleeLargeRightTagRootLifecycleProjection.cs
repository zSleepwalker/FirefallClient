using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightTagRootLifecycleCapture(
    CapacityFinalCalleeLargeRightFieldTagLifecycleCapture Parent,
    uint? FreshRootGlobalDWORD);

public enum CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
{
    ParentStopped,
    ParentStoppedWithUnusedFreshRootGlobalDWORD,
    MissingFreshRootGlobalDWORD,
    EqualToFallthrough92D0,
    UnequalToBranchTarget92EC
}

public enum CapacityFinalCalleeLargeRightTagRootLifecycleReadStage
{
    ParentRightFieldGate,
    ParentThresholdGate,
    RootGate
}

public readonly record struct CapacityFinalCalleeLargeRightTagRootLifecycleRead(
    CapacityFinalCalleeLargeRightTagRootLifecycleReadStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeRightTagRootLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeRightTagRootLifecyclePlan(
    CapacityFinalCalleeLargeRightTagRootLifecycleCapture Capture,
    CapacityFinalCalleeLargeRightFieldTagLifecyclePlan ParentLifecycle,
    CapacityFinalCalleeCommonTailRootGatePlan? RootGate,
    CapacityFinalCalleeLargeRightTagRootLifecycleOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeRightTagRootLifecycleRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeRightTagRootLifecycleRead>.Empty;

    public ImmutableArray<CapacityFinalCalleeLargeRightFieldTagLifecycleWrite> OrderedWrites =>
        ParentLifecycle.OrderedWrites;

    public bool ExactCleanParentHandoff =>
        ParentLifecycle.ExactCleanCompletion
        && ParentLifecycle.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeLargeRightTagRootLifecycleProjection.RootEntryRawFileOffset;

    public bool RootGateEntered => RootGate is not null;
    public bool RootEntryESICaptured => RootGate?.EntryESICaptured == true;
    public uint? RootEntryESI => RootGate?.Capture.EntryESI;
    public bool RootGlobalReadAttempted => RootGate?.FreshGlobalReadReached == true;
    public uint? RootGlobalReadAddress => RootGate?.FreshGlobalAddress;
    public bool RootGlobalCaptured => RootGate?.FreshGlobalCaptured == true;
    public uint? FreshRootGlobalDWORD => RootGate?.FreshGlobalDWORD;
    public bool RootCompareReached => RootGate?.CompareReached == true;
    public uint? RootCompareResult => RootGate?.CompareResult;
    public CapacityFinalCalleeCommonTailRootGateFlags? RootCompareFlags =>
        RootGate?.CompareFlags;
    public bool RootJnzReached => RootGate?.JnzReached == true;
    public bool? RootJnzTaken => RootGate?.JnzTaken;
    public uint? RootJnzSelectedDestinationRawFileOffset =>
        RootGate?.JnzSelectedDestinationRawFileOffset;

    public bool UnusedFreshRootGlobalDWORDCaptureProvided =>
        Capture.FreshRootGlobalDWORD.HasValue && RootGate is null;

    public CapacityFinalCalleeCommonTailTagWritesFlags? TagORFlags =>
        ParentLifecycle.TagORFlags;

    public CapacityFinalCalleeCommonTailTagWritesFlags? FlagsAtStop =>
        RootGate is { CompareReached: true, CompareFlags: { } flags }
            ? Convert(flags)
            : ParentLifecycle.FlagsAtStop;

    public bool FlagsPreservedUntilRootCompare
    {
        get
        {
            CapacityFinalCalleeCommonTailRootGatePlan? candidate = RootGate;
            if (candidate is null || !candidate.CompareReached)
            {
                return FlagsAtStop == ParentLifecycle.FlagsAtStop;
            }

            CapacityFinalCalleeCommonTailRootGatePlan rootGate = candidate;
            return rootGate.CompareFlags is { } flags
                && ParentLifecycle.TagORFlags is not null
                && FlagsAtStop == Convert(flags);
        }
    }

    public bool ExactCleanCompletion
    {
        get
        {
            if (!ExactCleanParentHandoff
                || !CapacityFinalCalleeLargeRightTagRootLifecycleProjection
                    .IsExactParentRoute(ParentLifecycle))
            {
                return false;
            }

            CapacityFinalCalleeCommonTailRootGatePlan? candidate = RootGate;
            if (candidate is null
                || !candidate.ExactCleanAdmission
                || !candidate.CompareReached
                || !candidate.JnzReached)
            {
                return false;
            }

            CapacityFinalCalleeCommonTailRootGatePlan rootGate = candidate;
            uint stopAt = rootGate.StopAtRawFileOffsetExclusive;
            bool branchTaken = stopAt
                == CapacityFinalCalleeLargeRightTagRootLifecycleProjection
                    .BranchTargetRawFileOffset;
            return (stopAt
                        == CapacityFinalCalleeLargeRightTagRootLifecycleProjection
                            .BranchTargetRawFileOffset
                    || stopAt
                        == CapacityFinalCalleeLargeRightTagRootLifecycleProjection
                            .FallthroughRawFileOffset)
                && rootGate.JnzSelectedDestinationRawFileOffset == stopAt
                && rootGate.JnzTaken == branchTaken
                && rootGate.EntryESICaptured
                && rootGate.Capture.EntryESI == Capture.Parent.LateEntryESI
                && rootGate.FreshGlobalReadReached
                && rootGate.FreshGlobalCaptured
                && rootGate.FreshGlobalDWORD == Capture.FreshRootGlobalDWORD
                && !RootBranchTargetEntered
                && !RootFallthroughEntered;
        }
    }

    public bool RootBranchTargetEntered => false;
    public bool RootFallthroughEntered => false;
    public bool OpaqueEntryReached => false;

    public uint StopAtRawFileOffsetExclusive =>
        RootGate?.StopAtRawFileOffsetExclusive
        ?? ParentLifecycle.StopAtRawFileOffsetExclusive;

    private static CapacityFinalCalleeCommonTailTagWritesFlags Convert(
        CapacityFinalCalleeCommonTailRootGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
}

/// <summary>
/// Composes the exact 92C8 parent handoff with its fresh global CMP/JNZ gate,
/// without entering either selected destination.
/// </summary>
public static class CapacityFinalCalleeLargeRightTagRootLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeRightFieldTagLifecycleProjection.EntryRawFileOffset;
    public const uint RootEntryRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.EntryRawFileOffset;
    public const uint ParentBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeLargeRightFieldTagLifecycleProjection
            .FinalBoundaryRawFileOffsetExclusive;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailRootGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint FreshRootGlobalDWORDVirtualAddress =
        CapacityFinalCalleeCommonTailRootGateProjection.FreshGlobalDWORDVirtualAddress;
    public const uint BranchTargetRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset;
    public const uint FallthroughRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        CapacityFinalCalleeLargeRightFieldTagLifecycleProjection
            .CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedWindowBytes =
        CapacityFinalCalleeLargeRightFieldTagLifecycleProjection.CapturedWindowBytes
        + CapacityFinalCalleeCommonTailRootGateProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeLargeRightTagRootLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeRightTagRootLifecyclePlan Project(
        CapacityFinalCalleeLargeRightTagRootLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan parent =
            CapacityFinalCalleeLargeRightFieldTagLifecycleProjection.Project(capture.Parent);

        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRightTagRootLifecycleRead>();
        foreach (CapacityFinalCalleeLargeRightFieldThresholdLifecycleRead read in parent.Reads)
        {
            reads.Add(new(
                read.Stage
                    == CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.RightFieldGate
                    ? CapacityFinalCalleeLargeRightTagRootLifecycleReadStage.ParentRightFieldGate
                    : CapacityFinalCalleeLargeRightTagRootLifecycleReadStage.ParentThresholdGate,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value));
        }

        if (!IsExactParentRoute(parent))
        {
            bool unusedRootCapture = capture.FreshRootGlobalDWORD.HasValue;
            return new(
                capture,
                parent,
                RootGate: null,
                unusedRootCapture
                    ? CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                        .ParentStoppedWithUnusedFreshRootGlobalDWORD
                    : CapacityFinalCalleeLargeRightTagRootLifecycleOutcome.ParentStopped)
            {
                Reads = reads.ToImmutable()
            };
        }

        CapacityFinalCalleeCommonTailRootGatePlan rootGate =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: capture.Parent.LateEntryESI,
                FreshGlobalDWORD: capture.FreshRootGlobalDWORD));
        if (rootGate.FreshGlobalReadReached)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeRightTagRootLifecycleReadStage.RootGate,
                CapacityFinalCalleeCommonTailRootGateProjection.CompareRawFileOffset,
                rootGate.FreshGlobalAddress,
                rootGate.FreshGlobalAddress.HasValue,
                rootGate.FreshGlobalCaptured,
                rootGate.FreshGlobalDWORD));
        }

        CapacityFinalCalleeLargeRightTagRootLifecycleOutcome outcome =
            rootGate.Outcome switch
            {
                CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD =>
                    CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                        .MissingFreshRootGlobalDWORD,
                CapacityFinalCalleeCommonTailRootGateOutcome.EqualToFallthrough =>
                    CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                        .EqualToFallthrough92D0,
                CapacityFinalCalleeCommonTailRootGateOutcome.UnequalToBranchTarget =>
                    CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                        .UnequalToBranchTarget92EC,
                _ => throw new InvalidOperationException(
                    "The exact parent handoff produced an unexpected root-gate outcome.")
            };

        return new(capture, parent, rootGate, outcome)
        {
            Reads = reads.ToImmutable()
        };
    }

    internal static bool IsExactParentRoute(
        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan parent)
    {
        bool zeroBypass =
            parent.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
                    .FreshZeroBypass92BD
            && parent.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome.ZeroTo92BD
            && parent.ParentBranchReached
            && parent.ParentBranchTaken == true
            && parent.ParentBranchSelectedDestinationRawFileOffset
                == CapacityFinalCalleeLargeRightFieldTagLifecycleProjection.ZeroBypassRawFileOffset
            && parent.ParentStopAtRawFileOffsetExclusive
                == CapacityFinalCalleeLargeRightFieldTagLifecycleProjection
                    .ZeroBypassRawFileOffset
            && parent.ParentLifecycle.PairedStores is null
            && parent.ParentWrites.IsEmpty;

        bool notBelow =
            parent.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
                    .ThresholdNotBelowPairedStores92BD
            && parent.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && parent.ParentBranchReached
            && parent.ParentBranchTaken == false
            && parent.ParentBranchSelectedDestinationRawFileOffset
                == CapacityFinalCalleeLargeRightFieldTagLifecycleProjection
                    .PairedStoresEntryRawFileOffset
            && parent.ParentStopAtRawFileOffsetExclusive
                == CapacityFinalCalleeLargeRightFieldTagLifecycleProjection
                    .PairedStoresEntryRawFileOffset
            && parent.ParentLifecycle.PairedStores is
            {
                Outcome: CapacityFinalCalleeLargeRightPairedStoresOutcome.Completed,
                ExactCleanCompletion: true,
                FirstStoreCommitted: true,
                SecondStoreCommitted: true,
                BoundaryInstructionExecuted: false,
                StopAtRawFileOffsetExclusive:
                    CapacityFinalCalleeLargeRightPairedStoresProjection
                        .BoundaryRawFileOffsetExclusive
            }
            && parent.ParentWrites.Length == 2;

        if (!zeroBypass && !notBelow)
        {
            return false;
        }

        uint[] expectedOffsets = zeroBypass
            ? new[]
            {
                CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset,
                CapacityFinalCalleeCommonTailTagWritesProjection.SecondStoreRawFileOffset
            }
            : new[]
            {
                CapacityFinalCalleeLargeRightPairedStoresProjection.FirstStoreRawFileOffset,
                CapacityFinalCalleeLargeRightPairedStoresProjection.SecondStoreRawFileOffset,
                CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset,
                CapacityFinalCalleeCommonTailTagWritesProjection.SecondStoreRawFileOffset
            };
        CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind[] expectedKinds =
            zeroBypass
                ? new[]
                {
                    CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.TagFirstStore,
                    CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.TagSecondStore
                }
                : new[]
                {
                    CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.ParentFirstStore,
                    CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.ParentSecondStore,
                    CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.TagFirstStore,
                    CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.TagSecondStore
                };
        CapacityFinalCalleeCommonTailTagWritesPlan? commonTail = parent.CommonTail;

        return parent.ExactCleanParentHandoff
            && parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive == RootEntryRawFileOffset
            && parent.ParentLifecycle.ExactCleanCompletion
            && parent.ParentLifecycle.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeLargeRightFieldTagLifecycleProjection
                    .ChildEntryRawFileOffset
            && parent.Child92BDEntered
            && parent.ChildMOVAt92BDExecuted
            && parent.ChildORReached
            && parent.FirstTagStoreCommitted
            && parent.SecondTagStoreCommitted
            && commonTail is
            {
                Outcome: CapacityFinalCalleeCommonTailTagWritesOutcome.Completed,
                ExactCleanExitAdmitted: true,
                StopAtRawFileOffsetExclusive:
                    CapacityFinalCalleeCommonTailTagWritesProjection
                        .BoundaryRawFileOffsetExclusive
            }
            && commonTail.Writes.Length == 2
            && parent.TagWrites.Length == 2
            && parent.LateEntryESICaptureProvided
            && parent.LateEntryESICaptureReached
            && !parent.UnusedLateEntryESICaptureProvided
            && !parent.UnusedLateEntryEDICaptureProvided
            && !parent.UnusedLateEntryEAXCaptureProvided
            && !parent.Final92C8Entered
            && !parent.FinalBoundaryInstructionExecuted
            && parent.OrderedWrites.Length == expectedOffsets.Length
            && parent.OrderedWrites.Select(write => write.RawFileOffset)
                .SequenceEqual(expectedOffsets)
            && parent.OrderedWrites.Select(write => write.Kind)
                .SequenceEqual(expectedKinds);
    }

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightTagRootLifecycleInstructionEvidence>
        BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRightTagRootLifecycleInstructionEvidence>();

        foreach (CapacityFinalCalleeLargeRightFieldTagLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeLargeRightFieldTagLifecycleProjection.Instructions)
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

        foreach (CapacityFinalCalleeCommonTailRootGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonTailRootGateProjection.Instructions)
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

        return instructions.ToImmutable();
    }
}
