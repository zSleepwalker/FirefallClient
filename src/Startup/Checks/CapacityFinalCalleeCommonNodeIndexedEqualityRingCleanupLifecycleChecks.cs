using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection = Firefall.Reconstruction.Startup.CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleProjection;

internal static class CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan cleanNonzero =
            Project(9);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan cleanZero =
            Project(1);
        check("Only the exact five committed ring stores admit the fresh DEC/JNZ gate",
            Projection.EntryRawFileOffset == 0x012C9354
            && Projection.RingCompletionRawFileOffset == 0x012C942F
            && Projection.CleanupCounterEntryRawFileOffset == 0x012C942F
            && Projection.CleanupCounterNonzeroTargetRawFileOffset == 0x012C9441
            && Projection.CleanupCounterBoundaryRawFileOffsetExclusive == 0x012C943C
            && Projection.CleanupCounterVirtualAddress == 0x01EDADE8
            && CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                == 0x01EDADC8
            && cleanNonzero.ExactParentStoreCompletion
            && cleanNonzero.ParentPlan.ThresholdRingPlan?.Writes is
            [
                { RawFileOffset: 0x012C941C, Attempted: true, Committed: true },
                { RawFileOffset: 0x012C941F, Attempted: true, Committed: true },
                { RawFileOffset: 0x012C9422, Attempted: true, Committed: true },
                { RawFileOffset: 0x012C9425, Attempted: true, Committed: true },
                { RawFileOffset: 0x012C9428, Attempted: true, Committed: true }
            ]
            && cleanNonzero.CleanupCounterGateEntered
            && cleanNonzero.CounterBefore == 9
            && cleanNonzero.CounterAfter == 8
            && cleanNonzero.CounterAccesses.Length == 2
            && cleanNonzero.CounterJnzTaken == true
            && cleanNonzero.CounterJnzDestinationRawFileOffset == 0x012C9441
            && cleanNonzero.StopAtRawFileOffsetExclusive == 0x012C9441
            && cleanNonzero.ExactBoundedCompletion
            && cleanNonzero.ExactCleanCompletion
            && cleanZero.CounterAfter == 0
            && cleanZero.CounterJnzTaken == false
            && cleanZero.ZeroPathMovEcxReached
            && cleanZero.ZeroPathEcx == 0x01EDADC8
            && cleanZero.StopAtRawFileOffsetExclusive == 0x012C943C
            && cleanZero.ExactBoundedCompletion
            && cleanZero.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan missingCounter =
            Project(null);
        check("A missing fresh counter stops at DEC without writing and retains the parent CMP flags",
            missingCounter.ExactParentStoreCompletion
            && missingCounter.CounterReadAttempted
            && !missingCounter.CounterValueCaptured
            && !missingCounter.CounterWriteAttempted
            && missingCounter.CounterAccesses.Length == 1
            && missingCounter.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                RawFileOffset: 0x012C942F,
                Address: 0x01EDADE8,
                ValueCaptured: false,
                Value: null
            }
            && missingCounter.ParentFlagsAtStop is
            {
                Carry: not null,
                Parity: not null,
                Auxiliary: not null,
                Zero: not null,
                Sign: not null,
                Overflow: not null
            }
            && missingCounter.CleanupCounterPlan?.Capture.IncomingCarryFlag
                == missingCounter.ParentFlagsAtStop?.Carry
            && missingCounter.FlagsAtStop == missingCounter.ParentFlagsAtStop
            && missingCounter.OrderedInstructions[^1] is
            {
                RawFileOffset: 0x012C942F,
                Reached: true,
                Executed: false
            }
            && missingCounter.CleanupCounterPlan?.JnzReached == false
            && !missingCounter.ExactBoundedCompletion
            && !missingCounter.ExactCleanCompletion
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan wrappedZero =
            Project(0);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan one =
            Project(1);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan belowSign =
            Project(0x7FFFFFFF);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan signBit =
            Project(0x80000000);
        check("DEC wraparound, zero, signed-boundary values, and inherited CF are modeled exactly",
            wrappedZero.CounterAfter == uint.MaxValue
            && wrappedZero.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
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
            && belowSign.CounterAfter == 0x7FFFFFFE
            && belowSign.FlagsAtStop is { Carry: false, Overflow: false }
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
            && wrappedZero.CounterDecrementPreservedParentCarry
            && one.CounterDecrementPreservedParentCarry
            && belowSign.CounterDecrementPreservedParentCarry
            && signBit.CounterDecrementPreservedParentCarry);

        const uint counterAddress = 0x01EDADE8;
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan aliased =
            Project(9, freshChildEDXPlus8: counterAddress - 0x0C);
        check("A fresh captured counter remains independent of an earlier ring store at its address",
            aliased.ExactParentStoreCompletion
            && aliased.ParentPlan.ThresholdRingPlan?.Writes.Any(write =>
                write.Address == counterAddress
                && write.Committed
                && write.Value != aliased.Capture.FreshCounterDword) == true
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
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                })
            && aliased.OrderedMemoryEffects.TakeLast(2).Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C942F, 0x012C942F }));

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan firstBelow =
            Project(7, freshChildGlobalValue: 0x80000000);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan secondBelow =
            Project(7, freshChildEDXPlus8: 0x800);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan cleanFirstBelow =
            Project(null, freshChildGlobalValue: 0x80000000);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan cleanSecondBelow =
            Project(null, freshChildEDXPlus8: 0x800);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan outerEqual =
            Project(7, freshIndexedDWORD: 0x200, freshTagDWORD: 0x100);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan loopBackedge =
            Project(7, freshChildPlus4: 0x109);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan zeroChild =
            Project(7, freshChildDWORD: 0);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan missingThreshold =
            Project(7, freshChildGlobalValue: null);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan dirtyOuterEqual =
            Project(7, freshIndexedDWORD: 0x200, freshTagDWORD: 0x100,
                freshChildGlobalValue: 0x1000, freshChildEDXPlus8: 0x3000);
        check("Threshold bypasses and earlier bounded stops never consume the supplied counter",
            firstBelow.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing
            && firstBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && firstBelow.ExactBoundedCompletion
            && firstBelow.ExactCleanCompletion == false
            && firstBelow.FreshCounterCaptureProvidedButUnused
            && !firstBelow.CounterReadAttempted
            && cleanFirstBelow.ExactBoundedCompletion
            && cleanFirstBelow.ExactCleanCompletion
            && cleanFirstBelow.ParentPlan.ThresholdRingPlan?.LateEntryESIProvidedButUnused == true
            && !cleanFirstBelow.CounterReadAttempted
            && secondBelow.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing
            && secondBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && secondBelow.ExactBoundedCompletion
            && secondBelow.ExactCleanCompletion == false
            && secondBelow.FreshCounterCaptureProvidedButUnused
            && !secondBelow.CounterReadAttempted
            && cleanSecondBelow.ExactBoundedCompletion
            && cleanSecondBelow.ExactCleanCompletion
            && cleanSecondBelow.ParentPlan.ThresholdRingPlan?.LateEntryESIProvidedButUnused == true
            && !cleanSecondBelow.CounterReadAttempted
            && outerEqual.ParentPlan.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ParentEqualToBounded9403
            && outerEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && outerEqual.ExactBoundedCompletion
            && outerEqual.FreshCounterCaptureProvidedButUnused
            && !outerEqual.CounterReadAttempted
            && loopBackedge.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C93E4
            && loopBackedge.ExactBoundedCompletion
            && loopBackedge.FreshCounterCaptureProvidedButUnused
            && !loopBackedge.CounterReadAttempted
            && zeroChild.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C945C
            && zeroChild.ExactBoundedCompletion
            && zeroChild.FreshCounterCaptureProvidedButUnused
            && !zeroChild.CounterReadAttempted
            && missingThreshold.StopAtRawFileOffsetExclusive == 0x012C9403
            && !missingThreshold.ExactBoundedCompletion
            && !missingThreshold.ExactCleanCompletion
            && missingThreshold.FreshCounterCaptureProvidedButUnused
            && !missingThreshold.CounterReadAttempted
            && dirtyOuterEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && dirtyOuterEqual.ExactBoundedCompletion
            && !dirtyOuterEqual.ExactCleanCompletion
            && dirtyOuterEqual.FreshCounterCaptureProvidedButUnused
            && !dirtyOuterEqual.CounterReadAttempted);

        check("Parent effects remain ordered before the fresh DEC, and selected destinations stay unentered",
            cleanNonzero.OrderedMemoryEffects.SkipLast(2).TakeLast(7).Select(effect =>
                effect.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C9403,
                    0x012C9409,
                    0x012C941C,
                    0x012C941F,
                    0x012C9422,
                    0x012C9425,
                    0x012C9428
                })
            && cleanNonzero.OrderedMemoryEffects.TakeLast(2).Select(effect =>
                (effect.Kind, effect.Attempted, effect.ValueCaptured, effect.Committed))
                .SequenceEqual(new[]
                {
                    (CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead, true, true, false),
                    (CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite, true, true, true)
                })
            && cleanNonzero.OrderedInstructions.TakeLast(7).Select(instruction =>
                instruction.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C941C,
                    0x012C941F,
                    0x012C9422,
                    0x012C9425,
                    0x012C9428,
                    0x012C942F,
                    0x012C9435
                })
            && cleanZero.OrderedInstructions[^1].RawFileOffset == 0x012C9437
            && cleanNonzero.OrderedInstructions.All(instruction =>
                instruction.RawFileOffset != 0x012C9441
                && instruction.RawFileOffset != 0x012C943C)
            && !cleanNonzero.CounterNonzeroTargetEntered
            && !cleanZero.ZeroPathCallReached
            && !cleanZero.ZeroPathCalleeEntered
            && !cleanNonzero.CallReached
            && !cleanNonzero.CalleeEntered
            && !cleanNonzero.EpilogueEntered);
    }

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan Project(
        uint? freshCounterDword,
        uint? freshChildGlobalValue = 0x1000,
        uint? freshChildEDXPlus8 = 0x3000,
        uint? freshIndexedDWORD = 0x300,
        uint? freshTagDWORD = 0,
        uint? freshChildDWORD = 0x5000,
        uint? freshChildPlus4 = 0x107,
        uint? entryESI = 0x3000) =>
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleProjection.Project(new(
            ParentCapture: new(
                Traversal: new(
                    Parent: new(
                        Parent: new(
                            EntryEDI: 0x100,
                            LateEntryEBP: 0x2000,
                            EntryESI: entryESI,
                            LateFreshGlobalValue: 1),
                        FreshIndexedDWORD: freshIndexedDWORD,
                        FreshEDXPlus4: freshTagDWORD),
                    FreshChildDWORD: freshChildDWORD,
                    LateFreshChildPlus4: freshChildPlus4),
                FreshChildGlobalValue: freshChildGlobalValue,
                FreshChildEDXPlus8: freshChildEDXPlus8),
            FreshCounterDword: freshCounterDword));
}
