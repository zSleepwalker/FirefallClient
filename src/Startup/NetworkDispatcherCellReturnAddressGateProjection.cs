using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherCellReturnAddressGateCapture(
    NetworkDispatcherPostCallCountGateCapture ParentCapture,
    bool? OpaqueCellReturnedNormally,
    uint? FreshPostCellEAX,
    uint? FreshPostCellEBP,
    uint? FreshPostCellESI);

public enum NetworkDispatcherCellReturnAddressGateCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public readonly record struct NetworkDispatcherCellReturnAddressGateCaptureStates(
    NetworkDispatcherCellReturnAddressGateCaptureState OpaqueCellReturnedNormally,
    NetworkDispatcherCellReturnAddressGateCaptureState FreshPostCellEAX,
    NetworkDispatcherCellReturnAddressGateCaptureState FreshPostCellEBP,
    NetworkDispatcherCellReturnAddressGateCaptureState FreshPostCellESI);

public enum NetworkDispatcherCellReturnAddressGateOutcome
{
    ParentHandoffNotAdmitted,
    MissingOpaqueCellReturnStatus,
    OpaqueCellDidNotReturnNormally,
    MissingFreshPostCellEBP,
    MissingFreshPostCellEAX,
    MissingFreshPostCellESI,
    BoundedBeforeEitherBranchDestination
}

public enum NetworkDispatcherCellReturnAddressGateOrderedEffectKind
{
    ParentPostCallCountGateEffect,
    OpaqueCellReturnStatus,
    FreshPostCellStackStore,
    LoadEffectiveAddressECX,
    LoadEffectiveAddressEAX,
    CompareEAXECX,
    ConditionalBranch
}

public readonly record struct NetworkDispatcherCellReturnAddressGateFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct NetworkDispatcherCellReturnAddressGateOrderedEffect(
    int Sequence,
    NetworkDispatcherPostCallCountGateOrderedEffect? ParentEffect,
    uint RawFileOffset,
    NetworkDispatcherCellReturnAddressGateOrderedEffectKind Kind,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Committed,
    NetworkDispatcherCellReturnAddressGateFlags? FlagsAfter);

public sealed record NetworkDispatcherCellReturnAddressGatePlan(
    NetworkDispatcherPostCallCountGatePlan ParentPlan,
    NetworkDispatcherCellReturnAddressGateCapture Capture,
    NetworkDispatcherCellReturnAddressGateOutcome Outcome,
    ImmutableArray<NetworkDispatcherCellReturnAddressGateOrderedEffect> OrderedEffects,
    ImmutableArray<uint> ReachedInstructionRawFileOffsets,
    NetworkDispatcherCellReturnAddressGateCaptureStates CaptureStates,
    bool ChildGateAdmitted,
    bool CellReturnStatusReached,
    bool? OpaqueCellReturnedNormally,
    uint? StackStoreAddress,
    uint? StackStoreValue,
    bool StackStoreAttempted,
    bool StackStoreCaptured,
    bool StackStoreCommitted,
    uint? FieldAddress,
    uint? ResultEAX,
    uint? ComparedEAX,
    uint? ComparedECX,
    NetworkDispatcherCellReturnAddressGateFlags? FlagsAfterCellReturn,
    NetworkDispatcherCellReturnAddressGateFlags? FlagsAfterStore,
    NetworkDispatcherCellReturnAddressGateFlags? FlagsAfterLEAECX,
    NetworkDispatcherCellReturnAddressGateFlags? FlagsAfterLEAEAX,
    NetworkDispatcherCellReturnAddressGateFlags? FlagsAfterCompare,
    NetworkDispatcherCellReturnAddressGateFlags? FlagsAtStop,
    bool CompareReached,
    bool BranchReached,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive)
{
    public uint? FreshPostCellEAX => Capture.FreshPostCellEAX;
    public uint? FreshPostCellEBP => Capture.FreshPostCellEBP;
    public uint? FreshPostCellESI => Capture.FreshPostCellESI;
    public uint? ResultECX => FieldAddress;
    public bool BranchTargetEntered => false;
    public bool FallthroughDestinationEntered => false;
    public bool CellTargetReadAttempted => false;
    public bool CellCallAttempted => false;
    public bool NativeInstructionsExecuted => false;
    public bool ChildReadsEmpty => true;

    public bool ChildGateReachedEndpoint =>
        Outcome == NetworkDispatcherCellReturnAddressGateOutcome.BoundedBeforeEitherBranchDestination
        && BranchReached
        && BranchTaken.HasValue;

    public bool ExactBoundedCompletion =>
        ChildGateReachedEndpoint
        || (!ChildGateAdmitted
            && ParentPlan.ExactBoundedCompletion
            && StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive);

    public bool ExactCleanCompletion =>
        ChildGateReachedEndpoint
        && ParentPlan.ExactCleanCompletion
        && CellReturnStatusReached
        && OpaqueCellReturnedNormally == true
        && CaptureStates.FreshPostCellEBP
            == NetworkDispatcherCellReturnAddressGateCaptureState.Captured
        && CaptureStates.FreshPostCellEAX
            == NetworkDispatcherCellReturnAddressGateCaptureState.Captured
        && CaptureStates.FreshPostCellESI
            == NetworkDispatcherCellReturnAddressGateCaptureState.Captured
        && StackStoreCommitted;
}

