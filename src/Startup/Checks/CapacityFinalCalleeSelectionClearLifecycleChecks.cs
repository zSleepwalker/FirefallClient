using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSelectionClearLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The joined projection uses the mapped selection, descent, and clear endpoints",
            CapacityFinalCalleeSelectionClearLifecycleProjection.SelectionEntryRawFileOffset
                == 0x012C8FA8
            && CapacityFinalCalleeSelectionClearLifecycleProjection.DescentEntryRawFileOffset
                == 0x012C8FC2
            && CapacityFinalCalleeSelectionClearLifecycleProjection.LoopbackRawFileOffset
                == 0x012C8FC5
            && CapacityFinalCalleeSelectionClearLifecycleProjection.TerminalRawFileOffset
                == 0x012C8FDD
            && CapacityFinalCalleeSelectionClearLifecycleProjection.FfeTargetRawFileOffset
                == 0x012C8FFE
            && CapacityFinalCalleeSelectionClearLifecycleProjection.BelowDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeSelectionClearLifecycleProjection
                .ClearTailCompletionRawFileOffsetExclusive == 0x012C8FFE);

        CapacityFinalCalleeSelectionClearLifecyclePlan firstFc2 =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0x1014,
                freshEsiPlus10: null,
                freshDescentFrameMinus4: 0x7FFFFFFF,
                freshDescentEaxPlus14: 0,
                freshDescentEaxPlus10: 0,
                freshGlobalForEntryCompare: 0x80000000,
                freshFrameMinus12: 0xFFFFFFFF,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        check("The first FC2 selection joins a clean terminal and derives independent EDX and entry EBP",
            firstFc2.SelectionDescentPlan.ExactFC2RouteSelected
            && firstFc2.ExactTerminalSelectionDescent
            && firstFc2.SelectionDescentPlan.DerivedDescentEntryEAX == 0x1014
            && firstFc2.SelectionDescentPlan.DerivedDescentEntryEBP == 0x2000
            && firstFc2.DescentClearPlan is
                CapacityFinalCalleeDescentClearLifecyclePlan firstClear
            && firstClear.DescentPlan.EDXLoadedFromFrameMinus4 == 0x7FFFFFFF
            && firstClear.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan firstTail
            && firstTail.Capture.ValidatedEntryEDX == 0x7FFFFFFF
            && firstTail.Capture.ValidatedEntryEBP == 0x2000
            && firstTail.SelectedBranchDestinationRawFileOffset == 0x012C8E76
            && firstTail.Writes[0].Address == 0x1FFC
            && firstTail.Writes[0].Value == 0x7FFFFFFF
            && firstFc2.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome.ClearBelowDestination
            && firstFc2.Admitted
            && firstFc2.SelectedDestinationRawFileOffset == 0x012C8E76
            && firstFc2.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeSelectionClearLifecyclePlan secondFc2 =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0,
                freshEsiPlus10: 0x2222,
                freshDescentFrameMinus4: 0xABCD,
                freshDescentEaxPlus14: 0,
                freshDescentEaxPlus10: 0,
                freshGlobalForEntryCompare: 0xABCD,
                freshFrameMinus12: 0x11111111,
                freshFrameMinus4ForZeroStoreAddress: 0x33333333,
                freshGlobalForTailLoad: 0));
        check("The second FC2 selection reaches FFE with clean clear captures and preserved aliases",
            secondFc2.SelectionDescentPlan.ExactFC2RouteSelected
            && secondFc2.SelectionDescentPlan.DerivedDescentEntryEAX == 0x2222
            && secondFc2.ExactTerminalSelectionDescent
            && secondFc2.DescentClearPlan?.DescentPlan.EDXLoadedFromFrameMinus4 == 0xABCD
            && secondFc2.DescentClearPlan?.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan secondTail
            && secondTail.Capture.ValidatedEntryEDX == 0xABCD
            && secondTail.Capture.ValidatedEntryEBP == 0x2000
            && secondTail.BranchTaken == false
            && secondTail.FrameMinus12Address == 0x1FF4
            && secondTail.FrameMinus4Address == 0x1FFC
            && secondTail.FrameMinus4ForZeroStoreAddress == 0x33333333
            && secondTail.TailGlobalCaptured
            && secondTail.TailGlobalValue == 0
            && secondTail.Reads[0].Address == secondTail.Reads[^1].Address
            && secondTail.Reads[0].Value == 0xABCD
            && secondTail.Reads[^1].Value == 0
            && secondTail.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x1FFC, 0xABCD),
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FF2, 0x33333333, 0)
            })
            && secondFc2.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome.ClearTailCompleted
            && secondFc2.Admitted
            && secondFc2.SelectedDestinationRawFileOffset == 0x012C8FFE
            && secondFc2.StopAtRawFileOffsetExclusive == 0x012C8FFE);

        CapacityFinalCalleeSelectionClearLifecyclePlan ffeBypass =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0,
                freshEsiPlus10: 0,
                freshDescentFrameMinus4: null,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: null,
                freshGlobalForEntryCompare: null,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        CapacityFinalCalleeSelectionClearLifecyclePlan ffeWithTailCapture =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0,
                freshEsiPlus10: 0,
                freshDescentFrameMinus4: null,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: null,
                freshGlobalForEntryCompare: 0,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        check("FFE stays a bounded bypass and rejects any downstream capture, including zero",
            ffeBypass.SelectionDescentPlan.ExactFfeRouteSelected
            && ffeBypass.DescentClearPlan is null
            && ffeBypass.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome.SelectionFfeBypass
            && ffeBypass.Admitted
            && ffeBypass.SelectedDestinationRawFileOffset == 0x012C8FFE
            && ffeBypass.StopAtRawFileOffsetExclusive == 0x012C8FFE
            && ffeWithTailCapture.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome
                    .SelectionFfeBypassContaminated
            && ffeWithTailCapture.ClearTailCaptureSupplied
            && ffeWithTailCapture.UnreachedClearTailCaptureProvided
            && !ffeWithTailCapture.ClearTailReached
            && ffeWithTailCapture.DescentClearPlan is null
            && !ffeWithTailCapture.Admitted);

        CapacityFinalCalleeSelectionClearLifecyclePlan fc5Loopback =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshDescentEaxPlus14: 1,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: 0,
                freshGlobalForEntryCompare: null,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        CapacityFinalCalleeSelectionClearLifecyclePlan fc5WithTailCapture =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshDescentEaxPlus14: 1,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: 0,
                freshGlobalForEntryCompare: 0,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        check("FC5 is a single bounded loopback and downstream captures do not start clear projection",
            fc5Loopback.SelectionDescentPlan.FieldDescentStepPlan is
                CapacityFinalCalleeFieldDescentStepPlan loopback
            && loopback.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback
            && loopback.EDXLoadedFromFrameMinus4 == 0xDADA
            && loopback.EAXLoadedFromEcx == 0
            && fc5Loopback.DescentClearPlan is null
            && fc5Loopback.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome.SelectionFc5Loopback
            && fc5Loopback.Admitted
            && fc5Loopback.SelectedDestinationRawFileOffset == 0x012C8FC5
            && fc5Loopback.StopAtRawFileOffsetExclusive == 0x012C8FC5
            && fc5WithTailCapture.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome
                    .SelectionFc5LoopbackContaminated
            && fc5WithTailCapture.UnreachedClearTailCaptureProvided
            && fc5WithTailCapture.DescentClearPlan is null
            && !fc5WithTailCapture.Admitted);

        CapacityFinalCalleeSelectionClearLifecyclePlan contaminatedSelection =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0x1014,
                freshEsiPlus10: 0x2222,
                freshDescentFrameMinus4: null,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: null,
                freshGlobalForEntryCompare: 0,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        CapacityFinalCalleeSelectionClearLifecyclePlan missingDescent =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshDescentFrameMinus4: 0xAAAA,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: null,
                freshGlobalForEntryCompare: 0,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        CapacityFinalCalleeSelectionClearLifecyclePlan contaminatedTerminal =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshDescentEcxValue: 0,
                freshGlobalForEntryCompare: 0,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        check("Missing or contaminated earlier gates preserve effects and never start the clear tail",
            contaminatedSelection.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome
                    .SelectionDescentBlocked
            && contaminatedSelection.SelectionDescentPlan
                .AlternateFieldSelectionPlan.FirstWriteReached
            && contaminatedSelection.SelectionDescentPlan
                .AlternateFieldSelectionPlan.FirstWriteValue == 0x1014
            && contaminatedSelection.DescentClearPlan is null
            && contaminatedSelection.UnreachedClearTailCaptureProvided
            && !contaminatedSelection.Admitted
            && missingDescent.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome.TerminalDescentBlocked
            && missingDescent.SelectionDescentPlan.FieldDescentStepPlan is
                CapacityFinalCalleeFieldDescentStepPlan missingStep
            && missingStep.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEaxPlus14
            && missingStep.EDXLoadedFromFrameMinus4 == 0xAAAA
            && missingDescent.DescentClearPlan is null
            && missingDescent.UnreachedClearTailCaptureProvided
            && !missingDescent.Admitted
            && contaminatedTerminal.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome
                    .TerminalDescentContaminated
            && contaminatedTerminal.SelectionDescentPlan
                .FieldDescentStepPlan?.TerminalReached == true
            && contaminatedTerminal.DescentClearPlan is null
            && contaminatedTerminal.UnreachedClearTailCaptureProvided
            && contaminatedTerminal.SelectedDestinationRawFileOffset == 0x012C8FDD
            && !contaminatedTerminal.Admitted);

        CapacityFinalCalleeSelectionClearLifecyclePlan missingSelection =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshEsiPlus14: 0,
                freshEsiPlus10: null,
                freshDescentFrameMinus4: null,
                freshDescentEaxPlus14: null,
                freshDescentEaxPlus10: null,
                freshDescentEcxValue: null,
                freshGlobalForEntryCompare: null,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        CapacityFinalCalleeSelectionClearLifecyclePlan missingClearGate =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshGlobalForEntryCompare: null,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: 0,
                freshGlobalForTailLoad: 0));
        check("Missing reads remain distinct from captured zero at every reached gate",
            missingSelection.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome
                    .SelectionDescentBlocked
            && missingSelection.SelectionDescentPlan
                .AlternateFieldSelectionPlan.FirstWriteReached
            && missingSelection.SelectionDescentPlan
                .AlternateFieldSelectionPlan.FirstWriteValue == 0x1014
            && missingSelection.DescentClearPlan is null
            && missingClearGate.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome.ClearTailContaminated
            && missingClearGate.DescentClearPlan?.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan missingTail
            && missingTail.CompareGlobalReadReached
            && !missingTail.CompareGlobalCaptured
            && missingTail.Writes.IsEmpty
            && !missingTail.UnreachedFreshFrameMinus12CaptureProvided
            && missingTail.UnreachedFreshFrameMinus4CaptureProvided
            && missingTail.UnreachedFreshGlobalForTailLoadCaptureProvided
            && missingClearGate.UnreachedClearTailCaptureProvided
            && !missingClearGate.Admitted);

        CapacityFinalCalleeSelectionClearLifecyclePlan contaminatedBelow =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                freshGlobalForEntryCompare: 0xDADB,
                freshFrameMinus12: 0xFFFFFFFF,
                freshFrameMinus4ForZeroStoreAddress: 0,
                freshGlobalForTailLoad: 0));
        check("JB to E76 preserves the reached store but rejects all captures beyond the branch",
            contaminatedBelow.Outcome
                == CapacityFinalCalleeSelectionClearLifecycleOutcome.ClearTailContaminated
            && contaminatedBelow.DescentClearPlan?.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan below
            && below.SelectedBranchDestinationRawFileOffset == 0x012C8E76
            && below.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x1FFC, 0xDADA)
            })
            && below.UnreachedFreshFrameMinus4CaptureProvided
            && below.UnreachedFreshGlobalForTailLoadCaptureProvided
            && contaminatedBelow.SelectedDestinationRawFileOffset == 0x012C8E76
            && contaminatedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && !contaminatedBelow.Admitted);

        CapacityFinalCalleeSelectionClearLifecyclePlan wrappedAliases =
            CapacityFinalCalleeSelectionClearLifecycleProjection.Project(Capture(
                entryEsi: 0xFFFFFFF4,
                entryEbp: 0,
                freshEsiPlus14: 0xFFFFFFEC,
                freshEsiPlus10: null,
                freshDescentFrameMinus4: 0,
                freshDescentEaxPlus14: 0,
                freshDescentEaxPlus10: 0,
                freshDescentEcxValue: null,
                freshGlobalForEntryCompare: 0,
                freshFrameMinus12: 0,
                freshFrameMinus4ForZeroStoreAddress: 0,
                freshGlobalForTailLoad: 0));
        check("Wrapped DWORD aliases and captured zero survive the full terminal composition",
            wrappedAliases.Admitted
            && wrappedAliases.DescentClearPlan?.DescentPlan.FreshFrameMinus4Address
                == 0xFFFFFFFC
            && wrappedAliases.DescentClearPlan?.DescentPlan.EDXLoadedFromFrameMinus4 == 0
            && wrappedAliases.DescentClearPlan?.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan wrappedTail
            && wrappedTail.Capture.ValidatedEntryEDX == 0
            && wrappedTail.Capture.ValidatedEntryEBP == 0
            && wrappedTail.FrameMinus12Address == 0xFFFFFFF4
            && wrappedTail.FrameMinus4Address == 0xFFFFFFFC
            && wrappedTail.FrameMinus4ForZeroStoreAddress == 0
            && wrappedTail.FrameMinus4Captured
            && wrappedTail.TailGlobalCaptured
            && wrappedTail.TailGlobalValue == 0
            && wrappedAliases.StopAtRawFileOffsetExclusive == 0x012C8FFE);
    }

    private static CapacityFinalCalleeSelectionClearLifecycleCapture Capture(
        uint? entryEsi = 0x1000,
        uint? entryEbp = 0x2000,
        uint? freshEsiPlus14 = 0x1014,
        uint? freshEsiPlus10 = null,
        uint? freshDescentFrameMinus4 = 0xDADA,
        uint? freshDescentEaxPlus14 = 0,
        uint? freshDescentEaxPlus10 = 0,
        uint? freshDescentEcxValue = null,
        uint? freshGlobalForEntryCompare = 0,
        uint? freshFrameMinus12 = 0x11111111,
        uint? freshFrameMinus4ForZeroStoreAddress = 0,
        uint? freshGlobalForTailLoad = 0) =>
        new(
            new(
                new(entryEsi, entryEbp, freshEsiPlus14, freshEsiPlus10),
                freshDescentFrameMinus4,
                freshDescentEaxPlus14,
                freshDescentEaxPlus10,
                freshDescentEcxValue),
            freshGlobalForEntryCompare,
            freshFrameMinus12,
            freshFrameMinus4ForZeroStoreAddress,
            freshGlobalForTailLoad);
}
