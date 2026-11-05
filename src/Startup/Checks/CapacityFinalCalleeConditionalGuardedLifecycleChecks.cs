using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeConditionalGuardedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeConditionalGuardedLifecyclePlan plus10 =
            Project();
        check("The exact conditional 9058 boundary carries retained EAX and ECX into a guarded plus-10 store",
            plus10.ConditionalFieldStore.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .Plus10StoredThenEaxNonzeroToBoundary
            && plus10.ConditionalFieldStore.FirstCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && plus10.ConditionalFieldStore.SecondCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && plus10.ConditionalFieldStore.TestEAXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && plus10.ConditionalFieldStore.StopAtRawFileOffsetExclusive
                == 0x012C9058
            && plus10.ExactConditionalBoundaryAdmitted
            && plus10.GuardedFieldStoreReached
            && plus10.GuardedEntryEAX == 0x40
            && plus10.GuardedEntryECX == 0x200
            && plus10.GuardedEntryESI == 0x20
            && plus10.GuardedEntryESIDerivedFromConditionalCapture
            && plus10.GuardedFieldStore?.Capture.EntryEAX == 0x40
            && plus10.GuardedFieldStore?.Capture.EntryECX == 0x200
            && plus10.GuardedFieldStore?.Capture.EntryESI == 0x20
            && plus10.GuardedFieldStore?.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .FreshEsiPlus10NonzeroToBoundary
            && plus10.GuardedFieldStore?.StoreAddress == 0x58
            && plus10.GuardedFieldStore?.StoreValue == 0x200
            && plus10.GuardedFieldStore?.StoreDisplacement == 0x18
            && plus10.GuardedFieldStore?.FreshEsiPlus10Address == 0x30
            && plus10.GuardedFieldStore?.TestECXValue == 0x10
            && plus10.StopAtRawFileOffsetExclusive == 0x012C906E
            && plus10.Admitted);

        CapacityFinalCalleeConditionalGuardedLifecyclePlan plus14 =
            Project(conditionalFreshEcxPlus10: 0x21);
        check("The conditional plus-14 route also reaches the same guarded plus-18 store",
            plus14.ConditionalFieldStore.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .Plus14StoredThenEaxNonzeroToBoundary
            && plus14.ConditionalFieldStore.StoreAddress == 0x214
            && plus14.ConditionalFieldStore.StoreDisplacement == 0x14
            && plus14.ConditionalFieldStore.StopAtRawFileOffsetExclusive
                == 0x012C9058
            && plus14.GuardedFieldStore?.StoreAddress == 0x58
            && plus14.GuardedFieldStore?.StoreValue == 0x200
            && plus14.GuardedFieldStore?.StoreDisplacement == 0x18
            && plus14.GuardedFieldStore?.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .FreshEsiPlus10NonzeroToBoundary
            && plus14.Admitted);

        CapacityFinalCalleeConditionalGuardedLifecyclePlan conditionalZero =
            Project(
                conditionalEntryEAX: 0,
                guardedFreshGlobalForCompare: null,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeConditionalGuardedLifecyclePlan conditionalZeroBypassed =
            Project(
                conditionalEntryEAX: 0,
                guardedFreshGlobalForCompare: 1,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeConditionalGuardedLifecyclePlan conditionalBelow =
            Project(
                conditionalEntryECX: 0x100,
                conditionalEntryEBX: 0x200,
                conditionalFreshEcxPlus10: null,
                conditionalEntryESI: null,
                conditionalEntryEAX: null,
                guardedFreshGlobalForCompare: null,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeConditionalGuardedLifecyclePlan conditionalBelowBypassed =
            Project(
                conditionalEntryECX: 0x100,
                conditionalEntryEBX: 0x200,
                conditionalFreshEcxPlus10: null,
                conditionalEntryESI: null,
                conditionalEntryEAX: null,
                guardedFreshGlobalForCompare: 1,
                guardedFreshEsiPlus10: null);
        check("Conditional zero EF2 and unsigned-below 8E76 remain bounded and reject downstream captures",
            conditionalZero.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .ConditionalEaxZeroExit
            && conditionalZero.ConditionalFieldStore.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C8EF2
            && conditionalZero.Admitted
            && !conditionalZero.GuardedFieldStoreReached
            && conditionalZero.GuardedFieldStore is null
            && conditionalZero.StopAtRawFileOffsetExclusive == 0x012C8EF2
            && conditionalZeroBypassed.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .ConditionalFieldStoreContaminated
            && !conditionalZeroBypassed.Admitted
            && conditionalZeroBypassed.UnreachedGuardedFreshGlobalForCompareCaptureProvided
            && conditionalZeroBypassed.GuardedFieldStore is null
            && conditionalBelow.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .ConditionalUnsignedBelowToValidation
            && conditionalBelow.ConditionalFieldStore.FirstBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && conditionalBelow.Admitted
            && !conditionalBelow.GuardedFieldStoreReached
            && conditionalBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && conditionalBelowBypassed.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .ConditionalFieldStoreContaminated
            && !conditionalBelowBypassed.Admitted
            && conditionalBelowBypassed.UnreachedGuardedFreshGlobalForCompareCaptureProvided);

        CapacityFinalCalleeConditionalGuardedLifecyclePlan guardedBelow =
            Project(
                guardedFreshGlobalForCompare: 0x80,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeConditionalGuardedLifecyclePlan guardedBelowContaminated =
            Project(
                guardedFreshGlobalForCompare: 0x80,
                guardedFreshEsiPlus10: 0xA5);
        check("Guarded unsigned-below branches before ECX/ESI derivation and stays bounded at 8E76",
            guardedBelow.ConditionalFieldStore.StopAtRawFileOffsetExclusive
                == 0x012C9058
            && guardedBelow.GuardedFieldStore?.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .EntryEAXUnsignedBelowGlobalToValidation
            && guardedBelow.GuardedFieldStore?.BranchTaken == true
            && guardedBelow.GuardedFieldStore?.BranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && guardedBelow.GuardedFieldStore?.Writes.IsEmpty == true
            && guardedBelow.GuardedFieldStore?.FreshEsiPlus10ReadReached == false
            && guardedBelow.GuardedEntryEAX == 0x40
            && guardedBelow.GuardedEntryECX is null
            && guardedBelow.GuardedEntryESI is null
            && !guardedBelow.GuardedEntryESIDerivedFromConditionalCapture
            && guardedBelow.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .GuardedUnsignedBelowToValidation
            && guardedBelow.Admitted
            && guardedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && guardedBelowContaminated.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .GuardedFieldStoreContaminated
            && !guardedBelowContaminated.Admitted
            && guardedBelowContaminated.UnreachedGuardedFreshEsiPlus10CaptureProvided
            && guardedBelowContaminated.GuardedFieldStore?.Writes.IsEmpty == true
            && guardedBelowContaminated.GuardedFieldStore?.FreshEsiPlus10ReadReached
                == false);

        CapacityFinalCalleeConditionalGuardedLifecyclePlan guardZero =
            Project(guardedFreshEsiPlus10: 0);
        CapacityFinalCalleeConditionalGuardedLifecyclePlan guardNonzero =
            Project(guardedFreshEsiPlus10: 0x80000001);
        check("The guarded zero route reaches 9080 while the independent nonzero read stops at 906E",
            guardZero.GuardedFieldStore?.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .FreshEsiPlus10ZeroToTarget
            && guardZero.GuardedFieldStore?.ZeroBranchTaken == true
            && guardZero.GuardedFieldStore?.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C9080
            && guardZero.GuardedFieldStore?.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && guardZero.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .GuardedFreshEsiPlus10ZeroToTarget
            && guardZero.Admitted
            && guardZero.StopAtRawFileOffsetExclusive == 0x012C9080
            && guardNonzero.GuardedFieldStore?.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .FreshEsiPlus10NonzeroToBoundary
            && guardNonzero.GuardedFieldStore?.ZeroBranchTaken == false
            && guardNonzero.GuardedFieldStore?.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C906E
            && guardNonzero.GuardedFieldStore?.TestECXFlags is
            {
                Carry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && guardNonzero.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .GuardedFreshEsiPlus10NonzeroToBoundary
            && guardNonzero.Admitted
            && guardNonzero.StopAtRawFileOffsetExclusive == 0x012C906E);

        CapacityFinalCalleeConditionalGuardedLifecyclePlan missingGlobal =
            Project(
                guardedFreshGlobalForCompare: null,
                guardedFreshEsiPlus10: 1);
        check("A missing guarded global read preserves the prior conditional write and blocks later loads",
            missingGlobal.ConditionalFieldStore.StoreReached
            && missingGlobal.ConditionalFieldStore.StoreValue == 0x40
            && missingGlobal.GuardedFieldStore?.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .MissingFreshGlobalForCompare
            && missingGlobal.GuardedFieldStore?.CompareGlobalReadReached == true
            && missingGlobal.GuardedFieldStore?.CompareGlobalCaptured == false
            && missingGlobal.GuardedFieldStore?.Writes.IsEmpty == true
            && missingGlobal.GuardedFieldStore?.FreshEsiPlus10ReadReached == false
            && missingGlobal.UnreachedGuardedFreshEsiPlus10CaptureProvided
            && missingGlobal.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .GuardedFieldStoreContaminated
            && !missingGlobal.Admitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9058);

        CapacityFinalCalleeConditionalGuardedLifecyclePlan missingConditionalGate =
            Project(
                conditionalEntryEBX: null,
                conditionalFreshEcxPlus10: 0x20,
                conditionalEntryESI: 0x20,
                conditionalEntryEAX: 0x40,
                guardedFreshGlobalForCompare: 0x20,
                guardedFreshEsiPlus10: 1);
        CapacityFinalCalleeConditionalGuardedLifecyclePlan missingConditionalEsi =
            Project(
                conditionalEntryESI: null,
                independentlySuppliedGuardedEntryESI: 0x300);
        CapacityFinalCalleeConditionalGuardedLifecyclePlan extraGuardedEsi =
            Project(independentlySuppliedGuardedEntryESI: 0x999);
        check("Missing conditional gates and uncaptured ESI cannot be bypassed with later register or memory inputs",
            missingConditionalGate.ConditionalFieldStore.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome.MissingEntryEBX
            && missingConditionalGate.ConditionalFieldStore.UnreachedFreshEcxPlus10CaptureProvided
            && missingConditionalGate.ConditionalFieldStore.UnreachedEntryESICaptureProvided
            && missingConditionalGate.ConditionalFieldStore.UnreachedEntryEAXCaptureProvided
            && !missingConditionalGate.GuardedFieldStoreReached
            && missingConditionalGate.GuardedFieldStore is null
            && missingConditionalGate.UnreachedGuardedFreshGlobalForCompareCaptureProvided
            && missingConditionalGate.UnreachedGuardedFreshEsiPlus10CaptureProvided
            && missingConditionalGate.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .ConditionalFieldStoreContaminated
            && !missingConditionalGate.Admitted
            && missingConditionalGate.StopAtRawFileOffsetExclusive == 0x012C903B
            && missingConditionalEsi.ConditionalFieldStore.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome.MissingEntryESI
            && !missingConditionalEsi.ConditionalFieldStore.EntryESICaptured
            && !missingConditionalEsi.GuardedFieldStoreReached
            && missingConditionalEsi.GuardedFieldStore is null
            && missingConditionalEsi.UnreachedIndependentlySuppliedGuardedEntryESICaptureProvided
            && missingConditionalEsi.GuardedEntryESI is null
            && !missingConditionalEsi.GuardedEntryESIDerivedFromConditionalCapture
            && !missingConditionalEsi.Admitted
            && extraGuardedEsi.GuardedFieldStore?.Capture.EntryESI == 0x20
            && extraGuardedEsi.GuardedEntryESI == 0x20
            && extraGuardedEsi.GuardedEntryESIDerivedFromConditionalCapture
            && extraGuardedEsi.UnreachedIndependentlySuppliedGuardedEntryESICaptureProvided
            && extraGuardedEsi.Outcome
                == CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .GuardedFieldStoreContaminated
            && !extraGuardedEsi.Admitted);

        CapacityFinalCalleeConditionalGuardedLifecyclePlan wrappedAliasedStores =
            Project(
                conditionalEntryECX: 0xFFFFFFF8,
                conditionalEntryEBX: 1,
                conditionalFreshEcxPlus10: 8,
                conditionalEntryESI: 0xFFFFFFF8,
                conditionalEntryEAX: 0xFFFFFFF0,
                guardedFreshGlobalForCompare: 0xFFFFFFE0,
                guardedFreshEsiPlus10: 0);
        check("Wrapped plus-14 and guarded plus-18 writes alias a later independent ESI+10 read in order",
            wrappedAliasedStores.ConditionalFieldStore.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .Plus14StoredThenEaxNonzeroToBoundary
            && wrappedAliasedStores.ConditionalFieldStore.StoreAddress == 0xC
            && wrappedAliasedStores.ConditionalFieldStore.StoreValue == 0xFFFFFFF0
            && wrappedAliasedStores.GuardedFieldStore?.StoreAddress == 8
            && wrappedAliasedStores.GuardedFieldStore?.StoreValue == 0xFFFFFFF8
            && wrappedAliasedStores.GuardedFieldStore?.FreshEsiPlus10Address == 8
            && wrappedAliasedStores.GuardedFieldStore?.Reads.Length == 2
            && wrappedAliasedStores.GuardedFieldStore?.Reads[1].Address == 8
            && wrappedAliasedStores.GuardedFieldStore?.Reads[1].Value == 0
            && wrappedAliasedStores.GuardedFieldStore?.Writes.Length == 1
            && wrappedAliasedStores.GuardedFieldStore?.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .FreshEsiPlus10ZeroToTarget
            && wrappedAliasedStores.GuardedEntryESI == 0xFFFFFFF8
            && wrappedAliasedStores.GuardedEntryESIDerivedFromConditionalCapture
            && wrappedAliasedStores.Admitted);

    }

    private static CapacityFinalCalleeConditionalGuardedLifecyclePlan Project(
        uint? conditionalEntryECX = 0x200,
        uint? conditionalEntryEBX = 0x100,
        uint? conditionalFreshEcxPlus10 = 0x20,
        uint? conditionalEntryESI = 0x20,
        uint? conditionalEntryEAX = 0x40,
        uint? guardedFreshGlobalForCompare = 0x20,
        uint? independentlySuppliedGuardedEntryESI = null,
        uint? guardedFreshEsiPlus10 = 0x10) =>
        CapacityFinalCalleeConditionalGuardedLifecycleProjection.Project(new(
            new(
                conditionalEntryECX,
                conditionalEntryEBX,
                conditionalFreshEcxPlus10,
                conditionalEntryESI,
                conditionalEntryEAX),
            guardedFreshGlobalForCompare,
                independentlySuppliedGuardedEntryESI,
            guardedFreshEsiPlus10));
}
