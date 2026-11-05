using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonRingCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The dual-threshold fallthrough reaches the ring boundary before the cleanup DEC",
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.EntryRawFileOffset
                == 0x012C9403
            && CapacityFinalCalleeCommonRingCleanupLifecycleProjection.RingEntryRawFileOffset
                == 0x012C941C
            && CapacityFinalCalleeCommonRingCleanupLifecycleProjection.CounterEntryRawFileOffset
                == 0x012C942F
            && CapacityFinalCalleeCommonRingCleanupLifecycleProjection
                .CleanupZeroPathStopRawFileOffset == 0x012C943C
            && CapacityFinalCalleeCommonRingCleanupLifecycleProjection
                .CleanupNonzeroPathStopRawFileOffset == 0x012C9441);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan firstBorrow =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0x200,
                ParentEntryEDX: 0x100,
                FreshEDXPlus8: 0x500,
                LateEntryESI: 0x900,
                FreshCounterDword: 5));
        check("The first unsigned borrow exits before ring work and leaves the late counter unused",
            firstBorrow.Outcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .FirstUnsignedBelowBypassesChild
            && firstBorrow.ParentPlan.ParentPlan.FirstJBTaken == true
            && firstBorrow.ParentPlan.ParentPlan.SecondCompareReached == false
            && firstBorrow.ParentPlan.ChildPlan is null
            && firstBorrow.CleanupCounterPlan is null
            && firstBorrow.LateEntryESIProvidedButUnused
            && firstBorrow.LateCounterProvidedButUnused
            && firstBorrow.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new[] { 0x012C9403u, 0x012C9409u })
            && firstBorrow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan secondBorrow =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0x180,
                ParentEntryEDX: 0x200,
                FreshEDXPlus8: 0x100,
                LateEntryESI: 0x300,
                FreshCounterDword: 7));
        check("The second unsigned borrow bypasses the ring and never reaches the late counter",
            secondBorrow.Outcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .SecondUnsignedBelowBypassesChild
            && secondBorrow.ParentPlan.ParentPlan.FirstJBTaken == false
            && secondBorrow.ParentPlan.ParentPlan.SecondJBTaken == true
            && secondBorrow.ParentPlan.ParentPlan.SecondCompareFlags?.Carry == true
            && secondBorrow.ParentPlan.ChildPlan is null
            && secondBorrow.CleanupCounterPlan is null
            && secondBorrow.LateEntryESIProvidedButUnused
            && secondBorrow.LateCounterProvidedButUnused
            && !secondBorrow.CleanupCounterGateAdmitted
            && secondBorrow.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new[] { 0x012C9403u, 0x012C9409u })
            && secondBorrow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan missingGlobal =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: null,
                ParentEntryEDX: 0x100,
                FreshEDXPlus8: 0x200,
                LateEntryESI: 0x300,
                FreshCounterDword: 1));
        check("A missing parent global read retains the reached read and does not admit cleanup",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .MissingFreshGlobalValue
            && missingGlobal.ParentPlan.ChildPlan is null
            && missingGlobal.CleanupCounterPlan is null
            && missingGlobal.LateEntryESIProvidedButUnused
            && missingGlobal.LateCounterProvidedButUnused
            && missingGlobal.OrderedMemoryEffects is
                [{ RawFileOffset: 0x012C9403, Attempted: true, Committed: false }]
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9403);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan missingParentEDX =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0x80,
                ParentEntryEDX: null,
                FreshEDXPlus8: 0x20,
                LateEntryESI: 0x30,
                FreshCounterDword: 2));
        check("Missing parent EDX stops after the global read and leaves the late counter unused",
            missingParentEDX.Outcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .MissingParentEntryEDX
            && missingParentEDX.ParentPlan.ParentPlan.FreshGlobalCaptured
            && !missingParentEDX.ParentPlan.ParentPlan.FreshEDXPlus8ReadReached
            && missingParentEDX.ParentPlan.ChildPlan is null
            && missingParentEDX.CleanupCounterPlan is null
            && missingParentEDX.LateEntryESIProvidedButUnused
            && missingParentEDX.LateCounterProvidedButUnused
            && missingParentEDX.OrderedMemoryEffects is
                [{ RawFileOffset: 0x012C9403, Kind:
                    CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind.FreshRead }]
            && missingParentEDX.StopAtRawFileOffsetExclusive == 0x012C9409);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan missingESI =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0x1000,
                ParentEntryEDX: 0x2000,
                FreshEDXPlus8: 0x3000,
                LateEntryESI: null,
                FreshCounterDword: 4));
        check("Missing late ESI retains the attempted first ring store but bypasses cleanup",
            missingESI.Outcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .MissingLateEntryESIForFirstWrite
            && missingESI.ParentPlan.ParentExactFallthroughHandoff
            && missingESI.ParentPlan.ChildPlan?.EntryESIReadReached == true
            && missingESI.ParentPlan.Writes is
                [{
                    RawFileOffset: 0x012C941C,
                    Address: 0x300C,
                    AddressCaptured: true,
                    Value: null,
                    ValueCaptured: false,
                    Committed: false
                }]
            && missingESI.ParentRingBoundaryReached == false
            && !missingESI.CleanupCounterGateAdmitted
            && missingESI.LateCounterProvidedButUnused
            && missingESI.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new[] { 0x012C9403u, 0x012C9409u, 0x012C941Cu })
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C941C);

        uint counterAddress =
            CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress;
        CapacityFinalCalleeCommonRingCleanupLifecyclePlan aliasedCounter =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0,
                ParentEntryEDX: unchecked(counterAddress - 8),
                FreshEDXPlus8: unchecked(counterAddress - 0xC),
                LateEntryESI: 1,
                FreshCounterDword: 1));
        check("An aliased counter remains an independent late capture after all five ordered stores",
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection
                .IsExactRingBoundaryHandoff(aliasedCounter.ParentPlan)
            && aliasedCounter.ParentPlan.ParentPlan.FreshGlobal == 0
            && aliasedCounter.ParentPlan.ParentPlan.SecondCompareFlags?.Carry == false
            && aliasedCounter.ParentPlan.ChildEntryEAX
                == unchecked(counterAddress - 0xC)
            && aliasedCounter.ParentPlan.Writes is
                [
                    { Address: 0x01EDADE8, Value: 1, Committed: true },
                    { Address: 0x01EDADE8, Value: 1, Committed: true },
                    { Address: 9, Value: 0x01EDADDC, Committed: true },
                    { Address: 13, Value: 0x01EDADE0, Committed: true },
                    { Address: 25, Value: 0, Committed: true }
                ]
            && aliasedCounter.CleanupCounterPlan is { } aliasedCleanup
            && aliasedCleanup.Capture.FreshCounterDword == 1
            && aliasedCleanup.Capture.IncomingCarryFlag
                == aliasedCounter.ParentPlan.ParentPlan.SecondCompareFlags?.Carry
            && aliasedCounter.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new[]
                {
                    0x012C9403u,
                    0x012C9409u,
                    0x012C941Cu,
                    0x012C941Fu,
                    0x012C9422u,
                    0x012C9425u,
                    0x012C9428u,
                    0x012C942Fu,
                    0x012C942Fu
                })
            && aliasedCounter.OrderedMemoryEffects[2].Address == counterAddress
            && aliasedCounter.OrderedMemoryEffects[3].Address == counterAddress
            && aliasedCounter.OrderedMemoryEffects[7] is
                {
                    Kind: CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                    Address: 0x01EDADE8,
                    Value: 1,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: false
                }
            && aliasedCounter.OrderedMemoryEffects[8] is
                {
                    Kind: CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite,
                    Address: 0x01EDADE8,
                    Value: 0,
                    ValueCaptured: true,
                    Attempted: true,
                    Committed: true
                }
            && aliasedCounter.ParentRingBoundaryReached
            && !aliasedCounter.ParentRingBoundaryInstructionEntered
            && aliasedCounter.CleanupCounterInstructionEntered
            && aliasedCounter.Outcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.DecrementedToZero
            && aliasedCounter.CleanupCounterPlan?.EcxValue == 0x01EDADC8
            && !aliasedCounter.LateCounterProvidedButUnused
            && aliasedCounter.CleanupCounterPlan?.FlagsAfterDecrement?.Carry == false
            && aliasedCounter.CarryAtStop == false
            && aliasedCounter.CleanupCounterPlan?.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C942F, 0x012C9435, 0x012C9437 }) == true
            && aliasedCounter.CleanupCounterPlan?.CounterAccesses.Length == 2
            && aliasedCounter.CleanupCounterPlan?.CallReached == false
            && aliasedCounter.CleanupCounterPlan?.CalleeEntered == false
            && !aliasedCounter.OrderedMemoryEffects.Any(effect =>
                effect.Address == 0x01EDADC8)
            && !aliasedCounter.DestinationEntered
            && aliasedCounter.StopAtRawFileOffsetExclusive == 0x012C943C
            && aliasedCounter.ExactCleanCompletion);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan counterMax =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0x1000,
                ParentEntryEDX: 0x2000,
                FreshEDXPlus8: 0x3000,
                LateEntryESI: 0x4000,
                FreshCounterDword: uint.MaxValue));
        check("The maximum fresh counter decrements with wrapping and stops at the unentered 9441 target",
            counterMax.Outcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.DecrementedNonzero
            && counterMax.CleanupCounterPlan is { } maxCleanup
            && maxCleanup.CounterBefore == uint.MaxValue
            && maxCleanup.CounterAfter == uint.MaxValue - 1
            && maxCleanup.JnzTaken == true
            && maxCleanup.SelectedDestinationRawFileOffset == 0x012C9441
            && !maxCleanup.MovEcxReached
            && maxCleanup.EcxValue is null
            && !maxCleanup.CallReached
            && !maxCleanup.CalleeEntered
            && maxCleanup.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C942F, 0x012C9435 })
            && !maxCleanup.ReachedInstructions.Contains(0x012C9441)
            && !counterMax.DestinationEntered
            && maxCleanup.StopAtRawFileOffsetExclusive == 0x012C9441
            && counterMax.ExactCleanCompletion);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan carryFromSecondCompare =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0x80,
                ParentEntryEDX: 0x100,
                FreshEDXPlus8: 0x100,
                LateEntryESI: 0x200,
                FreshCounterDword: 1));
        check("An admitted DEC derives and preserves the clear CF from parent CMP2",
            carryFromSecondCompare.ParentPlan.ParentPlan.FirstCompareFlags?.Carry == false
            && carryFromSecondCompare.ParentPlan.ParentPlan.SecondCompareFlags?.Carry == false
            && carryFromSecondCompare.CleanupCounterPlan is { } carryCleanup
            && carryCleanup.Capture.IncomingCarryFlag == false
            && carryCleanup.FlagsAfterDecrement is
                {
                    Carry: false,
                    Zero: true
                }
            && carryFromSecondCompare.CarryAtStop == false
            && carryFromSecondCompare.FlagsAtStop is
                {
                    Carry: false,
                    Parity: true,
                    Auxiliary: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
            && carryFromSecondCompare.ExactCleanCompletion);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan missingCounter =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0x80,
                ParentEntryEDX: 0x100,
                FreshEDXPlus8: 0x100,
                LateEntryESI: 0x200,
                FreshCounterDword: null));
        check("A missing late counter preserves all prior ring writes and parent CF without a DEC write",
            missingCounter.Outcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .MissingFreshCounterDword
            && missingCounter.ParentPlan.Writes.Length == 5
            && missingCounter.ParentPlan.Writes.All(write => write.Committed)
            && missingCounter.OrderedMemoryEffects.Take(7).All(effect => effect.Attempted)
            && missingCounter.OrderedMemoryEffects.Take(7)
                .Where(effect =>
                    effect.Kind
                        == CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind
                            .RingWrite)
                .All(effect => effect.Committed)
            && missingCounter.OrderedMemoryEffects.Skip(7).Single() is
                {
                    Kind: CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                    RawFileOffset: 0x012C942F,
                    Address: 0x01EDADE8,
                    ValueCaptured: false,
                    Attempted: true,
                    Committed: false
                }
            && missingCounter.CleanupCounterPlan?.CounterWriteReached == false
            && missingCounter.CleanupCounterPlan?.FlagsAfterDecrement is null
            && missingCounter.CleanupCounterPlan?.CarryAtStop == false
            && missingCounter.CarryAtStop == false
            && missingCounter.FlagsAtStop is
                {
                    Carry: false,
                    Parity: false,
                    Auxiliary: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && missingCounter.ParentRingBoundaryReached
            && !missingCounter.ParentRingBoundaryInstructionEntered
            && missingCounter.CleanupCounterInstructionEntered
            && !missingCounter.ExactCleanCompletion
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan zeroCounter =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0x80,
                ParentEntryEDX: 0x100,
                FreshEDXPlus8: 0x100,
                LateEntryESI: 0x200,
                FreshCounterDword: 0));
        check("DEC zero wraps to max, preserves derived CF, and takes the unentered 9441 branch",
            zeroCounter.Outcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.DecrementedNonzero
            && zeroCounter.CleanupCounterPlan is { } zeroCleanup
            && zeroCleanup.CounterAfter == uint.MaxValue
            && zeroCleanup.FlagsAfterDecrement is
                {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                }
            && zeroCleanup.JnzReached
            && zeroCleanup.JnzTaken == true
            && zeroCleanup.SelectedDestinationRawFileOffset == 0x012C9441
            && !zeroCleanup.MovEcxReached
            && zeroCleanup.EcxValue is null
            && zeroCleanup.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C942F, 0x012C9435 })
            && zeroCounter.StopAtRawFileOffsetExclusive == 0x012C9441
            && zeroCounter.ExactCleanCompletion);

        CapacityFinalCalleeCommonRingCleanupLifecyclePlan decrementOverflow =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                FreshGlobalValue: 0x80,
                ParentEntryEDX: 0x100,
                FreshEDXPlus8: 0x100,
                LateEntryESI: 0x200,
                FreshCounterDword: 0x80000000));
        check("DEC across the sign boundary sets OF while retaining the parent CMP2 flags",
            decrementOverflow.CleanupCounterPlan is { } overflowCleanup
            && overflowCleanup.CounterAfter == 0x7FFFFFFF
            && overflowCleanup.FlagsAfterDecrement is
                {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: true,
                    Zero: false,
                    Sign: false,
                    Overflow: true
                }
            && overflowCleanup.JnzTaken == true
            && overflowCleanup.SelectedDestinationRawFileOffset == 0x012C9441
            && !overflowCleanup.MovEcxReached
            && !overflowCleanup.CallReached
            && !overflowCleanup.CalleeEntered
            && decrementOverflow.FlagsAtStop is
                {
                    Carry: false,
                    Parity: true,
                    Auxiliary: true,
                    Zero: false,
                    Sign: false,
                    Overflow: true
                }
            && decrementOverflow.ExactCleanCompletion);

        check("The parent admits cleanup only after its exact clean five-store boundary",
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection
                .IsExactRingBoundaryHandoff(aliasedCounter.ParentPlan)
            && aliasedCounter.ParentPlan.Writes.Length == 5
            && aliasedCounter.ParentPlan.Writes.All(write => write.Committed)
            && aliasedCounter.ParentRingBoundaryReached
            && !aliasedCounter.ParentRingBoundaryInstructionEntered
            && aliasedCounter.CleanupCounterGateAdmitted
            && aliasedCounter.CleanupCounterInstructionEntered
            && aliasedCounter.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C942F
            && aliasedCounter.CleanupCounterPlan?.CounterReadReached == true
            && aliasedCounter.CleanupCounterPlan?.CallReached == false
            && aliasedCounter.CleanupCounterPlan?.CalleeEntered == false);
    }
}
