using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketCompletionErrorQueueChildPublicationCaptures(
    QueueAppendAcquisitionCapture<uint> OldTailNextPointer,
    QueueAppendAttemptCapture<bool> DiagnosticReturned,
    QueueAppendAcquisitionCapture<uint> PostDiagnosticEspBeforeCleanup,
    QueueAppendAcquisitionCapture<uint> FreshPayloadArgument,
    QueueAppendAcquisitionCapture<uint> FreshReloadedNewNode);

public readonly record struct GameSocketCompletionErrorQueuePublicationLifecycleCaptures(
    GameSocketCompletionErrorStateCapture ParentError,
    QueueAppendAcquisitionCapture<uint> CounterOldValue,
    QueueAppendAcquisitionCapture<uint> GlobalPoolPointer,
    QueueAppendAcquisitionCapture<uint> AcquisitionResult,
    QueueAppendAttemptCapture<uint> ReceiverTag,
    QueueAppendAttemptCapture<uint> ReceiverTail,
    QueueAppendAttemptCapture<bool> AttemptDiagnosticReturned,
    QueueAppendAttemptCapture<uint> CompareExchangeReturnedEax,
    QueueAppendAttemptCapture<uint> CompareExchangeReturnedEdx,
    GameSocketCompletionErrorQueueChildPublicationCaptures ChildPublication);

public enum GameSocketCompletionErrorQueuePublicationCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketCompletionErrorQueuePublicationCaptureStates(
    GameSocketCompletionErrorQueuePublicationCaptureState OldTailNextPointer,
    GameSocketCompletionErrorQueuePublicationCaptureState DiagnosticReturned,
    GameSocketCompletionErrorQueuePublicationCaptureState PostDiagnosticEspBeforeCleanup,
    GameSocketCompletionErrorQueuePublicationCaptureState FreshPayloadArgument,
    GameSocketCompletionErrorQueuePublicationCaptureState FreshReloadedNewNode);

public enum GameSocketCompletionErrorQueuePublicationLifecycleOutcome
{
    ParentOrAttemptBoundary,
    RetryBoundary,
    AwaitingNextPointerCapture,
    AwaitingDiagnosticReturnCapture,
    DiagnosticContinuationBoundary,
    AwaitingPostDiagnosticEspCapture,
    AwaitingFreshPayloadCapture,
    AwaitingFreshReloadedNewNodeCapture,
    PublicationProjected
}

public enum GameSocketCompletionErrorQueuePublicationLifecycleDisposition
{
    BoundedAtParentOrAttempt,
    BoundedInGenericAttemptProjection,
    BoundedInGenericPublicationProjection
}

public enum GameSocketCompletionErrorQueuePublicationLifecycleActionKind
{
    StopAtMissingPostDiagnosticEspCapture
}

public readonly record struct GameSocketCompletionErrorQueuePublicationLifecycleAction(
    GameSocketCompletionErrorQueuePublicationLifecycleActionKind Kind,
    uint InstructionRawFileOffset);

public enum GameSocketCompletionErrorQueuePublicationMemoryEffectKind
{
    Read,
    Write
}

public readonly record struct GameSocketCompletionErrorQueuePublicationMemoryEffect(
    GameSocketCompletionErrorQueuePublicationMemoryEffectKind Kind,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool ProjectedCommitted);

public readonly record struct GameSocketCompletionErrorQueuePublicationLifecycleTraceStep(
    int Sequence,
    GameSocketCompletionErrorQueueAttemptLifecycleTraceStep? AttemptStep,
    QueueAppendPublicationAction? PublicationAction,
    GameSocketCompletionErrorQueuePublicationLifecycleAction? LifecycleAction,
    GameSocketCompletionErrorQueuePublicationMemoryEffect? PublicationMemoryEffect);

