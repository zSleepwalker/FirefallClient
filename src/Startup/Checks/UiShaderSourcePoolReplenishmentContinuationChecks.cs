using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolReplenishmentContinuationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed projection uses the pinned sample and stops at the loop repeat decision",
            UiShaderSourcePoolReplenishmentContinuationProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolReplenishmentContinuationProjectionRules.EntryRawFileOffset
                == 0x12CE0F0
            && UiShaderSourcePoolReplenishmentContinuationProjectionRules
                .RepeatDecisionEndRawFileOffsetExclusive == 0x12CE1F5
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.RepeatBranchRawFileOffset
                + 2
                == UiShaderSourcePoolReplenishmentContinuationProjectionRules
                    .RepeatDecisionEndRawFileOffsetExclusive);

        var repeatPath = Project(
            allocatorResult: 0,
            firstPushCallReturnedNormally: true,
            afterFirstPush: new UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture(
                AlignmentDivisor: 4,
                AlignmentDiagnosticCallReturnedNormally: null,
                InitialLoopBound20: 1,
                FirstIteration: new UiShaderSourcePoolReplenishmentContinuationIterationCapture(
                    Field14Value: 1,
                    Field1CValue: 2,
                    PoolDivisor: 4,
                    DiagnosticCallReturnedNormally: true,
                    PushCallReturnedNormally: true,
                    LaterBaseCapture: 0x1004,
                    FreshLoopBound20: 2)));
        check("The composed stages reuse their projections through exactly one entered iteration",
            repeatPath.Request.RequestBytes == 13
            && repeatPath.Result.TakesNullResultDiagnosticPath
            && repeatPath.Result.OutOfMemoryReportId == 0xDA
            && repeatPath.Result.PushHelperArgument0 == 0
            && repeatPath.Alignment?.Candidate == 4
            && repeatPath.ReachedInitialLoopGate == true
            && repeatPath.EnteredFirstIteration == true
            && repeatPath.FirstIteration?.Capture.Index == 0
            && repeatPath.FirstIteration?.EntryValue == 4
            && repeatPath.FirstIteration?.ReachedLoopDecision == true
            && repeatPath.FirstIteration?.NextIndex == 1
            && repeatPath.FirstIteration?.TakesUnsignedRepeatBranch == true
            && repeatPath.Outcome
                == UiShaderSourcePoolReplenishmentContinuationOutcome.RepeatDecisionReached);

        var initialGateExit = Project(
            allocatorResult: 0x1000,
            firstPushCallReturnedNormally: true,
            afterFirstPush: new UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture(
                AlignmentDivisor: 4,
                AlignmentDiagnosticCallReturnedNormally: null,
                InitialLoopBound20: 0,
                FirstIteration: null));
        check("A zero fresh initial bound reaches but does not enter the loop",
            initialGateExit.ReachedInitialLoopGate == true
            && initialGateExit.EnteredFirstIteration == false
            && initialGateExit.FirstIteration is null
            && initialGateExit.Outcome
                == UiShaderSourcePoolReplenishmentContinuationOutcome.InitialLoopGateNotEntered);

        var firstPushStop = Project(
            allocatorResult: 0x1000,
            firstPushCallReturnedNormally: false,
            afterFirstPush: null);
        check("A reached first-push call needs an explicit normal return before alignment",
            firstPushStop.Result.ReachesPushHelperCall
            && firstPushStop.Alignment is null
            && firstPushStop.ReachedInitialLoopGate is null
            && firstPushStop.Outcome
                == UiShaderSourcePoolReplenishmentContinuationOutcome.FirstPushDidNotReturnNormally);

        var nullDiagnosticStop = Project(
            allocatorResult: 0,
            firstPushCallReturnedNormally: null,
            afterFirstPush: null,
            nullPathDiagnosticCallsReturnedNormally: false);
        check("A nonreturning null-result diagnostic stops the composed path before the first push",
            nullDiagnosticStop.Result.TakesNullResultDiagnosticPath
            && !nullDiagnosticStop.Result.ReachesPushHelperCall
            && nullDiagnosticStop.Alignment is null
            && nullDiagnosticStop.Outcome
                == UiShaderSourcePoolReplenishmentContinuationOutcome
                    .NullPathDiagnosticDidNotReturnNormally);

        var alignmentFault = Project(
            allocatorResult: 0x1000,
            firstPushCallReturnedNormally: true,
            afterFirstPush: new UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture(
                AlignmentDivisor: 0,
                AlignmentDiagnosticCallReturnedNormally: null,
                InitialLoopBound20: null,
                FirstIteration: null));
        check("A zero alignment divisor stops before the initial gate",
            alignmentFault.Alignment?.Outcome
                == UiShaderSourcePoolReplenishmentAlignmentOutcome.DivideFaultAtFirstDivision
            && alignmentFault.ReachedInitialLoopGate is null
            && alignmentFault.Outcome
                == UiShaderSourcePoolReplenishmentContinuationOutcome.AlignmentDivideFault);

        var alignmentDiagnosticStop = Project(
            allocatorResult: 0xFFFFFFFB,
            firstPushCallReturnedNormally: true,
            afterFirstPush: new UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture(
                AlignmentDivisor: 7,
                AlignmentDiagnosticCallReturnedNormally: false,
                InitialLoopBound20: null,
                FirstIteration: null));
        check("A reached alignment diagnostic requires a captured normal return before the loop gate",
            alignmentDiagnosticStop.Alignment?.Outcome
                == UiShaderSourcePoolReplenishmentAlignmentOutcome.DiagnosticCallReached
            && alignmentDiagnosticStop.ReachedInitialLoopGate == false
            && alignmentDiagnosticStop.Outcome
                == UiShaderSourcePoolReplenishmentContinuationOutcome
                    .AlignmentDiagnosticDidNotReturnNormally);

        var iterationDiagnosticStop = Project(
            allocatorResult: 0x1000,
            firstPushCallReturnedNormally: true,
            afterFirstPush: new UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture(
                AlignmentDivisor: 4,
                AlignmentDiagnosticCallReturnedNormally: null,
                InitialLoopBound20: 1,
                FirstIteration: new UiShaderSourcePoolReplenishmentContinuationIterationCapture(
                    Field14Value: 1,
                    Field1CValue: 1,
                    PoolDivisor: 3,
                    DiagnosticCallReturnedNormally: false,
                    PushCallReturnedNormally: true,
                    LaterBaseCapture: null,
                    FreshLoopBound20: null)));
        check("A nonreturning reached iteration diagnostic prevents the iteration push and tail captures",
            iterationDiagnosticStop.FirstIteration?.ReachedDiagnostic == true
            && iterationDiagnosticStop.FirstIteration?.ReachedPush == false
            && iterationDiagnosticStop.FirstIteration?.ReachedLoopDecision == false
            && iterationDiagnosticStop.Outcome
                == UiShaderSourcePoolReplenishmentContinuationOutcome
                    .IterationDiagnosticDidNotReturnNormally);

        var iterationPushStop = Project(
            allocatorResult: 0x1000,
            firstPushCallReturnedNormally: true,
            afterFirstPush: new UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture(
                AlignmentDivisor: 4,
                AlignmentDiagnosticCallReturnedNormally: null,
                InitialLoopBound20: 1,
                FirstIteration: new UiShaderSourcePoolReplenishmentContinuationIterationCapture(
                    Field14Value: 1,
                    Field1CValue: 3,
                    PoolDivisor: 4,
                    DiagnosticCallReturnedNormally: true,
                    PushCallReturnedNormally: false,
                    LaterBaseCapture: null,
                    FreshLoopBound20: null)));
        check("A reached iteration push needs a captured normal return before the repeat decision",
            iterationPushStop.FirstIteration?.ReachedPush == true
            && iterationPushStop.FirstIteration?.ReachedLoopDecision == false
            && iterationPushStop.Outcome
                == UiShaderSourcePoolReplenishmentContinuationOutcome.IterationPushDidNotReturnNormally);

        check("The composition rejects future-stage captures when their control-flow boundary is not reached",
            ThrowsArgumentException(() => Project(
                allocatorResult: 0x1000,
                firstPushCallReturnedNormally: false,
                afterFirstPush: new UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture(
                    AlignmentDivisor: 4,
                    AlignmentDiagnosticCallReturnedNormally: null,
                    InitialLoopBound20: 0,
                    FirstIteration: null)))
            && ThrowsArgumentException(() => Project(
                allocatorResult: 0x1000,
                firstPushCallReturnedNormally: true,
                afterFirstPush: new UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture(
                    AlignmentDivisor: 4,
                    AlignmentDiagnosticCallReturnedNormally: null,
                    InitialLoopBound20: 1,
                    FirstIteration: null))));

        check("First-push return captures are required only when the call is reached",
            ThrowsArgumentException(() => Project(
                allocatorResult: 0,
                firstPushCallReturnedNormally: true,
                afterFirstPush: null,
                nullPathDiagnosticCallsReturnedNormally: false))
            && ThrowsArgumentException(() => Project(
                allocatorResult: 0x1000,
                firstPushCallReturnedNormally: null,
                afterFirstPush: null)));
    }

    private static UiShaderSourcePoolReplenishmentContinuationProjection Project(
        uint allocatorResult,
        bool? firstPushCallReturnedNormally,
        UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture? afterFirstPush,
        bool nullPathDiagnosticCallsReturnedNormally = true) =>
        UiShaderSourcePoolReplenishmentContinuationProjectionRules.Project(
            new UiShaderSourcePoolReplenishmentContinuationCapture(
                Request: new UiShaderSourcePoolReplenishmentCapture(
                    Field14Value: 2,
                    Field1CValue: 1,
                    Field20Value: 2,
                    Field18Value: 4),
                AllocatorResult: allocatorResult,
                PoolPointer: 0x20000000,
                NullPathDiagnosticCallsReturnedNormally: nullPathDiagnosticCallsReturnedNormally,
                FirstPushCallReturnedNormally: firstPushCallReturnedNormally,
                AfterFirstPush: afterFirstPush));

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
