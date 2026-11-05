using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public enum GameSocketCompletionErrorQueueAcquisitionCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused,
}

public readonly record struct GameSocketCompletionErrorQueueAcquisitionChildCaptureStates(
    GameSocketCompletionErrorQueueAcquisitionCaptureState CounterOldValue,
    GameSocketCompletionErrorQueueAcquisitionCaptureState GlobalPoolPointer,
    GameSocketCompletionErrorQueueAcquisitionCaptureState AcquisitionResult);

public enum GameSocketCompletionErrorQueueAcquisitionOutcome
{
    ParentPreCallHandoffRejected,
    ChildAcquisitionStoppedAtCapture,
    ChildAcquisitionBranchProjected
}

public enum GameSocketCompletionErrorQueueAcquisitionDisposition
{
    BoundedAtParentObservation,
    BoundedInChildAcquisitionProjection
}

public readonly record struct GameSocketCompletionErrorQueueAcquisitionTraceStep(
    int Sequence,
    GameSocketCompletionErrorStateMemoryEffect? ParentMemoryEffect,
    QueueAppendAcquisitionAction? ChildAction);

public sealed class GameSocketCompletionErrorQueueAcquisitionPlan
{
    internal GameSocketCompletionErrorQueueAcquisitionPlan(
        GameSocketCompletionErrorStateCapture parentCapture,
        GameSocketCompletionErrorStatePlan parentPlan,
        QueueAppendAcquisitionCapture<uint> counterOldValueCapture,
        QueueAppendAcquisitionCapture<uint> globalPoolPointerCapture,
        QueueAppendAcquisitionCapture<uint> acquisitionResultCapture,
        QueueAppendAcquisitionPlan? childAcquisitionPlan,
        GameSocketCompletionErrorQueueAcquisitionChildCaptureStates childCaptureStates,
        ImmutableArray<GameSocketCompletionErrorQueueAcquisitionTraceStep> orderedTrace,
        GameSocketCompletionErrorQueueAcquisitionOutcome outcome,
        GameSocketCompletionErrorQueueAcquisitionDisposition disposition,
        bool cleanParentPreCallHandoffValidated,
        uint? derivedQueueReceiverPointer,
        GameSocketCompletionErrorStateFlags? parentCmpFlags,
        GameSocketCompletionErrorStateFlags? parentFlagsAtCallHandoff,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset,
        GameSocketCompletionErrorStateFlags? flagsAtStop,
        GameSocketCompletionErrorStateFlags? childFlagsAfterCounterXadd)
    {
        ParentCapture = parentCapture;
        ParentPlan = parentPlan;
        CounterOldValueCapture = counterOldValueCapture;
        GlobalPoolPointerCapture = globalPoolPointerCapture;
        AcquisitionResultCapture = acquisitionResultCapture;
        ChildAcquisitionPlan = childAcquisitionPlan;
        ChildCaptureStates = childCaptureStates;
        OrderedTrace = orderedTrace;
        Outcome = outcome;
        Disposition = disposition;
        CleanParentPreCallHandoffValidated = cleanParentPreCallHandoffValidated;
        DerivedQueueReceiverPointer = derivedQueueReceiverPointer;
        ParentCmpFlags = parentCmpFlags;
        ParentEflagsAtCallHandoff = parentFlagsAtCallHandoff;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        FlagsAtStop = flagsAtStop;
        ChildFlagsAfterCounterXadd = childFlagsAfterCounterXadd;
    }