/// <summary>
/// Reprojects the opaque cell's return-address setup and equality branch
/// without executing the call, reading memory, or entering either destination.
/// </summary>
public static class NetworkDispatcherCellReturnAddressGateProjection
{
    public const uint OpaqueCellCallBoundaryRawFileOffset = 0x00BBDE86;
    public const uint StackStoreRawFileOffset = 0x00BBDE8C;
    public const uint LoadECXRawFileOffset = 0x00BBDE8F;
    public const uint LoadEAXRawFileOffset = 0x00BBDE92;
    public const uint CompareRawFileOffset = 0x00BBDE95;
    public const uint EqualBranchRawFileOffset = 0x00BBDE97;
    public const uint FallthroughDestinationRawFileOffset = 0x00BBDE99;
    public const uint BranchTargetRawFileOffset = 0x00BBDEA4;

    public const uint StackDisplacement = 0x10;
    public const uint FieldOffsetFromESI = 0x68;
    public const string CapturedWindowBytes = "8945F08D4E688D45F03BC1740B";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherCellReturnAddressGatePlan Project(
        NetworkDispatcherCellReturnAddressGateCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherPostCallCountGatePlan parent =
            NetworkDispatcherPostCallCountGateProjection.Project(capture.ParentCapture);
        var effects =
            ImmutableArray.CreateBuilder<NetworkDispatcherCellReturnAddressGateOrderedEffect>();
        foreach (NetworkDispatcherPostCallCountGateOrderedEffect parentEffect
            in parent.OrderedEffects)
        {
            effects.Add(new(
                effects.Count,
                parentEffect,
                parentEffect.RawFileOffset,
                NetworkDispatcherCellReturnAddressGateOrderedEffectKind.ParentPostCallCountGateEffect,
                parentEffect.Address,
                parentEffect.Width,
                parentEffect.Value,
                parentEffect.Attempted,
                parentEffect.Committed,
                null));
        }

        bool admitted = parent.ExactCleanCompletion
            && parent.Outcome == NetworkDispatcherPostCallCountGateOutcome.OpaqueCellCallBoundary
            && parent.BelowCountBranchTaken == false
            && parent.StopAtRawFileOffsetExclusive == OpaqueCellCallBoundaryRawFileOffset
            && !parent.OpaqueCellReadAttempted
            && !parent.OpaqueCellCallAttempted;
        var reached = ImmutableArray.CreateBuilder<uint>();
        NetworkDispatcherCellReturnAddressGateCaptureState statusState;
        NetworkDispatcherCellReturnAddressGateCaptureState eaxState;
        NetworkDispatcherCellReturnAddressGateCaptureState ebpState;
        NetworkDispatcherCellReturnAddressGateCaptureState esiState;
        NetworkDispatcherCellReturnAddressGateOutcome outcome;
        bool statusReached = false;
        bool? returnedNormally = null;
        uint? storeAddress = null;
        uint? storeValue = null;
        bool storeAttempted = false;
        bool storeCaptured = false;
        bool storeCommitted = false;
        uint? fieldAddress = null;
        uint? resultEax = null;
        uint? comparedEax = null;
        uint? comparedEcx = null;
        NetworkDispatcherCellReturnAddressGateFlags? flagsAfterReturn = null;
        NetworkDispatcherCellReturnAddressGateFlags? flagsAfterStore = null;
        NetworkDispatcherCellReturnAddressGateFlags? flagsAfterLeaEcx = null;
        NetworkDispatcherCellReturnAddressGateFlags? flagsAfterLeaEax = null;
        NetworkDispatcherCellReturnAddressGateFlags? flagsAfterCompare = null;
        NetworkDispatcherCellReturnAddressGateFlags? flagsAtStop = null;
        bool compareReached = false;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? destination = null;
        uint stopAt = admitted ? OpaqueCellCallBoundaryRawFileOffset
            : parent.StopAtRawFileOffsetExclusive;

        if (!admitted)
        {
            outcome = NetworkDispatcherCellReturnAddressGateOutcome.ParentHandoffNotAdmitted;
            statusState = UnreachedOrUnused(capture.OpaqueCellReturnedNormally.HasValue);
            eaxState = UnreachedOrUnused(capture.FreshPostCellEAX.HasValue);
            ebpState = UnreachedOrUnused(capture.FreshPostCellEBP.HasValue);
            esiState = UnreachedOrUnused(capture.FreshPostCellESI.HasValue);
            flagsAtStop = ConvertFlags(parent.FlagsAtStop);
        }
        else
        {
            statusReached = true;
            statusState = State(capture.OpaqueCellReturnedNormally);
            returnedNormally = capture.OpaqueCellReturnedNormally;
            flagsAfterReturn = returnedNormally is true
                ? UnknownFlags()
                : ConvertFlags(parent.FlagsAtStop);
            effects.Add(Effect(
                OpaqueCellCallBoundaryRawFileOffset,
                NetworkDispatcherCellReturnAddressGateOrderedEffectKind.OpaqueCellReturnStatus,
                null,
                0,
                returnedNormally is true ? 1u : returnedNormally is false ? 0u : null,
                attempted: true,
                committed: returnedNormally.HasValue,
                flagsAfterReturn));

            if (returnedNormally is not true)
            {
                eaxState = SuppliedUnusedOrUnreached(capture.FreshPostCellEAX);
                ebpState = SuppliedUnusedOrUnreached(capture.FreshPostCellEBP);
                esiState = SuppliedUnusedOrUnreached(capture.FreshPostCellESI);
                flagsAtStop = ConvertFlags(parent.FlagsAtStop);
                outcome = returnedNormally is false
                    ? NetworkDispatcherCellReturnAddressGateOutcome.OpaqueCellDidNotReturnNormally
                    : NetworkDispatcherCellReturnAddressGateOutcome.MissingOpaqueCellReturnStatus;
            }
            else
            {
                flagsAtStop = flagsAfterReturn;
                reached.Add(StackStoreRawFileOffset);
                storeAttempted = true;
                stopAt = StackStoreRawFileOffset;
                if (capture.FreshPostCellEBP is not uint ebp)
                {
                    effects.Add(Effect(
                        StackStoreRawFileOffset,
                        NetworkDispatcherCellReturnAddressGateOrderedEffectKind.FreshPostCellStackStore,
                        null,
                        4,
                        null,
                        attempted: true,
                        committed: false,
                        flagsAfterReturn));
                    ebpState = NetworkDispatcherCellReturnAddressGateCaptureState.Missing;
                    eaxState = SuppliedUnusedOrUnreached(capture.FreshPostCellEAX);
                    esiState = SuppliedUnusedOrUnreached(capture.FreshPostCellESI);
                    outcome = NetworkDispatcherCellReturnAddressGateOutcome.MissingFreshPostCellEBP;
                }
                else
                {
                    ebpState = NetworkDispatcherCellReturnAddressGateCaptureState.Captured;
                    storeAddress = unchecked(ebp - StackDisplacement);
                    eaxState = State(capture.FreshPostCellEAX);
                    if (capture.FreshPostCellEAX is not uint eax)
                    {
                        esiState = SuppliedUnusedOrUnreached(capture.FreshPostCellESI);
                        effects.Add(Effect(
                            StackStoreRawFileOffset,
                            NetworkDispatcherCellReturnAddressGateOrderedEffectKind.FreshPostCellStackStore,
                            storeAddress,
                            4,
                            null,
                            attempted: true,
                            committed: false,
                            flagsAfterReturn));
                        flagsAfterStore = flagsAfterReturn;
                        flagsAtStop = flagsAfterStore;
                        outcome = NetworkDispatcherCellReturnAddressGateOutcome.MissingFreshPostCellEAX;
                    }
                    else
                    {
                        storeValue = eax;
                        storeCaptured = true;
                        storeCommitted = true;
                        effects.Add(Effect(
                            StackStoreRawFileOffset,
                            NetworkDispatcherCellReturnAddressGateOrderedEffectKind.FreshPostCellStackStore,
                            storeAddress,
                            4,
                            eax,
                            attempted: true,
                            committed: true,
                            flagsAfterReturn));
                        flagsAfterStore = flagsAfterReturn;
                        flagsAtStop = flagsAfterStore;

                        reached.Add(LoadECXRawFileOffset);
                        if (capture.FreshPostCellESI is not uint esi)
                        {
                            esiState = NetworkDispatcherCellReturnAddressGateCaptureState.Missing;
                            effects.Add(Effect(
                                LoadECXRawFileOffset,
                                NetworkDispatcherCellReturnAddressGateOrderedEffectKind.LoadEffectiveAddressECX,
                                null,
                                4,
                                null,
                                attempted: true,
                                committed: false,
                                flagsAfterReturn));
                            flagsAfterLeaEcx = flagsAfterReturn;
                            flagsAtStop = flagsAfterLeaEcx;
                            stopAt = LoadECXRawFileOffset;
                            outcome = NetworkDispatcherCellReturnAddressGateOutcome.MissingFreshPostCellESI;
                        }
                        else
                        {
                            esiState = NetworkDispatcherCellReturnAddressGateCaptureState.Captured;
                            fieldAddress = unchecked(esi + FieldOffsetFromESI);
                            effects.Add(Effect(
                                LoadECXRawFileOffset,
                                NetworkDispatcherCellReturnAddressGateOrderedEffectKind.LoadEffectiveAddressECX,
                                fieldAddress,
                                4,
                                fieldAddress,
                                attempted: true,
                                committed: true,
                                flagsAfterReturn));
                            flagsAfterLeaEcx = flagsAfterReturn;

                            reached.Add(LoadEAXRawFileOffset);
                            resultEax = storeAddress;
                            effects.Add(Effect(
                                LoadEAXRawFileOffset,
                                NetworkDispatcherCellReturnAddressGateOrderedEffectKind.LoadEffectiveAddressEAX,
                                storeAddress,
                                4,
                                storeAddress,
                                attempted: true,
                                committed: true,
                                flagsAfterReturn));
                            flagsAfterLeaEax = flagsAfterReturn;

                            comparedEax = resultEax;
                            comparedEcx = fieldAddress;
                            reached.Add(CompareRawFileOffset);
                            compareReached = true;
                            flagsAfterCompare = CompareFlags32(resultEax.Value, fieldAddress.Value);
                            flagsAtStop = flagsAfterCompare;
                            effects.Add(Effect(
                                CompareRawFileOffset,
                                NetworkDispatcherCellReturnAddressGateOrderedEffectKind.CompareEAXECX,
                                null,
                                4,
                                unchecked(resultEax.Value - fieldAddress.Value),
                                attempted: true,
                                committed: false,
                                flagsAfterCompare));

                            reached.Add(EqualBranchRawFileOffset);
                            branchReached = true;
                            branchTaken = flagsAfterCompare.Value.Zero;
                            destination = branchTaken.Value
                                ? BranchTargetRawFileOffset
                                : FallthroughDestinationRawFileOffset;
                            outcome =
                                NetworkDispatcherCellReturnAddressGateOutcome.BoundedBeforeEitherBranchDestination;
                            stopAt = destination.Value;
                            effects.Add(Effect(
                                EqualBranchRawFileOffset,
                                NetworkDispatcherCellReturnAddressGateOrderedEffectKind.ConditionalBranch,
                                null,
                                1,
                                branchTaken.Value ? 1u : 0u,
                                attempted: true,
                                committed: false,
                                flagsAfterCompare));
                        }
                    }
                }
            }
        }

        return new(
            parent,
            capture,
            outcome,
            effects.ToImmutable(),
            reached.ToImmutable(),
            new(statusState, eaxState, ebpState, esiState),
            admitted,
            statusReached,
            returnedNormally,
            storeAddress,
            storeValue,
            storeAttempted,
            storeCaptured,
            storeCommitted,
            fieldAddress,
            resultEax,
            comparedEax,
            comparedEcx,
            flagsAfterReturn,
            flagsAfterStore,
            flagsAfterLeaEcx,
            flagsAfterLeaEax,
            flagsAfterCompare,
            flagsAtStop,
            compareReached,
            branchReached,
            branchTaken,
            destination,
            stopAt);

        NetworkDispatcherCellReturnAddressGateOrderedEffect Effect(
            uint offset,
            NetworkDispatcherCellReturnAddressGateOrderedEffectKind kind,
            uint? address,
            byte width,
            uint? value,
            bool attempted,
            bool committed,
            NetworkDispatcherCellReturnAddressGateFlags? flags) =>
            new(
                effects.Count,
                null,
                offset,
                kind,
                address,
                width,
                value,
                attempted,
                committed,
                flags);
    }

