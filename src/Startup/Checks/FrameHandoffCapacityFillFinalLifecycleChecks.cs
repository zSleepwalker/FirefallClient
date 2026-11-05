using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityFillFinalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityFillFinalLifecyclePlan zeroFillZeroGate =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(fillEntryEdi: 0, finalReceiver4: 0));
        FrameHandoffCapacityFillFinalLifecyclePlan zeroFillNonzeroGate =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 0,
                    finalReceiver4: 1,
                    finalEdi: 0xD000,
                    finalEbx: 0x11223344,
                    normalCallReturned: true));
        FrameHandoffCapacityFillFinalLifecyclePlan repeatZeroGate =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(fillEntryEdi: 5, finalReceiver4: 0));
        FrameHandoffCapacityFillFinalLifecyclePlan repeatNonzeroGate =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 5,
                    finalReceiver4: 1,
                    finalEdi: 0xD000,
                    finalEbx: 0x55667788,
                    normalCallReturned: true));
        check("Only the exact clean fill bypass or completed shared REP admits either final gate route",
            IsComplete(zeroFillZeroGate)
            && zeroFillZeroGate.FillPlan.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome
                    .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && zeroFillZeroGate.FillPlan.FillJoinPlan.BranchTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset
            && IsComplete(zeroFillNonzeroGate)
            && zeroFillNonzeroGate.FillPlan.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome
                    .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && IsComplete(repeatZeroGate)
            && repeatZeroGate.FillPlan.RepeatPlan?.AllStoresCompleted == true
            && IsComplete(repeatNonzeroGate)
            && repeatNonzeroGate.FillPlan.RepeatPlan?.AllStoresCompleted == true
            && zeroFillZeroGate.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && zeroFillNonzeroGate.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned
            && repeatZeroGate.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && repeatNonzeroGate.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned);

        FrameHandoffCapacityFillFinalLifecyclePlan frameMismatch =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 5,
                    normalCallReturned: true,
                    finalGateEntryEbp: 0x9999));
        check("A final gate EBP mismatch is rejected before its fresh frame-minus-fourteen read",
            frameMismatch.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.FinalGateEntryEbpMismatch
            && frameMismatch.FinalGateEntryEbpValidated
            && !frameMismatch.FinalGateEntryEbpMatches
            && !frameMismatch.DeriveFinalGateCaptureValidatedJoin
            && frameMismatch.FinalGatePlan?.Capture.CaptureValidatedJoin == false
            && frameMismatch.FinalGatePlan?.Reads.IsEmpty == true
            && frameMismatch.FinalGatePlan is { FrameMinus14ReadReached: false }
            && frameMismatch.UnreachedFinalFrameMinus14CaptureProvided
            && frameMismatch.UnreachedFinalReceiver4CaptureProvided
            && frameMismatch.UnreachedFinalCallStatusCaptureProvided
            && frameMismatch.UnreachedFinalReceiver8OldCaptureProvided
            && frameMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalCallGateProjection.EntryRawFileOffset);

        FrameHandoffCapacityFillFinalLifecyclePlan finalEntryEbpMismatch =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 5,
                    finalEbp: 0x9999));
        FrameHandoffCapacityFillFinalLifecyclePlan finalBypassEdiMismatch =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 5,
                    finalReceiver4: 0,
                    finalEdi: 0x5001));
        check("FinalLifecycle independently enforces its final EBP and zero-bypass EDI identities after preserving the gate reads",
            finalEntryEbpMismatch.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.FinalIdentityMismatch
            && finalEntryEbpMismatch.FinalGateEntryEbpMatches
            && finalEntryEbpMismatch.FinalGatePlan?.Reads.Length == 2
            && finalEntryEbpMismatch.FinalLifecyclePlan?.FinalEntryEbpValidated == true
            && finalEntryEbpMismatch.FinalLifecyclePlan?.FinalEntryEbpMatches == false
            && finalEntryEbpMismatch.FinalLifecyclePlan?.FinalPlan is null
            && finalEntryEbpMismatch.UnreachedFinalReceiver8OldCaptureProvided
            && finalBypassEdiMismatch.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.FinalIdentityMismatch
            && finalBypassEdiMismatch.FinalLifecyclePlan?.ZeroBypassEdiValidated == true
            && finalBypassEdiMismatch.FinalLifecyclePlan?.ZeroBypassEdiMatches == false
            && finalBypassEdiMismatch.FinalLifecyclePlan?.FinalPlan is null
            && finalBypassEdiMismatch.FinalGatePlan?.Reads.Length == 2
            && finalBypassEdiMismatch.UnreachedFinalReceiver8OldCaptureProvided);

        FrameHandoffCapacityFillFinalLifecyclePlan bypassEbxMismatch =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 5,
                    fillFrameMinus4: 0xA000,
                    finalReceiver4: 0,
                    finalEbx: 0xA001));
        check("A zero-call final EBX mismatch blocks before final reads or writes while retaining both gate reads",
            bypassEbxMismatch.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.FinalBypassEbxMismatch
            && bypassEbxMismatch.FinalGateEntryEbpMatches
            && bypassEbxMismatch.DeriveFinalGateCaptureValidatedJoin
            && bypassEbxMismatch.FinalGatePlan?.Capture.CaptureValidatedJoin == true
            && bypassEbxMismatch.FinalGatePlan?.Reads.Length == 2
            && bypassEbxMismatch.FinalGatePlan?.Reads[0].Address == 0x1000
            && bypassEbxMismatch.FinalGatePlan?.Reads[1].Address == 0x5004
            && bypassEbxMismatch.FinalBypassEbxValidated
            && !bypassEbxMismatch.FinalBypassEbxMatches
            && bypassEbxMismatch.FinalLifecyclePlan is null
            && bypassEbxMismatch.UnreachedFinalReceiver8OldCaptureProvided
            && bypassEbxMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset);

        FrameHandoffCapacityFillFinalLifecyclePlan missingDirection =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 5,
                    normalCallReturned: true,
                    fillDirectionFlag: null));
        FrameHandoffCapacityFillFinalLifecyclePlan partialRep =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 5,
                    fillDirectionFlag: false,
                    fillRepCompleted: false));
        check("Missing DF and partial REP retain their fill effects and leave every final read unreached",
            missingDirection.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.FillRouteNotAdmitted
            && missingDirection.FillPlan.RepeatPlan is FrameHandoffCapacitySharedRepeatPlan noDf
            && noDf.XorEaxReached
            && noDf.RepInstructionReached
            && noDf.DirectionFlagSet is null
            && noDf.StoreDescriptor is null
            && missingDirection.UnreachedFinalFrameMinus14CaptureProvided
            && missingDirection.UnreachedFinalReceiver4CaptureProvided
            && missingDirection.UnreachedFinalCallStatusCaptureProvided
            && missingDirection.UnreachedFinalReceiver8OldCaptureProvided
            && missingDirection.FinalGatePlan?.Reads.IsEmpty == true
            && partialRep.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.FillRouteNotAdmitted
            && partialRep.FillPlan.RepeatPlan is FrameHandoffCapacitySharedRepeatPlan incomplete
            && incomplete.DirectionFlagSet == false
            && !incomplete.AllStoresCompleted
            && incomplete.EcxAfterRep is null
            && incomplete.StoreDescriptor is null
            && partialRep.UnreachedFinalFrameMinus14CaptureProvided
            && partialRep.FinalGatePlan?.Reads.IsEmpty == true);

        FrameHandoffCapacityFillFinalLifecyclePlan independentRegisters =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 5,
                    fillFrameMinus4: 0xABCDEF01,
                    finalFrameMinus14: 0x12345000,
                    finalReceiver4: 0,
                    finalEdi: 0x12345000,
                    finalEsi: 0xCAFEBABE,
                    finalEbx: 0xABCDEF01));
        FrameHandoffCapacityFillFinalLifecyclePlan nonzeroCallRegisters =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 5,
                    fillFrameMinus4: 0xABCDEF01,
                    finalFrameMinus14: 0x12345000,
                    finalReceiver4: 1,
                    normalCallReturned: true,
                    finalEdi: 0xDEADBEEF,
                    finalEsi: 0x76543210,
                    finalEbx: 0x0BADF00D));
        check("Fill frame-minus-four supplies EBX only on final zero bypass; frame-minus-fourteen, ESI, and opaque-call registers stay independent",
            IsComplete(independentRegisters)
            && independentRegisters.FinalLifecyclePlan?.Capture.Final.EntryEbx
                == 0xABCDEF01
            && independentRegisters.FinalLifecyclePlan?.Capture.Final.EntryEdi
                == 0x12345000
            && independentRegisters.FinalLifecyclePlan?.Capture.Final.EntryEsi
                == 0xCAFEBABE
            && independentRegisters.FinalGatePlan?.Reads[0].Value == 0x12345000
            && independentRegisters.FinalBypassEbxValidated
            && independentRegisters.FinalBypassEbxMatches
            && independentRegisters.DeriveFinalEntryEbxFromFillJoin
            && IsComplete(nonzeroCallRegisters)
            && nonzeroCallRegisters.FinalLifecyclePlan?.Capture.Final.EntryEbx
                == 0x0BADF00D
            && nonzeroCallRegisters.FinalLifecyclePlan?.Capture.Final.EntryEdi
                == 0xDEADBEEF
            && nonzeroCallRegisters.FinalLifecyclePlan?.Capture.Final.EntryEsi
                == 0x76543210
            && !nonzeroCallRegisters.FinalBypassEbxValidated
            && !nonzeroCallRegisters.DeriveFinalEntryEbxFromFillJoin);

        FrameHandoffCapacityFillFinalLifecyclePlan wrapping =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEbp: 3,
                    fillEntryEdi: 0,
                    fillFrameMinus4: uint.MaxValue,
                    finalGateEntryEbp: 3,
                    finalFrameMinus14: uint.MaxValue - 1,
                    finalReceiver4: 0,
                    finalEdi: uint.MaxValue - 1,
                    finalEbx: uint.MaxValue,
                    finalEbp: 3,
                    finalReceiver8Old: uint.MaxValue));
        check("DWORD-wrapped fill and final frame addresses remain independent and ordered through RET 4",
            IsComplete(wrapping)
            && wrapping.FillPlan.FillJoinPlan.FrameMinus4ReadAddress == uint.MaxValue
            && wrapping.FinalGatePlan?.Reads[0].Address == 0xFFFFFFEF
            && wrapping.FinalGatePlan?.Reads[1].Address == 2
            && wrapping.FinalLifecyclePlan?.FinalPlan?.MemoryAccesses[0].Address == 6
            && wrapping.FinalLifecyclePlan?.FinalPlan?.MemoryAccesses[1].Address == 6
            && wrapping.FinalLifecyclePlan?.FinalPlan?.MemoryAccesses[2].Address == 2
            && wrapping.FinalLifecyclePlan?.FinalPlan?.ReturnImmediateBytes == 4);

        FrameHandoffCapacityFillFinalLifecyclePlan flaggedZeroRoute =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 0,
                    fillExpectedRepeatEntryEcx: 1));
        FrameHandoffCapacityFillFinalLifecyclePlan flaggedZeroDirection =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(
                CompleteCapture(
                    fillEntryEdi: 0,
                    fillZeroRepeatCaptures: true));
        check("A supplied repeat capture on the zero fill bypass is unreached and cannot admit the final gate",
            flaggedZeroRoute.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.FillRouteNotAdmitted
            && flaggedZeroRoute.FillPlan.UnreachedExpectedRepeatEntryEcxCaptureProvided
            && flaggedZeroRoute.FinalGatePlan?.Reads.IsEmpty == true
            && flaggedZeroRoute.UnreachedFinalReceiver8OldCaptureProvided
            && flaggedZeroDirection.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.FillRouteNotAdmitted
            && flaggedZeroDirection.FillPlan.UnreachedDirectionFlagCaptureProvided
            && flaggedZeroDirection.FillPlan.UnreachedRepCompletionCaptureProvided
            && flaggedZeroDirection.FinalGatePlan?.Reads.IsEmpty == true);
    }

    private static FrameHandoffCapacityFillFinalLifecycleCapture CompleteCapture(
        uint fillEntryEbp = 0x1014,
        uint fillEntryEdi = 0,
        uint fillFrameMinus4 = 0xA000,
        uint? fillExpectedRepeatEntryEcx = null,
        uint? fillExpectedRepeatEntryEdi = null,
        bool? fillDirectionFlag = false,
        bool? fillRepCompleted = true,
        bool fillZeroRepeatCaptures = false,
        uint? finalGateEntryEbp = null,
        uint? finalFrameMinus14 = 0x5000,
        uint? finalReceiver4 = 0,
        bool? normalCallReturned = null,
        uint finalEdi = 0x5000,
        uint finalEsi = 5,
        uint finalEbx = 0xA000,
        uint? finalEbp = null,
        uint? finalReceiver8Old = 0xFFFFFFFC) =>
        new(
            Fill: new(
                FillJoinCapture: new(
                    CaptureValidatedJoin: true,
                    EntryEbp: fillEntryEbp,
                    EntryEdi: fillEntryEdi,
                    FrameMinus4: fillFrameMinus4),
                ExpectedRepeatEntryEcx: fillExpectedRepeatEntryEcx,
                ExpectedRepeatEntryEdi: fillExpectedRepeatEntryEdi,
                DirectionFlagSet:
                    fillEntryEdi == 0 && !fillZeroRepeatCaptures
                        ? null
                        : fillDirectionFlag,
                RepCompletedNormally:
                    fillEntryEdi == 0 && !fillZeroRepeatCaptures
                        ? null
                        : fillRepCompleted),
            Final: new(
                Gate: new(
                    CaptureValidatedJoin: false,
                    EntryEbp: finalGateEntryEbp ?? fillEntryEbp,
                    FrameMinus14: finalFrameMinus14,
                    Receiver4: finalReceiver4,
                    NormalCallReturned: normalCallReturned),
                Final: new(
                    ValidatedNormalTail: false,
                    EntryEdi: finalEdi,
                    EntryEsi: finalEsi,
                    EntryEbx: finalEbx,
                    EntryEbp: finalEbp ?? finalGateEntryEbp ?? fillEntryEbp,
                    Receiver8Old: finalReceiver8Old)));

    private static bool IsComplete(FrameHandoffCapacityFillFinalLifecyclePlan plan) =>
        plan.Outcome == FrameHandoffCapacityFillFinalLifecycleOutcome.ProjectionComplete
        && plan.FillOutcomeClean
        && plan.FillAdmissionSatisfied
        && plan.FinalGateEntryEbpValidated
        && plan.FinalGateEntryEbpMatches
        && plan.DeriveFinalGateCaptureValidatedJoin
        && plan.FinalGatePlan?.Capture.CaptureValidatedJoin == true
        && plan.FinalLifecyclePlan is FrameHandoffCapacityFinalLifecyclePlan lifecyclePlan
        && lifecyclePlan.GatePlan.NormalBoundaryAdmitted
        && lifecyclePlan.DeriveValidatedNormalTail
        && lifecyclePlan.FinalPlan?.ReturnReached == true
        && plan.FinalLifecycleOutcomeClean
        && !plan.UnreachedFinalFrameMinus14CaptureProvided
        && !plan.UnreachedFinalReceiver4CaptureProvided
        && !plan.UnreachedFinalCallStatusCaptureProvided
        && !plan.UnreachedFinalReceiver8OldCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryRawFileOffsetExclusive;
}
