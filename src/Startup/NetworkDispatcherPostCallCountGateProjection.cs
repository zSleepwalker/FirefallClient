using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum NetworkDispatcherPostCallCountGateOutcome
{
    ParentPreparationIncomplete,
    ParentPreparationDidNotSelectNonzeroVtableCall,
    ParentPreparationNotClean,
    MissingVtableCallReturnStatus,
    VtableCallDidNotReturnNormally,
    MissingFreshPostCallESI,
    MissingFreshEndDWORD,
    MissingFreshPostCallEBX,
    MissingFreshStartDWORD,
    BelowCountContinuationBoundary,
    OpaqueCellCallBoundary
}

public enum NetworkDispatcherPostCallCountGateReadKind
{
    FreshEndDWORD,
    FreshStartDWORD
}

public enum NetworkDispatcherPostCallCountGateCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherPostCallCountGateOrderedEffectKind
{
    ParentEntryCallPreparationEffect,
    VtableCallReturnStatus,
    FreshEndDWORDRead,
    PostCallEBXIncrement,
    FreshStartDWORDRead,
    SubtractStart,
    ArithmeticShiftRight,
    CompareCount,
    ConditionalBranch,
    PrepareOpaqueCellCall
}

public readonly record struct NetworkDispatcherPostCallCountGateFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct NetworkDispatcherPostCallCountGateCaptureStates(
    NetworkDispatcherPostCallCountGateCaptureState VtableCallReturnedNormally,
    NetworkDispatcherPostCallCountGateCaptureState FreshPostCallESI,
    NetworkDispatcherPostCallCountGateCaptureState FreshEndDWORD,
    NetworkDispatcherPostCallCountGateCaptureState FreshPostCallEBX,
    NetworkDispatcherPostCallCountGateCaptureState FreshStartDWORD);

public sealed record NetworkDispatcherPostCallCountGateCapture(
    NetworkDispatcherEntryCallPreparationInputs ParentEntryCallPreparation,
    bool? VtableCallReturnedNormally,
    uint? FreshPostCallESI,
    uint? FreshEndDWORD,
    uint? FreshPostCallEBX,
    uint? FreshStartDWORD);

