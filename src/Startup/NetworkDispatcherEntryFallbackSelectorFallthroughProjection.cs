using System.Collections.Immutable;
using System.Threading;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackSelectorFallthroughCapture(
    NetworkDispatcherEntryFallbackSelectorBranchCapture ParentCapture,
    uint? FreshEntryESP,
    uint? FreshEntryECX);

public enum NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
{
    ParentSelectorBranchNotAccepted,
    JgTakenRejected,
    MissingFreshEntryESP,
    MissingFreshEntryECX,
    MissingFreshEntryESPAndECX,
    ProjectedTwoPushPrefix
}

public enum NetworkDispatcherEntryFallbackSelectorFallthroughRegister
{
    ESP,
    ECX
}

public readonly record struct NetworkDispatcherEntryFallbackSelectorFallthroughRegisterRead(
    uint InstructionRawFileOffset,
    NetworkDispatcherEntryFallbackSelectorFallthroughRegister Register,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value);

public readonly record struct NetworkDispatcherEntryFallbackSelectorFallthroughPush(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    uint StackPointerBefore,
    uint StackAddress,
    uint StackPointerAfter,
    uint Value,
    byte Width,
    bool Attempted,
    bool Reached,
    bool Committed,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter);

public sealed class NetworkDispatcherEntryFallbackSelectorFallthroughPlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackSelectorBranchPlan> reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackSelectorFallthroughPlan(
        NetworkDispatcherEntryFallbackSelectorFallthroughCapture capture,
        NetworkDispatcherEntryFallbackSelectorBranchPlan parentPlan,
        NetworkDispatcherEntryFallbackSelectorFallthroughOutcome outcome,
        NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState freshEntryESPCaptureState,
        NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState freshEntryECXCaptureState,
        ImmutableArray<NetworkDispatcherEntryFallbackSelectorFallthroughRegisterRead> registerReads,
        ImmutableArray<NetworkDispatcherEntryFallbackSelectorFallthroughPush> pushes,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        uint? finalESP,
        bool parentJgNotTakenAccepted,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        FreshEntryESPCaptureState = freshEntryESPCaptureState;
        FreshEntryECXCaptureState = freshEntryECXCaptureState;
        RegisterReads = registerReads;
        Pushes = pushes;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        FinalESP = finalESP;
        ParentJgNotTakenAccepted = parentJgNotTakenAccepted;
        ProjectionEntered = projectionEntered;
        reprojectedParentPlan = new(
            () => NetworkDispatcherEntryFallbackSelectorBranchProjection.Project(
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

    public NetworkDispatcherEntryFallbackSelectorFallthroughCapture Capture { get; }
    public NetworkDispatcherEntryFallbackSelectorBranchPlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackSelectorFallthroughOutcome Outcome { get; }
    public NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState FreshEntryESPCaptureState { get; }
    public NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState FreshEntryECXCaptureState { get; }
    public ImmutableArray<NetworkDispatcherEntryFallbackSelectorFallthroughRegisterRead> RegisterReads { get; }
    public ImmutableArray<NetworkDispatcherEntryFallbackSelectorFallthroughPush> Pushes { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint EntryRawFileOffset =>
        NetworkDispatcherEntryFallbackSelectorFallthroughProjection.EntryRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public uint? FinalESP { get; }
    public bool ParentJgNotTakenAccepted { get; }
    public bool ProjectionEntered { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop =>
        Pushes.IsEmpty ? ParentPlan.FlagsAtStop : Pushes[^1].FlagsAfter;
    public bool FlagsPreserved =>
        ParentJgNotTakenAccepted
        && ParentPlan.FlagsAtStop.HasValue
        && Pushes.Length == NetworkDispatcherEntryFallbackSelectorFallthroughProjection
            .MaximumProjectedPushCount
        && Pushes.All(push =>
            push.FlagsBefore == ParentPlan.FlagsAtStop.Value
            && push.FlagsAfter == ParentPlan.FlagsAtStop.Value)
        && FlagsAtStop == ParentPlan.FlagsAtStop;
    public bool JgTakenRejected =>
        Outcome == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome.JgTakenRejected;
    public bool PushPrefixProjected => !Pushes.IsEmpty;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ParentInstructionsExecuted => false;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion()
    {
        if (!ParentJgNotTakenAccepted
            || !ProjectionEntered
            || ParentPlan.FlagsAtStop is not NetworkDispatcherLockGateFlags flags
            || Capture.FreshEntryESP is not uint entryESP
            || Capture.FreshEntryECX is not uint entryECX
            || Pushes.Length != NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                .MaximumProjectedPushCount)
        {
            return false;
        }

        uint firstESP = unchecked(entryESP
            - NetworkDispatcherEntryFallbackSelectorFallthroughProjection.StackPointerDecrement);
        uint secondESP = unchecked(firstESP
            - NetworkDispatcherEntryFallbackSelectorFallthroughProjection.StackPointerDecrement);
        return FreshEntryESPCaptureState
                == NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.Captured
            && FreshEntryECXCaptureState
                == NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.Captured
            && RegisterReads.Length == 2
            && RegisterReads[0] is {
                InstructionRawFileOffset: 0x00BBDE17,
                Register: NetworkDispatcherEntryFallbackSelectorFallthroughRegister.ESP,
                Width: 4,
                Attempted: true,
                Captured: true
            }
            && RegisterReads[0].Value == entryESP
            && RegisterReads[1] is {
                InstructionRawFileOffset: 0x00BBDE17,
                Register: NetworkDispatcherEntryFallbackSelectorFallthroughRegister.ECX,
                Width: 4,
                Attempted: true,
                Captured: true
            }
            && RegisterReads[1].Value == entryECX
            && Pushes[0] is {
                InstructionRawFileOffset: 0x00BBDE17,
                InstructionCodeVirtualAddress: 0x00FBEA17,
                InstructionByteLength: 1,
                InstructionBytes: "51",
                Width: 4,
                Attempted: true,
                Reached: true,
                Committed: true
            }
            && Pushes[0].StackPointerBefore == entryESP
            && Pushes[0].StackAddress == firstESP
            && Pushes[0].StackPointerAfter == firstESP
            && Pushes[0].Value == entryECX
            && Pushes[0].FlagsBefore == flags
            && Pushes[0].FlagsAfter == flags
            && Pushes[1] is {
                InstructionRawFileOffset: 0x00BBDE18,
                InstructionCodeVirtualAddress: 0x00FBEA18,
                InstructionByteLength: 2,
                InstructionBytes: "6A00",
                Width: 4,
                Attempted: true,
                Reached: true,
                Committed: true
            }
            && Pushes[1].StackPointerBefore == firstESP
            && Pushes[1].StackAddress == secondESP
            && Pushes[1].StackPointerAfter == secondESP
            && Pushes[1].Value == 0
            && Pushes[1].FlagsBefore == flags
            && Pushes[1].FlagsAfter == flags
            && FinalESP == secondESP
            && StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .BoundaryRawFileOffsetExclusive
            && CompletedInstructionOffsets.Length
                == ParentPlan.CompletedInstructionOffsets.Length + 2
            && CompletedInstructionOffsets
                .Take(ParentPlan.CompletedInstructionOffsets.Length)
                .SequenceEqual(ParentPlan.CompletedInstructionOffsets)
            && CompletedInstructionOffsets[^2] == EntryRawFileOffset
            && CompletedInstructionOffsets[^1]
                == NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .SecondPushRawFileOffset
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .BoundaryRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .SecondPushRawFileOffset
                    + NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                        .SecondPushInstructionByteLength
            && !NativeInstructionsExecuted
            && !NativeMemoryReadsExecuted
            && !NativeMemoryWritesExecuted
            && !ParentInstructionsExecuted;
    }

    private bool ComputeExactCleanCompletion() =>
        ParentJgNotTakenAccepted
        && ProjectionEntered
        && ParentPlan.ExactCleanCompletion
        && OrderedSemanticPrefixPreserved
        && ExactBoundedCompletion
        && FlagsPreserved;

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackSelectorBranchPlan actual = ParentPlan;
        NetworkDispatcherEntryFallbackSelectorBranchPlan reprojected =
            reprojectedParentPlan.Value;
        return actual.Capture == reprojected.Capture
            && actual.Outcome == reprojected.Outcome
            && actual.Instructions.SequenceEqual(reprojected.Instructions)
            && actual.CompletedInstructionOffsets.SequenceEqual(
                reprojected.CompletedInstructionOffsets)
            && actual.EntryRawFileOffset == reprojected.EntryRawFileOffset
            && actual.StopAtRawFileOffsetExclusive
                == reprojected.StopAtRawFileOffsetExclusive
            && actual.ParentSelectorCompareAccepted == reprojected.ParentSelectorCompareAccepted
            && actual.ProjectionEntered == reprojected.ProjectionEntered
            && actual.FlagsAtStop == reprojected.FlagsAtStop
            && actual.FlagsPreserved == reprojected.FlagsPreserved
            && actual.JgNotTaken == reprojected.JgNotTaken
            && actual.JgTaken == reprojected.JgTaken
            && actual.ExactBoundedCompletion == reprojected.ExactBoundedCompletion
            && actual.ExactCleanCompletion == reprojected.ExactCleanCompletion
            && actual.OrderedSemanticPrefixPreserved
            && actual.ParentPlan.Capture == reprojected.ParentPlan.Capture
            && actual.ParentPlan.Outcome == reprojected.ParentPlan.Outcome
            && actual.ParentPlan.CompareRead == reprojected.ParentPlan.CompareRead
            && actual.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojected.ParentPlan.CompletedInstructionOffsets)
            && actual.ParentPlan.CompareFlags == reprojected.ParentPlan.CompareFlags
            && actual.ParentPlan.FlagsAtStop == reprojected.ParentPlan.FlagsAtStop
            && actual.ParentPlan.ExactCleanCompletion
                == reprojected.ParentPlan.ExactCleanCompletion
            && actual.ParentPlan.OrderedSemanticPrefixPreserved;
    }
}

/// <summary>
/// Reprojects only PUSH ECX and PUSH 0 on the JG fallthrough, then stops at BBDE1A.
/// </summary>
public static class NetworkDispatcherEntryFallbackSelectorFallthroughProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE17;
    public const uint SecondPushRawFileOffset = 0x00BBDE18;
    public const uint BoundaryRawFileOffsetExclusive = 0x00BBDE1A;
    public const byte PushECXInstructionByteLength = 1;
    public const byte SecondPushInstructionByteLength = 2;
    public const byte PushDwordWidth = sizeof(uint);
    public const uint StackPointerDecrement = sizeof(uint);
    public const int MaximumProjectedPushCount = 2;
    public const string PushECXInstructionBytes = "51";
    public const string SecondPushInstructionBytes = "6A00";
    public const string CapturedWindowBytes = "516A00683C4FBB01";
    public const bool BytesIndependentlyVerified = true;

    public static uint EntryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(EntryRawFileOffset);
    public static uint SecondPushCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(SecondPushRawFileOffset);
    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackSelectorFallthroughPlan Project(
        NetworkDispatcherEntryFallbackSelectorFallthroughCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherEntryFallbackSelectorBranchPlan parent =
            NetworkDispatcherEntryFallbackSelectorBranchProjection.Project(
                capture.ParentCapture);
        bool parentJgNotTakenAccepted =
            parent.ExactCleanCompletion
            && parent.ParentSelectorCompareAccepted
            && parent.Outcome
                == NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgNotTakenToBBDE17
            && parent.JgNotTaken
            && !parent.JgTaken
            && parent.SelectedDestinationRawFileOffset == EntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.FlagsPreserved
            && parent.FlagsAtStop == parent.CompareFlags
            && parent.Instructions.Length == 1
            && parent.Instructions[0].InstructionRawFileOffset
                == NetworkDispatcherEntryFallbackSelectorBranchProjection.EntryRawFileOffset;

        if (!parentJgNotTakenAccepted)
        {
            bool takenJg =
                parent.ExactCleanCompletion
                && parent.ParentSelectorCompareAccepted
                && parent.Outcome
                    == NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgTakenToBBDE2B
                && parent.JgTaken
                && parent.SelectedDestinationRawFileOffset
                    == NetworkDispatcherEntryFallbackSelectorBranchProjection
                        .BranchTargetRawFileOffset;
            return new(
                capture,
                parent,
                takenJg
                    ? NetworkDispatcherEntryFallbackSelectorFallthroughOutcome.JgTakenRejected
                    : NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
                        .ParentSelectorBranchNotAccepted,
                StateForUnused(capture.FreshEntryESP),
                StateForUnused(capture.FreshEntryECX),
                ImmutableArray<NetworkDispatcherEntryFallbackSelectorFallthroughRegisterRead>.Empty,
                ImmutableArray<NetworkDispatcherEntryFallbackSelectorFallthroughPush>.Empty,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive,
                null,
                parentJgNotTakenAccepted: false,
                projectionEntered: false);
        }

        var reads = ImmutableArray.Create(
            new NetworkDispatcherEntryFallbackSelectorFallthroughRegisterRead(
                EntryRawFileOffset,
                NetworkDispatcherEntryFallbackSelectorFallthroughRegister.ESP,
                PushDwordWidth,
                Attempted: true,
                Captured: capture.FreshEntryESP.HasValue,
                Value: capture.FreshEntryESP),
            new NetworkDispatcherEntryFallbackSelectorFallthroughRegisterRead(
                EntryRawFileOffset,
                NetworkDispatcherEntryFallbackSelectorFallthroughRegister.ECX,
                PushDwordWidth,
                Attempted: true,
                Captured: capture.FreshEntryECX.HasValue,
                Value: capture.FreshEntryECX));

        if (capture.FreshEntryESP is not uint entryESP
            || capture.FreshEntryECX is not uint entryECX)
        {
            NetworkDispatcherEntryFallbackSelectorFallthroughOutcome outcome =
                capture.FreshEntryESP is null && capture.FreshEntryECX is null
                    ? NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
                        .MissingFreshEntryESPAndECX
                    : capture.FreshEntryESP is null
                        ? NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
                            .MissingFreshEntryESP
                        : NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
                            .MissingFreshEntryECX;
            return new(
                capture,
                parent,
                outcome,
                State(capture.FreshEntryESP),
                State(capture.FreshEntryECX),
                reads,
                ImmutableArray<NetworkDispatcherEntryFallbackSelectorFallthroughPush>.Empty,
                parent.CompletedInstructionOffsets,
                EntryRawFileOffset,
                null,
                parentJgNotTakenAccepted: true,
                projectionEntered: true);
        }

        NetworkDispatcherLockGateFlags flags = parent.FlagsAtStop
            ?? throw new InvalidOperationException("The accepted JG fallthrough has no flags.");
        uint firstESP = unchecked(entryESP - StackPointerDecrement);
        uint secondESP = unchecked(firstESP - StackPointerDecrement);
        ImmutableArray<NetworkDispatcherEntryFallbackSelectorFallthroughPush> pushes =
            ImmutableArray.Create(
                new NetworkDispatcherEntryFallbackSelectorFallthroughPush(
                    EntryRawFileOffset,
                    EntryCodeVirtualAddress,
                    PushECXInstructionByteLength,
                    PushECXInstructionBytes,
                    entryESP,
                    firstESP,
                    firstESP,
                    entryECX,
                    PushDwordWidth,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: flags,
                    FlagsAfter: flags),
                new NetworkDispatcherEntryFallbackSelectorFallthroughPush(
                    SecondPushRawFileOffset,
                    SecondPushCodeVirtualAddress,
                    SecondPushInstructionByteLength,
                    SecondPushInstructionBytes,
                    firstESP,
                    secondESP,
                    secondESP,
                    0,
                    PushDwordWidth,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: flags,
                    FlagsAfter: flags));
        return new(
            capture,
            parent,
            NetworkDispatcherEntryFallbackSelectorFallthroughOutcome.ProjectedTwoPushPrefix,
            State(capture.FreshEntryESP),
            State(capture.FreshEntryECX),
            reads,
            pushes,
            parent.CompletedInstructionOffsets
                .Add(EntryRawFileOffset)
                .Add(SecondPushRawFileOffset),
            BoundaryRawFileOffsetExclusive,
            secondESP,
            parentJgNotTakenAccepted: true,
            projectionEntered: true);
    }

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta);

    private static NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.Captured
            : NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.Missing;

    private static NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState StateForUnused(
        uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.Unreached;
}
