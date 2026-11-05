using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketCompletionErrorStateCapture(
    bool? PostLogReleaseContinuationValidated,
    uint? InheritedEntryEsi,
    uint? AtomicObservedDword,
    uint? FreshField108,
    bool? QueueAppendReturnedNormally);

public enum GameSocketCompletionErrorStateCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketCompletionErrorStateCaptureKind
{
    PostLogReleaseContinuationValidated,
    InheritedEntryEsi,
    AtomicObservedDword,
    FreshField108,
    QueueAppendReturnedNormally
}

public readonly record struct GameSocketCompletionErrorStateCaptureStates(
    GameSocketCompletionErrorStateCaptureState PostLogReleaseContinuationValidated,
    GameSocketCompletionErrorStateCaptureState InheritedEntryEsi,
    GameSocketCompletionErrorStateCaptureState AtomicObservedDword,
    GameSocketCompletionErrorStateCaptureState FreshField108,
    GameSocketCompletionErrorStateCaptureState QueueAppendReturnedNormally);

public readonly record struct GameSocketCompletionErrorStateFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct GameSocketCompletionErrorStateWrite(
    int FieldOffset,
    uint Address,
    uint Value,
    uint InstructionRawFileOffset);

public enum GameSocketCompletionErrorStateMemoryEffectKind
{
    AtomicStateRead,
    AtomicConditionalStateWrite,
    FreshFieldRead,
    CopyDestinationWrite
}

public readonly record struct GameSocketCompletionErrorStateMemoryEffect(
    GameSocketCompletionErrorStateMemoryEffectKind Kind,
    int FieldOffset,
    uint Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool? Committed,
    uint InstructionRawFileOffset);

public readonly record struct GameSocketCompletionErrorStateAddresses(
    uint? DerivedStateSlotAddress,
    uint? StateSlotAddressAtAtomicOperation,
    uint? DerivedFreshFieldAddress,
    uint? FreshFieldAddressAtRead,
    uint? DerivedCopyDestinationAddress,
    uint? CopyDestinationAddressAtWrite,
    uint? DerivedQueueObjectAddress,
    uint? QueueObjectAddressAtCallSite);

public enum GameSocketCompletionErrorStateOutcome
{
    MissingContinuationValidation,
    ContinuationNotValidated,
    MissingInheritedEntryEsi,
    MissingAtomicObservation,
    MissingFreshField108,
    MissingQueueAppendReturnObservation,
    QueueAppendDidNotReturnNormally,
    BoundedAtQueueAppendReturn
}

public enum GameSocketCompletionErrorStateDisposition
{
    BoundedBeforeInternalContinuation,
    BoundedBeforeEsiAddressCalculation,
    BoundedAtAtomicObservation,
    BoundedAtFreshFieldRead,
    BoundedAtQueueAppendCallSite,
    BoundedBeforePostCallContinuation,
    BoundedAfterQueueAppendReturn
}

