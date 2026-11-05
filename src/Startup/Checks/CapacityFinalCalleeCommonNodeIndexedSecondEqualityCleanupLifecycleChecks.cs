using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup.CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleProjection;

internal static class
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan cleanNonzero =
            Project(9);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan cleanZero =
            Project(1);
        check("Only the exact second-equality five stores admit the fresh DEC/JNZ gate",
            Projection.EntryRawFileOffset
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleProjection
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
                    Address: 0x400C,
                    Value: 0x3000,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: true
                },
                {
                    RawFileOffset: 0x012C941F,
                    Address: 0x7008,
                    Value: 0x3000,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: true
                },
                {
                    RawFileOffset: 0x012C9422,
                    Address: 0x3008,
                    Value: 0x4000,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: true
                },
                {
                    RawFileOffset: 0x012C9425,
                    Address: 0x300C,
                    Value: 0x7000,
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
            && cleanNonzero.ParentPlan.Capture.FreshThresholdEDXPlus8 == 0x4000
            && cleanNonzero.ParentPlan.DerivedChildEDX == 0x7000
            && cleanNonzero.ParentPlan.OriginalRootEntryESI == 0x3000
            && cleanNonzero.ParentPlan.Writes[0].Address
                == unchecked(cleanNonzero.ParentPlan.Capture.FreshThresholdEDXPlus8 + 0xCu)
            && cleanNonzero.ParentPlan.Writes[0].Address != 0x300C
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

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan missingCounter =
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
            && missingCounter.ParentPlan.ExactBoundedCompletion
            && !missingCounter.ExactBoundedCompletion
            && !missingCounter.ExactCleanCompletion
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan wrappedZero =
            Project(0);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan one =
            Project(1);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan two =
            Project(2);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan belowSign =
            Project(0x7FFFFFFF);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan signBit =
            Project(0x80000000);
        check("DEC wraparound, zero, signed-boundary values, and inherited CF are exact",
            wrappedZero.CounterAfter == uint.MaxValue
            && wrappedZero.CounterJnzTaken == true
            && wrappedZero.StopAtRawFileOffsetExclusive == 0x012C9441
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
            && one.CounterJnzTaken == false
            && one.StopAtRawFileOffsetExclusive == 0x012C943C
            && one.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && two.CounterAfter == 1
            && two.CounterJnzTaken == true
            && two.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
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
            && two.CounterDecrementPreservedParentCarry
            && belowSign.CounterDecrementPreservedParentCarry
            && signBit.CounterDecrementPreservedParentCarry);

        const uint counterAddress = 0x01EDADE8;
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan aliased =
            Project(9, rootESI: counterAddress - 0x18);
        check("The counter capture is fresh after the final parent store aliases its address",
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
                    CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                    CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                })
            && aliased.OrderedMemoryEffects.TakeLast(2).Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C942F, 0x012C942F })
            && aliased.OrderedMemoryEffects.Select(effect => effect.SourceIndex)
                .SequenceEqual(Enumerable.Range(0, aliased.OrderedMemoryEffects.Length)));

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan firstBelow =
            Project(
                7,
                freshThresholdGlobalValue: 0x80000000,
                freshThresholdEDXPlus8: 0x90000000);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan secondBelow =
            Project(
                7,
                freshThresholdGlobalValue: 0x1000,
                freshThresholdEDXPlus8: 0x800);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan secondZero =
            Project(7, freshSecondChildDWORD: 0);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan secondUnequal =
            Project(7, lateFreshSecondChildPlus4: 0x109);
        check("Threshold bypasses and second-zero/equality failures retain their bounded exits",
            firstBelow.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing
            && firstBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && firstBelow.ExactBoundedCompletion
            && firstBelow.FreshCounterCaptureProvidedButUnused
            && !firstBelow.CounterReadAttempted
            && firstBelow.CleanupCounterPlan is null
            && secondBelow.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing
            && secondBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && secondBelow.ExactBoundedCompletion
            && secondBelow.FreshCounterCaptureProvidedButUnused
            && !secondBelow.CounterReadAttempted
            && secondZero.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .SecondZeroChildTo945C
            && secondZero.StopAtRawFileOffsetExclusive == 0x012C945C
            && secondZero.ExactBoundedCompletion
            && secondZero.FreshCounterCaptureProvidedButUnused
            && !secondZero.CounterReadAttempted
            && secondUnequal.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .SecondMaskedChildDiffersFromEntryEDITo93E4
            && secondUnequal.StopAtRawFileOffsetExclusive == 0x012C93E4
            && secondUnequal.ExactBoundedCompletion
            && secondUnequal.FreshCounterCaptureProvidedButUnused
            && !secondUnequal.CounterReadAttempted
            && new[]
            {
                firstBelow,
                secondBelow,
                secondZero,
                secondUnequal
            }.All(plan => !plan.ExactCleanCompletion));

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan dirtyParent =
            Project(7, size: 0, mask: 0);
        check("A dirty earlier parent retains its actual bound and never consumes the counter",
            dirtyParent.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C93AC
            && dirtyParent.ParentPlan.ExactBoundedCompletion
            && dirtyParent.StopAtRawFileOffsetExclusive == 0x012C93AC
            && dirtyParent.ExactBoundedCompletion
            && dirtyParent.FreshCounterCaptureProvidedButUnused
            && !dirtyParent.CounterReadAttempted
            && dirtyParent.CleanupCounterPlan is null);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan missingChild =
            Project(7, freshSecondChildDWORD: null, lateFreshSecondChildPlus4: null);
        check("A missing second child does not manufacture a completed cleanup suffix",
            !missingChild.ExactParentStoreCompletion
            && !missingChild.ExactBoundedCompletion
            && !missingChild.ExactCleanCompletion
            && missingChild.FreshCounterCaptureProvidedButUnused
            && !missingChild.CounterReadAttempted
            && missingChild.CleanupCounterPlan is null
            && missingChild.StopAtRawFileOffsetExclusive
                == missingChild.ParentPlan.StopAtRawFileOffsetExclusive);

        check("The combined trace preserves parent order and ends before either selected target or call",
            cleanNonzero.OrderedMemoryEffects.Take(cleanNonzero.ParentPlan.OrderedMemoryEffects.Length)
                .Select(effect => (effect.RawFileOffset, effect.Address, effect.Value,
                    effect.ValueCaptured, effect.Attempted, effect.Committed))
                .SequenceEqual(cleanNonzero.ParentPlan.OrderedMemoryEffects.Select(effect =>
                    (effect.RawFileOffset, effect.Address, effect.Value,
                        effect.ValueCaptured, effect.Attempted, effect.Committed)))
            && cleanNonzero.OrderedMemoryEffects.TakeLast(7).Select(effect =>
                effect.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C941C,
                    0x012C941F,
                    0x012C9422,
                    0x012C9425,
                    0x012C9428,
                    0x012C942F,
                    0x012C942F
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
            && !cleanNonzero.EpilogueEntered
            && !cleanNonzero.NativeInstructionsExecuted);
    }

    private static CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan Project(
        uint? freshCounterDword,
        uint? freshSecondChildDWORD = 0x7000,
        uint? lateFreshSecondChildPlus4 = 0x107,
        uint? freshThresholdGlobalValue = 0x1000,
        uint? freshThresholdEDXPlus8 = 0x4000,
        uint? freshFirstChildDWORD = 0x5000,
        uint? lateFreshFirstChildPlus4 = 0x109,
        uint? rootESI = 0x3000,
        uint size = 0x100,
        uint mask = 1) =>
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleProjection.Project(new(
            ParentCapture: new(
                Traversal: new(
                    FirstTraversal: new(
                        Parent: new(
                            Parent: new(
                                EntryEDI: size,
                                LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                                EntryESI: rootESI,
                                LateFreshGlobalValue: mask),
                            FreshIndexedDWORD: 0x300,
                            FreshEDXPlus4: 0),
                        FreshChildDWORD: freshFirstChildDWORD,
                        LateFreshChildPlus4: lateFreshFirstChildPlus4),
                    FreshSecondChildDWORD: freshSecondChildDWORD,
                    LateFreshSecondChildPlus4: lateFreshSecondChildPlus4),
                FreshThresholdGlobalValue: freshThresholdGlobalValue,
                FreshThresholdEDXPlus8: freshThresholdEDXPlus8),
            FreshCounterDword: freshCounterDword));
}
