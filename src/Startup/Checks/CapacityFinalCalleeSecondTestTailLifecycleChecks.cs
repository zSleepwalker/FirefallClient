using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSecondTestTailLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The clean nonzero second-field test joins only at 908B and retains both destinations",
            CapacityFinalCalleeSecondTestTailLifecycleProjection.SecondFieldTestEntryRawFileOffset
                == 0x012C9080
            && CapacityFinalCalleeSecondTestTailLifecycleProjection.FinalTailEntryRawFileOffset
                == 0x012C908B
            && CapacityFinalCalleeSecondTestTailLifecycleProjection.ZeroBypassDestinationRawFileOffset
                == 0x012C8EF2
            && CapacityFinalCalleeSecondTestTailLifecycleProjection.UnsignedBelowDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeSecondTestTailLifecycleProjection.FinalPairJumpDestinationRawFileOffset
                == 0x012C8EF2);

        CapacityFinalCalleeSecondTestTailLifecyclePlan missingPriorEntry =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: null,
                freshECX: 0,
                freshGlobalForCompare: 1,
                callerLiveInEAX: 2));
        check("A missing prior ESI read is preserved and blocks the final-tail join",
            missingPriorEntry.Outcome
                == CapacityFinalCalleeSecondTestTailLifecycleOutcome.SecondFieldTestBlocked
            && missingPriorEntry.SecondFieldTestPlan.Outcome
                == CapacityFinalCalleeSecondFieldTestOutcome.MissingEntryESIForRead
            && missingPriorEntry.SecondFieldTestPlan.Reads.Length == 1
            && missingPriorEntry.SecondFieldTestPlan.Reads[0] is
            {
                RawFileOffset: 0x012C9080,
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && missingPriorEntry.SecondFieldTestPlan.UnreachedFreshECXCaptureProvided
            && !missingPriorEntry.FinalPairedTailReached
            && missingPriorEntry.FinalPairedTailPlan is null
            && missingPriorEntry.UnreachedFreshGlobalForCompareCaptureProvided
            && missingPriorEntry.UnreachedCallerLiveInEAXCaptureProvided
            && !missingPriorEntry.ExactCleanExitAdmitted
            && missingPriorEntry.StopAtRawFileOffsetExclusive == 0x012C9080);

        CapacityFinalCalleeSecondTestTailLifecyclePlan missingPriorECX =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: 0x100,
                freshECX: null,
                freshGlobalForCompare: 0,
                callerLiveInEAX: 0));
        check("A missing fresh ECX keeps the reached prior read but cannot be replaced by later captures",
            missingPriorECX.SecondFieldTestPlan.Outcome
                == CapacityFinalCalleeSecondFieldTestOutcome.MissingFreshECX
            && missingPriorECX.SecondFieldTestPlan.Reads.Length == 1
            && missingPriorECX.SecondFieldTestPlan.Reads[0] is
            {
                Address: 0x114,
                ValueCaptured: false,
                Value: null
            }
            && !missingPriorECX.FinalPairedTailReached
            && missingPriorECX.FinalPairedTailPlan is null
            && missingPriorECX.UnreachedFreshGlobalForCompareCaptureProvided
            && missingPriorECX.UnreachedCallerLiveInEAXCaptureProvided
            && !missingPriorECX.ExactCleanExitAdmitted
            && missingPriorECX.StopAtRawFileOffsetExclusive == 0x012C9080);

        CapacityFinalCalleeSecondTestTailLifecyclePlan zero =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: 0xFFFFFFEC,
                freshECX: 0,
                freshGlobalForCompare: null,
                callerLiveInEAX: null));
        check("Fresh zero bypasses the final tail at 8EF2 while retaining TEST flags and the wrapped read",
            zero.Outcome
                == CapacityFinalCalleeSecondTestTailLifecycleOutcome.ZeroBypassToTarget
            && zero.SecondFieldTestPlan.FreshEsiPlus14Address == 0
            && zero.SecondFieldTestPlan.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.SecondFieldTestPlan.ZeroBranchTaken == true
            && zero.SecondFieldTestPlan.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C8EF2
            && !zero.FinalPairedTailReached
            && zero.FinalPairedTailPlan is null
            && zero.ExactCleanExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeSecondTestTailLifecyclePlan contaminatedZero =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: 0x100,
                freshECX: 0,
                freshGlobalForCompare: 0,
                callerLiveInEAX: 0));
        check("Later captures contaminate the zero bypass and cannot enter the final-tail reads",
            contaminatedZero.Outcome
                == CapacityFinalCalleeSecondTestTailLifecycleOutcome.ZeroBypassContaminated
            && contaminatedZero.SecondFieldTestPlan.Reads.Length == 1
            && contaminatedZero.SecondFieldTestPlan.Reads[0].Value == 0
            && contaminatedZero.UnreachedFreshGlobalForCompareCaptureProvided
            && contaminatedZero.UnreachedCallerLiveInEAXCaptureProvided
            && !contaminatedZero.FinalPairedTailReached
            && contaminatedZero.FinalPairedTailPlan is null
            && !contaminatedZero.ExactCleanExitAdmitted
            && contaminatedZero.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeSecondTestTailLifecyclePlan missingGlobal =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: 0x100,
                freshECX: 0x200,
                freshGlobalForCompare: null,
                callerLiveInEAX: 0x300));
        check("Nonzero fresh ECX becomes final ECX, the global is independently read, and EAX remains unreached",
            missingGlobal.Outcome
                == CapacityFinalCalleeSecondTestTailLifecycleOutcome
                    .MissingFreshGlobalForCompare
            && missingGlobal.SecondFieldTestPlan.ZeroBranchTaken == false
            && missingGlobal.SecondFieldTestPlan.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C908B
            && missingGlobal.FinalPairedTailReached
            && missingGlobal.DerivedFinalEntryECX == 0x200
            && missingGlobal.FinalPairedTailPlan?.Capture.EntryECX == 0x200
            && missingGlobal.FinalPairedTailPlan?.CompareGlobalReadReached == true
            && missingGlobal.FinalPairedTailPlan?.Reads.Length == 1
            && missingGlobal.FinalPairedTailPlan?.Reads[0] is
            {
                RawFileOffset: 0x012C908B,
                Kind: CapacityFinalCalleeFinalPairedTailReadKind.FreshGlobalForCompare,
                Address: 0x01EDADD8,
                ValueCaptured: false,
                Value: null
            }
            && missingGlobal.UnreachedCallerLiveInEAXCaptureProvided
            && !missingGlobal.ExactCleanExitAdmitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C908B);

        CapacityFinalCalleeSecondTestTailLifecyclePlan unsignedBelow =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: 0x100,
                freshECX: 1,
                freshGlobalForCompare: 0xF0000000,
                callerLiveInEAX: null));
        check("The fresh global unsigned-below gate branches to 8E76 before requiring EAX",
            unsignedBelow.Outcome
                == CapacityFinalCalleeSecondTestTailLifecycleOutcome
                    .EntryECXUnsignedBelowGlobalToValidation
            && unsignedBelow.DerivedFinalEntryECX == 1
            && unsignedBelow.FinalPairedTailPlan?.CompareResult == 0x10000001
            && unsignedBelow.FinalPairedTailPlan?.CompareFlags is
            {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && unsignedBelow.FinalPairedTailPlan?.GuardBranchTaken == true
            && unsignedBelow.FinalPairedTailPlan?.GuardBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && !unsignedBelow.FinalPairedTailPlan!.EntryEAXCaptured
            && !unsignedBelow.UnreachedCallerLiveInEAXCaptureProvided
            && unsignedBelow.ExactCleanExitAdmitted
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeSecondTestTailLifecyclePlan overflowBelow =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: 0x100,
                freshECX: 0x7FFFFFFF,
                freshGlobalForCompare: 0xFFFFFFFF,
                callerLiveInEAX: null));
        check("Unsigned routing preserves all CMP flags, including signed overflow",
            overflowBelow.FinalPairedTailPlan?.CompareResult == 0x80000000
            && overflowBelow.FinalPairedTailPlan?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && overflowBelow.FinalPairedTailPlan?.GuardBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && overflowBelow.ExactCleanExitAdmitted);

        CapacityFinalCalleeSecondTestTailLifecyclePlan contaminatedBelow =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: 0x100,
                freshECX: 1,
                freshGlobalForCompare: 2,
                callerLiveInEAX: 0xABC));
        check("A caller EAX supplied on the unsigned-below path is unused and rejects clean admission",
            contaminatedBelow.Outcome
                == CapacityFinalCalleeSecondTestTailLifecycleOutcome
                    .EntryECXUnsignedBelowGlobalToValidation
            && contaminatedBelow.FinalPairedTailPlan?.UnreachedEntryEAXCaptureProvided == true
            && contaminatedBelow.UnreachedCallerLiveInEAXCaptureProvided
            && !contaminatedBelow.ExactCleanExitAdmitted
            && contaminatedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeSecondTestTailLifecyclePlan missingEAX =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: 0xFFFFFFF0,
                freshECX: 0x80000001,
                freshGlobalForCompare: 0x80000000,
                callerLiveInEAX: null));
        check("An above-global route requires caller-live EAX only at the first store",
            missingEAX.Outcome
                == CapacityFinalCalleeSecondTestTailLifecycleOutcome
                    .MissingCallerLiveInEAXForFirstStore
            && missingEAX.DerivedFinalEntryECX == 0x80000001
            && missingEAX.FinalPairedTailPlan?.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingEAX.FinalPairedTailPlan?.GuardBranchTaken == false
            && missingEAX.FinalPairedTailPlan?.GuardBranchSelectedDestinationRawFileOffset
                == 0x012C9097
            && missingEAX.FinalPairedTailPlan?.StopAtRawFileOffsetExclusive == 0x012C9097
            && !missingEAX.FinalPairedTailPlan!.EntryEAXCaptured
            && !missingEAX.ExactCleanExitAdmitted);

        CapacityFinalCalleeSecondTestTailLifecyclePlan paired =
            CapacityFinalCalleeSecondTestTailLifecycleProjection.Project(Capture(
                entryESI: 0xFFFFFFE8,
                freshECX: 0xFFFFFFE8,
                freshGlobalForCompare: 0xFFFFFFE8,
                callerLiveInEAX: 0xFFFFFFEC));
        check("Fresh ECX and caller-live EAX drive ordered wrapping stores, including an alias",
            paired.Outcome
                == CapacityFinalCalleeSecondTestTailLifecycleOutcome
                    .PairedStoresAndJumpToTarget
            && paired.DerivedFinalEntryECX == 0xFFFFFFE8
            && paired.FinalPairedTailPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && paired.FinalPairedTailPlan?.GuardBranchSelectedDestinationRawFileOffset
                == 0x012C9097
            && paired.FinalPairedTailPlan?.EntryEAXCaptured == true
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
            && paired.FinalPairedTailPlan?.FinalJumpReached == true
            && paired.FinalPairedTailPlan?.FinalJumpSelectedDestinationRawFileOffset
                == 0x012C8EF2
            && paired.ExactCleanExitAdmitted
            && paired.StopAtRawFileOffsetExclusive == 0x012C8EF2);
    }

    private static CapacityFinalCalleeSecondTestTailLifecycleCapture Capture(
        uint? entryESI,
        uint? freshECX,
        uint? freshGlobalForCompare,
        uint? callerLiveInEAX) =>
        new(
            new CapacityFinalCalleeSecondFieldTestCapture(entryESI, freshECX),
            freshGlobalForCompare,
            callerLiveInEAX);
}
