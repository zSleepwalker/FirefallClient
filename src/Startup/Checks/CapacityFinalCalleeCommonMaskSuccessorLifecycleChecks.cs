using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonMaskSuccessorLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan missingParentECX =
            Project(parentECX: null, parentGlobal: 1, childEDX: 0x100, edxPlus8: 2, childGlobal: 3);
        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan missingParentGlobal =
            Project(parentECX: 1, parentGlobal: null, childEDX: 0x100, edxPlus8: 2, childGlobal: 3);
        check("Missing parent captures stop before the child and leave child inputs unused",
            missingParentECX.Outcome
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome.ParentMissingEntryECX
            && !missingParentECX.ChildReached
            && missingParentECX.UnreachedChildCaptureProvided
            && missingParentECX.OrderedReads.IsEmpty
            && missingParentECX.StopAtRawFileOffsetExclusive == 0x012C930F
            && missingParentGlobal.Outcome
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                    .ParentMissingFreshGlobal1EDADC8
            && !missingParentGlobal.ChildReached
            && missingParentGlobal.UnreachedChildCaptureProvided
            && missingParentGlobal.ParentGlobalRead is
            {
                RawFileOffset: 0x012C9311,
                Address: 0x01EDADC8,
                ValueCaptured: false
            }
            && missingParentGlobal.OrderedReads.Length == 1
            && missingParentGlobal.StopAtRawFileOffsetExclusive == 0x012C9311);

        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan parentZero =
            Project(parentECX: 1, parentGlobal: 0, childEDX: null, edxPlus8: null, childGlobal: null);
        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan parentZeroWithUnusedChild =
            Project(parentECX: 1, parentGlobal: 0, childEDX: 0x100, edxPlus8: 2, childGlobal: 3);
        check("Parent TEST zero stops exactly at 931B and rejects supplied child captures",
            parentZero.Outcome
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                    .ParentZeroTestToBounded931B
            && !parentZero.ChildReached
            && parentZero.ExactCleanAdmission
            && parentZero.SelectedDestinationRawFileOffset == 0x012C931B
            && parentZero.StopAtRawFileOffsetExclusive == 0x012C931B
            && parentZeroWithUnusedChild.UnreachedChildCaptureProvided
            && !parentZeroWithUnusedChild.ChildReached
            && !parentZeroWithUnusedChild.ExactCleanAdmission
            && parentZeroWithUnusedChild.StopAtRawFileOffsetExclusive == 0x012C931B
            && parentZero.Writes.IsEmpty
            && !parentZero.FutureWritesReached);

        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan missingChildEDX =
            Project(parentECX: 1, parentGlobal: 2, childEDX: null, edxPlus8: 2, childGlobal: 3);
        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan missingChildField =
            Project(parentECX: 1, parentGlobal: 2, childEDX: 0x100, edxPlus8: null, childGlobal: 3);
        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan missingChildGlobal =
            Project(parentECX: 1, parentGlobal: 2, childEDX: 0x100, edxPlus8: 2, childGlobal: null);
        check("Missing child operands stop at their first dependent read and preserve parent TEST flags",
            missingChildEDX.Outcome
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome.ChildMissingEntryEDX
            && missingChildEDX.OrderedReads.Length == 1
            && !missingChildEDX.Child!.FreshEdxPlus8ReadReached
            && missingChildEDX.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false,
                Source: CapacityFinalCalleeCommonMaskSuccessorLifecycleFlagSource.ParentTest
            }
            && missingChildField.Outcome
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                    .ChildMissingFreshEDXPlus8
            && missingChildField.OrderedReads.Length == 2
            && missingChildField.Child!.Reads[0] is
            {
                Address: 0x108,
                ValueCaptured: false
            }
            && !missingChildField.Child.FreshGlobalReadReached
            && missingChildField.FlagsAtStop == missingChildEDX.FlagsAtStop
            && missingChildGlobal.Outcome
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                    .ChildMissingFreshGlobal1EDADD8
            && missingChildGlobal.OrderedReads.Length == 3
            && missingChildGlobal.Child!.FreshGlobalAddress == 0x01EDADD8
            && !missingChildGlobal.Child.Reads[1].ValueCaptured
            && missingChildGlobal.FlagsAtStop == missingChildEDX.FlagsAtStop
            && !missingChildGlobal.ExactCleanAdmission);

        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan below =
            Project(parentECX: 1, parentGlobal: 2, childEDX: 0x100, edxPlus8: 0, childGlobal: 1);
        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan notBelow =
            Project(parentECX: 1, parentGlobal: 2, childEDX: 0x100, edxPlus8: 1, childGlobal: 0);
        check("The child unsigned CMP selects exactly 8E76 or 9343 and replaces parent flags",
            below.Outcome
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                    .ChildUnsignedBelowBranchTarget
            && below.SelectedDestinationRawFileOffset == 0x012C8E76
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76
            && below.FlagsAtStop?.Source
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleFlagSource.ChildCompare
            && below.FlagsAtStop?.Auxiliary == true
            && notBelow.Outcome
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleOutcome
                    .ChildUnsignedNotBelowFallthrough
            && notBelow.SelectedDestinationRawFileOffset == 0x012C9343
            && notBelow.StopAtRawFileOffsetExclusive == 0x012C9343
            && notBelow.FlagsAtStop?.Source
                == CapacityFinalCalleeCommonMaskSuccessorLifecycleFlagSource.ChildCompare
            && below.ExactCleanAdmission
            && notBelow.ExactCleanAdmission
            && below.Writes.IsEmpty
            && notBelow.Writes.IsEmpty
            && !below.TargetEntered
            && !notBelow.TargetEntered
            && !below.FutureWritesReached
            && !notBelow.FutureWritesReached);

        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan highBit =
            Project(parentECX: 31, parentGlobal: 0x80000000, childEDX: 0x200, edxPlus8: 5, childGlobal: 6);
        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan zeroCount =
            Project(parentECX: 0, parentGlobal: 1, childEDX: 0x240, edxPlus8: 9, childGlobal: 10);
        check("Counts zero and 31 select their expected masks without deriving child EDX from mask or ECX",
            highBit.Parent.DestinationValueAfterShift == 0x80000000
            && highBit.Parent.TestResult == 0x80000000
            && highBit.ChildCapture?.EntryEDX == 0x200
            && highBit.Child?.FreshEdxPlus8Address == 0x208
            && highBit.Child?.CompareLeft == 5
            && highBit.Child?.CompareRight == 6
            && highBit.Child?.JBTaken == true
            && highBit.ChildCapture?.EntryEDX != highBit.Parent.DestinationValueAfterShift
            && highBit.ChildCapture?.EntryEDX != highBit.Capture.ParentEntryECX
            && zeroCount.Parent.ShiftCount == 0
            && zeroCount.Parent.DestinationValueAfterShift == 1
            && zeroCount.Parent.TestResult == 1
            && zeroCount.ChildCapture?.EntryEDX == 0x240
            && zeroCount.Child?.FreshEdxPlus8Address == 0x248);

        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan aliasedAddress =
            Project(
                parentECX: 31,
                parentGlobal: 0x80000000,
                childEDX: 0x01EDADC0,
                edxPlus8: 0x22,
                childGlobal: 0x23);
        CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan distinctGlobals =
            Project(parentECX: 0, parentGlobal: 1, childEDX: 0x300, edxPlus8: 100, childGlobal: 200);
        check("Aliased addresses and the two globals retain their independently captured values",
            aliasedAddress.ParentGlobalRead?.Address == 0x01EDADC8
            && aliasedAddress.Child?.FreshEdxPlus8Address == 0x01EDADC8
            && aliasedAddress.ParentGlobalRead?.Value == 0x80000000
            && aliasedAddress.Child?.FreshEdxPlus8 == 0x22
            && aliasedAddress.Child?.FreshGlobalAddress == 0x01EDADD8
            && aliasedAddress.Child?.FreshGlobal == 0x23
            && aliasedAddress.OrderedReads.Select(read => read.Address)
                .SequenceEqual(new uint?[] { 0x01EDADC8, 0x01EDADC8, 0x01EDADD8 })
            && distinctGlobals.ParentGlobalRead?.Value == 1
            && distinctGlobals.Child?.CompareRight == 200
            && distinctGlobals.Child?.JBTaken == true
            && distinctGlobals.Child?.CompareRight != distinctGlobals.ParentGlobalRead?.Value);

        check("The combined trace keeps the parent read first and includes only reached child reads",
            below.ParentGlobalRead is
            {
                RawFileOffset: 0x012C9311,
                Address: 0x01EDADC8,
                ValueCaptured: true
            }
            && below.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C9311, 0x012C9334, 0x012C9337 })
            && missingChildEDX.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C9311 })
            && missingChildField.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C9311, 0x012C9334 })
            && missingChildGlobal.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C9311, 0x012C9334, 0x012C9337 })
            && missingChildField.UnreachedChildCaptureProvided
            && !missingChildGlobal.UnreachedChildCaptureProvided);
    }

    private static CapacityFinalCalleeCommonMaskSuccessorLifecyclePlan Project(
        uint? parentECX,
        uint? parentGlobal,
        uint? childEDX,
        uint? edxPlus8,
        uint? childGlobal) =>
        CapacityFinalCalleeCommonMaskSuccessorLifecycleProjection.Project(new(
            parentECX,
            parentGlobal,
            childEDX,
            edxPlus8,
            childGlobal));
}
