using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeTagEqualityLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeTagEqualityLifecyclePlan equalityCapacityExit =
            ProjectEquality(
                initialGlobalDword: 0xFFFFFFF0,
                freshTagDword: 7,
                capacityDword: 0x1010,
                callReturnedNormally: null);
        CapacityFinalCalleeTagEqualityContinuationPlan capacityContinuation =
            equalityCapacityExit.Continuation!.Value;
        check("Only the clean equality boundary joins, with ESI, aligned EDI, and entry ESP",
            equalityCapacityExit.AlignmentTag.Admitted
            && equalityCapacityExit.AlignmentTag.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome
                    .TagEntryEdxEqualsGlobalDwordBoundary
            && equalityCapacityExit.AlignmentTag.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeTagEqualityLifecycleProjection
                    .EqualityBoundaryRawFileOffsetExclusive
            && equalityCapacityExit.ContinuationReached
            && equalityCapacityExit.ContinuationESI == 7
            && equalityCapacityExit.ContinuationEDI == 0x1000
            && equalityCapacityExit.ContinuationESP == 0x20000
            && capacityContinuation.Capture.EntryESI == 7
            && capacityContinuation.Capture.EntryEDI == 0x1000
            && capacityContinuation.Capture.EntryESP == 0x20000
            && equalityCapacityExit.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome.CapacityExitAt9441
            && equalityCapacityExit.Admitted
            && equalityCapacityExit.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeTagEqualityLifecyclePlan independentGlobalReads =
            ProjectEquality(
                initialGlobalDword: 0x12345678,
                freshTagDword: 0x87654321,
                capacityDword: 0x2000,
                callReturnedNormally: true);
        CapacityFinalCalleeTagEqualityContinuationPlan independentContinuation =
            independentGlobalReads.Continuation!.Value;
        check("Continuation reloads 1EDADD4 and 1EDADDC independently of earlier tag reads and writes",
            independentContinuation.Reads[0].Address == 0x01EDADD4
            && independentContinuation.Reads[0].Value == 0x12345678
            && independentContinuation.Reads[1].Address == 0x01EDADDC
            && independentContinuation.Reads[1].Value == 0x87654321
            && independentContinuation.Capture.InitialGlobalDword == 0x12345678
            && independentContinuation.Capture.FreshTagDword == 0x87654321
            && independentContinuation.Writes.Any(write =>
                write.Address == 0x01EDADD4 && write.Value == 0x12346678)
            && independentContinuation.Writes.All(write =>
                write.Address != 0x01EDADDC)
            && independentGlobalReads.Admitted);

        CapacityFinalCalleeTagEqualityLifecyclePlan missingInitialGlobal =
            ProjectEquality(
                initialGlobalDword: null,
                freshTagDword: 0,
                capacityDword: 1,
                callReturnedNormally: true);
        CapacityFinalCalleeTagEqualityLifecyclePlan missingFreshGlobal =
            ProjectEquality(
                initialGlobalDword: 0x10,
                freshTagDword: null,
                capacityDword: 0,
                callReturnedNormally: true);
        CapacityFinalCalleeTagEqualityContinuationPlan missingFreshContinuation =
            missingFreshGlobal.Continuation!.Value;
        check("Missing continuation globals never reuse earlier values and missing reads preserve prior writes",
            missingInitialGlobal.AlignmentTag.AlignmentDispatch.Reads.IsEmpty
            && missingInitialGlobal.Continuation!.Value.Reads.Length == 1
            && missingInitialGlobal.Continuation.Value.Reads[0].Address == 0x01EDADD4
            && !missingInitialGlobal.Continuation.Value.Reads[0].ValueCaptured
            && missingInitialGlobal.Continuation.Value.Writes.IsEmpty
            && missingInitialGlobal.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome.ContinuationIncomplete
            && missingFreshContinuation.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.MissingFreshTagDword
            && missingFreshContinuation.Reads.Length == 2
            && missingFreshContinuation.Reads[^1].Address == 0x01EDADDC
            && !missingFreshContinuation.Reads[^1].ValueCaptured
            && missingFreshContinuation.Writes.Length == 3
            && missingFreshContinuation.Writes[0].Address == 0x01EDADE0
            && missingFreshContinuation.Writes[1].Address == 0x01EDADD4
            && missingFreshContinuation.Writes[2].Address == 0x0B
            && missingFreshGlobal.UnreachedCapacityDwordCaptureProvided
            && missingFreshGlobal.UnreachedCallReturnStatusProvided
            && !missingFreshGlobal.Admitted);

        CapacityFinalCalleeTagEqualityLifecyclePlan wrappedEquality =
            ProjectEquality(
                entryEax: 0xFFFFFFF9,
                entryEsi: 7,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: uint.MaxValue,
                initialGlobalDword: 0x10,
                freshTagDword: 7,
                capacityDword: 8,
                callReturnedNormally: null);
        CapacityFinalCalleeTagEqualityContinuationPlan wrappedContinuation =
            wrappedEquality.Continuation!.Value;
        check("Alignment and continuation DWORD arithmetic wrap without changing register provenance",
            wrappedEquality.AlignmentTag.TagEntryEDX == uint.MaxValue
            && wrappedEquality.ContinuationEDI == 0xFFFFFFF8
            && wrappedEquality.ContinuationESI == 7
            && wrappedContinuation.ComputedEcx == 8
            && wrappedContinuation.EAX == 9
            && wrappedContinuation.Writes.Any(write =>
                write.Address == 0x0B && write.Value == 9)
            && wrappedEquality.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome.CapacityExitAt9441
            && wrappedEquality.Admitted);

        CapacityFinalCalleeTagEqualityLifecyclePlan directCallNormal =
            ProjectEquality(
                initialGlobalDword: 1,
                freshTagDword: 8,
                capacityDword: 0,
                callReturnedNormally: true);
        CapacityFinalCalleeTagEqualityLifecyclePlan directCallMissing =
            ProjectEquality(
                initialGlobalDword: 1,
                freshTagDword: 8,
                capacityDword: 0,
                callReturnedNormally: null);
        CapacityFinalCalleeTagEqualityLifecyclePlan directCallNonreturn =
            ProjectEquality(
                initialGlobalDword: 1,
                freshTagDword: 8,
                capacityDword: 0,
                callReturnedNormally: false);
        check("The exact capacity exit and normal direct return are distinct; missing and nonreturn stop at the call",
            directCallNormal.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .DirectCallReturnedNormallyAtF71
            && directCallNormal.Continuation!.Value.CallReached
            && directCallNormal.Continuation.Value.CallReturnedNormally == true
            && directCallNormal.Continuation.Value.StopAtRawFileOffsetExclusive
                == 0x012C8F71
            && directCallNormal.Admitted
            && directCallMissing.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .DirectCallReturnStatusMissing
            && directCallMissing.Continuation!.Value.CallReached
            && directCallMissing.Continuation.Value.StopAtRawFileOffsetExclusive
                == 0x012C8F6C
            && !directCallMissing.Admitted
            && directCallNonreturn.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .DirectCallDidNotReturnNormally
            && directCallNonreturn.Continuation!.Value.CallReturnedNormally == false
            && directCallNonreturn.Continuation.Value.StopAtRawFileOffsetExclusive
                == 0x012C8F6C
            && !directCallNonreturn.Admitted);

        CapacityFinalCalleeTagEqualityLifecyclePlan[] cleanNonEqualityRoutes =
        {
            ProjectEquality(
                entryEax: 0xFFFFFFF9,
                entryEsi: 0x10,
                tagEdxPlusFourDword: null,
                tagGlobalDword: null,
                initialGlobalDword: null,
                freshTagDword: null,
                capacityDword: null,
                callReturnedNormally: null),
            ProjectEquality(
                tagEdxPlusFourDword: 0,
                tagGlobalDword: null,
                initialGlobalDword: null,
                freshTagDword: null,
                capacityDword: null,
                callReturnedNormally: null),
            ProjectEquality(
                tagEdxPlusFourDword: 3,
                tagGlobalDword: null,
                initialGlobalDword: null,
                freshTagDword: null,
                capacityDword: null,
                callReturnedNormally: null),
            ProjectEquality(
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 0,
                initialGlobalDword: null,
                freshTagDword: null,
                capacityDword: null,
                callReturnedNormally: null)
        };
        check("Clean non-equality tag destinations remain admitted bounded routes without starting the continuation",
            cleanNonEqualityRoutes[0].AlignmentTag.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome
                    .TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit
            && cleanNonEqualityRoutes[1].AlignmentTag.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagClBitZeroClearExit
            && cleanNonEqualityRoutes[2].AlignmentTag.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagClBitOneSetExit
            && cleanNonEqualityRoutes[3].AlignmentTag.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome
                    .TagEntryEdxNotEqualGlobalDwordExit
            && cleanNonEqualityRoutes.All(plan =>
                plan.AlignmentTag.Admitted
                && plan.Outcome
                    == CapacityFinalCalleeTagEqualityLifecycleOutcome
                        .TagRouteCompletedWithoutEqualityBoundary
                && plan.Continuation is null
                && !plan.ContinuationReached
                && !plan.UnreachedInitialGlobalDwordCaptureProvided
                && !plan.UnreachedFreshTagDwordCaptureProvided
                && !plan.UnreachedCapacityDwordCaptureProvided
                && !plan.UnreachedCallReturnStatusProvided
                && plan.Admitted));

        CapacityFinalCalleeTagEqualityLifecyclePlan contaminatedNonEqualityRoute =
            ProjectEquality(
                tagEdxPlusFourDword: 0,
                tagGlobalDword: null,
                initialGlobalDword: 1,
                freshTagDword: null,
                capacityDword: 2,
                callReturnedNormally: true);

        CapacityFinalCalleeTagEqualityLifecyclePlan contaminatedEquality =
            ProjectEquality(
                capturedTagEntryEdx: 0x1004,
                initialGlobalDword: 1,
                freshTagDword: 2,
                capacityDword: 3,
                callReturnedNormally: true);
        CapacityFinalCalleeTagEqualityLifecyclePlan unselectedAlignment =
            ProjectEquality(
                entryEax: 0x1000,
                entryEsiDword: 0,
                globalDispatchDword: 0,
                callCellTargetVirtualAddress: 0,
                dispatchCallReturnedNormally: true,
                initialGlobalDword: 1,
                freshTagDword: 2,
                capacityDword: 3,
                callReturnedNormally: true);
        check("Contaminated or unselected tag plans never enter the continuation and preserve wrapper flags",
            contaminatedEquality.AlignmentTag.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagValidationIncomplete
            && !contaminatedEquality.AlignmentTag.Admitted
            && contaminatedEquality.Continuation is null
            && contaminatedEquality.UnreachedInitialGlobalDwordCaptureProvided
            && contaminatedEquality.UnreachedFreshTagDwordCaptureProvided
            && contaminatedEquality.UnreachedCapacityDwordCaptureProvided
            && contaminatedEquality.UnreachedCallReturnStatusProvided
            && unselectedAlignment.AlignmentTag.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.AlignmentRouteNotSelected
            && unselectedAlignment.Continuation is null
            && unselectedAlignment.UnreachedInitialGlobalDwordCaptureProvided
            && unselectedAlignment.UnreachedFreshTagDwordCaptureProvided
            && unselectedAlignment.UnreachedCapacityDwordCaptureProvided
            && unselectedAlignment.UnreachedCallReturnStatusProvided
            && contaminatedNonEqualityRoute.AlignmentTag.Admitted
            && contaminatedNonEqualityRoute.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .NonEqualityRouteContaminatedByContinuationCaptures
            && contaminatedNonEqualityRoute.Continuation is null
            && contaminatedNonEqualityRoute.UnreachedInitialGlobalDwordCaptureProvided
            && !contaminatedNonEqualityRoute.UnreachedFreshTagDwordCaptureProvided
            && contaminatedNonEqualityRoute.UnreachedCapacityDwordCaptureProvided
            && contaminatedNonEqualityRoute.UnreachedCallReturnStatusProvided
            && !contaminatedNonEqualityRoute.Admitted
            && !contaminatedEquality.Admitted
            && !unselectedAlignment.Admitted);
    }

    private static CapacityFinalCalleeTagEqualityLifecyclePlan ProjectEquality(
        uint entryEax = 0x1001,
        uint entryEsi = 7,
        uint entryEsp = 0x20000,
        uint? entryEsiDword = null,
        uint? globalDispatchDword = null,
        uint? callCellTargetVirtualAddress = null,
        bool? dispatchCallReturnedNormally = null,
        uint? tagEdxPlusFourDword = 1,
        uint? tagGlobalDword = 0x1007,
        uint? capturedTagEntryEdx = null,
        uint? initialGlobalDword = 0,
        uint? freshTagDword = 0,
        uint? capacityDword = 0,
        bool? callReturnedNormally = false)
    {
        var alignmentDispatch = new CapacityFinalCalleeAlignmentDispatchCapture(
            CaptureValidatedEntry: true,
            EntryEAX: entryEax,
            EntryESI: entryEsi,
            EntryECX: 0,
            EntryEBP: 0x10000,
            EntryESP: entryEsp,
            EntryEsiDword: entryEsiDword,
            GlobalDispatchDword: globalDispatchDword,
            CallCellTargetVirtualAddress: callCellTargetVirtualAddress,
            CallReturnedNormally: dispatchCallReturnedNormally);
        var alignmentTag = new CapacityFinalCalleeAlignmentTagLifecycleCapture(
            alignmentDispatch,
            CapturedTagEntryEDX: capturedTagEntryEdx,
            TagEdxPlusFourDword: tagEdxPlusFourDword,
            TagGlobalDword: tagGlobalDword);
        return CapacityFinalCalleeTagEqualityLifecycleProjection.Project(
            new(
                alignmentTag,
                initialGlobalDword,
                freshTagDword,
                capacityDword,
                callReturnedNormally));
    }
}
