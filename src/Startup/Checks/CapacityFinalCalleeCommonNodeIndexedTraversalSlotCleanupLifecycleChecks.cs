using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleProjection;

internal static class CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The node-indexed cleanup gate joins the committed store jump at 942F",
            Projection.EntryRawFileOffset == 0x012C9354
            && Projection.StoreTailEntryRawFileOffset == 0x012C9468
            && Projection.CleanupCounterEntryRawFileOffset == 0x012C942F
            && Projection.CleanupCounterNonzeroTargetRawFileOffset == 0x012C9441
            && Projection.CleanupCounterBoundaryRawFileOffsetExclusive == 0x012C943C
            && Projection.CleanupCounterVirtualAddress == 0x01EDADE8
            && CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                == 0x01EDADC8);

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan five =
            Project(0x100, 1, 0x300, 0, 0, null, 0x310, 0x3000, 5);
        check("Only the exact four committed parent stores admit a fresh DEC and nonzero JNZ",
            five.ExactParentStoreCompletion
            && five.ParentStorePlan.Writes.Length == 4
            && five.ParentStorePlan.Writes.All(write => write.Attempted && write.Committed)
            && five.ParentStorePlan.CleanupJumpReached
            && five.ParentStorePlan.StopAtRawFileOffsetExclusive == 0x012C942F
            && five.CleanupCounterGateEntered
            && five.CounterBefore == 5
            && five.CounterAfter == 4
            && five.CounterAccesses.Length == 2
            && five.CounterJnzTaken == true
            && five.CounterJnzDestinationRawFileOffset == 0x012C9441
            && five.StopAtRawFileOffsetExclusive == 0x012C9441
            && five.ExactBoundedCompletion
            && five.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan zero =
            Project(0x100, 1, 0x300, 0, 0, null, 0x310, 0x3000, 0);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan one =
            Project(0x100, 1, 0x300, 0, 0, null, 0x310, 0x3000, 1);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan signBit =
            Project(0x100, 1, 0x300, 0, 0, null, 0x310, 0x3000, 0x80000000);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan max =
            Project(0x100, 1, 0x300, 0, 0, null, 0x310, 0x3000, uint.MaxValue);
        check("DEC wrap, zero, sign-bit, and max values compute AF/PF/OF and preserve incoming CF",
            zero.CounterAfter == uint.MaxValue
            && zero.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zero.CounterJnzTaken == true
            && one.CounterAfter == 0
            && one.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && one.ZeroPathMovEcxReached
            && one.ZeroPathEcx == 0x01EDADC8
            && one.StopAtRawFileOffsetExclusive == 0x012C943C
            && signBit.CounterAfter == 0x7FFFFFFF
            && signBit.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && max.CounterAfter == 0xFFFFFFFE
            && max.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zero.CounterDecrementPreservedParentCarry
            && one.CounterDecrementPreservedParentCarry
            && signBit.CounterDecrementPreservedParentCarry
            && max.CounterDecrementPreservedParentCarry);

        const uint counterAddress = 0x01EDADE8;
        uint aliasedIndexedPointer = counterAddress - 0x10;
        uint aliasedESI = counterAddress - 0x18;
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan aliased =
            Project(0x100, 1, aliasedIndexedPointer, 0, 0, null,
                counterAddress, aliasedESI, 9);
        check("The captured counter is independent even when earlier stores alias its address",
            aliased.ExactParentStoreCompletion
            && aliased.ParentStorePlan.Writes.Any(write =>
                write.Address == counterAddress && write.Committed)
            && aliased.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: counterAddress,
                ValueCaptured: true,
                Value: 9
            }
            && aliased.CounterBefore == aliased.Capture.FreshCounterDword
            && aliased.CounterAfter == 8
            && aliased.OrderedMemoryEffects.TakeLast(2).Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                })
            && aliased.OrderedMemoryEffects.TakeLast(2).Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint?[] { 0x012C942F, 0x012C942F }));

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan missingCounter =
            Project(0x100, 1, 0x300, 0, 0, null, 0x310, 0x3000, null);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleFlags? parentFlags =
            missingCounter.ParentFlagsAtStop;
        check("A missing fresh counter stops before DEC execution and preserves every parent flag",
            missingCounter.ExactParentStoreCompletion
            && missingCounter.CounterReadAttempted
            && !missingCounter.CounterValueCaptured
            && !missingCounter.CounterWriteAttempted
            && missingCounter.CounterAccesses.Length == 1
            && missingCounter.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: counterAddress,
                ValueCaptured: false,
                Value: null
            }
            && parentFlags is
            {
                Carry: not null,
                Parity: not null,
                Auxiliary: not null,
                Zero: not null,
                Sign: not null,
                Overflow: not null
            }
            && missingCounter.OrderedInstructions[^1] is
            {
                RawFileOffset: 0x012C942F,
                Reached: true,
                Executed: false
            }
            && missingCounter.FlagsAtStop == parentFlags
            && missingCounter.CleanupCounterPlan?.JnzReached == false
            && !missingCounter.ExactBoundedCompletion
            && !missingCounter.ExactCleanCompletion
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan bypass =
            Project(0x100, 1, 0x300, 0, 0, null, 0x311, 0x3000, 7);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan dirty945C =
            Project(0x100, 1, 0x300, 0, 0, null, null, 0x3000, 7);
        check("Parent bounded bypasses and the 945C stop remain parent-owned, with unused captures visible",
            bypass.ParentOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget
            && bypass.StopAtRawFileOffsetExclusive == 0x012C8E76
            && bypass.ExactBoundedCompletion == bypass.ParentStorePlan.ExactBoundedCompletion
            && bypass.ExactCleanCompletion == false
            && bypass.FreshCounterCaptureProvidedButUnused
            && !bypass.CounterReadAttempted
            && dirty945C.ParentOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingFreshThresholdValueAt945C
            && dirty945C.StopAtRawFileOffsetExclusive == 0x012C945C
            && dirty945C.ExactBoundedCompletion
                == dirty945C.ParentStorePlan.ExactBoundedCompletion
            && dirty945C.FreshCounterCaptureProvidedButUnused
            && dirty945C.CleanupCounterPlan is null
            && !dirty945C.CounterReadAttempted);

        check("Parent reads and stores precede the counter access, without entering targets or an epilogue",
            five.OrderedMemoryEffects.TakeLast(2).Select(effect =>
                (effect.Kind, effect.RawFileOffset, effect.Attempted, effect.ValueCaptured, effect.Committed))
                .SequenceEqual(new[]
                {
                    (CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead, (uint?)0x012C942F, true, true, false),
                    (CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite, (uint?)0x012C942F, true, true, true)
                })
            && five.OrderedMemoryEffects.TakeWhile(effect =>
                effect.Kind is
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .ParentRead
                    or CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .ParentStore).Last(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .ParentStore).RawFileOffset == 0x012C9470
            && five.OrderedInstructions.TakeLast(7).Select(instruction =>
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
            && five.OrderedInstructions.All(instruction =>
                instruction.RawFileOffset != 0x012C9441
                && instruction.RawFileOffset != 0x012C943C)
            && !five.CounterNonzeroTargetEntered
            && !one.ZeroPathCallReached
            && !one.ZeroPathCalleeEntered
            && !five.CallReached
            && !five.CalleeEntered
            && !five.EpilogueEntered);
    }

    private static CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan Project(
        uint size,
        uint mask,
        uint? freshIndexedDWORD,
        uint? freshTag,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? freshGlobalValue,
        uint? entryESI,
        uint? freshCounterDword)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleCapture storeCapture =
            new(new(
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
                FreshGlobalValue: freshGlobalValue));
        return Projection.Project(new(storeCapture, freshCounterDword));
    }
}