    public GameSocketCompletionErrorStateCapture ParentCapture { get; }
    public GameSocketCompletionErrorStatePlan ParentPlan { get; }
    public QueueAppendAcquisitionCapture<uint> CounterOldValueCapture { get; }
    public QueueAppendAcquisitionCapture<uint> GlobalPoolPointerCapture { get; }
    public QueueAppendAcquisitionCapture<uint> AcquisitionResultCapture { get; }
    public QueueAppendAcquisitionPlan? ChildAcquisitionPlan { get; }
    public GameSocketCompletionErrorQueueAcquisitionChildCaptureStates ChildCaptureStates { get; }
    public ImmutableArray<GameSocketCompletionErrorQueueAcquisitionTraceStep> OrderedTrace { get; }
    public GameSocketCompletionErrorQueueAcquisitionOutcome Outcome { get; }
    public GameSocketCompletionErrorQueueAcquisitionDisposition Disposition { get; }
    public bool CleanParentPreCallHandoffValidated { get; }
    public uint? DerivedQueueReceiverPointer { get; }
    public GameSocketCompletionErrorStateFlags? ParentCmpFlags { get; }
    public GameSocketCompletionErrorStateFlags? ParentEflagsAtCallHandoff { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAtStop { get; }
    public GameSocketCompletionErrorStateFlags? ChildFlagsAfterCounterXadd { get; }

    /// <summary>The native helper body is projected from captures; it is never executed here.</summary>
    public bool ChildHelperInstructionsProjected => ChildAcquisitionPlan is not null;
    public bool ChildNativeInstructionsExecuted => false;
    public bool ChildQueueAppendEntryObserved => false;
    /// <summary>A normal parent return implies target execution, but does not add a direct entry capture.</summary>
    public bool ChildQueueAppendEntryImpliedByNormalParentReturn =>
        ParentPlan.QueueAppendReturnedNormally == true;
    public bool ChildAcquisitionNativeCallExecuted => false;
    public bool ChildNodeFieldsRead => false;
    public GameSocketCompletionErrorStateFlags? ChildFlagsAfterPrologue => null;
    public bool ChildAcquisitionReachedBoundedBoundary =>
        ChildAcquisitionPlan is { } childPlan
        && (childPlan.Outcome is
            QueueAppendAcquisitionOutcome.NullResultBranchProjected
            or QueueAppendAcquisitionOutcome.NonzeroResultBranchProjected)
        && StopBeforeRawFileOffset
            == GameSocketCompletionErrorQueueAcquisitionProjection
                .ChildStopBeforeFirstTailTagReadRawFileOffset;
    public bool ReachedBoundedProjectionBoundary =>
        ChildAcquisitionReachedBoundedBoundary
        || (!CleanParentPreCallHandoffValidated && ParentPlan.HasActualBoundedCompletion);
    public bool IsCleanCompletion =>
        CleanParentPreCallHandoffValidated
        && ChildAcquisitionReachedBoundedBoundary
        && ChildCaptureStates is
        {
            CounterOldValue: GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured,
            GlobalPoolPointer: GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured,
            AcquisitionResult: GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured
        };

    public bool? ParentQueueAppendReturnedNormally =>
        ParentPlan.QueueAppendReturnedNormally;
    public bool ParentQueueAppendReturnObserved =>
        ParentPlan.QueueAppendReturnedNormally.HasValue;
    public bool ParentPostCallContinuationEntered =>
        ParentPlan.ReachedBoundedPostCallContinuation;
    public bool HasActualBoundedCompletion =>
        ParentPlan.HasActualBoundedCompletion;

    public uint? QueueToken =>
        ParentPlan.QueueAppendCallSiteReached
            ? GameSocketCompletionErrorStateProjection.QueueAppendEventToken
            : null;

    public uint? ParentQueueAppendTargetRawFileOffset =>
        ParentPlan.QueueAppendCallSiteReached
            ? GameSocketCompletionErrorStateProjection.QueueAppendTargetRawFileOffset
            : null;

    public uint? ParentQueueAppendTargetVirtualAddress =>
        ParentPlan.QueueAppendCallSiteReached
            ? GameSocketCompletionErrorStateProjection.QueueAppendTargetVirtualAddress
            : null;

    public uint? FirstTailTagReadRawFileOffset =>
        CleanParentPreCallHandoffValidated
            ? QueueAppendAttemptProjection.TagCaptureRawFileOffset
            : null;

    public bool FirstTailTagReadNotReached =>
        ChildAcquisitionPlan is not null;
}

/// <summary>
/// Joins the completion-error state writes to the captured entry of the
/// existing queue-acquisition projection. A captured acquisition result
/// assumes the opaque call returned to its result store; no separate call-return
/// capture exists. Missing results stop at that store boundary. The projection
/// stops before the first tail-tag read and executes no native code.
/// </summary>
public static class GameSocketCompletionErrorQueueAcquisitionProjection
{
    public const uint ParentQueueAppendCallRawFileOffset =
        GameSocketCompletionErrorStateProjection.QueueAppendCallRawFileOffset;
    public const uint ParentQueueAppendTargetRawFileOffset =
        GameSocketCompletionErrorStateProjection.QueueAppendTargetRawFileOffset;
    public const uint ParentQueueAppendTargetVirtualAddress =
        GameSocketCompletionErrorStateProjection.QueueAppendTargetVirtualAddress;
    public const uint ChildCounterXaddRawFileOffset =
        QueueAppendAcquisitionProjection.ReceiverCounterIncrementRawFileOffset;
    public const uint ChildAcquisitionCallRawFileOffset =
        QueueAppendAcquisitionProjection.AcquisitionCallRawFileOffset;
    public const uint ChildAcquisitionTargetRawFileOffset =
        QueueAppendAcquisitionProjection.AcquisitionFunctionRawFileOffset;
    public const uint ChildAcquisitionTargetVirtualAddress =
        QueueAppendAcquisitionProjection.AcquisitionFunctionVirtualAddress;
    public const uint ChildStopBeforeFirstTailTagReadRawFileOffset =
        QueueAppendAttemptProjection.TagCaptureRawFileOffset;

