namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveResidueJoinCapture(
    UnreliableReceiveResidueHandoffEntry HandoffEntry,
    UnreliableReceiveResidueHandoffCapture HandoffCapture,
    UnreliableReceiveResidueCounterCapture? CounterCapture,
    UnreliableReceiveJoinedHeaderCapture? JoinedCapture);

public enum UnreliableReceiveResidueJoinOutcome
{
    EarlyJbeHasUnreachedHandoffCaptures,
    EarlyJbeHasCounterCapture,
    MissingJoinedCaptureAfterEarlyJbe,
    HandoffDidNotReturnToExclusiveBoundary,
    MissingCounterCapture,
    CounterPriorHelperDidNotReturnNormally,
    CounterReceiverDoesNotMatchHandoff,
    CounterRetainedEsiDoesNotMatchHandoffResidue,
    CounterDidNotComplete,
    MissingJoinedCapture,
    JoinedReceiverDoesNotMatchHandoff,
    JoinedHeaderDidNotComplete,
    JoinedZeroBranchTaken,
    JoinedNonzeroBranchFallthrough
}

public readonly record struct UnreliableReceiveResidueJoinPlan(
    UnreliableReceiveResidueJoinCapture Capture,
    UnreliableReceiveResidueJoinOutcome Outcome,
    UnreliableReceiveResidueHandoffPlan Handoff,
    UnreliableReceiveResidueCounterPlan? Counter,
    UnreliableReceiveJoinedHeaderPlan? Joined,
    bool HandoffCompleted,
    bool CounterCompleted,
    bool JoinedCompleted,
    bool? EarlyJbeTaken,
    bool? JoinedZeroBranchTaken,
    bool? JoinedNonzeroBranchFallthrough,
    bool UnreachedCounterCaptureProvided,
    bool UnreachedJoinedCaptureProvided,
    uint StopAtRawFileOffsetExclusive)
{
    public bool IsTerminalSuccess =>
        Outcome is UnreliableReceiveResidueJoinOutcome.JoinedZeroBranchTaken
            or UnreliableReceiveResidueJoinOutcome.JoinedNonzeroBranchFallthrough;
}

