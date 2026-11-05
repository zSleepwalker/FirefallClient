using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct NetworkDispatcherEntryFallbackZeroBranchCapture(
    NetworkDispatcherEntryFallbackClearStoresCapture ParentCapture);

public enum NetworkDispatcherEntryFallbackZeroBranchOutcome
{
    ParentClearStoresNotAccepted,
    MissingPreservedZeroFlag,
    JzTakenToBBDECA,
    JzNotTakenToBBDE0E
}

public readonly record struct NetworkDispatcherEntryFallbackZeroBranchInstruction(
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
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter);

public sealed class NetworkDispatcherEntryFallbackZeroBranchPlan
{
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackZeroBranchPlan(
        NetworkDispatcherEntryFallbackZeroBranchCapture capture,
        NetworkDispatcherEntryFallbackClearStoresPlan parentPlan,
        NetworkDispatcherEntryFallbackZeroBranchOutcome outcome,
        ImmutableArray<NetworkDispatcherEntryFallbackZeroBranchInstruction> instructions,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        bool parentClearStoresAccepted,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        Instructions = instructions;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        ParentClearStoresAccepted = parentClearStoresAccepted;
        ProjectionEntered = projectionEntered;
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactBoundedCompletion = new(ComputeExactBoundedCompletion);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public NetworkDispatcherEntryFallbackZeroBranchCapture Capture { get; }
    public NetworkDispatcherEntryFallbackClearStoresPlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackZeroBranchOutcome Outcome { get; }
    public ImmutableArray<NetworkDispatcherEntryFallbackZeroBranchInstruction> Instructions { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint EntryRawFileOffset =>
        NetworkDispatcherEntryFallbackZeroBranchProjection.EntryRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public bool ParentClearStoresAccepted { get; }
    public bool ProjectionEntered { get; }
    public bool JzProjected => Instructions.Length == 1;
    public bool JzTaken =>
        Outcome == NetworkDispatcherEntryFallbackZeroBranchOutcome.JzTakenToBBDECA;
    public bool JzNotTaken =>
        Outcome == NetworkDispatcherEntryFallbackZeroBranchOutcome.JzNotTakenToBBDE0E;
    public bool BranchExecuted => false;
    public bool BranchTargetInstructionExecuted => false;
    public bool FallThroughInstructionExecuted => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ParentInstructionsExecuted => false;
    public NetworkDispatcherLockGateFlags? FlagsAtStop =>
        Instructions.IsEmpty ? ParentPlan.FlagsAtStop : Instructions[^1].FlagsAfter;
    public uint? BranchTargetRawFileOffset =>
        ParentClearStoresAccepted
            ? NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetRawFileOffset
            : null;
    public uint? BranchTargetCodeVirtualAddress =>
        ParentClearStoresAccepted
            ? NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetCodeVirtualAddress
            : null;
    public uint? SelectedDestinationRawFileOffset =>
        JzProjected ? StopAtRawFileOffsetExclusive : null;
    public bool FlagsPreserved =>
        ParentClearStoresAccepted
        && ParentPlan.FlagsAtStop.HasValue
        && Instructions.Length == 1
        && Instructions[0].FlagsBefore == ParentPlan.FlagsAtStop
        && Instructions[0].FlagsAfter == ParentPlan.FlagsAtStop
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && ParentPlan.FlagsAtStop == ParentPlan.ParentPlan.CompareFlags;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion()
    {
        if (!ParentClearStoresAccepted)
        {
            return false;
        }

        if (!JzProjected)
        {
            return false;
        }

        uint expectedDestination = ParentPlan.FlagsAtStop?.Zero == true
            ? NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetRawFileOffset
            : NetworkDispatcherEntryFallbackZeroBranchProjection.FallThroughRawFileOffset;
        return ProjectionEntered
            && Instructions[0].Attempted
            && Instructions[0].Reached
            && Instructions[0].Committed
            && Instructions[0].InstructionRawFileOffset == EntryRawFileOffset
            && Instructions[0].InstructionCodeVirtualAddress
                == NetworkDispatcherEntryFallbackZeroBranchProjection.EntryCodeVirtualAddress
            && Instructions[0].InstructionByteLength
                == NetworkDispatcherEntryFallbackZeroBranchProjection.BranchInstructionByteLength
            && Instructions[0].InstructionBytes
                == NetworkDispatcherEntryFallbackZeroBranchProjection.BranchInstructionBytes
            && Instructions[0].SignedRelativeDisplacement
                == NetworkDispatcherEntryFallbackZeroBranchProjection.SignedRelativeDisplacement
            && Instructions[0].BranchTargetRawFileOffset
                == NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetRawFileOffset
            && Instructions[0].BranchTargetCodeVirtualAddress
                == NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetCodeVirtualAddress
            && Instructions[0].SelectedDestinationRawFileOffset == expectedDestination
            && StopAtRawFileOffsetExclusive == expectedDestination
            && Instructions[0].BranchTargetRawFileOffset
                == NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetRawFileOffset
            && Instructions[0].FlagsBefore == ParentPlan.FlagsAtStop
            && Instructions[0].FlagsAfter == ParentPlan.FlagsAtStop
            && NetworkDispatcherEntryFallbackZeroBranchProjection.FallThroughRawFileOffset
                == EntryRawFileOffset
                    + NetworkDispatcherEntryFallbackZeroBranchProjection
                        .BranchInstructionByteLength
            && CompletedInstructionOffsets.Length
                == ParentPlan.CompletedInstructionOffsets.Length + 1
            && CompletedInstructionOffsets
                .Take(ParentPlan.CompletedInstructionOffsets.Length)
                .SequenceEqual(ParentPlan.CompletedInstructionOffsets)
            && CompletedInstructionOffsets[^1] == EntryRawFileOffset
            && !BranchTargetInstructionExecuted
            && !FallThroughInstructionExecuted;
    }

    private bool ComputeExactCleanCompletion() =>
        ParentClearStoresAccepted
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
        && !NativeMemoryWritesExecuted;

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackClearStoresPlan actual = ParentPlan;
        NetworkDispatcherEntryFallbackClearStoresPlan reprojected =
            NetworkDispatcherEntryFallbackClearStoresProjection.Project(Capture.ParentCapture);
        return actual.Capture == reprojected.Capture
            && actual.Outcome == reprojected.Outcome
            && actual.FreshPostFallbackESICaptureState
                == reprojected.FreshPostFallbackESICaptureState
            && actual.Stores.SequenceEqual(reprojected.Stores)
            && actual.CompletedInstructionOffsets.SequenceEqual(
                reprojected.CompletedInstructionOffsets)
            && actual.EntryRawFileOffset == reprojected.EntryRawFileOffset
            && actual.StopAtRawFileOffsetExclusive
                == reprojected.StopAtRawFileOffsetExclusive
            && actual.FreshParentEBP == reprojected.FreshParentEBP
            && actual.FreshPostFallbackESI == reprojected.FreshPostFallbackESI
            && actual.FrameAddress == reprojected.FrameAddress
            && actual.ReceiverAddress == reprojected.ReceiverAddress
            && actual.FlagsAtStop == reprojected.FlagsAtStop
            && actual.BranchProjected == reprojected.BranchProjected
            && actual.ExactBoundedCompletion == reprojected.ExactBoundedCompletion
            && actual.ExactCleanCompletion == reprojected.ExactCleanCompletion
            && actual.ProjectionEntered == reprojected.ProjectionEntered
            && actual.OrderedSemanticPrefixPreserved
            && CompletedInstructionOffsets
                .Take(actual.CompletedInstructionOffsets.Length)
                .SequenceEqual(actual.CompletedInstructionOffsets)
            && actual.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojected.ParentPlan.CompletedInstructionOffsets)
            && actual.ParentPlan.ArgumentRead == reprojected.ParentPlan.ArgumentRead
            && actual.ParentPlan.CompareFlags == reprojected.ParentPlan.CompareFlags
            && actual.ParentPlan.FlagsAtStop == reprojected.ParentPlan.FlagsAtStop
            && actual.ParentPlan.ExactCleanCompletion
                == reprojected.ParentPlan.ExactCleanCompletion;
    }
}

