using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityFinalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityFinalLifecyclePlan zero =
            FrameHandoffCapacityFinalLifecycleProjection.Project(CompleteCapture());
        FrameHandoffCapacityFinalLifecyclePlan nonzero =
            FrameHandoffCapacityFinalLifecycleProjection.Project(
                CompleteCapture(
                    entryEbp: 0x2014,
                    frameMinus14: 0x4000,
                    receiver4: 1,
                    normalCallReturned: true,
                    finalEdi: 0xD000,
                    finalEsi: 7,
                    finalEbx: 0x11223344));
        check("Zero bypass and returned nonzero call admit the exact final return tail",
            zero.Outcome == FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete
            && zero.RequireNormalBoundaryAdmission
            && zero.NormalBoundaryAdmissionSatisfied
            && zero.GateOutcomeClean
            && zero.GateUnreachedCapturesClear
            && zero.GatePlan.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && zero.FinalPlan?.Capture.ValidatedNormalTail == true
            && zero.DeriveValidatedNormalTail
            && zero.FinalOutcomeClean
            && zero.FinalPlan?.Outcome
                == FrameHandoffCapacityFinalReturnOutcome.ReturnedNormally
            && zero.FinalPlan?.ReturnReached == true
            && nonzero.Outcome == FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete
            && nonzero.GatePlan.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned
            && nonzero.FinalPlan?.Outcome
                == FrameHandoffCapacityFinalReturnOutcome.ReturnedNormally
            && nonzero.FinalPlan?.ReturnReached == true
            && nonzero.FinalPlan?.Capture.EntryEdi == 0xD000
            && nonzero.Capture.Gate.FrameMinus14 == 0x4000
            && nonzero.Capture.Final.EntryEdi != nonzero.Capture.Gate.FrameMinus14
            && nonzero.FinalPlan?.Capture.EntryEsi == 7
            && nonzero.FinalPlan?.Capture.EntryEbx == 0x11223344);

        FrameHandoffCapacityFinalLifecyclePlan zeroWithCallStatus =
            FrameHandoffCapacityFinalLifecycleProjection.Project(
                CompleteCapture(normalCallReturned: true));
        FrameHandoffCapacityFinalLifecyclePlan missingGateRead =
            FrameHandoffCapacityFinalLifecycleProjection.Project(
                CompleteCapture(
                    frameMinus14: null,
                    receiver4: 0,
                    normalCallReturned: true));
        FrameHandoffCapacityFinalLifecyclePlan nonreturningCall =
            FrameHandoffCapacityFinalLifecycleProjection.Project(
                CompleteCapture(
                    receiver4: 1,
                    normalCallReturned: false));
        check("Unreached zero-call status, missing gate read, and nonreturning call block the final",
            zeroWithCallStatus.Outcome
                == FrameHandoffCapacityFinalLifecycleOutcome.GateHasUnreachedCaptures
            && !zeroWithCallStatus.NormalBoundaryAdmissionSatisfied
            && zeroWithCallStatus.GatePlan.UnreachedNormalCallStatusProvided
            && zeroWithCallStatus.FinalPlan is null
            && zeroWithCallStatus.UnreachedFinalReceiver8OldCaptureProvided
            && missingGateRead.Outcome
                == FrameHandoffCapacityFinalLifecycleOutcome.GateHasUnreachedCaptures
            && missingGateRead.GatePlan.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.MissingFrameMinus14
            && missingGateRead.FinalPlan is null
            && missingGateRead.GatePlan.UnreachedReceiver4CaptureProvided
            && missingGateRead.UnreachedFinalReceiver8OldCaptureProvided
            && nonreturningCall.Outcome
                == FrameHandoffCapacityFinalLifecycleOutcome.GateBlocked
            && nonreturningCall.GatePlan.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallDidNotReturn
            && nonreturningCall.FinalPlan is null
            && nonreturningCall.GatePlan.CallReached
            && !nonreturningCall.GatePlan.NormalBoundaryAdmitted);

        FrameHandoffCapacityFinalLifecyclePlan frameMismatch =
            FrameHandoffCapacityFinalLifecycleProjection.Project(
                CompleteCapture(finalEbp: 0x9999));
        FrameHandoffCapacityFinalLifecyclePlan zeroEdiMismatch =
            FrameHandoffCapacityFinalLifecycleProjection.Project(
                CompleteCapture(finalEdi: 0x3001));
        check("Frame identity and zero-bypass EDI are validated before the old receiver8 read",
            frameMismatch.Outcome
                == FrameHandoffCapacityFinalLifecycleOutcome.FinalEntryEbpMismatch
            && frameMismatch.FinalEntryEbpValidated
            && !frameMismatch.FinalEntryEbpMatches
            && frameMismatch.FinalPlan is null
            && frameMismatch.UnreachedFinalReceiver8OldCaptureProvided
            && frameMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset
            && zeroEdiMismatch.Outcome
                == FrameHandoffCapacityFinalLifecycleOutcome.ZeroBypassEdiMismatch
            && zeroEdiMismatch.FinalEntryEbpMatches
            && zeroEdiMismatch.ZeroBypassEdiValidated
            && !zeroEdiMismatch.ZeroBypassEdiMatches
            && zeroEdiMismatch.FinalPlan is null
            && zeroEdiMismatch.UnreachedFinalReceiver8OldCaptureProvided
            && zeroEdiMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset);

        FrameHandoffCapacityFinalLifecyclePlan missingOldValue =
            FrameHandoffCapacityFinalLifecycleProjection.Project(
                CompleteCapture(receiver8Old: null));
        check("An admitted tail reaches the old receiver8 read but cannot return without its value",
            missingOldValue.Outcome == FrameHandoffCapacityFinalLifecycleOutcome.FinalBlocked
            && missingOldValue.DeriveValidatedNormalTail
            && missingOldValue.FinalPlan?.Outcome
                == FrameHandoffCapacityFinalReturnOutcome.MissingReceiver8OldCapture
            && missingOldValue.FinalPlan?.Receiver8ReadReached == true
            && missingOldValue.FinalPlan?.Receiver8OldCaptured == false
            && missingOldValue.FinalPlan?.MemoryAccesses.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Read,
                    0x00BE6CB1,
                    0x3008,
                    null)
            }) == true
            && missingOldValue.FinalPlan?.ReturnReached == false
            && !missingOldValue.UnreachedFinalReceiver8OldCaptureProvided);

        FrameHandoffCapacityFinalLifecyclePlan wrapping =
            FrameHandoffCapacityFinalLifecycleProjection.Project(
                CompleteCapture(
                    entryEbp: 3,
                    frameMinus14: uint.MaxValue - 7,
                    receiver4: 0,
                    finalEdi: uint.MaxValue - 7,
                    finalEsi: 2,
                    finalEbx: uint.MaxValue,
                    finalEbp: 3,
                    receiver8Old: uint.MaxValue));
        check("DWORD-wrapped final writes stay ordered before the exact RET 4",
            wrapping.Outcome == FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete
            && wrapping.GatePlan.Reads[0].Address == 0xFFFFFFEF
            && wrapping.FinalPlan?.Capture.EntryEdi == uint.MaxValue - 7
            && wrapping.FinalPlan?.MemoryAccesses.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Read,
                    0x00BE6CB1,
                    0,
                    uint.MaxValue),
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
                    0x00BE6CB1,
                    0,
                    1),
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
                    0x00BE6CB4,
                    uint.MaxValue - 3,
                    uint.MaxValue)
            }) == true
            && wrapping.FinalPlan?.Receiver8WriteReached == true
            && wrapping.FinalPlan?.Receiver4StoreReached == true
            && wrapping.FinalPlan?.StackPopRegisters.SequenceEqual(
                new[] { "EDI", "ESI", "EBX", "EBP" }) == true
            && wrapping.FinalPlan?.EspSetFromEbpReached == true
            && wrapping.FinalPlan?.ReturnReached == true
            && wrapping.FinalPlan?.ReturnImmediateBytes == 4
            && wrapping.FinalPlan?.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityFinalLifecycleCapture CompleteCapture(
        uint entryEbp = 0x1014,
        uint? frameMinus14 = 0x3000,
        uint? receiver4 = 0,
        bool? normalCallReturned = null,
        uint finalEdi = 0x3000,
        uint finalEsi = 5,
        uint finalEbx = 0xAABBCCDD,
        uint? finalEbp = null,
        uint? receiver8Old = 0xFFFFFFFC) =>
        new(
            Gate: new(
                CaptureValidatedJoin: true,
                EntryEbp: entryEbp,
                FrameMinus14: frameMinus14,
                Receiver4: receiver4,
                NormalCallReturned: normalCallReturned),
            Final: new(
                ValidatedNormalTail: false,
                EntryEdi: finalEdi,
                EntryEsi: finalEsi,
                EntryEbx: finalEbx,
                EntryEbp: finalEbp ?? entryEbp,
                Receiver8Old: receiver8Old));
}
