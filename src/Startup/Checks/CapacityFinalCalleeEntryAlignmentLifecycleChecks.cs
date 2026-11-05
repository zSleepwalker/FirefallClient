using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeEntryAlignmentLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeEntryAlignmentLifecyclePlan zero =
            CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
                entryEcx: 0,
                testedByte: null,
                zeroPathPoppedEbp: 0xDEADC0DE,
                prefixValidatedNonzeroEntry: false,
                globalDword: null,
                validationEsi4Dword: null));
        CapacityFinalCalleeEntryAlignmentLifecyclePlan zeroWithAlignmentOnlyCapture =
            CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
                entryEcx: 0,
                testedByte: null,
                zeroPathPoppedEbp: 0xDEADC0DE,
                prefixValidatedNonzeroEntry: false,
                globalDword: null,
                validationEsi4Dword: null,
                alignmentEntryEBP: 0x1111));
        check("The clean zero-entry RET is preserved and any alignment-only capture is rejected",
            zero.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.ZeroEntryReturned
            && zero.ZeroEntryReturned
            && zero.EntryValidationOutcomeClean
            && !zero.AlignmentDispatchReached
            && zero.AlignmentDispatchPlan is null
            && zero.EntryValidationPlan.PrefixLifecyclePlan.EntryPlan.ReturnReached
            && zero.EntryValidationPlan.PrefixLifecyclePlan.EntryPlan.ZeroPathPoppedEbp
                == 0xDEADC0DE
            && zero.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeEntryAlignmentLifecycleProjection
                    .ZeroReturnBoundaryRawFileOffsetExclusive
            && zeroWithAlignmentOnlyCapture.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                    .UnreachedAlignmentCaptureProvided
            && zeroWithAlignmentOnlyCapture.ZeroEntryReturned
            && zeroWithAlignmentOnlyCapture.UnreachedAlignmentCaptureProvided
            && zeroWithAlignmentOnlyCapture.AlignmentEntryEBPCaptureProvided
            && !zeroWithAlignmentOnlyCapture.AlignmentDispatchReached
            && zeroWithAlignmentOnlyCapture.AlignmentDispatchPlan is null);

        CapacityFinalCalleeEntryAlignmentLifecyclePlan unsignedBelow =
            CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
                testedByte: 0x02,
                prefixCallTarget: 0,
                prefixCallReturnedNormally: true,
                validationEntryESI: 0,
                globalDword: 1,
                validationEsi4Dword: null));
        CapacityFinalCalleeEntryAlignmentLifecyclePlan tagOne =
            CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
                validationEsi4Dword: 0x12345679));
        CapacityFinalCalleeEntryAlignmentLifecyclePlan terminalWithAlignmentCapture =
            CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
                validationEsi4Dword: 0x12345679,
                alignmentGlobalDispatchDword: 0));
        check("Clean unsigned-below and tag-one destinations remain bounded validation terminals",
            unsignedBelow.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                    .ValidationTerminalDestination
            && unsignedBelow.ValidationTerminalDestinationSelected
            && unsignedBelow.ValidationTerminalDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection
                    .EsiUnsignedBelowDestinationRawFileOffset
            && unsignedBelow.EntryValidationPlan.ValidationLifecyclePlan?.ValidationPlan
                .Outcome == CapacityFinalCalleeFirstValidationOutcome
                    .EntryEsiUnsignedBelowGlobal
            && !unsignedBelow.AlignmentDispatchReached
            && unsignedBelow.AlignmentDispatchPlan is null
            && tagOne.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                    .ValidationTerminalDestination
            && tagOne.ValidationTerminalDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection
                    .MaskEqualsOneDestinationRawFileOffset
            && !tagOne.AlignmentDispatchReached
            && terminalWithAlignmentCapture.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                    .UnreachedAlignmentCaptureProvided
            && terminalWithAlignmentCapture.UnreachedAlignmentCaptureProvided
            && !terminalWithAlignmentCapture.AlignmentDispatchReached
            && terminalWithAlignmentCapture.AlignmentDispatchPlan is null);

        CapacityFinalCalleeEntryAlignmentLifecyclePlan tagZero =
            ProjectFallthrough(0x12345678);
        check("A clean tag-zero fallthrough derives alignment registers and reaches its call boundary",
            tagZero.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.ProjectionComplete
            && tagZero.EntryValidationOutcomeClean
            && tagZero.AlignmentDispatchReached
            && tagZero.AlignmentDispatchPlan?.Capture.CaptureValidatedEntry == true
            && tagZero.AlignmentDispatchPlan?.Capture.EntryEAX
                == tagZero.EntryValidationPlan.ValidationLifecyclePlan?.ValidationPlan.Esi4Dword
            && tagZero.AlignmentDispatchPlan?.Capture.EntryECX
                == tagZero.EntryValidationPlan.ValidationLifecyclePlan?.ValidationPlan.EcxAfterMask
            && tagZero.AlignmentDispatchPlan?.Capture.EntryESI
                == tagZero.EntryValidationPlan.ValidationLifecyclePlan?.ValidationPlan
                    .Capture.EntryESI
            && tagZero.AlignmentDispatchPlan?.Capture.EntryEBP == 0xA1B2C3D4
            && tagZero.AlignmentDispatchPlan?.Capture.EntryESP == 0x50607080
            && tagZero.AlignmentDispatchPlan?.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.CallReturnedNormally
            && !tagZero.UnreachedAlignmentCaptureProvided);

        CapacityFinalCalleeEntryAlignmentLifecyclePlan postCallEntryEcxBranch =
            CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
                testedByte: 0x02,
                prefixCallTarget: 0,
                prefixCallReturnedNormally: true,
                validationEntryESI: 0x80000000,
                validationEsi4Dword: 0x1234567A,
                alignmentEntryEBP: 0xABCDEF01,
                alignmentEntryESP: 0x10203040,
                alignmentEntryEsiDword: 0x20));
        check("A returned opaque prefix call supplies validation ESI independently on the tag-two branch",
            postCallEntryEcxBranch.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.AlignmentBranchDestination
            && postCallEntryEcxBranch.EntryValidationPlan.ValidationLifecyclePlan
                ?.DeriveValidationEntryESIFromPrefixOutgoing == false
            && postCallEntryEcxBranch.DerivedAlignmentEntryESI == 0x80000000
            && postCallEntryEcxBranch.DerivedAlignmentEntryEAX == 0x1234567A
            && postCallEntryEcxBranch.DerivedAlignmentEntryECX == 2
            && postCallEntryEcxBranch.AlignmentBranchDestinationSelected
            && postCallEntryEcxBranch.AlignmentBranchDestinationRawFileOffset
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryEcxBranchDestinationRawFileOffset
            && postCallEntryEcxBranch.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryEcxBranchDestinationRawFileOffset
            && !postCallEntryEcxBranch.UnreachedAlignmentCaptureProvided
            && postCallEntryEcxBranch.AlignmentDispatchPlan?.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryEcxNonzero
            && postCallEntryEcxBranch.AlignmentDispatchPlan?.Capture.EntryEBP == 0xABCDEF01
            && postCallEntryEcxBranch.AlignmentDispatchPlan?.Capture.EntryESP == 0x10203040);

        CapacityFinalCalleeEntryAlignmentLifecyclePlan tagThreeAlignmentBranch =
            ProjectAlignmentBranch(
                validationEsi4Dword: 0x1234567B,
                alignmentEntryEsiDword: null);
        CapacityFinalCalleeEntryAlignmentLifecyclePlan computedZeroAlignmentBranch =
            ProjectAlignmentBranch(
                validationEsi4Dword: 0,
                alignmentEntryEsiDword: 0xFFFFFFF0);
        CapacityFinalCalleeEntryAlignmentLifecyclePlan contaminatedAlignmentBranch =
            ProjectAlignmentBranch(
                validationEsi4Dword: 0x1234567B,
                alignmentEntryEsiDword: 0);
        check("Clean AL-bit and computed-zero branches are successful bounded destinations; contaminated captures stay flagged",
            tagThreeAlignmentBranch.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.AlignmentBranchDestination
            && tagThreeAlignmentBranch.AlignmentBranchDestinationSelected
            && tagThreeAlignmentBranch.AlignmentBranchDestinationRawFileOffset
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryAlBranchDestinationRawFileOffset
            && tagThreeAlignmentBranch.AlignmentDispatchPlan?.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
            && !tagThreeAlignmentBranch.UnreachedAlignmentCaptureProvided
            && computedZeroAlignmentBranch.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.AlignmentBranchDestination
            && computedZeroAlignmentBranch.AlignmentBranchDestinationSelected
            && computedZeroAlignmentBranch.AlignmentBranchDestinationRawFileOffset
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .ComputedEcxZeroBranchDestinationRawFileOffset
            && computedZeroAlignmentBranch.AlignmentDispatchPlan?.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero
            && !computedZeroAlignmentBranch.UnreachedAlignmentCaptureProvided
            && contaminatedAlignmentBranch.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.AlignmentDispatchBlocked
            && contaminatedAlignmentBranch.AlignmentDispatchPlan?.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
            && contaminatedAlignmentBranch.AlignmentDispatchPlan
                ?.UnreachedEntryEsiDwordCaptureProvided == true
            && contaminatedAlignmentBranch.UnreachedAlignmentEntryEsiDwordCaptureProvided
            && contaminatedAlignmentBranch.UnreachedAlignmentCaptureProvided
            && !contaminatedAlignmentBranch.AlignmentBranchDestinationSelected);

        CapacityFinalCalleeEntryAlignmentLifecyclePlan capturedZeroSource =
            ProjectFallthrough(
                validationEsi4Dword: 0,
                alignmentEntryEsiDword: 0);
        check("Captured zero stays the original EAX source while ECX is independently masked",
            capturedZeroSource.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.ProjectionComplete
            && capturedZeroSource.DerivedAlignmentEntryEAX == 0
            && capturedZeroSource.DerivedAlignmentEntryECX == 0
            && capturedZeroSource.AlignmentDispatchPlan?.Capture.EntryEAX == 0
            && capturedZeroSource.AlignmentDispatchPlan?.Capture.EntryECX == 0
            && capturedZeroSource.AlignmentDispatchPlan?.EntryEsiDwordCaptured == true
            && capturedZeroSource.AlignmentDispatchPlan?.EntryEsiDword == 0);

        CapacityFinalCalleeEntryAlignmentLifecyclePlan fullSourceVersusMask =
            ProjectFallthrough(validationEsi4Dword: 0xABCDEF02);
        check("Alignment EAX retains the original ESI+4 dword while ECX receives only its low two bits",
            fullSourceVersusMask.DerivedAlignmentEntryEAX == 0xABCDEF02
            && fullSourceVersusMask.DerivedAlignmentEntryECX == 2
            && fullSourceVersusMask.AlignmentDispatchPlan?.Capture.EntryEAX == 0xABCDEF02
            && fullSourceVersusMask.AlignmentDispatchPlan?.Capture.EntryECX == 2);

        CapacityFinalCalleeEntryAlignmentLifecyclePlan missingUpstream =
            CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
                entryValidated: false,
                alignmentEntryEBP: 0x1111,
                alignmentEntryESP: 0x2222,
                alignmentEntryEsiDword: 0,
                alignmentGlobalDispatchDword: 0,
                alignmentCallCellTarget: 0,
                alignmentCallReturnedNormally: true));
        CapacityFinalCalleeEntryAlignmentLifecyclePlan contaminatedValidation =
            CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
                entryEcx: 4,
                expectedValidationEntryESI: 0xFFFFFFFD,
                validationEsi4Dword: 0x12345678,
                alignmentEntryEsiDword: 0,
                alignmentGlobalDispatchDword: 0));
        check("Missing upstream admission and contaminated validation captures cannot enter alignment",
            missingUpstream.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.EntryValidationBlocked
            && !missingUpstream.EntryValidationOutcomeClean
            && missingUpstream.UnreachedAlignmentCaptureProvided
            && missingUpstream.UnreachedAlignmentEntryEsiDwordCaptureProvided
            && missingUpstream.UnreachedAlignmentGlobalDispatchDwordCaptureProvided
            && missingUpstream.UnreachedAlignmentCallCellTargetCaptureProvided
            && missingUpstream.UnreachedAlignmentCallReturnStatusProvided
            && !missingUpstream.AlignmentDispatchReached
            && missingUpstream.AlignmentDispatchPlan is null
            && contaminatedValidation.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.EntryValidationBlocked
            && !contaminatedValidation.EntryValidationOutcomeClean
            && contaminatedValidation.UnreachedAlignmentCaptureProvided
            && !contaminatedValidation.AlignmentDispatchReached);

        CapacityFinalCalleeEntryAlignmentLifecyclePlan missingAlignmentRead =
            ProjectFallthrough(
                alignmentEntryEsiDword: null,
                alignmentGlobalDispatchDword: 0,
                alignmentCallCellTarget: 0,
                alignmentCallReturnedNormally: true);
        CapacityFinalCalleeEntryAlignmentLifecyclePlan nonreturningAlignmentCall =
            ProjectFallthrough(alignmentCallReturnedNormally: false);
        check("Alignment read and opaque-call gates retain their own bounded stop states",
            missingAlignmentRead.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.AlignmentDispatchBlocked
            && missingAlignmentRead.AlignmentDispatchPlan?.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.MissingEntryEsiDword
            && missingAlignmentRead.AlignmentDispatchPlan?.EntryEsiReadReached == true
            && missingAlignmentRead.AlignmentDispatchPlan?.UnreachedGlobalDispatchDwordCaptureProvided
                == true
            && missingAlignmentRead.AlignmentDispatchPlan?.UnreachedCallCellTargetCaptureProvided
                == true
            && missingAlignmentRead.AlignmentDispatchPlan?.UnreachedCallReturnStatusProvided
                == true
            && missingAlignmentRead.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryEsiReadRawFileOffset
            && nonreturningAlignmentCall.Outcome
                == CapacityFinalCalleeEntryAlignmentLifecycleOutcome.AlignmentDispatchBlocked
            && nonreturningAlignmentCall.AlignmentDispatchPlan?.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.CallDidNotReturnNormally
            && nonreturningAlignmentCall.AlignmentDispatchPlan?.CallReached == true
            && nonreturningAlignmentCall.AlignmentDispatchPlan?.CallReturnedNormally == false
            && nonreturningAlignmentCall.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlignmentDispatchProjection.CallRawFileOffset);

        CapacityFinalCalleeEntryAlignmentLifecyclePlan wrapped =
            ProjectFallthrough(
                alignmentEntryEBP: 4,
                alignmentEntryESP: 1);
        check("Alignment EBP/ESP stay independently captured and DWORD frame/stack arithmetic wraps",
            wrapped.AlignmentDispatchPlan?.Capture.EntryEBP == 4
            && wrapped.AlignmentDispatchPlan?.Capture.EntryESP == 1
            && wrapped.AlignmentDispatchPlan?.FrameWrites[0].Address == 0xFFFFFFFC
            && wrapped.AlignmentDispatchPlan?.Pushes.Select(push => push.StackAddress)
                .SequenceEqual(new[] { 0xFFFFFFFDu, 0xFFFFFFF9u, 0xFFFFFFF5u }) == true
            && wrapped.AlignmentDispatchPlan?.Capture.EntryEBP
                != wrapped.EntryValidationPlan.PrefixLifecyclePlan.EntryPlan.Capture.EntryEbp
            && wrapped.AlignmentDispatchPlan?.Capture.EntryESP
                != wrapped.EntryValidationPlan.PrefixLifecyclePlan.EntryPlan.Capture.EntryEsp);
    }

    private static CapacityFinalCalleeEntryAlignmentLifecyclePlan ProjectFallthrough(
        uint validationEsi4Dword = 0x12345678,
        uint? alignmentEntryEsiDword = 0x20,
        uint? alignmentGlobalDispatchDword = 0,
        uint? alignmentCallCellTarget = 0x12345678,
        bool? alignmentCallReturnedNormally = true,
        uint alignmentEntryEBP = 0xA1B2C3D4,
        uint alignmentEntryESP = 0x50607080) =>
        CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
            validationEsi4Dword: validationEsi4Dword,
            alignmentEntryEBP: alignmentEntryEBP,
            alignmentEntryESP: alignmentEntryESP,
            alignmentEntryEsiDword: alignmentEntryEsiDword,
            alignmentGlobalDispatchDword: alignmentGlobalDispatchDword,
            alignmentCallCellTarget: alignmentCallCellTarget,
            alignmentCallReturnedNormally: alignmentCallReturnedNormally));

    private static CapacityFinalCalleeEntryAlignmentLifecyclePlan ProjectAlignmentBranch(
        uint validationEsi4Dword,
        uint? alignmentEntryEsiDword) =>
        CapacityFinalCalleeEntryAlignmentLifecycleProjection.Project(Capture(
            validationEsi4Dword: validationEsi4Dword,
            alignmentEntryEBP: 0xA1B2C3D4,
            alignmentEntryESP: 0x50607080,
            alignmentEntryEsiDword: alignmentEntryEsiDword));

    private static CapacityFinalCalleeEntryAlignmentLifecycleCapture Capture(
        bool entryValidated = true,
        uint entryEsp = 0x1000,
        uint entryEbp = 0x2000,
        uint entryEcx = 0x12345678,
        uint entryEbx = 0x11111111,
        uint entryEsi = 0x22222222,
        uint entryEdi = 0x33333333,
        byte? testedByte = 0,
        uint? zeroPathPoppedEbp = null,
        bool prefixValidatedNonzeroEntry = true,
        uint? prefixEntryEcx = null,
        bool? priorTestMaskSet = null,
        uint? prefixCallTarget = null,
        bool? prefixCallReturnedNormally = null,
        bool validationCaptureValidatedEntry = false,
        uint validationEntryESI = 0xDEADBEEF,
        uint? globalDword = 0,
        uint? validationEsi4Dword = 0x12345678,
        uint? expectedValidationEntryESI = null,
        uint? alignmentEntryEBP = null,
        uint? alignmentEntryESP = null,
        uint? alignmentEntryEsiDword = null,
        uint? alignmentGlobalDispatchDword = null,
        uint? alignmentCallCellTarget = null,
        bool? alignmentCallReturnedNormally = null) =>
        new(
            EntryValidation: new(
                InputPrefixLifecycleCapture: new(
                    Entry: new(
                        entryValidated,
                        entryEsp,
                        entryEbp,
                        entryEcx,
                        entryEbx,
                        entryEsi,
                        entryEdi,
                        testedByte,
                        zeroPathPoppedEbp),
                    PrefixCall: new(
                        prefixValidatedNonzeroEntry,
                        prefixEntryEcx ?? entryEcx,
                        priorTestMaskSet,
                        prefixCallTarget,
                        prefixCallReturnedNormally)),
                Validation: new(
                    validationCaptureValidatedEntry,
                    validationEntryESI,
                    globalDword,
                    validationEsi4Dword),
                ExpectedValidationEntryESI: expectedValidationEntryESI),
            AlignmentEntryEBP: alignmentEntryEBP,
            AlignmentEntryESP: alignmentEntryESP,
            AlignmentEntryEsiDword: alignmentEntryEsiDword,
            AlignmentGlobalDispatchDword: alignmentGlobalDispatchDword,
            AlignmentCallCellTargetVirtualAddress: alignmentCallCellTarget,
            AlignmentCallReturnedNormally: alignmentCallReturnedNormally);
}
