using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherFencedPublicationCapture(
    NetworkDispatcherCellReturnAddressGateCapture ParentCapture,
    uint? FreshStackScalarDWORD,
    uint? FreshFieldScalarDWORD,
    uint? FreshPostCellEBX,
    uint? FreshCounterDWORD);

public enum NetworkDispatcherFencedPublicationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public readonly record struct NetworkDispatcherFencedPublicationCaptureStates(
    NetworkDispatcherFencedPublicationCaptureState FreshStackScalarDWORD,
    NetworkDispatcherFencedPublicationCaptureState FreshFieldScalarDWORD,
    NetworkDispatcherFencedPublicationCaptureState FreshPostCellEBX,
    NetworkDispatcherFencedPublicationCaptureState FreshCounterDWORD);

public enum NetworkDispatcherFencedPublicationOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshStackScalarDWORD,
    MissingFreshFieldScalarDWORD,
    MissingFreshPostCellEBX,
    MissingFreshCounterDWORD,
    BoundedBeforeContinuation
}

public enum NetworkDispatcherFencedPublicationOrderedEffectKind
{
    ParentCellReturnAddressGateEffect,
    SFENCEObservation,
    LFENCEObservation,
    FreshStackScalarRead,
    FieldScalarStore,
    FreshFieldScalarRead,
    PublicationByteStore,
    CounterDecrementReadWrite
}

public readonly record struct NetworkDispatcherFencedPublicationFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct NetworkDispatcherFencedPublicationOrderedEffect(
    int Sequence,
    NetworkDispatcherCellReturnAddressGateOrderedEffect? ParentEffect,
    uint RawFileOffset,
    NetworkDispatcherFencedPublicationOrderedEffectKind Kind,
    uint? Address,
    byte Width,
    uint? ReadValue,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed,
    NetworkDispatcherFencedPublicationFlags? FlagsAfter);

public sealed record NetworkDispatcherFencedPublicationPlan(
    NetworkDispatcherCellReturnAddressGatePlan ParentPlan,
    NetworkDispatcherFencedPublicationCapture Capture,
    NetworkDispatcherFencedPublicationOutcome Outcome,
    ImmutableArray<NetworkDispatcherFencedPublicationOrderedEffect> OrderedEffects,
    ImmutableArray<uint> ReachedInstructionRawFileOffsets,
    NetworkDispatcherFencedPublicationCaptureStates CaptureStates,
    bool ChildGateAdmitted,
    bool? EqualParentBranch,
    uint EntryRawFileOffset,
    uint? StackScalarAddress,
    uint? FieldAddress,
    uint? StackScalarValue,
    bool FieldScalarStoreAttempted,
    bool FieldScalarStoreCommitted,
    uint? FreshFieldScalarValue,
    uint? PublicationByteAddress,
    byte? PublicationByteValue,
    bool PublicationByteStoreCommitted,
    uint? CounterAddress,
    uint? CounterValueBefore,
    uint? CounterValueAfter,
    bool CounterDecrementAttempted,
    bool CounterDecrementCommitted,
    NetworkDispatcherFencedPublicationFlags? FlagsAtParentBranch,
    NetworkDispatcherFencedPublicationFlags? FlagsAfterFences,
    NetworkDispatcherFencedPublicationFlags? FlagsAfterStackRead,
    NetworkDispatcherFencedPublicationFlags? FlagsAfterFieldStore,
    NetworkDispatcherFencedPublicationFlags? FlagsAfterFieldRead,
    NetworkDispatcherFencedPublicationFlags? FlagsAfterPublicationByteStore,
    NetworkDispatcherFencedPublicationFlags? FlagsAfterCounterDecrement,
    NetworkDispatcherFencedPublicationFlags? FlagsAtStop,
    uint StopAtRawFileOffsetExclusive)
{
    public uint? FreshPostCellEBP => ParentPlan.Capture.FreshPostCellEBP;
    public uint? FreshPostCellESI => ParentPlan.Capture.FreshPostCellESI;
    public uint? FreshPostCellEBX => Capture.FreshPostCellEBX;
    public uint? FreshStackScalarDWORD => Capture.FreshStackScalarDWORD;
    public uint? FreshFieldScalarDWORD => Capture.FreshFieldScalarDWORD;
    public uint? FreshCounterDWORD => Capture.FreshCounterDWORD;
    public bool NativeInstructionsExecuted => false;
    public bool OpaqueCallAttempted => false;
    public bool ThirdCallTargetEntered => false;
    public bool ChildGateReachedEndpoint =>
        Outcome == NetworkDispatcherFencedPublicationOutcome.BoundedBeforeContinuation
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherFencedPublicationProjection.ContinuationRawFileOffset;

    public bool ExactBoundedCompletion =>
        ChildGateReachedEndpoint
        || (!ChildGateAdmitted
            && ParentPlan.ExactBoundedCompletion
            && StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive);

    public bool ExactCleanCompletion =>
        ChildGateReachedEndpoint
        && ParentPlan.ExactCleanCompletion
        && (EqualParentBranch is true
            ? CaptureStates.FreshStackScalarDWORD
                == NetworkDispatcherFencedPublicationCaptureState.Unreached
            : CaptureStates.FreshStackScalarDWORD
                == NetworkDispatcherFencedPublicationCaptureState.Captured)
        && CaptureStates.FreshFieldScalarDWORD
            == NetworkDispatcherFencedPublicationCaptureState.Captured
        && CaptureStates.FreshPostCellEBX
            == NetworkDispatcherFencedPublicationCaptureState.Captured
        && CaptureStates.FreshCounterDWORD
            == NetworkDispatcherFencedPublicationCaptureState.Captured
        && (EqualParentBranch is true
            ? !FieldScalarStoreAttempted
            : FieldScalarStoreAttempted && FieldScalarStoreCommitted)
        && PublicationByteStoreCommitted
        && CounterDecrementCommitted;
}