    public static GameSocketCompletionErrorQueueAcquisitionPlan Project(
        GameSocketCompletionErrorStateCapture parentCapture,
        QueueAppendAcquisitionCapture<uint> counterOldValueCapture,
        QueueAppendAcquisitionCapture<uint> globalPoolPointerCapture,
        QueueAppendAcquisitionCapture<uint> acquisitionResultCapture)
    {
        GameSocketCompletionErrorStatePlan parentPlan =
            GameSocketCompletionErrorStateProjection.Project(parentCapture);
        bool cleanHandoff = IsCleanPreCallHandoff(parentCapture, parentPlan);
        var parentEffects = parentPlan.OrderedMemoryEffects;
        var trace = ImmutableArray.CreateBuilder<GameSocketCompletionErrorQueueAcquisitionTraceStep>(
            parentEffects.Length + 16);

        for (int i = 0; i < parentEffects.Length; i++)
        {
            trace.Add(new(
                trace.Count,
                parentEffects[i],
                null));
        }

        if (!cleanHandoff)
        {
            return new(
                parentCapture,
                parentPlan,
                counterOldValueCapture,
                globalPoolPointerCapture,
                acquisitionResultCapture,
                null,
                new(
                    ChildStateBeforeAdmission(counterOldValueCapture),
                    ChildStateBeforeAdmission(globalPoolPointerCapture),
                    ChildStateBeforeAdmission(acquisitionResultCapture)),
                trace.ToImmutable(),
                GameSocketCompletionErrorQueueAcquisitionOutcome.ParentPreCallHandoffRejected,
                GameSocketCompletionErrorQueueAcquisitionDisposition.BoundedAtParentObservation,
                false,
                null,
                null,
                null,
                parentPlan.LastReachedRawFileOffset,
                parentPlan.StopBeforeRawFileOffset,
                parentPlan.FlagsAtStop,
                null);
        }

        uint receiverPointer = parentPlan.QueueObjectAddress.GetValueOrDefault();
        QueueAppendAcquisitionCapture<uint> effectiveGlobalPoolCapture =
            counterOldValueCapture.Kind == QueueAppendAcquisitionCaptureKind.Missing
                ? QueueAppendAcquisitionCapture<uint>.Unreached()
                : globalPoolPointerCapture;
        QueueAppendAcquisitionCapture<uint> effectiveAcquisitionResultCapture =
            counterOldValueCapture.Kind == QueueAppendAcquisitionCaptureKind.Missing
                || (counterOldValueCapture.Kind == QueueAppendAcquisitionCaptureKind.Captured
                    && globalPoolPointerCapture.Kind == QueueAppendAcquisitionCaptureKind.Missing)
                ? QueueAppendAcquisitionCapture<uint>.Unreached()
                : acquisitionResultCapture;
        var childCapture = new QueueAppendAcquisitionCaptures(
            receiverPointer,
            counterOldValueCapture,
            effectiveGlobalPoolCapture,
            effectiveAcquisitionResultCapture);
        QueueAppendAcquisitionPlan childPlan =
            QueueAppendAcquisitionProjection.Project(childCapture);

        foreach (QueueAppendAcquisitionAction action in childPlan.Actions)
        {
            trace.Add(new(
                trace.Count,
                null,
                action));
        }

        bool childProjectedToBoundary =
            childPlan.Outcome is QueueAppendAcquisitionOutcome.NullResultBranchProjected
                or QueueAppendAcquisitionOutcome.NonzeroResultBranchProjected;
        (uint? childLastReached, uint? childStopBefore) =
            ChildBoundaries(childPlan);
        GameSocketCompletionErrorStateFlags? childXaddFlags =
            HasAction(
                childPlan,
                QueueAppendAcquisitionActionKind.IncrementReceiverCounterAndCaptureOldValue)
                ? AddOneFlags(childPlan.CounterOldValue!.Value)
                : null;

        return new(
            parentCapture,
            parentPlan,
            counterOldValueCapture,
            globalPoolPointerCapture,
            acquisitionResultCapture,
            childPlan,
            ChildStatesAfterProjection(
                childPlan,
                globalPoolPointerCapture,
                acquisitionResultCapture),
            trace.ToImmutable(),
            childProjectedToBoundary
                ? GameSocketCompletionErrorQueueAcquisitionOutcome.ChildAcquisitionBranchProjected
                : GameSocketCompletionErrorQueueAcquisitionOutcome.ChildAcquisitionStoppedAtCapture,
            GameSocketCompletionErrorQueueAcquisitionDisposition.BoundedInChildAcquisitionProjection,
            true,
            receiverPointer,
            parentPlan.FlagsAfterCompareExchange,
            parentPlan.FlagsAfterTestEax,
            childLastReached,
            childStopBefore,
            ChildFlagsAtStop(childPlan, childXaddFlags),
            childXaddFlags);
    }

