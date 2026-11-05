using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct NetworkDispatcherEntryFallbackClearStoresCapture(
    NetworkDispatcherEntryFallbackArgumentCompareCapture ParentCapture,
    uint? FreshPostFallbackESI);

public enum NetworkDispatcherEntryFallbackClearStoresCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackClearStoresOutcome
{
    ParentArgumentCompareNotAccepted,
    MissingFreshPostFallbackESI,
    CompletedBeforeBranch
}

public enum NetworkDispatcherEntryFallbackClearStoreKind
{
    FrameDwordClear,
    ReceiverByteClear
}

public readonly record struct NetworkDispatcherEntryFallbackClearStore(
    uint InstructionRawFileOffset,
    NetworkDispatcherEntryFallbackClearStoreKind Kind,
    uint? Address,
    byte Width,
    uint Value,
    bool Attempted,
    bool DestinationCaptured,
    bool Committed);

public sealed class NetworkDispatcherEntryFallbackClearStoresPlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackArgumentComparePlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackClearStoresPlan(
        NetworkDispatcherEntryFallbackClearStoresCapture capture,
        NetworkDispatcherEntryFallbackArgumentComparePlan parentPlan,
        NetworkDispatcherEntryFallbackClearStoresOutcome outcome,
        NetworkDispatcherEntryFallbackClearStoresCaptureState esiCaptureState,
        ImmutableArray<NetworkDispatcherEntryFallbackClearStore> stores,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? freshParentEBP,
        uint? freshPostFallbackESI,
        uint? frameAddress,
        uint? receiverAddress,
        NetworkDispatcherLockGateFlags? flagsAtStop,
        uint? stopAtRawFileOffsetExclusive,
        bool projectionEntered,
        bool branchProjected)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        FreshPostFallbackESICaptureState = esiCaptureState;
        Stores = stores;
        CompletedInstructionOffsets = completedInstructionOffsets;
        FreshParentEBP = freshParentEBP;
        FreshPostFallbackESI = freshPostFallbackESI;
        FrameAddress = frameAddress;
        ReceiverAddress = receiverAddress;
        FlagsAtStop = flagsAtStop;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        ProjectionEntered = projectionEntered;
        BranchProjected = branchProjected;
        reprojectedParentPlan = new(() =>
            NetworkDispatcherEntryFallbackArgumentCompareProjection.Project(
                Capture.ParentCapture));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactBoundedCompletion = new(ComputeExactBoundedCompletion);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public NetworkDispatcherEntryFallbackClearStoresCapture Capture { get; }
    public NetworkDispatcherEntryFallbackArgumentComparePlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackClearStoresOutcome Outcome { get; }
    public NetworkDispatcherEntryFallbackClearStoresCaptureState
        FreshPostFallbackESICaptureState { get; }
    public ImmutableArray<NetworkDispatcherEntryFallbackClearStore> Stores { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint? FreshParentEBP { get; }
    public uint? FreshPostFallbackESI { get; }
    public uint? FrameAddress { get; }
    public uint? ReceiverAddress { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop { get; }
    public uint EntryRawFileOffset => NetworkDispatcherEntryFallbackClearStoresProjection
        .EntryRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public bool ProjectionEntered { get; }
    public bool FrameDwordClearProjected =>
        Stores.Any(store =>
            store.Kind == NetworkDispatcherEntryFallbackClearStoreKind.FrameDwordClear
            && store.Committed);
    public bool ReceiverByteClearAttempted =>
        Stores.Any(store =>
            store.Kind == NetworkDispatcherEntryFallbackClearStoreKind.ReceiverByteClear
            && store.Attempted);
    public bool ReceiverByteClearProjected =>
        Stores.Any(store =>
            store.Kind == NetworkDispatcherEntryFallbackClearStoreKind.ReceiverByteClear
            && store.Committed);
    public bool BranchProjected { get; }
    public bool BranchExecuted => false;
    public bool CompareFlagsPreserved => true;
    public bool ParentEBPReusedOnlyAfterCompare => FreshParentEBP.HasValue;
    public bool PostCallESIForwarded => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private NetworkDispatcherEntryFallbackArgumentComparePlan ReprojectedParentPlan =>
        reprojectedParentPlan.Value;

    private bool ComputeExactBoundedCompletion() =>
        ProjectionEntered
        && Outcome == NetworkDispatcherEntryFallbackClearStoresOutcome.CompletedBeforeBranch
        && ParentPlan.ExactCleanCompletion
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryFallbackClearStoresProjection
                .BoundaryRawFileOffsetExclusive
        && FrameDwordClearProjected
        && ReceiverByteClearProjected
        && CompletedInstructionOffsets.Length >= 2
        && CompletedInstructionOffsets[^2]
            == NetworkDispatcherEntryFallbackClearStoresProjection
                .FrameDwordClearRawFileOffset
        && CompletedInstructionOffsets[^1]
            == NetworkDispatcherEntryFallbackClearStoresProjection
                .ReceiverByteClearRawFileOffset
        && NetworkDispatcherEntryFallbackClearStoresProjection
                .FrameDwordClearRawFileOffset
            + NetworkDispatcherEntryFallbackClearStoresProjection
                .FrameDwordClearInstructionByteLength
            == NetworkDispatcherEntryFallbackClearStoresProjection
                .ReceiverByteClearRawFileOffset
        && NetworkDispatcherEntryFallbackClearStoresProjection
                .BoundaryRawFileOffsetExclusive
            == NetworkDispatcherEntryFallbackClearStoresProjection
                    .ReceiverByteClearRawFileOffset
                + NetworkDispatcherEntryFallbackClearStoresProjection
                    .ReceiverByteClearInstructionByteLength
        && OrderedSemanticPrefixPreserved
        && !BranchProjected;

    private bool ComputeExactCleanCompletion() =>
        ExactBoundedCompletion
        && FreshParentEBP.HasValue
        && FreshPostFallbackESI.HasValue
        && FrameAddress.HasValue
        && ReceiverAddress.HasValue
        && Stores.Length == 2
        && Stores.All(store => store.Committed && store.DestinationCaptured);

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackArgumentComparePlan actual = ParentPlan;
        NetworkDispatcherEntryFallbackArgumentComparePlan reprojected = ReprojectedParentPlan;
        return actual.Outcome == reprojected.Outcome
            && actual.FreshPostFallbackEBPCaptureState
                == reprojected.FreshPostFallbackEBPCaptureState
            && actual.FreshArgumentByteCaptureState
                == reprojected.FreshArgumentByteCaptureState
            && actual.ArgumentRead == reprojected.ArgumentRead
            && actual.CompletedInstructionOffsets.SequenceEqual(
                reprojected.CompletedInstructionOffsets)
            && actual.EntryRawFileOffset == reprojected.EntryRawFileOffset
            && actual.StopAtRawFileOffsetExclusive
                == reprojected.StopAtRawFileOffsetExclusive
            && actual.FreshPostFallbackEBP == reprojected.FreshPostFallbackEBP
            && actual.ArgumentAddress == reprojected.ArgumentAddress
            && actual.ArgumentByte == reprojected.ArgumentByte
            && actual.CompareFlags == reprojected.CompareFlags
            && actual.FlagsAtStop == reprojected.FlagsAtStop
            && actual.CompareReached == reprojected.CompareReached
            && actual.ExactBoundedCompletion == reprojected.ExactBoundedCompletion
            && actual.ExactCleanCompletion == reprojected.ExactCleanCompletion
            && actual.ProjectionEntered == reprojected.ProjectionEntered
            && actual.ParentFallbackCounterPlan.Outcome
                == reprojected.ParentFallbackCounterPlan.Outcome
            && actual.ParentFallbackCounterPlan.CounterAccesses.SequenceEqual(
                reprojected.ParentFallbackCounterPlan.CounterAccesses)
            && actual.ParentFallbackCounterPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojected.ParentFallbackCounterPlan.CompletedInstructionOffsets)
            && actual.ParentFallbackCounterPlan.StopAtRawFileOffsetExclusive
                == reprojected.ParentFallbackCounterPlan.StopAtRawFileOffsetExclusive
            && actual.CompletedInstructionOffsets
                .Take(ParentPlan.CompletedInstructionOffsets.Length)
                .SequenceEqual(ParentPlan.CompletedInstructionOffsets)
            && ChildStoreSemanticsPreserved();
    }

    private bool ChildStoreSemanticsPreserved()
    {
        if (!ProjectionEntered)
        {
            return Stores.IsEmpty
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                && StopAtRawFileOffsetExclusive
                    == (ParentPlan.StopAtRawFileOffsetExclusive
                        ?? NetworkDispatcherEntryFallbackClearStoresProjection.EntryRawFileOffset)
                && FreshPostFallbackESICaptureState
                    == (Capture.FreshPostFallbackESI.HasValue
                        ? NetworkDispatcherEntryFallbackClearStoresCaptureState.SuppliedUnused
                        : NetworkDispatcherEntryFallbackClearStoresCaptureState.Unreached)
                && FlagsAtStop == ParentPlan.FlagsAtStop
                && !BranchProjected;
        }

        if (FreshParentEBP is not uint freshEBP)
            return false;

        uint frameAddress = unchecked(
            freshEBP - NetworkDispatcherEntryFallbackClearStoresProjection.FrameDwordDisplacement);
        var expectedFrameStore = new NetworkDispatcherEntryFallbackClearStore(
            NetworkDispatcherEntryFallbackClearStoresProjection.FrameDwordClearRawFileOffset,
            NetworkDispatcherEntryFallbackClearStoreKind.FrameDwordClear,
            frameAddress,
            NetworkDispatcherEntryFallbackClearStoresProjection.FrameDwordWidth,
            NetworkDispatcherEntryFallbackClearStoresProjection.FrameDwordClearValue,
            Attempted: true,
            DestinationCaptured: true,
            Committed: true);

        if (FreshPostFallbackESI is not uint freshESI)
        {
            var expectedReceiverAttempt = new NetworkDispatcherEntryFallbackClearStore(
                NetworkDispatcherEntryFallbackClearStoresProjection
                    .ReceiverByteClearRawFileOffset,
                NetworkDispatcherEntryFallbackClearStoreKind.ReceiverByteClear,
                Address: null,
                Width: NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteWidth,
                Value: NetworkDispatcherEntryFallbackClearStoresProjection
                    .ReceiverByteClearValue,
                Attempted: true,
                DestinationCaptured: false,
                Committed: false);
            return Outcome
                    == NetworkDispatcherEntryFallbackClearStoresOutcome
                        .MissingFreshPostFallbackESI
                && Stores.SequenceEqual(new[] { expectedFrameStore, expectedReceiverAttempt })
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets.Add(
                        NetworkDispatcherEntryFallbackClearStoresProjection
                            .FrameDwordClearRawFileOffset))
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackClearStoresProjection
                        .ReceiverByteClearRawFileOffset
                && FreshPostFallbackESICaptureState
                    == NetworkDispatcherEntryFallbackClearStoresCaptureState.Missing
                && FlagsAtStop == ParentPlan.CompareFlags
                && !BranchProjected;
        }

        uint receiverAddress = unchecked(
            freshESI
            + NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteDisplacement);
        var expectedReceiverStore = new NetworkDispatcherEntryFallbackClearStore(
            NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteClearRawFileOffset,
            NetworkDispatcherEntryFallbackClearStoreKind.ReceiverByteClear,
            receiverAddress,
            NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteWidth,
            NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteClearValue,
            Attempted: true,
            DestinationCaptured: true,
            Committed: true);
        return Outcome == NetworkDispatcherEntryFallbackClearStoresOutcome.CompletedBeforeBranch
            && Stores.SequenceEqual(new[] { expectedFrameStore, expectedReceiverStore })
            && CompletedInstructionOffsets.SequenceEqual(
                ParentPlan.CompletedInstructionOffsets
                    .Add(NetworkDispatcherEntryFallbackClearStoresProjection
                        .FrameDwordClearRawFileOffset)
                    .Add(NetworkDispatcherEntryFallbackClearStoresProjection
                        .ReceiverByteClearRawFileOffset))
            && StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackClearStoresProjection
                    .BoundaryRawFileOffsetExclusive
            && FreshPostFallbackESICaptureState
                == NetworkDispatcherEntryFallbackClearStoresCaptureState.Captured
            && FlagsAtStop == ParentPlan.CompareFlags
            && !BranchProjected;
    }
}

