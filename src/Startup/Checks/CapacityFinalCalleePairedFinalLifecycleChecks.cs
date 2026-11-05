using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleePairedFinalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The lifecycle joins only the bounded 908B successor edge to the final 908B tail",
            CapacityFinalCalleePairedFinalLifecycleProjection.SuccessorEntryRawFileOffset
                == 0x012C906E
            && CapacityFinalCalleePairedFinalLifecycleProjection.FinalTailEntryRawFileOffset
                == 0x012C908B
            && CapacityFinalCalleePairedFinalLifecycleProjection.SuccessorZeroDestinationRawFileOffset
                == 0x012C8EF2
            && CapacityFinalCalleePairedFinalLifecycleProjection.SuccessorUnsignedBelowDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleePairedFinalLifecycleProjection.FinalUnsignedBelowDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleePairedFinalLifecycleProjection.FinalPairJumpDestinationRawFileOffset
                == 0x012C8EF2);

        CapacityFinalCalleePairedFinalLifecyclePlan paired = Project(
            successorEntryECX: 0x200,
            successorFreshGlobal: 0x100,
            successorEntryEAX: 0xFFFFFFEC,
            successorEntryESI: 0x204,
            successorFreshEsiPlus14: 0xFFFFFFE8,
            finalFreshGlobal: 0xFFFFFFE8);
        check("Fresh successor ECX and retained successor EAX drive ordered wrapping final stores",
            paired.Outcome
                == CapacityFinalCalleePairedFinalLifecycleOutcome.FinalPairedStoresAndJumpToTarget
            && paired.SuccessorPairedStoresPlan.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.FreshEsiPlus14NonzeroToBoundary
            && paired.SuccessorPairedStoresPlan.CompareResult == 0x100
            && paired.SuccessorPairedStoresPlan.Capture.EntryECX == 0x200
            && paired.SuccessorPairedStoresPlan.FreshEsiPlus14Address == 0x218
            && paired.SuccessorPairedStoresPlan.SecondStoreAddress == 0x218
            && paired.SuccessorPairedStoresPlan.Reads[0] is
            {
                RawFileOffset: 0x012C906E,
                ValueCaptured: true,
                Value: 0x100
            }
            && paired.SuccessorPairedStoresPlan.Reads[1] is
            {
                RawFileOffset: 0x012C9080,
                Address: 0x218,
                ValueCaptured: true,
                Value: 0xFFFFFFE8
            }
            && paired.SuccessorPairedStoresPlan.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && paired.SuccessorPairedStoresPlan.TestFreshEcxFlags is
            {
                Carry: false,
                Parity: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && paired.SuccessorPairedStoresPlan.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C908B
            && paired.FinalPairedTailReached
            && paired.DerivedFinalEntryECX == 0xFFFFFFE8
            && paired.DerivedFinalEntryEAX == 0xFFFFFFEC
            && paired.FinalPairedTailPlan?.Capture.EntryECX
                != paired.SuccessorPairedStoresPlan.Capture.EntryECX
            && paired.FinalPairedTailPlan?.Capture.EntryECX == 0xFFFFFFE8
            && paired.FinalPairedTailPlan?.Capture.EntryEAX == 0xFFFFFFEC
            && paired.FinalPairedTailPlan?.Reads[0] is
            {
                RawFileOffset: 0x012C908B,
                ValueCaptured: true,
                Value: 0xFFFFFFE8
            }
            && paired.FinalPairedTailPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && paired.FinalPairedTailPlan?.FirstStoreAddress == 0
            && paired.FinalPairedTailPlan?.FirstStoreValue == 0xFFFFFFE8
            && paired.FinalPairedTailPlan?.SecondStoreAddress == 0
            && paired.FinalPairedTailPlan?.SecondStoreValue == 0xFFFFFFEC
            && paired.FinalPairedTailPlan?.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9097u, 0u, 0xFFFFFFE8u),
                    (0x012C909Au, 0u, 0xFFFFFFECu)
                }) == true
            && paired.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C907Au, 0xFFFFFFFCu, 0x200u),
                    (0x012C907Du, 0x218u, 0xFFFFFFECu),
                    (0x012C9097u, 0u, 0xFFFFFFE8u),
                    (0x012C909Au, 0u, 0xFFFFFFECu)
                })
            && paired.FinalPairedTailPlan?.FinalJumpSelectedDestinationRawFileOffset
                == 0x012C8EF2
            && paired.ExactCleanExitAdmitted
            && paired.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleePairedFinalLifecyclePlan changedFinalGlobal = Project(
            successorEntryECX: 0x200,
            successorFreshGlobal: 0x100,
            successorEntryEAX: 0x500,
            successorEntryESI: 0x100,
            successorFreshEsiPlus14: 0x200,
            finalFreshGlobal: 0x300);
        check("A distinct fresh final global can take the below edge without deriving unused EAX",
            changedFinalGlobal.SuccessorPairedStoresPlan.CompareResult == 0x100
            && changedFinalGlobal.SuccessorPairedStoresPlan.GuardBranchTaken == false
            && changedFinalGlobal.FinalPairedTailPlan?.Reads.Length == 1
            && changedFinalGlobal.FinalPairedTailPlan?.Reads[0] is
            {
                RawFileOffset: 0x012C908B,
                Address: 0x01EDADD8,
                ValueCaptured: true,
                Value: 0x300
            }
            && changedFinalGlobal.FinalPairedTailPlan?.Capture.EntryECX == 0x200
            && changedFinalGlobal.FinalPairedTailPlan?.Capture.FreshGlobalForCompare == 0x300
            && changedFinalGlobal.FinalPairedTailPlan?.Capture.EntryEAX is null
            && changedFinalGlobal.FinalPairedTailPlan?.CompareResult == 0xFFFFFF00
            && changedFinalGlobal.FinalPairedTailPlan?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && changedFinalGlobal.FinalPairedTailPlan?.GuardBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && changedFinalGlobal.DerivedFinalEntryEAX is null
            && !changedFinalGlobal.FinalPairedTailPlan!.EntryEAXCaptured
            && changedFinalGlobal.FinalPairedTailPlan.UnreachedEntryEAXCaptureProvided == false
            && changedFinalGlobal.ExactCleanExitAdmitted
            && changedFinalGlobal.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleePairedFinalLifecyclePlan missingFinalGlobal = Project(
            successorEntryECX: 0x200,
            successorFreshGlobal: 0x100,
            successorEntryEAX: 0x300,
            successorEntryESI: 0x400,
            successorFreshEsiPlus14: 0x200,
            finalFreshGlobal: null);
        check("A missing final global stops at its fresh compare read with no derived final EAX",
            missingFinalGlobal.FinalPairedTailReached
            && missingFinalGlobal.FinalPairedTailPlan?.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.MissingFreshGlobalForCompare
            && missingFinalGlobal.FinalPairedTailPlan?.Capture.EntryECX == 0x200
            && missingFinalGlobal.FinalPairedTailPlan?.Capture.EntryEAX is null
            && missingFinalGlobal.FinalPairedTailPlan?.CompareGlobalReadReached == true
            && missingFinalGlobal.FinalPairedTailPlan?.Reads.Length == 1
            && missingFinalGlobal.FinalPairedTailPlan?.Reads[0] is
            {
                RawFileOffset: 0x012C908B,
                Address: 0x01EDADD8,
                ValueCaptured: false,
                Value: null
            }
            && missingFinalGlobal.DerivedFinalEntryEAX is null
            && missingFinalGlobal.Writes.Length == 2
            && !missingFinalGlobal.ExactCleanExitAdmitted
            && missingFinalGlobal.StopAtRawFileOffsetExclusive == 0x012C908B);

        CapacityFinalCalleePairedFinalLifecyclePlan successorZero = Project(
            successorEntryECX: 0,
            successorFreshGlobal: 0,
            successorEntryEAX: 0x100,
            successorEntryESI: 0x200,
            successorFreshEsiPlus14: 0,
            finalFreshGlobal: null);
        CapacityFinalCalleePairedFinalLifecyclePlan contaminatedSuccessorZero = Project(
            successorEntryECX: 0,
            successorFreshGlobal: 0,
            successorEntryEAX: 0x100,
            successorEntryESI: 0x200,
            successorFreshEsiPlus14: 0,
            finalFreshGlobal: 0);
        check("Successor zero remains bounded at EF2 and rejects any later final-global capture",
            successorZero.SuccessorPairedStoresPlan.TestFreshEcxFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && successorZero.Outcome
                == CapacityFinalCalleePairedFinalLifecycleOutcome.SuccessorZeroExitToTarget
            && successorZero.SuccessorPairedStoresPlan.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C8EF2
            && !successorZero.FinalPairedTailReached
            && successorZero.FinalPairedTailPlan is null
            && successorZero.ExactCleanExitAdmitted
            && successorZero.StopAtRawFileOffsetExclusive == 0x012C8EF2
            && contaminatedSuccessorZero.Outcome
                == CapacityFinalCalleePairedFinalLifecycleOutcome.SuccessorZeroExitContaminated
            && contaminatedSuccessorZero.UnreachedFreshGlobalForFinalCompareCaptureProvided
            && !contaminatedSuccessorZero.FinalPairedTailReached
            && contaminatedSuccessorZero.FinalPairedTailPlan is null
            && !contaminatedSuccessorZero.ExactCleanExitAdmitted
            && contaminatedSuccessorZero.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleePairedFinalLifecyclePlan successorBelow = Project(
            successorEntryECX: 1,
            successorFreshGlobal: 2,
            successorEntryEAX: null,
            successorEntryESI: null,
            successorFreshEsiPlus14: null,
            finalFreshGlobal: null);
        CapacityFinalCalleePairedFinalLifecyclePlan contaminatedSuccessorBelow = Project(
            successorEntryECX: 1,
            successorFreshGlobal: 2,
            successorEntryEAX: null,
            successorEntryESI: null,
            successorFreshEsiPlus14: null,
            finalFreshGlobal: 0);
        check("Successor unsigned-below remains bounded at E76 and rejects downstream final-global data",
            successorBelow.SuccessorPairedStoresPlan.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && successorBelow.Outcome
                == CapacityFinalCalleePairedFinalLifecycleOutcome
                    .SuccessorUnsignedBelowToValidation
            && successorBelow.SuccessorPairedStoresPlan.GuardBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && !successorBelow.FinalPairedTailReached
            && successorBelow.FinalPairedTailPlan is null
            && successorBelow.ExactCleanExitAdmitted
            && successorBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && contaminatedSuccessorBelow.Outcome
                == CapacityFinalCalleePairedFinalLifecycleOutcome
                    .SuccessorUnsignedBelowContaminated
            && contaminatedSuccessorBelow.UnreachedFreshGlobalForFinalCompareCaptureProvided
            && !contaminatedSuccessorBelow.FinalPairedTailReached
            && !contaminatedSuccessorBelow.ExactCleanExitAdmitted
            && contaminatedSuccessorBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleePairedFinalLifecyclePlan missingSuccessorGlobal = Project(
            successorEntryECX: 0x100,
            successorFreshGlobal: null,
            successorEntryEAX: 0x200,
            successorEntryESI: 0x300,
            successorFreshEsiPlus14: 0x400,
            finalFreshGlobal: 0x500);
        CapacityFinalCalleePairedFinalLifecyclePlan missingSuccessorEsi = Project(
            successorEntryECX: 0x200,
            successorFreshGlobal: 0x100,
            successorEntryEAX: 0x300,
            successorEntryESI: null,
            successorFreshEsiPlus14: 0x400,
            finalFreshGlobal: 0x500);
        check("Missing successor gates block the child while retaining only already-reached writes",
            missingSuccessorGlobal.Outcome
                == CapacityFinalCalleePairedFinalLifecycleOutcome.SuccessorPairedStoresBlocked
            && missingSuccessorGlobal.SuccessorPairedStoresPlan.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingFreshGlobalValue
            && missingSuccessorGlobal.SuccessorPairedStoresPlan.Writes.IsEmpty
            && !missingSuccessorGlobal.FinalPairedTailReached
            && missingSuccessorGlobal.UnreachedFreshGlobalForFinalCompareCaptureProvided
            && missingSuccessorGlobal.StopAtRawFileOffsetExclusive == 0x012C906E
            && missingSuccessorEsi.SuccessorPairedStoresPlan.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingEntryESIForFreshRead
            && missingSuccessorEsi.SuccessorPairedStoresPlan.Writes.Length == 2
            && missingSuccessorEsi.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C907Au, 0x310u, 0x200u),
                    (0x012C907Du, 0x218u, 0x300u)
                })
            && !missingSuccessorEsi.FinalPairedTailReached
            && missingSuccessorEsi.UnreachedFreshGlobalForFinalCompareCaptureProvided
            && missingSuccessorEsi.StopAtRawFileOffsetExclusive == 0x012C9080);
    }

    private static CapacityFinalCalleePairedFinalLifecyclePlan Project(
        uint? successorEntryECX = 0x200,
        uint? successorFreshGlobal = 0x100,
        uint? successorEntryEAX = 0x300,
        uint? successorEntryESI = 0x400,
        uint? successorFreshEsiPlus14 = 0x200,
        uint? finalFreshGlobal = 0x200) =>
        CapacityFinalCalleePairedFinalLifecycleProjection.Project(new(
            new CapacityFinalCalleeSuccessorPairedStoresCapture(
                successorEntryECX,
                successorFreshGlobal,
                successorEntryEAX,
                successorEntryESI,
                successorFreshEsiPlus14),
            finalFreshGlobal));
}
