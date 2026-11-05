using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityFirstFinalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        bool matrixComplete = true;
        bool matrixInputsIndependent = true;
        foreach (bool firstGateNonzero in new[] { false, true })
        foreach (bool fillEntryEdiNonzero in new[] { false, true })
        foreach (bool finalReceiver4Nonzero in new[] { false, true })
        {
            FrameHandoffCapacityFirstFinalLifecyclePlan plan =
                FrameHandoffCapacityFirstFinalLifecycleProjection.Project(CompleteCapture(
                    firstGateNonzero,
                    fillEntryEdiNonzero ? 0x6000u : 0u,
                    finalReceiver4Nonzero));
            matrixComplete &= plan.Outcome
                == FrameHandoffCapacityFirstFinalLifecycleOutcome.ProjectionComplete
                && plan.RepeatFillJoinAdmissionSatisfied
                && plan.FillFinalProjectionReached
                && plan.FillFinalLifecyclePlan?.Outcome
                    == FrameHandoffCapacityFillFinalLifecycleOutcome.ProjectionComplete
                && plan.FillFinalLifecyclePlan?.FinalGatePlan?.Outcome
                    == (finalReceiver4Nonzero
                        ? FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned
                        : FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped)
                && plan.RepeatFillJoinPlan.Outcome
                    == (firstGateNonzero
                        ? fillEntryEdiNonzero
                            ? FrameHandoffCapacityRepeatFillJoinOutcome
                                .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
                            : FrameHandoffCapacityRepeatFillJoinOutcome
                                .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
                        : fillEntryEdiNonzero
                            ? FrameHandoffCapacityRepeatFillJoinOutcome
                                .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
                            : FrameHandoffCapacityRepeatFillJoinOutcome
                                .ZeroEntryEdiBranchToRawFileOffsetBE6CA2)
                && plan.FillFinalLifecyclePlan?.FinalLifecyclePlan?.Capture.Final.EntryEsi
                    == 0xCAFEBABE;
            matrixInputsIndependent &= plan.FillFinalLifecyclePlan?.FinalLifecyclePlan
                    ?.Capture.Final.EntryEsi == 0xCAFEBABE
                && plan.FillFinalLifecyclePlan?.FinalLifecyclePlan
                    ?.Capture.Final.EntryEdi
                    == (finalReceiver4Nonzero ? 0xDEADBEEFu : 0x5000u);
        }

        check("All first-gate, fill-join, and final-call route combinations compose as sequential projections",
            matrixComplete && matrixInputsIndependent);

        FrameHandoffCapacityFirstFinalLifecyclePlan partialFirstRep =
            FrameHandoffCapacityFirstFinalLifecycleProjection.Project(CompleteCapture(
                firstGateNonzero: true,
                fillEntryEdi: 0x6000,
                finalReceiver4Nonzero: true,
                gateEbp: 0x3000,
                firstRepCompleted: false));
        FrameHandoffCapacityFirstFinalLifecyclePlan completeFirstRepMissingReload =
            FrameHandoffCapacityFirstFinalLifecycleProjection.Project(CompleteCapture(
                firstGateNonzero: true,
                fillEntryEdi: 0x6000,
                finalReceiver4Nonzero: true,
                gateEbp: 0x3000,
                omitFirstFrameMinus8: true));
        check("Upstream REP failures retain reached first-store state and mark later shared-repeat and final captures unreached",
            partialFirstRep.RepeatFillJoinPlan.GateRepeatPlan.RepeatPlan
                is FrameHandoffCapacityRepeatStorePlan partialStores
            && partialStores.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.RepDidNotReturnNormally
            && !partialStores.AllStoresCompleted
            && partialFirstRep.FillFinalLifecyclePlan is null
            && partialFirstRep.UnreachedSharedRepeatEntryEcxCaptureProvided
            && partialFirstRep.UnreachedSharedRepeatEntryEdiCaptureProvided
            && partialFirstRep.UnreachedSharedRepeatDirectionFlagCaptureProvided
            && partialFirstRep.UnreachedSharedRepeatCompletionCaptureProvided
            && partialFirstRep.UnreachedFinalFrameMinus14CaptureProvided
            && partialFirstRep.UnreachedFinalReceiver4CaptureProvided
            && partialFirstRep.UnreachedFinalCallStatusCaptureProvided
            && partialFirstRep.UnreachedFinalReceiver8OldCaptureProvided
            && completeFirstRepMissingReload.RepeatFillJoinPlan.GateRepeatPlan.RepeatPlan
                is FrameHandoffCapacityRepeatStorePlan completedStores
            && completedStores.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.MissingFreshFrameMinus8Capture
            && completedStores.AllStoresCompleted
            && completedStores.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 1, 4, 0)
            && completedStores.EcxAfterRep == 0
            && completedStores.FreshFrameMinus8ReadReached
            && completedStores.FreshFrameMinus8Value is null
            && completeFirstRepMissingReload.FillFinalLifecyclePlan is null
            && completeFirstRepMissingReload.UnreachedSharedRepeatDirectionFlagCaptureProvided
            && completeFirstRepMissingReload.UnreachedSharedRepeatCompletionCaptureProvided
            && completeFirstRepMissingReload.UnreachedFinalFrameMinus14CaptureProvided
            && completeFirstRepMissingReload.UnreachedFinalReceiver8OldCaptureProvided);

        FrameHandoffCapacityFirstFinalLifecyclePlan partialSharedRep =
            FrameHandoffCapacityFirstFinalLifecycleProjection.Project(CompleteCapture(
                firstGateNonzero: false,
                fillEntryEdi: 0x6000,
                finalReceiver4Nonzero: true,
                sharedRepeatCompleted: false));
        check("A partial shared REP is retained without reaching any final gate reads",
            partialSharedRep.Outcome
                == FrameHandoffCapacityFirstFinalLifecycleOutcome.FillFinalRouteNotAdmitted
            && partialSharedRep.RepeatFillJoinAdmissionSatisfied
            && partialSharedRep.FillFinalLifecyclePlan?.FillPlan.RepeatPlan
                is FrameHandoffCapacitySharedRepeatPlan partialSharedStores
            && partialSharedStores.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.RepDidNotCompleteNormally
            && !partialSharedStores.AllStoresCompleted
            && partialSharedStores.StoreDescriptor is null
            && partialSharedRep.FillFinalLifecyclePlan?.FinalGatePlan?.Reads.IsEmpty == true
            && partialSharedRep.UnreachedFinalFrameMinus14CaptureProvided
            && partialSharedRep.UnreachedFinalReceiver4CaptureProvided
            && partialSharedRep.UnreachedFinalCallStatusCaptureProvided
            && partialSharedRep.UnreachedFinalReceiver8OldCaptureProvided);

        FrameHandoffCapacityFirstFinalLifecyclePlan freshFrame4 =
            FrameHandoffCapacityFirstFinalLifecycleProjection.Project(CompleteCapture(
                firstGateNonzero: false,
                fillEntryEdi: 0x6000,
                fillFrameMinus4: 0xABCDEF01,
                finalReceiver4Nonzero: false,
                finalEbx: 0xABCDEF01));
        check("The admitted fill-join capture supplies an independent fresh frame-minus-four value to the downstream lifecycle",
            freshFrame4.DerivedFillFinalFillJoinCapture
                == freshFrame4.RepeatFillJoinPlan.FillJoinPlan?.Capture
            && freshFrame4.FillFinalLifecyclePlan?.FillPlan.FillJoinPlan.Capture.FrameMinus4
                == 0xABCDEF01
            && freshFrame4.FillFinalLifecyclePlan?.FinalLifecyclePlan?.Capture.Final.EntryEbx
                == 0xABCDEF01
            && freshFrame4.FillFinalLifecyclePlan?.DeriveFinalEntryEbxFromFillJoin == true);

        FrameHandoffCapacityFirstFinalLifecyclePlan firstJoinEbpMismatch =
            FrameHandoffCapacityFirstFinalLifecycleProjection.Project(CompleteCapture(
                firstGateNonzero: false,
                fillEntryEdi: 0x6000,
                gateEbp: 0x2000,
                fillJoinEbp: 0x2004));
        FrameHandoffCapacityFirstFinalLifecyclePlan sharedIdentityMismatch =
            FrameHandoffCapacityFirstFinalLifecycleProjection.Project(CompleteCapture(
                firstGateNonzero: false,
                fillEntryEdi: 0x6000,
                expectedSharedRepeatEntryEcx: 0x6001));
        FrameHandoffCapacityFirstFinalLifecyclePlan finalEbpMismatch =
            FrameHandoffCapacityFirstFinalLifecycleProjection.Project(CompleteCapture(
                firstGateNonzero: false,
                fillEntryEdi: 0x6000,
                finalEbp: 0x9999));
        check("Join and repeat identity mismatches block only their downstream stages while final EBP identity remains downstream-owned",
            firstJoinEbpMismatch.Outcome
                == FrameHandoffCapacityFirstFinalLifecycleOutcome
                    .RepeatFillJoinHasUnreachedCaptures
            && firstJoinEbpMismatch.RepeatFillJoinPlan.FillJoinPlan is null
            && firstJoinEbpMismatch.FillFinalLifecyclePlan is null
            && firstJoinEbpMismatch.UnreachedFinalFrameMinus14CaptureProvided
            && sharedIdentityMismatch.Outcome
                == FrameHandoffCapacityFirstFinalLifecycleOutcome.FillFinalRouteNotAdmitted
            && sharedIdentityMismatch.FillFinalLifecyclePlan?.FillPlan.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome.RepeatEntryIdentityMismatch
            && sharedIdentityMismatch.FillFinalLifecyclePlan?.FinalGatePlan?.Reads.IsEmpty == true
            && finalEbpMismatch.Outcome
                == FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalIdentityMismatch
            && finalEbpMismatch.FillFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalEntryEbpMatches == false);

        FrameHandoffCapacityFirstFinalLifecyclePlan wrapped =
            FrameHandoffCapacityFirstFinalLifecycleProjection.Project(CompleteCapture(
                firstGateNonzero: true,
                fillEntryEdi: 0,
                gateEbp: 3,
                fillFrameMinus4: uint.MaxValue,
                finalFrameMinus14: uint.MaxValue - 1,
                finalReceiver4Nonzero: false,
                finalEdi: 0x10,
                finalEbx: uint.MaxValue,
                finalEbp: 3,
                finalReceiver8Old: uint.MaxValue,
                sharedRepeatDirectionFlag: true));
        check("The composed frame reads and receiver return writes preserve independent DWORD-wrapped addresses",
            wrapped.Outcome
                == FrameHandoffCapacityFirstFinalLifecycleOutcome.ProjectionComplete
            && wrapped.RepeatFillJoinPlan.GateRepeatPlan.RepeatPlan
                ?.FreshFrameMinus8ReadAddress == 0xFFFFFFFB
            && wrapped.RepeatFillJoinPlan.FillJoinPlan?.FrameMinus4ReadAddress
                == uint.MaxValue
            && wrapped.FillFinalLifecyclePlan?.FillPlan.FillJoinPlan.FrameMinus4ReadAddress
                == uint.MaxValue
            && wrapped.FillFinalLifecyclePlan?.FinalGatePlan?.Reads[0].Address
                == 0xFFFFFFEF
            && wrapped.FillFinalLifecyclePlan?.FinalLifecyclePlan?.FinalPlan
                ?.MemoryAccesses[0].Address == 6
            && wrapped.FillFinalLifecyclePlan?.FinalLifecyclePlan?.FinalPlan
                ?.MemoryAccesses[2].Address == 2);
    }

    private static FrameHandoffCapacityFirstFinalLifecycleCapture CompleteCapture(
        bool firstGateNonzero,
        uint? fillEntryEdi,
        bool finalReceiver4Nonzero = false,
        uint gateEbp = 0x2000,
        uint? fillJoinEbp = null,
        uint fillFrameMinus4 = 0xA000,
        uint? firstFrameMinus8 = null,
        bool omitFirstFrameMinus8 = false,
        bool firstRepCompleted = true,
        uint? expectedSharedRepeatEntryEcx = null,
        uint? expectedSharedRepeatEntryEdi = null,
        bool? sharedRepeatDirectionFlag = false,
        bool? sharedRepeatCompleted = true,
        uint? finalFrameMinus14 = 0x5000,
        uint finalEdi = 0x5000,
        uint finalEsi = 0xCAFEBABE,
        uint finalEbx = 0xA000,
        uint? finalEbp = null,
        uint? finalReceiver8Old = 0xFFFFFFFC) =>
        new(
            RepeatFillJoin: new(
                GateRepeatCapture: new(
                    GatePlan: Gate(
                        frameMinusC: firstGateNonzero ? 3u : 0u,
                        entryEbp: gateEbp),
                    RepeatEntryEcx: firstGateNonzero ? 3u : null,
                    RepeatEntryEdx: firstGateNonzero ? 1u : null,
                    RepeatEntryEbp: firstGateNonzero ? gateEbp : null,
                    DirectionFlagSet: firstGateNonzero ? false : null,
                    RepReturnedNormally: firstGateNonzero ? firstRepCompleted : null,
                    FreshFrameMinus8Value:
                        firstGateNonzero && !omitFirstFrameMinus8
                            ? firstFrameMinus8 ?? fillEntryEdi
                            : null),
                FillJoinEntryEbp: fillJoinEbp ?? gateEbp,
                FillJoinEntryEdi: fillEntryEdi,
                FrameMinus4: fillFrameMinus4),
            Final: new(
                Gate: new(
                    CaptureValidatedJoin: false,
                    EntryEbp: fillJoinEbp ?? gateEbp,
                    FrameMinus14: finalFrameMinus14,
                    Receiver4: finalReceiver4Nonzero ? 1u : 0u,
                    NormalCallReturned: finalReceiver4Nonzero ? true : null),
                Final: new(
                    ValidatedNormalTail: false,
                    EntryEdi: finalReceiver4Nonzero
                        ? 0xDEADBEEF
                        : finalFrameMinus14 ?? finalEdi,
                    EntryEsi: finalEsi,
                    EntryEbx: finalReceiver4Nonzero ? 0x0BADF00D : finalEbx,
                    EntryEbp: finalEbp ?? fillJoinEbp ?? gateEbp,
                    Receiver8Old: finalReceiver8Old)),
            ExpectedSharedRepeatEntryEcx: expectedSharedRepeatEntryEcx
                ?? (fillEntryEdi is uint nonzeroFillEdi && nonzeroFillEdi != 0
                    ? nonzeroFillEdi
                    : null),
            ExpectedSharedRepeatEntryEdi: expectedSharedRepeatEntryEdi
                ?? (fillEntryEdi is uint nonzeroFillEdi2 && nonzeroFillEdi2 != 0
                    ? fillFrameMinus4
                    : null),
            SharedRepeatDirectionFlagSet:
                fillEntryEdi is uint sharedCount && sharedCount == 0
                    ? null
                    : sharedRepeatDirectionFlag,
            SharedRepeatCompletedNormally:
                fillEntryEdi is uint repeatCount && repeatCount == 0
                    ? null
                    : sharedRepeatCompleted);

    private static FrameHandoffCapacitySecondReturnGatePlan Gate(
        uint frameMinusC,
        uint entryEbp) =>
        FrameHandoffCapacitySecondReturnGateProjection.Project(new(
            ValidatedNormalEntry: true,
            EntryEbp: entryEbp,
            EntryEbx: 0xFFFFFFFE,
            FrameMinusC: frameMinusC,
            HelperReturnEax: 3));
}
