using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlignmentNonzeroLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan clean =
            Project(Create(
                resultEsi: 0x1000,
                loadedWord: 0xFF,
                nonzeroEntryEBX: 0,
                nonzeroGlobalDword: 0,
                nonzeroEsiPlus8Dword: 0,
                nonzeroEsiPlusCDword: 0x1000));
        check("Only the clean ECX-nonzero alignment branch derives the nonzero entry",
            clean.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiEqualsEsiPlusCDwordDestination
            && clean.AlignmentDispatchOutcomeClean
            && clean.ExactEntryEcxNonzeroBranch
            && clean.NonzeroValidationReached
            && clean.AlignmentDestinationSelected
            && clean.AlignmentDestinationRawFileOffset == 0x012C8E45
            && clean.NonzeroDestinationSelected
            && clean.NonzeroDestinationRawFileOffset == 0x012C8E76
            && clean.DerivedNonzeroEntryEAX == 0xFF
            && clean.DerivedNonzeroEntryEDI == 0x100
            && clean.DerivedNonzeroEntryESI == 0x1000
            && clean.DerivedNonzeroEntryEBP == 0x2000
            && clean.NonzeroEntryEBXCaptureProvided
            && clean.NonzeroEntryEBX == 0
            && clean.AlignmentDispatchPlan.EAX == clean.DerivedNonzeroEntryEAX
            && clean.AlignmentDispatchPlan.EDI == clean.DerivedNonzeroEntryEDI
            && clean.AlignmentDispatchPlan.ESI == clean.DerivedNonzeroEntryESI
            && clean.AlignmentDispatchPlan.Reads.Length == 1
            && clean.AlignmentDispatchPlan.Reads[0].Address == 0x10FF
            && clean.AlignmentDispatchPlan.Reads[0].Value == 0xFF
            && clean.NonzeroValidationPlan is
                CapacityFinalCalleeNonzeroValidationPlan nonzero
            && nonzero.Capture.EntryEAX == 0xFF
            && nonzero.Capture.EntryEDI == 0x100
            && nonzero.Capture.EntryESI == 0x1000
            && nonzero.Capture.EntryEBX == 0
            && nonzero.Capture.EntryEBP == 0x2000
            && nonzero.Capture.GlobalDword == 0
            && nonzero.Reads.Length == 3
            && nonzero.Reads[0].ValueCaptured
            && nonzero.Reads[0].Value == 0
            && nonzero.Reads[0].Address
                == CapacityFinalCalleeNonzeroValidationProjection
                    .GlobalDwordVirtualAddress
            && nonzero.Reads[1].ValueCaptured
            && nonzero.Reads[1].Value == 0
            && nonzero.Reads[1].Address == 0x1008
            && nonzero.Reads[2].Address == 0x100C
            && nonzero.Reads.Select(read => read.Kind).SequenceEqual(
                new[]
                {
                    CapacityFinalCalleeNonzeroValidationReadKind.GlobalDword,
                    CapacityFinalCalleeNonzeroValidationReadKind.EsiPlus8Dword,
                    CapacityFinalCalleeNonzeroValidationReadKind.EsiPlusCDword
                })
            && !clean.UnreachedAlignmentCaptureProvided
            && !clean.UnreachedNonzeroCaptureProvided
            && clean.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan missingEBX =
            Project(Create(
                resultEsi: 0x1000,
                nonzeroEntryEBX: null,
                nonzeroGlobalDword: 0,
                nonzeroEsiPlus8Dword: 0,
                nonzeroEsiPlusCDword: 0));
        check("Missing independent EBX blocks the read window but retains alignment effects",
            missingEBX.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .MissingIndependentNonzeroEntryEBX
            && missingEBX.ExactEntryEcxNonzeroBranch
            && !missingEBX.NonzeroValidationReached
            && missingEBX.NonzeroValidationPlan is null
            && missingEBX.AlignmentDispatchPlan.FrameWrites.Length == 2
            && missingEBX.DerivedNonzeroEntryEAX == 0xFF
            && missingEBX.DerivedNonzeroEntryEDI == 0x100
            && missingEBX.DerivedNonzeroEntryESI == 0x1000
            && missingEBX.DerivedNonzeroEntryEBP == 0x2000
            && !missingEBX.NonzeroEntryEBXCaptureProvided
            && missingEBX.UnreachedNonzeroGlobalDwordCaptureProvided
            && missingEBX.UnreachedNonzeroEsiPlus8DwordCaptureProvided
            && missingEBX.UnreachedNonzeroEsiPlusCDwordCaptureProvided
            && missingEBX.UnreachedNonzeroCaptureProvided
            && missingEBX.StopAtRawFileOffsetExclusive == 0x012C8E45);

        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan wrongRoute =
            Project(Create(
                resultEsi: 0x1000,
                entryEcx: 0,
                loadedWord: 0xFFFFFEF0,
                nonzeroEntryEBX: null,
                nonzeroGlobalDword: null,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: null));
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan wrongRouteCapture =
            Project(Create(
                resultEsi: 0x1000,
                entryEcx: 0,
                loadedWord: 0xFFFFFEF0,
                nonzeroEntryEBX: 0,
                nonzeroGlobalDword: null,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: null));
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan lowBitRoute =
            Project(Create(
                outerEAX: 0x101,
                captureAlignmentEsiDword: false,
                nonzeroEntryEBX: null,
                nonzeroGlobalDword: null,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: null));
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan lowBitRouteCapture =
            Project(Create(
                outerEAX: 0x101,
                captureAlignmentEsiDword: false,
                nonzeroEntryEBX: 0,
                nonzeroGlobalDword: null,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: null));
        check("Other clean alignment destinations are bounded and reject nonzero captures",
            wrongRoute.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .AlignmentComputedEcxZeroDestination
            && wrongRoute.AlignmentDispatchOutcomeClean
            && !wrongRoute.ExactEntryEcxNonzeroBranch
            && !wrongRoute.NonzeroValidationReached
            && !wrongRoute.UnreachedNonzeroCaptureProvided
            && wrongRoute.AlignmentDestinationRawFileOffset == 0x012C8E3A
            && wrongRoute.NonzeroValidationPlan is null
            && wrongRoute.StopAtRawFileOffsetExclusive == 0x012C8E3A
            && wrongRouteCapture.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .UnreachedNonzeroCaptureProvided
            && wrongRouteCapture.UnreachedNonzeroEntryEBXCaptureProvided
            && wrongRouteCapture.UnreachedNonzeroCaptureProvided
            && !wrongRouteCapture.NonzeroValidationReached
            && lowBitRoute.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .AlignmentEntryAlLowBitSetDestination
            && lowBitRoute.AlignmentDestinationRawFileOffset == 0x012C8EF8
            && !lowBitRoute.NonzeroValidationReached
            && lowBitRoute.AlignmentDispatchPlan.Reads.IsEmpty
            && lowBitRoute.StopAtRawFileOffsetExclusive == 0x012C8EF8
            && lowBitRouteCapture.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .UnreachedNonzeroCaptureProvided
            && lowBitRouteCapture.AlignmentDestinationRawFileOffset == 0x012C8EF8
            && lowBitRouteCapture.UnreachedNonzeroEntryEBXCaptureProvided);

        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan returnedAlignmentCall =
            Project(Create(
                resultEsi: 0x200,
                loadedWord: 0x100,
                entryEcx: 0,
                alignmentGlobalDword: 0,
                alignmentCallCellTarget: 0,
                alignmentCallReturnedNormally: true,
                nonzeroEntryEBX: null,
                nonzeroGlobalDword: null,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: null));
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan contaminatedAlignment =
            Project(Create(
                resultEsi: 0x1000,
                alignmentGlobalDword: 0,
                nonzeroEntryEBX: 0,
                nonzeroGlobalDword: 0x1001,
                nonzeroEsiPlus8Dword: 0,
                nonzeroEsiPlusCDword: 0));
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan returnedCallCapture =
            Project(Create(
                resultEsi: 0x200,
                loadedWord: 0x100,
                entryEcx: 0,
                alignmentGlobalDword: 0,
                alignmentCallCellTarget: 0,
                alignmentCallReturnedNormally: true,
                nonzeroEntryEBX: 0,
                nonzeroGlobalDword: null,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: null));
        check("A normal alignment call is a bounded route; an unreached alignment read contaminates ECX admission",
            returnedAlignmentCall.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .AlignmentCallReturnedNormally
            && returnedAlignmentCall.AlignmentDispatchOutcomeClean
            && returnedAlignmentCall.AlignmentDestinationSelected
            && returnedAlignmentCall.AlignmentDestinationRawFileOffset == 0x012C8DED
            && returnedAlignmentCall.AlignmentDispatchPlan.EAX == 0x1FD0
            && returnedAlignmentCall.AlignmentDispatchPlan.ESI == 0x200
            && returnedAlignmentCall.AlignmentDispatchPlan.ECX == 0x210
            && returnedAlignmentCall.AlignmentDispatchPlan.EDI == 0x210
            && returnedAlignmentCall.AlignmentDispatchPlan.ESP == 0x2FF4
            && returnedAlignmentCall.AlignmentDispatchPlan.Pushes.SequenceEqual(
                new[]
                {
                    new CapacityFinalCalleeAlignmentDispatchPush(
                        0x012C8DE0,
                        0x2FFC,
                        0x1C),
                    new CapacityFinalCalleeAlignmentDispatchPush(
                        0x012C8DE5,
                        0x2FF8,
                        0x1FD0),
                    new CapacityFinalCalleeAlignmentDispatchPush(
                        0x012C8DE6,
                        0x2FF4,
                        0x200)
                })
            && returnedAlignmentCall.AlignmentDispatchPlan.TestEntryEcxFlags is
                { Carry: false, Parity: true, AuxiliaryCarry: null, Zero: true, Sign: false, Overflow: false }
            && returnedAlignmentCall.AlignmentDispatchPlan.TestComputedEcxFlags is
                { Carry: false, Parity: false, AuxiliaryCarry: null, Zero: false, Sign: false, Overflow: false }
            && !returnedAlignmentCall.NonzeroValidationReached
            && !returnedAlignmentCall.UnreachedNonzeroCaptureProvided
            && returnedAlignmentCall.StopAtRawFileOffsetExclusive == 0x012C8DED
            && contaminatedAlignment.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .AlignmentDispatchContaminated
            && contaminatedAlignment.AlignmentDispatchPlan.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryEcxNonzero
            && contaminatedAlignment.UnreachedAlignmentGlobalDispatchDwordCaptureProvided
            && contaminatedAlignment.UnreachedAlignmentCaptureProvided
            && !contaminatedAlignment.NonzeroValidationReached
            && contaminatedAlignment.NonzeroValidationPlan is null
            && contaminatedAlignment.StopAtRawFileOffsetExclusive == 0x012C8E45
            && returnedCallCapture.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .UnreachedNonzeroCaptureProvided
            && returnedCallCapture.AlignmentDestinationRawFileOffset == 0x012C8DED
            && returnedCallCapture.UnreachedNonzeroEntryEBXCaptureProvided);

        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan unsignedBelow =
            Project(Create(
                resultEsi: 0x7FFFFFFF,
                loadedWord: 0xFF,
                nonzeroEntryEBX: 0x80000000,
                nonzeroGlobalDword: null,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: null));
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan signedAboveUnsigned =
            Project(Create(
                resultEsi: 0x80000000,
                loadedWord: 0xFF,
                nonzeroEntryEBX: 0x7FFFFFFF,
                nonzeroGlobalDword: 0x7FFFFFFF,
                nonzeroEsiPlus8Dword: 0,
                nonzeroEsiPlusCDword: 0x80000000));
        check("The independent EBX capture controls the unsigned guard, not signed ordering or alignment globals",
            unsignedBelow.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiUnsignedBelowEntryEbxDestination
            && unsignedBelow.NonzeroDestinationRawFileOffset == 0x012C8E76
            && unsignedBelow.NonzeroValidationPlan is
                CapacityFinalCalleeNonzeroValidationPlan unsignedPlan
            && unsignedPlan.CompareEsiEbxFlags is
                { Carry: true, Sign: true, Overflow: true, Zero: false }
            && !unsignedPlan.GlobalReadReached
            && !unsignedBelow.UnreachedNonzeroCaptureProvided
            && signedAboveUnsigned.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiEqualsEsiPlusCDwordDestination
            && signedAboveUnsigned.NonzeroValidationPlan is
                CapacityFinalCalleeNonzeroValidationPlan signedPlan
            && signedPlan.CompareEsiEbxFlags is
                { Carry: false, Sign: false, Overflow: true, Zero: false }
            && signedPlan.Capture.EntryEBX == 0x7FFFFFFF
            && signedPlan.Capture.GlobalDword == 0x7FFFFFFF
            && signedPlan.Capture.EntryESI == 0x80000000
            && signedAboveUnsigned.NonzeroDestinationRawFileOffset == 0x012C8E76);

        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan equalGlobal =
            Project(Create(
                resultEsi: 0x1000,
                loadedWord: 0xFF,
                nonzeroEntryEBX: 0,
                nonzeroGlobalDword: 0x1000,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: null));
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan capacityExit =
            Project(Create(
                resultEsi: 0x80000000,
                loadedWord: 0x100,
                nonzeroEntryEBX: 0x7FFFFFFF,
                nonzeroGlobalDword: 0x7FFFFFFF,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: null));
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan finalUnequal =
            Project(Create(
                resultEsi: 0x1000,
                loadedWord: 0xFF,
                nonzeroEntryEBX: 0,
                nonzeroGlobalDword: 0,
                nonzeroEsiPlus8Dword: 0xAABBCCDD,
                nonzeroEsiPlusCDword: 0x1001));
        check("All five nonzero validation destinations retain distinct successful routes",
            equalGlobal.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiEqualsGlobalDwordDestination
            && equalGlobal.NonzeroDestinationRawFileOffset == 0x012C90A2
            && equalGlobal.NonzeroValidationPlan?.CompareEsiGlobalFlags?.Zero == true
            && capacityExit.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroShiftedEaxUnsignedAboveOrEqual32Destination
            && capacityExit.NonzeroValidationPlan?.ShiftedEax == 0x20
            && capacityExit.NonzeroDestinationRawFileOffset == 0x012C8F76
            && finalUnequal.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiNotEqualEsiPlusCDwordDestination
            && finalUnequal.NonzeroValidationPlan?.CompareEsiEsiPlusCFlags?.Zero
                == false
            && finalUnequal.NonzeroDestinationRawFileOffset == 0x012C8E7C
            && signedAboveUnsigned.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiEqualsEsiPlusCDwordDestination
            && signedAboveUnsigned.NonzeroValidationPlan?.CompareEsiEsiPlusCFlags?.Zero
                == true
            && signedAboveUnsigned.NonzeroDestinationRawFileOffset == 0x012C8E76);

        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan missingEsiPlus8 =
            Project(Create(
                resultEsi: 0x1000,
                loadedWord: 0xFF,
                nonzeroEntryEBX: 0,
                nonzeroGlobalDword: 0,
                nonzeroEsiPlus8Dword: null,
                nonzeroEsiPlusCDword: 0x1000));
        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan missingEsiPlusC =
            Project(Create(
                resultEsi: 0x1000,
                loadedWord: 0xFF,
                nonzeroEntryEBX: 0,
                nonzeroGlobalDword: 0,
                nonzeroEsiPlus8Dword: 0,
                nonzeroEsiPlusCDword: null));
        check("A missing fresh read blocks only after preserving earlier nonzero stack writes",
            missingEsiPlus8.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroValidationBlocked
            && missingEsiPlus8.NonzeroValidationPlan?.Writes.Select(
                write => write.RawFileOffset).SequenceEqual(new uint[] { 0x012C8E5A })
                == true
            && missingEsiPlus8.NonzeroValidationPlan?.Reads.Length == 2
            && missingEsiPlus8.NonzeroValidationPlan?.Reads[1].ValueCaptured == false
            && missingEsiPlus8.UnreachedNonzeroEsiPlusCDwordCaptureProvided
            && missingEsiPlus8.StopAtRawFileOffsetExclusive == 0x012C8E66
            && missingEsiPlusC.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroValidationBlocked
            && missingEsiPlusC.NonzeroValidationPlan?.Writes.Select(
                write => write.RawFileOffset).SequenceEqual(
                    new uint[] { 0x012C8E5A, 0x012C8E69 }) == true
            && missingEsiPlusC.NonzeroValidationPlan?.Writes[1].Value == 0
            && missingEsiPlusC.StopAtRawFileOffsetExclusive == 0x012C8E6C);

        CapacityFinalCalleeAlignmentNonzeroLifecyclePlan wrapped =
            Project(Create(
                resultEsi: 0xFFFFFFF0,
                loadedWord: 0x20,
                outerEAX: 0x100,
                entryEBP: 0,
                nonzeroEntryEBX: 0xFFFFFFF0,
                nonzeroGlobalDword: 0xFFFFFFF1,
                nonzeroEsiPlus8Dword: 0,
                nonzeroEsiPlusCDword: 0xFFFFFFEF));
        check("The composed subtraction, fresh addresses, and frame writes preserve DWORD wrapping",
            wrapped.DerivedNonzeroEntryESI == 0xFFFFFFF0
            && wrapped.AlignmentDispatchPlan.Reads[0].Address == 0x10
            && wrapped.AlignmentDispatchPlan.ESI == 0xFFFFFFF0
            && wrapped.AlignmentDispatchPlan.FrameWrites[0].Address == 0xFFFFFFF8
            && wrapped.AlignmentDispatchPlan.FrameWrites[1].Address == 0xFFFFFFF4
            && wrapped.NonzeroValidationPlan?.EsiPlus8Address == 0xFFFFFFF8
            && wrapped.NonzeroValidationPlan?.EsiPlusCAddress == 0xFFFFFFFC
            && wrapped.NonzeroValidationPlan?.Writes.Select(write => write.Address)
                .SequenceEqual(new uint[] { 0xFFFFFFF0, 0xFFFFFFFC, 0xFFFFFFEC })
                == true
            && wrapped.Outcome
                == CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiNotEqualEsiPlusCDwordDestination
            && wrapped.NonzeroDestinationRawFileOffset == 0x012C8E7C);

        check("The alignment-entry VA mapping is raw offset plus 00400C00",
            CapacityFinalCalleeAlignmentDispatchProjection.RawFileOffsetToVirtualAddress(
                0x012C8DA7) == 0x016C99A7
            && CapacityFinalCalleeNonzeroValidationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlignmentNonzeroLifecycleProjection
                    .NonzeroEntryRawFileOffset) == 0x016C9A45);
    }

    private static CapacityFinalCalleeAlignmentNonzeroLifecyclePlan Project(
        CapacityFinalCalleeAlignmentNonzeroLifecycleCapture capture) =>
        CapacityFinalCalleeAlignmentNonzeroLifecycleProjection.Project(capture);

    private static CapacityFinalCalleeAlignmentNonzeroLifecycleCapture Create(
        uint resultEsi = 0x1000,
        uint loadedWord = 0xFF,
        uint? nonzeroEntryEBX = 0,
        uint? nonzeroGlobalDword = 0,
        uint? nonzeroEsiPlus8Dword = 0,
        uint? nonzeroEsiPlusCDword = 0x1000,
        uint outerEAX = 0x100,
        uint entryEcx = 1,
        uint entryEBP = 0x2000,
        bool captureAlignmentEsiDword = true,
        uint? alignmentGlobalDword = null,
        uint? alignmentCallCellTarget = null,
        bool? alignmentCallReturnedNormally = null)
    {
        uint alignmentEntryESI = unchecked(resultEsi + loadedWord);
        var alignmentCapture = new CapacityFinalCalleeAlignmentDispatchCapture(
            CaptureValidatedEntry: true,
            EntryEAX: outerEAX,
            EntryESI: alignmentEntryESI,
            EntryECX: entryEcx,
            EntryEBP: entryEBP,
            EntryESP: 0x3000,
            EntryEsiDword: captureAlignmentEsiDword ? loadedWord : null,
            GlobalDispatchDword: alignmentGlobalDword,
            CallCellTargetVirtualAddress: alignmentCallCellTarget,
            CallReturnedNormally: alignmentCallReturnedNormally);
        return new(
            alignmentCapture,
            nonzeroEntryEBX,
            nonzeroGlobalDword,
            nonzeroEsiPlus8Dword,
            nonzeroEsiPlusCDword);
    }
}