    private static NetworkDispatcherCellReturnAddressGateCaptureState State(bool? value) =>
        value.HasValue
            ? NetworkDispatcherCellReturnAddressGateCaptureState.Captured
            : NetworkDispatcherCellReturnAddressGateCaptureState.Missing;

    private static NetworkDispatcherCellReturnAddressGateCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherCellReturnAddressGateCaptureState.Captured
            : NetworkDispatcherCellReturnAddressGateCaptureState.Missing;

    private static NetworkDispatcherCellReturnAddressGateCaptureState UnreachedOrUnused(
        bool supplied) =>
        supplied
            ? NetworkDispatcherCellReturnAddressGateCaptureState.SuppliedUnused
            : NetworkDispatcherCellReturnAddressGateCaptureState.Unreached;

    private static NetworkDispatcherCellReturnAddressGateCaptureState SuppliedUnusedOrUnreached(
        uint? value) => UnreachedOrUnused(value.HasValue);

    private static NetworkDispatcherCellReturnAddressGateFlags? ConvertFlags(
        NetworkDispatcherPostCallCountGateFlags? flags) =>
        flags is NetworkDispatcherPostCallCountGateFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static NetworkDispatcherCellReturnAddressGateFlags UnknownFlags() =>
        new(null, null, null, null, null, null);

    private static NetworkDispatcherCellReturnAddressGateFlags CompareFlags32(
        uint left,
        uint right)
    {
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }
}
