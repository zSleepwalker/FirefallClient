using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSelectionDescentLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeSelectionDescentLifecyclePlan firstSelected =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0x1014,
                freshEsiPlus10: null,
                freshDescentFrameMinus4: 0xDADA,
                freshDescentEaxPlus14: 0x7777,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: 0x3333));
        check("The first selected nonzero EAX joins FC2 once with entry EBP",
            firstSelected.AlternateFieldSelectionPlan.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome
                    .FirstFreshEsiPlus14NonzeroToBoundary
            && firstSelected.ExactFC2RouteSelected
            && firstSelected.DerivedDescentEntryEAX == 0x1014
            && firstSelected.DerivedDescentEntryEBP == 0x2000
            && firstSelected.FieldDescentStepPlan is
                CapacityFinalCalleeFieldDescentStepPlan firstDescent
            && firstDescent.Capture.ValidatedEntryEAX == 0x1014
            && firstDescent.Capture.ValidatedEntryEBP == 0x2000
            && firstDescent.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback
            && firstSelected.Outcome
                == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .FieldDescentReachedLoopback
            && firstSelected.Admitted
            && firstSelected.SelectedDestinationRawFileOffset == 0x012C8FC5
            && firstSelected.StopAtRawFileOffsetExclusive == 0x012C8FC5
            && !firstDescent.AnyUnreachedCaptureProvided);

        CapacityFinalCalleeSelectionDescentLifecyclePlan secondSelected =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0,
                freshEsiPlus10: 0x2222,
                freshDescentFrameMinus4: 0xABCD,
                freshDescentEaxPlus14: 0,
                freshDescentEaxPlus10: 0,
                freshDescentEcxValue: null));
        check("The second selected nonzero EAX joins FC2 once and may terminate at FDD",
            secondSelected.AlternateFieldSelectionPlan.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome
                    .SecondFreshEsiPlus10NonzeroToBoundary
            && secondSelected.ExactFC2RouteSelected
            && secondSelected.DerivedDescentEntryEAX == 0x2222
            && secondSelected.DerivedDescentEntryEBP == 0x2000
            && secondSelected.FieldDescentStepPlan is
                CapacityFinalCalleeFieldDescentStepPlan secondDescent
            && secondDescent.Capture.ValidatedEntryEAX == 0x2222
            && secondDescent.Capture.ValidatedEntryEBP == 0x2000
            && secondDescent.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedTerminal
            && secondDescent.TerminalReached
            && !secondDescent.FreshEcxReadReached
            && secondSelected.Outcome
                == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .FieldDescentReachedTerminal
            && secondSelected.Admitted
            && secondSelected.SelectedDestinationRawFileOffset == 0x012C8FDD
            && secondSelected.StopAtRawFileOffsetExclusive == 0x012C8FDD);

        CapacityFinalCalleeSelectionDescentLifecyclePlan cleanFfe =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0,
                freshEsiPlus10: 0,
                freshDescentFrameMinus4: null,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: null));
        CapacityFinalCalleeSelectionDescentLifecyclePlan ffeWithDescentData =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0,
                freshEsiPlus10: 0,
                freshDescentFrameMinus4: 1,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: null));
        check("Clean FFE stays bounded while any supplied descent capture is unselected",
            cleanFfe.ExactFfeRouteSelected
            && cleanFfe.AlternateFieldSelectionClean
            && cleanFfe.FieldDescentStepPlan is null
            && cleanFfe.Outcome
                == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .AlternateSecondFreshEsiPlus10ZeroTarget
            && cleanFfe.Admitted
            && cleanFfe.SelectedDestinationRawFileOffset == 0x012C8FFE
            && cleanFfe.StopAtRawFileOffsetExclusive == 0x012C8FFE
            && !cleanFfe.AnyUnreachedDescentCaptureProvided
            && !ffeWithDescentData.Admitted
            && ffeWithDescentData.FieldDescentStepPlan is null
            && ffeWithDescentData.Outcome
                == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .UnreachedDescentCaptures
            && ffeWithDescentData.UnreachedDescentFrameMinus4CaptureProvided
            && ffeWithDescentData.AnyUnreachedDescentCaptureProvided
            && ffeWithDescentData.SelectedDestinationRawFileOffset == 0x012C8FFE);

        CapacityFinalCalleeSelectionDescentLifecyclePlan independentFrameRead =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0x1014,
                freshEsiPlus10: null,
                freshDescentFrameMinus4: 0xDEADBEEF,
                freshDescentEaxPlus14: 1,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: 0));
        check("The descent frame-minus-four load is independent from the selection write",
            independentFrameRead.AlternateFieldSelectionPlan.FirstWriteReached
            && independentFrameRead.AlternateFieldSelectionPlan.FirstWriteAddress == 0x1FFC
            && independentFrameRead.AlternateFieldSelectionPlan.FirstWriteValue == 0x1014
            && independentFrameRead.FieldDescentStepPlan is
                CapacityFinalCalleeFieldDescentStepPlan independentDescent
            && independentDescent.FreshFrameMinus4Address == 0x1FFC
            && independentDescent.FreshFrameMinus4Captured
            && independentDescent.EDXLoadedFromFrameMinus4 == 0xDEADBEEF
            && independentDescent.EDXLoadedFromFrameMinus4
                != independentFrameRead.AlternateFieldSelectionPlan.FirstWriteValue);

        CapacityFinalCalleeSelectionDescentLifecyclePlan missingSelectionRead =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0,
                freshEsiPlus10: null,
                freshDescentFrameMinus4: null,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: null));
        CapacityFinalCalleeSelectionDescentLifecyclePlan missingDescentRead =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0x1014,
                freshEsiPlus10: null,
                freshDescentFrameMinus4: 0xAAAA,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: 0,
                freshDescentEcxValue: 0));
        CapacityFinalCalleeSelectionDescentLifecyclePlan contaminatedSelection =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0x1014,
                freshEsiPlus10: 0xBBBB,
                freshDescentFrameMinus4: null,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: null));
        CapacityFinalCalleeSelectionDescentLifecyclePlan contaminatedDescent =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0x1014,
                freshEsiPlus10: null,
                freshDescentFrameMinus4: 0xAAAA,
                freshDescentEaxPlus14: 1,
                freshDescentEaxPlus10: 0xCCCC,
                freshDescentEcxValue: 0));
        check("A missing selection read preserves the prior selection write",
            missingSelectionRead.Outcome
                == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .AlternateFieldSelectionIncomplete
            && missingSelectionRead.FieldDescentStepPlan is null
            && missingSelectionRead.AlternateFieldSelectionPlan.FirstWriteReached
            && missingSelectionRead.AlternateFieldSelectionPlan.FirstWriteAddress == 0x1FFC
            && missingSelectionRead.AlternateFieldSelectionPlan.FirstWriteValue == 0x1014
            && missingSelectionRead.AlternateFieldSelectionPlan.StopAtRawFileOffsetExclusive
                == 0x012C8FB5);
        check("A missing descent read preserves its frame load and the selection write",
            missingDescentRead.Outcome
                == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .MissingFieldDescentCapture
            && missingDescentRead.FieldDescentStepPlan is
                CapacityFinalCalleeFieldDescentStepPlan missingDescent
            && missingDescent.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEaxPlus14
            && missingDescent.EDXLoadedFromFrameMinus4 == 0xAAAA
            && missingDescentRead.AlternateFieldSelectionPlan.FirstWriteReached
            && missingDescentRead.AlternateFieldSelectionPlan.FirstWriteValue == 0x1014
            && !missingDescentRead.Admitted);
        check("An unselected alternate read contaminates FC2 without discarding its write",
            contaminatedSelection.Outcome
                == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .ContaminatedAlternateFieldSelection
            && !contaminatedSelection.AlternateFieldSelectionClean
            && !contaminatedSelection.ExactFC2RouteSelected
            && contaminatedSelection.AlternateFieldSelectionPlan
                .UnreachedFreshEsiPlus10CaptureProvided
            && contaminatedSelection.AlternateFieldSelectionPlan.FirstWriteReached
            && contaminatedSelection.AlternateFieldSelectionPlan.FirstWriteValue == 0x1014
            && !contaminatedSelection.Admitted);
        check("An unreached descent read contaminates an otherwise selected loopback",
            contaminatedDescent.Outcome
                == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .ContaminatedFieldDescentCaptures
            && contaminatedDescent.UnreachedDescentEaxPlus10CaptureProvided
            && contaminatedDescent.AnyUnreachedDescentCaptureProvided
            && !contaminatedDescent.Admitted);

        CapacityFinalCalleeSelectionDescentLifecyclePlan wrapped =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(Capture(
                entryEsi: 0xFFFFFFEC,
                entryEbp: 0,
                freshEsiPlus14: 0xFFFFFFEC,
                freshEsiPlus10: null,
                freshDescentFrameMinus4: 0x1234,
                freshDescentEaxPlus14: 0,
                freshDescentEaxPlus10: 1,
                freshDescentEcxValue: 0));
        check("Zero captures remain present and selected DWORD addresses wrap",
            wrapped.Admitted
            && wrapped.DerivedDescentEntryEAX == 0xFFFFFFEC
            && wrapped.DerivedDescentEntryEBP == 0
            && wrapped.AlternateFieldSelectionPlan.FirstWriteAddress == 0xFFFFFFFC
            && wrapped.AlternateFieldSelectionPlan.FirstWriteValue == 0
            && wrapped.FieldDescentStepPlan is
                CapacityFinalCalleeFieldDescentStepPlan wrappedDescent
            && wrappedDescent.FreshFrameMinus4Address == 0xFFFFFFFC
            && wrappedDescent.FreshFrameMinus4Captured
            && wrappedDescent.FreshEaxPlus14Address == 0
            && wrappedDescent.EaxPlus14Captured
            && wrappedDescent.FreshEaxPlus14Value == 0
            && wrappedDescent.FreshEaxPlus10Address == 0xFFFFFFFC
            && wrappedDescent.EaxPlus10Captured
            && wrappedDescent.FreshEcxValueCaptured
            && wrappedDescent.FreshEcxValue == 0
            && wrappedDescent.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C8FC5);
    }

    private static CapacityFinalCalleeSelectionDescentLifecycleCapture Capture(
        uint? entryEsi = 0x1000,
        uint? entryEbp = 0x2000,
        uint? freshEsiPlus14 = 1,
        uint? freshEsiPlus10 = 2,
        uint? freshDescentFrameMinus4 = 0x3000,
        uint? freshDescentEaxPlus14 = 1,
        uint? freshDescentEaxPlus10 = null,
        uint? freshDescentEcxValue = 0x4000) =>
        new(
            new(
                entryEsi,
                entryEbp,
                freshEsiPlus14,
                freshEsiPlus10),
            freshDescentFrameMinus4,
            freshDescentEaxPlus14,
            freshDescentEaxPlus10,
            freshDescentEcxValue);
}