/// <summary>
/// Joins the residue handoff, its optional normal-return counter update, and the joined header.
/// </summary>
public static class UnreliableReceiveResidueJoinProjection
{
    public static UnreliableReceiveResidueJoinPlan Project(
        UnreliableReceiveResidueHandoffEntry entry,
        UnreliableReceiveResidueHandoffCapture handoffCapture,
        UnreliableReceiveResidueCounterCapture? counterCapture,
        UnreliableReceiveJoinedHeaderCapture? joinedCapture)
    {
        var capture = new UnreliableReceiveResidueJoinCapture(
            entry,
            handoffCapture,
            counterCapture,
            joinedCapture);
        UnreliableReceiveResidueHandoffPlan handoff =
            UnreliableReceiveResidueHandoffProjection.Project(entry, handoffCapture);

        if (handoff.Outcome == UnreliableReceiveResidueHandoffOutcome.EarlyJbeTaken)
        {
            if (handoff.ReceiverDwordAtOffset04Read
                || handoff.ReceiverDwordAtOffset18Read
                || handoff.HelperCallReached
                || handoff.UnreachedReceiverDwordAtOffset04CaptureProvided
                || handoff.UnreachedReceiverDwordAtOffset18CaptureProvided
                || handoff.UnreachedHelperReturnCaptureProvided)
            {
                return Create(
                    capture,
                    UnreliableReceiveResidueJoinOutcome.EarlyJbeHasUnreachedHandoffCaptures,
                    handoff,
                    null,
                    null,
                    handoffCompleted: false,
                    counterCompleted: false,
                    joinedCompleted: false,
                    handoff.EarlyJbeTaken,
                    joinedZeroBranchTaken: null,
                    joinedNonzeroBranchFallthrough: null,
                    unreachedCounterCaptureProvided: counterCapture.HasValue,
                    unreachedJoinedCaptureProvided: joinedCapture.HasValue,
                    UnreliableReceiveResidueHandoffProjection.EarlyBranchTargetRawFileOffset);
            }

            if (counterCapture.HasValue)
            {
                return Create(
                    capture,
                    UnreliableReceiveResidueJoinOutcome.EarlyJbeHasCounterCapture,
                    handoff,
                    null,
                    null,
                    handoffCompleted: false,
                    counterCompleted: false,
                    joinedCompleted: false,
                    handoff.EarlyJbeTaken,
                    joinedZeroBranchTaken: null,
                    joinedNonzeroBranchFallthrough: null,
                    unreachedCounterCaptureProvided: true,
                    unreachedJoinedCaptureProvided: joinedCapture.HasValue,
                    UnreliableReceiveResidueHandoffProjection.EarlyBranchTargetRawFileOffset);
            }

            if (!joinedCapture.HasValue)
            {
                return Create(
                    capture,
                    UnreliableReceiveResidueJoinOutcome.MissingJoinedCaptureAfterEarlyJbe,
                    handoff,
                    null,
                    null,
                    handoffCompleted: true,
                    counterCompleted: false,
                    joinedCompleted: false,
                    handoff.EarlyJbeTaken,
                    joinedZeroBranchTaken: null,
                    joinedNonzeroBranchFallthrough: null,
                    unreachedCounterCaptureProvided: false,
                    unreachedJoinedCaptureProvided: false,
                    UnreliableReceiveJoinedHeaderProjection.StartRawFileOffset);
            }

            return ProjectJoined(
                capture,
                handoff,
                counter: null,
                joinedCapture.Value,
                handoffCompleted: true,
                counterCompleted: false,
                handoff.EarlyJbeTaken);
        }

        if (handoff.Outcome != UnreliableReceiveResidueHandoffOutcome.HelperReturnedToExclusiveBoundary)
        {
            return Create(
                capture,
                UnreliableReceiveResidueJoinOutcome.HandoffDidNotReturnToExclusiveBoundary,
                handoff,
                null,
                null,
                handoffCompleted: false,
                counterCompleted: false,
                joinedCompleted: false,
                handoff.EarlyJbeTaken,
                joinedZeroBranchTaken: null,
                joinedNonzeroBranchFallthrough: null,
                unreachedCounterCaptureProvided: counterCapture.HasValue,
                unreachedJoinedCaptureProvided: joinedCapture.HasValue,
                handoff.StopAtRawFileOffsetExclusive);
        }

        if (!counterCapture.HasValue)
        {
            return Create(
                capture,
                UnreliableReceiveResidueJoinOutcome.MissingCounterCapture,
                handoff,
                null,
                null,
                handoffCompleted: true,
                counterCompleted: false,
                joinedCompleted: false,
                handoff.EarlyJbeTaken,
                joinedZeroBranchTaken: null,
                joinedNonzeroBranchFallthrough: null,
                unreachedCounterCaptureProvided: false,
                unreachedJoinedCaptureProvided: joinedCapture.HasValue,
                UnreliableReceiveResidueCounterProjection.CounterUpdateStartRawFileOffset);
        }

        UnreliableReceiveResidueCounterCapture counterInput = counterCapture.Value;
        if (counterInput.PriorHelperReturnedNormally != true)
        {
            return Create(
                capture,
                UnreliableReceiveResidueJoinOutcome.CounterPriorHelperDidNotReturnNormally,
                handoff,
                null,
                null,
                handoffCompleted: true,
                counterCompleted: false,
                joinedCompleted: false,
                handoff.EarlyJbeTaken,
                joinedZeroBranchTaken: null,
                joinedNonzeroBranchFallthrough: null,
                unreachedCounterCaptureProvided: false,
                unreachedJoinedCaptureProvided: joinedCapture.HasValue,
                UnreliableReceiveResidueCounterProjection.CounterUpdateStartRawFileOffset);
        }

        if (counterInput.ReceiverEdi != handoffCapture.ReceiverBaseAddress)
        {
            return Create(
                capture,
                UnreliableReceiveResidueJoinOutcome.CounterReceiverDoesNotMatchHandoff,
                handoff,
                null,
                null,
                handoffCompleted: true,
                counterCompleted: false,
                joinedCompleted: false,
                handoff.EarlyJbeTaken,
                joinedZeroBranchTaken: null,
                joinedNonzeroBranchFallthrough: null,
                unreachedCounterCaptureProvided: false,
                unreachedJoinedCaptureProvided: joinedCapture.HasValue,
                UnreliableReceiveResidueCounterProjection.CounterUpdateStartRawFileOffset);
        }

        if (counterInput.RetainedEsi != handoff.HandoffResidue)
        {
            return Create(
                capture,
                UnreliableReceiveResidueJoinOutcome.CounterRetainedEsiDoesNotMatchHandoffResidue,
                handoff,
                null,
                null,
                handoffCompleted: true,
                counterCompleted: false,
                joinedCompleted: false,
                handoff.EarlyJbeTaken,
                joinedZeroBranchTaken: null,
                joinedNonzeroBranchFallthrough: null,
                unreachedCounterCaptureProvided: false,
                unreachedJoinedCaptureProvided: joinedCapture.HasValue,
                UnreliableReceiveResidueCounterProjection.CounterUpdateStartRawFileOffset);
        }

        UnreliableReceiveResidueCounterPlan counter =
            UnreliableReceiveResidueCounterProjection.Project(counterInput);
        if (counter.Outcome != UnreliableReceiveResidueCounterOutcome.CounterDwordAdded)
        {
            return Create(
                capture,
                UnreliableReceiveResidueJoinOutcome.CounterDidNotComplete,
                handoff,
                counter,
                null,
                handoffCompleted: true,
                counterCompleted: false,
                joinedCompleted: false,
                handoff.EarlyJbeTaken,
                joinedZeroBranchTaken: null,
                joinedNonzeroBranchFallthrough: null,
                unreachedCounterCaptureProvided: false,
                unreachedJoinedCaptureProvided: joinedCapture.HasValue,
                counter.StopAtRawFileOffsetExclusive);
        }

        if (!joinedCapture.HasValue)
        {
            return Create(
                capture,
                UnreliableReceiveResidueJoinOutcome.MissingJoinedCapture,
                handoff,
                counter,
                null,
                handoffCompleted: true,
                counterCompleted: true,
                joinedCompleted: false,
                handoff.EarlyJbeTaken,
                joinedZeroBranchTaken: null,
                joinedNonzeroBranchFallthrough: null,
                unreachedCounterCaptureProvided: false,
                unreachedJoinedCaptureProvided: false,
                UnreliableReceiveJoinedHeaderProjection.StartRawFileOffset);
        }

        return ProjectJoined(
            capture,
            handoff,
            counter,
            joinedCapture.Value,
            handoffCompleted: true,
            counterCompleted: true,
            handoff.EarlyJbeTaken);
    }

