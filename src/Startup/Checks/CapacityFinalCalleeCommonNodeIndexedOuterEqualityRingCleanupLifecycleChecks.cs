using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection = Firefall.Reconstruction.Startup.CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleProjection;

internal static class CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan cleanNonzero =
            Project(9);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan cleanZero =
            Project(1);
        check("Only the exact clean outer-equality five stores admit the fresh DEC/JNZ gate",
            Projection.EntryRawFileOffset
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection
                    .EntryRawFileOffset
            && Projection.RingCompletionRawFileOffset == 0x012C942F
            && Projection.CleanupCounterEntryRawFileOffset == 0x012C942F
            && Projection.CleanupCounterNonzeroTargetRawFileOffset == 0x012C9441
            && Projection.CleanupCounterBoundaryRawFileOffsetExclusive == 0x012C943C
            && Projection.CleanupCounterVirtualAddress == 0x01EDADE8
            && CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                == 0x01EDADC8
            && cleanNonzero.ExactParentStoreCompletion
            && cleanNonzero.ParentPlan.Writes is
            [
                {
                    RawFileOffset: 0x012C941C,
                    Address: 0x0D,
                    Value: 0x3000,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: true
                },
                {
                    RawFileOffset: 0x012C941F,
                    Address: 0x208,
                    Value: 0x3000,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: true
                },
                {
                    RawFileOffset: 0x012C9422,
                    Address: 0x3008,
                    Value: 1,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: true
                },
                {
                    RawFileOffset: 0x012C9425,
                    Address: 0x300C,
                    Value: 0x200,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: true
                },
                {
                    RawFileOffset: 0x012C9428,
                    Address: 0x3018,
                    Value: 0,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: true
                }
            ]
            && cleanNonzero.CleanupCounterGateEntered
            && cleanNonzero.CounterBefore == 9
            && cleanNonzero.CounterAfter == 8
            && cleanNonzero.CounterAccesses.Length == 2
            && cleanNonzero.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                RawFileOffset: 0x012C942F,
                Address: 0x01EDADE8,
                ValueCaptured: true,
                Value: 9
            }
            && cleanNonzero.CounterAccesses[1] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Write,
                RawFileOffset: 0x012C942F,
                Address: 0x01EDADE8,
                ValueCaptured: true,
                Value: 8
            }
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

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan missingCounter =
            Project(null);
        check("A missing fresh counter stops at DEC without writing and retains the outer CMP flags",
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
                Carry: false,
                Parity: not null,
                Auxiliary: not null,
                Zero: not null,
                Sign: not null,
                Overflow: not null
            }
            && missingCounter.CleanupCounterPlan?.Capture.IncomingCarryFlag == false
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

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan wrappedZero =
            Project(0);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan one =
            Project(1);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan belowSign =
            Project(0x7FFFFFFF);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan signBit =
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

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan separateRootESI =
            Project(
                9,
                indexedDWORD: 0x9000,
                tagDWORD: 0x100,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: 0x7000,
                rootESI: 0x3000);
        check("The five stores retain root ESI independently from the outer index and child EAX",
            separateRootESI.ExactParentStoreCompletion
            && separateRootESI.ParentPlan.DerivedEntryEDX == 0x9000
            && separateRootESI.ParentPlan.ThresholdRingPlan?.ChildEntryEAX == 0x7000
            && separateRootESI.ParentPlan.ThresholdRingPlan?.ChildEntryESI == 0x3000
            && separateRootESI.ParentPlan.EntryESIInheritedFromOriginalParentCapture
            && separateRootESI.ParentPlan.Writes is
            [
                { Address: 0x700C, Value: 0x3000, Committed: true },
                { Address: 0x9008, Value: 0x3000, Committed: true },
                { Address: 0x3008, Value: 0x7000, Committed: true },
                { Address: 0x300C, Value: 0x9000, Committed: true },
                { Address: 0x3018, Value: 0, Committed: true }
            ]
            && separateRootESI.CounterBefore == 9
            && separateRootESI.ExactCleanCompletion);

        const uint counterAddress = 0x01EDADE8;
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan aliased =
            Project(9, rootESI: counterAddress - 0x18);
        check("The fresh counter remains independent when the final ring store aliases its address",
            aliased.ExactParentStoreCompletion
            && aliased.ParentPlan.Writes[^1] is
            {
                Address: counterAddress,
                Value: 0,
                Committed: true
            }
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
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                })
            && aliased.OrderedMemoryEffects.TakeLast(2).Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C942F, 0x012C942F }));

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan firstBelow =
            Project(
                7,
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 0x300,
                freshThresholdEDXPlus8: 0x400);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan secondBelow =
            Project(
                7,
                indexedDWORD: 0x400,
                tagDWORD: 0x100,
                freshThresholdGlobal: 0x300,
                freshThresholdEDXPlus8: 0x200);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan dirtyParent =
            Project(
                7,
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: 1,
                size: 0,
                mask: 0);
        check("Threshold bypasses and dirty parent stops retain bounded exits without consuming the counter",
            firstBelow.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing
            && firstBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && firstBelow.ExactBoundedCompletion
            && !firstBelow.ExactCleanCompletion
            && firstBelow.FreshCounterCaptureProvidedButUnused
            && !firstBelow.CounterReadAttempted
            && secondBelow.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing
            && secondBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && secondBelow.ExactBoundedCompletion
            && !secondBelow.ExactCleanCompletion
            && secondBelow.FreshCounterCaptureProvidedButUnused
            && !secondBelow.CounterReadAttempted
            && dirtyParent.ParentPlan.ParentOutcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentZeroToBounded93AC
            && dirtyParent.StopAtRawFileOffsetExclusive == 0x012C93AC
            && dirtyParent.ExactBoundedCompletion
            && !dirtyParent.ExactCleanCompletion
            && dirtyParent.FreshCounterCaptureProvidedButUnused
            && !dirtyParent.CounterReadAttempted
            && dirtyParent.CleanupCounterPlan is null);

        check("An unreached supplied counter differs from the executed zero-valued DEC path",
            firstBelow.FreshCounterCaptureProvidedButUnused
            && !firstBelow.CounterReadAttempted
            && firstBelow.CounterAfter is null
            && !one.FreshCounterCaptureProvidedButUnused
            && one.CounterReadAttempted
            && one.CounterAfter == 0
            && one.ZeroPathMovEcxReached);

        check("The model stops before either selected destination or the following direct call",
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
                    (CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead, true, true, false),
                    (CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
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

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan Project(
        uint? freshCounterDword,
        uint? indexedDWORD = 0x200,
        uint? tagDWORD = 0x100,
        uint? freshThresholdGlobal = 1,
        uint? freshThresholdEDXPlus8 = 1,
        uint? rootESI = 0x3000,
        uint size = 0x100,
        uint mask = 1) =>
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleProjection.Project(
            new(
                ParentCapture: new(
                    Parent: new(
                        Parent: new(
                            EntryEDI: size,
                            LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                            EntryESI: rootESI,
                            LateFreshGlobalValue: mask),
                        FreshIndexedDWORD: indexedDWORD,
                        FreshEDXPlus4: tagDWORD),
                    FreshThresholdGlobalValue: freshThresholdGlobal,
                    FreshThresholdEDXPlus8: freshThresholdEDXPlus8),
                FreshCounterDword: freshCounterDword));
}
