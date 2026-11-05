using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleProjection;

internal static class CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The indexed zero-slot composition shares the exact traversal and threshold boundaries",
            Projection.EntryRawFileOffset == 0x012C93C0
            && Projection.ZeroChildBoundaryRawFileOffsetExclusive == 0x012C945C
            && Projection.ThresholdBoundaryRawFileOffsetExclusive == 0x012C9468);

        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan count31ZeroChild =
            Project(0x80, 0x100, 31, 0, 8, 0, null, 0x120);
        check("Count 31 derives zero, consumes the parent EDI, and derives threshold ECX from the child LEA",
            count31ZeroChild.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && count31ZeroChild.CountPlan.XorEAXResult == 0
            && count31ZeroChild.TagPlan?.FreshEDXPlus4OperandConsumed == true
            && count31ZeroChild.TraversalPlan.ParentUnequalHandoffExact
            && count31ZeroChild.TraversalPlan.TraversalInheritedEDIAlreadyConsumed
            && count31ZeroChild.TraversalPlan.TraversalEntryEBX == 0
            && count31ZeroChild.TraversalPlan.TraversalPlan?.ChildFieldOffset == 16
            && count31ZeroChild.ChildFieldAddressAt93EE == 0x110
            && count31ZeroChild.ThresholdEntryECX == 0x110
            && count31ZeroChild.ThresholdEntryECXIsDerivedFromChildFieldAddress
            && count31ZeroChild.ExactParentZeroHandoff
            && count31ZeroChild.ParentZeroChildOutcome
            && count31ZeroChild.ChildZeroBranchReached
            && count31ZeroChild.ChildZeroBranchTaken
            && count31ZeroChild.ChildZeroBranchDestinationRawFileOffset == 0x012C945C
            && count31ZeroChild.ChildZeroStopAt945C
            && count31ZeroChild.Internal945CEntered
            && count31ZeroChild.ThresholdCompareReached
            && count31ZeroChild.ThresholdBranchTaken == true
            && count31ZeroChild.ThresholdBranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && count31ZeroChild.ExactCleanCompletion
            && count31ZeroChild.StopAtRawFileOffsetExclusive == 0x012C8E76
            && !count31ZeroChild.ChildZero945CBoundaryTargetEntered
            && !count31ZeroChild.Outer8E76Entered
            && !count31ZeroChild.Outer9468Entered
            && !count31ZeroChild.TargetEntered);

        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan count30Slot16 =
            Project(0x90, 0x100, 30, 0x40000000, 8, 0, null, 0x200);
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan count30Slot20 =
            Project(0x94, 0xFFFFFFF8, 30, 0x00200000, 8, 0, null, 13);
        check("Count 30 arithmetic and the high-bit-dependent 16- and 20-byte slots stay independent",
            count30Slot16.CountPlan.SubtractResult == 10
            && count30Slot16.TagPlan?.ShiftCountMasked == 10
            && count30Slot16.TagPlan?.ShiftResult == 0
            && count30Slot16.ChildPlan?.ChildFieldOffset == 16
            && count30Slot16.ChildFieldAddressAt93EE == 0x110
            && count30Slot16.ThresholdEntryECX == 0x110
            && count30Slot16.ExactParentZeroHandoff
            && count30Slot16.ExactCleanCompletion
            && count30Slot20.CountPlan.SubtractResult == 10
            && count30Slot20.TagPlan?.ShiftCountMasked == 10
            && count30Slot20.TagPlan?.ShiftResult == 0x80000000
            && count30Slot20.ChildPlan?.ChildFieldOffset == 20
            && count30Slot20.ChildFieldAddressAt93EE == 12
            && count30Slot20.ThresholdEntryECX == 12
            && count30Slot20.ThresholdEntryECXIsDerivedFromChildFieldAddress
            && count30Slot20.ThresholdPlan?.CompareRight == 13
            && count30Slot20.ExactParentZeroHandoff
            && count30Slot20.ExactCleanCompletion);

        const uint aliasAddress = 0x01EDADD8;
        uint aliasParentEDX = aliasAddress - 16;
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan below =
            Project(0xA0, aliasParentEDX, 31, 0, 8, 0, null, aliasAddress + 1);
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan equal =
            Project(0xA0, aliasParentEDX, 31, 0, 8, 0, null, aliasAddress);
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan above =
            Project(0xA0, aliasParentEDX, 31, 0, 8, 0, null, aliasAddress - 1);
        check("Aliased child/global addresses are independent ordered reads with unsigned CMP/JB flags",
            below.OrderedReads.Select(read => read.Address).SequenceEqual(new uint?[]
            {
                0xA0,
                aliasParentEDX + 4,
                aliasAddress,
                aliasAddress
            })
            && below.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(new uint[]
            {
                0x012C93C0,
                0x012C93D8,
                0x012C93F1,
                0x012C945C
            })
            && below.OrderedReads.Select(read => read.Value).SequenceEqual(new uint?[]
            {
                aliasParentEDX,
                8,
                0,
                aliasAddress + 1
            })
            && below.ExactParentZeroHandoff
            && below.ExactCleanCompletion
            && below.ThresholdPlan?.CompareResult == uint.MaxValue
            && below.ThresholdPlan?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && below.ThresholdBranchTaken == true
            && below.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && equal.ThresholdPlan?.CompareResult == 0
            && equal.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.ThresholdBranchTaken == false
            && equal.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C9468
            && above.ThresholdPlan?.CompareResult == 1
            && above.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && above.ThresholdBranchTaken == false
            && above.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C9468
            && below.OrderedInstructions.Count(instruction =>
                instruction.RawFileOffset == 0x012C93D8) == 1
            && below.OrderedInstructions.Count(instruction =>
                instruction.RawFileOffset == 0x012C93E4) == 1
            && below.OrderedInstructions.Count(instruction =>
                instruction.RawFileOffset == 0x012C945C) == 1
            && below.OrderedInstructions.All(instruction =>
                instruction.RawFileOffset != 0x012C8E76
                && instruction.RawFileOffset != 0x012C9468)
            && !below.Outer8E76Entered
            && !equal.Outer9468Entered
            && !above.Outer9468Entered);

        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan missingParent =
            Project(null, null, 31, 0, 8, 0, null, 0x100);
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan missingParentCount =
            Project(0x20, null, 31, 0, 8, 0, null, 0x100);
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan missingTag =
            Project(0x24, 0x300, 30, 0x10, null, 0x100, 0x200, 0x100);
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan missingChild =
            Project(0x28, 0x100, 31, 0, 8, null, null, 0x100);
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan missingGlobal =
            Project(0x2C, 0x100, 31, 0, 8, 0, null, null);
        check("Missing parent, tag, child, and global inputs stop at their reached instruction",
            missingParent.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome.ParentBlocked
            && missingParent.TraversalPlan.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.MissingEntryEBX
            && missingParent.OrderedReads.IsEmpty
            && missingParent.FreshGlobalValueCaptureProvidedButUnused
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C93C0
            && missingParentCount.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome.ParentBlocked
            && missingParentCount.OrderedReads is
            [{
                RawFileOffset: 0x012C93C0,
                Address: 0x20,
                AddressCaptured: true,
                ValueCaptured: false,
                ReadReached: true
            }]
            && missingParentCount.StopAtRawFileOffsetExclusive == 0x012C93C0
            && missingTag.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ParentTagGateStopped
            && missingTag.TagPlan?.Outcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingFreshEDXPlus4
            && missingTag.OrderedReads[^1] is
            {
                RawFileOffset: 0x012C93D8,
                Address: 0x304,
                AddressCaptured: true,
                ValueCaptured: false,
                ReadReached: true
            }
            && missingTag.ThresholdPlan is null
            && missingTag.FreshGlobalValueCaptureProvidedButUnused
            && missingChild.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1
            && missingChild.ChildFieldAddressAt93EE == 0x110
            && missingChild.OrderedReads[^1] is
            {
                RawFileOffset: 0x012C93F1,
                Address: 0x110,
                AddressCaptured: true,
                ValueCaptured: false,
                ReadReached: true
            }
            && missingChild.ThresholdPlan is null
            && missingChild.FreshGlobalValueCaptureProvidedButUnused
            && missingGlobal.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C
            && missingGlobal.ExactParentZeroHandoff
            && missingGlobal.Internal945CEntered
            && missingGlobal.ThresholdGlobalReadReached
            && missingGlobal.ThresholdPlan?.CompareLeft == 0x110
            && missingGlobal.ThresholdPlan?.CompareRight is null
            && missingGlobal.ThresholdPlan?.CompareFlags is null
            && missingGlobal.ThresholdPlan?.BranchReached == false
            && missingGlobal.OrderedReads[^1] is
            {
                RawFileOffset: 0x012C945C,
                Address: 0x01EDADD8,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null,
                ReadReached: true
            }
            && missingGlobal.FlagsAtStop == missingGlobal.ChildPlan?.ChildTestFlags
            && missingGlobal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !missingGlobal.ExactCleanCompletion
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan dirtyLateChildPlus4 =
            Project(0x30, 0x100, 31, 0, 8, 0, 0xDEADBEEF, 0x100);
        check("A dirty unused child plus-four blocks threshold entry without dirtying consumed parent EDI",
            dirtyLateChildPlus4.ParentZeroChildOutcome
            && dirtyLateChildPlus4.TraversalPlan.TraversalInheritedEDIAlreadyConsumed
            && dirtyLateChildPlus4.TraversalPlan.LateEntryEDICaptureProvidedButUnused == false
            && dirtyLateChildPlus4.ChildZero945CSelected
            && !dirtyLateChildPlus4.ExactParentZeroHandoff
            && dirtyLateChildPlus4.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C
            && dirtyLateChildPlus4.LateFreshChildPlus4CaptureProvidedButUnused
            && dirtyLateChildPlus4.FreshGlobalValueCaptureProvidedButUnused
            && dirtyLateChildPlus4.ThresholdPlan is null
            && !dirtyLateChildPlus4.Internal945CEntered
            && !dirtyLateChildPlus4.ChildZero945CEntered
            && !dirtyLateChildPlus4.ChildPlus4ReadReached
            && dirtyLateChildPlus4.StopAtRawFileOffsetExclusive == 0x012C945C
            && !dirtyLateChildPlus4.ExactCleanCompletion);

        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan equalBypass =
            Project(0x34, 0x100, 31, 0, 0, null, null, 0xFFFFFFFF);
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan loopBack =
            Project(0x38, 0x100, 31, 0, 8, 0x200, 0x300, 0xFFFFFFFF);
        check("Parent equality and the selected loop-back bypass the global threshold read",
            equalBypass.TraversalOutcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ParentEqualBypassesTraversalTo9403
            && equalBypass.ThresholdPlan is null
            && equalBypass.FreshGlobalValueCaptureProvidedButUnused
            && equalBypass.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C93C0, 0x012C93D8 })
            && !equalBypass.ExactCleanCompletion
            && loopBack.TraversalOutcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4
            && loopBack.TraversalPlan.ChildLoopBack93E4Selected
            && !loopBack.TraversalPlan.ChildLoopBack93E4Entered
            && loopBack.ThresholdPlan is null
            && loopBack.FreshGlobalValueCaptureProvidedButUnused
            && loopBack.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C93C0, 0x012C93D8, 0x012C93F1, 0x012C93F9 })
            && !loopBack.ExactCleanCompletion
            && !loopBack.Outer8E76Entered
            && !loopBack.Outer9468Entered);
    }

    private static CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan Project(
        uint? entryEBX,
        uint? freshIndexedDWORD,
        uint? lateEntryECX,
        uint? lateEntryEDI,
        uint? freshEDXPlus4,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? freshGlobalValue) =>
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleProjection.Project(new(
            Parent: new(
                Parent: new(entryEBX, freshIndexedDWORD, lateEntryECX),
                LateEntryEDI: lateEntryEDI,
                FreshEDXPlus4: freshEDXPlus4,
                FreshChildDWORD: freshChildDWORD,
                LateFreshChildPlus4: lateFreshChildPlus4),
            FreshGlobalValue: freshGlobalValue));
}
