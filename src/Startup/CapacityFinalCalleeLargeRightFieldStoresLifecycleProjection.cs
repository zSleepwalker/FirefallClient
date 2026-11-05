using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightFieldStoresLifecycleCapture(
    uint? EntryEDX,
    uint? FreshEDXPlus14,
    uint? FreshGlobalValue,
    uint? LateEntryEAX);

public enum CapacityFinalCalleeLargeRightFieldStoresLifecycleOutcome
{
    ParentStopped,
    ParentStoppedWithUnusedLateEntryEAX,
    MissingLateEntryEAXAtFirstStore,
    StoresCompleted
}

public readonly record struct CapacityFinalCalleeLargeRightFieldStoresLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan(
    CapacityFinalCalleeLargeRightFieldStoresLifecycleCapture Capture,
    CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan ParentLifecycle,
    CapacityFinalCalleeLargeRightPairedStoresPlan? PairedStores,
    CapacityFinalCalleeLargeRightFieldStoresLifecycleOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeRightFieldThresholdLifecycleRead> Reads =>
        ParentLifecycle.Reads;

    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome ParentOutcome =>
        ParentLifecycle.Outcome;

    public bool ParentBranchReached => ParentLifecycle.BranchReached;
    public bool? ParentBranchTaken => ParentLifecycle.BranchTaken;
    public uint? ParentBranchSelectedDestinationRawFileOffset =>
        ParentLifecycle.BranchSelectedDestinationRawFileOffset;
    public uint ParentStopAtRawFileOffsetExclusive =>
        ParentLifecycle.StopAtRawFileOffsetExclusive;
    public bool ParentThresholdGateReached => ParentLifecycle.ThresholdGateReached;
    public bool UnusedFreshGlobalValueCaptureProvided =>
        ParentLifecycle.UnusedFreshGlobalValueCaptureProvided;
    public bool ExactCleanParentExitAdmitted => ParentLifecycle.ExactCleanExitAdmitted;

    public bool LateEntryEAXCaptureProvided => Capture.LateEntryEAX.HasValue;
    public bool LateEntryEAXCaptureReached => PairedStores is not null;
    public bool UnusedLateEntryEAXCaptureProvided =>
        LateEntryEAXCaptureProvided && !LateEntryEAXCaptureReached;

    public bool Child92B7Entered => PairedStores is not null;
    public uint? ChildEntryECX => PairedStores?.Capture.EntryECX;
    public uint? ChildEntryEAX => PairedStores?.Capture.EntryEAX;
    public bool ChildEntryEAXCaptured => PairedStores?.EntryEAXCaptured ?? false;
    public bool Final92BDEntered => false;
    public bool FirstStoreAttempted => PairedStores?.FirstStoreReached == true;
    public bool FirstStoreCommitted => PairedStores?.FirstStoreCommitted == true;
    public bool SecondStoreAttempted => PairedStores?.SecondStoreReached == true;
    public bool SecondStoreCommitted => PairedStores?.SecondStoreCommitted == true;
    public ImmutableArray<CapacityFinalCalleeLargeRightPairedStoresWrite> Writes =>
        PairedStores?.Writes
        ?? ImmutableArray<CapacityFinalCalleeLargeRightPairedStoresWrite>.Empty;

    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? ParentFlagsAtStop =>
        ParentLifecycle.FlagsAtStop;
    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? FlagsAtStop =>
        ParentLifecycle.FlagsAtStop;
    public CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? ParentCompareFlags =>
        ParentLifecycle.ThresholdCompareFlags;
    public bool StoresPreserveParentFlags => PairedStores?.FlagsUnaffected ?? true;

    /// <summary>
    /// Defined EFLAGS bits carried into the two MOVs. Other incoming flag bits are unknown.
    /// </summary>
    public uint? ChildIncomingEFlagsDefinedBits => PairedStores?.IncomingEFlags;

    /// <summary>
    /// The MOVs preserve the known parent CMP flags; this is not a reconstruction of unknown bits.
    /// </summary>
    public uint? EFlagsAtStopDefinedBits => PairedStores?.EFlagsAtStop;

    public bool ExactCleanCompletion
    {
        get
        {
            if (!ParentLifecycle.ExactCleanExitAdmitted
                || UnusedLateEntryEAXCaptureProvided)
            {
                return false;
            }

            return PairedStores is null
                || (PairedStores.ExactCleanCompletion
                    && PairedStores.FirstStoreCommitted
                    && PairedStores.SecondStoreCommitted
                    && PairedStores.Writes.Length == 2
                    && PairedStores.StopAtRawFileOffsetExclusive
                        == CapacityFinalCalleeLargeRightPairedStoresProjection.BoundaryRawFileOffsetExclusive
                    && !PairedStores.BoundaryInstructionExecuted);
        }
    }

    public uint StopAtRawFileOffsetExclusive =>
        PairedStores?.StopAtRawFileOffsetExclusive
        ?? ParentLifecycle.StopAtRawFileOffsetExclusive;
}

