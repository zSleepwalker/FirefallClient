using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonIndexedMaskLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The lifecycle joins the exact clean high-EDI branch at 930A",
            CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.EntryRawFileOffset
                == 0x012C92F6
            && CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.EntryVirtualAddress
                == 0x016C9EF6
            && CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.ParentHandoffRawFileOffset
                == 0x012C930A
            && CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.ChildEntryRawFileOffset
                == 0x012C930A
            && CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.FreshGlobalVirtualAddress
                == 0x01EDADC8
            && CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.ZeroDestinationRawFileOffset
                == 0x012C931B
            && CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.NonzeroDestinationRawFileOffset
                == 0x012C9334);

        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan[] countCases =
        {
            Project(entryECX: 0, entryEDI: 0x10, freshGlobal: 1),
            Project(entryECX: 1, entryEDI: 0x10, freshGlobal: 2),
            Project(entryECX: 31, entryEDI: 0x10, freshGlobal: 0x80000000),
            Project(entryECX: 32, entryEDI: 0x10, freshGlobal: 1),
            Project(entryECX: uint.MaxValue, entryEDI: 0x10, freshGlobal: 0x80000000)
        };
        uint[] expectedIndices = { 0, 1, 31, 32, uint.MaxValue };
        uint[] expectedShiftCounts = { 0, 1, 31, 0, 31 };
        uint[] expectedMasks = { 1, 2, 0x80000000, 1, 0x80000000 };
        check("Counts 0, 1, 31, 32, and FFFFFFFF derive child ECX from the reached parent LEA",
            countCases.Select((plan, index) =>
                plan.ChildReached
                && plan.ExactCleanHandoffToChild
                && plan.ChildEntryECXDerivedFromReachedLEA
                && plan.ChildEntryECX == expectedIndices[index]
                && plan.ParentDerivedEDX
                    == CapacityFinalCalleeCommonIndexedAddressGateProjection.DeriveAddress(
                        expectedIndices[index])
                && plan.ParentDerivedEAX == plan.ParentDerivedEDX
                && plan.ChildMaskSelectionGate?.ShiftCount == expectedShiftCounts[index]
                && plan.ChildMaskSelectionGate?.DestinationValueAfterShift
                    == expectedMasks[index]
                && plan.ChildMaskSelectionGate?.Capture.FreshGlobalValue
                    == expectedMasks[index]
                && plan.ChildMaskSelectionGate?.FreshGlobalReadAddress == 0x01EDADC8
                && plan.ChildMaskSelectionGate?.FreshGlobalValueCaptured == true
                && plan.ChildMaskSelectionGate?.TestResult == expectedMasks[index]
                && plan.ChildMaskSelectionGate?.JnzTaken == true
                && plan.ChildMaskSelectionGate?.JnzSelectedDestinationRawFileOffset
                    == 0x012C9334
                && plan.ExactCleanAdmission
                && plan.StopAtRawFileOffsetExclusive == 0x012C9334)
                .All(result => result));

        uint[] maskedCounts = { 0, 1, 31, 0, 31 };
        uint[] masks = { 1, 2, 0x80000000, 1, 0x80000000 };
        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan[] nonselectedCases =
            maskedCounts.Select((count, index) =>
                Project(
                    entryECX: expectedIndices[index],
                    entryEDI: 0x10,
                    freshGlobal: ~masks[index]))
                .ToArray();
        check("Each tested mask rejects a fresh global with only nonselected bits",
            nonselectedCases.Select((plan, index) =>
                plan.ChildMaskSelectionGate?.ShiftCount == maskedCounts[index]
                && plan.ChildMaskSelectionGate?.DestinationValueAfterShift == masks[index]
                && plan.ChildMaskSelectionGate?.TestResult == 0
                && plan.ChildMaskSelectionGate?.TestFlags is
                {
                    Carry: false,
                    Parity: true,
                    Auxiliary: null,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
                && plan.ChildMaskSelectionGate?.JnzTaken == false
                && plan.ChildMaskSelectionGate?.JnzSelectedDestinationRawFileOffset
                    == 0x012C931B
                && plan.ExactCleanAdmission
                && plan.StopAtRawFileOffsetExclusive == 0x012C931B)
                .All(result => result));

        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan missingParentECX =
            Project(entryECX: null, entryEDI: 0x10, freshGlobal: 1);
        check("Missing parent index prevents child entry and leaves its supplied global unused",
            missingParentECX.Outcome
                == CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentMissingEntryECX
            && !missingParentECX.ParentAddressGate.LEAReached
            && !missingParentECX.ChildReached
            && missingParentECX.UnreachedChildGlobalCaptureProvided
            && !missingParentECX.FreshGlobalReadReached
            && !missingParentECX.ExactCleanAdmission
            && missingParentECX.StopAtRawFileOffsetExclusive == 0x012C92F6);

        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan missingParentEDI =
            Project(entryECX: 3, entryEDI: null, freshGlobal: 8);
        check("Missing parent EDI retains LEA-derived addresses but prevents the child and contaminates clean admission",
            missingParentEDI.Outcome
                == CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentMissingEntryEDI
            && missingParentEDI.ParentAddressGate.LEAReached
            && missingParentEDI.ParentDerivedEDX
                == CapacityFinalCalleeCommonIndexedAddressGateProjection.DeriveAddress(3)
            && missingParentEDI.ParentDerivedEAX == missingParentEDI.ParentDerivedEDX
            && !missingParentEDI.ParentAddressGate.CompareReached
            && !missingParentEDI.ChildReached
            && missingParentEDI.UnreachedChildGlobalCaptureProvided
            && !missingParentEDI.ExactCleanAdmission
            && missingParentEDI.StopAtRawFileOffsetExclusive == 0x012C92FF);

        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan lowEDI =
            Project(entryECX: 5, entryEDI: 0xF, freshGlobal: 0xFFFFFFFF);
        check("Low EDI stops at the opaque 9304 sibling without invoking it or entering the child",
            lowEDI.Outcome
                == CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentLowEDIOpaqueStop
            && lowEDI.ParentAddressGate.JnbTaken == false
            && lowEDI.ParentAddressGate.JnbSelectedDestinationRawFileOffset == 0x012C9304
            && lowEDI.OpaqueCallInstructionAtStop
            && !lowEDI.OpaqueCallEntered
            && !lowEDI.OpaqueCallReturnObserved
            && !lowEDI.CalleeIdentityKnown
            && !lowEDI.ChildReached
            && lowEDI.UnreachedChildGlobalCaptureProvided
            && !lowEDI.ExactCleanAdmission
            && lowEDI.StopAtRawFileOffsetExclusive == 0x012C9304);

        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan missingGlobalZeroCount =
            Project(entryECX: 32, entryEDI: 0x10, freshGlobal: null);
        check("A missing global after zero-count SHL preserves the parent CMP flags unchanged",
            missingGlobalZeroCount.ChildMaskSelectionGate?.ShiftCount == 0
            && missingGlobalZeroCount.ChildMaskSelectionGate?.ShiftFlags is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingGlobalZeroCount.ParentAddressGate.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingGlobalZeroCount.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !missingGlobalZeroCount.ChildMaskSelectionGate!.TestReached
            && !missingGlobalZeroCount.ExactCleanAdmission
            && missingGlobalZeroCount.StopAtRawFileOffsetExclusive == 0x012C9311);

        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan missingGlobalNonzeroCount =
            Project(entryECX: 1, entryEDI: 0x10, freshGlobal: null);
        check("A missing global after nonzero SHL leaves the child's latest shift flags",
            missingGlobalNonzeroCount.ChildMaskSelectionGate?.ShiftCount == 1
            && missingGlobalNonzeroCount.FlagsAtStop
                == missingGlobalNonzeroCount.ChildMaskSelectionGate?.ShiftFlags
            && missingGlobalNonzeroCount.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingGlobalNonzeroCount.StopAtRawFileOffsetExclusive == 0x012C9311);

        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan latestTestFlags =
            Project(entryECX: 1, entryEDI: 0x10, freshGlobal: 2);
        check("When TEST executes its flags supersede both the SHL and parent CMP flags",
            latestTestFlags.FlagsAtStop == latestTestFlags.ChildMaskSelectionGate?.TestFlags
            && latestTestFlags.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && latestTestFlags.FlagsAtStop?.Zero !=
                latestTestFlags.ParentAddressGate.CompareFlags?.Zero);

        check("All child stops exclude the later OR/store and keep the opaque call unentered",
            countCases.Concat(nonselectedCases)
                .Append(missingGlobalZeroCount)
                .Append(missingGlobalNonzeroCount)
                .Append(latestTestFlags)
                .All(plan =>
                    !plan.OpaqueCallEntered
                    && !plan.OpaqueCallReturnObserved
                    && !plan.CalleeIdentityKnown
                    && (plan.ChildMaskSelectionGate is null
                        || (!plan.ChildMaskSelectionGate.LaterOrOrStoreReached
                            && !plan.ChildMaskSelectionGate.MemoryWritePerformed
                            && plan.ChildMaskSelectionGate.Writes.IsEmpty))));
    }

    private static CapacityFinalCalleeCommonIndexedMaskLifecyclePlan Project(
        uint? entryECX,
        uint? entryEDI,
        uint? freshGlobal) =>
        CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.Project(new(
            new(entryECX, entryEDI),
            freshGlobal));
}
