using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackSelectorCompareCapture(
    NetworkDispatcherEntryFallbackZeroBranchCapture ParentCapture,
    uint? FreshComparedDword);

public enum NetworkDispatcherEntryFallbackSelectorCompareCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackSelectorCompareOutcome
{
    ParentFallbackZeroBranchNotAccepted,
    MissingFreshComparedDword,
    Compared
}

public readonly record struct NetworkDispatcherEntryFallbackSelectorCompareRead(
    uint InstructionRawFileOffset,
    uint OperandVirtualAddress,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public sealed class NetworkDispatcherEntryFallbackSelectorComparePlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackZeroBranchPlan> reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackSelectorComparePlan(
        NetworkDispatcherEntryFallbackSelectorCompareCapture capture,
        NetworkDispatcherEntryFallbackZeroBranchPlan parentPlan,
        NetworkDispatcherEntryFallbackSelectorCompareOutcome outcome,
        NetworkDispatcherEntryFallbackSelectorCompareCaptureState comparedDwordCaptureState,
        NetworkDispatcherEntryFallbackSelectorCompareRead? compareRead,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        uint? comparedDword,
        NetworkDispatcherLockGateFlags? compareFlags,
        NetworkDispatcherLockGateFlags? flagsAtStop,
        bool compareReached,
        bool parentZeroBranchAccepted,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        ComparedDwordCaptureState = comparedDwordCaptureState;
        CompareRead = compareRead;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        ComparedDword = comparedDword;
        CompareFlags = compareFlags;
        FlagsAtStop = flagsAtStop;
        CompareReached = compareReached;
        ParentZeroBranchAccepted = parentZeroBranchAccepted;
        ProjectionEntered = projectionEntered;
        reprojectedParentPlan = new(() =>
            NetworkDispatcherEntryFallbackZeroBranchProjection.Project(Capture.ParentCapture));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactBoundedCompletion = new(ComputeExactBoundedCompletion);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public NetworkDispatcherEntryFallbackSelectorCompareCapture Capture { get; }
    public NetworkDispatcherEntryFallbackZeroBranchPlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackSelectorCompareOutcome Outcome { get; }
    public NetworkDispatcherEntryFallbackSelectorCompareCaptureState ComparedDwordCaptureState { get; }
    public NetworkDispatcherEntryFallbackSelectorCompareRead? CompareRead { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint EntryRawFileOffset =>
        NetworkDispatcherEntryFallbackSelectorCompareProjection.EntryRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public uint? ComparedDword { get; }
    public NetworkDispatcherLockGateFlags? CompareFlags { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop { get; }
    public bool CompareReached { get; }
    public bool CompareProjected => CompareReached;
    public bool ParentZeroBranchAccepted { get; }
    public bool ProjectionEntered { get; }
    public bool NextBranchProjected => false;
    public bool NextBranchExecuted => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion()
    {
        if (!ParentZeroBranchAccepted || !CompareReached || CompareRead is not { } read)
        {
            return false;
        }

        return ProjectionEntered
            && ParentPlan.ExactCleanCompletion
            && ParentPlan.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackZeroBranchOutcome.JzNotTakenToBBDE0E
            && ParentPlan.FlagsAtStop is { Zero: false }
            && read.InstructionRawFileOffset
                == NetworkDispatcherEntryFallbackSelectorCompareProjection.CompareRawFileOffset
            && read.OperandVirtualAddress
                == NetworkDispatcherEntryFallbackSelectorCompareProjection
                    .ComparedMemoryOperandVirtualAddress
            && read.Width
                == NetworkDispatcherEntryFallbackSelectorCompareProjection.ComparedDwordWidth
            && read.Attempted
            && read.Captured
            && read.Value == ComparedDword
            && ComparedDword == Capture.FreshComparedDword
            && !read.Committed
            && ComparedDword.HasValue
            && CompareFlags.HasValue
            && FlagsAtStop == CompareFlags
            && StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackSelectorCompareProjection
                    .BoundaryRawFileOffsetExclusive
            && CompletedInstructionOffsets.Length
                == ParentPlan.CompletedInstructionOffsets.Length + 1
            && CompletedInstructionOffsets
                .Take(ParentPlan.CompletedInstructionOffsets.Length)
                .SequenceEqual(ParentPlan.CompletedInstructionOffsets)
            && CompletedInstructionOffsets[^1]
                == NetworkDispatcherEntryFallbackSelectorCompareProjection.CompareRawFileOffset
            && NetworkDispatcherEntryFallbackSelectorCompareProjection
                    .BoundaryRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackSelectorCompareProjection.CompareRawFileOffset
                    + NetworkDispatcherEntryFallbackSelectorCompareProjection
                        .CompareInstructionByteLength
            && !NextBranchProjected
            && !NextBranchExecuted;
    }

    private bool ComputeExactCleanCompletion() =>
        ParentZeroBranchAccepted
        && ProjectionEntered
        && ParentPlan.ExactCleanCompletion
        && OrderedSemanticPrefixPreserved
        && ExactBoundedCompletion
        && ComparedDwordCaptureState == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.Captured
        && !NextBranchProjected
        && !NextBranchExecuted
        && !NativeInstructionsExecuted
        && !NativeMemoryReadsExecuted
        && !NativeMemoryWritesExecuted;

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackZeroBranchPlan actual = ParentPlan;
        NetworkDispatcherEntryFallbackZeroBranchPlan reprojected = reprojectedParentPlan.Value;
        return actual.Capture == reprojected.Capture
            && actual.Outcome == reprojected.Outcome
            && actual.Instructions.SequenceEqual(reprojected.Instructions)
            && actual.CompletedInstructionOffsets.SequenceEqual(
                reprojected.CompletedInstructionOffsets)
            && actual.EntryRawFileOffset == reprojected.EntryRawFileOffset
            && actual.StopAtRawFileOffsetExclusive
                == reprojected.StopAtRawFileOffsetExclusive
            && actual.ParentClearStoresAccepted == reprojected.ParentClearStoresAccepted
            && actual.ProjectionEntered == reprojected.ProjectionEntered
            && actual.JzProjected == reprojected.JzProjected
            && actual.JzTaken == reprojected.JzTaken
            && actual.JzNotTaken == reprojected.JzNotTaken
            && actual.BranchTargetRawFileOffset == reprojected.BranchTargetRawFileOffset
            && actual.BranchTargetCodeVirtualAddress
                == reprojected.BranchTargetCodeVirtualAddress
            && actual.SelectedDestinationRawFileOffset
                == reprojected.SelectedDestinationRawFileOffset
            && actual.FlagsAtStop == reprojected.FlagsAtStop
            && actual.FlagsPreserved == reprojected.FlagsPreserved
            && actual.ExactBoundedCompletion == reprojected.ExactBoundedCompletion
            && actual.ExactCleanCompletion == reprojected.ExactCleanCompletion
            && actual.OrderedSemanticPrefixPreserved
            && actual.ParentPlan.Capture == reprojected.ParentPlan.Capture
            && actual.ParentPlan.Outcome == reprojected.ParentPlan.Outcome
            && actual.ParentPlan.Stores.SequenceEqual(reprojected.ParentPlan.Stores)
            && actual.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojected.ParentPlan.CompletedInstructionOffsets)
            && actual.ParentPlan.FreshParentEBP == reprojected.ParentPlan.FreshParentEBP
            && actual.ParentPlan.FreshPostFallbackESI
                == reprojected.ParentPlan.FreshPostFallbackESI
            && actual.ParentPlan.FrameAddress == reprojected.ParentPlan.FrameAddress
            && actual.ParentPlan.ReceiverAddress == reprojected.ParentPlan.ReceiverAddress
            && actual.ParentPlan.FlagsAtStop == reprojected.ParentPlan.FlagsAtStop
            && actual.ParentPlan.StopAtRawFileOffsetExclusive
                == reprojected.ParentPlan.StopAtRawFileOffsetExclusive
            && actual.ParentPlan.ExactBoundedCompletion
                == reprojected.ParentPlan.ExactBoundedCompletion
            && actual.ParentPlan.ExactCleanCompletion
                == reprojected.ParentPlan.ExactCleanCompletion
            && actual.ParentPlan.OrderedSemanticPrefixPreserved
            && actual.ParentPlan.ParentPlan.Outcome
                == reprojected.ParentPlan.ParentPlan.Outcome
            && actual.ParentPlan.ParentPlan.ArgumentRead
                == reprojected.ParentPlan.ParentPlan.ArgumentRead
            && actual.ParentPlan.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojected.ParentPlan.ParentPlan.CompletedInstructionOffsets)
            && actual.ParentPlan.ParentPlan.CompareFlags
                == reprojected.ParentPlan.ParentPlan.CompareFlags
            && actual.ParentPlan.ParentPlan.FlagsAtStop
                == reprojected.ParentPlan.ParentPlan.FlagsAtStop
            && actual.ParentPlan.ParentPlan.ExactCleanCompletion
                == reprojected.ParentPlan.ParentPlan.ExactCleanCompletion
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.Outcome
                == reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan.Outcome
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.CounterAccesses
                .SequenceEqual(
                    reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan.CounterAccesses)
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.CompletedInstructionOffsets
                .SequenceEqual(
                    reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan
                        .CompletedInstructionOffsets)
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.FlagsAtStop
                == reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan.FlagsAtStop
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.ExactCleanCompletion
                == reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan.ExactCleanCompletion
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.ParentFallbackImportReturnPlan
                .Outcome
                == reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan
                    .ParentFallbackImportReturnPlan.Outcome
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.ParentFallbackImportReturnPlan
                .TargetRead
                == reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan
                    .ParentFallbackImportReturnPlan.TargetRead
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.ParentFallbackImportReturnPlan
                .Call
                == reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan
                    .ParentFallbackImportReturnPlan.Call
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.ParentFallbackImportReturnPlan
                .CompletedInstructionOffsets.SequenceEqual(
                    reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan
                        .ParentFallbackImportReturnPlan.CompletedInstructionOffsets)
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.ParentFallbackImportReturnPlan
                .FlagsAtStop
                == reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan
                    .ParentFallbackImportReturnPlan.FlagsAtStop
            && actual.ParentPlan.ParentPlan.ParentFallbackCounterPlan.ParentFallbackImportReturnPlan
                .ExactCleanCompletion
                == reprojected.ParentPlan.ParentPlan.ParentFallbackCounterPlan
                    .ParentFallbackImportReturnPlan.ExactCleanCompletion;
    }
}

