using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonRootStoreLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonRootStoreLifecyclePlan allMissing =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: null,
                FreshGlobalDWORD: null,
                CallerEDI: null));
        check("All missing parent operands retain the independent global read and stop at 92C8",
            allMissing.Parent.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome
                    .MissingEntryESIAndFreshGlobalDWORD
            && allMissing.Parent.FreshGlobalReadReached
            && allMissing.Parent.FreshGlobalAddress == 0x01EDADDC
            && !allMissing.Parent.FreshGlobalCaptured
            && allMissing.Parent.CompareReached == false
            && allMissing.Parent.JnzReached == false
            && allMissing.Child is null
            && allMissing.OrderedMemoryAccesses.SequenceEqual(new[]
            {
                new CapacityFinalCalleeCommonRootStoreLifecycleAccess(
                    CapacityFinalCalleeCommonRootStoreLifecycleAccessKind.Read,
                    0x012C92C8,
                    0x01EDADDC,
                    null)
            })
            && allMissing.Writes.IsEmpty
            && !allMissing.CallerEDICaptureReached
            && !allMissing.CallerEDICaptureProvidedButUnreached
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C92C8
            && !allMissing.EpilogueEntered);

        CapacityFinalCalleeCommonRootStoreLifecyclePlan missingESI =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: null,
                FreshGlobalDWORD: 0,
                CallerEDI: null));
        check("A captured zero global read is preserved when parent ESI is missing",
            missingESI.Parent.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI
            && missingESI.Parent.FreshGlobalReadReached
            && missingESI.Parent.FreshGlobalCaptured
            && missingESI.Parent.FreshGlobalDWORD == 0
            && missingESI.OrderedMemoryAccesses[0].Value == 0
            && missingESI.Child is null
            && !missingESI.ExactCleanExitAdmitted
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeCommonRootStoreLifecyclePlan missingGlobal =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: null,
                CallerEDI: null));
        check("A missing fresh global read blocks comparison and child entry",
            missingGlobal.Parent.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD
            && missingGlobal.Parent.FreshGlobalReadReached
            && !missingGlobal.Parent.FreshGlobalCaptured
            && missingGlobal.Parent.EntryESICaptured
            && !missingGlobal.Parent.CompareReached
            && missingGlobal.Child is null
            && missingGlobal.Writes.IsEmpty
            && !missingGlobal.ExactCleanExitAdmitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeCommonRootStoreLifecyclePlan cleanEqual =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: 0,
                CallerEDI: 0));
        check("The clean equal branch alone consumes caller EDI and reaches the single root store",
            cleanEqual.Outcome
                == CapacityFinalCalleeCommonRootStoreLifecycleOutcome.StoredAndJumped
            && cleanEqual.ExactCleanParentAdmission
            && cleanEqual.Parent.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.EqualToFallthrough
            && cleanEqual.Parent.StopAtRawFileOffsetExclusive == 0x012C92D0
            && cleanEqual.Child?.Outcome
                == CapacityFinalCalleeCommonTailRootStoreOutcome.StoredAndJumped
            && cleanEqual.CallerEDICaptureReached
            && !cleanEqual.CallerEDICaptureProvidedButUnreached
            && cleanEqual.OrderedMemoryAccesses.Select(access =>
                (access.Kind, access.RawFileOffset, access.Address, access.Value))
                .SequenceEqual(new[]
                {
                    (CapacityFinalCalleeCommonRootStoreLifecycleAccessKind.Read,
                        0x012C92C8u, 0x01EDADDCu, (uint?)0),
                    (CapacityFinalCalleeCommonRootStoreLifecycleAccessKind.Write,
                        0x012C92D0u, 0x01EDADD0u, (uint?)0)
                })
            && cleanEqual.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeCommonRootStoreLifecycleWrite(
                    0x012C92D0,
                    0x01EDADD0,
                    0)
            })
            && cleanEqual.Child?.JumpTargetRawFileOffset == 0x012C9441
            && cleanEqual.Child?.JumpTargetVirtualAddress == 0x016CA041
            && cleanEqual.FlagsAtStop == new CapacityFinalCalleeCommonTailRootGateFlags(
                false, true, false, true, false, false)
            && cleanEqual.FlagsPreservedThroughChild
            && cleanEqual.ExactCleanExitAdmitted
            && cleanEqual.StopAtRawFileOffsetExclusive == 0x012C9441
            && !cleanEqual.EpilogueEntered);

        CapacityFinalCalleeCommonRootStoreLifecyclePlan cleanUnequal =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: 1,
                CallerEDI: null));
        check("The clean unequal branch is admitted only as a bounded 92EC stop",
            cleanUnequal.Outcome
                == CapacityFinalCalleeCommonRootStoreLifecycleOutcome.UnequalToBranchTarget
            && cleanUnequal.ExactCleanParentAdmission
            && cleanUnequal.Parent.JnzTaken == true
            && cleanUnequal.Parent.JnzSelectedDestinationRawFileOffset == 0x012C92EC
            && cleanUnequal.Parent.StopAtRawFileOffsetExclusive == 0x012C92EC
            && cleanUnequal.Child is null
            && cleanUnequal.Writes.IsEmpty
            && cleanUnequal.OrderedMemoryAccesses.Length == 1
            && !cleanUnequal.CallerEDICaptureReached
            && !cleanUnequal.CallerEDICaptureProvidedButUnreached
            && cleanUnequal.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && cleanUnequal.ExactCleanExitAdmitted
            && cleanUnequal.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeCommonRootStoreLifecyclePlan missingCallerEDI =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: 0x42,
                FreshGlobalDWORD: 0x42,
                CallerEDI: null));
        check("Equal parent comparison without caller EDI stops before the store",
            missingCallerEDI.Outcome
                == CapacityFinalCalleeCommonRootStoreLifecycleOutcome.MissingCallerEDI
            && missingCallerEDI.Parent.ExactCleanAdmission
            && missingCallerEDI.Child?.Outcome
                == CapacityFinalCalleeCommonTailRootStoreOutcome.MissingEntryEDI
            && missingCallerEDI.CallerEDICaptureReached
            && missingCallerEDI.Writes.IsEmpty
            && !missingCallerEDI.ExactCleanExitAdmitted
            && missingCallerEDI.StopAtRawFileOffsetExclusive == 0x012C92D0);

        CapacityFinalCalleeCommonRootStoreLifecyclePlan wrappedStore =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: 0xFFFFFFFF,
                FreshGlobalDWORD: 0xFFFFFFFF,
                CallerEDI: 0xFFFFFFFF));
        check("Captured wrapped DWORD data is stored unchanged at the absolute address",
            wrappedStore.Parent.CompareResult == 0
            && wrappedStore.Writes.Length == 1
            && wrappedStore.Writes[0]
                == new CapacityFinalCalleeCommonRootStoreLifecycleWrite(
                    0x012C92D0,
                    0x01EDADD0,
                    0xFFFFFFFF)
            && wrappedStore.Child?.StoreValue == 0xFFFFFFFF
            && wrappedStore.FlagsPreservedThroughChild
            && wrappedStore.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonRootStoreLifecyclePlan sixCmpFlags =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: 0x80000000,
                FreshGlobalDWORD: 1,
                CallerEDI: null));
        check("All six CMP flags survive the unequal MOV/JMP-free bounded path",
            sixCmpFlags.Parent.CompareResult == 0x7FFFFFFF
            && sixCmpFlags.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && sixCmpFlags.Parent.JnzTaken == true
            && sixCmpFlags.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeCommonRootStoreLifecyclePlan contaminatedUnequal =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: 1,
                CallerEDI: 0));
        check("A supplied but unused caller EDI blocks clean unequal admission",
            contaminatedUnequal.Outcome
                == CapacityFinalCalleeCommonRootStoreLifecycleOutcome
                    .UnequalWithUnusedCallerEDI
            && contaminatedUnequal.CallerEDICaptureProvidedButUnreached
            && !contaminatedUnequal.CallerEDICaptureReached
            && contaminatedUnequal.Child is null
            && contaminatedUnequal.Writes.IsEmpty
            && !contaminatedUnequal.ExactCleanExitAdmitted
            && contaminatedUnequal.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeCommonRootStoreLifecyclePlan contaminatedMissing =
            CapacityFinalCalleeCommonRootStoreLifecycleProjection.Project(new(
                EntryESI: null,
                FreshGlobalDWORD: null,
                CallerEDI: 0));
        check("A supplied but unused caller EDI blocks clean admission on a missing parent",
            contaminatedMissing.Outcome
                == CapacityFinalCalleeCommonRootStoreLifecycleOutcome
                    .MissingParentWithUnusedCallerEDI
            && contaminatedMissing.Parent.FreshGlobalReadReached
            && contaminatedMissing.CallerEDICaptureProvidedButUnreached
            && !contaminatedMissing.CallerEDICaptureReached
            && contaminatedMissing.Child is null
            && !contaminatedMissing.ExactCleanExitAdmitted
            && contaminatedMissing.StopAtRawFileOffsetExclusive == 0x012C92C8);
    }
}
