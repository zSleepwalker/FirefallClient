using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection;

internal static class CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition begins at the tagged traversal and bounds cleanup before its call",
            Projection.EntryRawFileOffset == 0x012C93D4
            && Projection.StoreTailEntryRawFileOffset == 0x012C9468
            && Projection.CleanupCounterEntryRawFileOffset == 0x012C942F
            && Projection.CleanupCounterNonzeroTargetRawFileOffset == 0x012C9441
            && Projection.CleanupCounterBoundaryRawFileOffsetExclusive == 0x012C943C
            && Projection.CleanupCounterVirtualAddress == 0x01EDADE8
            && CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                == 0x01EDADC8);

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan msbClear =
            Project(0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, 7);
        check("The clean 16-byte-slot route admits exactly four stores and enters DEC at 942F",
            msbClear.ExactParentStoreCompletion
            && msbClear.Outcome
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .DecrementedNonzero
            && msbClear.ParentLifecycle.Outcome
                == CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .CompletedToCleanupJump
            && msbClear.ParentLifecycle.ExactCleanCompletion
            && msbClear.ParentLifecycle.ParentLifecycle.Internal945CEntered
            && msbClear.ParentLifecycle.Internal9468Entered
            && msbClear.ParentLifecycle.StoreTailEntryECX == 0
            && msbClear.ParentLifecycle.StoreTailEntryECXIsDerivedFromThresholdLeft
            && msbClear.ParentLifecycle.StoreTailLateEntryEDX == 0xFFFFFFF0
            && msbClear.ParentLifecycle.StoreTailLateEntryEDXIsOriginalTraversalParentPointer
            && msbClear.StoreWrites.Length == 4
            && msbClear.StoreWrites.All(write => write.Attempted && write.Committed)
            && msbClear.ParentStoreJumpReached
            && msbClear.ParentStoreJumpTargetRawFileOffset == 0x012C942F
            && !msbClear.ParentStoreJumpTargetEntered
            && msbClear.CleanupCounterGateEntered
            && msbClear.Internal942FEntered
            && msbClear.CounterValueCaptured
            && msbClear.CounterBefore == 7
            && msbClear.CounterAfter == 6
            && msbClear.ExactCleanupCounterCompletion
            && msbClear.CleanupCarryDerivedFromParent
            && msbClear.CounterDecrementPreservedParentCarry
            && msbClear.CleanupFlagsAfterDecrement is { Carry: false }
            && msbClear.CleanupCounterPlan?.JnzReached == true
            && msbClear.CleanupCounterPlan?.JnzTaken == true
            && msbClear.CleanupCounterPlan?.SelectedDestinationRawFileOffset == 0x012C9441
            && !msbClear.CounterNonzeroTargetEntered
            && msbClear.ExactBoundedCompletion
            && msbClear.ExactCleanCompletion
            && !msbClear.DestinationEntered
            && msbClear.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan msbSet =
            Project(0x80000000, 0xFFFFFFF8, 0x100, 0, null, 0xC, 1);
        check("The set-MSB 20-byte slot wraps independently and carries the derived ECX into cleanup",
            msbSet.ExactParentStoreCompletion
            && msbSet.Outcome
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .DecrementedToZero
            && msbSet.ParentLifecycle.ChildFieldAddressAt93EE == 0xC
            && msbSet.ParentLifecycle.StoreTailEntryECX == 0xC
            && msbSet.ParentLifecycle.Writes[0].Address == 0xC
            && msbSet.ParentLifecycle.StoreTailLateEntryEDX == 0xFFFFFFF8
            && msbSet.ExactCleanupCounterCompletion
            && msbSet.CleanupCounterPlan?.CounterAfter == 0
            && msbSet.CleanupCounterPlan?.JnzTaken == false
            && msbSet.StopAtRawFileOffsetExclusive == 0x012C943C);

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan dirtyChildPlus4 =
            Project(0x40000000, 0x100, 0x200, 0, 0xDEADBEEF, 1, 3);
        check("A supplied unreached child plus-four blocks the parent handoff and leaves counter unused",
            dirtyChildPlus4.ParentLifecycle.ParentLifecycle.ChildZero945CSelected
            && !dirtyChildPlus4.ParentLifecycle.ParentLifecycle.Internal945CEntered
            && dirtyChildPlus4.ParentLifecycle.ParentLifecycle
                .LateFreshChildPlus4CaptureProvidedButUnused
            && dirtyChildPlus4.Outcome
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C
            && !dirtyChildPlus4.ExactParentStoreCompletion
            && dirtyChildPlus4.CleanupCounterPlan is null
            && !dirtyChildPlus4.Internal942FEntered
            && dirtyChildPlus4.FreshCounterCaptureProvidedButUnused
            && dirtyChildPlus4.ExactBoundedCompletion == false
            && !dirtyChildPlus4.ExactCleanCompletion
            && dirtyChildPlus4.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan equalBypass =
            Project(0x12345678, 0x100, 0x1234567F, null, null, null, 3, null);
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan equalBypassClean =
            Project(0x12345678, 0x100, 0x1234567F, null, null, null, null, null);
        check("An exact parent bypass remains bounded while an unreached counter makes it unclean",
            equalBypass.ParentLifecycle.TraversalOutcome
                == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ParentEqualBypassesChildTo9403
            && equalBypass.CleanupCounterPlan is null
            && equalBypass.FreshCounterCaptureProvidedButUnused
            && equalBypass.ExactBoundedCompletion
            && !equalBypass.ExactCleanCompletion
            && equalBypass.StopAtRawFileOffsetExclusive == 0x012C9403
            && equalBypassClean.ExactBoundedCompletion
            && equalBypassClean.ExactCleanCompletion);

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan missingESI =
            Project(0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, 1, null);
        check("A missing late ESI attempts but cannot commit the first store or enter cleanup",
            missingESI.ParentLifecycle.Outcome
                == CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .MissingLateEntryESIForFirstStore
            && missingESI.ParentLifecycle.Writes is
            [
                {
                    RawFileOffset: 0x012C9468,
                    Address: 0,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Attempted: true,
                    Committed: false
                }
            ]
            && !missingESI.ExactParentStoreCompletion
            && missingESI.CleanupCounterPlan is null
            && missingESI.FreshCounterCaptureProvidedButUnused
            && !missingESI.CounterReadAttempted
            && missingESI.OrderedInstructions[^1] is
            {
                RawFileOffset: 0x012C9468,
                Reached: true,
                Executed: false
            }
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9468);

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan missingGlobal =
            Project(0x40000000, 0x100, 0x200, 0, null, null, 9);
        check("A missing global is attempted at 945C but blocks all stores and cleanup-counter access",
            missingGlobal.ParentLifecycle.ParentLifecycle.ThresholdPlan?.GlobalReadReached == true
            && missingGlobal.ParentLifecycle.ParentLifecycle.ThresholdPlan?.GlobalAddress
                == 0x01EDADD8
            && missingGlobal.ParentLifecycle.Writes.IsEmpty
            && !missingGlobal.ExactParentStoreCompletion
            && missingGlobal.CleanupCounterPlan is null
            && missingGlobal.FreshCounterCaptureProvidedButUnused
            && !missingGlobal.CounterReadAttempted
            && missingGlobal.OrderedInstructions[^1] is
            {
                RawFileOffset: 0x012C945C,
                Reached: true,
                Executed: false
            }
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan below =
            Project(0x40000000, 0xFFFFFFF0, 0x100, 0, null, 1, 4, 0x2000);
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan equal =
            Project(0x40000000, 0x100, 0x200, 0, null, 0x110, 4);
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan above =
            Project(0x40000000, 0x100, 0x200, 0, null, 1, 4);
        check("Unsigned below exits before stores; equal and above complete the store-to-cleanup route",
            below.ParentLifecycle.ThresholdBranchTaken == true
            && below.ParentLifecycle.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.ParentLifecycle.Writes.IsEmpty
            && below.CleanupCounterPlan is null
            && below.FreshCounterCaptureProvidedButUnused
            && below.ExactBoundedCompletion
            && !below.ExactCleanCompletion
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76
            && equal.ParentLifecycle.ThresholdBranchTaken == false
            && equal.ParentLifecycle.ThresholdPlan?.CompareResult == 0
            && equal.ExactParentStoreCompletion
            && above.ParentLifecycle.ThresholdBranchTaken == false
            && above.ParentLifecycle.ThresholdPlan?.CompareResult is > 0
            && above.ExactParentStoreCompletion
            && above.CounterAfter == 3);

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan missingCounter =
            Project(0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, null);
        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags expectedParentFlags = new(
            Carry: false,
            Parity: true,
            AuxiliaryCarry: false,
            Zero: true,
            Sign: false,
            Overflow: false);
        check("A missing fresh counter attempts its read only and preserves all six parent flags",
            missingCounter.ExactParentStoreCompletion
            && missingCounter.Outcome
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .MissingFreshCounterDword
            && missingCounter.CounterReadAttempted
            && !missingCounter.CounterValueCaptured
            && !missingCounter.CounterWriteAttempted
            && missingCounter.CounterAccesses.Length == 1
            && missingCounter.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: 0x01EDADE8,
                ValueCaptured: false,
                Value: null
            }
            && missingCounter.OrderedInstructions[^1] is
            {
                RawFileOffset: 0x012C942F,
                Reached: true,
                Executed: false
            }
            && missingCounter.FlagsAtStop == expectedParentFlags
            && missingCounter.ParentFlagsAtStop == expectedParentFlags
            && missingCounter.CleanupCounterPlan?.CarryAtStop == false
            && missingCounter.OrderedMemoryEffects.TakeLast(1).Single() is
            {
                Kind: CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead,
                Attempted: true,
                ValueCaptured: false,
                Committed: false
            }
            && !missingCounter.ExactBoundedCompletion
            && !missingCounter.ExactCleanCompletion
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        uint counterAddress = 0x01EDADE8;
        uint aliasParentEDX = counterAddress - 16;
        uint aliasESI = counterAddress - 0x18;
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan aliasing =
            Project(0x40000000, aliasParentEDX, 0x100, 0, null, counterAddress, 5, aliasESI);
        check("A fresh counter read remains independent when the earlier second store aliases it",
            aliasing.ExactParentStoreCompletion
            && aliasing.ParentLifecycle.Writes[1].Address == counterAddress
            && aliasing.ParentLifecycle.Writes[1].Value == aliasParentEDX
            && aliasing.CounterAccesses[0].Address == counterAddress
            && aliasing.CounterAccesses[0].Value == 5
            && aliasing.CounterBefore == 5
            && aliasing.CounterAfter == 4
            && aliasing.OrderedReads.Select(read => read.Address).SequenceEqual(
                new uint?[] { aliasParentEDX + 4, counterAddress, 0x01EDADD8 })
            && aliasing.OrderedReads[2].Value == counterAddress
            && aliasing.OrderedReads[2].RawFileOffset == 0x012C945C
            && aliasing.OrderedMemoryEffects.Select(effect => effect.Kind).SequenceEqual(
                Enumerable.Repeat(
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentRead,
                        3)
                    .Concat(Enumerable.Repeat(
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentStore,
                        4))
                    .Concat(new[]
                    {
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                            .CleanupCounterRead,
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                            .CleanupCounterWrite
                    }))
            && aliasing.OrderedMemoryEffects.TakeLast(2).Select(effect =>
                (effect.Attempted, effect.ValueCaptured, effect.Committed)).SequenceEqual(
                new[] { (true, true, false), (true, true, true) }));

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan zeroCounter =
            Project(0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, 0);
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan oneCounter =
            Project(0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, 1);
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan maxCounter =
            Project(0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, uint.MaxValue);
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan signBitCounter =
            Project(0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, 0x80000000);
        check("DEC zero, one, max, and sign-bit transitions preserve parent CF=false and compute flags",
            zeroCounter.CounterAfter == uint.MaxValue
            && zeroCounter.CleanupFlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zeroCounter.CleanupCounterPlan?.JnzTaken == true
            && zeroCounter.StopAtRawFileOffsetExclusive == 0x012C9441
            && oneCounter.CounterAfter == 0
            && oneCounter.CleanupFlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && oneCounter.CleanupCounterPlan?.MovEcxReached == true
            && oneCounter.CleanupCounterPlan?.EcxValue == 0x01EDADC8
            && oneCounter.StopAtRawFileOffsetExclusive == 0x012C943C
            && maxCounter.CounterAfter == 0xFFFFFFFE
            && maxCounter.CleanupFlagsAfterDecrement is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && maxCounter.CleanupCounterPlan?.JnzTaken == true
            && signBitCounter.CounterAfter == 0x7FFFFFFF
            && signBitCounter.CleanupFlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signBitCounter.CleanupCounterPlan?.JnzTaken == true);

        check("Ordered instructions stop at the selected branch and never enter 9441 or the zero-path call",
            zeroCounter.OrderedInstructions.Count(instruction =>
                instruction.RawFileOffset == 0x012C93E4) == 1
            && zeroCounter.ParentLifecycle.TraversalPlan.ExactSingleIterationEvidence
            && !zeroCounter.ParentLifecycle.TraversalPlan.ChildLoopBack93E4Selected
            && zeroCounter.OrderedInstructions.Any(instruction =>
                instruction.RawFileOffset == 0x012C945C
                && instruction.Phase
                    == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstructionPhase
                        .ZeroSlotThreshold)
            && zeroCounter.OrderedInstructions.TakeLast(7).Select(instruction =>
                instruction.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C9473,
                    0x012C942F,
                    0x012C9435
                })
            && !zeroCounter.OrderedInstructions.Any(instruction =>
                instruction.RawFileOffset == 0x012C9441
                || instruction.RawFileOffset == 0x012C943C)
            && oneCounter.OrderedInstructions[^1] is
            {
                RawFileOffset: 0x012C9437,
                Reached: true,
                Executed: true
            }
            && !oneCounter.ZeroPathCallReached
            && !oneCounter.ZeroPathCalleeEntered
            && !oneCounter.ZeroPathImmediateWasMemoryRead
            && !oneCounter.CounterNonzeroTargetEntered
            && !oneCounter.Outer9441Entered
            && !oneCounter.Outer943CCallEntered);
    }

    private static CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan Project(
        uint entryEDI,
        uint entryEDX,
        uint freshEDXPlus4,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? freshGlobalValue,
        uint? freshCounterDword,
        uint? lateEntryESI = 0xFFFFFFF8) =>
        Projection.Project(new(
            new(
                new(
                    EntryEAX: 0,
                    EntryEDI: entryEDI,
                    EntryEDX: entryEDX,
                    FreshEDXPlus4: freshEDXPlus4,
                    FreshChildDWORD: freshChildDWORD,
                    LateFreshChildPlus4: lateFreshChildPlus4,
                    FreshGlobalValue: freshGlobalValue),
                LateEntryESI: lateEntryESI),
            FreshCounterDword: freshCounterDword));
}
