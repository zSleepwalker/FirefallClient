using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleProjection;

internal static class CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The node-indexed store projection uses the bounded 93E4/945C/9468/942F windows",
            Projection.EntryRawFileOffset == 0x012C9354
            && Projection.ThresholdEntryRawFileOffset == 0x012C945C
            && Projection.StoreTailEntryRawFileOffset == 0x012C9468
            && Projection.BelowBranchTargetRawFileOffset == 0x012C8E76
            && Projection.CleanupJumpTargetRawFileOffset == 0x012C942F
            && Projection.CleanupJumpBoundaryRawFileOffsetExclusive == 0x012C9475);

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan clean =
            Project(0x100, 1, 0x300, 0, 0, null, 0x310, 0x3000);
        check("An exact clean zero-child fallthrough stores the derived child slot and inherited ESI in native order",
            clean.ExactParentZeroHandoff
            && clean.ParentLifecycle.IterationPlan?.FreshChildDWORD == 0
            && clean.ParentLifecycle.OrderedReads.Any(read =>
                read.RawFileOffset == 0x012C93F1
                && read.ValueCaptured
                && read.Value == 0)
            && clean.ChildFieldAddressAt93EE == 0x310
            && clean.ThresholdEntryECX == 0x310
            && clean.StoreTailEntryECX == clean.ChildFieldAddressAt93EE
            && clean.InheritedNodeInitializerESI == 0x3000
            && clean.StoreTailEntryESI == clean.InheritedNodeInitializerESI
            && clean.StoreTailLateEntryEDX == clean.ParentDerivedEntryEDX
            && clean.StoreTailLateEntryEDX == 0x300
            && !clean.StoreTailLateEntryEDXIsDerivedChildIndexEBX
            && !clean.StoreTailLateEntryEDXIsZeroChildValue
            && clean.Internal945CEntered
            && clean.ThresholdBranchTaken == false
            && clean.StoreTailEntryReached
            && clean.Internal9468Entered
            && !clean.Outer9468Entered
            && clean.ExactBoundedCompletion
            && clean.ExactCleanCompletion);

        check("The four 9468 stores preserve operands, wrap arithmetic, and jump without entering 942F",
            clean.StoreTail?.Writes.Select(write => write.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C9468, 0x012C946A, 0x012C946D, 0x012C9470 }) == true
            && clean.StoreTail?.Writes.Select(write => write.Address).SequenceEqual(
                new uint?[] { 0x310, 0x3018, 0x300C, 0x3008 }) == true
            && clean.StoreTail?.Writes.Select(write => write.Value).SequenceEqual(
                new uint?[] { 0x3000, 0x300, 0x3000, 0x3000 }) == true
            && clean.StoreTail?.Writes.All(write => write.Attempted && write.Committed) == true
            && clean.FirstStoreAttempted
            && clean.FirstStoreCommitted
            && clean.SecondStoreAttempted
            && clean.SecondStoreCommitted
            && clean.ThirdStoreAttempted
            && clean.ThirdStoreCommitted
            && clean.FourthStoreAttempted
            && clean.FourthStoreCommitted
            && clean.ThresholdCompareFlagsPreservedThroughStores
            && clean.CleanupJumpReached
            && clean.StopAtRawFileOffsetExclusive == 0x012C942F
            && !clean.Outer942FEntered
            && clean.StoreTail?.JumpTargetEntered == false
            && clean.StoreTail?.BoundaryReached == true
            && clean.StoreTail?.BoundaryInstructionEntered == false);

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan equal =
            Project(0x100, 1, 0x300, 0, 0, null, 0x310, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan signedOverflow =
            Project(0x100, 1, 0x7FFFFFF0, 0, 0, null, 1, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan wrapped =
            Project(0x100, 1, 0xFFFFFFF8, 0, 0, null, 0, 0xFFFFFFF8);
        check("Unsigned equality/overflow and wrapped registers retain the not-below route",
            equal.ThresholdPlan?.CompareResult == 0
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
            && equal.StoreTailEntryECX == 0x310
            && equal.ExactCleanCompletion
            && signedOverflow.ThresholdPlan?.CompareResult == 0x7FFFFFFF
            && signedOverflow.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.ThresholdBranchTaken == false
            && signedOverflow.Internal9468Entered
            && wrapped.ChildFieldAddressAt93EE == 8
            && wrapped.StoreTailEntryECX == 8
            && wrapped.StoreTail?.Writes.Select(write => write.Address).SequenceEqual(
                new uint?[] { 8, 0x10, 4, 0 }) == true
            && wrapped.StoreTail?.Writes.Select(write => write.Value).SequenceEqual(
                new uint?[] { 0xFFFFFFF8, 0xFFFFFFF8, 0xFFFFFFF8, 0xFFFFFFF8 }) == true
            && wrapped.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan below =
            Project(0x100, 1, 0x300, 0, 0, null, 0x311, 0x3000);
        check("The exact unsigned-below edge stops at 8E76 and never enters the 9468 tail",
            below.ThresholdBranchTaken == true
            && below.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76
            && below.ExactBoundedCompletion
            && below.ExactCleanCompletion
            && below.StoreTail is null
            && !below.Internal9468Entered
            && !below.Outer9468Entered
            && !below.Outer8E76Entered
            && !below.Outer942FEntered);

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan missingGlobal =
            Project(0x100, 1, 0x300, 0, 0, null, null, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan dirtyChildPlus4 =
            Project(0x100, 1, 0x300, 0, 0, 0xDEADBEEF, 0x310, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan missingParent =
            Project(0x100, 1, null, 0, 0, null, 0x310, 0x3000);
        check("Missing or unreached threshold operands and dirty late captures do not admit stores",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingFreshThresholdValueAt945C
            && missingGlobal.Internal945CEntered
            && missingGlobal.ThresholdPlan?.GlobalReadReached == true
            && missingGlobal.ThresholdPlan?.CompareReached == false
            && missingGlobal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingGlobal.StoreTail is null
            && !missingGlobal.Internal9468Entered
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C
            && dirtyChildPlus4.ParentLifecycle.LateFreshChildPlus4CaptureProvidedButUnused
            && !dirtyChildPlus4.ExactParentZeroHandoff
            && dirtyChildPlus4.ExactBoundedCompletion
                == dirtyChildPlus4.ParentLifecycle.ExactBoundedCompletion
            && !dirtyChildPlus4.ExactCleanCompletion
            && !dirtyChildPlus4.ParentLifecycle.ExactCleanCompletion
            && dirtyChildPlus4.StoreTail is null
            && !dirtyChildPlus4.Internal945CEntered
            && !dirtyChildPlus4.Internal9468Entered
            && dirtyChildPlus4.StopAtRawFileOffsetExclusive == 0x012C945C
            && missingParent.StoreTail is null
            && !missingParent.Internal9468Entered
            && missingParent.StopAtRawFileOffsetExclusive < 0x012C93E4);

        const uint thresholdAddress = 0x01EDADD8;
        uint indexedPointerAliasingStore = thresholdAddress - 0x10;
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan aliasedRead =
            Project(0x100, 1, indexedPointerAliasingStore, 0, 0, null,
                thresholdAddress, 0x3000);
        int thresholdEffectIndex = Array.FindIndex(
            aliasedRead.OrderedMemoryEffects.ToArray(),
            effect =>
            effect.Kind
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind
                    .ThresholdFreshGlobalRead);
        int aliasedStoreIndex = Array.FindIndex(
            aliasedRead.OrderedMemoryEffects.ToArray(),
            effect =>
            effect.Kind
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind
                    .StoreWrite
            && effect.Address == thresholdAddress);
        check("A threshold load remains fresh and precedes a later store to its aliased address",
            aliasedRead.ChildFieldAddressAt93EE == thresholdAddress
            && thresholdEffectIndex >= 0
            && aliasedStoreIndex > thresholdEffectIndex
            && aliasedRead.OrderedMemoryEffects[thresholdEffectIndex] is
            {
                RawFileOffset: 0x012C945C,
                Address: thresholdAddress,
                Value: thresholdAddress,
                ValueCaptured: true,
                Attempted: true,
                Committed: false,
                ForwardedFromPriorWrite: false
            }
            && aliasedRead.OrderedMemoryEffects[aliasedStoreIndex] is
            {
                RawFileOffset: 0x012C9468,
                Address: thresholdAddress,
                Value: 0x3000,
                Attempted: true,
                Committed: true,
                ForwardedFromPriorWrite: false
            }
            && aliasedRead.OrderedReads[^1].RawFileOffset == 0x012C945C
            && aliasedRead.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan outerZero =
            Project(0, 0, null, null, 0, null, 0xFFFFFFFF, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan cleanOuterZero =
            Project(0, 0, null, null, null, null, null, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan outerEqual =
            Project(0x100, 1, 0x200, 0x100, 0, null, 0xFFFFFFFF, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan cleanOuterEqual =
            Project(0x100, 1, 0x200, 0x100, null, null, null, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan nonzeroChild =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x107, 0xFFFFFFFF, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan loopChild =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x109, 0xFFFFFFFF, 0x3000);
        check("Parent zero/equal and other child exits retain their original bounded routes",
            outerZero.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .ParentOtherBoundedExit
            && outerZero.StopAtRawFileOffsetExclusive == 0x012C93AC
            && outerZero.ExactBoundedCompletion
                == outerZero.ParentLifecycle.ExactBoundedCompletion
            && outerZero.ExactCleanCompletion
                == outerZero.ParentLifecycle.ExactCleanCompletion
            && !outerZero.Outer93ACEntered
            && cleanOuterZero.StopAtRawFileOffsetExclusive == 0x012C93AC
            && cleanOuterZero.ExactBoundedCompletion
                == cleanOuterZero.ParentLifecycle.ExactBoundedCompletion
            && cleanOuterZero.ExactCleanCompletion
                == cleanOuterZero.ParentLifecycle.ExactCleanCompletion
            && outerEqual.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .ParentOtherBoundedExit
            && outerEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && outerEqual.ExactBoundedCompletion
                == outerEqual.ParentLifecycle.ExactBoundedCompletion
            && outerEqual.ExactCleanCompletion
                == outerEqual.ParentLifecycle.ExactCleanCompletion
            && !outerEqual.Outer9403Entered
            && cleanOuterEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && cleanOuterEqual.ExactBoundedCompletion
                == cleanOuterEqual.ParentLifecycle.ExactBoundedCompletion
            && cleanOuterEqual.ExactCleanCompletion
                == cleanOuterEqual.ParentLifecycle.ExactCleanCompletion
            && nonzeroChild.StopAtRawFileOffsetExclusive == 0x012C9403
            && nonzeroChild.ExactBoundedCompletion
                == nonzeroChild.ParentLifecycle.ExactBoundedCompletion
            && nonzeroChild.ExactCleanCompletion
                == nonzeroChild.ParentLifecycle.ExactCleanCompletion
            && !nonzeroChild.Outer9403Entered
            && loopChild.StopAtRawFileOffsetExclusive == 0x012C93E4
            && loopChild.ExactBoundedCompletion
                == loopChild.ParentLifecycle.ExactBoundedCompletion
            && loopChild.ExactCleanCompletion
                == loopChild.ParentLifecycle.ExactCleanCompletion
            && !loopChild.Outer93E4Entered
            && outerZero.StoreTail is null
            && cleanOuterZero.StoreTail is null
            && outerEqual.StoreTail is null
            && cleanOuterEqual.StoreTail is null
            && nonzeroChild.StoreTail is null
            && loopChild.StoreTail is null
            && !outerZero.Internal945CEntered
            && !outerEqual.Internal945CEntered
            && !nonzeroChild.Internal945CEntered
            && !loopChild.Internal945CEntered
            && !outerZero.Internal9468Entered
            && !outerEqual.Internal9468Entered
            && !nonzeroChild.Internal9468Entered
            && !loopChild.Internal9468Entered
            && !outerZero.Outer9468Entered
            && !outerEqual.Outer9468Entered
            && !nonzeroChild.Outer942FEntered);
    }

    private static CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan Project(
        uint size,
        uint mask,
        uint? freshIndexedDWORD,
        uint? freshTag,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? freshGlobalValue,
        uint? entryESI) =>
        Projection.Project(new(
            new(
                new(
                    new(
                        new(
                            EntryEDI: size,
                            LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                            EntryESI: entryESI,
                            LateFreshGlobalValue: mask),
                        FreshIndexedDWORD: freshIndexedDWORD,
                        FreshEDXPlus4: freshTag),
                    FreshChildDWORD: freshChildDWORD,
                    LateFreshChildPlus4: lateFreshChildPlus4),
                FreshGlobalValue: freshGlobalValue)));
}