public sealed class GameSocketCompletionErrorQueuePublicationLifecyclePlan
{
    internal GameSocketCompletionErrorQueuePublicationLifecyclePlan(
        GameSocketCompletionErrorQueuePublicationLifecycleCaptures captures,
        GameSocketCompletionErrorQueueAttemptLifecyclePlan attemptPlan,
        QueueAppendPublicationPlan? childPublicationPlan,
        GameSocketCompletionErrorQueuePublicationCaptureStates publicationCaptureStates,
        ImmutableArray<GameSocketCompletionErrorQueuePublicationLifecycleTraceStep> orderedTrace,
        GameSocketCompletionErrorQueuePublicationLifecycleOutcome outcome,
        GameSocketCompletionErrorQueuePublicationLifecycleDisposition disposition,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset,
        GameSocketCompletionErrorStateFlags? flagsAtStop)
    {
        Captures = captures;
        AttemptPlan = attemptPlan;
        ChildPublicationPlan = childPublicationPlan;
        PublicationCaptureStates = publicationCaptureStates;
        OrderedTrace = orderedTrace;
        Outcome = outcome;
        Disposition = disposition;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        FlagsAtStop = flagsAtStop;
    }

    public GameSocketCompletionErrorQueuePublicationLifecycleCaptures Captures { get; }
    public GameSocketCompletionErrorQueueAttemptLifecyclePlan AttemptPlan { get; }
    public QueueAppendPublicationPlan? ChildPublicationPlan { get; }
    public GameSocketCompletionErrorQueuePublicationCaptureStates PublicationCaptureStates { get; }
    public ImmutableArray<GameSocketCompletionErrorQueuePublicationLifecycleTraceStep> OrderedTrace { get; }
    public GameSocketCompletionErrorQueuePublicationLifecycleOutcome Outcome { get; }
    public GameSocketCompletionErrorQueuePublicationLifecycleDisposition Disposition { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAtStop { get; }

    public bool PublicationProjectionEntered => ChildPublicationPlan is not null;
    public bool NativeInstructionsExecuted => false;
    public bool QueuePublicationObserved => false;

    public bool ReachedBoundedProjectionBoundary =>
        ChildPublicationPlan?.Outcome == QueueAppendPublicationOutcome.ReturnedCapturedNewNode
        || ChildPublicationPlan is null && AttemptPlan.ReachedBoundedProjectionBoundary;

    public bool IsCleanPublicationProjection =>
        AttemptPlan.IsCleanAttemptProjection
        && AttemptPlan.ChildAttemptPlan?.Outcome == QueueAppendAttemptOutcome.SuccessBoundary
        && ChildPublicationPlan?.Outcome == QueueAppendPublicationOutcome.ReturnedCapturedNewNode
        && PublicationCaptureStates.OldTailNextPointer
            == GameSocketCompletionErrorQueuePublicationCaptureState.Captured
        && PublicationCaptureStates.FreshPayloadArgument
            == GameSocketCompletionErrorQueuePublicationCaptureState.Captured
        && PublicationCaptureStates.FreshReloadedNewNode
            == GameSocketCompletionErrorQueuePublicationCaptureState.Captured
        && PublicationCaptureStates.DiagnosticReturned
            != GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused
        && PublicationCaptureStates.PostDiagnosticEspBeforeCleanup
            != GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused;

    public uint? QueueToken => AttemptPlan.QueueToken;
    public uint? DerivedQueueReceiverPointer => AttemptPlan.DerivedQueueReceiverPointer;
    public uint? CapturedOldTailPointer =>
        AttemptPlan.ChildAttemptPlan is { } attempt
            && attempt.Captures.CapturedTail.Value.HasValue
                ? attempt.Captures.CapturedTail.Value.Value
                : null;
    public uint? CapturedNewNode => AttemptPlan.CapturedNewNode;
    public uint? FreshPayloadArgument =>
        PublicationCaptureStates.FreshPayloadArgument
            == GameSocketCompletionErrorQueuePublicationCaptureState.Captured
                ? Captures.ChildPublication.FreshPayloadArgument.Value
                : null;
    public uint? FreshReloadedNewNode =>
        PublicationCaptureStates.FreshReloadedNewNode
            == GameSocketCompletionErrorQueuePublicationCaptureState.Captured
                ? Captures.ChildPublication.FreshReloadedNewNode.Value
                : null;
    public bool? OuterQueueAppendReturnedNormally =>
        AttemptPlan.ParentPlan.ParentQueueAppendReturnedNormally;
    public bool OuterQueueAppendReturnObserved =>
        OuterQueueAppendReturnedNormally.HasValue;
}

/// <summary>
/// Reprojects the immutable parent/acquisition/attempt captures, then admits
/// the existing publication projection only after a successful single CAS.
/// It does not execute instructions or infer the outer call's return.
/// </summary>
public static class GameSocketCompletionErrorQueuePublicationLifecycleProjection
{
    public const uint PublicationStartRawFileOffset =
        QueueAppendPublicationProjection.ContinuationStartRawFileOffset;
    public const uint PublicationReturnRawFileOffset =
        QueueAppendPublicationProjection.ReturnRawFileOffset;
    public const uint PostDiagnosticStackCleanupRawFileOffset =
        QueueAppendPublicationProjection.DiagnosticReturnContinuationRawFileOffset;
    public const uint FreshPayloadArgumentRawFileOffset =
        QueueAppendPublicationProjection.PayloadArgumentReloadRawFileOffset;
    public const uint FreshNewNodeReloadRawFileOffset =
        QueueAppendPublicationProjection.NewNodeReloadRawFileOffset;
    public const uint OuterQueueAppendContinuationRawFileOffset = 0xBD2E19;
    public const uint PostDiagnosticStackCleanupBytes = 0x10;

