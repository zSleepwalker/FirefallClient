using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeGuardedFinalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The guarded successor keeps both 908B roots and their exact exits",
            CapacityFinalCalleeGuardedFinalLifecycleProjection.GuardedEntryRawFileOffset
                == 0x012C9058
            && CapacityFinalCalleeGuardedFinalLifecycleProjection.SuccessorEntryRawFileOffset
                == 0x012C906E
            && CapacityFinalCalleeGuardedFinalLifecycleProjection.SecondFieldEntryRawFileOffset
                == 0x012C9080
            && CapacityFinalCalleeGuardedFinalLifecycleProjection.FinalTailEntryRawFileOffset
                == 0x012C908B
            && CapacityFinalCalleeGuardedFinalLifecycleProjection.ZeroDestinationRawFileOffset
                == 0x012C8EF2
            && CapacityFinalCalleeGuardedFinalLifecycleProjection
                .GuardedValidationDestinationRawFileOffset == 0x012C8E76
            && CapacityFinalCalleeGuardedFinalLifecycleProjection
                .SuccessorValidationDestinationRawFileOffset == 0x012C8E76
            && CapacityFinalCalleeGuardedFinalLifecycleProjection
                .FinalValidationDestinationRawFileOffset == 0x012C8E76
            && CapacityFinalCalleeGuardedFinalLifecycleProjection.FinalJumpDestinationRawFileOffset
                == 0x012C8EF2);

        CapacityFinalCalleeGuardedFinalLifecyclePlan successorRoute = Project(
            guardedEax: 0x100,
            guardedGlobal: 0x100,
            guardedEcx: 0xCAFE,
            guardedEsi: 0x300,
            guardedFresh10: 0x200,
            successorGlobal: 0x100,
            successorFresh14: 0x200,
            finalGlobal: 0x100);
        check("The clean 906E route takes final ECX only from its selected fresh +14 read",
            successorRoute.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .FinalPairedStoresAndJumpToTarget
            && successorRoute.GuardedSuccessorLifecyclePlan.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .SuccessorFreshEsiPlus14NonzeroToBoundary
            && successorRoute.GuardedSuccessorLifecyclePlan.GuardedFieldStore.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && successorRoute.GuardedSuccessorLifecyclePlan.Successor?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && successorRoute.FinalEntryECXSource
                == CapacityFinalCalleeGuardedFinalLifecycleECXSource.SuccessorFreshEsiPlus14
            && successorRoute.DerivedFinalEntryECX == 0x200
            && successorRoute.DerivedFinalEntryECX
                == successorRoute.GuardedSuccessorLifecyclePlan.Successor?.FreshEsiPlus14
            && successorRoute.DerivedFinalEntryECX
                != successorRoute.GuardedSuccessorLifecyclePlan.GuardedFieldStore.Capture.EntryECX
            && successorRoute.FinalPairedTailPlan?.Capture.EntryECX == 0x200
            && successorRoute.FinalPairedTailPlan?.Capture.EntryEAX == 0x100
            && successorRoute.FinalPairedTailPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && successorRoute.FinalPairedTailPlan?.Reads[0] is
            {
                RawFileOffset: 0x012C908B,
                Address: 0x01EDADD8,
                ValueCaptured: true,
                Value: 0x100
            }
            && successorRoute.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9064u, 0x118u, 0xCAFEu),
                    (0x012C907Au, 0x110u, 0x200u),
                    (0x012C907Du, 0x218u, 0x100u),
                    (0x012C9097u, 0x114u, 0x200u),
                    (0x012C909Au, 0x218u, 0x100u)
                })
            && successorRoute.FinalPairedTailPlan?.FinalJumpSelectedDestinationRawFileOffset
                == 0x012C8EF2
            && successorRoute.ExactCleanExitAdmitted
            && successorRoute.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeGuardedFinalLifecyclePlan secondFieldRoute = Project(
            guardedEax: 0x300,
            guardedGlobal: 0x300,
            guardedEcx: 0x400,
            guardedEsi: 0x100,
            guardedFresh10: 0,
            successorGlobal: null,
            successorFresh14: null,
            secondFieldFresh: 0x200,
            finalGlobal: 0x100);
        check("The clean 9080 route uses its independent fresh ECX and retains prior writes",
            secondFieldRoute.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .FinalPairedStoresAndJumpToTarget
            && secondFieldRoute.GuardedSuccessorLifecyclePlan.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .SecondFieldFreshECXNonzeroToBoundary
            && secondFieldRoute.GuardedSuccessorLifecyclePlan.Successor is null
            && secondFieldRoute.GuardedSuccessorLifecyclePlan.GuardedFieldStore.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && secondFieldRoute.GuardedSuccessorLifecyclePlan.GuardedFieldStore.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && secondFieldRoute.GuardedSuccessorLifecyclePlan.SecondFieldTest?.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && secondFieldRoute.FinalEntryECXSource
                == CapacityFinalCalleeGuardedFinalLifecycleECXSource
                    .IndependentSecondFieldFreshECX
            && secondFieldRoute.DerivedFinalEntryECX == 0x200
            && secondFieldRoute.DerivedFinalEntryECX
                == secondFieldRoute.GuardedSuccessorLifecyclePlan.SecondFieldTest?.FreshECX
            && secondFieldRoute.FinalPairedTailPlan?.Capture.EntryEAX == 0x300
            && secondFieldRoute.FinalPairedTailPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && secondFieldRoute.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9064u, 0x318u, 0x400u),
                    (0x012C9097u, 0x314u, 0x200u),
                    (0x012C909Au, 0x218u, 0x300u)
                })
            && secondFieldRoute.ExactCleanExitAdmitted
            && secondFieldRoute.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeGuardedFinalLifecyclePlan successorZero = Project(
            guardedFresh10: 0x200,
            successorFresh14: 0,
            finalGlobal: 0x100);
        CapacityFinalCalleeGuardedFinalLifecyclePlan secondFieldZero = Project(
            guardedFresh10: 0,
            successorGlobal: null,
            successorFresh14: null,
            secondFieldFresh: 0,
            finalGlobal: 0x100);
        check("Both zero tests bypass 908B and reject supplied final captures before the final child",
            successorZero.GuardedSuccessorLifecyclePlan.Successor?.TestFreshEcxFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && successorZero.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome.SuccessorZeroContaminated
            && successorZero.GuardedSuccessorLifecyclePlan.StopAtRawFileOffsetExclusive
                == 0x012C8EF2
            && !successorZero.FinalPairedTailReached
            && successorZero.FinalPairedTailPlan is null
            && successorZero.UnreachedFreshGlobalForFinalCompareCaptureProvided
            && !successorZero.ExactCleanExitAdmitted
            && secondFieldZero.GuardedSuccessorLifecyclePlan.SecondFieldTest?.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && secondFieldZero.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome.SecondFieldZeroContaminated
            && secondFieldZero.GuardedSuccessorLifecyclePlan.StopAtRawFileOffsetExclusive
                == 0x012C8EF2
            && !secondFieldZero.FinalPairedTailReached
            && secondFieldZero.FinalPairedTailPlan is null
            && secondFieldZero.UnreachedFreshGlobalForFinalCompareCaptureProvided
            && !secondFieldZero.ExactCleanExitAdmitted);

        CapacityFinalCalleeGuardedFinalLifecyclePlan cleanSuccessorZero = Project(
            guardedFresh10: 0x200,
            successorFresh14: 0,
            finalGlobal: null);
        CapacityFinalCalleeGuardedFinalLifecyclePlan cleanSecondFieldZero = Project(
            guardedFresh10: 0,
            successorGlobal: null,
            successorFresh14: null,
            secondFieldFresh: 0,
            finalGlobal: null);
        check("Zero exits remain clean when the final compare capture is absent",
            cleanSuccessorZero.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome.SuccessorZeroToTarget
            && cleanSuccessorZero.ExactCleanExitAdmitted
            && !cleanSuccessorZero.UnreachedFreshGlobalForFinalCompareCaptureProvided
            && cleanSecondFieldZero.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome.SecondFieldZeroToTarget
            && cleanSecondFieldZero.ExactCleanExitAdmitted
            && !cleanSecondFieldZero.UnreachedFreshGlobalForFinalCompareCaptureProvided);

        CapacityFinalCalleeGuardedFinalLifecyclePlan missingSuccessorGlobal = Project(
            guardedFresh10: 0x200,
            successorGlobal: null,
            successorFresh14: 0x100,
            finalGlobal: 0x100);
        CapacityFinalCalleeGuardedFinalLifecyclePlan missingSecondFieldECX = Project(
            guardedFresh10: 0,
            successorGlobal: null,
            successorFresh14: null,
            secondFieldFresh: null,
            finalGlobal: 0x100);
        check("Missing 906E and 9080 reads stay distinct from zero and block final-tail entry",
            missingSuccessorGlobal.GuardedSuccessorLifecyclePlan.Successor is
            {
                Outcome: CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingFreshGlobalValue,
                CompareGlobalReadReached: true,
                CompareGlobalValueCaptured: false,
                StopAtRawFileOffsetExclusive: 0x012C906E,
                Writes.IsEmpty: true
            }
            && missingSuccessorGlobal.GuardedSuccessorLifecyclePlan.StopAtRawFileOffsetExclusive
                == 0x012C906E
            && missingSuccessorGlobal.Writes.Length == 1
            && !missingSuccessorGlobal.FinalPairedTailReached
            && missingSuccessorGlobal.FinalPairedTailPlan is null
            && missingSuccessorGlobal.UnreachedFreshGlobalForFinalCompareCaptureProvided
            && missingSecondFieldECX.GuardedSuccessorLifecyclePlan.SecondFieldTest is
            {
                FreshECXReadReached: true,
                FreshECXCaptured: false,
                FreshECX: null,
                TestECXReached: false,
                ZeroBranchReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9080
            }
            && missingSecondFieldECX.GuardedSuccessorLifecyclePlan.StopAtRawFileOffsetExclusive
                == 0x012C9080
            && !missingSecondFieldECX.FinalPairedTailReached
            && missingSecondFieldECX.FinalPairedTailPlan is null
            && missingSecondFieldECX.UnreachedFreshGlobalForFinalCompareCaptureProvided);

        CapacityFinalCalleeGuardedFinalLifecyclePlan missingFinalGlobal = Project(
            guardedFresh10: 0x200,
            successorFresh14: 0x200,
            finalGlobal: null);
        check("A missing final global is a reached read at 908B with earlier writes preserved",
            missingFinalGlobal.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .MissingFreshGlobalForFinalCompare
            && missingFinalGlobal.FinalPairedTailReached
            && missingFinalGlobal.FinalEntryECXSource
                == CapacityFinalCalleeGuardedFinalLifecycleECXSource.SuccessorFreshEsiPlus14
            && missingFinalGlobal.FinalPairedTailPlan?.Reads.Length == 1
            && missingFinalGlobal.FinalPairedTailPlan?.Reads[0] is
            {
                RawFileOffset: 0x012C908B,
                ValueCaptured: false,
                Value: null
            }
            && missingFinalGlobal.Writes.Length == 3
            && missingFinalGlobal.StopAtRawFileOffsetExclusive == 0x012C908B);

        CapacityFinalCalleeGuardedFinalLifecyclePlan finalBelow = Project(
            guardedFresh10: 0,
            successorGlobal: null,
            successorFresh14: null,
            secondFieldFresh: 1,
            finalGlobal: 2);
        check("Final JB stops at validation without forwarding retained EAX",
            finalBelow.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .FinalEntryECXUnsignedBelowGlobalToValidation
            && finalBelow.FinalPairedTailPlan?.CompareResult == 0xFFFFFFFF
            && finalBelow.FinalPairedTailPlan?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && finalBelow.FinalPairedTailPlan?.GuardBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && finalBelow.FinalPairedTailPlan?.Capture.EntryEAX is null
            && !finalBelow.FinalPairedTailPlan!.EntryEAXCaptured
            && finalBelow.Writes.Length == 1
            && finalBelow.ExactCleanExitAdmitted
            && finalBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeGuardedFinalLifecyclePlan retainedSecondFieldEAX =
            CapacityFinalCalleeGuardedFinalLifecycleProjection.Project(new(
                new(
                    new CapacityFinalCalleeGuardedFieldStoreCapture(
                        EntryEAX: 0x100,
                        FreshGlobalForCompare: 0x100,
                        EntryECX: 0x200,
                        EntryESI: 0x300,
                        FreshEsiPlus10: 0),
                    SuccessorFreshGlobalValue: null,
                    SuccessorFreshEsiPlus14: null,
                    SecondFieldFreshECX: 0x200),
                FreshGlobalForFinalCompare: 0x100));
        check("The selected 9080 route forwards retained EAX only after its final guard passes",
            retainedSecondFieldEAX.GuardedSuccessorLifecyclePlan.GuardedFieldStore.Capture.EntryEAX
                == 0x100
            && retainedSecondFieldEAX.GuardedSuccessorLifecyclePlan.SecondFieldTest is not null
            && retainedSecondFieldEAX.FinalPairedTailReached
            && retainedSecondFieldEAX.FinalPairedTailPlan?.Capture.EntryEAX == 0x100
            && retainedSecondFieldEAX.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .FinalPairedStoresAndJumpToTarget);

        CapacityFinalCalleeGuardedFinalLifecyclePlan contaminatedUnselectedCapture = Project(
            guardedFresh10: 0x200,
            successorFresh14: 0x200,
            secondFieldFresh: 0,
            finalGlobal: 0x100);
        CapacityFinalCalleeGuardedFinalLifecyclePlan missingGuardRoot = Project(
            guardedGlobal: null,
            guardedFresh10: 0,
            secondFieldFresh: 0x200,
            finalGlobal: 0x100);
        check("Unselected child data and missing root inputs block before downstream children",
            contaminatedUnselectedCapture.GuardedSuccessorLifecyclePlan.Outcome
                == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .GuardedFieldStoreContaminated
            && contaminatedUnselectedCapture.GuardedSuccessorLifecyclePlan.Successor is null
            && contaminatedUnselectedCapture.GuardedSuccessorLifecyclePlan.SecondFieldTest is null
            && contaminatedUnselectedCapture.GuardedSuccessorLifecyclePlan
                .UnselectedSecondFieldFreshECXCaptureProvided
            && !contaminatedUnselectedCapture.FinalPairedTailReached
            && contaminatedUnselectedCapture.FinalPairedTailPlan is null
            && missingGuardRoot.GuardedSuccessorLifecyclePlan.GuardedFieldStore.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome.MissingFreshGlobalForCompare
            && missingGuardRoot.GuardedSuccessorLifecyclePlan.SecondFieldTest is null
            && !missingGuardRoot.FinalPairedTailReached
            && missingGuardRoot.FinalPairedTailPlan is null
            && missingGuardRoot.StopAtRawFileOffsetExclusive == 0x012C9058);

        CapacityFinalCalleeGuardedFinalLifecyclePlan wrapped = Project(
            guardedEax: 0xFFFFFFEC,
            guardedGlobal: 0xFFFFFFEC,
            guardedEcx: 0x1234,
            guardedEsi: 0xFFFFFFF0,
            guardedFresh10: 0x200,
            successorGlobal: 0x100,
            successorFresh14: 0xFFFFFFE8,
            finalGlobal: 0xFFFFFFE8);
        check("Wrapped aliasing stores retain the full ordered write history across both tails",
            wrapped.Outcome
                == CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .FinalPairedStoresAndJumpToTarget
            && wrapped.FinalPairedTailPlan?.FirstStoreAddress == 0
            && wrapped.FinalPairedTailPlan?.FirstStoreValue == 0xFFFFFFE8
            && wrapped.FinalPairedTailPlan?.SecondStoreAddress == 0
            && wrapped.FinalPairedTailPlan?.SecondStoreValue == 0xFFFFFFEC
            && wrapped.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9064u, 4u, 0x1234u),
                    (0x012C907Au, 0xFFFFFFFCu, 0x200u),
                    (0x012C907Du, 0x218u, 0xFFFFFFECu),
                    (0x012C9097u, 0u, 0xFFFFFFE8u),
                    (0x012C909Au, 0u, 0xFFFFFFECu)
                })
            && wrapped.ExactCleanExitAdmitted
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C8EF2);
    }

    private static CapacityFinalCalleeGuardedFinalLifecyclePlan Project(
        uint? guardedEax = 0x100,
        uint? guardedGlobal = 0x100,
        uint? guardedEcx = 0xCAFE,
        uint? guardedEsi = 0x300,
        uint? guardedFresh10 = 0x200,
        uint? successorGlobal = 0x100,
        uint? successorFresh14 = 0x200,
        uint? secondFieldFresh = null,
        uint? finalGlobal = 0x100) =>
        CapacityFinalCalleeGuardedFinalLifecycleProjection.Project(new(
            new(
                new CapacityFinalCalleeGuardedFieldStoreCapture(
                    guardedEax,
                    guardedGlobal,
                    guardedEcx,
                    guardedEsi,
                    guardedFresh10),
                successorGlobal,
                successorFresh14,
                secondFieldFresh),
            finalGlobal));
}
