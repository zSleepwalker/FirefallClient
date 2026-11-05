using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityFirstFillLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityFirstFillLifecyclePlan nonzeroFillRoute =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                CompleteCapture(
                    frameMinusC: 3,
                    fillJoinEntryEdi: null,
                    frameMinus4: 0xA1B2C3D4,
                    freshFrameMinus8: 0x87654321));
        FrameHandoffCapacityFirstFillLifecyclePlan zeroFillRoute =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                CompleteCapture(
                    frameMinusC: 3,
                    fillJoinEntryEdi: null,
                    frameMinus4: 0xA1B2C3D4,
                    freshFrameMinus8: 0));
        check("A successful second-transfer lifecycle derives the gate and reaches both fill routes",
            IsComplete(nonzeroFillRoute)
            && nonzeroFillRoute.DerivedRepeatFillJoinGatePlan
            && nonzeroFillRoute.RepeatFillJoinPlan?.GateRepeatPlan.Capture.GatePlan
                == nonzeroFillRoute.SecondTransferLifecyclePlan.ReturnGatePlan
            && nonzeroFillRoute.RepeatFillJoinPlan?.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome
                    .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            && nonzeroFillRoute.RepeatFillJoinPlan?.DerivedFillJoinEntryEdi
                == 0x87654321
            && nonzeroFillRoute.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset
            && IsComplete(zeroFillRoute)
            && zeroFillRoute.RepeatFillJoinPlan?.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome
                    .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && zeroFillRoute.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset);

        FrameHandoffCapacityFirstFillLifecyclePlan zeroGateBypass =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                CompleteCapture(
                    frameMinusC: 0,
                    fillJoinEntryEdi: 0x4000,
                    frameMinus4: 0x5000,
                    includeRepeatCaptures: false));
        check("The second-return zero branch bypasses REP and keeps fill EDI independently captured",
            IsComplete(zeroGateBypass)
            && zeroGateBypass.RepeatFillJoinPlan?.GateRepeatPlan.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.ZeroBranchBypassedRepeat
            && zeroGateBypass.RepeatFillJoinPlan?.ZeroBranchBypassAdmission == true
            && zeroGateBypass.RepeatFillJoinPlan?.DerivedFillJoinEntryEdi == 0x4000
            && zeroGateBypass.RepeatFillJoinPlan?.FillJoinPlan?.Capture.EntryEdi == 0x4000
            && zeroGateBypass.RepeatFillJoinPlan?.FillJoinPlan?.EdiAfterMove == 0x5000);

        FrameHandoffCapacityFirstFillLifecyclePlan fillFrameMismatch =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                CompleteCapture(
                    frameMinusC: 3,
                    fillJoinEntryEbp: 0x4004,
                    fillJoinEntryEdi: 0x9999,
                    frameMinus4: 0x8888));
        FrameHandoffCapacityFirstFillLifecyclePlan returnFrameMismatch =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                CompleteCapture(returnEntryEbp: 0x4004));
        check("Fill-entry EBP continuity is checked before frame-minus-four, and the transfer lifecycle rejects a mismatched return frame",
            fillFrameMismatch.Outcome
                == FrameHandoffCapacityFirstFillLifecycleOutcome
                    .RepeatFillJoinHasUnreachedCaptures
            && fillFrameMismatch.RepeatFillJoinPlan?.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.FillJoinEntryEbpMismatch
            && fillFrameMismatch.RepeatFillJoinPlan?.FillJoinPlan is null
            && fillFrameMismatch.UnreachedFillJoinFrameMinus4CaptureProvided
            && !fillFrameMismatch.UnreachedFillJoinEntryEbpCaptureProvided
            && returnFrameMismatch.Outcome
                == FrameHandoffCapacityFirstFillLifecycleOutcome
                    .SecondReturnEntryIdentityMismatch
            && returnFrameMismatch.SecondTransferLifecyclePlan.ReturnGatePlan is null
            && returnFrameMismatch.RepeatFillJoinPlan is null
            && returnFrameMismatch.UnreachedFillJoinFrameMinus4CaptureProvided);

        FrameHandoffCapacityFirstFillLifecycleCapture laterCapturesOnMissingRead =
            CompleteCapture(
                callCell: null,
                frameMinus10: 0x1234,
                callReturned: true,
                includeRepeatCaptures: true,
                fillJoinEntryEdi: 0x99,
                frameMinus4: 0x88);
        FrameHandoffCapacityFirstFillLifecyclePlan missingRead =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                laterCapturesOnMissingRead);
        FrameHandoffCapacityFirstFillLifecyclePlan nonreturning =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                CompleteCapture(callReturned: false));
        FrameHandoffCapacityFirstFillLifecyclePlan missingReturnEax =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                CompleteCapture(helperReturnEax: null));
        check("A missing call-cell read, nonreturning call, or missing return EAX blocks all fill and REP captures",
            missingRead.Outcome
                == FrameHandoffCapacityFirstFillLifecycleOutcome
                    .SecondTransferHasUnreachedCaptures
            && missingRead.SecondTransferLifecyclePlan.TransferPlan.Outcome
                == FrameHandoffCapacitySecondTransferOutcome.MissingCallCellCapture
            && missingRead.RepeatFillJoinPlan is null
            && missingRead.UnreachedFillJoinFrameMinus4CaptureProvided
            && missingRead.UnreachedFillJoinEntryEbpCaptureProvided
            && missingRead.UnreachedDirectionFlagCaptureProvided
            && missingRead.UnreachedRepCompletionCaptureProvided
            && missingRead.UnreachedFreshFrameMinus8CaptureProvided
            && nonreturning.Outcome
                == FrameHandoffCapacityFirstFillLifecycleOutcome.SecondTransferBlocked
            && nonreturning.SecondTransferLifecyclePlan.TransferPlan.CallReached
            && nonreturning.SecondTransferLifecyclePlan.ReturnGatePlan is null
            && nonreturning.RepeatFillJoinPlan is null
            && nonreturning.UnreachedFillJoinFrameMinus4CaptureProvided
            && missingReturnEax.Outcome
                == FrameHandoffCapacityFirstFillLifecycleOutcome.SecondReturnBlocked
            && missingReturnEax.SecondTransferLifecyclePlan.ReturnGatePlan?.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.MissingHelperReturnEaxCapture
            && missingReturnEax.SecondTransferLifecyclePlan.ReturnGatePlan?.StackCleanupReached
                == true
            && missingReturnEax.SecondTransferLifecyclePlan.ReturnGatePlan?.EdxLeaReached
                == false
            && missingReturnEax.RepeatFillJoinPlan is null
            && missingReturnEax.UnreachedDirectionFlagCaptureProvided
            && missingReturnEax.UnreachedFillJoinFrameMinus4CaptureProvided);

        FrameHandoffCapacityFirstFillLifecyclePlan partialRep =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                CompleteCapture(
                    frameMinusC: 3,
                    directionFlagSet: false,
                    repReturnedNormally: false,
                    freshFrameMinus8: 0x1234,
                    fillJoinEntryEdi: 0x5678,
                    frameMinus4: 0x9ABC));
        check("Partial REP preserves completed-store evidence but cannot admit the later frame-minus-four read",
            partialRep.Outcome
                == FrameHandoffCapacityFirstFillLifecycleOutcome
                    .RepeatFillJoinHasUnreachedCaptures
            && partialRep.RepeatFillJoinPlan?.GateRepeatPlan.RepeatPlan
                is FrameHandoffCapacityRepeatStorePlan repeatPlan
            && repeatPlan.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.RepDidNotReturnNormally
            && !repeatPlan.AllStoresCompleted
            && repeatPlan.StoreDescriptor is null
            && partialRep.RepeatFillJoinPlan?.FillJoinPlan is null
            && partialRep.UnreachedFillJoinFrameMinus4CaptureProvided
            && !partialRep.RepeatFillJoinAdmissionValidated);

        FrameHandoffCapacityFirstFillLifecyclePlan wrapped =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(
                CompleteCapture(
                    entryEbp: 3,
                    entryEdi: 1,
                    entryEsi: 0x200,
                    entryEbx: uint.MaxValue,
                    frameMinusC: 0x123,
                    helperReturnEax: 2,
                    freshFrameMinus8: uint.MaxValue,
                    fillJoinEntryEdi: null,
                    frameMinus4: 0xAABBCCDD));
        check("Frame addresses and the independent post-call EBX return input preserve DWORD wraparound",
            IsComplete(wrapped)
            && wrapped.SecondTransferLifecyclePlan.TransferPlan.FrameMinusCWriteAddress
                == 0xFFFFFFF7
            && wrapped.SecondTransferLifecyclePlan.ReturnGatePlan?.Reads[0].Address
                == 0xFFFFFFF7
            && wrapped.SecondTransferLifecyclePlan.ReturnGatePlan?.Capture.EntryEbx
                == uint.MaxValue
            && wrapped.SecondTransferLifecyclePlan.ReturnGatePlan?.EdxAfterLea == 1
            && wrapped.RepeatFillJoinPlan?.FillJoinPlan?.FrameMinus4ReadAddress
                == uint.MaxValue
            && wrapped.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset);

        FrameHandoffCapacityFirstFillLifecycleCapture forgedGatePlanCapture =
            CompleteCapture(
                callReturned: false,
                includeRepeatCaptures: true,
                fillJoinEntryEdi: 0x7777,
                frameMinus4: 0x8888) with
            {
                RepeatFillJoinCapture = CompleteCapture().RepeatFillJoinCapture with
                {
                    GateRepeatCapture =
                        CompleteCapture().RepeatFillJoinCapture.GateRepeatCapture with
                        {
                            GatePlan = FrameHandoffCapacitySecondReturnGateProjection.Project(
                                new(
                                    ValidatedNormalEntry: true,
                                    EntryEbp: 0x4000,
                                    EntryEbx: 0x9999,
                                    FrameMinusC: 1,
                                    HelperReturnEax: 0))
                        },
                    FillJoinEntryEdi = 0x7777,
                    FrameMinus4 = 0x8888
                }
            };
        FrameHandoffCapacityFirstFillLifecyclePlan forgedGatePlan =
            FrameHandoffCapacityFirstFillLifecycleProjection.Project(forgedGatePlanCapture);
        check("A supplied successful-looking nullable gate plan cannot bypass a failed transfer lifecycle",
            forgedGatePlan.Outcome
                == FrameHandoffCapacityFirstFillLifecycleOutcome.SecondTransferBlocked
            && forgedGatePlan.SecondTransferLifecyclePlan.ReturnGatePlan is null
            && forgedGatePlan.RepeatFillJoinPlan is null
            && !forgedGatePlan.DerivedRepeatFillJoinGatePlan
            && forgedGatePlan.UnreachedFillJoinFrameMinus4CaptureProvided
            && forgedGatePlan.UnreachedFillJoinEntryEbpCaptureProvided
            && forgedGatePlan.UnreachedFreshFrameMinus8CaptureProvided);
    }

    private static FrameHandoffCapacityFirstFillLifecycleCapture CompleteCapture(
        uint entryEbp = 0x4000,
        uint entryEdi = 3,
        uint entryEsi = 0x20,
        uint entryEcx = 0x99,
        uint? callCell = 0x1234,
        uint? frameMinus10 = 0x5678,
        bool? callReturned = true,
        uint? frameMinusC = 3,
        uint entryEbx = 0x9000,
        uint? helperReturnEax = 0x100,
        uint? freshFrameMinus8 = 0x87654321,
        uint? returnEntryEbp = null,
        uint? fillJoinEntryEbp = null,
        uint? fillJoinEntryEdi = null,
        uint? frameMinus4 = 0xA1B2C3D4,
        bool? directionFlagSet = false,
        bool? repReturnedNormally = true,
        bool includeRepeatCaptures = true) =>
        new(
            SecondTransferLifecycleCapture: new(
                Transfer: new(
                    EntryEdi: entryEdi,
                    EntryEsi: entryEsi,
                    EntryEcx: entryEcx,
                    EntryEbp: entryEbp,
                    CallCellDword: callCell,
                    FrameMinus10Dword: frameMinus10,
                    CallReturnedNormally: callReturned),
                ReturnGate: new(
                    ValidatedNormalEntry: false,
                    EntryEbp: returnEntryEbp ?? entryEbp,
                    EntryEbx: entryEbx,
                    FrameMinusC: frameMinusC,
                    HelperReturnEax: helperReturnEax)),
            RepeatFillJoinCapture: new(
                GateRepeatCapture: new(
                    GatePlan: null,
                    RepeatEntryEcx: includeRepeatCaptures && frameMinusC is uint ecx
                        && ecx != 0 ? ecx : null,
                    RepeatEntryEdx: includeRepeatCaptures
                        ? unchecked(entryEbx + (helperReturnEax ?? 0))
                        : null,
                    RepeatEntryEbp: includeRepeatCaptures ? entryEbp : null,
                    DirectionFlagSet: includeRepeatCaptures ? directionFlagSet : null,
                    RepReturnedNormally: includeRepeatCaptures
                        ? repReturnedNormally
                        : null,
                    FreshFrameMinus8Value: includeRepeatCaptures
                        ? freshFrameMinus8
                        : null),
                FillJoinEntryEbp: fillJoinEntryEbp ?? entryEbp,
                FillJoinEntryEdi: fillJoinEntryEdi,
                FrameMinus4: frameMinus4));

    private static bool IsComplete(FrameHandoffCapacityFirstFillLifecyclePlan plan) =>
        plan.Outcome == FrameHandoffCapacityFirstFillLifecycleOutcome.ProjectionComplete
        && plan.SecondTransferLifecycleSuccessAdmission
        && plan.SecondTransferLifecycleUnreachedCapturesClear
        && plan.DerivedRepeatFillJoinGatePlan
        && plan.RepeatFillJoinAdmissionValidated
        && plan.RepeatFillJoinOutcomeClean
        && plan.RepeatFillJoinPlan is FrameHandoffCapacityRepeatFillJoinPlan fillPlan
        && fillPlan.ExactFillJoinOutcome
        && fillPlan.NoUnreachedFillJoinCaptures
        && !plan.UnreachedRepeatEntryEcxCaptureProvided
        && !plan.UnreachedRepeatEntryEdxCaptureProvided
        && !plan.UnreachedRepeatEntryEbpCaptureProvided
        && !plan.UnreachedDirectionFlagCaptureProvided
        && !plan.UnreachedRepCompletionCaptureProvided
        && !plan.UnreachedFreshFrameMinus8CaptureProvided
        && !plan.UnreachedFillJoinEntryEbpCaptureProvided
        && !plan.UnreachedFillJoinEntryEdiCaptureProvided
        && !plan.UnreachedFillJoinFrameMinus4CaptureProvided;
}