    public static GameSocketCompletionErrorQueuePublicationLifecyclePlan Project(
        GameSocketCompletionErrorQueuePublicationLifecycleCaptures captures)
    {
        GameSocketCompletionErrorQueueAttemptLifecyclePlan attemptPlan =
            GameSocketCompletionErrorQueueAttemptLifecycleProjection.Project(
                captures.ParentError,
                captures.CounterOldValue,
                captures.GlobalPoolPointer,
                captures.AcquisitionResult,
                captures.ReceiverTag,
                captures.ReceiverTail,
                captures.AttemptDiagnosticReturned,
                captures.CompareExchangeReturnedEax,
                captures.CompareExchangeReturnedEdx);

        var trace = ImmutableArray.CreateBuilder<GameSocketCompletionErrorQueuePublicationLifecycleTraceStep>(
            attemptPlan.OrderedTrace.Length + 20);
        foreach (GameSocketCompletionErrorQueueAttemptLifecycleTraceStep step in attemptPlan.OrderedTrace)
        {
            trace.Add(new(trace.Count, step, null, null, null));
        }

        if (attemptPlan.ChildAttemptPlan?.Outcome != QueueAppendAttemptOutcome.SuccessBoundary
            || !attemptPlan.IsCleanAttemptProjection)
        {
            var unreachedStates = UnreachedStates(captures.ChildPublication);
            bool retry = attemptPlan.ChildAttemptPlan?.Outcome == QueueAppendAttemptOutcome.RetryBoundary;
            return new(
                captures,
                attemptPlan,
                null,
                unreachedStates,
                trace.ToImmutable(),
                retry
                    ? GameSocketCompletionErrorQueuePublicationLifecycleOutcome.RetryBoundary
                    : GameSocketCompletionErrorQueuePublicationLifecycleOutcome.ParentOrAttemptBoundary,
                attemptPlan.Disposition
                    == GameSocketCompletionErrorQueueAttemptLifecycleDisposition.BoundedAtParentAcquisition
                    ? GameSocketCompletionErrorQueuePublicationLifecycleDisposition.BoundedAtParentOrAttempt
                    : GameSocketCompletionErrorQueuePublicationLifecycleDisposition.BoundedInGenericAttemptProjection,
                attemptPlan.LastReachedRawFileOffset,
                attemptPlan.StopBeforeRawFileOffset,
                attemptPlan.FlagsAtStop);
        }

        uint oldTail = attemptPlan.ChildAttemptPlan!.Value.Captures.CapturedTail.Value!.Value;
        QueueAppendAcquisitionCapture<uint> nextCapture = captures.ChildPublication.OldTailNextPointer;
        RequireReached(nextCapture, nameof(captures.ChildPublication.OldTailNextPointer));

        bool nextCaptured = nextCapture.Kind == QueueAppendAcquisitionCaptureKind.Captured;
        bool diagnosticReached = nextCaptured && nextCapture.Value!.Value != 0;
        QueueAppendAttemptCapture<bool> diagnosticCapture = captures.ChildPublication.DiagnosticReturned;
        if (diagnosticReached)
        {
            RequireReached(diagnosticCapture, nameof(captures.ChildPublication.DiagnosticReturned));
        }

        bool diagnosticReturnedNormally =
            diagnosticReached
            && diagnosticCapture.Kind == QueueAppendAttemptCaptureKind.Captured
            && diagnosticCapture.Value!.Value;
        bool espReached = diagnosticReturnedNormally;
        QueueAppendAcquisitionCapture<uint> postDiagnosticEspCapture =
            captures.ChildPublication.PostDiagnosticEspBeforeCleanup;
        if (espReached)
        {
            RequireReached(
                postDiagnosticEspCapture,
                nameof(captures.ChildPublication.PostDiagnosticEspBeforeCleanup));
        }

        bool postDiagnosticEspCaptured =
            espReached && postDiagnosticEspCapture.Kind == QueueAppendAcquisitionCaptureKind.Captured;
        bool payloadReached =
            nextCaptured
            && (nextCapture.Value!.Value == 0
                || diagnosticReturnedNormally && postDiagnosticEspCaptured);
        QueueAppendAcquisitionCapture<uint> payloadCapture =
            captures.ChildPublication.FreshPayloadArgument;
        if (payloadReached)
        {
            RequireReached(payloadCapture, nameof(captures.ChildPublication.FreshPayloadArgument));
        }

        bool payloadCaptured =
            payloadReached && payloadCapture.Kind == QueueAppendAcquisitionCaptureKind.Captured;
        bool newNodeReached = payloadCaptured;
        QueueAppendAcquisitionCapture<uint> newNodeCapture =
            captures.ChildPublication.FreshReloadedNewNode;
        if (newNodeReached)
        {
            RequireReached(newNodeCapture, nameof(captures.ChildPublication.FreshReloadedNewNode));
        }

        var states = new GameSocketCompletionErrorQueuePublicationCaptureStates(
            StateForReached(nextCapture),
            diagnosticReached
                ? StateForReached(diagnosticCapture)
                : StateForUnreached(diagnosticCapture),
            espReached
                ? StateForReached(postDiagnosticEspCapture)
                : StateForUnreached(postDiagnosticEspCapture),
            payloadReached
                ? StateForReached(payloadCapture)
                : StateForUnreached(payloadCapture),
            newNodeReached
                ? StateForReached(newNodeCapture)
                : StateForUnreached(newNodeCapture));

        uint? capturedNextPointer = nextCaptured ? nextCapture.Value : null;
        bool? capturedDiagnosticReturn = diagnosticReached
            && diagnosticCapture.Kind == QueueAppendAttemptCaptureKind.Captured
                ? diagnosticCapture.Value
                : null;
        uint? capturedPayload = payloadReached
            && payloadCapture.Kind == QueueAppendAcquisitionCaptureKind.Captured
                ? payloadCapture.Value
                : null;
        uint? capturedNewNode = newNodeReached
            && newNodeCapture.Kind == QueueAppendAcquisitionCaptureKind.Captured
                ? newNodeCapture.Value
                : null;

        QueueAppendPublicationPlan publicationPlan = QueueAppendPublicationProjection.Project(
            new QueueAppendPublicationCapture(
                CapturedOldTailPointer: oldTail,
                CapturedNextPointer: capturedNextPointer,
                DiagnosticCallReturned: capturedDiagnosticReturn,
                CapturedPayloadArgument: capturedPayload,
                CapturedNewNodePointer: capturedNewNode));

        GameSocketCompletionErrorQueuePublicationLifecycleOutcome outcome =
            LifecycleOutcome(publicationPlan, states, diagnosticReturnedNormally);
        bool missingPostDiagnosticEsp =
            outcome == GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingPostDiagnosticEspCapture;
        foreach (QueueAppendPublicationAction action in publicationPlan.Actions)
        {
            if (missingPostDiagnosticEsp
                && action.Kind is (QueueAppendPublicationActionKind.ContinueAfterDiagnosticReturns
                    or QueueAppendPublicationActionKind.StopAtPayloadCaptureBoundary))
            {
                break;
            }

            trace.Add(new(
                trace.Count,
                null,
                action,
                null,
                MemoryEffect(action)));
        }
        if (missingPostDiagnosticEsp)
        {
            trace.Add(new(
                trace.Count,
                null,
                null,
                new(
                    GameSocketCompletionErrorQueuePublicationLifecycleActionKind
                        .StopAtMissingPostDiagnosticEspCapture,
                    PostDiagnosticStackCleanupRawFileOffset),
                null));
        }

        GameSocketCompletionErrorStateFlags? flagsAtStop =
            PublicationFlagsAtStop(
                attemptPlan.FlagsAtStop,
                publicationPlan,
                capturedNextPointer,
                postDiagnosticEspCaptured
                    ? postDiagnosticEspCapture.Value
                    : null,
                missingPostDiagnosticEsp);
        uint lastReached = missingPostDiagnosticEsp
            ? QueueAppendPublicationProjection.DiagnosticCallRawFileOffset
            : publicationPlan.Actions[^1].RawFileOffset;
        uint stopBefore = missingPostDiagnosticEsp
            ? PostDiagnosticStackCleanupRawFileOffset
            : lastReached;
        return new(
            captures,
            attemptPlan,
            publicationPlan,
            states,
            trace.ToImmutable(),
            outcome,
            GameSocketCompletionErrorQueuePublicationLifecycleDisposition.BoundedInGenericPublicationProjection,
            lastReached,
            stopBefore,
            flagsAtStop);
    }