/// <summary>
/// Reprojects the observed publication instructions without executing them or
/// claiming runtime ordering semantics for the fence instructions.
/// </summary>
public static class NetworkDispatcherFencedPublicationProjection
{
    public const uint NotEqualEntryRawFileOffset = 0x00BBDE99;
    public const uint SFENCERawFileOffset = 0x00BBDE99;
    public const uint LFENCERawFileOffset = 0x00BBDE9C;
    public const uint StackScalarReadRawFileOffset = 0x00BBDE9F;
    public const uint FieldScalarStoreRawFileOffset = 0x00BBDEA2;
    public const uint EqualEntryRawFileOffset = 0x00BBDEA4;
    public const uint FieldScalarReadRawFileOffset = 0x00BBDEA4;
    public const uint PublicationByteStoreRawFileOffset = 0x00BBDEA6;
    public const uint CounterDecrementRawFileOffset = 0x00BBDEAA;
    public const uint ContinuationRawFileOffset = 0x00BBDEAD;
    public const uint StackDisplacement = 0x10;
    public const uint PublicationByteOffsetFromESI = 0x64;
    public const uint CounterOffsetFromEBX = 0x30;
    public const string CapturedWindowBytes =
        "0FAEF80FAEE88B45F089018B01C6466401FF4B30";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherFencedPublicationPlan Project(
        NetworkDispatcherFencedPublicationCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherCellReturnAddressGatePlan parent =
            NetworkDispatcherCellReturnAddressGateProjection.Project(capture.ParentCapture);
        var effects =
            ImmutableArray.CreateBuilder<NetworkDispatcherFencedPublicationOrderedEffect>();
        foreach (NetworkDispatcherCellReturnAddressGateOrderedEffect parentEffect
            in parent.OrderedEffects)
        {
            effects.Add(new(
                effects.Count,
                parentEffect,
                parentEffect.RawFileOffset,
                NetworkDispatcherFencedPublicationOrderedEffectKind
                    .ParentCellReturnAddressGateEffect,
                parentEffect.Address,
                parentEffect.Width,
                null,
                parentEffect.Value,
                parentEffect.Attempted,
                parentEffect.Committed || parentEffect.Value.HasValue,
                parentEffect.Committed,
                ConvertFlags(parentEffect.FlagsAfter)));
        }

        bool equalBranch = parent.BranchTaken == true;
        uint candidateEntry = equalBranch ? EqualEntryRawFileOffset : NotEqualEntryRawFileOffset;
        bool admitted = parent.ExactCleanCompletion
            && parent.Outcome
                == NetworkDispatcherCellReturnAddressGateOutcome
                    .BoundedBeforeEitherBranchDestination
            && parent.BranchReached
            && parent.BranchTaken.HasValue
            && parent.SelectedDestinationRawFileOffset == candidateEntry
            && parent.StopAtRawFileOffsetExclusive == candidateEntry
            && !parent.CellTargetReadAttempted
            && !parent.CellCallAttempted
            && !parent.NativeInstructionsExecuted;
        uint entry = admitted ? candidateEntry : parent.StopAtRawFileOffsetExclusive;
        var reached = ImmutableArray.CreateBuilder<uint>();
        var stackState = UnreachedOrUnused(capture.FreshStackScalarDWORD.HasValue);
        var fieldState = UnreachedOrUnused(capture.FreshFieldScalarDWORD.HasValue);
        var ebxState = UnreachedOrUnused(capture.FreshPostCellEBX.HasValue);
        var counterState = UnreachedOrUnused(capture.FreshCounterDWORD.HasValue);
        NetworkDispatcherFencedPublicationOutcome outcome;
        uint? stackAddress = null;
        uint? fieldAddress = parent.FieldAddress;
        uint? stackValue = null;
        bool fieldStoreAttempted = false;
        bool fieldStoreCommitted = false;
        uint? freshFieldValue = null;
        uint? byteAddress = null;
        byte? byteValue = null;
        bool byteStoreCommitted = false;
        uint? counterAddress = null;
        uint? counterBefore = null;
        uint? counterAfter = null;
        bool counterAttempted = false;
        bool counterCommitted = false;
        NetworkDispatcherFencedPublicationFlags? flagsAtBranch =
            ConvertFlags(parent.FlagsAtStop);
        NetworkDispatcherFencedPublicationFlags? flagsAfterFences = null;
        NetworkDispatcherFencedPublicationFlags? flagsAfterStackRead = null;
        NetworkDispatcherFencedPublicationFlags? flagsAfterFieldStore = null;
        NetworkDispatcherFencedPublicationFlags? flagsAfterFieldRead = null;
        NetworkDispatcherFencedPublicationFlags? flagsAfterByteStore = null;
        NetworkDispatcherFencedPublicationFlags? flagsAfterCounter = null;
        NetworkDispatcherFencedPublicationFlags? flagsAtStop = flagsAtBranch;
        uint stopAt = admitted ? entry : parent.StopAtRawFileOffsetExclusive;

        if (!admitted)
        {
            outcome = NetworkDispatcherFencedPublicationOutcome.ParentHandoffNotAdmitted;
        }
        else
        {
            NetworkDispatcherFencedPublicationFlags? unchangedFlags = flagsAtBranch;
            if (!equalBranch)
            {
                reached.Add(SFENCERawFileOffset);
                effects.Add(Effect(
                    SFENCERawFileOffset,
                    NetworkDispatcherFencedPublicationOrderedEffectKind.SFENCEObservation,
                    null,
                    0,
                    null,
                    null,
                    attempted: false,
                    captured: true,
                    committed: false,
                    unchangedFlags));
                reached.Add(LFENCERawFileOffset);
                effects.Add(Effect(
                    LFENCERawFileOffset,
                    NetworkDispatcherFencedPublicationOrderedEffectKind.LFENCEObservation,
                    null,
                    0,
                    null,
                    null,
                    attempted: false,
                    captured: true,
                    committed: false,
                    unchangedFlags));
                flagsAfterFences = unchangedFlags;

                reached.Add(StackScalarReadRawFileOffset);
                stackAddress = parent.Capture.FreshPostCellEBP is uint ebp
                    ? unchecked(ebp - StackDisplacement)
                    : null;
                stackState = State(capture.FreshStackScalarDWORD);
                bool stackCaptured = capture.FreshStackScalarDWORD.HasValue;
                stackValue = capture.FreshStackScalarDWORD;
                effects.Add(Effect(
                    StackScalarReadRawFileOffset,
                    NetworkDispatcherFencedPublicationOrderedEffectKind.FreshStackScalarRead,
                    stackAddress,
                    4,
                    stackValue,
                    stackValue,
                    attempted: true,
                    captured: stackCaptured,
                    committed: false,
                    unchangedFlags));
                flagsAfterStackRead = unchangedFlags;
                flagsAtStop = flagsAfterStackRead;
                if (!stackCaptured)
                {
                    fieldState = UnreachedOrUnused(capture.FreshFieldScalarDWORD.HasValue);
                    ebxState = UnreachedOrUnused(capture.FreshPostCellEBX.HasValue);
                    counterState = UnreachedOrUnused(capture.FreshCounterDWORD.HasValue);
                    outcome =
                        NetworkDispatcherFencedPublicationOutcome.MissingFreshStackScalarDWORD;
                    stopAt = StackScalarReadRawFileOffset;
                    return BuildPlan();
                }

                fieldStoreAttempted = true;
                fieldStoreCommitted = true;
                reached.Add(FieldScalarStoreRawFileOffset);
                effects.Add(Effect(
                    FieldScalarStoreRawFileOffset,
                    NetworkDispatcherFencedPublicationOrderedEffectKind.FieldScalarStore,
                    fieldAddress,
                    4,
                    null,
                    stackValue,
                    attempted: true,
                    captured: true,
                    committed: true,
                    unchangedFlags));
                flagsAfterFieldStore = unchangedFlags;
            }

            reached.Add(FieldScalarReadRawFileOffset);
            fieldState = State(capture.FreshFieldScalarDWORD);
            bool fieldCaptured = capture.FreshFieldScalarDWORD.HasValue;
            freshFieldValue = capture.FreshFieldScalarDWORD;
            effects.Add(Effect(
                FieldScalarReadRawFileOffset,
                NetworkDispatcherFencedPublicationOrderedEffectKind.FreshFieldScalarRead,
                fieldAddress,
                4,
                freshFieldValue,
                freshFieldValue,
                attempted: true,
                captured: fieldCaptured,
                committed: false,
                unchangedFlags));
            flagsAfterFieldRead = unchangedFlags;
            flagsAtStop = flagsAfterFieldRead;
            if (!fieldCaptured)
            {
                ebxState = UnreachedOrUnused(capture.FreshPostCellEBX.HasValue);
                counterState = UnreachedOrUnused(capture.FreshCounterDWORD.HasValue);
                outcome =
                    NetworkDispatcherFencedPublicationOutcome.MissingFreshFieldScalarDWORD;
                stopAt = FieldScalarReadRawFileOffset;
                return BuildPlan();
            }

            reached.Add(PublicationByteStoreRawFileOffset);
            byteAddress = parent.Capture.FreshPostCellESI is uint esi
                ? unchecked(esi + PublicationByteOffsetFromESI)
                : null;
            byteValue = 1;
            byteStoreCommitted = true;
            effects.Add(Effect(
                PublicationByteStoreRawFileOffset,
                NetworkDispatcherFencedPublicationOrderedEffectKind.PublicationByteStore,
                byteAddress,
                1,
                null,
                1,
                attempted: true,
                captured: true,
                committed: true,
                unchangedFlags));
            flagsAfterByteStore = unchangedFlags;

            reached.Add(CounterDecrementRawFileOffset);
            counterAttempted = true;
            ebxState = State(capture.FreshPostCellEBX);
            counterAddress = capture.FreshPostCellEBX is uint ebx
                ? unchecked(ebx + CounterOffsetFromEBX)
                : null;
            counterState = capture.FreshPostCellEBX.HasValue
                ? State(capture.FreshCounterDWORD)
                : UnreachedOrUnused(capture.FreshCounterDWORD.HasValue);
            bool counterCaptured = capture.FreshPostCellEBX.HasValue
                && capture.FreshCounterDWORD is uint;
            if (counterCaptured)
            {
                counterBefore = capture.FreshCounterDWORD;
                counterAfter = unchecked(counterBefore.Value - 1);
                counterCommitted = true;
                flagsAfterCounter = DecrementFlags32(
                    counterBefore.Value,
                    counterAfter.Value,
                    flagsAtBranch);
                flagsAtStop = flagsAfterCounter;
            }
            else
            {
                flagsAfterCounter = unchangedFlags;
                flagsAtStop = flagsAfterCounter;
            }

            effects.Add(Effect(
                CounterDecrementRawFileOffset,
                NetworkDispatcherFencedPublicationOrderedEffectKind.CounterDecrementReadWrite,
                counterAddress,
                4,
                counterBefore,
                counterAfter,
                attempted: true,
                captured: counterCaptured,
                committed: counterCommitted,
                flagsAfterCounter));
            if (capture.FreshPostCellEBX is null)
            {
                outcome =
                    NetworkDispatcherFencedPublicationOutcome.MissingFreshPostCellEBX;
                stopAt = CounterDecrementRawFileOffset;
            }
            else if (!counterCaptured)
            {
                outcome =
                    NetworkDispatcherFencedPublicationOutcome.MissingFreshCounterDWORD;
                stopAt = CounterDecrementRawFileOffset;
            }
            else
            {
                outcome = NetworkDispatcherFencedPublicationOutcome.BoundedBeforeContinuation;
                stopAt = ContinuationRawFileOffset;
            }
        }

        return BuildPlan();

        NetworkDispatcherFencedPublicationPlan BuildPlan() =>
            new(
                parent,
                capture,
                outcome,
                effects.ToImmutable(),
                reached.ToImmutable(),
                new(stackState, fieldState, ebxState, counterState),
                admitted,
                parent.BranchTaken,
                entry,
                stackAddress,
                fieldAddress,
                stackValue,
                fieldStoreAttempted,
                fieldStoreCommitted,
                freshFieldValue,
                byteAddress,
                byteValue,
                byteStoreCommitted,
                counterAddress,
                counterBefore,
                counterAfter,
                counterAttempted,
                counterCommitted,
                flagsAtBranch,
                flagsAfterFences,
                flagsAfterStackRead,
                flagsAfterFieldStore,
                flagsAfterFieldRead,
                flagsAfterByteStore,
                flagsAfterCounter,
                flagsAtStop,
                stopAt);

        NetworkDispatcherFencedPublicationOrderedEffect Effect(
            uint offset,
            NetworkDispatcherFencedPublicationOrderedEffectKind kind,
            uint? address,
            byte width,
            uint? readValue,
            uint? value,
            bool attempted,
            bool captured,
            bool committed,
            NetworkDispatcherFencedPublicationFlags? flags) =>
            new(
                effects.Count,
                null,
                offset,
                kind,
                address,
                width,
                readValue,
                value,
                attempted,
                captured,
                committed,
                flags);
    }

    private static NetworkDispatcherFencedPublicationCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherFencedPublicationCaptureState.Captured
            : NetworkDispatcherFencedPublicationCaptureState.Missing;

    private static NetworkDispatcherFencedPublicationCaptureState UnreachedOrUnused(
        bool supplied) =>
        supplied
            ? NetworkDispatcherFencedPublicationCaptureState.SuppliedUnused
            : NetworkDispatcherFencedPublicationCaptureState.Unreached;

    private static NetworkDispatcherFencedPublicationFlags? ConvertFlags(
        NetworkDispatcherCellReturnAddressGateFlags? flags) =>
        flags is NetworkDispatcherCellReturnAddressGateFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static NetworkDispatcherFencedPublicationFlags DecrementFlags32(
        uint before,
        uint after,
        NetworkDispatcherFencedPublicationFlags? previous)
    {
        uint result = after;
        return new(
            Carry: previous?.Carry,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            Auxiliary: (before & 0xF) == 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: before == 0x80000000);
    }
}