/// <summary>
/// Reprojects the preserved-CMP JZ at BBDE08 and stops before its selected
/// destination. IDA raw EAs match file offsets here; the PE .text mapping is
/// raw offset + 400C00. The target and fall-through instructions are not run.
/// </summary>
public static class NetworkDispatcherEntryFallbackZeroBranchProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE08;
    public const uint FallThroughRawFileOffset = 0x00BBDE0E;
    public const byte BranchInstructionByteLength = 6;
    public const int SignedRelativeDisplacement = 0xBC;
    public const string BranchInstructionBytes = "0F84BC000000";
    public const string FallThroughInstructionBytes = "833D9085E60101";
    public const byte FallThroughInstructionByteLength = 7;
    public const string BranchTargetInstructionBytes = "8B86A8000000";
    public const byte BranchTargetInstructionByteLength = 6;
    public const string CapturedBranchAndFallThroughBytes =
        "0F84BC000000833D9085E60101";
    public const bool BytesIndependentlyVerified = true;

    public static uint EntryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(EntryRawFileOffset);
    public static uint FallThroughCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(FallThroughRawFileOffset);
    public static uint BranchTargetCodeVirtualAddress =>
        CalculateRelativeTargetVirtualAddress(EntryCodeVirtualAddress, SignedRelativeDisplacement);
    public static uint BranchTargetRawFileOffset =>
        CodeVirtualAddressToRawFileOffset(BranchTargetCodeVirtualAddress);
    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackZeroBranchPlan Project(
        NetworkDispatcherEntryFallbackZeroBranchCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture.ParentCapture.ParentCapture);

        NetworkDispatcherEntryFallbackClearStoresPlan parent =
            NetworkDispatcherEntryFallbackClearStoresProjection.Project(capture.ParentCapture);
        bool parentClearStoresAccepted =
            parent.ExactCleanCompletion
            && parent.Outcome
                == NetworkDispatcherEntryFallbackClearStoresOutcome.CompletedBeforeBranch
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.ParentPlan.CompareReached
            && parent.ParentPlan.CompareFlags.HasValue
            && parent.FlagsAtStop == parent.ParentPlan.CompareFlags
            && parent.FrameDwordClearProjected
            && parent.ReceiverByteClearProjected
            && !parent.BranchProjected;

        if (!parentClearStoresAccepted)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackZeroBranchOutcome.ParentClearStoresNotAccepted,
                ImmutableArray<NetworkDispatcherEntryFallbackZeroBranchInstruction>.Empty,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive ?? EntryRawFileOffset,
                parentClearStoresAccepted: false,
                projectionEntered: false);
        }

        if (parent.FlagsAtStop is not NetworkDispatcherLockGateFlags flags
            || flags.Zero is null)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackZeroBranchOutcome.MissingPreservedZeroFlag,
                ImmutableArray<NetworkDispatcherEntryFallbackZeroBranchInstruction>.Empty,
                parent.CompletedInstructionOffsets,
                EntryRawFileOffset,
                parentClearStoresAccepted: true,
                projectionEntered: false);
        }

        bool taken = flags.Zero == true;
        uint selectedDestination = taken ? BranchTargetRawFileOffset : FallThroughRawFileOffset;
        var branch = new NetworkDispatcherEntryFallbackZeroBranchInstruction(
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
            FlagsBefore: flags,
            FlagsAfter: flags);
        return new(
            capture,
            parent,
            taken
                ? NetworkDispatcherEntryFallbackZeroBranchOutcome.JzTakenToBBDECA
                : NetworkDispatcherEntryFallbackZeroBranchOutcome.JzNotTakenToBBDE0E,
            ImmutableArray.Create(branch),
            parent.CompletedInstructionOffsets.Add(EntryRawFileOffset),
            selectedDestination,
            parentClearStoresAccepted: true,
            projectionEntered: true);
    }

    public static uint CalculateRelativeTargetVirtualAddress(
        uint branchInstructionCodeVirtualAddress,
        int signedRelativeDisplacement) =>
        unchecked(
            branchInstructionCodeVirtualAddress
            + BranchInstructionByteLength
            + (uint)signedRelativeDisplacement);

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta);

    public static uint CodeVirtualAddressToRawFileOffset(uint codeVirtualAddress) =>
        unchecked(codeVirtualAddress - NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta);
}