    private static GameSocketCompletionErrorQueuePublicationLifecycleOutcome LifecycleOutcome(
        QueueAppendPublicationPlan plan,
        GameSocketCompletionErrorQueuePublicationCaptureStates states,
        bool diagnosticReturnedNormally)
    {
        if (plan.Outcome == QueueAppendPublicationOutcome.AwaitingNextPointerCapture)
        {
            return GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingNextPointerCapture;
        }

        if (plan.Outcome == QueueAppendPublicationOutcome.AwaitingDiagnosticReturnCapture)
        {
            return GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingDiagnosticReturnCapture;
        }

        if (plan.Outcome == QueueAppendPublicationOutcome.DiagnosticContinuationBoundary)
        {
            return GameSocketCompletionErrorQueuePublicationLifecycleOutcome.DiagnosticContinuationBoundary;
        }

        if (diagnosticReturnedNormally
            && states.PostDiagnosticEspBeforeCleanup
                == GameSocketCompletionErrorQueuePublicationCaptureState.Missing)
        {
            return GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingPostDiagnosticEspCapture;
        }

        if (plan.Outcome == QueueAppendPublicationOutcome.AwaitingPayloadCapture)
        {
            return GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingFreshPayloadCapture;
        }

        if (plan.Outcome == QueueAppendPublicationOutcome.AwaitingNewNodeCapture)
        {
            return GameSocketCompletionErrorQueuePublicationLifecycleOutcome.AwaitingFreshReloadedNewNodeCapture;
        }

        return GameSocketCompletionErrorQueuePublicationLifecycleOutcome.PublicationProjected;
    }