    private static bool IsCleanPreCallHandoff(
        GameSocketCompletionErrorStateCapture capture,
        GameSocketCompletionErrorStatePlan plan)
    {
        if (capture.PostLogReleaseContinuationValidated != true
            || capture.InheritedEntryEsi is null
            || capture.AtomicObservedDword is null
            || capture.QueueAppendReturnedNormally is not null
            || plan.Outcome
                != GameSocketCompletionErrorStateOutcome.MissingQueueAppendReturnObservation
            || plan.Disposition
                != GameSocketCompletionErrorStateDisposition.BoundedAtQueueAppendCallSite
            || !plan.QueueAppendCallSiteReached
            || !plan.AtomicOperationAttempted
            || !plan.AtomicReadEffectCaptured
            || plan.QueueAppendReturnedNormally is not null
            || plan.CaptureStates.QueueAppendReturnedNormally
                != GameSocketCompletionErrorStateCaptureState.Missing
            || !plan.UnusedCaptures.IsEmpty
            || plan.LastReachedRawFileOffset != ParentQueueAppendCallRawFileOffset
            || plan.StopBeforeRawFileOffset
                != GameSocketCompletionErrorStateProjection.PostQueueAppendContinuationRawFileOffset
            || plan.ReachedInstructionRawFileOffsets.IsEmpty
            || plan.ReachedInstructionRawFileOffsets[^1] != ParentQueueAppendCallRawFileOffset
            || plan.FlagsAtStop != plan.FlagsAfterTestEax
            || plan.FlagsAfterTestEax is null)
        {
            return false;
        }

        uint esi = capture.InheritedEntryEsi.Value;
        uint oldState = capture.AtomicObservedDword.Value;
        bool stateWriteCommitted = oldState == 0;
        uint stateAddress = unchecked(
            esi + GameSocketCompletionErrorStateProjection.StateFieldOffset);
        uint freshAddress = unchecked(
            esi + GameSocketCompletionErrorStateProjection.FreshFieldOffset);
        uint copyAddress = unchecked(
            esi + GameSocketCompletionErrorStateProjection.CopyDestinationFieldOffset);
        uint queueAddress = unchecked(
            esi + GameSocketCompletionErrorStateProjection.QueueObjectOffset);

        if (plan.StateCompareExchangeWriteCommitted != stateWriteCommitted
            || plan.ZeroTestBranchTaken != !stateWriteCommitted
            || plan.EsiStateSlotAddress != stateAddress
            || plan.QueueObjectAddress != queueAddress
            || plan.DerivedEsiStateSlotAddress != stateAddress
            || plan.DerivedEsiFreshFieldAddress != freshAddress
            || plan.DerivedEsiCopyDestinationAddress != copyAddress
            || plan.DerivedQueueObjectAddress != queueAddress
            || plan.MemoryEffects.Length != (stateWriteCommitted ? 4 : 2)
            || plan.Writes.Length != (stateWriteCommitted ? 2 : 0))
        {
            return false;
        }

        if (plan.MemoryEffects[0] is not
            {
                Kind: GameSocketCompletionErrorStateMemoryEffectKind.AtomicStateRead,
                FieldOffset: GameSocketCompletionErrorStateProjection.StateFieldOffset,
                Value: not null,
                Attempted: true,
                Captured: true,
                Committed: false,
                InstructionRawFileOffset:
                    GameSocketCompletionErrorStateProjection.StateCompareExchangeRawFileOffset
            } atomicRead
            || atomicRead.Address != stateAddress
            || atomicRead.Value != oldState
            || plan.MemoryEffects[1] is not
            {
                Kind: GameSocketCompletionErrorStateMemoryEffectKind.AtomicConditionalStateWrite,
                FieldOffset: GameSocketCompletionErrorStateProjection.StateFieldOffset,
                Value: GameSocketCompletionErrorStateProjection.StateCompareExchangeValue,
                Attempted: true,
                Captured: true,
                InstructionRawFileOffset:
                    GameSocketCompletionErrorStateProjection.StateCompareExchangeRawFileOffset
            } stateWrite
            || stateWrite.Address != stateAddress
            || stateWrite.Committed != stateWriteCommitted)
        {
            return false;
        }

        if (!stateWriteCommitted)
        {
            return capture.FreshField108 is null
                && plan.CaptureStates.FreshField108
                    == GameSocketCompletionErrorStateCaptureState.NotReached
                && plan.EsiFreshFieldAddress is null
                && plan.EsiCopyDestinationAddress is null;
        }

        return capture.FreshField108.HasValue
            && plan.CaptureStates.FreshField108
                == GameSocketCompletionErrorStateCaptureState.Captured
            && plan.EsiFreshFieldAddress == freshAddress
            && plan.EsiCopyDestinationAddress == copyAddress
            && plan.MemoryEffects[2] is
            {
                Kind: GameSocketCompletionErrorStateMemoryEffectKind.FreshFieldRead,
                FieldOffset: GameSocketCompletionErrorStateProjection.FreshFieldOffset,
                Value: not null,
                Attempted: true,
                Captured: true,
                Committed: false,
                InstructionRawFileOffset:
                    GameSocketCompletionErrorStateProjection.FreshFieldReadRawFileOffset
            } freshRead
            && freshRead.Address == freshAddress
            && freshRead.Value == capture.FreshField108.Value
            && plan.MemoryEffects[3] is
            {
                Kind: GameSocketCompletionErrorStateMemoryEffectKind.CopyDestinationWrite,
                FieldOffset: GameSocketCompletionErrorStateProjection.CopyDestinationFieldOffset,
                Value: not null,
                Attempted: true,
                Captured: true,
                Committed: true,
                InstructionRawFileOffset:
                    GameSocketCompletionErrorStateProjection.CopyDestinationWriteRawFileOffset
            } copyWrite
            && copyWrite.Address == copyAddress
            && copyWrite.Value == capture.FreshField108.Value
            && plan.Writes[0] == new GameSocketCompletionErrorStateWrite(
                GameSocketCompletionErrorStateProjection.StateFieldOffset,
                stateAddress,
                GameSocketCompletionErrorStateProjection.StateCompareExchangeValue,
                GameSocketCompletionErrorStateProjection.StateCompareExchangeRawFileOffset)
            && plan.Writes[1] == new GameSocketCompletionErrorStateWrite(
                GameSocketCompletionErrorStateProjection.CopyDestinationFieldOffset,
                copyAddress,
                capture.FreshField108.Value,
                GameSocketCompletionErrorStateProjection.CopyDestinationWriteRawFileOffset);
    }

