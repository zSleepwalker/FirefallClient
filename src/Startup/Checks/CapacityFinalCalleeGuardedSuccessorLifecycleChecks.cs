using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeGuardedSuccessorLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The lifecycle boundaries compose the committed 9058, 906E, and 9080 routes",
            CapacityFinalCalleeGuardedSuccessorLifecycleProjection.EntryRawFileOffset
                == 0x012C9058
            && CapacityFinalCalleeGuardedSuccessorLifecycleProjection.SuccessorEntryRawFileOffset
                == 0x012C906E
            && CapacityFinalCalleeGuardedSuccessorLifecycleProjection.SecondFieldEntryRawFileOffset
                == 0x012C9080
            && CapacityFinalCalleeGuardedSuccessorLifecycleProjection
                .GuardedValidationDestinationRawFileOffset == 0x012C8E76
            && CapacityFinalCalleeGuardedSuccessorLifecycleProjection
                .SuccessorValidationDestinationRawFileOffset == 0x012C8E76
            && CapacityFinalCalleeGuardedSuccessorLifecycleProjection
                .ZeroDestinationRawFileOffset == 0x012C8EF2
            && CapacityFinalCalleeGuardedSuccessorLifecycleProjection
                .BoundaryRawFileOffsetExclusive == 0x012C908B
            && CapacityFinalCalleeGuardedFieldStoreProjection.BoundaryRawFileOffsetExclusive
                == CapacityFinalCalleeGuardedSuccessorLifecycleProjection.SuccessorEntryRawFileOffset
            && CapacityFinalCalleeGuardedFieldStoreProjection.ZeroDestinationRawFileOffset
                == CapacityFinalCalleeGuardedSuccessorLifecycleProjection.SecondFieldEntryRawFileOffset
            && CapacityFinalCalleeSecondFieldTestProjection.BoundaryRawFileOffsetExclusive
                == CapacityFinalCalleeGuardedSuccessorLifecycleProjection.BoundaryRawFileOffsetExclusive
            && CapacityFinalCalleeSuccessorPairedStoresProjection.BoundaryRawFileOffsetExclusive
                == CapacityFinalCalleeGuardedSuccessorLifecycleProjection.BoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan guardedBelow = Project(
            guardedEax: 1,
            guardedGlobal: 2,
            guardedEcx: null,
            guardedEsi: null,
            guardedFresh10: null);
        check("A clean guarded unsigned-below path stays bounded at 8E76 without child plans",
            guardedBelow.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .GuardedUnsignedBelowToValidation
            && guardedBelow.GuardedFieldStore.BranchTaken == true
            && guardedBelow.GuardedFieldStore.BranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && guardedBelow.GuardedFieldStore.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && guardedBelow.GuardedFieldStore.ExactCleanExitAdmitted
            && guardedBelow.Successor is null
            && guardedBelow.SecondFieldTest is null
            && guardedBelow.Admitted
            && guardedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan guardedBelowContaminated =
            Project(
                guardedEax: 1,
                guardedGlobal: 2,
                successorGlobal: 0,
                secondFieldFresh: 0);
        check("Captures belonging to either bypassed child contaminate the bounded guard exit",
            guardedBelowContaminated.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .GuardedFieldStoreContaminated
            && guardedBelowContaminated.Successor is null
            && guardedBelowContaminated.SecondFieldTest is null
            && guardedBelowContaminated.UnselectedSuccessorFreshGlobalValueCaptureProvided
            && !guardedBelowContaminated.UnselectedSuccessorFreshEsiPlus14CaptureProvided
            && guardedBelowContaminated.UnselectedSecondFieldFreshECXCaptureProvided
            && !guardedBelowContaminated.Admitted
            && guardedBelowContaminated.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan guardedZero =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 0,
                secondFieldFresh: 0);
        check("A clean zero guarded ESI+10 reaches the independent 9080 child",
            guardedZero.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .SecondFieldFreshECXZeroToTarget
            && guardedZero.GuardedFieldStore.StoreReached
            && guardedZero.GuardedFieldStore.StoreAddress == 0x118
            && guardedZero.GuardedFieldStore.BranchTaken == false
            && guardedZero.GuardedFieldStore.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && guardedZero.GuardedFieldStore.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C9080
            && !guardedZero.UnselectedSuccessorFreshGlobalValueCaptureProvided
            && !guardedZero.UnselectedSuccessorFreshEsiPlus14CaptureProvided
            && !guardedZero.UnselectedSecondFieldFreshECXCaptureProvided
            && guardedZero.Successor is null
            && guardedZero.SecondFieldTest is
            {
                FreshEsiPlus14Address: 0x314,
                FreshECX: 0,
                ZeroBranchTaken: true,
                ZeroBranchSelectedDestinationRawFileOffset: 0x012C8EF2,
                ExactCleanExitAdmitted: true
            }
            && guardedZero.Admitted
            && guardedZero.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan secondFieldNonzero =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 0,
                secondFieldFresh: 0x80000001);
        check("The independent 9080 nonzero read selects the exact shared 908B boundary",
            secondFieldNonzero.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .SecondFieldFreshECXNonzeroToBoundary
            && secondFieldNonzero.SecondFieldTest?.TestECXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && secondFieldNonzero.SecondFieldTest.ZeroBranchTaken == false
            && secondFieldNonzero.SecondFieldTest.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C908B
            && secondFieldNonzero.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan missingSecondField =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 0,
                secondFieldFresh: null);
        check("Missing 9080 data stays distinct from zero and leaves the child branch unreached",
            missingSecondField.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome.SecondFieldTestIncomplete
            && missingSecondField.SecondFieldTest is
            {
                FreshECXReadReached: true,
                FreshECXCaptured: false,
                FreshECX: null,
                TestECXReached: false,
                ZeroBranchReached: false,
                ExactCleanExitAdmitted: false,
                StopAtRawFileOffsetExclusive: 0x012C9080
            }
            && !missingSecondField.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan zeroRouteContaminated =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 0,
                successorGlobal: 0x100,
                successorFresh14: 0);
        check("Successor captures are rejected when guarded zero bypasses 906E",
            zeroRouteContaminated.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .GuardedFieldStoreContaminated
            && zeroRouteContaminated.GuardedFieldStore.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C9080
            && zeroRouteContaminated.GuardedFieldStore.Writes.Length == 1
            && zeroRouteContaminated.Successor is null
            && zeroRouteContaminated.SecondFieldTest is null
            && zeroRouteContaminated.UnselectedSuccessorFreshGlobalValueCaptureProvided
            && zeroRouteContaminated.UnselectedSuccessorFreshEsiPlus14CaptureProvided
            && !zeroRouteContaminated.UnselectedSecondFieldFreshECXCaptureProvided
            && zeroRouteContaminated.StopAtRawFileOffsetExclusive == 0x012C9080
            && !zeroRouteContaminated.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan successorZero =
            Project(
                guardedEax: 0x01EDADC8,
                guardedGlobal: 0x01EDADC8,
                guardedEcx: 0xCAFED00D,
                guardedEsi: 0xFFFFFFEC,
                guardedFresh10: 0x01EDADE0,
                successorGlobal: 0x01EDADD8,
                successorFresh14: 0);
        check("The guarded fresh value supplies successor ECX, while inherited EAX and ESI feed stores and the fresh read",
            successorZero.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .SuccessorFreshEsiPlus14ZeroToTarget
            && successorZero.DerivedSuccessorEntryECX == 0x01EDADE0
            && successorZero.DerivedSuccessorEntryEAX == 0x01EDADC8
            && successorZero.DerivedSuccessorEntryESI == 0xFFFFFFEC
            && successorZero.Successor is
            {
                Capture.EntryECX: 0x01EDADE0,
                Capture.EntryEAX: 0x01EDADC8,
                Capture.EntryESI: 0xFFFFFFEC,
                FirstStoreAddress: 0x01EDADD8,
                FirstStoreValue: 0x01EDADE0,
                SecondStoreAddress: 0x01EDADF8,
                SecondStoreValue: 0x01EDADC8,
                FreshEsiPlus14Address: 0,
                FreshEsiPlus14: 0,
                ZeroBranchSelectedDestinationRawFileOffset: 0x012C8EF2,
                ExactCleanExitAdmitted: true
            }
            && successorZero.GuardedFieldStore.Writes.Length == 1
            && successorZero.GuardedFieldStore.Writes[0].Address == 0x01EDADE0
            && successorZero.Successor.Writes.Length == 2
            && successorZero.Successor.Reads[0] is
            {
                Address: 0x01EDADD8,
                ValueCaptured: true,
                Value: 0x01EDADD8
            }
            && successorZero.Successor.FirstStoreAddress
                == CapacityFinalCalleeSuccessorPairedStoresProjection.GlobalValueVirtualAddress
            && successorZero.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan successorFromFresh =
            Project(
                guardedEax: 0xFFFFFFF0,
                guardedGlobal: 0xFFFFFFF0,
                guardedEcx: 0xCAFED00D,
                guardedEsi: 0xFFFFFFEC,
                guardedFresh10: 0x200,
                successorGlobal: 0x100,
                successorFresh14: 0);
        check("The successor uses the fresh +10 value, not stored ECX, and preserves ordered writes across an aliasing read",
            successorFromFresh.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .SuccessorFreshEsiPlus14ZeroToTarget
            && successorFromFresh.DerivedSuccessorEntryECX == 0x200
            && successorFromFresh.DerivedSuccessorEntryECX
                != successorFromFresh.GuardedFieldStore.StoreValue
            && successorFromFresh.GuardedFieldStore.StoreAddress == 8
            && successorFromFresh.Successor is
            {
                FirstStoreAddress: 0,
                FirstStoreValue: 0x200,
                SecondStoreAddress: 0x218,
                SecondStoreValue: 0xFFFFFFF0,
                FreshEsiPlus14Address: 0,
                FreshEsiPlus14: 0,
                ExactCleanExitAdmitted: true
            }
            && successorFromFresh.Successor.Writes.Length == 2
            && successorFromFresh.Successor.Writes[0].RawFileOffset == 0x012C907A
            && successorFromFresh.Successor.Writes[1].RawFileOffset == 0x012C907D
            && successorFromFresh.Successor.FirstStoreAddress
                == successorFromFresh.Successor.FreshEsiPlus14Address
            && successorFromFresh.Successor.Reads[1] is
            {
                RawFileOffset: 0x012C9080,
                Address: 0,
                ValueCaptured: true,
                Value: 0
            }
            && successorFromFresh.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan successorBelow =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 1,
                successorGlobal: 2);
        check("A successor unsigned-below exit does not derive unused EAX or ESI captures",
            successorBelow.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .SuccessorUnsignedBelowToValidation
            && successorBelow.Successor is
            {
                Capture.EntryECX: 1,
                Capture.EntryEAX: null,
                Capture.EntryESI: null,
                EntryECXUnsignedBelowGlobal: true,
                GuardBranchSelectedDestinationRawFileOffset: 0x012C8E76,
                Writes.IsEmpty: true,
                FreshEsiPlus14ReadReached: false,
                ExactCleanExitAdmitted: true
            }
            && successorBelow.DerivedSuccessorEntryECX == 1
            && successorBelow.DerivedSuccessorEntryEAX is null
            && successorBelow.DerivedSuccessorEntryESI is null
            && successorBelow.Admitted
            && successorBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan successorBelowContaminated =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 1,
                successorGlobal: 2,
                successorFresh14: 0);
        check("A downstream fresh +14 capture is rejected when successor JB bypasses its read",
            successorBelowContaminated.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome.SuccessorContaminated
            && successorBelowContaminated.Successor?.UnreachedFreshEsiPlus14CaptureProvided == true
            && successorBelowContaminated.Successor?.Capture.EntryEAX is null
            && successorBelowContaminated.Successor?.Capture.EntryESI is null
            && !successorBelowContaminated.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan missingSuccessorGlobalClean =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 1,
                successorGlobal: null);
        check("Missing successor global remains an incomplete CMP read without manufacturing later registers",
            missingSuccessorGlobalClean.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome.SuccessorIncomplete
            && missingSuccessorGlobalClean.Successor is
            {
                Outcome: CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingFreshGlobalValue,
                Capture.EntryECX: 1,
                Capture.EntryEAX: null,
                Capture.EntryESI: null,
                CompareGlobalReadReached: true,
                CompareGlobalValueCaptured: false,
                UnreachedFreshGlobalValueCaptureProvided: false,
                UnreachedEntryEAXCaptureProvided: false,
                UnreachedEntryESICaptureProvided: false,
                UnreachedFreshEsiPlus14CaptureProvided: false,
                Writes.IsEmpty: true,
                StopAtRawFileOffsetExclusive: 0x012C906E
            }
            && missingSuccessorGlobalClean.DerivedSuccessorEntryECX == 1
            && missingSuccessorGlobalClean.DerivedSuccessorEntryEAX is null
            && missingSuccessorGlobalClean.DerivedSuccessorEntryESI is null
            && !missingSuccessorGlobalClean.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan missingSuccessorGlobal =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 1,
                successorGlobal: null,
                successorFresh14: 0);
        check("A missing fresh successor global blocks later register derivation and marks bypassed data",
            missingSuccessorGlobal.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome.SuccessorContaminated
            && missingSuccessorGlobal.Successor is
            {
                Outcome: CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingFreshGlobalValue,
                Capture.EntryEAX: null,
                Capture.EntryESI: null,
                CompareGlobalReadReached: true,
                CompareGlobalValueCaptured: false,
                UnreachedFreshEsiPlus14CaptureProvided: true,
                Writes.IsEmpty: true
            }
            && missingSuccessorGlobal.DerivedSuccessorEntryECX == 1
            && missingSuccessorGlobal.DerivedSuccessorEntryEAX is null
            && missingSuccessorGlobal.DerivedSuccessorEntryESI is null
            && !missingSuccessorGlobal.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan missingSuccessorFresh =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 0x200,
                successorGlobal: 0x100,
                successorFresh14: null);
        check("A missing successor +14 capture preserves both earlier writes and stops at its load",
            missingSuccessorFresh.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome.SuccessorIncomplete
            && missingSuccessorFresh.Successor is
            {
                FirstStoreReached: true,
                SecondStoreReached: true,
                FreshEsiPlus14ReadReached: true,
                FreshEsiPlus14Captured: false,
                FreshEsiPlus14Address: 0x314,
                Writes.Length: 2,
                TestFreshEcxReached: false,
                ExactCleanExitAdmitted: false,
                StopAtRawFileOffsetExclusive: 0x012C9080
            }
            && !missingSuccessorFresh.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan successorNonzero =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 0x200,
                successorGlobal: 0x100,
                successorFresh14: 0x80000000);
        check("A nonzero successor read falls through to its exact 908B boundary with TEST flags",
            successorNonzero.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .SuccessorFreshEsiPlus14NonzeroToBoundary
            && successorNonzero.Successor is
            {
                TestFreshEcxFlags:
                {
                    Carry: false,
                    Parity: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                },
                ZeroBranchSelectedDestinationRawFileOffset: 0x012C908B
            }
            && successorNonzero.Admitted
            && successorNonzero.StopAtRawFileOffsetExclusive == 0x012C908B);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan upstreamContaminated =
            Project(
                guardedEax: null,
                guardedGlobal: 0x100,
                guardedEcx: 0x55,
                guardedEsi: 0x300,
                guardedFresh10: 0,
                successorGlobal: 0x100);
        check("An unreached guarded entry capture prevents either successor join",
            upstreamContaminated.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .GuardedFieldStoreContaminated
            && upstreamContaminated.GuardedFieldStore.UnreachedFreshGlobalForCompareCaptureProvided
            && upstreamContaminated.GuardedFieldStore.UnreachedEntryECXCaptureProvided
            && upstreamContaminated.GuardedFieldStore.UnreachedEntryESICaptureProvided
            && upstreamContaminated.GuardedFieldStore.UnreachedFreshEsiPlus10CaptureProvided
            && upstreamContaminated.Successor is null
            && upstreamContaminated.SecondFieldTest is null
            && !upstreamContaminated.Admitted);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan successorBypassContaminated =
            Project(
                guardedEax: 0x100,
                guardedGlobal: 0x100,
                guardedFresh10: 0x200,
                successorGlobal: 0x100,
                secondFieldFresh: 0);
        check("The 9080-only capture is rejected when the guarded nonzero route enters 906E",
            successorBypassContaminated.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .GuardedFieldStoreContaminated
            && successorBypassContaminated.Successor is null
            && successorBypassContaminated.SecondFieldTest is null
            && successorBypassContaminated.GuardedFieldStore.Writes.Length == 1
            && successorBypassContaminated.UnselectedSecondFieldFreshECXCaptureProvided
            && !successorBypassContaminated.UnselectedSuccessorFreshGlobalValueCaptureProvided
            && !successorBypassContaminated.UnselectedSuccessorFreshEsiPlus14CaptureProvided
            && !successorBypassContaminated.Admitted
            && successorBypassContaminated.StopAtRawFileOffsetExclusive == 0x012C906E);
    }

    private static CapacityFinalCalleeGuardedSuccessorLifecyclePlan Project(
        uint? guardedEax = 0x100,
        uint? guardedGlobal = 0x100,
        uint? guardedEcx = 0xCAFEBABE,
        uint? guardedEsi = 0x300,
        uint? guardedFresh10 = 0x40,
        uint? successorGlobal = null,
        uint? successorFresh14 = null,
        uint? secondFieldFresh = null) =>
        CapacityFinalCalleeGuardedSuccessorLifecycleProjection.Project(new(
            new(
                guardedEax,
                guardedGlobal,
                guardedEcx,
                guardedEsi,
                guardedFresh10),
            successorGlobal,
            successorFresh14,
            secondFieldFresh));
}