/// <summary>
/// Reprojects the scalar dword CMP at BBDE0E and stops at BBDE15, before the
/// next branch. The absolute memory operand is captured as a DWORD value.
/// </summary>
public static class NetworkDispatcherEntryFallbackSelectorCompareProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE0E;
    public const uint CompareRawFileOffset = EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive = 0x00BBDE15;
    public const uint ComparedMemoryOperandVirtualAddress = 0x01E68590;
    public const byte ComparedDwordWidth = sizeof(uint);
    public const uint ComparedImmediate = 1;
    public const byte CompareInstructionByteLength = 7;
    public const string CompareInstructionBytes = "833D9085E60101";
    public const string CapturedWindowBytes = "833D9085E601017F14";

    public static uint EntryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(EntryRawFileOffset);
    public static uint CompareCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(CompareRawFileOffset);
    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackSelectorComparePlan Project(
        NetworkDispatcherEntryFallbackSelectorCompareCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherEntryFallbackZeroBranchPlan parent =
            NetworkDispatcherEntryFallbackZeroBranchProjection.Project(capture.ParentCapture);
        bool parentZeroBranchAccepted =
            parent.ExactCleanCompletion
            && parent.ParentClearStoresAccepted
            && parent.Outcome
                == NetworkDispatcherEntryFallbackZeroBranchOutcome.JzNotTakenToBBDE0E
            && parent.JzNotTaken
            && !parent.JzTaken
            && parent.SelectedDestinationRawFileOffset == EntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.FlagsPreserved
            && parent.FlagsAtStop is { Zero: false }
            && parent.ParentPlan.ExactCleanCompletion
            && parent.ParentPlan.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackZeroBranchProjection.EntryRawFileOffset;

        if (!parentZeroBranchAccepted)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackSelectorCompareOutcome
                    .ParentFallbackZeroBranchNotAccepted,
                StateForUnreachedParent(capture.FreshComparedDword),
                null,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive ?? EntryRawFileOffset,
                null,
                null,
                parent.FlagsAtStop,
                compareReached: false,
                parentZeroBranchAccepted: false,
                projectionEntered: false);
        }

        if (capture.FreshComparedDword is not uint comparedDword)
        {
            var attemptedRead = new NetworkDispatcherEntryFallbackSelectorCompareRead(
                CompareRawFileOffset,
                ComparedMemoryOperandVirtualAddress,
                ComparedDwordWidth,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false);
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackSelectorCompareOutcome.MissingFreshComparedDword,
                NetworkDispatcherEntryFallbackSelectorCompareCaptureState.Missing,
                attemptedRead,
                parent.CompletedInstructionOffsets,
                CompareRawFileOffset,
                null,
                null,
                parent.FlagsAtStop,
                compareReached: false,
                parentZeroBranchAccepted: true,
                projectionEntered: true);
        }

        var compareRead = new NetworkDispatcherEntryFallbackSelectorCompareRead(
            CompareRawFileOffset,
            ComparedMemoryOperandVirtualAddress,
            ComparedDwordWidth,
            Attempted: true,
            Captured: true,
            Value: comparedDword,
            Committed: false);
        NetworkDispatcherLockGateFlags compareFlags = CompareDwordToImmediateFlags(comparedDword);
        return new(
            capture,
            parent,
            NetworkDispatcherEntryFallbackSelectorCompareOutcome.Compared,
            NetworkDispatcherEntryFallbackSelectorCompareCaptureState.Captured,
            compareRead,
            parent.CompletedInstructionOffsets.Add(CompareRawFileOffset),
            BoundaryRawFileOffsetExclusive,
            comparedDword,
            compareFlags,
            compareFlags,
            compareReached: true,
            parentZeroBranchAccepted: true,
            projectionEntered: true);
    }

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta);

    private static NetworkDispatcherLockGateFlags CompareDwordToImmediateFlags(uint value)
    {
        uint result = unchecked(value - ComparedImmediate);
        return new(
            Carry: value < ComparedImmediate,
            Parity: (BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            Auxiliary: ((value ^ ComparedImmediate ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((value ^ ComparedImmediate) & (value ^ result) & 0x80000000) != 0);
    }

    private static NetworkDispatcherEntryFallbackSelectorCompareCaptureState StateForUnreachedParent(
        uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackSelectorCompareCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackSelectorCompareCaptureState.Unreached;
}