    private static GameSocketCompletionErrorQueueAcquisitionCaptureState
        ChildStateBeforeAdmission(QueueAppendAcquisitionCapture<uint> capture) =>
        capture.Kind == QueueAppendAcquisitionCaptureKind.Captured
            ? GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused
            : GameSocketCompletionErrorQueueAcquisitionCaptureState.NotReached;

    private static GameSocketCompletionErrorQueueAcquisitionChildCaptureStates
        ChildStatesAfterProjection(
            QueueAppendAcquisitionPlan plan,
            QueueAppendAcquisitionCapture<uint> globalPoolCapture,
            QueueAppendAcquisitionCapture<uint> acquisitionResultCapture)
    {
        bool counterReached = HasAction(
            plan,
            QueueAppendAcquisitionActionKind.IncrementReceiverCounterAndCaptureOldValue);
        bool counterMissing = HasAction(
            plan,
            QueueAppendAcquisitionActionKind.StopAtMissingCounterCapture);
        bool globalReached = HasAction(
            plan,
            QueueAppendAcquisitionActionKind.LoadIndependentGlobalPoolPointerIntoEcx);
        bool globalMissing = HasAction(
            plan,
            QueueAppendAcquisitionActionKind.StopAtMissingGlobalPoolCapture);
        bool acquisitionCallReached = HasAction(
            plan,
            QueueAppendAcquisitionActionKind.CallAcquisitionFunction);
        bool acquisitionResultMissing = HasAction(
            plan,
            QueueAppendAcquisitionActionKind.StopAtMissingAcquisitionResult);
        bool acquisitionResultCaptured = HasAction(
            plan,
            QueueAppendAcquisitionActionKind.StoreCapturedAcquisitionResult);

        GameSocketCompletionErrorQueueAcquisitionCaptureState counterState =
            counterMissing
                ? GameSocketCompletionErrorQueueAcquisitionCaptureState.Missing
                : counterReached
                    ? GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured
                    : GameSocketCompletionErrorQueueAcquisitionCaptureState.NotReached;
        GameSocketCompletionErrorQueueAcquisitionCaptureState globalState =
            globalMissing
                ? GameSocketCompletionErrorQueueAcquisitionCaptureState.Missing
                : globalReached
                    ? GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured
                    : NotReachedOrUnused(globalPoolCapture);
        GameSocketCompletionErrorQueueAcquisitionCaptureState resultState =
            !acquisitionCallReached
                ? NotReachedOrUnused(acquisitionResultCapture)
                : acquisitionResultMissing
                    ? GameSocketCompletionErrorQueueAcquisitionCaptureState.Missing
                    : acquisitionResultCaptured
                        ? GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured
                        : NotReachedOrUnused(acquisitionResultCapture);

        return new(counterState, globalState, resultState);
    }