    private static UnreliableReceiveResidueJoinPlan ProjectJoined(
        UnreliableReceiveResidueJoinCapture capture,
        UnreliableReceiveResidueHandoffPlan handoff,
        UnreliableReceiveResidueCounterPlan? counter,
        UnreliableReceiveJoinedHeaderCapture joinedCapture,
        bool handoffCompleted,
        bool counterCompleted,
        bool? earlyJbeTaken)
    {
        if (joinedCapture.ReceiverBaseAddress != capture.HandoffCapture.ReceiverBaseAddress)
        {
            return Create(
                capture,
                UnreliableReceiveResidueJoinOutcome.JoinedReceiverDoesNotMatchHandoff,
                handoff,
                counter,
                null,
                handoffCompleted,
                counterCompleted,
                joinedCompleted: false,
                earlyJbeTaken,
                joinedZeroBranchTaken: null,
                joinedNonzeroBranchFallthrough: null,
                unreachedCounterCaptureProvided: false,
                unreachedJoinedCaptureProvided: false,
                UnreliableReceiveJoinedHeaderProjection.StartRawFileOffset);
        }

        UnreliableReceiveJoinedHeaderPlan joined =
            UnreliableReceiveJoinedHeaderProjection.Project(joinedCapture);
        if (joined.Outcome is UnreliableReceiveJoinedHeaderOutcome.ZeroBranchTaken
            or UnreliableReceiveJoinedHeaderOutcome.NonzeroBranchFallthrough)
        {
            bool zeroBranchTaken =
                joined.Outcome == UnreliableReceiveJoinedHeaderOutcome.ZeroBranchTaken;
            return Create(
                capture,
                zeroBranchTaken
                    ? UnreliableReceiveResidueJoinOutcome.JoinedZeroBranchTaken
                    : UnreliableReceiveResidueJoinOutcome.JoinedNonzeroBranchFallthrough,
                handoff,
                counter,
                joined,
                handoffCompleted,
                counterCompleted,
                joinedCompleted: true,
                earlyJbeTaken,
                zeroBranchTaken,
                !zeroBranchTaken,
                unreachedCounterCaptureProvided: false,
                unreachedJoinedCaptureProvided: false,
                joined.StopAtRawFileOffsetExclusive);
        }

        return Create(
            capture,
            UnreliableReceiveResidueJoinOutcome.JoinedHeaderDidNotComplete,
            handoff,
            counter,
            joined,
            handoffCompleted,
            counterCompleted,
            joinedCompleted: false,
            earlyJbeTaken,
            joined.ZeroBranchTaken,
            joined.ZeroBranchTaken is bool zeroBranch && !zeroBranch,
            unreachedCounterCaptureProvided: false,
            unreachedJoinedCaptureProvided: false,
            joined.StopAtRawFileOffsetExclusive);
    }

    private static UnreliableReceiveResidueJoinPlan Create(
        UnreliableReceiveResidueJoinCapture capture,
        UnreliableReceiveResidueJoinOutcome outcome,
        UnreliableReceiveResidueHandoffPlan handoff,
        UnreliableReceiveResidueCounterPlan? counter,
        UnreliableReceiveJoinedHeaderPlan? joined,
        bool handoffCompleted,
        bool counterCompleted,
        bool joinedCompleted,
        bool? earlyJbeTaken,
        bool? joinedZeroBranchTaken,
        bool? joinedNonzeroBranchFallthrough,
        bool unreachedCounterCaptureProvided,
        bool unreachedJoinedCaptureProvided,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            handoff,
            counter,
            joined,
            handoffCompleted,
            counterCompleted,
            joinedCompleted,
            earlyJbeTaken,
            joinedZeroBranchTaken,
            joinedNonzeroBranchFallthrough,
            unreachedCounterCaptureProvided,
            unreachedJoinedCaptureProvided,
            stopAtRawFileOffsetExclusive);
}
