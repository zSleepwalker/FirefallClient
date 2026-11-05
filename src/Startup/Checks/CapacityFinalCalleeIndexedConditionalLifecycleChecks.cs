using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeIndexedConditionalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeIndexedConditionalLifecyclePlan unequalJoin =
            Project();
        check("The clean unequal indexed comparison retains ECX and ESI into the independent conditional-store captures",
            unequalJoin.IndexedRootMask.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.FirstComparisonUnequal
            && unequalJoin.IndexedRootMask.FirstCompareFlags is
            {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && unequalJoin.IndexedRootMask.FreshFrameMinusCReadReached
            && unequalJoin.IndexedRootMask.FreshFrameMinusCCaptured
            && unequalJoin.IndexedRootMask.UnequalBranchTaken == true
            && unequalJoin.IndexedRootMask.UnequalBranchDestinationRawFileOffset
                == 0x012C903B
            && unequalJoin.IndexedRootMask.StopAtRawFileOffsetExclusive == 0x012C903B
            && unequalJoin.DerivedConditionalEntryECX == 0x200
            && unequalJoin.PreservedConditionalEntryESI == 1
            && unequalJoin.ConditionalFieldStoreReached
            && unequalJoin.ConditionalFieldStore?.Capture.EntryECX == 0x200
            && unequalJoin.ConditionalFieldStore?.Capture.EntryEBX == 0x100
            && unequalJoin.ConditionalFieldStore?.Capture.EntryESI == 1
            && unequalJoin.ConditionalFieldStore?.Capture.EntryEAX == 0x45
            && unequalJoin.IndexedRootMask.Capture.EntryEAX is null
            && unequalJoin.IndexedRootMask.Capture.FreshSecondIndex is null
            && unequalJoin.ConditionalFieldStore?.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .Plus14StoredThenEaxNonzeroToBoundary
            && unequalJoin.ConditionalFieldStore?.FirstCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && unequalJoin.ConditionalFieldStore?.SecondCompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && unequalJoin.ConditionalFieldStore?.StoreAddress == 0x214
            && unequalJoin.ConditionalFieldStore?.StoreValue == 0x45
            && unequalJoin.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus14StoredThenEaxNonzeroToBoundary
            && unequalJoin.Admitted
            && unequalJoin.StopAtRawFileOffsetExclusive == 0x012C9058);

        CapacityFinalCalleeIndexedConditionalLifecyclePlan missingEBX =
            Project(conditionalEntryEBX: null, conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null);
        check("Missing independent EBX preserves the root comparison and stops before the conditional CMP",
            missingEBX.IndexedRootMask.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.FirstComparisonUnequal
            && missingEBX.IndexedRootMask.UnequalBranchTaken == true
            && missingEBX.ConditionalFieldStoreReached
            && missingEBX.ConditionalFieldStore?.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome.MissingEntryEBX
            && missingEBX.ConditionalFieldStore?.EntryECXCaptured == true
            && missingEBX.ConditionalFieldStore?.EntryEBXCaptured == false
            && missingEBX.ConditionalFieldStore?.Capture.EntryESI is null
            && missingEBX.ConditionalFieldStore?.UnreachedEntryESICaptureProvided == false
            && missingEBX.ConditionalFieldStore?.FirstCompareResult is null
            && missingEBX.ConditionalFieldStore?.StoreReached == false
            && missingEBX.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalFieldStoreIncomplete
            && !missingEBX.Admitted
            && missingEBX.StopAtRawFileOffsetExclusive == 0x012C903B);

        CapacityFinalCalleeIndexedConditionalLifecyclePlan missingEAX =
            Project(
                conditionalFreshEcxPlus10: 2,
                conditionalEntryEAX: null);
        check("Missing independent EAX retains the fresh compare and selected branch without fabricating a store",
            missingEAX.ConditionalFieldStore?.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .MissingEntryEAXForPlus14Store
            && missingEAX.ConditionalFieldStore?.SecondCompareResult == 1
            && missingEAX.ConditionalFieldStore?.SecondCompareFlags is
            {
                Carry: false,
                Zero: false,
                Sign: false
            }
            && missingEAX.ConditionalFieldStore?.SecondBranchTaken == true
            && missingEAX.ConditionalFieldStore?.EntryEAXCaptured == false
            && missingEAX.ConditionalFieldStore?.StoreReached == false
            && missingEAX.IndexedRootMask.UnequalBranchTaken == true
            && missingEAX.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalFieldStoreIncomplete
            && !missingEAX.Admitted
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C904D);

        CapacityFinalCalleeIndexedConditionalLifecyclePlan capturedZeroEAX =
            Project(
                conditionalFreshEcxPlus10: 1,
                conditionalEntryEAX: 0);
        check("A captured zero is stored on the equal plus-10 route before branching to EF2",
            capturedZeroEAX.ConditionalFieldStore?.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .Plus10StoredThenEaxZeroToTarget
            && capturedZeroEAX.ConditionalFieldStore?.SecondCompareResult == 0
            && capturedZeroEAX.ConditionalFieldStore?.SecondCompareFlags?.Zero == true
            && capturedZeroEAX.ConditionalFieldStore?.StoreAddress == 0x210
            && capturedZeroEAX.ConditionalFieldStore?.StoreValue == 0
            && capturedZeroEAX.ConditionalFieldStore?.TestEAXReached == true
            && capturedZeroEAX.ConditionalFieldStore?.TestEAXValue == 0
            && capturedZeroEAX.ConditionalFieldStore?.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C8EF2
            && capturedZeroEAX.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus10StoredThenEaxZeroToMaskContinuation
            && capturedZeroEAX.Admitted
            && capturedZeroEAX.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeIndexedConditionalLifecyclePlan noCallFromRetainedCarry =
            Project();
        CapacityFinalCalleeIndexedConditionalLifecyclePlan callBoundary =
            Project(
                conditionalEntryEBX: 0x300,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null);
        check("The root CMP carry is retained for its JNZ only; the later ECX CMP independently avoids the validation call",
            noCallFromRetainedCarry.IndexedRootMask.FirstCompareFlags?.Carry == true
            && noCallFromRetainedCarry.IndexedRootMask.UnequalBranchTaken == true
            && noCallFromRetainedCarry.ConditionalFieldStore?.FirstCompareFlags is
            {
                Carry: false,
                Zero: false
            }
            && noCallFromRetainedCarry.ConditionalFieldStore?.EntryECXUnsignedBelowEntryEBX
                == false
            && noCallFromRetainedCarry.ConditionalFieldStore?.FirstBranchTaken == false
            && noCallFromRetainedCarry.ConditionalFieldStore?.FirstBranchSelectedDestinationRawFileOffset
                == 0x012C9043
            && noCallFromRetainedCarry.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus14StoredThenEaxNonzeroToBoundary
            && noCallFromRetainedCarry.Admitted
            && callBoundary.ConditionalFieldStore?.FirstCompareFlags is
            {
                Carry: true,
                Zero: false
            }
            && callBoundary.ConditionalFieldStore?.FirstBranchTaken == true
            && callBoundary.ConditionalFieldStore?.FirstBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && callBoundary.ConditionalFieldStore?.Capture.EntryESI is null
            && callBoundary.ConditionalFieldStore?.UnreachedEntryESICaptureProvided == false
            && callBoundary.ConditionalFieldStore?.UnreachedEntryEBXCaptureProvided == false
            && callBoundary.ConditionalFieldStore?.UnreachedFreshEcxPlus10CaptureProvided
                == false
            && callBoundary.ConditionalFieldStore?.UnreachedEntryEAXCaptureProvided
                == false
            && callBoundary.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalUnsignedBelowToValidation
            && callBoundary.Admitted
            && callBoundary.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeIndexedConditionalLifecyclePlan missingFrameMinusC =
            Project(
                freshFrameMinusC: null,
                conditionalEntryEBX: null,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null);
        check("A missing fresh EBP-minus-C read blocks the nested JNZ join at the MOV before 9013",
            missingFrameMinusC.IndexedRootMask.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFrameMinusC
            &&
            missingFrameMinusC.IndexedRootMask.FreshFrameMinusCReadReached
            && !missingFrameMinusC.IndexedRootMask.FreshFrameMinusCCaptured
            && !missingFrameMinusC.IndexedRootMask.UnequalBranchReached
            && !missingFrameMinusC.ConditionalFieldStoreReached
            && missingFrameMinusC.ConditionalFieldStore is null
            && missingFrameMinusC.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskIncomplete
            && !missingFrameMinusC.Admitted
            && missingFrameMinusC.StopAtRawFileOffsetExclusive == 0x012C9013);

        CapacityFinalCalleeIndexedConditionalLifecyclePlan zeroExit =
            Project(
                freshFrameMinus14: 0,
                entryESI: null,
                freshFirstIndex: null,
                firstIndexedTableValue: null,
                freshFrameMinusC: null,
                freshSecondIndex: null,
                freshFrameMinus8: null,
                freshThirdIndex: null,
                freshGlobalValue: null,
                conditionalEntryEBX: null,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null);
        CapacityFinalCalleeIndexedConditionalLifecyclePlan nonzeroExit =
            Project(
                freshFrameMinus14: 8,
                entryESI: 0x80,
                firstIndexedTableValue: 0x80,
                freshSecondIndex: 3,
                entryEAX: 0xAABBCCDD,
                conditionalEntryEBX: null,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null);
        CapacityFinalCalleeIndexedConditionalLifecyclePlan maskContinuation =
            Project(
                freshFrameMinus14: 8,
                entryESI: 0x80,
                firstIndexedTableValue: 0x80,
                freshSecondIndex: 3,
                entryEAX: 0,
                freshFrameMinus8: 4,
                freshThirdIndex: 5,
                freshGlobalValue: 0xFFFFFFFF,
                conditionalEntryEBX: null,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null);
        check("The other clean root exits remain separately admitted bounded outcomes and retain the root table write",
            zeroExit.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskZeroExit
            && zeroExit.Admitted
            && !zeroExit.UnreachedConditionalEntryEBXCaptureProvided
            && !zeroExit.UnreachedConditionalFreshEcxPlus10CaptureProvided
            && !zeroExit.UnreachedConditionalEntryEAXCaptureProvided
            && zeroExit.IndexedRootMask.ZeroBranchDestinationRawFileOffset
                == 0x012C8EF8
            && zeroExit.IndexedRootMask.TableWriteReached == false
            && zeroExit.ConditionalFieldStore is null
            && zeroExit.StopAtRawFileOffsetExclusive == 0x012C8EF8
            && nonzeroExit.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskNonzeroTableWriteExit
            && nonzeroExit.Admitted
            && !nonzeroExit.UnreachedConditionalEntryEBXCaptureProvided
            && !nonzeroExit.UnreachedConditionalFreshEcxPlus10CaptureProvided
            && !nonzeroExit.UnreachedConditionalEntryEAXCaptureProvided
            && nonzeroExit.IndexedRootMask.TableWriteReached
            && nonzeroExit.IndexedRootMask.TableWriteValue == 0xAABBCCDD
            && nonzeroExit.IndexedRootMask.NonzeroBranchDestinationRawFileOffset
                == 0x012C9058
            && nonzeroExit.ConditionalFieldStore is null
            && nonzeroExit.StopAtRawFileOffsetExclusive == 0x012C9058
            && maskContinuation.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskContinuation
            && maskContinuation.Admitted
            && !maskContinuation.UnreachedConditionalEntryEBXCaptureProvided
            && !maskContinuation.UnreachedConditionalFreshEcxPlus10CaptureProvided
            && !maskContinuation.UnreachedConditionalEntryEAXCaptureProvided
            && maskContinuation.IndexedRootMask.TableWriteReached
            && maskContinuation.IndexedRootMask.TableWriteValue == 0
            && maskContinuation.IndexedRootMask.GlobalWriteReached
            && maskContinuation.IndexedRootMask.StopAtRawFileOffsetExclusive
                == 0x012C8EF2
            && maskContinuation.ConditionalFieldStore is null
            && maskContinuation.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeIndexedConditionalLifecyclePlan rootRegisterContamination =
            Project(entryEAX: 0x10);
        CapacityFinalCalleeIndexedConditionalLifecyclePlan partialTableWriteContamination =
            Project(
                freshFrameMinus14: 8,
                entryESI: 0x80,
                firstIndexedTableValue: 0x80,
                freshSecondIndex: 3,
                entryEAX: 0xAABBCCDD);
        CapacityFinalCalleeIndexedConditionalLifecyclePlan lateBoundedCapture =
            Project(
                freshFrameMinus14: 0,
                entryESI: null,
                freshFirstIndex: null,
                firstIndexedTableValue: null,
                freshFrameMinusC: null,
                freshSecondIndex: null,
                freshFrameMinus8: null,
                freshThirdIndex: null,
                freshGlobalValue: null,
                conditionalEntryEBX: 1,
                conditionalFreshEcxPlus10: null,
                conditionalEntryEAX: null);
        CapacityFinalCalleeIndexedConditionalLifecyclePlan conditionalCallContamination =
            Project(
                conditionalEntryEBX: 0x300,
                conditionalFreshEcxPlus10: 0x20,
                conditionalEntryEAX: 0x44);
        check("Unreached root and post-branch captures contaminate admission without erasing prior writes or branch evidence",
            rootRegisterContamination.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskContaminated
            && rootRegisterContamination.UnreachedIndexedRootEntryEAXCaptureProvided
            && !rootRegisterContamination.ConditionalFieldStoreReached
            && rootRegisterContamination.UnreachedConditionalEntryEBXCaptureProvided
            && rootRegisterContamination.UnreachedConditionalFreshEcxPlus10CaptureProvided
            && rootRegisterContamination.UnreachedConditionalEntryEAXCaptureProvided
            && rootRegisterContamination.IndexedRootMask.FirstCompareReached
            && !rootRegisterContamination.Admitted
            && partialTableWriteContamination.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskContaminated
            && partialTableWriteContamination.IndexedRootMask.TableWriteReached
            && partialTableWriteContamination.IndexedRootMask.TableWriteValue
                == 0xAABBCCDD
            && partialTableWriteContamination.IndexedRootMask.NonzeroBranchDestinationRawFileOffset
                == 0x012C9058
            && !partialTableWriteContamination.ConditionalFieldStoreReached
            && !partialTableWriteContamination.Admitted
            && lateBoundedCapture.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskContaminated
            && lateBoundedCapture.UnreachedConditionalEntryEBXCaptureProvided
            && lateBoundedCapture.IndexedRootMask.ZeroBranchDestinationRawFileOffset
                == 0x012C8EF8
            && !lateBoundedCapture.Admitted
            && conditionalCallContamination.ConditionalFieldStore?.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .EntryECXUnsignedBelowEntryEBXToValidation
            && conditionalCallContamination.ConditionalFieldStore?.Capture.EntryESI is null
            && conditionalCallContamination.ConditionalFieldStore?.UnreachedEntryESICaptureProvided
                == false
            && conditionalCallContamination.ConditionalFieldStore?.FirstBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && conditionalCallContamination.UnreachedConditionalFreshEcxPlus10CaptureProvided
            && conditionalCallContamination.UnreachedConditionalEntryEAXCaptureProvided
            && conditionalCallContamination.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalFieldStoreContaminated
            && !conditionalCallContamination.Admitted);

        CapacityFinalCalleeIndexedConditionalLifecyclePlan wrapped =
            Project(
                entryEBP: 0x10,
                freshFrameMinus14: 0xFFFFFFF0,
                entryESI: 5,
                firstIndexedTableValue: 6,
                freshFrameMinusC: 0,
                conditionalEntryEBX: 0,
                conditionalFreshEcxPlus10: 0x77,
                conditionalEntryEAX: 0xAABBCCDD);
        check("Retained ECX, the fresh ECX-plus-10 read, and the selected store address all use wrapping DWORD arithmetic",
            wrapped.IndexedRootMask.FreshFrameMinus14Address == 0xFFFFFFFC
            && wrapped.IndexedRootMask.FreshFrameMinusCAddress == 4
            && wrapped.DerivedConditionalEntryECX == 0xFFFFFFF0
            && wrapped.ConditionalFieldStore?.FreshEcxPlus10Address == 0
            && wrapped.ConditionalFieldStore?.FreshEcxPlus10 == 0x77
            && wrapped.ConditionalFieldStore?.StoreAddress == 4
            && wrapped.ConditionalFieldStore?.StoreValue == 0xAABBCCDD
            && wrapped.ConditionalFieldStore?.SecondCompareResult == 0x72
            && wrapped.Outcome
                == CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus14StoredThenEaxNonzeroToBoundary
            && wrapped.Admitted);
    }

    private static CapacityFinalCalleeIndexedConditionalLifecyclePlan Project(
        bool captureValidatedEntryRegisters = true,
        uint? entryEBP = 0x1000,
        uint? freshFrameMinus14 = 0x200,
        uint? entryESI = 1,
        uint? entryEAX = null,
        uint? freshFirstIndex = 2,
        uint? firstIndexedTableValue = 0x80000000,
        uint? freshFrameMinusC = 0xABC,
        uint? freshSecondIndex = null,
        uint? freshFrameMinus8 = null,
        uint? freshThirdIndex = null,
        uint? freshGlobalValue = null,
        uint? conditionalEntryEBX = 0x100,
        uint? conditionalFreshEcxPlus10 = 0x20,
        uint? conditionalEntryEAX = 0x45)
    {
        var rootCapture = new CapacityFinalCalleeIndexedRootMaskCapture(
            captureValidatedEntryRegisters,
            entryEBP,
            entryESI,
            entryEAX,
            freshFrameMinus14,
            freshFirstIndex,
            firstIndexedTableValue,
            freshFrameMinusC,
            freshSecondIndex,
            freshFrameMinus8,
            freshThirdIndex,
            freshGlobalValue);
        return CapacityFinalCalleeIndexedConditionalLifecycleProjection.Project(
            new(
                rootCapture,
                conditionalEntryEBX,
                conditionalFreshEcxPlus10,
                conditionalEntryEAX));
    }
}
