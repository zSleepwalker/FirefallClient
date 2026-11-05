using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleProjection;

internal static class CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The indexed composition shares the exact traversal and store-tail boundaries",
            Projection.EntryRawFileOffset == 0x012C93C0
            && Projection.ThresholdEntryRawFileOffset == 0x012C945C
            && Projection.StoreTailEntryRawFileOffset == 0x012C9468
            && Projection.BelowBranchTargetRawFileOffset == 0x012C8E76
            && Projection.CleanupJumpTargetRawFileOffset == 0x012C942F
            && Projection.CleanupJumpBoundaryRawFileOffsetExclusive == 0x012C9475);

        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan count31ZeroChild =
            Project(0x80, 0x100, 31, 0, 8, 0, null, 0x110, 0x180);
        check("Count 31 derives zero, uses the child-slot address, and passes the parent EDX pointer",
            count31ZeroChild.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && count31ZeroChild.CountPlan.XorEAXResult == 0
            && count31ZeroChild.TraversalPlan.ParentDerivedCountEAX == 0
            && count31ZeroChild.TraversalPlan.ParentDerivedTagEDX == 0x100
            && count31ZeroChild.TraversalPlan.TraversalEntryEBX == 0
            && count31ZeroChild.TraversalPlan.TraversalOperandsAreParentDerived
            && count31ZeroChild.ChildPlan?.ChildFieldOffset == 16
            && count31ZeroChild.ChildFieldAddressAt93EE == 0x110
            && count31ZeroChild.ThresholdEntryECX == 0x110
            && count31ZeroChild.ThresholdEntryECXIsDerivedFromChildFieldAddress
            && count31ZeroChild.ExactParentZeroHandoff
            && count31ZeroChild.Internal945CEntered
            && count31ZeroChild.ThresholdCompareReached
            && count31ZeroChild.ThresholdBranchTaken == false
            && count31ZeroChild.StoreTailEntryECX == 0x110
            && count31ZeroChild.StoreTailEntryECXIsDerivedFromThresholdLeft
            && count31ZeroChild.StoreTailLateEntryEDX == 0x100
            && count31ZeroChild.StoreTailLateEntryEDXIsOriginalParentDerivedTagEDX
            && !count31ZeroChild.StoreTailLateEntryEDXIsIndexedParentSlot
            && !count31ZeroChild.StoreTailLateEntryEDXIsZeroChildValue
            && count31ZeroChild.StoreTailEntryInstructionEntered
            && count31ZeroChild.Internal9468Entered
            && !count31ZeroChild.Outer9468Entered
            && count31ZeroChild.ExactCleanCompletion);

        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan count30Slot16 =
            Project(0x90, 0x100, 30, 0x40000000, 8, 0, null, 0x108, 0x180);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan count30Slot20 =
            Project(0x94, 0xFFFFFFF8, 30, 0x00200000, 8, 0, null, 0xC, 0x180);
        check("Count 30 computes ten and selects independent 16- and 20-byte child slots",
            count30Slot16.CountPlan.SubtractResult == 10
            && count30Slot16.TraversalPlan.ParentDerivedCountEAX == 10
            && count30Slot16.ChildPlan?.ChildFieldOffset == 16
            && count30Slot16.ChildFieldAddressAt93EE == 0x110
            && count30Slot16.ThresholdEntryECX == 0x110
            && count30Slot16.ExactCleanCompletion
            && count30Slot20.CountPlan.SubtractResult == 10
            && count30Slot20.TraversalPlan.ParentDerivedCountEAX == 10
            && count30Slot20.ChildPlan?.ChildFieldOffset == 20
            && count30Slot20.ChildFieldAddressAt93EE == 12
            && count30Slot20.ThresholdEntryECX == 12
            && count30Slot20.StoreTailEntryECXIsDerivedFromThresholdLeft
            && count30Slot20.StoreTailLateEntryEDX == 0xFFFFFFF8
            && count30Slot20.ExactCleanCompletion);

        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan wrappedSlot16 =
            Project(0xA0, 0xFFFFFFF8, 31, 0, 8, 0, null, 8, 0xFFFFFFE0);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan wrappedSlot20 =
            Project(0xA4, 0xFFFFFFF8, 30, 0x00200000, 8, 0, null, 12, 0xFFFFFFE0);
        check("High-bit slot selection wraps address arithmetic without losing source registers",
            wrappedSlot16.ChildPlan?.ChildFieldOffset == 16
            && wrappedSlot16.ChildFieldAddressAt93EE == 8
            && wrappedSlot16.StoreTailEntryECX == 8
            && wrappedSlot16.StoreTailLateEntryEDX == 0xFFFFFFF8
            && wrappedSlot16.StoreTailLateEntryEDXIsOriginalParentDerivedTagEDX
            && wrappedSlot20.ChildPlan?.ChildFieldOffset == 20
            && wrappedSlot20.ChildFieldAddressAt93EE == 12
            && wrappedSlot20.StoreTailEntryECX == 12
            && wrappedSlot20.StoreTailLateEntryEDX == 0xFFFFFFF8
            && wrappedSlot20.StoreTailLateEntryEDXIsOriginalParentDerivedTagEDX);

        const uint aliasAddress = 0x01EDADD8;
        uint aliasParentPointer = aliasAddress - 16;
        uint aliasESI = aliasAddress - 0x18;
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan aliases =
            Project(0xB0, aliasParentPointer, 31, 0, 8, 0, null, aliasAddress, aliasESI);
        check("Parent observations remain ordered and aliased stores retain all four attempts",
            aliases.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(new uint[]
                { 0x012C93C0, 0x012C93D8, 0x012C93F1, 0x012C945C })
            && aliases.OrderedReads.Select(read => read.Address).SequenceEqual(
                new uint?[] { 0xB0, aliasParentPointer + 4, aliasParentPointer + 16, aliasAddress })
            && aliases.OrderedReads.Select(read => read.Value).SequenceEqual(
                new uint?[] { aliasParentPointer, 8, 0, aliasAddress })
            && aliases.Writes.Length == 4
            && aliases.Writes.Select(write => write.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C9468, 0x012C946A, 0x012C946D, 0x012C9470 })
            && aliases.Writes.All(write => write.Attempted && write.Committed)
            && aliases.Writes[0].Address == aliasAddress
            && aliases.Writes[1].Address == aliasAddress
            && aliases.Writes[0].Value == aliasESI
            && aliases.Writes[1].Value == aliasParentPointer
            && aliases.Writes[2].Address == aliasESI + 0x0C
            && aliases.Writes[2].Value == aliasESI
            && aliases.Writes[3].Address == aliasESI + 8
            && aliases.Writes[3].Value == aliasESI
            && aliases.StoreTailLateEntryEDXIsOriginalParentDerivedTagEDX);

        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan missingParent =
            Project(null, 0x100, 31, 0, 8, 0, null, 0x120, 0x180);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan missingGlobal =
            Project(0xC0, 0x100, 31, 0, 8, 0, null, null, 0x180);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan missingESI =
            Project(0xC4, 0x100, 31, 0, 8, 0, null, 0x110, null);
        check("Missing parent, global, and late ESI inputs stop at their reached boundaries",
            missingParent.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome.ParentBlocked
            && missingParent.OrderedReads.IsEmpty
            && missingParent.StoreTail is null
            && missingParent.FreshGlobalValueCaptureProvidedButUnused
            && missingParent.LateEntryESIUnusedBecauseThresholdBypassed
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C93C0
            && missingGlobal.Internal945CEntered
            && missingGlobal.ThresholdPlan?.GlobalReadReached == true
            && missingGlobal.ThresholdPlan?.CompareLeft == 0x110
            && missingGlobal.ThresholdPlan?.CompareRight is null
            && missingGlobal.ThresholdPlan?.BranchReached == false
            && missingGlobal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingGlobal.StoreTail is null
            && missingGlobal.LateEntryESIUnusedBecauseExactStoreHandoffBlocked
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C
            && missingESI.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingLateEntryESIForFirstStore
            && missingESI.Internal945CEntered
            && missingESI.StoreTailEntryInstructionEntered
            && missingESI.Writes.Length == 1
            && missingESI.Writes[0] is
            {
                RawFileOffset: 0x012C9468,
                Address: 0x110,
                AddressCaptured: true,
                Value: null,
                ValueCaptured: false,
                Attempted: true,
                Committed: false
            }
            && missingESI.StoreTail?.LateEntryEDXReadReached == false
            && missingESI.StoreTail?.LateEntryEDXUnusedBecauseFirstStoreBlocked == true
            && missingESI.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9468
            && !missingESI.ExactCleanCompletion);

        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan dirtyChildPlus4 =
            Project(0xC8, 0x100, 31, 0, 8, 0, 0xDEADBEEF, 0x120, 0x180);
        check("A dirty unreached child plus-four blocks both threshold and stores",
            dirtyChildPlus4.ParentZeroChildOutcome
            && dirtyChildPlus4.ChildZero945CSelected
            && !dirtyChildPlus4.ExactParentZeroHandoff
            && dirtyChildPlus4.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C
            && dirtyChildPlus4.ThresholdPlan is null
            && dirtyChildPlus4.FreshGlobalValueCaptureProvidedButUnused
            && dirtyChildPlus4.LateFreshChildPlus4CaptureProvidedButUnused
            && dirtyChildPlus4.LateEntryESIUnusedBecauseThresholdBypassed
            && dirtyChildPlus4.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C93C0, 0x012C93D8, 0x012C93F1 })
            && dirtyChildPlus4.StoreTail is null
            && !dirtyChildPlus4.Internal945CEntered
            && !dirtyChildPlus4.Internal9468Entered
            && dirtyChildPlus4.StopAtRawFileOffsetExclusive == 0x012C945C
            && !dirtyChildPlus4.ExactBoundedCompletion
            && !dirtyChildPlus4.ExactCleanCompletion);

        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan missingChildPlus4 =
            Project(0x88, 0x100, 31, 0, 8, 0x120, null, null, null);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan missingChildPlus4WithUnusedInputs =
            Project(0x8A, 0x100, 31, 0, 8, 0x120, null, 0xFFFFFFFF, 0x180);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan nonzeroChildEqual =
            Project(0x94, 0xFFFFFFF8, 30, 0x00200000, 8, 0xFFFFFFFC, 0x00200007,
                null, null);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan nonzeroChildEqualWithUnusedInputs =
            Project(0x96, 0xFFFFFFF8, 30, 0x00200000, 8, 0xFFFFFFFC, 0x00200007,
                0xFFFFFFFF, 0x180);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan nonzeroChildDifferent =
            Project(0x9C, 0x100, 31, 0, 8, 0x120, 0x11111111, null, null);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan nonzeroChildDifferentWithUnusedInputs =
            Project(0x9E, 0x100, 31, 0, 8, 0x120, 0x11111111, 0xFFFFFFFF, 0x180);
        check("Nonzero-child plus-four routes stay distinct, with no reread of the indexed tag slot",
            missingChildPlus4.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9
            && missingChildPlus4.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C93C0, 0x012C93D8, 0x012C93F1, 0x012C93F9 })
            && missingChildPlus4.OrderedReads.Count(read => read.RawFileOffset == 0x012C93D8) == 1
            && !missingChildPlus4.FreshGlobalValueCaptureProvidedButUnused
            && !missingChildPlus4.LateEntryESICaptureVisible
            && !missingChildPlus4.ExactBoundedCompletion
            && missingChildPlus4WithUnusedInputs.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9
            && missingChildPlus4WithUnusedInputs.FreshGlobalValueCaptureProvidedButUnused
            && missingChildPlus4WithUnusedInputs.LateEntryESIUnusedBecauseThresholdBypassed
            && missingChildPlus4WithUnusedInputs.OrderedReads.Count(
                read => read.RawFileOffset == 0x012C93D8) == 1
            && !missingChildPlus4WithUnusedInputs.ExactBoundedCompletion
            && nonzeroChildEqual.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403
            && nonzeroChildEqual.ChildPlan?.FreshChildPlus4Address == 0
            && nonzeroChildEqual.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C93C0, 0x012C93D8, 0x012C93F1, 0x012C93F9 })
            && nonzeroChildEqual.OrderedReads.Count(read => read.RawFileOffset == 0x012C93D8) == 1
            && nonzeroChildEqual.ExactBoundedCompletion
            && nonzeroChildEqual.ExactCleanCompletion
            && nonzeroChildEqualWithUnusedInputs.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403
            && nonzeroChildEqualWithUnusedInputs.FreshGlobalValueCaptureProvidedButUnused
            && nonzeroChildEqualWithUnusedInputs.LateEntryESIUnusedBecauseThresholdBypassed
            && nonzeroChildEqualWithUnusedInputs.ExactBoundedCompletion
            && !nonzeroChildEqualWithUnusedInputs.ExactCleanCompletion
            && nonzeroChildDifferent.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4
            && nonzeroChildDifferent.ChildPlan?.LoopBranchTaken == true
            && nonzeroChildDifferent.OrderedReads.Count(
                read => read.RawFileOffset == 0x012C93D8) == 1
            && nonzeroChildDifferent.ExactBoundedCompletion
            && nonzeroChildDifferent.ExactCleanCompletion
            && nonzeroChildDifferentWithUnusedInputs.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4
            && nonzeroChildDifferentWithUnusedInputs.FreshGlobalValueCaptureProvidedButUnused
            && nonzeroChildDifferentWithUnusedInputs.LateEntryESIUnusedBecauseThresholdBypassed
            && nonzeroChildDifferentWithUnusedInputs.OrderedReads.Count(
                read => read.RawFileOffset == 0x012C93D8) == 1
            && nonzeroChildDifferentWithUnusedInputs.ExactBoundedCompletion
            && !nonzeroChildDifferentWithUnusedInputs.ExactCleanCompletion);

        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan below =
            Project(0xD0, 0x100, 31, 0, 8, 0, null, 0x111, 0x180);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan equal =
            Project(0xD4, 0x100, 31, 0, 8, 0, null, 0x110, 0x180);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan above =
            Project(0xD8, 0x100, 31, 0, 8, 0, null, 0x100, 0x180);
        check("Unsigned below branches out, equality stores, and above retains the store fallthrough",
            below.ThresholdPlan?.CompareResult == uint.MaxValue
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
            && below.Internal945CEntered
            && below.StoreTail is null
            && !below.Internal9468Entered
            && below.LateEntryESIUnused
            && below.ExactBoundedCompletion
            && !below.ExactCleanCompletion
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76
            && equal.ThresholdPlan?.CompareResult == 0
            && equal.ThresholdBranchTaken == false
            && equal.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.StoreTailEntryReached
            && equal.ExactCleanCompletion
            && above.ThresholdPlan?.CompareResult == 0x10
            && above.ThresholdBranchTaken == false
            && above.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && above.StoreTailEntryReached
            && above.ExactCleanCompletion);

        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan equalBypass =
            Project(0xDC, 0x100, 31, 0, 0, null, null, 0xFFFFFFFF, 0x180);
        check("Bypassed global and late ESI captures are visible without erasing bounded completion",
            equalBypass.TraversalOutcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ParentEqualBypassesTraversalTo9403
            && equalBypass.ThresholdPlan is null
            && equalBypass.FreshGlobalValueCaptureProvidedButUnused
            && equalBypass.LateEntryESICaptureVisible
            && equalBypass.LateEntryESIUnusedBecauseThresholdBypassed
            && equalBypass.StoreTail is null
            && equalBypass.ExactBoundedCompletion
            && !equalBypass.ExactCleanCompletion
            && equalBypass.StopAtRawFileOffsetExclusive == 0x012C9403
            && !equalBypass.Outer9468Entered
            && !equalBypass.DestinationEntered);

        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan complete =
            Project(0xE0, 0x100, 31, 0, 8, 0, null, 0x110, 0x180);
        check("All six CMP flags survive four committed stores and the cleanup jump boundary",
            complete.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .CompletedToCleanupJump
            && complete.ChildFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && complete.FlagsAtStop is { } flagsAtStop
            && complete.ChildFlags is { } childFlags
            && flagsAtStop.Carry == childFlags.Carry
            && flagsAtStop.Parity == childFlags.Parity
            && flagsAtStop.AuxiliaryCarry == childFlags.AuxiliaryCarry
            && flagsAtStop.Zero == childFlags.Zero
            && flagsAtStop.Sign == childFlags.Sign
            && flagsAtStop.Overflow == childFlags.Overflow
            && complete.ChildFlagsDerivedFromThresholdCompare
            && !complete.ChildFlagsUserProvided
            && complete.StoreTail?.MoveInstructionsPreserveFlags == true
            && complete.StoreTail?.JumpPreservesFlags == true
            && complete.ThresholdCompareFlagsPreservedThroughStoreTail
            && complete.Writes.Length == 4
            && complete.Writes.All(write => write.Attempted && write.Committed)
            && complete.FirstStoreCommitted
            && complete.SecondStoreCommitted
            && complete.ThirdStoreCommitted
            && complete.FourthStoreCommitted
            && complete.OrderedInstructions.TakeLast(5).Select(instruction =>
                instruction.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C9473
                })
            && complete.OrderedInstructions.TakeLast(5).All(instruction =>
                instruction.Phase
                    == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                        .StoreTail)
            && complete.CleanupJumpReached
            && complete.CleanupJumpTargetRawFileOffset == 0x012C942F
            && complete.CleanupJumpTargetVirtualAddress == 0x016CA02F
            && !complete.CleanupJumpTargetEntered
            && complete.CleanupJumpBoundaryReached
            && !complete.CleanupJumpBoundaryInstructionEntered
            && !complete.DestinationEntered
            && !complete.Outer9468Entered
            && complete.ExactBoundedCompletion
            && complete.ExactCleanCompletion
            && complete.StopAtRawFileOffsetExclusive == 0x012C942F);
    }

    private static CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan Project(
        uint? entryEBX,
        uint? freshIndexedDWORD,
        uint? lateEntryECX,
        uint? lateEntryEDI,
        uint? freshEDXPlus4,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? freshGlobalValue,
        uint? lateEntryESI) =>
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleProjection.Project(new(
            new(
                new(
                    new(entryEBX, freshIndexedDWORD, lateEntryECX),
                    lateEntryEDI,
                    freshEDXPlus4,
                    freshChildDWORD,
                    lateFreshChildPlus4),
                freshGlobalValue),
            lateEntryESI));
}
