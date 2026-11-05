using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleePairedTagLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The paired-write commit and tag entry meet at the exact EF8 boundary",
            CapacityFinalCalleePairedFieldWritesProjection.BoundaryRawFileOffsetExclusive
                == CapacityFinalCalleePairedTagLifecycleProjection.TagEntryRawFileOffset
            && CapacityFinalCalleeTagValidationProjection.EntryRawFileOffset
                == CapacityFinalCalleePairedTagLifecycleProjection.TagEntryRawFileOffset
            && CapacityFinalCalleePairedTagLifecycleProjection.TagEntryRawFileOffset
                == 0x012C8EF8
            && CapacityFinalCalleePairedTagLifecycleProjection.TagBoundaryRawFileOffsetExclusive
                == 0x012C8F21);

        CapacityFinalCalleePairedTagLifecyclePlan missingEntry =
            CapacityFinalCalleePairedTagLifecycleProjection.Project(Capture(
                validatedEntryEBP: null,
                tagEntryESI: 0,
                tagEntryEDX: 1,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 1));
        check("A missing paired-window entry blocks continuation and marks all later captures unreached",
            missingEntry.PairedFieldWrites.Outcome
                == CapacityFinalCalleePairedFieldWritesOutcome.MissingValidatedEntryEBP
            && missingEntry.PairedFieldWrites.FirstWriteReached == false
            && missingEntry.PairedFieldWrites.SecondWriteReached == false
            && missingEntry.TagValidation is null
            && !missingEntry.TagValidationReached
            && missingEntry.UnreachedTagEntryESIProvided
            && missingEntry.UnreachedTagEntryEDXProvided
            && missingEntry.UnreachedTagEdxPlusFourDwordProvided
            && missingEntry.UnreachedTagGlobalDwordProvided
            && !missingEntry.Admitted
            && missingEntry.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePairedFieldWritesProjection.EntryRawFileOffset);

        CapacityFinalCalleePairedTagLifecyclePlan zeroEntry =
            CapacityFinalCalleePairedTagLifecycleProjection.Project(Capture(
                validatedEntryEBP: 0,
                validatedEntryEAX: 0,
                freshFrameMinus4: 0,
                freshGlobalValue: 0,
                tagEntryESI: 0,
                tagEntryEDX: 0,
                tagEdxPlusFourDword: null,
                tagGlobalDword: null));
        check("Captured zero paired values and independent zero tag registers are valid",
            zeroEntry.PairedFieldWrites.Outcome
                == CapacityFinalCalleePairedFieldWritesOutcome.Completed
            && zeroEntry.PairedFieldWrites.FreshFrameMinus4Address == 0xFFFFFFFC
            && zeroEntry.PairedFieldWrites.ECXLoaded == 0
            && zeroEntry.PairedFieldWrites.EAXRetained == 0
            && zeroEntry.PairedFieldWrites.FirstWriteAddress == 0xC
            && zeroEntry.PairedFieldWrites.FirstWriteValue == 0
            && zeroEntry.PairedFieldWrites.SecondWriteAddress == 8
            && zeroEntry.PairedFieldWrites.SecondWriteValue == 0
            && zeroEntry.PairedFieldWrites.EBXFresh == 0
            && zeroEntry.TagValidationReached
            && zeroEntry.TagValidation?.Capture.EntryESI == 0
            && zeroEntry.TagValidation?.Capture.EntryEDX == 0
            && zeroEntry.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome
                    .TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit
            && zeroEntry.Admitted
            && zeroEntry.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePairedTagLifecycleProjection.TagUnsignedExitRawFileOffset);

        CapacityFinalCalleePairedTagLifecyclePlan missingPairEax =
            CapacityFinalCalleePairedTagLifecycleProjection.Project(Capture(
                validatedEntryEAX: null,
                tagEntryESI: 0,
                tagEntryEDX: 1,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 1));
        CapacityFinalCalleePairedTagLifecyclePlan missingPairGlobal =
            CapacityFinalCalleePairedTagLifecycleProjection.Project(Capture(
                freshGlobalValue: null,
                tagEntryESI: null,
                tagEntryEDX: null,
                tagEdxPlusFourDword: null,
                tagGlobalDword: null));
        check("Partial writes stop before both stores; a missing global reload preserves both reached writes",
            missingPairEax.PairedFieldWrites.Outcome
                == CapacityFinalCalleePairedFieldWritesOutcome.MissingValidatedEntryEAX
            && !missingPairEax.PairedFieldWrites.FirstWriteReached
            && !missingPairEax.PairedFieldWrites.SecondWriteReached
            && missingPairEax.UnreachedTagEntryESIProvided
            && missingPairEax.UnreachedTagEntryEDXProvided
            && !missingPairEax.Admitted
            && missingPairGlobal.PairedFieldWrites.Outcome
                == CapacityFinalCalleePairedFieldWritesOutcome.MissingFreshGlobalValue
            && missingPairGlobal.PairedFieldWrites.FirstWriteReached
            && missingPairGlobal.PairedFieldWrites.SecondWriteReached
            && missingPairGlobal.PairedFieldWrites.FirstWriteAddress == 0x30C
            && missingPairGlobal.PairedFieldWrites.FirstWriteValue == 0x200
            && missingPairGlobal.PairedFieldWrites.SecondWriteAddress == 0x208
            && missingPairGlobal.PairedFieldWrites.SecondWriteValue == 0x300
            && missingPairGlobal.PairedFieldWrites.FreshGlobalReadReached
            && !missingPairGlobal.PairedFieldWrites.FreshGlobalValueCaptured
            && missingPairGlobal.TagValidation is null
            && !missingPairGlobal.Admitted);

        CapacityFinalCalleePairedTagLifecyclePlan missingTagEsi =
            CapacityFinalCalleePairedTagLifecycleProjection.Project(Capture(
                tagEntryESI: null,
                tagEntryEDX: 1,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 1));
        CapacityFinalCalleePairedTagLifecyclePlan missingTagEdx =
            CapacityFinalCalleePairedTagLifecycleProjection.Project(Capture(
                tagEntryESI: 0,
                tagEntryEDX: null,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 1));
        check("Neither tag entry register is inferred from the preceding writes",
            missingTagEsi.PairedFieldWrites.FirstWriteReached
            && missingTagEsi.PairedFieldWrites.SecondWriteReached
            && missingTagEsi.PairedFieldWrites.FreshGlobalValueCaptured
            && missingTagEsi.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome.TagEntryRegistersMissing
            && !missingTagEsi.TagValidationReached
            && !missingTagEsi.UnreachedTagEntryESIProvided
            && missingTagEsi.UnreachedTagEntryEDXProvided
            && missingTagEsi.UnreachedTagEdxPlusFourDwordProvided
            && missingTagEsi.UnreachedTagGlobalDwordProvided
            && !missingTagEsi.Admitted
            && missingTagEdx.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome.TagEntryRegistersMissing
            && !missingTagEdx.TagValidationReached
            && missingTagEdx.UnreachedTagEntryESIProvided
            && !missingTagEdx.UnreachedTagEntryEDXProvided
            && !missingTagEdx.Admitted
            && missingTagEdx.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePairedTagLifecycleProjection.TagEntryRawFileOffset);

        CapacityFinalCalleePairedTagLifecyclePlan missingTagEdxPlusFour =
            ProjectTag(0, 1, null, null);
        CapacityFinalCalleePairedTagLifecyclePlan missingTagGlobal =
            ProjectTag(0, 1, 1, null);
        check("Missing later tag reads retain the already completed paired writes",
            missingTagEdxPlusFour.PairedFieldWrites.FirstWriteReached
            && missingTagEdxPlusFour.PairedFieldWrites.SecondWriteReached
            && missingTagEdxPlusFour.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.MissingEdxPlusFourDword
            && missingTagEdxPlusFour.TagValidation?.EdxPlusFourReadReached == true
            && !missingTagEdxPlusFour.Admitted
            && missingTagGlobal.PairedFieldWrites.FirstWriteReached
            && missingTagGlobal.PairedFieldWrites.SecondWriteReached
            && missingTagGlobal.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.MissingGlobalDword
            && missingTagGlobal.TagValidation?.GlobalReadReached == true
            && !missingTagGlobal.Admitted);

        CapacityFinalCalleePairedTagLifecyclePlan unsignedEqual =
            ProjectTag(0x80000000, 0x80000000, null, null);
        CapacityFinalCalleePairedTagLifecyclePlan unsignedAbove =
            ProjectTag(0x80000000, 0x7FFFFFFF, null, null);
        CapacityFinalCalleePairedTagLifecyclePlan unsignedBelow =
            ProjectTag(0x7FFFFFFF, 0x80000000, 0, null);
        check("Unsigned equality and above take the bounded JNB route; below continues",
            unsignedEqual.TagValidation?.CompareEsiEdxFlags is
            {
                Carry: false,
                Zero: true
            }
            && unsignedEqual.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome
                    .TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit
            && unsignedEqual.Admitted
            && unsignedEqual.TagValidation?.Reads.IsEmpty == true
            && unsignedAbove.TagValidation?.CompareEsiEdxFlags is
            {
                Carry: false,
                Zero: false
            }
            && unsignedAbove.Admitted
            && unsignedAbove.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePairedTagLifecycleProjection.TagUnsignedExitRawFileOffset
            && unsignedBelow.TagValidation?.CompareEsiEdxFlags is
            {
                Carry: true,
                Zero: false
            }
            && unsignedBelow.TagValidation?.EdxPlusFourAddress == 0x80000004
            && unsignedBelow.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome.TagClBitZeroClearExit
            && unsignedBelow.Admitted);

        CapacityFinalCalleePairedTagLifecyclePlan bitZeroClear =
            ProjectTag(0, 1, 0, null);
        CapacityFinalCalleePairedTagLifecyclePlan bitOneSet =
            ProjectTag(0, 1, 3, null);
        CapacityFinalCalleePairedTagLifecyclePlan globalUnequal =
            ProjectTag(0, 1, 1, 2);
        CapacityFinalCalleePairedTagLifecyclePlan globalEqual =
            ProjectTag(0, 1, 1, 1);
        check("Tag-bit exits and both fresh global-comparison routes are bounded outcomes",
            bitZeroClear.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome.TagClBitZeroClearExit
            && bitZeroClear.TagValidation?.ClZeroBranchTaken == true
            && bitZeroClear.TagValidation?.UnreachedGlobalDwordCaptureProvided == false
            && bitZeroClear.Admitted
            && bitZeroClear.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePairedTagLifecycleProjection.TagClBitZeroClearExitRawFileOffset
            && bitOneSet.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome.TagClBitOneSetExit
            && bitOneSet.TagValidation?.ClOneBranchTaken == true
            && bitOneSet.TagValidation?.UnreachedGlobalDwordCaptureProvided == false
            && bitOneSet.Admitted
            && bitOneSet.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePairedTagLifecycleProjection.TagClBitOneSetExitRawFileOffset
            && globalUnequal.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome.TagEntryEdxNotEqualGlobalDwordExit
            && globalUnequal.TagValidation?.NotEqualBranchTaken == true
            && globalUnequal.Admitted
            && globalUnequal.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePairedTagLifecycleProjection.TagNotEqualExitRawFileOffset
            && globalEqual.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome.TagEntryEdxEqualsGlobalDwordBoundary
            && globalEqual.TagValidation?.NotEqualBranchTaken == false
            && globalEqual.Admitted
            && globalEqual.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePairedTagLifecycleProjection.TagBoundaryRawFileOffsetExclusive);

        CapacityFinalCalleePairedTagLifecyclePlan laterCapturesOnUnsignedExit =
            ProjectTag(2, 1, 1, 1);
        CapacityFinalCalleePairedTagLifecyclePlan laterGlobalOnBitExit =
            ProjectTag(0, 1, 0, 7);
        check("Supplied captures for unselected tag reads are propagated and block admission",
            laterCapturesOnUnsignedExit.TagValidation?.UnreachedEdxPlusFourDwordCaptureProvided
            == true
            && laterCapturesOnUnsignedExit.TagValidation?.UnreachedGlobalDwordCaptureProvided
                == true
            && laterCapturesOnUnsignedExit.UnreachedTagEdxPlusFourDwordProvided
            && laterCapturesOnUnsignedExit.UnreachedTagGlobalDwordProvided
            && !laterCapturesOnUnsignedExit.Admitted
            && laterGlobalOnBitExit.TagValidation?.UnreachedGlobalDwordCaptureProvided == true
            && laterGlobalOnBitExit.UnreachedTagGlobalDwordProvided
            && !laterGlobalOnBitExit.Admitted
            && laterGlobalOnBitExit.Outcome
                == CapacityFinalCalleePairedTagLifecycleOutcome.TagValidationIncomplete);

        CapacityFinalCalleePairedTagLifecyclePlan aliasingFreshReads =
            CapacityFinalCalleePairedTagLifecycleProjection.Project(Capture(
                validatedEntryEAX: 0x1000,
                freshFrameMinus4:
                    CapacityFinalCalleeTagValidationProjection.GlobalDwordVirtualAddress - 0xC,
                freshGlobalValue: 0xAABBCCDD,
                tagEntryESI: 0,
                tagEntryEDX: 0x1004,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 0x11223344));
        check("Fresh tag reads stay independent when their addresses alias preceding writes",
            aliasingFreshReads.PairedFieldWrites.FirstWriteAddress
                == CapacityFinalCalleeTagValidationProjection.GlobalDwordVirtualAddress
            && aliasingFreshReads.PairedFieldWrites.FirstWriteValue == 0x1000
            && aliasingFreshReads.PairedFieldWrites.SecondWriteAddress == 0x1008
            && aliasingFreshReads.PairedFieldWrites.SecondWriteValue
                == CapacityFinalCalleeTagValidationProjection.GlobalDwordVirtualAddress - 0xC
            && aliasingFreshReads.PairedFieldWrites.EBXFresh == 0xAABBCCDD
            && aliasingFreshReads.TagValidation?.Reads[0].Address == 0x1008
            && aliasingFreshReads.TagValidation?.Reads[0].Value == 1
            && aliasingFreshReads.TagValidation?.Reads[1].Address
                == CapacityFinalCalleeTagValidationProjection.GlobalDwordVirtualAddress
            && aliasingFreshReads.TagValidation?.Reads[1].Value == 0x11223344
            && aliasingFreshReads.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword
            && aliasingFreshReads.Admitted);

        CapacityFinalCalleePairedTagLifecyclePlan wrapped =
            CapacityFinalCalleePairedTagLifecycleProjection.Project(Capture(
                validatedEntryEBP: 0,
                validatedEntryEAX: uint.MaxValue - 3,
                freshFrameMinus4: uint.MaxValue - 7,
                freshGlobalValue: 0,
                tagEntryESI: 0,
                tagEntryEDX: uint.MaxValue,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: uint.MaxValue));
        check("Paired writes and the fresh [EDX+4] read use independent wrapping DWORD addresses",
            wrapped.PairedFieldWrites.FreshFrameMinus4Address == 0xFFFFFFFC
            && wrapped.PairedFieldWrites.FirstWriteAddress == 4
            && wrapped.PairedFieldWrites.SecondWriteAddress == 4
            && wrapped.TagValidation?.EdxPlusFourAddress == 3
            && wrapped.TagValidation?.Reads[0].Address == 3
            && wrapped.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword
            && wrapped.Admitted
            && wrapped.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePairedTagLifecycleProjection.TagBoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleePairedTagLifecyclePlan ProjectTag(
        uint entryEsi,
        uint entryEdx,
        uint? edxPlusFourDword,
        uint? globalDword) =>
        CapacityFinalCalleePairedTagLifecycleProjection.Project(Capture(
            tagEntryESI: entryEsi,
            tagEntryEDX: entryEdx,
            tagEdxPlusFourDword: edxPlusFourDword,
            tagGlobalDword: globalDword));

    private static CapacityFinalCalleePairedTagLifecycleCapture Capture(
        uint? validatedEntryEBP = 0x1000,
        uint? validatedEntryEAX = 0x200,
        uint? freshFrameMinus4 = 0x300,
        uint? freshGlobalValue = 0x400,
        uint? tagEntryESI = 0,
        uint? tagEntryEDX = 1,
        uint? tagEdxPlusFourDword = 1,
        uint? tagGlobalDword = 1) =>
        new(
            new(
                validatedEntryEBP,
                validatedEntryEAX,
                freshFrameMinus4,
                freshGlobalValue),
            tagEntryESI,
            tagEntryEDX,
            tagEdxPlusFourDword,
            tagGlobalDword);
}