/// <summary>
/// Reprojects the adjacent frame DWORD and receiver byte clears after the
/// fallback byte CMP, stopping before the conditional branch.
/// Static evidence: FirefallClient.exe, raw/IDA EAs BBDDF9-BBDE0D inclusive;
/// .text VA mapping is raw + 400C00.
/// </summary>
public static class NetworkDispatcherEntryFallbackClearStoresProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDFD;
    public const uint FrameDwordClearRawFileOffset = EntryRawFileOffset;
    public const uint ReceiverByteClearRawFileOffset = 0x00BBDE04;
    public const uint BoundaryRawFileOffsetExclusive = 0x00BBDE08;
    public const uint FrameDwordDisplacement = 0x04;
    public const uint ReceiverByteDisplacement = 0x64;
    public const byte FrameDwordWidth = sizeof(uint);
    public const byte ReceiverByteWidth = sizeof(byte);
    public const byte FrameDwordClearInstructionByteLength = 7;
    public const byte ReceiverByteClearInstructionByteLength = 4;
    public const byte BranchInstructionByteLength = 6;
    public const uint FrameDwordClearValue = 0;
    public const byte ReceiverByteClearValue = 0;
    public const string FrameDwordClearInstructionBytes = "C745FC00000000";
    public const string ReceiverByteClearInstructionBytes = "C6466400";
    public const string BranchInstructionBytes = "0F84BC000000";
    public const string CapturedWindowBytes =
        "C745FC00000000C64664000F84BC000000";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackClearStoresPlan Project(
        NetworkDispatcherEntryFallbackClearStoresCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture.ParentCapture);

        NetworkDispatcherEntryFallbackArgumentComparePlan parent =
            NetworkDispatcherEntryFallbackArgumentCompareProjection.Project(
                capture.ParentCapture);
        bool admitted =
            parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackArgumentCompareProjection
                    .BoundaryRawFileOffsetExclusive
            && parent.CompareReached
            && parent.FreshPostFallbackEBP.HasValue
            && parent.Outcome
                == NetworkDispatcherEntryFallbackArgumentCompareOutcome.Compared;

        if (!admitted)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackClearStoresOutcome
                    .ParentArgumentCompareNotAccepted,
                StateForUnreachedParent(capture.FreshPostFallbackESI),
                ImmutableArray<NetworkDispatcherEntryFallbackClearStore>.Empty,
                parent.CompletedInstructionOffsets,
                null,
                null,
                null,
                null,
                parent.FlagsAtStop,
                parent.StopAtRawFileOffsetExclusive ?? EntryRawFileOffset,
                false,
                false);
        }

        uint freshEBP = parent.FreshPostFallbackEBP!.Value;
        uint frameAddress = unchecked(freshEBP - FrameDwordDisplacement);
        var frameStore = new NetworkDispatcherEntryFallbackClearStore(
            FrameDwordClearRawFileOffset,
            NetworkDispatcherEntryFallbackClearStoreKind.FrameDwordClear,
            frameAddress,
            FrameDwordWidth,
            FrameDwordClearValue,
            Attempted: true,
            DestinationCaptured: true,
            Committed: true);

        if (capture.FreshPostFallbackESI is not uint freshESI)
        {
            var attemptedReceiverStore = new NetworkDispatcherEntryFallbackClearStore(
                ReceiverByteClearRawFileOffset,
                NetworkDispatcherEntryFallbackClearStoreKind.ReceiverByteClear,
                Address: null,
                Width: ReceiverByteWidth,
                Value: ReceiverByteClearValue,
                Attempted: true,
                DestinationCaptured: false,
                Committed: false);
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackClearStoresOutcome
                    .MissingFreshPostFallbackESI,
                NetworkDispatcherEntryFallbackClearStoresCaptureState.Missing,
                ImmutableArray.Create(frameStore, attemptedReceiverStore),
                parent.CompletedInstructionOffsets.Add(FrameDwordClearRawFileOffset),
                freshEBP,
                null,
                frameAddress,
                null,
                parent.CompareFlags,
                ReceiverByteClearRawFileOffset,
                true,
                false);
        }

        uint receiverAddress = unchecked(freshESI + ReceiverByteDisplacement);
        var receiverStore = new NetworkDispatcherEntryFallbackClearStore(
            ReceiverByteClearRawFileOffset,
            NetworkDispatcherEntryFallbackClearStoreKind.ReceiverByteClear,
            receiverAddress,
            ReceiverByteWidth,
            ReceiverByteClearValue,
            Attempted: true,
            DestinationCaptured: true,
            Committed: true);
        ImmutableArray<NetworkDispatcherEntryFallbackClearStore> stores =
            ImmutableArray.Create(frameStore, receiverStore);

        return new(
            capture,
            parent,
            NetworkDispatcherEntryFallbackClearStoresOutcome.CompletedBeforeBranch,
            NetworkDispatcherEntryFallbackClearStoresCaptureState.Captured,
            stores,
            parent.CompletedInstructionOffsets
                .Add(FrameDwordClearRawFileOffset)
                .Add(ReceiverByteClearRawFileOffset),
            freshEBP,
            freshESI,
            frameAddress,
            receiverAddress,
            parent.CompareFlags,
            BoundaryRawFileOffsetExclusive,
            true,
            false);
    }

    private static NetworkDispatcherEntryFallbackClearStoresCaptureState StateForUnreachedParent(
        uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackClearStoresCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackClearStoresCaptureState.Unreached;
}