    private static GameSocketCompletionErrorQueuePublicationMemoryEffect? MemoryEffect(
        QueueAppendPublicationAction action) =>
        action.Kind switch
        {
            QueueAppendPublicationActionKind.CaptureOldTailNextPointer =>
                new(
                    GameSocketCompletionErrorQueuePublicationMemoryEffectKind.Read,
                    action.Pointer,
                    action.CapturedValue,
                    Attempted: true,
                    Captured: true,
                    ProjectedCommitted: false),
            QueueAppendPublicationActionKind.StopAtNextPointerCaptureBoundary =>
                new(
                    GameSocketCompletionErrorQueuePublicationMemoryEffectKind.Read,
                    action.Pointer,
                    null,
                    Attempted: false,
                    Captured: false,
                    ProjectedCommitted: false),
            QueueAppendPublicationActionKind.CapturePayloadArgument
                or QueueAppendPublicationActionKind.CaptureNewNodePointer =>
                new(
                    GameSocketCompletionErrorQueuePublicationMemoryEffectKind.Read,
                    null,
                    action.CapturedValue,
                    Attempted: true,
                    Captured: true,
                    ProjectedCommitted: false),
            QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail
                or QueueAppendPublicationActionKind.AttemptNewNodePublicationToOldTail =>
                new(
                    GameSocketCompletionErrorQueuePublicationMemoryEffectKind.Write,
                    action.Pointer,
                    action.CapturedValue,
                    Attempted: true,
                    Captured: true,
                    ProjectedCommitted: true),
            _ => null
        };

