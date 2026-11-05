using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityAboveFinalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityAboveFinalLifecyclePlan aboveZeroFinalZero =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(CreateCapture());
        FrameHandoffCapacityAboveFinalLifecyclePlan aboveZeroFinalCall =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(
                    finalGateReceiver4: 1,
                    finalCallReturned: true,
                    finalEdi: 0xD000,
                    finalEsi: 17,
                    finalEbx: 0x11223344,
                    receiver8Old: 0xFFFFFFFE));
        FrameHandoffCapacityAboveFinalLifecyclePlan aboveRepeatFinalZero =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(aboveEntryEsi: 3));
        FrameHandoffCapacityAboveFinalLifecyclePlan aboveRepeatFinalCall =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(
                    aboveEntryEsi: 3,
                    finalGateReceiver4: 1,
                    finalCallReturned: true,
                    finalEdi: 0xE000,
                    finalEsi: 0xFFFFFFFE,
                    finalEbx: 0x55667788,
                    receiver8Old: 1));
        check("Above zero and completed shared REP each compose with final zero bypass and returned call",
            IsComplete(aboveZeroFinalZero)
            && aboveZeroFinalZero.AboveRepeatPlan.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.ZeroBranchTaken
            && aboveZeroFinalZero.FinalGatePlan?.Capture.CaptureValidatedJoin == true
            && aboveZeroFinalZero.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && IsComplete(aboveZeroFinalCall)
            && aboveZeroFinalCall.AboveRepeatPlan.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.ZeroBranchTaken
            && aboveZeroFinalCall.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned
            && aboveZeroFinalCall.FinalLifecyclePlan?.FinalPlan?.Capture.EntryEdi == 0xD000
            && aboveZeroFinalCall.FinalLifecyclePlan?.FinalPlan?.Capture.EntryEsi == 17
            && aboveZeroFinalCall.FinalLifecyclePlan?.FinalPlan?.Capture.EntryEbx == 0x11223344
            && aboveZeroFinalCall.FinalLifecyclePlan?.FinalPlan?.Receiver8ValueAfterAdd == 15
            && IsComplete(aboveRepeatFinalZero)
            && aboveRepeatFinalZero.AboveRepeatPlan.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.ProjectionComplete
            && aboveRepeatFinalZero.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && IsComplete(aboveRepeatFinalCall)
            && aboveRepeatFinalCall.AboveRepeatPlan.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.ProjectionComplete
            && aboveRepeatFinalCall.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned
            && aboveRepeatFinalCall.FinalLifecyclePlan?.FinalPlan?.Capture.EntryEsi == 0xFFFFFFFE
            && aboveRepeatFinalCall.FinalLifecyclePlan?.FinalPlan?.Capture.EntryEbx == 0x55667788
            && aboveRepeatFinalCall.FinalLifecyclePlan?.FinalPlan?.Receiver8ValueAfterAdd
                == uint.MaxValue);

        FrameHandoffCapacityAboveFinalLifecyclePlan gateFrameMismatch =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(
                    finalGateEbp: 0x2018,
                    finalGateFrameMinus14: 0x7000,
                    finalGateReceiver4: 0,
                    finalCallReturned: true,
                    receiver8Old: 0x1234));
        FrameHandoffCapacityAboveFinalLifecyclePlan finalFrameMismatch =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(finalReturnEbp: 0x9999));
        FrameHandoffCapacityAboveFinalLifecyclePlan bypassEdiMismatch =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(finalEdi: 0x5004, receiver8Old: 0x1234));
        check("Both frame identities and zero-bypass EDI are rejected before the final receiver8 read",
            gateFrameMismatch.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalGateEntryEbpMismatch
            && gateFrameMismatch.FinalGatePlan is null
            && gateFrameMismatch.FinalGateEntryEbpValidated
            && !gateFrameMismatch.FinalGateEntryEbpMatches
            && gateFrameMismatch.UnreachedFinalGateFrameMinus14CaptureProvided
            && gateFrameMismatch.UnreachedFinalGateReceiver4CaptureProvided
            && gateFrameMismatch.UnreachedFinalGateNormalCallStatusProvided
            && gateFrameMismatch.UnreachedFinalReturnReceiver8OldCaptureProvided
            && gateFrameMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalCallGateProjection.EntryRawFileOffset
            && finalFrameMismatch.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalEntryEbpMismatch
            && finalFrameMismatch.FinalLifecyclePlan?.GatePlan.Reads.Length == 2
            && finalFrameMismatch.FinalLifecyclePlan?.FinalPlan is null
            && finalFrameMismatch.FinalLifecyclePlan?.UnreachedFinalReceiver8OldCaptureProvided == true
            && finalFrameMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset
            && bypassEdiMismatch.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalBypassEdiMismatch
            && bypassEdiMismatch.FinalLifecyclePlan?.GatePlan.Reads.Length == 2
            && bypassEdiMismatch.FinalLifecyclePlan?.FinalPlan is null
            && bypassEdiMismatch.UnreachedFinalReturnReceiver8OldCaptureProvided
            && bypassEdiMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset);

        FrameHandoffCapacityAboveFinalLifecyclePlan bypassEbxMismatch =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(finalEbx: 0xAABBCCDE, receiver8Old: 0x1234));
        FrameHandoffCapacityAboveFinalLifecyclePlan bypassEsiMismatch =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(finalEsi: 1, receiver8Old: 0x1234));
        check("Final zero-call bypass validates preserved EBX and ESI after gate reads but before receiver8",
            bypassEbxMismatch.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalBypassEbxMismatch
            && bypassEbxMismatch.FinalGatePlan?.Capture.CaptureValidatedJoin == true
            && bypassEbxMismatch.FinalGatePlan?.Reads.Length == 2
            && bypassEbxMismatch.FinalBypassEbxValidated
            && !bypassEbxMismatch.FinalBypassEbxMatches
            && !bypassEbxMismatch.FinalBypassEsiValidated
            && bypassEbxMismatch.FinalLifecyclePlan is null
            && bypassEbxMismatch.UnreachedFinalReturnReceiver8OldCaptureProvided
            && bypassEbxMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset
            && bypassEsiMismatch.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalBypassEsiMismatch
            && bypassEsiMismatch.FinalGatePlan?.Reads.Length == 2
            && bypassEsiMismatch.FinalBypassEbxMatches
            && bypassEsiMismatch.FinalBypassEsiValidated
            && !bypassEsiMismatch.FinalBypassEsiMatches
            && bypassEsiMismatch.FinalLifecyclePlan is null
            && bypassEsiMismatch.UnreachedFinalReturnReceiver8OldCaptureProvided);

        FrameHandoffCapacityAboveFinalLifecyclePlan incompleteRepeat =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(
                    aboveEntryEsi: 3,
                    repCompletedNormally: false,
                    finalGateReceiver4: 1,
                    finalCallReturned: true,
                    receiver8Old: 0x1234));
        check("An incomplete shared REP stops upstream and marks all later final-gate/tail captures unreached",
            incompleteRepeat.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.AboveRepeatBlocked
            && incompleteRepeat.AboveRepeatPlan.SharedRepeatPlan?.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.RepDidNotCompleteNormally
            && incompleteRepeat.FinalGatePlan is null
            && incompleteRepeat.UnreachedFinalGateFrameMinus14CaptureProvided
            && incompleteRepeat.UnreachedFinalGateReceiver4CaptureProvided
            && incompleteRepeat.UnreachedFinalGateNormalCallStatusProvided
            && incompleteRepeat.UnreachedFinalReturnReceiver8OldCaptureProvided
            && incompleteRepeat.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySharedRepeatProjection.RepStosdRawFileOffset);

        FrameHandoffCapacityAboveFinalLifecyclePlan missingGateRead =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(finalGateFrameMinus14: null,
                    finalGateReceiver4: 1, finalCallReturned: true));
        check("A missing final frame read propagates unreached receiver and call captures",
            missingGateRead.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalGateHasUnreachedCaptures
            && missingGateRead.UnreachedFinalGateReceiver4CaptureProvided
            && missingGateRead.UnreachedFinalGateNormalCallStatusProvided
            && missingGateRead.UnreachedFinalReturnReceiver8OldCaptureProvided
            && missingGateRead.FinalLifecyclePlan is null);

        FrameHandoffCapacityAboveFinalLifecyclePlan wrapping =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                CreateCapture(
                    entryEbp: 3,
                    finalGateFrameMinus14: uint.MaxValue - 1,
                    finalEdi: uint.MaxValue - 1,
                    receiver8Old: uint.MaxValue));
        check("Wrapping frame and receiver addresses retain ordered writes and normal return",
            IsComplete(wrapping)
            && wrapping.FinalGatePlan?.Reads[0].Address == 0xFFFFFFEF
            && wrapping.FinalGatePlan?.Reads[1].Address == 2
            && wrapping.FinalLifecyclePlan?.FinalPlan?.MemoryAccesses.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Read,
                    0x00BE6CB1,
                    6,
                    uint.MaxValue),
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
                    0x00BE6CB1,
                    6,
                    uint.MaxValue),
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
                    0x00BE6CB4,
                    2,
                    0xAABBCCDD)
            }) == true
            && wrapping.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveFinalLifecycleProjection.LifecycleEndRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityAboveFinalLifecycleCapture CreateCapture(
        uint aboveEntryEsi = 0,
        uint entryEbp = 0x2014,
        bool? repCompletedNormally = true,
        uint? finalGateEbp = null,
        uint? finalGateFrameMinus14 = 0x5000,
        uint? finalGateReceiver4 = 0,
        bool? finalCallReturned = null,
        uint? finalEdi = null,
        uint? finalEsi = null,
        uint? finalEbx = null,
        uint? finalReturnEbp = null,
        uint? receiver8Old = 0xFFFFFFFC) =>
        new(
            AboveRepeat: new(
                AboveReturnGate: new(
                    ValidatedNormalEntry: true,
                    EntryEbp: entryEbp,
                    EntryEbx: 0x1000,
                    EntryEsi: aboveEntryEsi,
                    HelperReturnEax: 0,
                    FrameMinus4: 0xAABBCCDD),
                RepeatEntryEcx: aboveEntryEsi == 0 ? null : aboveEntryEsi,
                RepeatEntryEdi: aboveEntryEsi == 0 ? null : 0x1000,
                DirectionFlagSet: aboveEntryEsi == 0 ? null : false,
                RepCompletedNormally: aboveEntryEsi == 0 ? null : repCompletedNormally),
            FinalGate: new(
                CaptureValidatedJoin: false,
                EntryEbp: finalGateEbp ?? entryEbp,
                FrameMinus14: finalGateFrameMinus14,
                Receiver4: finalGateReceiver4,
                NormalCallReturned: finalCallReturned),
            FinalReturn: new(
                ValidatedNormalTail: false,
                EntryEdi: finalEdi ?? finalGateFrameMinus14 ?? 0x5000,
                EntryEsi: finalEsi ?? aboveEntryEsi,
                EntryEbx: finalEbx ?? 0xAABBCCDD,
                EntryEbp: finalReturnEbp ?? entryEbp,
                Receiver8Old: receiver8Old));

    private static bool IsComplete(FrameHandoffCapacityAboveFinalLifecyclePlan plan) =>
        plan.Outcome == FrameHandoffCapacityAboveFinalLifecycleOutcome.ProjectionComplete
        && plan.AboveRepeatAdmissionSatisfied
        && plan.FinalGateCaptureValidatedJoinDerived
        && plan.FinalGatePlan is
        {
            Capture.CaptureValidatedJoin: true,
            NormalBoundaryAdmitted: true,
            UnreachedFrameMinus14CaptureProvided: false,
            UnreachedReceiver4CaptureProvided: false,
            UnreachedNormalCallStatusProvided: false
        }
        && plan.FinalLifecyclePlan?.Outcome
            == FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete
        && plan.FinalLifecyclePlan?.FinalPlan?.ReturnReached == true
        && plan.FinalOutcomeClean
        && !plan.UnreachedFinalGateFrameMinus14CaptureProvided
        && !plan.UnreachedFinalGateReceiver4CaptureProvided
        && !plan.UnreachedFinalGateNormalCallStatusProvided
        && !plan.UnreachedFinalReturnReceiver8OldCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityAboveFinalLifecycleProjection.LifecycleEndRawFileOffsetExclusive;
}
