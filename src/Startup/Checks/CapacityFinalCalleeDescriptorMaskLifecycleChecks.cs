using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeDescriptorMaskLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition joins only descriptor equality at the frame-mask entry",
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.DescriptorEntryRawFileOffset
                == 0x012C8E9A
            && CapacityFinalCalleeDescriptorMaskLifecycleProjection
                .DescriptorEqualityDestinationRawFileOffset == 0x012C8EBE
            && CapacityFinalCalleeDescriptorMaskLifecycleProjection
                .UnselectedDescriptorDestinationRawFileOffset == 0x012C8EB8
            && CapacityFinalCalleeDescriptorMaskLifecycleProjection.FrameMaskEntryRawFileOffset
                == 0x012C8EBE
            && CapacityFinalCalleeDescriptorMaskLifecycleProjection
                .FrameMaskMismatchDestinationRawFileOffset == 0x012C8ED5
            && CapacityFinalCalleeDescriptorMaskLifecycleProjection
                .FrameMaskContinuationRawFileOffset == 0x012C8EF8);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan firstEquality =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0x20,
                descriptorFreshFrameMinus4: 0x11111111,
                descriptorFreshFrameMinus10: 0x11111111,
                freshFrameMinus14: 0xA5A5A5A5,
                freshFrameMinus4: 0xA5A5A5A5,
                freshFrameMinus8: 0x80000001,
                freshGlobalValue: 0xFFFFFFFF));
        check("A clean first comparison equality admits the independent frame-mask reads",
            firstEquality.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.GlobalValueUpdated
            && firstEquality.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
            && firstEquality.DescriptorPlan.DescriptorAddressWriteReached
            && firstEquality.DescriptorEqualityDestinationReached
            && firstEquality.FrameMaskUpdateReached
            && firstEquality.FrameMaskValidatedEntryEBP == 0x1000
            && firstEquality.FrameMaskValidatedEntryECX == 0x20
            && firstEquality.FrameMaskPlan is
            {
                Outcome: CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated,
                FreshFrameMinus14: 0xA5A5A5A5,
                FreshFrameMinus4: 0xA5A5A5A5,
                FreshFrameMinus8: 0x80000001,
                ShiftCount: 0,
                GlobalValueAfter: 0x7FFFFFFE,
                StopAtRawFileOffsetExclusive: 0x012C8EF8
            }
            && firstEquality.FrameMaskRouteAdmitted
            && NoUnreachedCaptures(firstEquality)
            && firstEquality.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan finalEquality =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0x1F,
                entryEbx: 0x200,
                entryEsi: 0x50,
                descriptorFreshFrameMinus4: 0x300,
                descriptorFreshFrameMinus10: 0x100,
                freshEaxPlusC: 0x50,
                freshFrameMinus14: 9,
                freshFrameMinus4: 9,
                freshFrameMinus8: 0x80000001,
                freshGlobalValue: 0xFFFFFFFF));
        check("A clean final comparison equality also admits the local frame-mask route",
            finalEquality.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.GlobalValueUpdated
            && finalEquality.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FreshEaxPlusCEqualExit
            && finalEquality.DescriptorPlan.FinalSelectedDestinationRawFileOffset
                == 0x012C8EBE
            && finalEquality.DescriptorPlan.DescriptorAddressWriteReached
            && finalEquality.FrameMaskUpdateReached
            && finalEquality.FrameMaskValidatedEntryECX == 0x1F
            && finalEquality.FrameMaskPlan?.ShiftCount == 31
            && finalEquality.FrameMaskPlan?.ShiftedValue == 0x80000000
            && finalEquality.FrameMaskPlan?.NotMask == 0x7FFFFFFF
            && finalEquality.FrameMaskPlan?.GlobalValueAfter == 0x7FFFFFFF
            && finalEquality.FrameMaskRouteAdmitted
            && NoUnreachedCaptures(finalEquality)
            && finalEquality.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan independentFrameFour =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 3,
                descriptorFreshFrameMinus4: 0x1111,
                descriptorFreshFrameMinus10: 0x1111,
                freshFrameMinus14: 0x2222,
                freshFrameMinus4: 0x3333,
                freshFrameMinus8: 0x4444,
                freshGlobalValue: 0xFFFFFFFF));
        check("The frame-mask EBP-4 read is not reused from descriptor reload",
            independentFrameFour.DescriptorPlan.FreshFrameMinus4 == 0x1111
            && independentFrameFour.FrameMaskPlan?.FreshFrameMinus4 == 0x3333
            && independentFrameFour.FrameMaskPlan?.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch
            && independentFrameFour.FrameMaskPlan?.StopAtRawFileOffsetExclusive == 0x012C8ED5);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan preservedEntryCount =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0x20,
                descriptorFreshFrameMinus4: 0x44,
                descriptorFreshFrameMinus10: 0x44,
                freshFrameMinus14: 4,
                freshFrameMinus4: 4,
                freshFrameMinus8: 0x80000001,
                freshGlobalValue: 0xFFFFFFFF));
        check("The preserved initial ECX, not scaled or reloaded EAX, supplies the shift count",
            preservedEntryCount.DescriptorPlan.WrittenFrameMinus10
                == CapacityFinalCalleeDescriptorReloadProjection.DeriveDescriptorAddress(0x20)
            && preservedEntryCount.DescriptorPlan.FreshFrameMinus10 == 0x44
            && preservedEntryCount.FrameMaskValidatedEntryECX == 0x20
            && preservedEntryCount.FrameMaskPlan?.ShiftCount == 0
            && preservedEntryCount.FrameMaskPlan?.ShiftedValue == 0x80000001);

        (uint Input, uint Expected)[] countCases =
        [
            (0, 0),
            (31, 31),
            (0xFFFFFFFF, 31)
        ];
        bool countCasesMatch = true;
        foreach ((uint input, uint expected) in countCases)
        {
            CapacityFinalCalleeDescriptorMaskLifecyclePlan countPlan =
                CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                    initialFrameMinus10: input,
                    descriptorFreshFrameMinus4: 7,
                    descriptorFreshFrameMinus10: 7,
                    freshFrameMinus14: 6,
                    freshFrameMinus4: 6,
                    freshFrameMinus8: 0x80000001,
                    freshGlobalValue: 0xFFFFFFFF));
            countCasesMatch &= countPlan.FrameMaskValidatedEntryECX == input
                && countPlan.DescriptorPlan.WrittenFrameMinus10
                    == CapacityFinalCalleeDescriptorReloadProjection
                        .DeriveDescriptorAddress(input)
                && countPlan.FrameMaskPlan?.ShiftCount == expected
                && countPlan.FrameMaskPlan?.ShiftedValue
                    == unchecked(0x80000001u << (int)expected)
                && countPlan.StopAtRawFileOffsetExclusive == 0x012C8EF8;
        }

        check("Entry shift counts 0, 31, and wrapped DWORD counts use x86's low five bits",
            countCasesMatch);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan missingFirstRead =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: null,
                freshFrameMinus14: 0,
                freshFrameMinus4: 0,
                freshFrameMinus8: 0,
                freshGlobalValue: 0));
        check("A missing descriptor entry read blocks composition and flags later zero captures",
            missingFirstRead.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.ContaminatedDescriptorRoute
            && missingFirstRead.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.MissingInitialFrameMinus10
            && missingFirstRead.DescriptorPlan.DescriptorAddressWriteReached == false
            && !missingFirstRead.FrameMaskUpdateReached
            && missingFirstRead.UnreachedFreshFrameMinus14CaptureProvided
            && missingFirstRead.UnreachedFreshFrameMinus4CaptureProvided
            && missingFirstRead.UnreachedFreshFrameMinus8CaptureProvided
            && missingFirstRead.UnreachedFreshGlobalValueCaptureProvided
            && missingFirstRead.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan cleanMissingFirstRead =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: null,
                descriptorFreshFrameMinus4: null,
                descriptorFreshFrameMinus10: null,
                freshFrameMinus14: null,
                freshFrameMinus4: null,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("Missing descriptor input remains distinct from a captured zero",
            cleanMissingFirstRead.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.MissingInitialFrameMinus10
            && !cleanMissingFirstRead.FrameMaskUpdateReached
            && !cleanMissingFirstRead.UnreachedFreshFrameMinus14CaptureProvided
            && cleanMissingFirstRead.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan missingFrameRead =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0,
                descriptorFreshFrameMinus10: 0,
                freshFrameMinus14: null,
                freshFrameMinus4: 0,
                freshFrameMinus8: 0,
                freshGlobalValue: 0));
        check("A missing frame read preserves the earlier descriptor-address write",
            missingFrameRead.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.MissingFreshFrameMinus14
            && missingFrameRead.DescriptorPlan.DescriptorAddressWriteReached
            && missingFrameRead.DescriptorPlan.WrittenFrameMinus10
                == CapacityFinalCalleeDescriptorReloadProjection
                    .EncodedDescriptorBasePeVirtualAddress
            && missingFrameRead.FrameMaskPlan?.FreshFrameMinus14ReadReached == true
            && missingFrameRead.FrameMaskPlan?.FreshFrameMinus14Captured == false
            && missingFrameRead.FrameMaskPlan?.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan missingGlobal =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0x20,
                descriptorFreshFrameMinus4: 0x88,
                descriptorFreshFrameMinus10: 0x88,
                freshFrameMinus14: 4,
                freshFrameMinus4: 4,
                freshFrameMinus8: 0x12345678,
                freshGlobalValue: null));
        check("A missing global read leaves prior descriptor and local mask computations intact",
            missingGlobal.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.MissingFreshGlobalValue
            && missingGlobal.DescriptorPlan.DescriptorAddressWriteReached
            && missingGlobal.FrameMaskPlan?.ShiftReached == true
            && missingGlobal.FrameMaskPlan?.NotMask == ~0x12345678u
            && missingGlobal.FrameMaskPlan?.GlobalReadReached == true
            && missingGlobal.FrameMaskPlan?.GlobalValueCaptured == false
            && missingGlobal.FrameMaskPlan?.GlobalWriteReached == false
            && missingGlobal.FrameMaskPlan?.GlobalValueAfter is null
            && missingGlobal.FrameMaskPlan?.StopAtRawFileOffsetExclusive == 0x012C8ECD);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan zeroCaptures =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0,
                descriptorFreshFrameMinus10: 0,
                freshFrameMinus14: 0,
                freshFrameMinus4: 0,
                freshFrameMinus8: 0,
                freshGlobalValue: 0));
        check("Captured zero values are valid reads and can complete the mask write",
            zeroCaptures.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.GlobalValueUpdated
            && zeroCaptures.DescriptorPlan.InitialFrameMinus10Captured
            && zeroCaptures.FrameMaskPlan?.FreshFrameMinus14Captured == true
            && zeroCaptures.FrameMaskPlan?.FreshFrameMinus4Captured == true
            && zeroCaptures.FrameMaskPlan?.FreshFrameMinus8Captured == true
            && zeroCaptures.FrameMaskPlan?.ShiftCount == 0
            && zeroCaptures.FrameMaskPlan?.NotMask == 0xFFFFFFFF
            && zeroCaptures.FrameMaskPlan?.GlobalValueCaptured == true
            && zeroCaptures.FrameMaskPlan?.GlobalValueBefore == 0
            && zeroCaptures.FrameMaskPlan?.GlobalValueAfter == 0
            && NoUnreachedCaptures(zeroCaptures));

        CapacityFinalCalleeDescriptorMaskLifecyclePlan cleanMismatch =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0x20,
                descriptorFreshFrameMinus4: 0x100,
                descriptorFreshFrameMinus10: 0x100,
                freshFrameMinus14: 1,
                freshFrameMinus4: 2,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("A clean frame mismatch is a bounded ED5 route with no later reads",
            cleanMismatch.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.FrameMismatch
            && cleanMismatch.DescriptorEqualityDestinationReached
            && cleanMismatch.FrameMaskPlan is { } mismatchPlan
            && mismatchPlan.Outcome == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch
            && cleanMismatch.FrameMaskRouteAdmitted
            && mismatchPlan.SelectedBranchDestinationRawFileOffset == 0x012C8ED5
            && !mismatchPlan.FreshFrameMinus8ReadReached
            && !mismatchPlan.GlobalReadReached
            && !mismatchPlan.GlobalWriteReached
            && NoUnreachedCaptures(cleanMismatch)
            && cleanMismatch.StopAtRawFileOffsetExclusive == 0x012C8ED5);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan cleanUnsignedBelow =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x10,
                descriptorFreshFrameMinus10: 0x20,
                entryEbx: 0x20,
                freshFrameMinus14: null,
                freshFrameMinus4: null,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("The descriptor unsigned-below destination remains bounded at EB8",
            cleanUnsignedBelow.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .DescriptorEntryEaxUnsignedBelowExit
            && cleanUnsignedBelow.DescriptorPlan.SecondSelectedDestinationRawFileOffset
                == 0x012C8EB8
            && !cleanUnsignedBelow.DescriptorPlan
                .UnreachedFreshEaxPlusCCaptureProvided
            && !cleanUnsignedBelow.FrameMaskUpdateReached
            && cleanUnsignedBelow.FrameMaskPlan is null
            && cleanUnsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan contaminatedBelowDescriptorRead =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x10,
                descriptorFreshFrameMinus10: 0x20,
                entryEbx: 0x20,
                freshEaxPlusC: 0,
                freshFrameMinus14: null,
                freshFrameMinus4: null,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("An unselected EB8 route rejects an unreached descriptor EAX+0C capture",
            contaminatedBelowDescriptorRead.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .ContaminatedDescriptorRoute
            && contaminatedBelowDescriptorRead.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.EntryEaxUnsignedBelowExit
            && contaminatedBelowDescriptorRead.DescriptorPlan
                .UnreachedFreshEaxPlusCCaptureProvided
            && !contaminatedBelowDescriptorRead.FrameMaskUpdateReached
            && !contaminatedBelowDescriptorRead.UnreachedFreshFrameMinus14CaptureProvided
            && !contaminatedBelowDescriptorRead.UnreachedFreshFrameMinus4CaptureProvided
            && !contaminatedBelowDescriptorRead.UnreachedFreshFrameMinus8CaptureProvided
            && !contaminatedBelowDescriptorRead.UnreachedFreshGlobalValueCaptureProvided
            && contaminatedBelowDescriptorRead.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan contaminatedBelow =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x10,
                descriptorFreshFrameMinus10: 0x20,
                entryEbx: 0x20,
                freshFrameMinus14: 0,
                freshFrameMinus4: null,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("Mask-only captures on the unselected EB8 route are rejected as unreached",
            contaminatedBelow.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.ContaminatedDescriptorRoute
            && contaminatedBelow.DescriptorPlan.StopAtRawFileOffsetExclusive == 0x012C8EB8
            && !contaminatedBelow.FrameMaskUpdateReached
            && contaminatedBelow.UnreachedFreshFrameMinus14CaptureProvided
            && !contaminatedBelow.UnreachedFreshFrameMinus4CaptureProvided
            && !contaminatedBelow.UnreachedFreshFrameMinus8CaptureProvided
            && !contaminatedBelow.UnreachedFreshGlobalValueCaptureProvided
            && contaminatedBelow.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan cleanFinalUnequal =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x40,
                descriptorFreshFrameMinus10: 0x30,
                entryEbx: 0x20,
                entryEsi: 0x50,
                freshEaxPlusC: 0x51,
                freshFrameMinus14: null,
                freshFrameMinus4: null,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("A final descriptor inequality remains bounded at EB8 without an opaque call",
            cleanFinalUnequal.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .DescriptorFinalCompareUnequalAtBoundary
            && cleanFinalUnequal.DescriptorPlan.FinalBranchTaken == false
            && cleanFinalUnequal.DescriptorPlan.FinalSelectedDestinationRawFileOffset is null
            && !cleanFinalUnequal.FrameMaskUpdateReached
            && cleanFinalUnequal.FrameMaskPlan is null
            && cleanFinalUnequal.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan contaminatedFinalUnequal =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x40,
                descriptorFreshFrameMinus10: 0x30,
                entryEbx: 0x20,
                entryEsi: 0x50,
                freshEaxPlusC: 0x51,
                freshFrameMinus14: null,
                freshFrameMinus4: 0,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("A frame-mask capture supplied after final inequality is flagged as unreached",
            contaminatedFinalUnequal.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.ContaminatedDescriptorRoute
            && !contaminatedFinalUnequal.FrameMaskUpdateReached
            && contaminatedFinalUnequal.UnreachedFreshFrameMinus4CaptureProvided
            && contaminatedFinalUnequal.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorMaskLifecyclePlan contaminatedDescriptorRead =
            CapacityFinalCalleeDescriptorMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 3,
                descriptorFreshFrameMinus4: 3,
                descriptorFreshFrameMinus10: 3,
                freshEaxPlusC: 0x20,
                freshFrameMinus14: 0,
                freshFrameMinus4: 0,
                freshFrameMinus8: 0,
                freshGlobalValue: 0));
        check("A descriptor read captured after the first equality cannot extend that route",
            contaminatedDescriptorRead.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
            && contaminatedDescriptorRead.DescriptorPlan
                .UnreachedFreshEaxPlusCCaptureProvided
            && contaminatedDescriptorRead.Outcome
                == CapacityFinalCalleeDescriptorMaskLifecycleOutcome.ContaminatedDescriptorRoute
            && !contaminatedDescriptorRead.FrameMaskUpdateReached
            && contaminatedDescriptorRead.UnreachedFreshFrameMinus14CaptureProvided
            && contaminatedDescriptorRead.StopAtRawFileOffsetExclusive == 0x012C8EBE);
    }

    private static bool NoUnreachedCaptures(
        CapacityFinalCalleeDescriptorMaskLifecyclePlan plan) =>
        plan.FrameMaskRouteAdmitted
        && plan.FrameMaskPlan?.UnreachedEntryECXCaptureProvided != true
        && !plan.DescriptorPlan.UnreachedInitialFrameMinus10CaptureProvided
        && !plan.DescriptorPlan.UnreachedFreshFrameMinus4CaptureProvided
        && !plan.DescriptorPlan.UnreachedFreshFrameMinus10CaptureProvided
        && !plan.DescriptorPlan.UnreachedFreshEaxPlusCCaptureProvided
        && !plan.UnreachedFreshFrameMinus14CaptureProvided
        && !plan.UnreachedFreshFrameMinus4CaptureProvided
        && !plan.UnreachedFreshFrameMinus8CaptureProvided
        && !plan.UnreachedFreshGlobalValueCaptureProvided;

    private static CapacityFinalCalleeDescriptorMaskLifecycleCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEbp = 0x1000,
        uint entryEbx = 0x100,
        uint entryEsi = 0x200,
        uint? initialFrameMinus10 = 0x20,
        uint? descriptorFreshFrameMinus4 = 0x11111111,
        uint? descriptorFreshFrameMinus10 = 0x11111111,
        uint? freshEaxPlusC = null,
        uint? freshFrameMinus14 = 1,
        uint? freshFrameMinus4 = 1,
        uint? freshFrameMinus8 = 1,
        uint? freshGlobalValue = 0xFFFFFFFF) =>
        new(
            new CapacityFinalCalleeDescriptorReloadCapture(
                CaptureValidatedEntry: captureValidatedEntry,
                EntryEBP: entryEbp,
                EntryEBX: entryEbx,
                EntryESI: entryEsi,
                InitialFrameMinus10: initialFrameMinus10,
                FreshFrameMinus4: descriptorFreshFrameMinus4,
                FreshFrameMinus10: descriptorFreshFrameMinus10,
                FreshEaxPlusC: freshEaxPlusC),
            freshFrameMinus14,
            freshFrameMinus4,
            freshFrameMinus8,
            freshGlobalValue);
}
