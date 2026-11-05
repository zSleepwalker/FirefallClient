namespace Firefall.Reconstruction.Startup;

public enum ReliableReceiveForwardingReturnStopReason
{
    CallerDidNotCompleteCleanup,
    TailCaptureProvidedBeforeCallerCleanup,
    MissingTailCapture,
    TailRouteMismatch,
    TailReceiverPointerMismatch,
    TailDidNotCompleteReturn,
    ReturnCompleted
}

[Flags]
public enum ReliableReceiveForwardingReturnCompletedStages
{
    None = 0,
    CallerCleanup = 1,
    RecycleReturn = 2
}

public readonly record struct ReliableReceiveForwardingReturnPlan(
    ReliableReceiveForwardingCallerPlan CallerPlan,
    ReliableReceiveRecycleReturnPlan? RecycleReturnPlan,
    ReliableReceiveForwardingReturnStopReason StopReason,
    ReliableReceiveForwardingReturnCompletedStages CompletedStages,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the observed forwarding caller cleanup with its recycle-return tail.
/// </summary>
public static class ReliableReceiveForwardingReturnProjection
{
    public static ReliableReceiveForwardingReturnPlan Project(
        ReliableReceiveForwardingCallerCapture callerCapture,
        ReliableReceiveRecycleReturnCapture? tailCapture)
    {
        ReliableReceiveForwardingCallerPlan callerPlan =
            ReliableReceiveForwardingCallerProjection.Project(callerCapture);

        if (callerPlan.Outcome != ReliableReceiveForwardingCallerOutcome.CleanupCompleted)
        {
            return new(
                callerPlan,
                null,
                tailCapture.HasValue
                    ? ReliableReceiveForwardingReturnStopReason.TailCaptureProvidedBeforeCallerCleanup
                    : ReliableReceiveForwardingReturnStopReason.CallerDidNotCompleteCleanup,
                ReliableReceiveForwardingReturnCompletedStages.None,
                callerPlan.StopAtRawFileOffsetExclusive);
        }

        if (tailCapture is not ReliableReceiveRecycleReturnCapture tail)
        {
            return new(
                callerPlan,
                null,
                ReliableReceiveForwardingReturnStopReason.MissingTailCapture,
                ReliableReceiveForwardingReturnCompletedStages.CallerCleanup,
                callerPlan.StopAtRawFileOffsetExclusive);
        }

        ReliableReceiveRecycleReturnRoute expectedRoute = callerCapture.Route switch
        {
            ReliableReceiveForwardingCallerRoute.HighDelta =>
                ReliableReceiveRecycleReturnRoute.HighDelta,
            ReliableReceiveForwardingCallerRoute.ExistingRecord =>
                ReliableReceiveRecycleReturnRoute.ExistingRecord,
            _ => throw new ArgumentOutOfRangeException(nameof(callerCapture))
        };
        if (expectedRoute != tail.Route)
        {
            return new(
                callerPlan,
                null,
                ReliableReceiveForwardingReturnStopReason.TailRouteMismatch,
                ReliableReceiveForwardingReturnCompletedStages.CallerCleanup,
                callerPlan.StopAtRawFileOffsetExclusive);
        }

        if (callerPlan.ReceiverOffset8DwordLoadedIntoEax is not uint callerReceiverEax
            || callerReceiverEax != tail.ReceiverPointerInEax)
        {
            return new(
                callerPlan,
                null,
                ReliableReceiveForwardingReturnStopReason.TailReceiverPointerMismatch,
                ReliableReceiveForwardingReturnCompletedStages.CallerCleanup,
                callerPlan.StopAtRawFileOffsetExclusive);
        }

        ReliableReceiveRecycleReturnPlan tailPlan =
            ReliableReceiveRecycleReturnProjection.Project(tail);
        if (tailPlan.Outcome != ReliableReceiveRecycleReturnOutcome.ReturnCompleted)
        {
            return new(
                callerPlan,
                tailPlan,
                ReliableReceiveForwardingReturnStopReason.TailDidNotCompleteReturn,
                ReliableReceiveForwardingReturnCompletedStages.CallerCleanup,
                tailPlan.StopAtRawFileOffsetExclusive);
        }

        return new(
            callerPlan,
            tailPlan,
            ReliableReceiveForwardingReturnStopReason.ReturnCompleted,
            ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
                | ReliableReceiveForwardingReturnCompletedStages.RecycleReturn,
            tailPlan.StopAtRawFileOffsetExclusive);
    }
}