    private static GameSocketCompletionErrorStateFlags? PublicationFlagsAtStop(
        GameSocketCompletionErrorStateFlags? attemptFlags,
        QueueAppendPublicationPlan plan,
        uint? capturedNextPointer,
        uint? postDiagnosticEsp,
        bool missingPostDiagnosticEsp)
    {
        if (!capturedNextPointer.HasValue)
        {
            return attemptFlags;
        }

        GameSocketCompletionErrorStateFlags nextCompareFlags =
            CompareFlags(capturedNextPointer.Value, 0);
        if (plan.Outcome is QueueAppendPublicationOutcome.AwaitingDiagnosticReturnCapture
            or QueueAppendPublicationOutcome.DiagnosticContinuationBoundary)
        {
            return nextCompareFlags;
        }

        if (capturedNextPointer.Value == 0)
        {
            return nextCompareFlags;
        }

        if (missingPostDiagnosticEsp || !postDiagnosticEsp.HasValue)
        {
            return UnknownFlags;
        }

        return AddFlags(postDiagnosticEsp.Value, PostDiagnosticStackCleanupBytes);
    }

    private static GameSocketCompletionErrorQueuePublicationCaptureStates UnreachedStates(
        GameSocketCompletionErrorQueueChildPublicationCaptures captures) =>
        new(
            StateForUnreached(captures.OldTailNextPointer),
            StateForUnreached(captures.DiagnosticReturned),
            StateForUnreached(captures.PostDiagnosticEspBeforeCleanup),
            StateForUnreached(captures.FreshPayloadArgument),
            StateForUnreached(captures.FreshReloadedNewNode));

    private static GameSocketCompletionErrorQueuePublicationCaptureState StateForReached(
        QueueAppendAcquisitionCapture<uint> capture) =>
        capture.Kind switch
        {
            QueueAppendAcquisitionCaptureKind.Missing =>
                GameSocketCompletionErrorQueuePublicationCaptureState.Missing,
            QueueAppendAcquisitionCaptureKind.Captured =>
                GameSocketCompletionErrorQueuePublicationCaptureState.Captured,
            _ => throw InvalidCapture(nameof(capture), "A reached capture cannot be unreached.")
        };

    private static GameSocketCompletionErrorQueuePublicationCaptureState StateForReached(
        QueueAppendAttemptCapture<bool> capture) =>
        capture.Kind switch
        {
            QueueAppendAttemptCaptureKind.Missing =>
                GameSocketCompletionErrorQueuePublicationCaptureState.Missing,
            QueueAppendAttemptCaptureKind.Captured =>
                GameSocketCompletionErrorQueuePublicationCaptureState.Captured,
            _ => throw InvalidCapture(nameof(capture), "A reached capture cannot be unreached.")
        };

    private static GameSocketCompletionErrorQueuePublicationCaptureState StateForUnreached(
        QueueAppendAcquisitionCapture<uint> capture) =>
        capture.Kind == QueueAppendAcquisitionCaptureKind.Captured
            ? GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused
            : GameSocketCompletionErrorQueuePublicationCaptureState.NotReached;

    private static GameSocketCompletionErrorQueuePublicationCaptureState StateForUnreached(
        QueueAppendAttemptCapture<bool> capture) =>
        capture.Kind == QueueAppendAttemptCaptureKind.Captured
            ? GameSocketCompletionErrorQueuePublicationCaptureState.SuppliedButUnused
            : GameSocketCompletionErrorQueuePublicationCaptureState.NotReached;

    private static void RequireReached(
        QueueAppendAcquisitionCapture<uint> capture,
        string parameterName)
    {
        if (capture.Kind == QueueAppendAcquisitionCaptureKind.Unreached)
        {
            throw InvalidCapture(parameterName, "A reached capture cannot be marked unreached.");
        }
    }

    private static void RequireReached(
        QueueAppendAttemptCapture<bool> capture,
        string parameterName)
    {
        if (capture.Kind == QueueAppendAttemptCaptureKind.Unreached)
        {
            throw InvalidCapture(parameterName, "A reached capture cannot be marked unreached.");
        }
    }

    private static ArgumentException InvalidCapture(string parameterName, string message) =>
        new(message, parameterName);

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

    private static GameSocketCompletionErrorStateFlags AddFlags(uint left, uint right)
    {
        uint result = unchecked(left + right);
        return new(
            Carry: result < left,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((~(left ^ right) & (left ^ result) & 0x80000000) != 0));
    }

    private static bool HasEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static GameSocketCompletionErrorStateFlags UnknownFlags =>
        new(null, null, null, null, null, null);
}
