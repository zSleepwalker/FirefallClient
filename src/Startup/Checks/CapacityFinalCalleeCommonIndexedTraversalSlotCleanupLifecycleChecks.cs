using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleProjection;

internal static class CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The indexed four-store handoff targets the counter gate and stops before its call",
            Projection.EntryRawFileOffset == 0x012C93C0
            && Projection.StoreTailEntryRawFileOffset == 0x012C9468
            && Projection.CleanupCounterEntryRawFileOffset == 0x012C942F
            && Projection.CleanupCounterNonzeroTargetRawFileOffset == 0x012C9441
            && Projection.CleanupCounterBoundaryRawFileOffsetExclusive == 0x012C943C
            && Projection.CleanupCounterVirtualAddress == 0x01EDADE8
            && CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                == 0x01EDADC8
            && CapacityFinalCalleeCleanupCounterGateProjection.DirectCallTargetRawFileOffset
                == 0x012C78A0);

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan count31Slot16 =
            Project(0x80, 0x100, 31, 0, 8, 0, null, 0x110, 0x180, 7);
        check("Count 31 selects the 16-byte child slot and forwards the original loaded EDX",
            count31Slot16.ExactParentStoreCompletion
            && count31Slot16.ParentLifecycle.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && count31Slot16.ParentLifecycle.ChildFieldAddressAt93EE == 0x110
            && count31Slot16.ParentLifecycle.StoreTailEntryECX == 0x110
            && count31Slot16.ParentLifecycle.StoreTailEntryECXIsDerivedFromThresholdLeft
            && count31Slot16.ParentLifecycle.StoreTailLateEntryEDX == 0x100
            && count31Slot16.ParentLifecycle.StoreTailLateEntryEDXIsOriginalParentDerivedTagEDX
            && count31Slot16.ParentLifecycle.StoreTailLateEntryEDX != 0x80
            && count31Slot16.ParentLifecycle.StoreTailLateEntryEDX != 0
            && count31Slot16.ParentLifecycle.Writes.Length == 4
            && count31Slot16.ParentLifecycle.Writes.All(write =>
                write.Attempted && write.Committed)
            && count31Slot16.ExactCleanupCounterCompletion
            && count31Slot16.CounterBefore == 7
            && count31Slot16.CounterAfter == 6
            && count31Slot16.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan count30Slot16 =
            Project(0x90, 0x100, 30, 0x40000000, 8, 0, null, 0x108, 0x180, 6);
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan count30Slot20 =
            Project(0x94, 0xFFFFFFF8, 30, 0x00200000, 8, 0, null, 0xC, 0x180, 1);
        check("Count 30 independently selects 16- and 20-byte slots and preserves 20-byte wrapping",
            count30Slot16.ParentLifecycle.ChildPlan?.ChildFieldOffset == 16
            && count30Slot16.ParentLifecycle.ChildFieldAddressAt93EE == 0x110
            && count30Slot16.ParentLifecycle.Writes[0].Address == 0x110
            && count30Slot20.ParentLifecycle.ChildPlan?.ChildFieldOffset == 20
            && count30Slot20.ParentLifecycle.ChildFieldAddressAt93EE == 0xC
            && count30Slot20.ParentLifecycle.StoreTailEntryECX == 0xC
            && count30Slot20.ParentLifecycle.StoreTailLateEntryEDX == 0xFFFFFFF8
            && count30Slot20.ParentLifecycle.Writes[0].Address == 0xC
            && count30Slot20.ParentLifecycle.Writes[1].Value == 0xFFFFFFF8
            && count30Slot20.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .DecrementedToZero
            && count30Slot20.CleanupCounterPlan?.MovEcxReached == true
            && count30Slot20.CleanupCounterPlan?.EcxValue == 0x01EDADC8
            && count30Slot20.StopAtRawFileOffsetExclusive == 0x012C943C);

        const uint counterAddress = 0x01EDADE8;
        uint aliasedParentEDX = counterAddress - 16;
        uint aliasedESI = counterAddress - 0x18;
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan aliasedCounter =
            Project(0xB0, aliasedParentEDX, 31, 0, 8, 0, null, counterAddress,
                aliasedESI, 5);
        check("The fresh counter remains an independent observation after an aliased parent store",
            aliasedCounter.ExactParentStoreCompletion
            && aliasedCounter.ParentLifecycle.Writes[0].Address == counterAddress
            && aliasedCounter.ParentLifecycle.Writes[1].Address == counterAddress
            && aliasedCounter.ParentLifecycle.Writes[0].Value == aliasedESI
            && aliasedCounter.ParentLifecycle.Writes[1].Value == aliasedParentEDX
            && aliasedCounter.ParentLifecycle.Writes.All(write =>
                write.Value != aliasedCounter.Capture.FreshCounterDword)
            && aliasedCounter.CounterAccesses[0].Address == counterAddress
            && aliasedCounter.CounterAccesses[0].Value == 5
            && aliasedCounter.CounterBefore == 5
            && aliasedCounter.CounterAfter == 4
            && aliasedCounter.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C93C0, 0x012C93D8, 0x012C93F1, 0x012C945C })
            && aliasedCounter.OrderedReads.Select(read => read.Address).SequenceEqual(
                new uint?[]
                {
                    0xB0,
                    aliasedParentEDX + 4,
                    aliasedParentEDX + 16,
                    0x01EDADD8
                })
            && aliasedCounter.OrderedReads.Select(read => read.Value).SequenceEqual(
                new uint?[] { aliasedParentEDX, 8, 0, counterAddress })
            && aliasedCounter.OrderedMemoryEffects.Select(effect => effect.Kind).SequenceEqual(
                Enumerable.Repeat(
                        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentRead,
                        4)
                    .Concat(Enumerable.Repeat(
                        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentStore,
                        4))
                    .Concat(new[]
                    {
                        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                            .CleanupCounterRead,
                        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                            .CleanupCounterWrite
                    })));

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan missingParent =
            Project(null, 0x100, 31, 0, 8, 0, null, 0x110, 0x180, 3);
        check("A missing indexed parent does not synthesize a route to the cleanup counter",
            missingParent.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome.ParentBlocked
            && missingParent.OrderedReads.IsEmpty
            && missingParent.CleanupCounterPlan is null
            && missingParent.FreshCounterCaptureProvidedButUnused
            && !missingParent.CounterReadAttempted
            && !missingParent.ExactBoundedCompletion
            && !missingParent.ExactCleanCompletion);

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan missingIndexedRead =
            Project(0x84, null, 31, 0, 8, 0, null, 0x110, 0x180, 3);
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan missingIndexedECX =
            Project(0x86, 0x100, null, 0, 8, 0, null, 0x110, 0x180, 3);
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan missingChildRead =
            Project(0x88, 0x100, 31, 0, 8, null, null, null, 0x180, 3);
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan nonzeroChildMissingPlus4 =
            Project(0x94, 0xFFFFFFF8, 30, 0x00200000, 8, 0xFFFFFFFC, null,
                null, null, 3);
        check("Missing indexed load, ECX, and child load retain their reached-versus-executed status",
            missingIndexedRead.ParentLifecycle.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingFreshIndexedDWORDAt93C0
            && missingIndexedRead.OrderedInstructions.Single(instruction =>
                instruction.RawFileOffset == 0x012C93C0) is
                {
                    Reached: true,
                    Executed: false
                }
            && missingIndexedECX.ParentLifecycle.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingLateEntryECXAt93C2
            && missingIndexedECX.OrderedInstructions.Single(instruction =>
                instruction.RawFileOffset == 0x012C93C2) is
                {
                    Reached: true,
                    Executed: false
                }
            && missingChildRead.ParentLifecycle.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1
            && missingChildRead.ParentLifecycle.ChildPlan is
                {
                    FreshChildReadReached: true,
                    FreshChildDWORDCaptured: false
                }
            && missingChildRead.OrderedInstructions.Single(instruction =>
                instruction.RawFileOffset == 0x012C93F1) is
                {
                    Reached: true,
                    Executed: false
                }
            && missingChildRead.OrderedMemoryEffects.Single(effect =>
                effect.RawFileOffset == 0x012C93F1) is
                {
                    Attempted: true,
                    ValueCaptured: false,
                    Committed: false
                }
            && nonzeroChildMissingPlus4.ParentLifecycle.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9
            && nonzeroChildMissingPlus4.ParentLifecycle.ChildPlan is
                {
                    FreshChildDWORD: 0xFFFFFFFC,
                    FreshChildPlus4ReadReached: true,
                    FreshChildPlus4Captured: false
                }
            && nonzeroChildMissingPlus4.OrderedInstructions.Single(instruction =>
                instruction.RawFileOffset == 0x012C93F9) is
                {
                    Reached: true,
                    Executed: false
                }
            && nonzeroChildMissingPlus4.OrderedMemoryEffects.Single(effect =>
                effect.RawFileOffset == 0x012C93F9) is
                {
                    Attempted: true,
                    ValueCaptured: false,
                    Committed: false
                });

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan missingGlobal =
            Project(0xC0, 0x100, 31, 0, 8, 0, null, null, 0x180, 3);
        check("A missing global is observed at 945C and blocks stores and counter access",
            missingGlobal.ParentLifecycle.ThresholdPlan?.GlobalReadReached == true
            && missingGlobal.ParentLifecycle.ThresholdPlan?.GlobalAddress == 0x01EDADD8
            && missingGlobal.ParentLifecycle.Writes.IsEmpty
            && missingGlobal.CleanupCounterPlan is null
            && missingGlobal.FreshCounterCaptureProvidedButUnused
            && !missingGlobal.CounterReadAttempted
            && missingGlobal.OrderedMemoryEffects.Last().Kind
                == CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .ParentRead
            && missingGlobal.OrderedMemoryEffects.Last().Attempted
            && !missingGlobal.OrderedMemoryEffects.Last().ValueCaptured
            && !missingGlobal.ExactBoundedCompletion
            && !missingGlobal.ExactCleanCompletion
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan missingESI =
            Project(0xC4, 0x100, 31, 0, 8, 0, null, 0x110, null, 3);
        check("Missing late ESI attempts but cannot execute the first store or reach cleanup",
            missingESI.ParentLifecycle.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingLateEntryESIForFirstStore
            && missingESI.ParentLifecycle.Writes is
            [
                {
                    RawFileOffset: 0x012C9468,
                    Address: 0x110,
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

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan dirtyChildPlus4 =
            Project(0xC8, 0x100, 31, 0, 8, 0, 0xDEADBEEF, 0x110, 0x180, 3);
        check("A supplied unreached child plus-four prevents the 945C handoff and counter read",
            dirtyChildPlus4.ParentLifecycle.ParentLifecycle.ChildZero945CSelected
            && !dirtyChildPlus4.ParentLifecycle.ParentLifecycle.Internal945CEntered
            && dirtyChildPlus4.ParentLifecycle.ParentLifecycle
                .LateFreshChildPlus4CaptureProvidedButUnused
            && !dirtyChildPlus4.ExactParentStoreCompletion
            && dirtyChildPlus4.CleanupCounterPlan is null
            && dirtyChildPlus4.FreshCounterCaptureProvidedButUnused
            && !dirtyChildPlus4.CounterReadAttempted
            && !dirtyChildPlus4.ExactBoundedCompletion
            && !dirtyChildPlus4.ExactCleanCompletion
            && dirtyChildPlus4.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan below =
            Project(0xD0, 0x100, 31, 0, 8, 0, null, 0x111, 0x180, 4);
        check("An actually selected below-threshold outer branch stays visible but is not clean",
            below.ParentLifecycle.ThresholdBranchTaken == true
            && below.ParentLifecycle.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.ParentLifecycle.Writes.IsEmpty
            && below.CleanupCounterPlan is null
            && below.FreshCounterCaptureProvidedButUnused
            && below.ExactBoundedCompletion
            && !below.ExactCleanCompletion
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan equalBypass =
            Project(0xDC, 0x100, 31, 0, 0, null, null, 0xFFFFFFFF, 0x180, 4);
        check("The exact equal-parent bypass remains bounded without inventing a counter read",
            equalBypass.ParentLifecycle.TraversalOutcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ParentEqualBypassesTraversalTo9403
            && equalBypass.CleanupCounterPlan is null
            && equalBypass.FreshCounterCaptureProvidedButUnused
            && !equalBypass.CounterReadAttempted
            && equalBypass.ExactBoundedCompletion
            && !equalBypass.ExactCleanCompletion
            && equalBypass.StopAtRawFileOffsetExclusive == 0x012C9403);

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan missingCounter =
            Project(0xE0, 0x100, 31, 0, 8, 0, null, 0x110, 0x180, null);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags parentFlags = new(
            Carry: false,
            Parity: true,
            AuxiliaryCarry: false,
            Zero: true,
            Sign: false,
            Overflow: false);
        check("A missing fresh counter only attempts the first instruction and preserves CMP flags",
            missingCounter.ExactParentStoreCompletion
            && missingCounter.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
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
            && missingCounter.FlagsAtStop == parentFlags
            && missingCounter.ParentFlagsAtStop == parentFlags
            && missingCounter.CleanupCounterPlan?.Capture.IncomingCarryFlag == false
            && missingCounter.CleanupCounterPlan?.JnzReached == false
            && !missingCounter.ExactBoundedCompletion
            && !missingCounter.ExactCleanCompletion
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan wrapZero =
            Project(0xE4, 0x100, 31, 0, 8, 0, null, 0x110, 0x180, 0);
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan one =
            Project(0xE8, 0x100, 31, 0, 8, 0, null, 0x110, 0x180, 1);
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan max =
            Project(0xEC, 0x100, 31, 0, 8, 0, null, 0x110, 0x180, uint.MaxValue);
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan signBit =
            Project(0xF0, 0x100, 31, 0, 8, 0, null, 0x110, 0x180, 0x80000000);
        check("DEC wrap, zero, one, max, and sign-bit transitions preserve carry and compute flags",
            wrapZero.CounterAfter == uint.MaxValue
            && wrapZero.CleanupFlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && wrapZero.CleanupCounterPlan?.JnzTaken == true
            && wrapZero.StopAtRawFileOffsetExclusive == 0x012C9441
            && one.CounterAfter == 0
            && one.CleanupFlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && one.CleanupCounterPlan?.MovEcxReached == true
            && one.CleanupCounterPlan?.EcxValue == 0x01EDADC8
            && one.StopAtRawFileOffsetExclusive == 0x012C943C
            && max.CounterAfter == 0xFFFFFFFE
            && max.CleanupFlagsAfterDecrement is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && signBit.CounterAfter == 0x7FFFFFFF
            && signBit.CleanupFlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signBit.CleanupCounterPlan?.JnzTaken == true);

        check("Ordered reads, four writes, DEC, and JNZ stop before 9441, MOV's call, and callee entry",
            count31Slot16.OrderedMemoryEffects.Select(effect =>
                effect.RawFileOffset).SequenceEqual(new uint?[]
                {
                    0x012C93C0,
                    0x012C93D8,
                    0x012C93F1,
                    0x012C945C,
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C942F,
                    0x012C942F
                })
            && count31Slot16.OrderedMemoryEffects.TakeLast(2).Select(effect =>
                (effect.Attempted, effect.ValueCaptured, effect.Committed)).SequenceEqual(
                new[] { (true, true, false), (true, true, true) })
            && count31Slot16.OrderedInstructions.TakeLast(7).Select(instruction =>
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
            && count31Slot16.OrderedInstructions.TakeLast(7).All(instruction =>
                instruction.Reached && instruction.Executed)
            && !count31Slot16.OrderedInstructions.Any(instruction =>
                instruction.RawFileOffset == 0x012C9441
                || instruction.RawFileOffset == 0x012C943C)
            && count31Slot16.CleanupCounterPlan?.SelectedDestinationRawFileOffset
                == 0x012C9441
            && !count31Slot16.CounterNonzeroTargetEntered
            && count31Slot16.ExactBoundedCompletion
            && !one.ZeroPathCallReached
            && !one.ZeroPathCalleeEntered
            && !one.CounterNonzeroTargetEntered
            && one.CleanupCounterPlan?.CallReached == false
            && one.CleanupCounterPlan?.CalleeEntered == false
            && one.CleanupCounterPlan?.StopAtRawFileOffsetExclusive == 0x012C943C
            && one.OrderedInstructions[^1] is
            {
                RawFileOffset: 0x012C9437,
                Reached: true,
                Executed: true
            });
    }

    private static CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan Project(
        uint? entryEBX,
        uint? freshIndexedDWORD,
        uint? lateEntryECX,
        uint? lateEntryEDI,
        uint? freshEDXPlus4,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? freshGlobalValue,
        uint? lateEntryESI,
        uint? freshCounterDword)
    {
        var countCapture =
            new CapacityFinalCalleeCommonIndexedCountClassificationCapture(
                entryEBX,
                freshIndexedDWORD,
                lateEntryECX);
        var tagCapture = new CapacityFinalCalleeCommonIndexedTagTraversalLifecycleCapture(
            countCapture,
            lateEntryEDI,
            freshEDXPlus4,
            freshChildDWORD,
            lateFreshChildPlus4);
        var zeroSlotCapture =
            new CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleCapture(
                tagCapture,
                freshGlobalValue);
        var storesCapture =
            new CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleCapture(
                zeroSlotCapture,
                lateEntryESI);
        return Projection.Project(new(storesCapture, freshCounterDword));
    }
}