    private static GameSocketCompletionErrorQueueAcquisitionCaptureState
        NotReachedOrUnused(QueueAppendAcquisitionCapture<uint> capture) =>
        capture.Kind == QueueAppendAcquisitionCaptureKind.Captured
            ? GameSocketCompletionErrorQueueAcquisitionCaptureState.SuppliedButUnused
            : GameSocketCompletionErrorQueueAcquisitionCaptureState.NotReached;

    private static bool HasAction(
        QueueAppendAcquisitionPlan plan,
        QueueAppendAcquisitionActionKind kind) =>
        plan.Actions.Any(action => action.Kind == kind);

    private static (uint? LastReached, uint? StopBefore) ChildBoundaries(
        QueueAppendAcquisitionPlan plan)
    {
        if (plan.Actions.Count == 0)
            return (null, null);

        QueueAppendAcquisitionAction last = plan.Actions[^1];
        if (last.Kind == QueueAppendAcquisitionActionKind.StopBeforeReceiverWordCRead)
        {
            uint? lastReached = plan.Actions.Count > 1
                ? plan.Actions[^2].InstructionRawFileOffset
                : null;
            return (lastReached, last.InstructionRawFileOffset);
        }

        return (last.InstructionRawFileOffset, last.InstructionRawFileOffset);
    }

    private static GameSocketCompletionErrorStateFlags? ChildFlagsAtStop(
        QueueAppendAcquisitionPlan plan,
        GameSocketCompletionErrorStateFlags? flagsAfterXadd)
    {
        if (HasAction(plan, QueueAppendAcquisitionActionKind.StopAtMissingCounterCapture))
            return null;

        if (HasAction(plan, QueueAppendAcquisitionActionKind.StopAtMissingGlobalPoolCapture))
            return flagsAfterXadd;

        if (HasAction(plan, QueueAppendAcquisitionActionKind.StopAtMissingAcquisitionResult))
            return null;

        if (plan.AcquisitionResult == 0)
            return new(false, true, null, true, false, false);

        if (plan.AcquisitionResult is uint result)
            return TestFlags(result);

        return null;
    }

    private static GameSocketCompletionErrorStateFlags AddOneFlags(uint oldValue)
    {
        uint newValue = unchecked(oldValue + 1);
        return new(
            Carry: newValue < oldValue,
            Parity: HasEvenParity((byte)newValue),
            AuxiliaryCarry: ((oldValue ^ 1u ^ newValue) & 0x10) != 0,
            Zero: newValue == 0,
            Sign: (newValue & 0x80000000) != 0,
            Overflow: ((~(oldValue ^ 1u) & (oldValue ^ newValue) & 0x80000000) != 0));
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
