using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeIndexedGuardedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeIndexedGuardedLifecyclePlan plus10 =
            Project(
                conditionalFreshEcxPlus10: 1,
                guardedFreshGlobalForCompare: 0x10,
                guardedFreshEsiPlus10: 0x20);
        check("The clean conditional plus-10 nonzero 9058 boundary reaches the guarded store with retained EAX, ECX, and ESI",
            plus10.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .GuardedFreshEsiPlus10NonzeroToBoundary
            && plus10.IndexedConditionalLifecycle.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus10StoredThenEaxNonzeroToBoundary
            && plus10.IndexedConditionalLifecycle.ConditionalFieldStore?.TestEAXFlags
                is { Zero: false }
            && plus10.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.FirstCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && plus10.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.SecondCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && plus10.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.TestEAXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && plus10.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.StoreAddress == 0x210
            && plus10.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.StoreValue == 0x45
            && plus10.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.ZeroBranchSelectedDestinationRawFileOffset == 0x012C9058
            && plus10.GuardedFieldStoreReached
            && plus10.GuardedEntryEAX == 0x45
            && plus10.GuardedEntryECX == 0x200
            && plus10.GuardedEntryESI == 1
            && plus10.GuardedEntryECXDerived
            && plus10.GuardedEntryESIDerived
            && plus10.GuardedFieldStore?.Capture.EntryEAX == 0x45
            && plus10.GuardedFieldStore?.Capture.EntryECX == 0x200
            && plus10.GuardedFieldStore?.Capture.EntryESI == 1
            && plus10.GuardedFieldStore?.StoreAddress == 0x5D
            && plus10.GuardedFieldStore?.StoreValue == 0x200
            && plus10.GuardedFieldStore?.StoreDisplacement == 0x18
            && plus10.GuardedFieldStore?.FreshEsiPlus10Address == 0x11
            && plus10.GuardedFieldStore?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && plus10.GuardedFieldStore?.TestECXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && plus10.GuardedFieldStore?.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C906E
            && plus10.StopAtRawFileOffsetExclusive == 0x012C906E
            && plus10.Admitted);

        CapacityFinalCalleeIndexedGuardedLifecyclePlan plus14 =
            Project(
                conditionalFreshEcxPlus10: 0x21,
                guardedFreshGlobalForCompare: 0x10,
                guardedFreshEsiPlus10: 0x21);
        check("The clean conditional plus-14 nonzero boundary composes the same independent guarded plus-18 store",
            plus14.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .GuardedFreshEsiPlus10NonzeroToBoundary
            && plus14.IndexedConditionalLifecycle.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus14StoredThenEaxNonzeroToBoundary
            && plus14.IndexedConditionalLifecycle.ConditionalFieldStore?.StoreDisplacement
                == 0x14
            && plus14.IndexedConditionalLifecycle.ConditionalFieldStore?.StoreAddress
                == 0x214
            && plus14.IndexedConditionalLifecycle.ConditionalFieldStore?.StoreValue
                == 0x45
            && plus14.GuardedFieldStore?.StoreDisplacement == 0x18
            && plus14.GuardedFieldStore?.StoreAddress == 0x5D
            && plus14.GuardedFieldStore?.StoreValue == 0x200
            && plus14.GuardedFieldStore?.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C906E
            && plus14.Admitted);

        CapacityFinalCalleeIndexedGuardedLifecyclePlan rootNonzero =
            Project(
                rootFreshSecondIndex: 3,
                rootEntryEAX: 0x11223344,
                firstIndexedTableValue: 1,
                conditionalEntryEBX: null,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null,
                guardedFreshGlobalForCompare: null,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeIndexedGuardedLifecyclePlan rootNonzeroContaminated =
            Project(
                rootFreshSecondIndex: 3,
                rootEntryEAX: 0x11223344,
                firstIndexedTableValue: 1,
                conditionalEntryEBX: null,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null,
                guardedFreshGlobalForCompare: 0x10);
        check("The root's different nonzero table-write route stays bounded at 9058 and never joins its known frame-minus-14 value to the guarded store",
            rootNonzero.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .IndexedRootNonzeroTableWriteExit
            && rootNonzero.IndexedConditionalLifecycle.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskNonzeroTableWriteExit
            && rootNonzero.IndexedConditionalLifecycle.IndexedRootMask.TableWriteReached
            && rootNonzero.IndexedConditionalLifecycle.IndexedRootMask.TableWriteValue
                == 0x11223344
            && rootNonzero.IndexedConditionalLifecycle.IndexedRootMask
                .FreshFrameMinus14Captured
            && rootNonzero.IndexedConditionalLifecycle.DerivedConditionalEntryECX is null
            && rootNonzero.IndexedConditionalLifecycle.IndexedRootMask.StopAtRawFileOffsetExclusive
                == 0x012C9058
            && rootNonzero.GuardedFieldStore is null
            && !rootNonzero.GuardedFieldStoreReached
            && rootNonzero.GuardedEntryECX is null
            && rootNonzero.GuardedEntryESI is null
            && rootNonzero.StopAtRawFileOffsetExclusive == 0x012C9058
            && rootNonzero.Admitted
            && rootNonzeroContaminated.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .DownstreamGuardCaptureOnBypass
            && rootNonzeroContaminated.UnreachedGuardedFreshGlobalForCompareCaptureProvided
            && rootNonzeroContaminated.GuardedFieldStore is null
            && !rootNonzeroContaminated.Admitted);

        CapacityFinalCalleeIndexedGuardedLifecyclePlan missingGuardGlobal =
            Project(
                guardedFreshGlobalForCompare: null,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeIndexedGuardedLifecyclePlan missingGuardGlobalWithLateCapture =
            Project(
                guardedFreshGlobalForCompare: null,
                guardedFreshEsiPlus10: 1);
        check("A missing fresh guarded global preserves the complete conditional store without deriving unused ECX or ESI",
            missingGuardGlobal.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome.GuardedIncomplete
            && missingGuardGlobal.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.StoreReached == true
            && missingGuardGlobal.GuardedFieldStore?.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .MissingFreshGlobalForCompare
            && missingGuardGlobal.GuardedFieldStore?.CompareGlobalReadReached == true
            && missingGuardGlobal.GuardedEntryEAX == 0x45
            && missingGuardGlobal.GuardedEntryECX is null
            && missingGuardGlobal.GuardedEntryESI is null
            && !missingGuardGlobal.GuardedEntryECXDerived
            && !missingGuardGlobal.GuardedEntryESIDerived
            && missingGuardGlobal.GuardedFieldStore?.UnreachedEntryECXCaptureProvided == false
            && missingGuardGlobal.GuardedFieldStore?.UnreachedEntryESICaptureProvided == false
            && missingGuardGlobal.IndexedConditionalLifecycle.IndexedRootMask
                .UnequalBranchTaken == true
            && missingGuardGlobal.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.StoreAddress == 0x214
            && missingGuardGlobal.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.StoreValue == 0x45
            && !missingGuardGlobal.Admitted
            && missingGuardGlobalWithLateCapture.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome.GuardedContaminated
            && missingGuardGlobalWithLateCapture.UnreachedGuardedFreshEsiPlus10CaptureProvided
            && missingGuardGlobalWithLateCapture.IndexedConditionalLifecycle
                .ConditionalFieldStore?.StoreReached == true
            && !missingGuardGlobalWithLateCapture.GuardedEntryECXDerived
            && !missingGuardGlobalWithLateCapture.GuardedEntryESIDerived);

        CapacityFinalCalleeIndexedGuardedLifecyclePlan guardedBelow =
            Project(
                guardedFreshGlobalForCompare: 0x80,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeIndexedGuardedLifecyclePlan guardedBelowContaminated =
            Project(
                guardedFreshGlobalForCompare: 0x80,
                guardedFreshEsiPlus10: 0xA5);
        check("The guarded unsigned-below edge preserves all CMP flags and branches directly to 8E76 before deriving ECX or ESI",
            guardedBelow.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .GuardedUnsignedBelowToValidation
            && guardedBelow.GuardedFieldStore?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && guardedBelow.GuardedFieldStore?.BranchTaken == true
            && guardedBelow.GuardedFieldStore?.BranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && guardedBelow.GuardedEntryEAX == 0x45
            && guardedBelow.GuardedEntryECX is null
            && guardedBelow.GuardedEntryESI is null
            && !guardedBelow.GuardedEntryECXDerived
            && !guardedBelow.GuardedEntryESIDerived
            && guardedBelow.GuardedFieldStore?.UnreachedEntryECXCaptureProvided == false
            && guardedBelow.GuardedFieldStore?.UnreachedEntryESICaptureProvided == false
            && guardedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && guardedBelow.Admitted
            && guardedBelowContaminated.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome.GuardedContaminated
            && guardedBelowContaminated.UnreachedGuardedFreshEsiPlus10CaptureProvided
            && !guardedBelowContaminated.GuardedEntryECXDerived
            && !guardedBelowContaminated.GuardedEntryESIDerived
            && !guardedBelowContaminated.Admitted);

        CapacityFinalCalleeIndexedGuardedLifecyclePlan guardedZero =
            Project(
                guardedFreshGlobalForCompare: 0x10,
                guardedFreshEsiPlus10: 0);
        check("The guarded fresh zero read branches to 9080 while preserving both earlier field stores",
            guardedZero.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .GuardedFreshEsiPlus10ZeroToTarget
            && guardedZero.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.StoreReached == true
            && guardedZero.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.ZeroBranchSelectedDestinationRawFileOffset == 0x012C9058
            && guardedZero.GuardedFieldStore?.StoreReached == true
            && guardedZero.GuardedFieldStore?.StoreAddress == 0x5D
            && guardedZero.GuardedFieldStore?.FreshEsiPlus10Address == 0x11
            && guardedZero.GuardedFieldStore?.FreshEsiPlus10 == 0
            && guardedZero.GuardedFieldStore?.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && guardedZero.GuardedFieldStore?.ZeroBranchTaken == true
            && guardedZero.GuardedFieldStore?.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C9080
            && guardedZero.StopAtRawFileOffsetExclusive == 0x012C9080
            && guardedZero.Admitted);

        CapacityFinalCalleeIndexedGuardedLifecyclePlan conditionalZero =
            Project(
                conditionalEntryEAX: 0,
                guardedFreshGlobalForCompare: null,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeIndexedGuardedLifecyclePlan rootZero =
            Project(
                freshFrameMinus14: 0,
                entryESI: null,
                freshFirstIndex: null,
                firstIndexedTableValue: null,
                freshFrameMinusC: null,
                rootFreshSecondIndex: null,
                freshFrameMinus8: null,
                freshThirdIndex: null,
                freshRootGlobalValue: null,
                conditionalEntryEBX: null,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null,
                guardedFreshGlobalForCompare: null,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeIndexedGuardedLifecyclePlan conditionalBelow =
            Project(
                freshFrameMinus14: 0x100,
                conditionalEntryEBX: 0x200,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null,
                guardedFreshGlobalForCompare: null,
                guardedFreshEsiPlus10: null);
        CapacityFinalCalleeIndexedGuardedLifecyclePlan conditionalGateMissing =
            Project(
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null,
                guardedFreshGlobalForCompare: 1,
                guardedFreshEsiPlus10: 2);
        check("The root EF8, conditional EF2, and conditional E76 paths remain bounded and reject downstream guard captures",
            rootZero.IndexedConditionalLifecycle.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskZeroExit
            && rootZero.IndexedConditionalLifecycle.IndexedRootMask
                .ZeroBranchDestinationRawFileOffset == 0x012C8EF8
            && rootZero.GuardedFieldStore is null
            && rootZero.StopAtRawFileOffsetExclusive == 0x012C8EF8
            && rootZero.Admitted
            && conditionalZero.IndexedConditionalLifecycle.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus14StoredThenEaxZeroToMaskContinuation
            && conditionalZero.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .ConditionalEaxZeroExit
            && conditionalZero.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.ZeroBranchSelectedDestinationRawFileOffset == 0x012C8EF2
            && conditionalZero.GuardedFieldStore is null
            && conditionalZero.StopAtRawFileOffsetExclusive == 0x012C8EF2
            && conditionalZero.Admitted
            && conditionalBelow.IndexedConditionalLifecycle.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalUnsignedBelowToValidation
            && conditionalBelow.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .ConditionalUnsignedBelowToValidation
            && conditionalBelow.IndexedConditionalLifecycle
                .ConditionalFieldStore?.FirstBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && conditionalBelow.IndexedConditionalLifecycle.ConditionalFieldStore
                ?.FirstCompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && conditionalBelow.GuardedFieldStore is null
            && conditionalBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && conditionalBelow.Admitted
            && conditionalGateMissing.Outcome
                == CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .DownstreamGuardCaptureOnBypass
            && conditionalGateMissing.IndexedConditionalLifecycle
                .Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalFieldStoreIncomplete
            && !conditionalGateMissing.GuardedFieldStoreReached
            && conditionalGateMissing.UnreachedGuardedFreshGlobalForCompareCaptureProvided
            && conditionalGateMissing.UnreachedGuardedFreshEsiPlus10CaptureProvided
            && conditionalGateMissing.GuardedFieldStore is null
            && !conditionalGateMissing.Admitted);

        CapacityFinalCalleeIndexedGuardedLifecyclePlan wrappedAlias =
            Project(
                entryEBP: 4,
                freshFrameMinus14: 0xFFFFFFF0,
                entryESI: 0xFFFFFFF8,
                firstIndexedTableValue: 0,
                freshFrameMinusC: 0xFFFFFFF0,
                conditionalFreshEcxPlus10: 0xFFFFFFF8,
                conditionalEntryEAX: 0xFFFFFFF0,
                guardedFreshGlobalForCompare: 0xFFFFFFF0,
                guardedFreshEsiPlus10: 0);
        check("The guarded store and independent fresh ESI+10 read preserve DWORD wrap and address aliasing",
            wrappedAlias.IndexedConditionalLifecycle.DerivedConditionalEntryECX
                == 0xFFFFFFF0
            && wrappedAlias.IndexedConditionalLifecycle
                .ConditionalFieldStore?.FreshEcxPlus10Address == 0
            && wrappedAlias.GuardedFieldStore?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && wrappedAlias.GuardedFieldStore?.StoreAddress == 8
            && wrappedAlias.GuardedFieldStore?.FreshEsiPlus10Address == 8
            && wrappedAlias.GuardedFieldStore?.Reads[1].Address == 8
            && wrappedAlias.GuardedFieldStore?.Writes[0].Address == 8
            && wrappedAlias.GuardedFieldStore?.TestECXFlags?.Zero == true
            && wrappedAlias.GuardedFieldStore?.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C9080
            && wrappedAlias.Admitted);
    }

    private static CapacityFinalCalleeIndexedGuardedLifecyclePlan Project(
        uint? entryEBP = 0x1000,
        uint? freshFrameMinus14 = 0x200,
        uint? entryESI = 1,
        uint? rootEntryEAX = null,
        uint? freshFirstIndex = 2,
        uint? firstIndexedTableValue = 0x80000000,
        uint? freshFrameMinusC = 0xABC,
        uint? rootFreshSecondIndex = null,
        uint? freshFrameMinus8 = null,
        uint? freshThirdIndex = null,
        uint? freshRootGlobalValue = null,
        uint? conditionalEntryEBX = 0x100,
        uint? conditionalFreshEcxPlus10 = 0x20,
        uint? conditionalEntryEAX = 0x45,
        uint? guardedFreshGlobalForCompare = 0x10,
        uint? guardedFreshEsiPlus10 = 0x20)
    {
        var rootCapture = new CapacityFinalCalleeIndexedRootMaskCapture(
            CaptureValidatedEntryRegisters: true,
            EntryEBP: entryEBP,
            EntryESI: entryESI,
            EntryEAX: rootEntryEAX,
            FreshFrameMinus14: freshFrameMinus14,
            FreshFirstIndex: freshFirstIndex,
            FirstIndexedTableValue: firstIndexedTableValue,
            FreshFrameMinusC: freshFrameMinusC,
            FreshSecondIndex: rootFreshSecondIndex,
            FreshFrameMinus8: freshFrameMinus8,
            FreshThirdIndex: freshThirdIndex,
            FreshGlobalValue: freshRootGlobalValue);
        return CapacityFinalCalleeIndexedGuardedLifecycleProjection.Project(new(
            new(
                rootCapture,
                conditionalEntryEBX,
                conditionalFreshEcxPlus10,
                conditionalEntryEAX),
            guardedFreshGlobalForCompare,
            guardedFreshEsiPlus10));
    }
}
