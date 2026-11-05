using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum GameSocketCompletionErrorQueueAttemptCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketCompletionErrorQueueAttemptCaptureStates(
    GameSocketCompletionErrorQueueAttemptCaptureState ReceiverTag,
    GameSocketCompletionErrorQueueAttemptCaptureState ReceiverTail,
    GameSocketCompletionErrorQueueAttemptCaptureState DiagnosticReturned,
    GameSocketCompletionErrorQueueAttemptCaptureState CompareExchangeReturnedEax,
    GameSocketCompletionErrorQueueAttemptCaptureState CompareExchangeReturnedEdx);

public enum GameSocketCompletionErrorQueueAttemptLifecycleOutcome
{
    ParentAcquisitionBoundary,
    NullAcquisitionResultBoundary,
    MissingReceiverTagBoundary,
    MissingReceiverTailBoundary,
    AttemptProjected
}

public enum GameSocketCompletionErrorQueueAttemptLifecycleDisposition
{
    BoundedAtParentAcquisition,
    BoundedAtAttemptCapture,
    BoundedInGenericAttemptProjection
}

public enum GameSocketCompletionErrorQueueAttemptLifecycleActionKind
{
    ReadReceiverTag,
    StopAtMissingReceiverTagCapture,
    ReadReceiverTail,
    StopAtMissingReceiverTailCapture,
    StopAtNullTailDiagnosticCall
}

public readonly record struct GameSocketCompletionErrorQueueAttemptLifecycleAction(
    GameSocketCompletionErrorQueueAttemptLifecycleActionKind Kind,
    uint InstructionRawFileOffset,
    uint? Address = null,
    uint? Value = null,
    bool Attempted = false,
    bool Captured = false,
    bool? Committed = null);

public readonly record struct GameSocketCompletionErrorQueueAttemptLifecycleTraceStep(
    int Sequence,
    GameSocketCompletionErrorQueueAcquisitionTraceStep? ParentStep,
    GameSocketCompletionErrorQueueAttemptLifecycleAction? LifecycleAction,
    QueueAppendAttemptAction? AttemptAction);

public readonly record struct GameSocketCompletionErrorQueueAttemptLifecycleCaptures(
    QueueAppendAttemptCapture<uint> ReceiverTag,
    QueueAppendAttemptCapture<uint> ReceiverTail,
    QueueAppendAttemptCapture<bool> DiagnosticReturned,
    QueueAppendAttemptCapture<uint> CompareExchangeReturnedEax,
    QueueAppendAttemptCapture<uint> CompareExchangeReturnedEdx);

public sealed class GameSocketCompletionErrorQueueAttemptLifecyclePlan
{
    internal GameSocketCompletionErrorQueueAttemptLifecyclePlan(
        GameSocketCompletionErrorQueueAcquisitionPlan parentPlan,
        GameSocketCompletionErrorQueueAttemptLifecycleCaptures captures,
        QueueAppendAttemptPlan? childAttemptPlan,
        GameSocketCompletionErrorQueueAttemptCaptureStates captureStates,
        ImmutableArray<GameSocketCompletionErrorQueueAttemptLifecycleTraceStep> orderedTrace,
        GameSocketCompletionErrorQueueAttemptLifecycleOutcome outcome,
        GameSocketCompletionErrorQueueAttemptLifecycleDisposition disposition,
        bool cleanNonzeroAcquisitionAdmitted,
        uint? derivedQueueReceiverPointer,
        uint? capturedNewNode,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset,
        GameSocketCompletionErrorStateFlags? flagsAtAttemptEntry,
        GameSocketCompletionErrorStateFlags? flagsAfterTailTest,
        GameSocketCompletionErrorStateFlags? flagsAfterDiagnosticReturn,
        GameSocketCompletionErrorStateFlags? flagsBeforeCompareExchange,
        GameSocketCompletionErrorStateFlags? flagsAfterCompareExchange,
        GameSocketCompletionErrorStateFlags? flagsAtStop)
    {
        ParentPlan = parentPlan;
        Captures = captures;
        ChildAttemptPlan = childAttemptPlan;
        CaptureStates = captureStates;
        OrderedTrace = orderedTrace;
        Outcome = outcome;
        Disposition = disposition;
        CleanNonzeroAcquisitionAdmitted = cleanNonzeroAcquisitionAdmitted;
        DerivedQueueReceiverPointer = derivedQueueReceiverPointer;
        CapturedNewNode = capturedNewNode;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        FlagsAtAttemptEntry = flagsAtAttemptEntry;
        FlagsAfterTailTest = flagsAfterTailTest;
        FlagsAfterDiagnosticReturn = flagsAfterDiagnosticReturn;
        FlagsBeforeCompareExchange = flagsBeforeCompareExchange;
        FlagsAfterCompareExchange = flagsAfterCompareExchange;
        FlagsAtStop = flagsAtStop;
    }