/// <summary>
/// Composes the fresh right-field tests and unsigned threshold branch with the
/// fallthrough-only paired stores at 92B7.
/// </summary>
public static class CapacityFinalCalleeLargeRightFieldStoresLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection.EntryVirtualAddress;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeLargeRightPairedStoresProjection.EntryRawFileOffset;
    public const uint FinalBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeLargeRightPairedStoresProjection.BoundaryRawFileOffsetExclusive;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection
            .CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DefinedEFlagsBitsMask = 0x000008D7;

    public const string CapturedWindowBytes =
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection.CapturedWindowBytes
        + CapacityFinalCalleeLargeRightPairedStoresProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeLargeRightFieldStoresLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan Project(
        CapacityFinalCalleeLargeRightFieldStoresLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan parent =
            CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection.Project(new(
                capture.EntryEDX,
                capture.FreshEDXPlus14,
                capture.FreshGlobalValue));

        bool childAdmission = parent.Outcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && parent.ExactCleanBranchToThresholdGate
            && parent.ExactCleanExitAdmitted
            && parent.BranchReached
            && parent.BranchTaken == false
            && parent.BranchSelectedDestinationRawFileOffset
                == ChildEntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
            && parent.ThresholdGate is
            {
                CompareReached: true,
                BranchReached: true,
                BranchTaken: false,
                BranchSelectedDestinationRawFileOffset: ChildEntryRawFileOffset
            }
            && parent.RightFieldGate.FreshECX.HasValue
            && parent.ThresholdCompareFlags is not null;

        if (!childAdmission)
        {
            bool unusedLateEAX = capture.LateEntryEAX.HasValue;
            return new(
                capture,
                parent,
                PairedStores: null,
                unusedLateEAX
                    ? CapacityFinalCalleeLargeRightFieldStoresLifecycleOutcome
                        .ParentStoppedWithUnusedLateEntryEAX
                    : CapacityFinalCalleeLargeRightFieldStoresLifecycleOutcome.ParentStopped);
        }

        CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags flags =
            parent.ThresholdCompareFlags!.Value;
        uint knownEFlags = EncodeDefinedEFlagsBits(flags);
        CapacityFinalCalleeLargeRightPairedStoresPlan stores =
            CapacityFinalCalleeLargeRightPairedStoresProjection.Project(new(
                EntryEAX: capture.LateEntryEAX,
                EntryECX: parent.RightFieldGate.FreshECX,
                IncomingEFlags: knownEFlags));

        return new(
            capture,
            parent,
            stores,
            stores.Outcome
                == CapacityFinalCalleeLargeRightPairedStoresOutcome.Completed
                ? CapacityFinalCalleeLargeRightFieldStoresLifecycleOutcome.StoresCompleted
                : CapacityFinalCalleeLargeRightFieldStoresLifecycleOutcome
                    .MissingLateEntryEAXAtFirstStore);
    }

    private static uint EncodeDefinedEFlagsBits(
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags flags)
    {
        if (flags.AuxiliaryCarry is not bool auxiliaryCarry)
        {
            throw new InvalidOperationException(
                "The child stores require the parent's fully defined CMP flags.");
        }

        return 0x2u
            | (flags.Carry ? 1u << 0 : 0)
            | (flags.Parity ? 1u << 2 : 0)
            | (auxiliaryCarry ? 1u << 4 : 0)
            | (flags.Zero ? 1u << 6 : 0)
            | (flags.Sign ? 1u << 7 : 0)
            | (flags.Overflow ? 1u << 11 : 0);
    }

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightFieldStoresLifecycleInstructionEvidence>
        BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRightFieldStoresLifecycleInstructionEvidence>();

        foreach (CapacityFinalCalleeLargeRightFieldThresholdLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection.Instructions)
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

        foreach (CapacityFinalCalleeLargeRightPairedStoresInstructionEvidence instruction
            in CapacityFinalCalleeLargeRightPairedStoresProjection.Instructions)
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
