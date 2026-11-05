using System.Collections.Immutable;
using System.Threading;

namespace Firefall.Reconstruction.Startup;

public readonly record struct NetworkDispatcherEntryFallbackSelectorBranchCapture(
    NetworkDispatcherEntryFallbackSelectorCompareCapture ParentCapture);

public enum NetworkDispatcherEntryFallbackSelectorBranchOutcome
{
    ParentSelectorCompareNotAccepted,
    MissingRequiredCompareFlags,
    JgNotTakenToBBDE17,
    JgTakenToBBDE2B
}

public readonly record struct NetworkDispatcherEntryFallbackSelectorBranchInstruction(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    int SignedRelativeDisplacement,
    uint BranchTargetCodeVirtualAddress,
    uint BranchTargetRawFileOffset,
    uint SelectedDestinationRawFileOffset,
    bool Attempted,
    bool Reached,
    bool Committed,
    bool Taken,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter);

public sealed class NetworkDispatcherEntryFallbackSelectorBranchPlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackSelectorComparePlan> reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackSelectorBranchPlan(
        NetworkDispatcherEntryFallbackSelectorBranchCapture capture,
        NetworkDispatcherEntryFallbackSelectorComparePlan parentPlan,
        NetworkDispatcherEntryFallbackSelectorBranchOutcome outcome,
        ImmutableArray<NetworkDispatcherEntryFallbackSelectorBranchInstruction> instructions,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        bool parentSelectorCompareAccepted,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        Instructions = instructions;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        ParentSelectorCompareAccepted = parentSelectorCompareAccepted;
        ProjectionEntered = projectionEntered;
        reprojectedParentPlan = new(
            () => NetworkDispatcherEntryFallbackSelectorCompareProjection.Project(
                Capture.ParentCapture),
            LazyThreadSafetyMode.ExecutionAndPublication);
        orderedSemanticPrefixPreserved = new(
            ComputeOrderedSemanticPrefixPreserved,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactBoundedCompletion = new(
            ComputeExactBoundedCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactCleanCompletion = new(
            ComputeExactCleanCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public NetworkDispatcherEntryFallbackSelectorBranchCapture Capture { get; }
    public NetworkDispatcherEntryFallbackSelectorComparePlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackSelectorBranchOutcome Outcome { get; }
    public ImmutableArray<NetworkDispatcherEntryFallbackSelectorBranchInstruction> Instructions { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint EntryRawFileOffset =>
        NetworkDispatcherEntryFallbackSelectorBranchProjection.EntryRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public bool ParentSelectorCompareAccepted { get; }
    public bool ProjectionEntered { get; }
    public bool JgProjected => Instructions.Length == 1;
    public bool JgTaken =>
        Outcome == NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgTakenToBBDE2B;
    public bool JgNotTaken =>
        Outcome == NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgNotTakenToBBDE17;
    public uint? BranchTargetRawFileOffset =>
        ParentSelectorCompareAccepted
            ? NetworkDispatcherEntryFallbackSelectorBranchProjection.BranchTargetRawFileOffset
            : null;
    public uint? BranchTargetCodeVirtualAddress =>
        ParentSelectorCompareAccepted
            ? NetworkDispatcherEntryFallbackSelectorBranchProjection.BranchTargetCodeVirtualAddress
            : null;
    public uint? FallThroughRawFileOffset =>
        ParentSelectorCompareAccepted
            ? NetworkDispatcherEntryFallbackSelectorBranchProjection.FallThroughRawFileOffset
            : null;
    public uint? SelectedDestinationRawFileOffset =>
        JgProjected ? StopAtRawFileOffsetExclusive : null;
    public NetworkDispatcherEntryFallbackSelectorCompareRead? CompareRead =>
        ParentPlan.CompareRead;
    public uint? ComparedDword => ParentPlan.ComparedDword;
    public NetworkDispatcherLockGateFlags? CompareFlags => ParentPlan.CompareFlags;
    public NetworkDispatcherLockGateFlags? FlagsAtStop =>
        Instructions.IsEmpty ? ParentPlan.FlagsAtStop : Instructions[^1].FlagsAfter;
    public bool FlagsPreserved =>
        ParentSelectorCompareAccepted
        && ParentPlan.CompareFlags.HasValue
        && Instructions.Length == 1
        && Instructions[0].FlagsBefore == ParentPlan.CompareFlags
        && Instructions[0].FlagsAfter == ParentPlan.CompareFlags
        && FlagsAtStop == ParentPlan.CompareFlags;
    public bool BranchExecuted => false;
    public bool BranchTargetInstructionExecuted => false;
    public bool FallThroughInstructionExecuted => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ParentInstructionsExecuted => false;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion()
    {
        if (!ParentSelectorCompareAccepted
            || !JgProjected
            || ParentPlan.CompareFlags is not NetworkDispatcherLockGateFlags flags
            || flags.Zero is not bool zero
            || flags.Sign is not bool sign
            || flags.Overflow is not bool overflow)
        {
            return false;
        }

        uint expectedDestination = !zero && sign == overflow
            ? NetworkDispatcherEntryFallbackSelectorBranchProjection.BranchTargetRawFileOffset
            : NetworkDispatcherEntryFallbackSelectorBranchProjection.FallThroughRawFileOffset;
        return ProjectionEntered
            && Instructions[0].Attempted
            && Instructions[0].Reached
            && Instructions[0].Committed
            && Instructions[0].InstructionRawFileOffset == EntryRawFileOffset
            && Instructions[0].InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackSelectorBranchProjection.EntryCodeVirtualAddress
            && Instructions[0].InstructionByteLength
                == NetworkDispatcherEntryFallbackSelectorBranchProjection
                    .BranchInstructionByteLength
            && Instructions[0].InstructionBytes
                == NetworkDispatcherEntryFallbackSelectorBranchProjection
                    .BranchInstructionBytes
            && Instructions[0].SignedRelativeDisplacement
                == NetworkDispatcherEntryFallbackSelectorBranchProjection
                    .SignedRelativeDisplacement
            && Instructions[0].BranchTargetRawFileOffset
                == NetworkDispatcherEntryFallbackSelectorBranchProjection
                    .BranchTargetRawFileOffset
            && Instructions[0].BranchTargetCodeVirtualAddress
                == NetworkDispatcherEntryFallbackSelectorBranchProjection
                    .BranchTargetCodeVirtualAddress
            && Instructions[0].SelectedDestinationRawFileOffset == expectedDestination
            && Outcome == (!zero && sign == overflow
                ? NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgTakenToBBDE2B
                : NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgNotTakenToBBDE17)
            && Instructions[0].Taken == (!zero && sign == overflow)
            && Instructions[0].FlagsBefore == ParentPlan.CompareFlags
            && Instructions[0].FlagsAfter == ParentPlan.CompareFlags
            && StopAtRawFileOffsetExclusive == expectedDestination
            && NetworkDispatcherEntryFallbackSelectorBranchProjection
                    .FallThroughRawFileOffset
                == EntryRawFileOffset
                    + NetworkDispatcherEntryFallbackSelectorBranchProjection
                        .BranchInstructionByteLength
            && NetworkDispatcherEntryFallbackSelectorBranchProjection
                    .BranchTargetRawFileOffset
                == NetworkDispatcherEntryFallbackSelectorBranchProjection
                    .FallThroughRawFileOffset
                    + (uint)NetworkDispatcherEntryFallbackSelectorBranchProjection
                        .SignedRelativeDisplacement
            && CompletedInstructionOffsets.Length
                == ParentPlan.CompletedInstructionOffsets.Length + 1
            && CompletedInstructionOffsets
                .Take(ParentPlan.CompletedInstructionOffsets.Length)
                .SequenceEqual(ParentPlan.CompletedInstructionOffsets)
            && CompletedInstructionOffsets[^1] == EntryRawFileOffset
            && CompareRead == ParentPlan.CompareRead
            && ComparedDword == ParentPlan.ComparedDword
            && CompareFlags == ParentPlan.CompareFlags
            && !BranchTargetInstructionExecuted
            && !FallThroughInstructionExecuted;
    }

    private bool ComputeExactCleanCompletion() =>
        ParentSelectorCompareAccepted
        && ProjectionEntered
        && ParentPlan.ExactCleanCompletion
        && OrderedSemanticPrefixPreserved
        && ExactBoundedCompletion
        && FlagsPreserved
        && SelectedDestinationRawFileOffset.HasValue
        && !BranchExecuted
        && !BranchTargetInstructionExecuted
        && !FallThroughInstructionExecuted
        && !NativeInstructionsExecuted
        && !NativeMemoryReadsExecuted
        && !NativeMemoryWritesExecuted
        && !ParentInstructionsExecuted;

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackSelectorComparePlan actual = ParentPlan;
        NetworkDispatcherEntryFallbackSelectorComparePlan reprojected =
            reprojectedParentPlan.Value;
        return actual.Capture == reprojected.Capture
            && actual.Outcome == reprojected.Outcome
            && actual.ComparedDwordCaptureState == reprojected.ComparedDwordCaptureState
            && actual.CompareRead == reprojected.CompareRead
            && actual.CompletedInstructionOffsets.SequenceEqual(
                reprojected.CompletedInstructionOffsets)
            && actual.EntryRawFileOffset == reprojected.EntryRawFileOffset
            && actual.StopAtRawFileOffsetExclusive
                == reprojected.StopAtRawFileOffsetExclusive
            && actual.ComparedDword == reprojected.ComparedDword
            && actual.CompareFlags == reprojected.CompareFlags
            && actual.FlagsAtStop == reprojected.FlagsAtStop
            && actual.CompareReached == reprojected.CompareReached
            && actual.ParentZeroBranchAccepted == reprojected.ParentZeroBranchAccepted
            && actual.ProjectionEntered == reprojected.ProjectionEntered
            && actual.NextBranchProjected == reprojected.NextBranchProjected
            && actual.NextBranchExecuted == reprojected.NextBranchExecuted
            && actual.ExactBoundedCompletion == reprojected.ExactBoundedCompletion
            && actual.ExactCleanCompletion == reprojected.ExactCleanCompletion
            && actual.OrderedSemanticPrefixPreserved
            && actual.ParentPlan.Capture == reprojected.ParentPlan.Capture
            && actual.ParentPlan.Outcome == reprojected.ParentPlan.Outcome
            && actual.ParentPlan.Instructions.SequenceEqual(
                reprojected.ParentPlan.Instructions)
            && actual.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojected.ParentPlan.CompletedInstructionOffsets)
            && actual.ParentPlan.FlagsAtStop == reprojected.ParentPlan.FlagsAtStop
            && actual.ParentPlan.ParentPlan.Capture
                == reprojected.ParentPlan.ParentPlan.Capture
            && actual.ParentPlan.ParentPlan.Outcome
                == reprojected.ParentPlan.ParentPlan.Outcome
            && actual.ParentPlan.ParentPlan.Stores.SequenceEqual(
                reprojected.ParentPlan.ParentPlan.Stores)
            && actual.ParentPlan.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojected.ParentPlan.ParentPlan.CompletedInstructionOffsets)
            && actual.ParentPlan.ParentPlan.FlagsAtStop
                == reprojected.ParentPlan.ParentPlan.FlagsAtStop
            && actual.ParentPlan.ParentPlan.ParentPlan.ArgumentRead
                == reprojected.ParentPlan.ParentPlan.ParentPlan.ArgumentRead
            && actual.ParentPlan.ParentPlan.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojected.ParentPlan.ParentPlan.ParentPlan.CompletedInstructionOffsets)
            && actual.ParentPlan.OrderedSemanticPrefixPreserved
            && actual.ParentPlan.ParentPlan.OrderedSemanticPrefixPreserved
            && actual.ParentPlan.ParentPlan.ParentPlan.ExactCleanCompletion
                == reprojected.ParentPlan.ParentPlan.ParentPlan.ExactCleanCompletion;
    }
}