    public GameSocketCompletionErrorQueueAcquisitionPlan ParentPlan { get; }
    public GameSocketCompletionErrorQueueAttemptLifecycleCaptures Captures { get; }
    public QueueAppendAttemptPlan? ChildAttemptPlan { get; }
    public GameSocketCompletionErrorQueueAttemptCaptureStates CaptureStates { get; }
    public ImmutableArray<GameSocketCompletionErrorQueueAttemptLifecycleTraceStep> OrderedTrace { get; }
    public GameSocketCompletionErrorQueueAttemptLifecycleOutcome Outcome { get; }
    public GameSocketCompletionErrorQueueAttemptLifecycleDisposition Disposition { get; }
    public bool CleanNonzeroAcquisitionAdmitted { get; }
    public uint? DerivedQueueReceiverPointer { get; }
    public uint? CapturedNewNode { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAtAttemptEntry { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAfterTailTest { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAfterDiagnosticReturn { get; }
    public GameSocketCompletionErrorStateFlags? FlagsBeforeCompareExchange { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAfterCompareExchange { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAtStop { get; }

    public uint? QueueToken => ParentPlan.QueueToken;
    public bool AttemptProjectionEntered => ChildAttemptPlan is not null;
    public bool NativeInstructionsExecuted => false;
    public bool QueuePublicationReached => false;
    public bool QueuePublicationObserved => false;

    public bool ReachedBoundedProjectionBoundary =>
        Disposition == GameSocketCompletionErrorQueueAttemptLifecycleDisposition.BoundedAtParentAcquisition
            ? ParentPlan.ReachedBoundedProjectionBoundary
            : ChildAttemptPlan?.Outcome is
                QueueAppendAttemptOutcome.RetryBoundary
                or QueueAppendAttemptOutcome.SuccessBoundary;

    public bool IsCleanAttemptProjection =>
        CleanNonzeroAcquisitionAdmitted
        && ChildAttemptPlan is not null
        && CaptureStates.ReceiverTag == GameSocketCompletionErrorQueueAttemptCaptureState.Captured
        && CaptureStates.ReceiverTail == GameSocketCompletionErrorQueueAttemptCaptureState.Captured
        && CaptureStates.DiagnosticReturned != GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
        && CaptureStates.CompareExchangeReturnedEax != GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
        && CaptureStates.CompareExchangeReturnedEdx != GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
        && ChildAttemptPlan?.Outcome is
            QueueAppendAttemptOutcome.RetryBoundary
            or QueueAppendAttemptOutcome.SuccessBoundary;
}

/// <summary>
/// Reprojects the parent acquisition from its captures before joining a clean,
/// nonzero result to one generic CMPXCHG8B attempt. It does not retry, execute
/// native instructions, or project queue publication.
/// </summary>
public static class GameSocketCompletionErrorQueueAttemptLifecycleProjection
{
    public const uint FirstTagCaptureRawFileOffset = QueueAppendAttemptProjection.TagCaptureRawFileOffset;
    public const uint FirstTailCaptureRawFileOffset = QueueAppendAttemptProjection.TailCaptureRawFileOffset;
    public const uint AttemptStopBeforePublicationRawFileOffset =
        QueueAppendAttemptProjection.SuccessBoundaryRawFileOffset;
    public const int QueueReceiverOffsetFromParentEsi = 0x260;

    public static GameSocketCompletionErrorQueueAttemptLifecyclePlan Project(
        GameSocketCompletionErrorStateCapture parentCapture,
        QueueAppendAcquisitionCapture<uint> counterOldValueCapture,
        QueueAppendAcquisitionCapture<uint> globalPoolPointerCapture,
        QueueAppendAcquisitionCapture<uint> acquisitionResultCapture,
        QueueAppendAttemptCapture<uint> receiverTagCapture,
        QueueAppendAttemptCapture<uint> receiverTailCapture,
        QueueAppendAttemptCapture<bool> diagnosticReturnedCapture,
        QueueAppendAttemptCapture<uint> compareExchangeReturnedEaxCapture,
        QueueAppendAttemptCapture<uint> compareExchangeReturnedEdxCapture)
    {
        GameSocketCompletionErrorQueueAcquisitionPlan parentPlan =
            GameSocketCompletionErrorQueueAcquisitionProjection.Project(
                parentCapture,
                counterOldValueCapture,
                globalPoolPointerCapture,
                acquisitionResultCapture);
        var captures = new GameSocketCompletionErrorQueueAttemptLifecycleCaptures(
            receiverTagCapture,
            receiverTailCapture,
            diagnosticReturnedCapture,
            compareExchangeReturnedEaxCapture,
            compareExchangeReturnedEdxCapture);

        if (!IsCleanNonzeroAcquisition(parentPlan))
        {
            bool nullResult = parentPlan.CleanParentPreCallHandoffValidated
                && parentPlan.ChildAcquisitionPlan is
                {
                    Outcome: QueueAppendAcquisitionOutcome.NullResultBranchProjected,
                    AcquisitionResult: 0
                };
            return ParentBounded(
                parentPlan,
                captures,
                nullResult
                    ? GameSocketCompletionErrorQueueAttemptLifecycleOutcome.NullAcquisitionResultBoundary
                    : GameSocketCompletionErrorQueueAttemptLifecycleOutcome.ParentAcquisitionBoundary);
        }

        uint receiver = parentPlan.DerivedQueueReceiverPointer!.Value;
        uint newNode = parentPlan.ChildAcquisitionPlan!.Value.AcquisitionResult!.Value;
        var trace = CopyParentTrace(parentPlan);
        var attemptEntryFlags = parentPlan.FlagsAtStop;

        if (receiverTagCapture.Kind == QueueAppendAttemptCaptureKind.Unreached)
        {
            throw InvalidCapture(nameof(receiverTagCapture), "The tag read is reached at attempt entry.");
        }

        trace.Add(new(
            trace.Count,
            null,
            new(
                GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTag,
                FirstTagCaptureRawFileOffset,
                unchecked(receiver + (uint)QueueAppendAttemptProjection.TagOffset),
                receiverTagCapture.Value,
                Attempted: true,
                Captured: receiverTagCapture.Kind == QueueAppendAttemptCaptureKind.Captured,
                Committed: false),
            null));

        if (receiverTagCapture.Kind == QueueAppendAttemptCaptureKind.Missing)
        {
            trace.Add(new(
                trace.Count,
                null,
                new(
                    GameSocketCompletionErrorQueueAttemptLifecycleActionKind.StopAtMissingReceiverTagCapture,
                    FirstTagCaptureRawFileOffset),
                null));
            return AttemptCaptureBounded(
                parentPlan,
                captures,
                trace,
                GameSocketCompletionErrorQueueAttemptLifecycleOutcome.MissingReceiverTagBoundary,
                GameSocketCompletionErrorQueueAttemptCaptureState.Missing,
                StateForUnreached(receiverTailCapture),
                StateForUnreached(diagnosticReturnedCapture),
                StateForUnreached(compareExchangeReturnedEaxCapture),
                StateForUnreached(compareExchangeReturnedEdxCapture),
                receiver,
                newNode,
                FirstTagCaptureRawFileOffset,
                FirstTagCaptureRawFileOffset,
                attemptEntryFlags);
        }

        if (receiverTailCapture.Kind == QueueAppendAttemptCaptureKind.Unreached)
        {
            throw InvalidCapture(nameof(receiverTailCapture), "The tail read follows the captured tag read.");
        }

        trace.Add(new(
            trace.Count,
            null,
            new(
                GameSocketCompletionErrorQueueAttemptLifecycleActionKind.ReadReceiverTail,
                FirstTailCaptureRawFileOffset,
                unchecked(receiver + (uint)QueueAppendAttemptProjection.TailOffset),
                receiverTailCapture.Value,
                Attempted: true,
                Captured: receiverTailCapture.Kind == QueueAppendAttemptCaptureKind.Captured,
                Committed: false),
            null));

        if (receiverTailCapture.Kind == QueueAppendAttemptCaptureKind.Missing)
        {
            trace.Add(new(
                trace.Count,
                null,
                new(
                    GameSocketCompletionErrorQueueAttemptLifecycleActionKind.StopAtMissingReceiverTailCapture,
                    FirstTailCaptureRawFileOffset),
                null));
            return AttemptCaptureBounded(
                parentPlan,
                captures,
                trace,
                GameSocketCompletionErrorQueueAttemptLifecycleOutcome.MissingReceiverTailBoundary,
                GameSocketCompletionErrorQueueAttemptCaptureState.Captured,
                GameSocketCompletionErrorQueueAttemptCaptureState.Missing,
                StateForUnreached(diagnosticReturnedCapture),
                StateForUnreached(compareExchangeReturnedEaxCapture),
                StateForUnreached(compareExchangeReturnedEdxCapture),
                receiver,
                newNode,
                FirstTailCaptureRawFileOffset,
                FirstTailCaptureRawFileOffset,
                attemptEntryFlags);
        }

        QueueAppendAttemptCapture<uint> capturedTail = receiverTailCapture;
        bool tailIsNull = capturedTail.Value!.Value == 0;
        QueueAppendAttemptCapture<bool> effectiveDiagnostic =
            tailIsNull
                ? diagnosticReturnedCapture
                : QueueAppendAttemptCapture<bool>.Unreached();
        bool diagnosticContinues =
            !tailIsNull
            || effectiveDiagnostic.Kind == QueueAppendAttemptCaptureKind.Captured
                && effectiveDiagnostic.Value!.Value;
        QueueAppendAttemptCapture<uint> effectiveEax =
            diagnosticContinues
                ? compareExchangeReturnedEaxCapture
                : QueueAppendAttemptCapture<uint>.Unreached();
        QueueAppendAttemptCapture<uint> effectiveEdx =
            diagnosticContinues
                ? compareExchangeReturnedEdxCapture
                : QueueAppendAttemptCapture<uint>.Unreached();

        if (diagnosticContinues
            && effectiveEax.Kind == QueueAppendAttemptCaptureKind.Captured
            && effectiveEax.Value!.Value != capturedTail.Value!.Value)
        {
            effectiveEdx = QueueAppendAttemptCapture<uint>.Unreached();
        }
        else if (diagnosticContinues
            && effectiveEax.Kind == QueueAppendAttemptCaptureKind.Missing)
        {
            effectiveEdx = QueueAppendAttemptCapture<uint>.Unreached();
        }

        var genericCaptures = new QueueAppendAttemptCaptures(
            receiver,
            newNode,
            receiverTagCapture,
            receiverTailCapture,
            effectiveDiagnostic,
            effectiveEax,
            effectiveEdx);
        QueueAppendAttemptPlan attemptPlan = QueueAppendAttemptProjection.Project(genericCaptures);
        foreach (QueueAppendAttemptAction action in attemptPlan.Actions)
        {
            if (action.Kind is QueueAppendAttemptActionKind.CaptureReceiverTag
                or QueueAppendAttemptActionKind.CaptureReceiverTail)
            {
                continue;
            }

            if (action.Kind is QueueAppendAttemptActionKind.StopAtMissingDiagnosticReturnBoundary
                or QueueAppendAttemptActionKind.StopAtDiagnosticDidNotReturnBoundary)
            {
                trace.Add(new(
                    trace.Count,
                    null,
                    new(
                        GameSocketCompletionErrorQueueAttemptLifecycleActionKind.StopAtNullTailDiagnosticCall,
                        QueueAppendAttemptProjection.DiagnosticCallRawFileOffset,
                        QueueAppendAttemptProjection.DiagnosticCallCellVirtualAddress,
                        Attempted: true),
                    null));
                continue;
            }

            trace.Add(new(trace.Count, null, null, action));
        }

        GameSocketCompletionErrorStateFlags? tailTestFlags =
            TestFlags(capturedTail.Value!.Value);
        GameSocketCompletionErrorStateFlags? diagnosticReturnFlags =
            tailIsNull
                && diagnosticReturnedCapture.Kind == QueueAppendAttemptCaptureKind.Captured
                && diagnosticReturnedCapture.Value!.Value
                    ? UnknownFlags
                    : null;
        bool compareExchangeReached =
            HasAction(attemptPlan, QueueAppendAttemptActionKind.AttemptLockedCompareExchange);
        GameSocketCompletionErrorStateFlags? preCompareExchangeFlags =
            compareExchangeReached ? OrFlags(newNode) : null;
        GameSocketCompletionErrorStateFlags? casFlags =
            compareExchangeReached
                ? CompareExchangeFlags(attemptPlan, preCompareExchangeFlags!.Value)
                : null;
        bool diagnosticBoundary =
            attemptPlan.Outcome is QueueAppendAttemptOutcome.MissingDiagnosticReturnBoundary
                or QueueAppendAttemptOutcome.DiagnosticDidNotReturnBoundary;
        GameSocketCompletionErrorStateFlags? stopFlags =
            diagnosticBoundary
                ? tailTestFlags
                : FlagsAtAttemptOutcome(
                    attemptPlan,
                    capturedTail.Value!.Value,
                    receiverTagCapture.Value!.Value,
                    casFlags);
        uint lastReached = diagnosticBoundary
            ? QueueAppendAttemptProjection.DiagnosticCallRawFileOffset
            : attemptPlan.Actions[^1].InstructionRawFileOffset;
        uint stopBefore = diagnosticBoundary
            ? QueueAppendAttemptProjection.DiagnosticCallRawFileOffset
            : attemptPlan.Outcome == QueueAppendAttemptOutcome.RetryBoundary
                ? QueueAppendAttemptProjection.RetryTargetRawFileOffset
                : attemptPlan.Actions[^1].InstructionRawFileOffset;

        return new(
            parentPlan,
            captures,
            attemptPlan,
            AttemptCaptureStates(
                attemptPlan,
                captures,
                effectiveDiagnostic,
                effectiveEax,
                effectiveEdx),
            trace.ToImmutable(),
            GameSocketCompletionErrorQueueAttemptLifecycleOutcome.AttemptProjected,
            GameSocketCompletionErrorQueueAttemptLifecycleDisposition.BoundedInGenericAttemptProjection,
            true,
            receiver,
            newNode,
            lastReached,
            stopBefore,
            attemptEntryFlags,
            tailTestFlags,
            diagnosticReturnFlags,
            preCompareExchangeFlags,
            casFlags,
            stopFlags);
    }

    private static GameSocketCompletionErrorQueueAttemptLifecyclePlan ParentBounded(
        GameSocketCompletionErrorQueueAcquisitionPlan parentPlan,
        GameSocketCompletionErrorQueueAttemptLifecycleCaptures captures,
        GameSocketCompletionErrorQueueAttemptLifecycleOutcome outcome)
    {
        var trace = CopyParentTrace(parentPlan);
        return new(
            parentPlan,
            captures,
            null,
            new(
                StateForUnreached(captures.ReceiverTag),
                StateForUnreached(captures.ReceiverTail),
                StateForUnreached(captures.DiagnosticReturned),
                StateForUnreached(captures.CompareExchangeReturnedEax),
                StateForUnreached(captures.CompareExchangeReturnedEdx)),
            trace.ToImmutable(),
            outcome,
            GameSocketCompletionErrorQueueAttemptLifecycleDisposition.BoundedAtParentAcquisition,
            false,
            null,
            null,
            parentPlan.LastReachedRawFileOffset,
            parentPlan.StopBeforeRawFileOffset,
            null,
            null,
            null,
            null,
            null,
            parentPlan.FlagsAtStop);
    }

    private static GameSocketCompletionErrorQueueAttemptLifecyclePlan AttemptCaptureBounded(
        GameSocketCompletionErrorQueueAcquisitionPlan parentPlan,
        GameSocketCompletionErrorQueueAttemptLifecycleCaptures captures,
        ImmutableArray<GameSocketCompletionErrorQueueAttemptLifecycleTraceStep>.Builder trace,
        GameSocketCompletionErrorQueueAttemptLifecycleOutcome outcome,
        GameSocketCompletionErrorQueueAttemptCaptureState tagState,
        GameSocketCompletionErrorQueueAttemptCaptureState tailState,
        GameSocketCompletionErrorQueueAttemptCaptureState diagnosticState,
        GameSocketCompletionErrorQueueAttemptCaptureState eaxState,
        GameSocketCompletionErrorQueueAttemptCaptureState edxState,
        uint receiver,
        uint newNode,
        uint lastReached,
        uint stopBefore,
        GameSocketCompletionErrorStateFlags? flags)
    {
        return new(
            parentPlan,
            captures,
            null,
            new(tagState, tailState, diagnosticState, eaxState, edxState),
            trace.ToImmutable(),
            outcome,
            GameSocketCompletionErrorQueueAttemptLifecycleDisposition.BoundedAtAttemptCapture,
            true,
            receiver,
            newNode,
            lastReached,
            stopBefore,
            parentPlan.FlagsAtStop,
            null,
            null,
            null,
            null,
            flags);
    }

    private static ImmutableArray<GameSocketCompletionErrorQueueAttemptLifecycleTraceStep>.Builder
        CopyParentTrace(GameSocketCompletionErrorQueueAcquisitionPlan parentPlan)
    {
        var trace =
            ImmutableArray.CreateBuilder<GameSocketCompletionErrorQueueAttemptLifecycleTraceStep>(
                parentPlan.OrderedTrace.Length + 20);
        foreach (GameSocketCompletionErrorQueueAcquisitionTraceStep step in parentPlan.OrderedTrace)
        {
            trace.Add(new(trace.Count, step, null, null));
        }

        return trace;
    }

    private static bool IsCleanNonzeroAcquisition(
        GameSocketCompletionErrorQueueAcquisitionPlan plan) =>
        plan.CleanParentPreCallHandoffValidated
        && plan.ChildAcquisitionPlan is
        {
            Outcome: QueueAppendAcquisitionOutcome.NonzeroResultBranchProjected,
            AcquisitionResult: uint result
        }
        && result != 0
        && plan.ChildAcquisitionReachedBoundedBoundary
        && plan.DerivedQueueReceiverPointer.HasValue
        && plan.ChildCaptureStates is
        {
            CounterOldValue: GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured,
            GlobalPoolPointer: GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured,
            AcquisitionResult: GameSocketCompletionErrorQueueAcquisitionCaptureState.Captured
        };

    private static GameSocketCompletionErrorQueueAttemptCaptureStates AttemptCaptureStates(
        QueueAppendAttemptPlan plan,
        GameSocketCompletionErrorQueueAttemptLifecycleCaptures original,
        QueueAppendAttemptCapture<bool> effectiveDiagnostic,
        QueueAppendAttemptCapture<uint> effectiveEax,
        QueueAppendAttemptCapture<uint> effectiveEdx)
    {
        bool diagnosticReached = HasAction(plan, QueueAppendAttemptActionKind.ReportNullTailDiagnostic);
        bool compareExchangeReached = HasAction(plan, QueueAppendAttemptActionKind.AttemptLockedCompareExchange);
        bool eaxCompared = HasAction(plan, QueueAppendAttemptActionKind.CompareReturnedEax);
        bool edxCompared = HasAction(plan, QueueAppendAttemptActionKind.CompareReturnedEdx);
        return new(
            GameSocketCompletionErrorQueueAttemptCaptureState.Captured,
            GameSocketCompletionErrorQueueAttemptCaptureState.Captured,
            diagnosticReached
                ? StateForReached(effectiveDiagnostic)
                : StateForUnreached(original.DiagnosticReturned),
            compareExchangeReached && HasAction(
                plan, QueueAppendAttemptActionKind.StopAtMissingCompareExchangeEaxBoundary)
                    ? GameSocketCompletionErrorQueueAttemptCaptureState.Missing
                    : eaxCompared
                        ? GameSocketCompletionErrorQueueAttemptCaptureState.Captured
                        : compareExchangeReached
                            ? StateForReached(effectiveEax)
                            : StateForUnreached(original.CompareExchangeReturnedEax),
            edxCompared
                ? GameSocketCompletionErrorQueueAttemptCaptureState.Captured
                : compareExchangeReached
                    && HasAction(plan, QueueAppendAttemptActionKind.StopAtMissingCompareExchangeEdxBoundary)
                        ? GameSocketCompletionErrorQueueAttemptCaptureState.Missing
                        : StateForUnreached(original.CompareExchangeReturnedEdx));
    }

    private static GameSocketCompletionErrorStateFlags? FlagsAtAttemptOutcome(
        QueueAppendAttemptPlan plan,
        uint capturedTail,
        uint capturedTag,
        GameSocketCompletionErrorStateFlags? compareExchangeFlags)
    {
        if (plan.Outcome is QueueAppendAttemptOutcome.MissingDiagnosticReturnBoundary
            or QueueAppendAttemptOutcome.DiagnosticDidNotReturnBoundary)
        {
            return null;
        }

        if (plan.Outcome == QueueAppendAttemptOutcome.MissingCompareExchangeEaxBoundary)
        {
            return compareExchangeFlags;
        }

        uint returnedEax = plan.Captures.CompareExchangeReturnedEax.Value!.Value;
        GameSocketCompletionErrorStateFlags eaxCompareFlags =
            CompareFlags(returnedEax, capturedTail);
        if (plan.Outcome == QueueAppendAttemptOutcome.RetryBoundary
            && returnedEax != capturedTail)
        {
            return eaxCompareFlags;
        }

        if (plan.Outcome == QueueAppendAttemptOutcome.MissingCompareExchangeEdxBoundary)
        {
            return eaxCompareFlags;
        }

        uint returnedEdx = plan.Captures.CompareExchangeReturnedEdx.Value!.Value;
        return CompareFlags(returnedEdx, capturedTag);
    }

    private static GameSocketCompletionErrorStateFlags CompareExchangeFlags(
        QueueAppendAttemptPlan plan,
        GameSocketCompletionErrorStateFlags flagsBeforeCompareExchange)
    {
        bool? zeroFlag = null;
        if (plan.Captures.CompareExchangeReturnedEax.Kind
            == QueueAppendAttemptCaptureKind.Captured)
        {
            uint eax = plan.Captures.CompareExchangeReturnedEax.Value!.Value;
            uint expectedTail = plan.Captures.CapturedTail.Value!.Value;
            if (eax != expectedTail)
            {
                zeroFlag = false;
            }
            else if (plan.Captures.CompareExchangeReturnedEdx.Kind
                == QueueAppendAttemptCaptureKind.Captured)
            {
                zeroFlag = plan.Captures.CompareExchangeReturnedEdx.Value!.Value
                    == plan.Captures.CapturedTag.Value!.Value;
            }
        }

        return flagsBeforeCompareExchange with { Zero = zeroFlag };
    }

    private static GameSocketCompletionErrorStateFlags TestFlags(uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            AuxiliaryCarry: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static GameSocketCompletionErrorStateFlags OrFlags(uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            AuxiliaryCarry: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static GameSocketCompletionErrorStateFlags CompareFlags(uint left, uint right)
    {
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: (((left ^ right) & (left ^ result) & 0x80000000) != 0));
    }

    private static bool HasEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static GameSocketCompletionErrorStateFlags UnknownFlags =>
        new(null, null, null, null, null, null);

    private static GameSocketCompletionErrorQueueAttemptCaptureState StateForReached<T>(
        QueueAppendAttemptCapture<T> capture) where T : struct =>
        capture.Kind switch
        {
            QueueAppendAttemptCaptureKind.Missing =>
                GameSocketCompletionErrorQueueAttemptCaptureState.Missing,
            QueueAppendAttemptCaptureKind.Captured =>
                GameSocketCompletionErrorQueueAttemptCaptureState.Captured,
            _ => throw InvalidCapture(nameof(capture), "A reached capture cannot be unreached.")
        };

    private static GameSocketCompletionErrorQueueAttemptCaptureState StateForUnreached<T>(
        QueueAppendAttemptCapture<T> capture) where T : struct =>
        capture.Kind == QueueAppendAttemptCaptureKind.Captured
            ? GameSocketCompletionErrorQueueAttemptCaptureState.SuppliedButUnused
            : GameSocketCompletionErrorQueueAttemptCaptureState.NotReached;

    private static ArgumentException InvalidCapture(string parameterName, string message) =>
        new(message, parameterName);

    private static bool HasAction(QueueAppendAttemptPlan plan, QueueAppendAttemptActionKind kind) =>
        plan.Actions.Any(action => action.Kind == kind);
}