public sealed class GameSocketCompletionErrorStatePlan
{
    internal GameSocketCompletionErrorStatePlan(
        GameSocketCompletionErrorStateCaptureStates captureStates,
        ImmutableArray<GameSocketCompletionErrorStateCaptureKind> unusedCaptures,
        ImmutableArray<GameSocketCompletionErrorStateWrite> writes,
        ImmutableArray<GameSocketCompletionErrorStateMemoryEffect> memoryEffects,
        ImmutableArray<uint> reachedInstructionRawFileOffsets,
        GameSocketCompletionErrorStateOutcome outcome,
        GameSocketCompletionErrorStateDisposition disposition,
        bool atomicOperationAttempted,
        bool? stateCompareExchangeWriteCommitted,
        bool? zeroTestBranchTaken,
        bool queueAppendCallSiteReached,
        bool? queueAppendReturnedNormally,
        GameSocketCompletionErrorStateAddresses addresses,
        GameSocketCompletionErrorStateAddresses derivedAddresses,
        GameSocketCompletionErrorStateFlags? flagsBeforeAtomicOperation,
        GameSocketCompletionErrorStateFlags? flagsAfterCompareExchange,
        GameSocketCompletionErrorStateFlags? flagsAfterTestEax,
        GameSocketCompletionErrorStateFlags? flagsAtStop,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset)
    {
        CaptureStates = captureStates;
        UnusedCaptures = unusedCaptures;
        Writes = writes;
        MemoryEffects = memoryEffects;
        ReachedInstructionRawFileOffsets = reachedInstructionRawFileOffsets;
        Outcome = outcome;
        Disposition = disposition;
        AtomicOperationAttempted = atomicOperationAttempted;
        StateCompareExchangeWriteCommitted = stateCompareExchangeWriteCommitted;
        ZeroTestBranchTaken = zeroTestBranchTaken;
        QueueAppendCallSiteReached = queueAppendCallSiteReached;
        QueueAppendReturnedNormally = queueAppendReturnedNormally;
        Addresses = addresses;
        DerivedAddresses = derivedAddresses;
        FlagsBeforeAtomicOperation = flagsBeforeAtomicOperation;
        FlagsAfterCompareExchange = flagsAfterCompareExchange;
        FlagsAfterTestEax = flagsAfterTestEax;
        FlagsAtStop = flagsAtStop;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketCompletionErrorStateCaptureStates CaptureStates { get; }
    public ImmutableArray<GameSocketCompletionErrorStateCaptureKind> UnusedCaptures { get; }
    public ImmutableArray<GameSocketCompletionErrorStateWrite> Writes { get; }
    public ImmutableArray<GameSocketCompletionErrorStateMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketCompletionErrorStateMemoryEffect> OrderedMemoryEffects =>
        MemoryEffects;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets { get; }
    public GameSocketCompletionErrorStateOutcome Outcome { get; }
    public GameSocketCompletionErrorStateDisposition Disposition { get; }
    public bool AtomicOperationAttempted { get; }
    public bool? StateCompareExchangeWriteCommitted { get; }
    public bool AtomicReadAttempted => AtomicOperationAttempted;
    public bool AtomicReadEffectCaptured =>
        MemoryEffects.Any(effect =>
            effect.Kind == GameSocketCompletionErrorStateMemoryEffectKind.AtomicStateRead
            && effect.Captured);
    public bool FreshFieldReadAttempted =>
        MemoryEffects.Any(effect =>
            effect.Kind == GameSocketCompletionErrorStateMemoryEffectKind.FreshFieldRead
            && effect.Attempted);
    public bool FreshFieldReadEffectCaptured =>
        MemoryEffects.Any(effect =>
            effect.Kind == GameSocketCompletionErrorStateMemoryEffectKind.FreshFieldRead
            && effect.Captured);
    public bool? ZeroTestBranchTaken { get; }
    /// <summary>Reached the native CALL instruction; does not assert target entry.</summary>
    public bool QueueAppendCallSiteReached { get; }
    public bool? QueueAppendReturnedNormally { get; }
    public GameSocketCompletionErrorStateAddresses Addresses { get; }
    public GameSocketCompletionErrorStateAddresses DerivedAddresses { get; }
    public uint? EsiStateSlotAddress => Addresses.StateSlotAddressAtAtomicOperation;
    public uint? EsiFreshFieldAddress => Addresses.FreshFieldAddressAtRead;
    public uint? EsiCopyDestinationAddress => Addresses.CopyDestinationAddressAtWrite;
    public uint? QueueObjectAddress => Addresses.QueueObjectAddressAtCallSite;
    public uint? DerivedEsiStateSlotAddress => DerivedAddresses.DerivedStateSlotAddress;
    public uint? DerivedEsiFreshFieldAddress => DerivedAddresses.DerivedFreshFieldAddress;
    public uint? DerivedEsiCopyDestinationAddress =>
        DerivedAddresses.DerivedCopyDestinationAddress;
    public uint? DerivedQueueObjectAddress => DerivedAddresses.DerivedQueueObjectAddress;
    public GameSocketCompletionErrorStateFlags? FlagsBeforeAtomicOperation { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAfterCompareExchange { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAfterTestEax { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAtStop { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public bool ReachedBoundedPostCallContinuation =>
        Outcome == GameSocketCompletionErrorStateOutcome.BoundedAtQueueAppendReturn;
    public bool HasActualBoundedCompletion => ReachedBoundedPostCallContinuation;
    public bool IsCleanCompletion =>
        HasActualBoundedCompletion && UnusedCaptures.IsEmpty;
    public bool InternalContinuationEntered =>
        ReachedInstructionRawFileOffsets.Contains(
            GameSocketCompletionErrorStateProjection.ContinuationRawFileOffset);
}

/// <summary>
/// Projects only the internal post-log/release continuation. The caller's
/// error path and the queue append implementation are represented by captures,
/// not executed or inferred here.
/// </summary>
public static class GameSocketCompletionErrorStateProjection
{
    public const uint ContinuationRawFileOffset = 0x00BD2DEB;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint ContinuationVirtualAddress =
        ContinuationRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint StateCompareExchangeRawFileOffset = 0x00BD2DF8;
    public const uint TestEaxRawFileOffset = 0x00BD2DFC;
    public const uint ZeroTestBranchRawFileOffset = 0x00BD2DFE;
    public const uint FreshFieldReadRawFileOffset = 0x00BD2E00;
    public const uint CopyDestinationWriteRawFileOffset = 0x00BD2E06;
    public const uint QueueAppendCallRawFileOffset = 0x00BD2E14;
    public const uint PostQueueAppendContinuationRawFileOffset = 0x00BD2E19;
    public const uint QueueAppendTargetRawFileOffset = 0x012CE310;
    public const uint QueueAppendTargetVirtualAddress =
        QueueAppendTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint QueueAppendEventToken =
        (uint)GameSocketReceivedQueueEvent.PumpCloseRequest;
    public const uint StateCompareExchangeValue = 10;
    public const int StateFieldOffset = 0x124;
    public const int FreshFieldOffset = 0x108;
    public const int CopyDestinationFieldOffset = 0x128;
    public const int QueueObjectOffset = 0x260;

    private static readonly GameSocketCompletionErrorStateFlags FlagsAfterXorEaxZero =
        new(false, true, null, true, false, false);

    public static GameSocketCompletionErrorStatePlan Project(
        GameSocketCompletionErrorStateCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var writes = ImmutableArray.CreateBuilder<GameSocketCompletionErrorStateWrite>();
        var memoryEffects =
            ImmutableArray.CreateBuilder<GameSocketCompletionErrorStateMemoryEffect>();
        var unused = ImmutableArray.CreateBuilder<GameSocketCompletionErrorStateCaptureKind>();

        GameSocketCompletionErrorStateCaptureState continuationState;
        GameSocketCompletionErrorStateCaptureState esiState = NotReachedOrUnused(
            capture.InheritedEntryEsi.HasValue);
        GameSocketCompletionErrorStateCaptureState atomicState = NotReachedOrUnused(
            capture.AtomicObservedDword.HasValue);
        GameSocketCompletionErrorStateCaptureState freshState = NotReachedOrUnused(
            capture.FreshField108.HasValue);
        GameSocketCompletionErrorStateCaptureState queueState = NotReachedOrUnused(
            capture.QueueAppendReturnedNormally.HasValue);

        if (capture.PostLogReleaseContinuationValidated is null)
        {
            continuationState = GameSocketCompletionErrorStateCaptureState.Missing;
            MarkUnusedIfProvided(capture.InheritedEntryEsi,
                GameSocketCompletionErrorStateCaptureKind.InheritedEntryEsi, unused);
            MarkUnusedIfProvided(capture.AtomicObservedDword,
                GameSocketCompletionErrorStateCaptureKind.AtomicObservedDword, unused);
            MarkUnusedIfProvided(capture.FreshField108,
                GameSocketCompletionErrorStateCaptureKind.FreshField108, unused);
            MarkUnusedIfProvided(capture.QueueAppendReturnedNormally,
                GameSocketCompletionErrorStateCaptureKind.QueueAppendReturnedNormally, unused);
            return Stop(
                capture,
                new(continuationState, esiState, atomicState, freshState, queueState),
                unused,
                writes,
                memoryEffects,
                reached,
                GameSocketCompletionErrorStateOutcome.MissingContinuationValidation,
                GameSocketCompletionErrorStateDisposition.BoundedBeforeInternalContinuation,
                false, null, null, false, null,
                default, default, null,
                null, null, null,
                null, ContinuationRawFileOffset);
        }

        continuationState = GameSocketCompletionErrorStateCaptureState.Captured;
        if (!capture.PostLogReleaseContinuationValidated.Value)
        {
            MarkUnusedIfProvided(capture.InheritedEntryEsi,
                GameSocketCompletionErrorStateCaptureKind.InheritedEntryEsi, unused);
            MarkUnusedIfProvided(capture.AtomicObservedDword,
                GameSocketCompletionErrorStateCaptureKind.AtomicObservedDword, unused);
            MarkUnusedIfProvided(capture.FreshField108,
                GameSocketCompletionErrorStateCaptureKind.FreshField108, unused);
            MarkUnusedIfProvided(capture.QueueAppendReturnedNormally,
                GameSocketCompletionErrorStateCaptureKind.QueueAppendReturnedNormally, unused);
            return Stop(
                capture,
                new(continuationState, esiState, atomicState, freshState, queueState),
                unused,
                writes,
                memoryEffects,
                reached,
                GameSocketCompletionErrorStateOutcome.ContinuationNotValidated,
                GameSocketCompletionErrorStateDisposition.BoundedBeforeInternalContinuation,
                false, null, null, false, null,
                default, default, null,
                null, null, null,
                null, ContinuationRawFileOffset);
        }

        reached.Add(ContinuationRawFileOffset);
        if (capture.InheritedEntryEsi is null)
        {
            esiState = GameSocketCompletionErrorStateCaptureState.Missing;
            MarkUnusedIfProvided(capture.AtomicObservedDword,
                GameSocketCompletionErrorStateCaptureKind.AtomicObservedDword, unused);
            MarkUnusedIfProvided(capture.FreshField108,
                GameSocketCompletionErrorStateCaptureKind.FreshField108, unused);
            MarkUnusedIfProvided(capture.QueueAppendReturnedNormally,
                GameSocketCompletionErrorStateCaptureKind.QueueAppendReturnedNormally, unused);
            return Stop(
                capture,
                new(continuationState, esiState, atomicState, freshState, queueState),
                unused,
                writes,
                memoryEffects,
                reached,
                GameSocketCompletionErrorStateOutcome.MissingInheritedEntryEsi,
                GameSocketCompletionErrorStateDisposition.BoundedBeforeEsiAddressCalculation,
                false, null, null, false, null,
                default, default, null,
                null, null, null,
                ContinuationRawFileOffset, 0x00BD2DF0);
        }

        esiState = GameSocketCompletionErrorStateCaptureState.Captured;
        uint esi = capture.InheritedEntryEsi.Value;
        uint stateAddress = unchecked(esi + StateFieldOffset);
        uint freshAddress = unchecked(esi + FreshFieldOffset);
        uint destinationAddress = unchecked(esi + CopyDestinationFieldOffset);
        uint queueAddress = unchecked(esi + QueueObjectOffset);
        GameSocketCompletionErrorStateAddresses derivedAddresses = new(
            stateAddress, null, freshAddress, null,
            destinationAddress, null, queueAddress, null);
        GameSocketCompletionErrorStateAddresses addresses = default;
        reached.Add(0x00BD2DF0);
        reached.Add(0x00BD2DF6);

        reached.Add(StateCompareExchangeRawFileOffset);
        addresses = addresses with { StateSlotAddressAtAtomicOperation = stateAddress };
        if (capture.AtomicObservedDword is null)
        {
            atomicState = GameSocketCompletionErrorStateCaptureState.Missing;
            memoryEffects.Add(new(
                GameSocketCompletionErrorStateMemoryEffectKind.AtomicStateRead,
                StateFieldOffset, stateAddress, null, true, false, false,
                StateCompareExchangeRawFileOffset));
            memoryEffects.Add(new(
                GameSocketCompletionErrorStateMemoryEffectKind.AtomicConditionalStateWrite,
                StateFieldOffset, stateAddress, StateCompareExchangeValue, true, false, null,
                StateCompareExchangeRawFileOffset));
            MarkUnusedIfProvided(capture.FreshField108,
                GameSocketCompletionErrorStateCaptureKind.FreshField108, unused);
            MarkUnusedIfProvided(capture.QueueAppendReturnedNormally,
                GameSocketCompletionErrorStateCaptureKind.QueueAppendReturnedNormally, unused);
            return Stop(
                capture,
                new(continuationState, esiState, atomicState, freshState, queueState),
                unused,
                writes,
                memoryEffects,
                reached,
                GameSocketCompletionErrorStateOutcome.MissingAtomicObservation,
                GameSocketCompletionErrorStateDisposition.BoundedAtAtomicObservation,
                true, null, null, false, null,
                addresses, derivedAddresses, FlagsAfterXorEaxZero,
                null, null, FlagsAfterXorEaxZero,
                StateCompareExchangeRawFileOffset, StateCompareExchangeRawFileOffset);
        }

        atomicState = GameSocketCompletionErrorStateCaptureState.Captured;
        uint observedOldDword = capture.AtomicObservedDword.Value;
        memoryEffects.Add(new(
            GameSocketCompletionErrorStateMemoryEffectKind.AtomicStateRead,
            StateFieldOffset, stateAddress, observedOldDword, true, true, false,
            StateCompareExchangeRawFileOffset));
        GameSocketCompletionErrorStateFlags compareFlags =
            CompareZeroAgainstObservedOldDword(observedOldDword);
        bool writeCommitted = observedOldDword == 0;
        memoryEffects.Add(new(
            GameSocketCompletionErrorStateMemoryEffectKind.AtomicConditionalStateWrite,
            StateFieldOffset, stateAddress, StateCompareExchangeValue, true, true,
            writeCommitted, StateCompareExchangeRawFileOffset));
        if (writeCommitted)
        {
            writes.Add(new(StateFieldOffset, stateAddress, StateCompareExchangeValue,
                StateCompareExchangeRawFileOffset));
        }

        reached.Add(TestEaxRawFileOffset);
        uint eaxAfterCompareExchange = writeCommitted ? 0u : observedOldDword;
        GameSocketCompletionErrorStateFlags testFlags = TestFlags(eaxAfterCompareExchange);
        reached.Add(ZeroTestBranchRawFileOffset);
        bool branchTaken = eaxAfterCompareExchange != 0;

        if (!branchTaken)
        {
            reached.Add(FreshFieldReadRawFileOffset);
            addresses = addresses with { FreshFieldAddressAtRead = freshAddress };
            memoryEffects.Add(new(
                GameSocketCompletionErrorStateMemoryEffectKind.FreshFieldRead,
                FreshFieldOffset, freshAddress, capture.FreshField108, true,
                capture.FreshField108.HasValue, false, FreshFieldReadRawFileOffset));
            if (capture.FreshField108 is null)
            {
                freshState = GameSocketCompletionErrorStateCaptureState.Missing;
                MarkUnusedIfProvided(capture.QueueAppendReturnedNormally,
                    GameSocketCompletionErrorStateCaptureKind.QueueAppendReturnedNormally, unused);
                return Stop(
                    capture,
                    new(continuationState, esiState, atomicState, freshState, queueState),
                    unused,
                    writes,
                    memoryEffects,
                    reached,
                    GameSocketCompletionErrorStateOutcome.MissingFreshField108,
                    GameSocketCompletionErrorStateDisposition.BoundedAtFreshFieldRead,
                    true, writeCommitted, false, false, null,
                    addresses, derivedAddresses, FlagsAfterXorEaxZero,
                    compareFlags, testFlags, testFlags,
                    FreshFieldReadRawFileOffset, FreshFieldReadRawFileOffset);
            }

            freshState = GameSocketCompletionErrorStateCaptureState.Captured;
            reached.Add(CopyDestinationWriteRawFileOffset);
            addresses = addresses with
            {
                CopyDestinationAddressAtWrite = destinationAddress
            };
            memoryEffects.Add(new(
                GameSocketCompletionErrorStateMemoryEffectKind.CopyDestinationWrite,
                CopyDestinationFieldOffset, destinationAddress,
                capture.FreshField108.Value, true, true, true,
                CopyDestinationWriteRawFileOffset));
            writes.Add(new(CopyDestinationFieldOffset, destinationAddress,
                capture.FreshField108.Value, CopyDestinationWriteRawFileOffset));
        }
        else
        {
            freshState = capture.FreshField108.HasValue
                ? GameSocketCompletionErrorStateCaptureState.SuppliedButUnused
                : GameSocketCompletionErrorStateCaptureState.NotReached;
            if (capture.FreshField108.HasValue)
                unused.Add(GameSocketCompletionErrorStateCaptureKind.FreshField108);
        }

        reached.Add(0x00BD2E0C);
        reached.Add(0x00BD2E0E);
        addresses = addresses with { QueueObjectAddressAtCallSite = queueAddress };
        reached.Add(QueueAppendCallRawFileOffset);
        if (capture.QueueAppendReturnedNormally is null)
        {
            queueState = GameSocketCompletionErrorStateCaptureState.Missing;
            return Stop(
                capture,
                new(continuationState, esiState, atomicState, freshState, queueState),
                unused,
                writes,
                memoryEffects,
                reached,
                GameSocketCompletionErrorStateOutcome.MissingQueueAppendReturnObservation,
                GameSocketCompletionErrorStateDisposition.BoundedAtQueueAppendCallSite,
                true, writeCommitted, branchTaken, true, null,
                addresses, derivedAddresses, FlagsAfterXorEaxZero,
                compareFlags, testFlags, testFlags,
                QueueAppendCallRawFileOffset, PostQueueAppendContinuationRawFileOffset);
        }

        queueState = GameSocketCompletionErrorStateCaptureState.Captured;
        if (!capture.QueueAppendReturnedNormally.Value)
        {
            return Stop(
                capture,
                new(continuationState, esiState, atomicState, freshState, queueState),
                unused,
                writes,
                memoryEffects,
                reached,
                GameSocketCompletionErrorStateOutcome.QueueAppendDidNotReturnNormally,
                GameSocketCompletionErrorStateDisposition.BoundedBeforePostCallContinuation,
                true, writeCommitted, branchTaken, true, false,
                addresses, derivedAddresses, FlagsAfterXorEaxZero,
                compareFlags, testFlags, testFlags,
                QueueAppendCallRawFileOffset, PostQueueAppendContinuationRawFileOffset);
        }

        return Stop(
            capture,
            new(continuationState, esiState, atomicState, freshState, queueState),
            unused,
            writes,
            memoryEffects,
            reached,
            GameSocketCompletionErrorStateOutcome.BoundedAtQueueAppendReturn,
            GameSocketCompletionErrorStateDisposition.BoundedAfterQueueAppendReturn,
            true, writeCommitted, branchTaken, true, true,
            addresses, derivedAddresses, FlagsAfterXorEaxZero,
            compareFlags, testFlags, null,
            QueueAppendCallRawFileOffset, PostQueueAppendContinuationRawFileOffset);
    }

    private static GameSocketCompletionErrorStatePlan Stop(
        GameSocketCompletionErrorStateCapture capture,
        GameSocketCompletionErrorStateCaptureStates captureStates,
        ImmutableArray<GameSocketCompletionErrorStateCaptureKind>.Builder unused,
        ImmutableArray<GameSocketCompletionErrorStateWrite>.Builder writes,
        ImmutableArray<GameSocketCompletionErrorStateMemoryEffect>.Builder memoryEffects,
        ImmutableArray<uint>.Builder reached,
        GameSocketCompletionErrorStateOutcome outcome,
        GameSocketCompletionErrorStateDisposition disposition,
        bool atomicOperationAttempted,
        bool? stateCompareExchangeWriteCommitted,
        bool? zeroTestBranchTaken,
        bool queueAppendCallSiteReached,
        bool? queueAppendReturnedNormally,
        GameSocketCompletionErrorStateAddresses addresses,
        GameSocketCompletionErrorStateAddresses derivedAddresses,
        GameSocketCompletionErrorStateFlags? flagsBeforeAtomicOperation,
        GameSocketCompletionErrorStateFlags? flagsAfterCompareExchange,
        GameSocketCompletionErrorStateFlags? flagsAfterTestEax,
        GameSocketCompletionErrorStateFlags? flagsAtStop,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset)
    {
        return new(
            captureStates,
            unused.ToImmutable(),
            writes.ToImmutable(),
            memoryEffects.ToImmutable(),
            reached.ToImmutable(),
            outcome,
            disposition,
            atomicOperationAttempted,
            stateCompareExchangeWriteCommitted,
            zeroTestBranchTaken,
            queueAppendCallSiteReached,
            queueAppendReturnedNormally,
            addresses,
            derivedAddresses,
            flagsBeforeAtomicOperation,
            flagsAfterCompareExchange,
            flagsAfterTestEax,
            flagsAtStop,
            lastReachedRawFileOffset,
            stopBeforeRawFileOffset);
    }

    private static void MarkUnusedIfProvided<T>(
        T? value,
        GameSocketCompletionErrorStateCaptureKind kind,
        ImmutableArray<GameSocketCompletionErrorStateCaptureKind>.Builder unused)
        where T : struct
    {
        if (value.HasValue)
            unused.Add(kind);
    }

    private static GameSocketCompletionErrorStateCaptureState NotReachedOrUnused(
        bool captureWasProvided) =>
        captureWasProvided
            ? GameSocketCompletionErrorStateCaptureState.SuppliedButUnused
            : GameSocketCompletionErrorStateCaptureState.NotReached;

    private static GameSocketCompletionErrorStateFlags CompareZeroAgainstObservedOldDword(
        uint observedOldDword)
    {
        uint result = unchecked(0u - observedOldDword);
        return new(
            Carry: observedOldDword != 0,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: (observedOldDword & 0xF) != 0,
            Zero: observedOldDword == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: observedOldDword == 0x80000000);
    }

    private static GameSocketCompletionErrorStateFlags TestFlags(uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            AuxiliaryCarry: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;
}