public readonly record struct NetworkDispatcherPostCallCountGateRead(
    uint RawFileOffset,
    NetworkDispatcherPostCallCountGateReadKind Kind,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkDispatcherPostCallCountGateOrderedEffect(
    int Sequence,
    NetworkDispatcherEntryCallPreparationOrderedEffect? ParentEntryCallPreparationEffect,
    uint RawFileOffset,
    NetworkDispatcherPostCallCountGateOrderedEffectKind Kind,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Committed);

public sealed record NetworkDispatcherPostCallCountGatePlan(
    NetworkDispatcherEntryCallPreparationPlan ParentEntryCallPreparationPlan,
    NetworkDispatcherPostCallCountGateCapture Capture,
    NetworkDispatcherPostCallCountGateOutcome Outcome,
    ImmutableArray<NetworkDispatcherPostCallCountGateRead> Reads,
    ImmutableArray<NetworkDispatcherPostCallCountGateOrderedEffect> OrderedEffects,
    NetworkDispatcherPostCallCountGateCaptureStates CaptureStates,
    bool ChildGateAdmitted,
    bool VtableCallReturnStatusReached,
    bool? VtableCallReturnedNormally,
    uint? PostCallESI,
    uint? FreshEndAddress,
    uint? FreshEndValue,
    uint? FreshPostCallEBX,
    uint? CountAfterIncrement,
    NetworkDispatcherPostCallCountGateFlags? IncrementFlags,
    uint? FreshStartAddress,
    uint? FreshStartValue,
    uint? DifferenceValue,
    NetworkDispatcherPostCallCountGateFlags? SubtractFlags,
    uint? ShiftedDifference,
    NetworkDispatcherPostCallCountGateFlags? ShiftFlags,
    bool CompareReached,
    NetworkDispatcherPostCallCountGateFlags? CompareFlags,
    bool BranchReached,
    bool? BelowCountBranchTaken,
    uint? EBXAtOpaqueCallBoundary,
    uint? SelectedDestinationRawFileOffset,
    bool OpaqueCellReadAttempted,
    bool OpaqueCellCallAttempted,
    NetworkDispatcherPostCallCountGateFlags? FlagsAtStop,
    uint StopAtRawFileOffsetExclusive)
{
    public bool BoundedContinuationEntered => false;

    public bool ParentBoundedCompletionBoundaryReached =>
        ParentEntryCallPreparationPlan.BoundedCompletionBoundaryReached;

    public bool BoundedCompletionBoundaryReached =>
        BranchReached
        || Outcome == NetworkDispatcherPostCallCountGateOutcome.OpaqueCellCallBoundary;

    public bool ExactBoundedCompletion =>
        BoundedCompletionBoundaryReached
        || (!ChildGateAdmitted && ParentEntryCallPreparationPlan.ExactBoundedCompletion);

    public bool ExactCleanCompletion =>
        ChildGateAdmitted
        && BoundedCompletionBoundaryReached
        && ParentEntryCallPreparationPlan.ExactCleanCompletion
        && CaptureStates.VtableCallReturnedNormally
            == NetworkDispatcherPostCallCountGateCaptureState.Captured
        && CaptureStates.FreshPostCallESI
            == NetworkDispatcherPostCallCountGateCaptureState.Captured
        && CaptureStates.FreshEndDWORD
            == NetworkDispatcherPostCallCountGateCaptureState.Captured
        && CaptureStates.FreshPostCallEBX
            == NetworkDispatcherPostCallCountGateCaptureState.Captured
        && CaptureStates.FreshStartDWORD
            == NetworkDispatcherPostCallCountGateCaptureState.Captured
        && !OpaqueCellReadAttempted
        && !OpaqueCellCallAttempted;
}

/// <summary>
/// Value-only projection after the nonzero-entry vtable-call boundary. It
/// neither executes the call nor reads the later opaque import cell.
/// </summary>
public static class NetworkDispatcherPostCallCountGateProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE6C;
    public const uint RawToVirtualAddressDelta = 0x00400C00;

    public const uint VtableCallRawFileOffset = 0x00BBDE6C;
    public const uint FreshEndDWORDReadRawFileOffset = 0x00BBDE6F;
    public const uint IncrementPostCallEBXRawFileOffset = 0x00BBDE75;
    public const uint FreshStartDWORDReadRawFileOffset = 0x00BBDE76;
    public const uint SubtractStartRawFileOffset = 0x00BBDE76;
    public const uint ArithmeticShiftRightRawFileOffset = 0x00BBDE7C;
    public const uint CompareCountRawFileOffset = 0x00BBDE7F;
    public const uint BelowCountBranchRawFileOffset = 0x00BBDE81;
    public const uint BelowCountContinuationRawFileOffset = 0x00BBDE40;
    public const uint PrepareOpaqueCellCallRawFileOffset = 0x00BBDE83;
    public const uint OpaqueCellCallBoundaryRawFileOffset = 0x00BBDE86;
    public const uint OpaqueCellVirtualAddress = 0x01AC4D24;

    public const uint EndOffsetFromESI = 0xA8;
    public const uint StartOffsetFromESI = 0xA4;
    public const uint EBXOffsetFromESI = 0x6C;
    public const uint ShiftCount = 2;
    public const string CapturedWindowBytes =
        "8B86A8000000432B86A4000000C1F8023BD872BD8D5E6CFF15244DAC01";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherPostCallCountGatePlan Project(
        NetworkDispatcherPostCallCountGateCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherEntryCallPreparationPlan parent =
            NetworkDispatcherEntryCallPreparationProjection.Project(
                capture.ParentEntryCallPreparation);
        var reads = ImmutableArray.CreateBuilder<NetworkDispatcherPostCallCountGateRead>();
        var effects =
            ImmutableArray.CreateBuilder<NetworkDispatcherPostCallCountGateOrderedEffect>();
        foreach (NetworkDispatcherEntryCallPreparationOrderedEffect parentEffect
            in parent.OrderedEffects)
        {
            uint? parentOffset =
                parentEffect.Read?.RawFileOffset
                ?? parentEffect.Push?.RawFileOffset
                ?? parentEffect.ParentCollectionGateEffect?.RawFileOffset;
            uint? parentAddress =
                parentEffect.Read?.Address
                ?? parentEffect.Push?.StackPointerAfter
                ?? parentEffect.ParentCollectionGateEffect?.Address;
            uint? parentValue =
                parentEffect.Read?.Value
                ?? parentEffect.Push?.Value
                ?? parentEffect.ParentCollectionGateEffect?.Value;
            byte parentWidth =
                parentEffect.Read?.Width
                ?? parentEffect.ParentCollectionGateEffect?.Width
                ?? (parentEffect.Push.HasValue ? (byte)4 : (byte)0);
            bool parentAttempted =
                parentEffect.Read?.Attempted
                ?? parentEffect.Push?.Attempted
                ?? parentEffect.ParentCollectionGateEffect?.Attempted
                ?? false;
            bool parentCommitted =
                parentEffect.Read?.Committed
                ?? parentEffect.Push?.Committed
                ?? parentEffect.ParentCollectionGateEffect?.Committed
                ?? false;
            if (parentOffset is uint offset)
            {
                effects.Add(new(
                    effects.Count,
                    parentEffect,
                    offset,
                    NetworkDispatcherPostCallCountGateOrderedEffectKind.ParentEntryCallPreparationEffect,
                    parentAddress,
                    parentWidth,
                    parentValue,
                    parentAttempted,
                    parentCommitted));
            }
        }

        NetworkDispatcherPostCallCountGateCaptureState callState =
            UnreachedOrUnused(capture.VtableCallReturnedNormally.HasValue);
        NetworkDispatcherPostCallCountGateCaptureState esiState =
            UnreachedOrUnused(capture.FreshPostCallESI.HasValue);
        NetworkDispatcherPostCallCountGateCaptureState endState =
            UnreachedOrUnused(capture.FreshEndDWORD.HasValue);
        NetworkDispatcherPostCallCountGateCaptureState ebxState =
            UnreachedOrUnused(capture.FreshPostCallEBX.HasValue);
        NetworkDispatcherPostCallCountGateCaptureState startState =
            UnreachedOrUnused(capture.FreshStartDWORD.HasValue);

        NetworkDispatcherPostCallCountGateOutcome? rejection = null;
        if (!parent.BoundedCompletionBoundaryReached)
        {
            rejection =
                NetworkDispatcherPostCallCountGateOutcome.ParentPreparationIncomplete;
        }
        else if (parent.Outcome
            == NetworkDispatcherEntryCallPreparationOutcome.ZeroEntryOpaqueImportBoundary)
        {
            rejection =
                NetworkDispatcherPostCallCountGateOutcome.ParentPreparationDidNotSelectNonzeroVtableCall;
        }
        else if (!parent.ExactCleanCompletion)
        {
            rejection =
                NetworkDispatcherPostCallCountGateOutcome.ParentPreparationNotClean;
        }
        else if (parent.Outcome
                != NetworkDispatcherEntryCallPreparationOutcome.NonzeroEntryVtableCallBoundary
            || parent.StopAtRawFileOffsetExclusive != VtableCallRawFileOffset)
        {
            rejection =
                NetworkDispatcherPostCallCountGateOutcome.ParentPreparationDidNotSelectNonzeroVtableCall;
        }

        bool admitted = rejection is null;
        NetworkDispatcherPostCallCountGateFlags? parentFlags = ConvertParentFlags(parent.FlagsAtStop);
        NetworkDispatcherPostCallCountGateFlags? flags = parentFlags;
        NetworkDispatcherPostCallCountGateOutcome outcome;
        bool returnReached = false;
        bool? returnedNormally = null;
        uint? postCallEsi = null;
        uint? endAddress = null;
        uint? endValue = null;
        uint? freshEbx = null;
        uint? countAfterIncrement = null;
        NetworkDispatcherPostCallCountGateFlags? incrementFlags = null;
        uint? startAddress = null;
        uint? startValue = null;
        uint? difference = null;
        NetworkDispatcherPostCallCountGateFlags? subtractFlags = null;
        uint? shiftedDifference = null;
        NetworkDispatcherPostCallCountGateFlags? shiftFlags = null;
        bool compareReached = false;
        NetworkDispatcherPostCallCountGateFlags? compareFlags = null;
        bool branchReached = false;
        bool? belowCountTaken = null;
        uint? ebxAtCall = null;
        uint? destination = null;
        bool cellReadAttempted = false;
        bool cellCallAttempted = false;
        uint stopAt = admitted
            ? VtableCallRawFileOffset
            : parent.StopAtRawFileOffsetExclusive;

        if (rejection is NetworkDispatcherPostCallCountGateOutcome parentOutcome)
        {
            outcome = parentOutcome;
        }
        else
        {
            returnReached = true;
            callState = State(capture.VtableCallReturnedNormally);
            returnedNormally = capture.VtableCallReturnedNormally;
            effects.Add(Effect(VtableCallRawFileOffset,
                NetworkDispatcherPostCallCountGateOrderedEffectKind.VtableCallReturnStatus,
                null, 0, returnedNormally is true ? 1u
                    : returnedNormally is false ? 0u : null,
                attempted: true,
                committed: returnedNormally.HasValue));

            if (returnedNormally is not bool callReturnedNormally)
            {
                outcome =
                    NetworkDispatcherPostCallCountGateOutcome.MissingVtableCallReturnStatus;
            }
            else if (!callReturnedNormally)
            {
                outcome =
                    NetworkDispatcherPostCallCountGateOutcome.VtableCallDidNotReturnNormally;
            }
            else
            {
                flags = UnknownFlags();
                esiState = State(capture.FreshPostCallESI);
                if (capture.FreshPostCallESI is not uint esi)
                {
                    ReadEnd(null, null);
                    outcome =
                        NetworkDispatcherPostCallCountGateOutcome.MissingFreshPostCallESI;
                    stopAt = FreshEndDWORDReadRawFileOffset;
                }
                else
                {
                    postCallEsi = esi;
                    endAddress = unchecked(esi + EndOffsetFromESI);
                    if (capture.FreshEndDWORD is not uint end)
                    {
                        endState = NetworkDispatcherPostCallCountGateCaptureState.Missing;
                        ReadEnd(endAddress, null);
                        outcome = NetworkDispatcherPostCallCountGateOutcome.MissingFreshEndDWORD;
                        stopAt = FreshEndDWORDReadRawFileOffset;
                    }
                    else
                    {
                        endState = NetworkDispatcherPostCallCountGateCaptureState.Captured;
                        endValue = end;
                        ReadEnd(endAddress, end);

                        ebxState = State(capture.FreshPostCallEBX);
                        if (capture.FreshPostCallEBX is not uint ebx)
                        {
                            outcome =
                                NetworkDispatcherPostCallCountGateOutcome.MissingFreshPostCallEBX;
                            stopAt = IncrementPostCallEBXRawFileOffset;
                        }
                        else
                        {
                            freshEbx = ebx;
                            countAfterIncrement = unchecked(ebx + 1);
                            incrementFlags = IncrementFlags(
                                ebx, countAfterIncrement.Value, flags.Value.Carry);
                            flags = incrementFlags;
                            effects.Add(Effect(IncrementPostCallEBXRawFileOffset,
                                NetworkDispatcherPostCallCountGateOrderedEffectKind.PostCallEBXIncrement,
                                null, 4, countAfterIncrement, true, true));

                            startAddress = unchecked(esi + StartOffsetFromESI);
                            if (capture.FreshStartDWORD is not uint start)
                            {
                                startState = NetworkDispatcherPostCallCountGateCaptureState.Missing;
                                ReadStart(startAddress.Value, null);
                                outcome =
                                    NetworkDispatcherPostCallCountGateOutcome.MissingFreshStartDWORD;
                                stopAt = FreshStartDWORDReadRawFileOffset;
                            }
                            else
                            {
                                startState = NetworkDispatcherPostCallCountGateCaptureState.Captured;
                                startValue = start;
                                ReadStart(startAddress.Value, start);
                                difference = unchecked(end - start);
                                subtractFlags = SubtractFlags(end, start, difference.Value);
                                flags = subtractFlags;
                                effects.Add(Effect(SubtractStartRawFileOffset,
                                    NetworkDispatcherPostCallCountGateOrderedEffectKind.SubtractStart,
                                    null, 4, difference, true, true));

                                shiftedDifference =
                                    unchecked((uint)((int)difference.Value >> (int)ShiftCount));
                                shiftFlags = ArithmeticShiftRightFlags(
                                    difference.Value, shiftedDifference.Value, ShiftCount);
                                flags = shiftFlags;
                                effects.Add(Effect(ArithmeticShiftRightRawFileOffset,
                                    NetworkDispatcherPostCallCountGateOrderedEffectKind.ArithmeticShiftRight,
                                    null, 4, shiftedDifference, true, true));

                                compareReached = true;
                                compareFlags = SubtractFlags(
                                    countAfterIncrement.Value,
                                    shiftedDifference.Value,
                                    unchecked(countAfterIncrement.Value - shiftedDifference.Value));
                                flags = compareFlags;
                                effects.Add(Effect(CompareCountRawFileOffset,
                                    NetworkDispatcherPostCallCountGateOrderedEffectKind.CompareCount,
                                    null, 4,
                                    unchecked(countAfterIncrement.Value - shiftedDifference.Value),
                                    true, false));

                                branchReached = true;
                                belowCountTaken = compareFlags.Value.Carry;
                                effects.Add(Effect(BelowCountBranchRawFileOffset,
                                    NetworkDispatcherPostCallCountGateOrderedEffectKind.ConditionalBranch,
                                    null, 1, belowCountTaken is true ? 1u : 0u,
                                    true, false));
                                if (belowCountTaken is true)
                                {
                                    destination = BelowCountContinuationRawFileOffset;
                                    outcome =
                                        NetworkDispatcherPostCallCountGateOutcome.BelowCountContinuationBoundary;
                                    stopAt = BelowCountContinuationRawFileOffset;
                                }
                                else
                                {
                                    ebxAtCall = unchecked(esi + EBXOffsetFromESI);
                                    effects.Add(Effect(PrepareOpaqueCellCallRawFileOffset,
                                        NetworkDispatcherPostCallCountGateOrderedEffectKind.PrepareOpaqueCellCall,
                                        ebxAtCall, 4, ebxAtCall, true, true));
                                    destination = OpaqueCellCallBoundaryRawFileOffset;
                                    outcome =
                                        NetworkDispatcherPostCallCountGateOutcome.OpaqueCellCallBoundary;
                                    stopAt = OpaqueCellCallBoundaryRawFileOffset;
                                }
                            }
                        }
                    }
                }
            }
        }

        var captureStates = new NetworkDispatcherPostCallCountGateCaptureStates(
            callState, esiState, endState, ebxState, startState);
        return new(
            parent,
            capture,
            outcome,
            reads.ToImmutable(),
            effects.ToImmutable(),
            captureStates,
            admitted,
            returnReached,
            returnedNormally,
            postCallEsi,
            endAddress,
            endValue,
            freshEbx,
            countAfterIncrement,
            incrementFlags,
            startAddress,
            startValue,
            difference,
            subtractFlags,
            shiftedDifference,
            shiftFlags,
            compareReached,
            compareFlags,
            branchReached,
            belowCountTaken,
            ebxAtCall,
            destination,
            cellReadAttempted,
            cellCallAttempted,
            flags,
            stopAt);

        void ReadEnd(uint? address, uint? value)
        {
            bool captured = value.HasValue;
            reads.Add(new(
                FreshEndDWORDReadRawFileOffset,
                NetworkDispatcherPostCallCountGateReadKind.FreshEndDWORD,
                address,
                4,
                Attempted: true,
                Captured: captured,
                Value: value,
                Committed: false));
            effects.Add(Effect(FreshEndDWORDReadRawFileOffset,
                NetworkDispatcherPostCallCountGateOrderedEffectKind.FreshEndDWORDRead,
                address, 4, value, true, false));
        }

        void ReadStart(uint address, uint? value)
        {
            reads.Add(new(
                FreshStartDWORDReadRawFileOffset,
                NetworkDispatcherPostCallCountGateReadKind.FreshStartDWORD,
                address,
                4,
                Attempted: true,
                Captured: value.HasValue,
                Value: value,
                Committed: false));
            effects.Add(Effect(FreshStartDWORDReadRawFileOffset,
                NetworkDispatcherPostCallCountGateOrderedEffectKind.FreshStartDWORDRead,
                address, 4, value, true, false));
        }

        NetworkDispatcherPostCallCountGateOrderedEffect Effect(
            uint offset,
            NetworkDispatcherPostCallCountGateOrderedEffectKind kind,
            uint? address,
            byte width,
            uint? value,
            bool attempted,
            bool committed) =>
            new(effects.Count, null, offset, kind, address, width, value,
                attempted, committed);
    }

    private static NetworkDispatcherPostCallCountGateCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherPostCallCountGateCaptureState.Captured
            : NetworkDispatcherPostCallCountGateCaptureState.Missing;

    private static NetworkDispatcherPostCallCountGateCaptureState State(bool? value) =>
        value.HasValue
            ? NetworkDispatcherPostCallCountGateCaptureState.Captured
            : NetworkDispatcherPostCallCountGateCaptureState.Missing;

    private static NetworkDispatcherPostCallCountGateCaptureState UnreachedOrUnused(
        bool supplied) =>
        supplied
            ? NetworkDispatcherPostCallCountGateCaptureState.SuppliedUnused
            : NetworkDispatcherPostCallCountGateCaptureState.Unreached;

    private static NetworkDispatcherPostCallCountGateFlags IncrementFlags(
        uint before,
        uint after,
        bool? carry) =>
        new(
            carry,
            HasEvenParity((byte)after),
            ((before ^ 1 ^ after) & 0x10) != 0,
            after == 0,
            (after & 0x80000000) != 0,
            before == 0x7FFFFFFF);

    private static NetworkDispatcherPostCallCountGateFlags SubtractFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static NetworkDispatcherPostCallCountGateFlags ArithmeticShiftRightFlags(
        uint before,
        uint after,
        uint count) =>
        new(
            count == 0 ? null : ((before >> ((int)count - 1)) & 1) != 0,
            HasEvenParity((byte)after),
            null,
            after == 0,
            (after & 0x80000000) != 0,
            count == 1 ? false : null);

    private static NetworkDispatcherPostCallCountGateFlags UnknownFlags() =>
        new(null, null, null, null, null, null);

    private static NetworkDispatcherPostCallCountGateFlags? ConvertParentFlags(
        NetworkDispatcherCollectionGateFlags? flags) =>
        flags is NetworkDispatcherCollectionGateFlags parentFlags
            ? new(parentFlags.Carry, parentFlags.Parity, parentFlags.Auxiliary,
                parentFlags.Zero, parentFlags.Sign, parentFlags.Overflow)
            : null;

    private static bool HasEvenParity(byte value)
    {
        int setBits = 0;
        for (int bit = 0; bit < 8; bit++)
        {
            setBits += (value >> bit) & 1;
        }

        return (setBits & 1) == 0;
    }
}
