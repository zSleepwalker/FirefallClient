using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeDescriptorFallbackMaskLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The EB8 fallback call and EBE mask anchors match the verified instruction window",
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.CallRawFileOffset
                == 0x012C8EB8
            && CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection
                .CallBoundaryRawFileOffsetExclusive == 0x012C8EBE
            && CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection
                .FrameMaskEntryRawFileOffset == 0x012C8EBE
            && CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection
                .CallCellVirtualAddress == 0x01AC4A24
            && CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection
                .CallInstructionVirtualAddress == 0x016C9AB8
            && Normalize(CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection
                .CallInstructionBytes) == "FF15244AAC01");

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan directEquality =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0x20,
                descriptorFreshFrameMinus4: 0x11111111,
                descriptorFreshFrameMinus10: 0x11111111,
                freshFrameMinus14: 0xA5A5A5A5,
                freshFrameMinus4: 0xA5A5A5A5,
                freshFrameMinus8: 0x80000001,
                freshGlobalValue: 0xFFFFFFFF));
        check("A clean descriptor equality bypasses the call and shifts by the initial ECX load",
            directEquality.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.GlobalValueUpdated
            && directEquality.DescriptorEqualityDestinationReached
            && !directEquality.DescriptorBelowDestinationReached
            && !directEquality.CallCellReadReached
            && !directEquality.OpaqueCallReached
            && directEquality.OpaqueCallPreCallEBP is null
            && directEquality.OpaqueCallPreCallECX is null
            && directEquality.FrameMaskValidatedEntryEBP == 0x1000
            && directEquality.FrameMaskValidatedEntryECX == 0x20
            && directEquality.FrameMaskPlan is
            {
                Outcome: CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated,
                ShiftCount: 0,
                ShiftedValue: 0x80000001,
                NotMask: 0x7FFFFFFE,
                BranchTaken: false,
                SelectedBranchDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: 0x012C8EF8
            }
            && directEquality.StopAtRawFileOffsetExclusive == 0x012C8EF8
            && NoUnreachedCaptures(directEquality));

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan calledBelow =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x10,
                descriptorFreshFrameMinus10: 0x20,
                entryEbx: 0x20,
                callCellTarget: 0,
                callReturnedNormally: true,
                postCallEBP: 0x1000,
                postCallECX: 0x1F,
                freshFrameMinus14: 4,
                freshFrameMinus4: 4,
                freshFrameMinus8: 0x80000001,
                freshGlobalValue: 0xFFFFFFFF));
        check("The unsigned-below EB8 route calls even when the captured cell target is zero",
            calledBelow.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.GlobalValueUpdated
            && calledBelow.DescriptorBelowDestinationReached
            && !calledBelow.DescriptorEqualityDestinationReached
            && calledBelow.DescriptorPlan.DescriptorAddressWriteReached
            && calledBelow.CallCellReadReached
            && calledBelow.CallCellTargetCaptured
            && calledBelow.CallCellTargetVirtualAddress == 0
            && calledBelow.OpaqueCallReached
            && calledBelow.CallReturnedNormally == true
            && calledBelow.CallOutputsOpaque
            && calledBelow.OpaqueCallPreCallEBP == 0x1000
            && calledBelow.OpaqueCallPreCallECX == 0
            && calledBelow.PostCallEBPCaptured
            && calledBelow.PostCallEBP == 0x1000
            && calledBelow.PostCallECXCaptured
            && calledBelow.PostCallECX == 0x1F
            && calledBelow.FrameMaskValidatedEntryEBP == 0x1000
            && calledBelow.FrameMaskValidatedEntryECX == 0x1F
            && calledBelow.FrameMaskPlan?.ShiftCount == 31
            && calledBelow.FrameMaskPlan?.ShiftedValue == 0x80000000
            && calledBelow.FrameMaskPlan?.NotMask == 0x7FFFFFFF
            && calledBelow.FrameMaskPlan?.GlobalValueAfter == 0x7FFFFFFF
            && calledBelow.StopAtRawFileOffsetExclusive == 0x012C8EF8
            && NoUnreachedCaptures(calledBelow));

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan calledFallThrough =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 3,
                descriptorFreshFrameMinus4: 0x40,
                descriptorFreshFrameMinus10: 0x30,
                entryEbx: 0x20,
                entryEsi: 0x50,
                freshEaxPlusC: 0x51,
                callCellTarget: 0x00401234,
                callReturnedNormally: true,
                postCallEBP: 0x2000,
                postCallECX: 0x18,
                freshFrameMinus14: 7,
                freshFrameMinus4: 7,
                freshFrameMinus8: 0x10,
                freshGlobalValue: 0xFFFFFFFF));
        check("A clean final descriptor inequality also enters the EB8 call path",
            calledFallThrough.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .GlobalValueUpdated
            && calledFallThrough.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FellThroughToBoundary
            && calledFallThrough.DescriptorPlan.WrittenFrameMinus10
                == CapacityFinalCalleeDescriptorReloadProjection.DeriveDescriptorAddress(3)
            && calledFallThrough.DescriptorBelowDestinationReached == false
            && calledFallThrough.CallCellReadReached
            && calledFallThrough.OpaqueCallPreCallEBP == 0x1000
            && calledFallThrough.OpaqueCallPreCallECX == 3
            && calledFallThrough.FrameMaskValidatedEntryEBP == 0x2000
            && calledFallThrough.FrameMaskValidatedEntryECX == 0x18
            && calledFallThrough.FrameMaskPlan?.FreshFrameMinus4 == 7
            && calledFallThrough.FrameMaskPlan?.StopAtRawFileOffsetExclusive == 0x012C8EF8
            && NoUnreachedCaptures(calledFallThrough));

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan missingCell =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 3,
                descriptorFreshFrameMinus4: 0x10,
                descriptorFreshFrameMinus10: 0x20,
                entryEbx: 0x20,
                freshFrameMinus14: null,
                freshFrameMinus4: null,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("A missing call-cell target stops at EB8 and retains the descriptor store",
            missingCell.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingCallCellTarget
            && missingCell.CallCellReadReached
            && !missingCell.CallCellTargetCaptured
            && !missingCell.OpaqueCallReached
            && missingCell.DescriptorPlan.DescriptorAddressWriteReached
            && missingCell.DescriptorPlan.WrittenFrameMinus10
                == CapacityFinalCalleeDescriptorReloadProjection.DeriveDescriptorAddress(3)
            && missingCell.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan missingStatus =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x10,
                descriptorFreshFrameMinus10: 0x20,
                entryEbx: 0x20,
                callCellTarget: 0,
                callReturnedNormally: null,
                freshFrameMinus14: null,
                freshFrameMinus4: null,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("A present zero target with missing return status is distinct and stops at EB8",
            missingStatus.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingCallReturnStatus
            && missingStatus.CallCellTargetCaptured
            && missingStatus.CallCellTargetVirtualAddress == 0
            && missingStatus.OpaqueCallReached
            && missingStatus.CallReturnedNormally is null
            && missingStatus.CallOutputsOpaque
            && missingStatus.UnreachedCallReturnStatusProvided == false
            && missingStatus.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan nonreturning =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x10,
                descriptorFreshFrameMinus10: 0x20,
                entryEbx: 0x20,
                callCellTarget: 0,
                callReturnedNormally: false,
                freshFrameMinus14: null,
                freshFrameMinus4: null,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("A false return status is a captured nonreturning call, not a missing status",
            nonreturning.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .CallDidNotReturnNormally
            && nonreturning.CallReturnedNormally == false
            && nonreturning.OpaqueCallReached
            && !nonreturning.FrameMaskUpdateReached
            && nonreturning.DescriptorPlan.DescriptorAddressWriteReached
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan missingPostEBP =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x10,
                descriptorFreshFrameMinus10: 0x20,
                entryEbx: 0x20,
                callCellTarget: 0x1000,
                callReturnedNormally: true,
                postCallEBP: null,
                postCallECX: 0,
                freshFrameMinus14: null,
                freshFrameMinus4: null,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("The normal return requires independently captured post-call EBP and ECX",
            missingPostEBP.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.MissingPostCallEBP
            && missingPostEBP.FrameMaskUpdateReached
            && !missingPostEBP.PostCallEBPCaptured
            && missingPostEBP.PostCallECXCaptured
            && missingPostEBP.FrameMaskValidatedEntryEBP is null
            && missingPostEBP.FrameMaskValidatedEntryECX == 0
            && missingPostEBP.FrameMaskPlan?.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryEBP
            && missingPostEBP.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan callOnlyOnBypass =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0x20,
                descriptorFreshFrameMinus4: 4,
                descriptorFreshFrameMinus10: 4,
                callCellTarget: 0,
                callReturnedNormally: false,
                postCallEBP: 0x2000,
                postCallECX: 1,
                freshFrameMinus14: 1,
                freshFrameMinus4: 1,
                freshFrameMinus8: 0x80000001,
                freshGlobalValue: 0xFFFFFFFF));
        check("Call-only captures on a direct equality bypass stop before the frame-mask window",
            callOnlyOnBypass.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedCallRoute
            && callOnlyOnBypass.DescriptorEqualityDestinationReached
            && !callOnlyOnBypass.OpaqueCallReached
            && callOnlyOnBypass.UnreachedCallCellTargetCaptureProvided
            && callOnlyOnBypass.UnreachedCallReturnStatusProvided
            && callOnlyOnBypass.UnreachedPostCallEBPCaptureProvided
            && callOnlyOnBypass.UnreachedPostCallECXCaptureProvided
            && !callOnlyOnBypass.FrameMaskUpdateReached
            && callOnlyOnBypass.FrameMaskPlan is null
            && callOnlyOnBypass.UnreachedFreshFrameMinus14CaptureProvided
            && callOnlyOnBypass.UnreachedFreshFrameMinus4CaptureProvided
            && callOnlyOnBypass.UnreachedFreshFrameMinus8CaptureProvided
            && callOnlyOnBypass.UnreachedFreshGlobalValueCaptureProvided
            && callOnlyOnBypass.DescriptorPlan.DescriptorAddressWriteReached
            && callOnlyOnBypass.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan contaminatedDescriptor =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 3,
                descriptorFreshFrameMinus4: 3,
                descriptorFreshFrameMinus10: 3,
                freshEaxPlusC: 0x20,
                freshFrameMinus14: 0,
                freshFrameMinus4: 0,
                freshFrameMinus8: 0,
                freshGlobalValue: 0));
        check("Unreached nested descriptor captures prevent the equality route from succeeding",
            contaminatedDescriptor.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedDescriptorRoute
            && contaminatedDescriptor.DescriptorPlan.DescriptorAddressWriteReached
            && contaminatedDescriptor.DescriptorPlan.UnreachedFreshEaxPlusCCaptureProvided
            && !contaminatedDescriptor.FrameMaskUpdateReached
            && contaminatedDescriptor.UnreachedFreshFrameMinus14CaptureProvided
            && contaminatedDescriptor.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan independentFreshReads =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0x20,
                descriptorFreshFrameMinus4: 0x1111,
                descriptorFreshFrameMinus10: 0x1111,
                freshFrameMinus14: 0x2222,
                freshFrameMinus4: 0x3333,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("A clean direct-route mismatch is bounded at ED5 without fabricating an unused ECX capture",
            independentFreshReads.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.FrameMismatch
            && independentFreshReads.DescriptorPlan.FreshFrameMinus4 == 0x1111
            && independentFreshReads.FrameMaskPlan?.FreshFrameMinus4 == 0x3333
            && independentFreshReads.FrameMaskPlan?.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch
            && independentFreshReads.FrameMaskValidatedEntryECX is null
            && independentFreshReads.FrameMaskPlan?.UnreachedEntryECXCaptureProvided == false
            && !independentFreshReads.UnreachedFreshFrameMinus8CaptureProvided
            && !independentFreshReads.UnreachedFreshGlobalValueCaptureProvided
            && independentFreshReads.FrameMaskPlan?.SelectedBranchDestinationRawFileOffset
                == 0x012C8ED5
            && independentFreshReads.StopAtRawFileOffsetExclusive == 0x012C8ED5);

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan calledMismatch =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 0x10,
                descriptorFreshFrameMinus10: 0x20,
                entryEbx: 0x20,
                callCellTarget: 0x1000,
                callReturnedNormally: true,
                postCallEBP: 0x2000,
                postCallECX: 0x1F,
                freshFrameMinus14: 1,
                freshFrameMinus4: 2,
                freshFrameMinus8: null,
                freshGlobalValue: null));
        check("An independently supplied post-call ECX remains flagged as unreached on mismatch",
            calledMismatch.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedFrameMaskRoute
            && calledMismatch.FrameMaskPlan?.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch
            && calledMismatch.FrameMaskPlan?.UnreachedEntryECXCaptureProvided == true
            && calledMismatch.StopAtRawFileOffsetExclusive == 0x012C8ED5);

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan wrappedAddresses =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                entryEbp: 8,
                initialFrameMinus10: 0xFFFFFFFF,
                descriptorFreshFrameMinus4: 0,
                descriptorFreshFrameMinus10: 0,
                freshFrameMinus14: 7,
                freshFrameMinus4: 7,
                freshFrameMinus8: 0x80000001,
                freshGlobalValue: 0));
        check("Entry-derived frame addresses wrap and captured zero remains a valid value",
            wrappedAddresses.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.GlobalValueUpdated
            && wrappedAddresses.FrameMaskPlan?.FreshFrameMinus14Address == 0xFFFFFFF4
            && wrappedAddresses.FrameMaskValidatedEntryECX == 0xFFFFFFFF
            && wrappedAddresses.FrameMaskPlan?.ShiftCount == 31
            && wrappedAddresses.FrameMaskPlan?.ShiftedValue == 0x80000000
            && wrappedAddresses.FrameMaskPlan?.GlobalValueCaptured == true
            && wrappedAddresses.FrameMaskPlan?.GlobalValueBefore == 0
            && wrappedAddresses.FrameMaskPlan?.GlobalValueAfter == 0
            && wrappedAddresses.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan maskTailContamination =
            CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection.Project(Capture(
                initialFrameMinus10: 0,
                descriptorFreshFrameMinus4: 4,
                descriptorFreshFrameMinus10: 4,
                freshFrameMinus14: 1,
                freshFrameMinus4: 2,
                freshFrameMinus8: 3,
                freshGlobalValue: 0xFFFFFFFF));
        check("Captures after the ED5 mismatch retain the bounded mismatch but cannot be admitted",
            maskTailContamination.Outcome
                == CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedFrameMaskRoute
            && maskTailContamination.FrameMaskPlan?.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch
            && maskTailContamination.FrameMaskPlan?.SelectedBranchDestinationRawFileOffset
                == 0x012C8ED5
            && !maskTailContamination.FrameMaskPlan.Value.GlobalWriteReached
            && maskTailContamination.UnreachedFreshFrameMinus8CaptureProvided
            && maskTailContamination.UnreachedFreshGlobalValueCaptureProvided
            && maskTailContamination.StopAtRawFileOffsetExclusive == 0x012C8ED5);
    }

    private static bool NoUnreachedCaptures(
        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan plan) =>
        !plan.DescriptorPlan.UnreachedInitialFrameMinus10CaptureProvided
        && !plan.DescriptorPlan.UnreachedFreshFrameMinus4CaptureProvided
        && !plan.DescriptorPlan.UnreachedFreshFrameMinus10CaptureProvided
        && !plan.DescriptorPlan.UnreachedFreshEaxPlusCCaptureProvided
        && !plan.UnreachedCallCellTargetCaptureProvided
        && !plan.UnreachedCallReturnStatusProvided
        && !plan.UnreachedPostCallEBPCaptureProvided
        && !plan.UnreachedPostCallECXCaptureProvided
        && !plan.UnreachedFreshFrameMinus14CaptureProvided
        && !plan.UnreachedFreshFrameMinus4CaptureProvided
        && !plan.UnreachedFreshFrameMinus8CaptureProvided
        && !plan.UnreachedFreshGlobalValueCaptureProvided
        && plan.FrameMaskPlan?.UnreachedEntryECXCaptureProvided != true;

    private static CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEbp = 0x1000,
        uint entryEbx = 0x100,
        uint entryEsi = 0x200,
        uint? initialFrameMinus10 = 0x20,
        uint? descriptorFreshFrameMinus4 = 0x11111111,
        uint? descriptorFreshFrameMinus10 = 0x11111111,
        uint? freshEaxPlusC = null,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null,
        uint? postCallEBP = null,
        uint? postCallECX = null,
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
            callCellTarget,
            callReturnedNormally,
            postCallEBP,
            postCallECX,
            freshFrameMinus14,
            freshFrameMinus4,
            freshFrameMinus8,
            freshGlobalValue);

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