/// <summary>
/// Reprojects the signed-greater JG at BBDE15 and stops before its selected
/// destination. The branch consumes the preserved CMP flags without executing
/// either destination instruction.
/// </summary>
public static class NetworkDispatcherEntryFallbackSelectorBranchProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE15;
    public const uint FallThroughRawFileOffset = 0x00BBDE17;
    public const uint BranchTargetRawFileOffset = 0x00BBDE2B;
    public const byte BranchInstructionByteLength = 2;
    public const int SignedRelativeDisplacement = 0x14;
    public const string BranchInstructionBytes = "7F14";
    public const bool BytesIndependentlyVerified = true;

    public static uint EntryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(EntryRawFileOffset);
    public static uint FallThroughCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(FallThroughRawFileOffset);
    public static uint BranchTargetCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(BranchTargetRawFileOffset);
    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackSelectorBranchPlan Project(
        NetworkDispatcherEntryFallbackSelectorBranchCapture capture)
    {
        NetworkDispatcherEntryFallbackSelectorComparePlan parent =
            NetworkDispatcherEntryFallbackSelectorCompareProjection.Project(
                capture.ParentCapture);
        bool parentSelectorCompareAccepted =
            parent.ExactCleanCompletion
            && parent.Outcome
                == NetworkDispatcherEntryFallbackSelectorCompareOutcome.Compared
            && parent.ParentZeroBranchAccepted
            && parent.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackZeroBranchOutcome.JzNotTakenToBBDE0E
            && parent.CompareReached
            && parent.ComparedDwordCaptureState
                == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.Captured
            && parent.CompareRead is {
                InstructionRawFileOffset: 0x00BBDE0E,
                OperandVirtualAddress: 0x01E68590,
                Width: 4,
                Attempted: true,
                Captured: true,
                Committed: false
            }
            && parent.CompareRead.Value.Value == parent.ComparedDword
            && parent.ComparedDword == capture.ParentCapture.FreshComparedDword
            && parent.CompareFlags.HasValue
            && parent.FlagsAtStop == parent.CompareFlags
            && parent.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackSelectorCompareProjection
                    .BoundaryRawFileOffsetExclusive
            && parent.CompletedInstructionOffsets.LastOrDefault()
                == NetworkDispatcherEntryFallbackSelectorCompareProjection.CompareRawFileOffset;

        if (!parentSelectorCompareAccepted)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackSelectorBranchOutcome
                    .ParentSelectorCompareNotAccepted,
                ImmutableArray<NetworkDispatcherEntryFallbackSelectorBranchInstruction>.Empty,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive,
                parentSelectorCompareAccepted: false,
                projectionEntered: false);
        }

        if (parent.CompareFlags is not NetworkDispatcherLockGateFlags flags
            || flags.Zero is not bool zero
            || flags.Sign is not bool sign
            || flags.Overflow is not bool overflow)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackSelectorBranchOutcome
                    .MissingRequiredCompareFlags,
                ImmutableArray<NetworkDispatcherEntryFallbackSelectorBranchInstruction>.Empty,
                parent.CompletedInstructionOffsets,
                EntryRawFileOffset,
                parentSelectorCompareAccepted: true,
                projectionEntered: true);
        }

        bool taken = !zero && sign == overflow;
        uint selectedDestination = taken ? BranchTargetRawFileOffset : FallThroughRawFileOffset;
        var branch = new NetworkDispatcherEntryFallbackSelectorBranchInstruction(
            EntryRawFileOffset,
            EntryCodeVirtualAddress,
            BranchInstructionByteLength,
            BranchInstructionBytes,
            SignedRelativeDisplacement,
            BranchTargetCodeVirtualAddress,
            BranchTargetRawFileOffset,
            selectedDestination,
            Attempted: true,
            Reached: true,
            Committed: true,
            Taken: taken,
            FlagsBefore: flags,
            FlagsAfter: flags);
        return new(
            capture,
            parent,
            taken
                ? NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgTakenToBBDE2B
                : NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgNotTakenToBBDE17,
            ImmutableArray.Create(branch),
            parent.CompletedInstructionOffsets.Add(EntryRawFileOffset),
            selectedDestination,
            parentSelectorCompareAccepted: true,
            projectionEntered: true);
    }

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta);
}
