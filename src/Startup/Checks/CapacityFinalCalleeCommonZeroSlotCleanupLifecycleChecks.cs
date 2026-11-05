using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonZeroSlotCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan missingParent =
            CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.Project(new(
                ParentEntryECX: null,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                FreshCounterDword: 5));
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan missingGlobal =
            CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.Project(new(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: null,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                FreshCounterDword: 5));
        check("Missing parent CMP inputs prevent the cleanup gate and leave captures unused",
            missingParent.Outcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .ThresholdCompareOperandsMissing
            && missingParent.StoresLifecycle.ThresholdGate.CompareFlags is null
            && !missingParent.CleanupCounterGateEntryReached
            && !missingParent.CounterReadAttempted
            && missingParent.FreshCounterDwordCaptureUnused
            && missingParent.LateEntryESIUnusedBecauseThresholdBypassed
            && missingParent.LateEntryEDXUnusedBecauseThresholdBypassed
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C945C
            && !missingParent.CleanupCounterGate.ExactBoundedGateCompletion);
        check("A missing fresh global prevents the counter read after the parent comparison",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .ThresholdCompareOperandsMissing
            && missingGlobal.StoresLifecycle.ThresholdGate.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                    .MissingFreshGlobalValue
            && missingGlobal.StoresLifecycle.ThresholdGate.Reads.Length == 1
            && !missingGlobal.CleanupCounterGateEntryReached
            && !missingGlobal.CounterReadAttempted
            && missingGlobal.FreshCounterDwordCaptureUnused
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan below =
            CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.Project(new(
                ParentEntryECX: 0x0FFF,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                FreshCounterDword: 5));
        check("Unsigned-below borrow branches away before stores and leaves the counter untouched",
            below.Outcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .UnsignedBelowBranchTarget
            && below.ParentCompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && below.StoresLifecycle.ThresholdGate.BranchTaken == true
            && below.StoresLifecycle.ThresholdGate.BranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && below.Writes.IsEmpty
            && !below.CleanupCounterGateEntryReached
            && !below.CounterReadAttempted
            && below.FreshCounterDwordCaptureUnused
            && below.LateEntryESIUnusedBecauseThresholdBypassed
            && below.LateEntryEDXUnusedBecauseThresholdBypassed
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan missingESI =
            CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.Project(new(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: null,
                LateEntryEDX: 0x3000,
                FreshCounterDword: 5));
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan missingEDX =
            CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.Project(new(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: null,
                FreshCounterDword: 5));
        check("Missing late ESI stops at the first store without consuming EDX or the counter",
            missingESI.Outcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .MissingLateEntryESIForFirstStore
            && missingESI.Writes.Length == 1
            && missingESI.Writes[0].Attempted
            && !missingESI.Writes[0].Committed
            && missingESI.LateEntryEDXUnusedBecauseFirstStoreBlocked
            && !missingESI.CleanupCounterGateEntryReached
            && !missingESI.CounterReadAttempted
            && missingESI.FreshCounterDwordCaptureUnused
            && missingESI.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C945C, 0x012C9468 })
            && missingESI.OrderedMemoryEffects[0].Kind
                == CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Read
            && missingESI.OrderedMemoryEffects[1].Kind
                == CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Write
            && !missingESI.OrderedMemoryEffects[1].Committed
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9468);
        check("Missing late EDX retains the first committed store but bypasses the cleanup counter",
            missingEDX.Outcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .MissingLateEntryEDXForSecondStore
            && missingEDX.Writes.Length == 2
            && missingEDX.Writes[0].Committed
            && missingEDX.Writes[1].Attempted
            && !missingEDX.Writes[1].Committed
            && !missingEDX.CleanupCounterGateEntryReached
            && !missingEDX.CounterReadAttempted
            && missingEDX.FreshCounterDwordCaptureUnused
            && missingEDX.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C945C, 0x012C9468, 0x012C946A })
            && missingEDX.OrderedMemoryEffects[1].Committed
            && missingEDX.OrderedMemoryEffects[2].Attempted
            && !missingEDX.OrderedMemoryEffects[2].Committed
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C946A);

        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan missingCounter =
            CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.Project(new(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                FreshCounterDword: null));
        check("An exact store jump enters 942F, attempts a missing counter read, and retains stores",
            missingCounter.Outcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .MissingFreshCounterDword
            && missingCounter.CleanupCounterGateEntryReached
            && missingCounter.StoresLifecycle.ExactBoundedCompletion
            && missingCounter.AllFourStoresCommitted
            && missingCounter.Writes.Length == 4
            && missingCounter.Writes.All(write => write.Committed)
            && missingCounter.CleanupJumpReached
            && missingCounter.CleanupJumpTargetRawFileOffset == 0x012C942F
            && missingCounter.StoreJumpTargetStoppedBeforeEntry
            && missingCounter.CounterReadAttempted
            && !missingCounter.CounterReadValueCaptured
            && !missingCounter.CounterWriteCommitted
            && missingCounter.CleanupCounterGate.CounterAccesses.Length == 1
            && missingCounter.CleanupCounterGate.CounterAccesses[0].Kind
                == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
            && missingCounter.CleanupCounterGate.CounterAccesses[0].Address == 0x01EDADE8
            && missingCounter.ParentCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingCounter.StoresLifecycle.ParentCompareFlagsPreservedThroughChild
            && missingCounter.DerivedIncomingCarryFlag == false
            && missingCounter.IncomingCarryFlagDerivedFromParentCompare
            && missingCounter.CleanupCounterGate.FlagsAfterDecrement is null
            && missingCounter.CleanupCounterGate.CarryAtStop == false
            && missingCounter.CounterReadOccursAfterAllFourStores
            && !missingCounter.CounterWriteOccursAfterCounterRead
            && missingCounter.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C945C,
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C942F
                })
            && missingCounter.OrderedMemoryEffects.All(effect => effect.Attempted)
            && !missingCounter.OrderedMemoryEffects[0].Committed
            && missingCounter.OrderedMemoryEffects.Skip(1).Take(4).All(effect => effect.Committed)
            && missingCounter.OrderedMemoryEffects[5].Kind
                == CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Read
            && !missingCounter.OrderedMemoryEffects[5].ValueCaptured
            && missingCounter.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !missingCounter.DirectCallReached
            && !missingCounter.CalleeEntered
            && !missingCounter.CleanupCounterGate.ExactBoundedGateCompletion
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan zeroCounter =
            ProjectComplete(0);
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan oneCounter =
            ProjectComplete(1);
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan maxCounter =
            ProjectComplete(uint.MaxValue);
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan signBitCounter =
            ProjectComplete(0x80000000);
        check("DEC zero wraps to max and takes the unentered 9441 branch target",
            zeroCounter.Outcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .DecrementedNonzeroBranchTarget
            && zeroCounter.CleanupCounterGate.CounterBefore == 0
            && zeroCounter.CleanupCounterGate.CounterAfter == uint.MaxValue
            && zeroCounter.CleanupCounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zeroCounter.CleanupCounterGate.JnzTaken == true
            && zeroCounter.CleanupCounterGate.SelectedDestinationRawFileOffset == 0x012C9441
            && zeroCounter.NonzeroBranchTargetEntered == false
            && zeroCounter.CleanupCounterGate.MovEcxReached == false
            && zeroCounter.EntryRawFileOffset == 0x012C945C
            && zeroCounter.CleanupCounterGateEntryReached
            && zeroCounter.CleanupCounterGateEntryRawFileOffset == 0x012C942F
            && !zeroCounter.DestinationEntered
            && zeroCounter.ExactBoundedCompletion
            && zeroCounter.CounterReadOccursAfterAllFourStores
            && zeroCounter.CounterWriteOccursAfterCounterRead
            && zeroCounter.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C945C,
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C942F,
                    0x012C942F
                })
            && !zeroCounter.CleanupCounterGate.UnreachedFreshCounterDwordCaptureProvided
            && !zeroCounter.CleanupCounterGate.UnreachedIncomingCarryFlagCaptureProvided
            && !zeroCounter.DirectCallReached
            && !zeroCounter.CalleeEntered
            && zeroCounter.StopAtRawFileOffsetExclusive == 0x012C9441);
        check("DEC one reaches the zero fallthrough and stops before the direct call",
            oneCounter.Outcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .DecrementedToZeroBeforeCall
            && oneCounter.CleanupCounterGate.CounterBefore == 1
            && oneCounter.CleanupCounterGate.CounterAfter == 0
            && oneCounter.CleanupCounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && oneCounter.CleanupCounterGate.JnzTaken == false
            && oneCounter.CleanupCounterGate.SelectedDestinationRawFileOffset == 0x012C9437
            && oneCounter.ZeroPathFallthroughEntered
            && oneCounter.CleanupCounterGate.MovEcxReached
            && oneCounter.CleanupCounterGate.EcxValue == 0x01EDADC8
            && oneCounter.CleanupCounterGate.StopAtRawFileOffsetExclusive == 0x012C943C
            && oneCounter.ExactBoundedCompletion
            && oneCounter.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !oneCounter.DestinationEntered
            && !oneCounter.DirectCallReached
            && !oneCounter.CalleeEntered);
        check("DEC max wraps to max-minus-one and takes the unentered 9441 branch target",
            maxCounter.CleanupCounterGate.CounterBefore == uint.MaxValue
            && maxCounter.CleanupCounterGate.CounterAfter == 0xFFFFFFFE
            && maxCounter.CleanupCounterGate.JnzTaken == true
            && maxCounter.CleanupCounterGate.SelectedDestinationRawFileOffset == 0x012C9441
            && maxCounter.NonzeroBranchTargetEntered == false
            && maxCounter.StopAtRawFileOffsetExclusive == 0x012C9441);
        check("DEC across the sign bit sets OF, retains derived CF, and takes 9441",
            signBitCounter.CleanupCounterGate.CounterBefore == 0x80000000
            && signBitCounter.CleanupCounterGate.CounterAfter == 0x7FFFFFFF
            && signBitCounter.CleanupCounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signBitCounter.CleanupCounterGate.JnzTaken == true
            && signBitCounter.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan aliasing =
            CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.Project(new(
                ParentEntryECX: 0x01EDADE8,
                FreshGlobalValue: 0x01EDADE8,
                LateEntryESI: 0x01EDADE8,
                LateEntryEDX: 0xAABBCCDD,
                FreshCounterDword: 1));
        check("The counter is read fresh after ordered aliasing stores without forwarding",
            aliasing.Writes is
            [
                { RawFileOffset: 0x012C9468, Address: 0x01EDADE8, Value: 0x01EDADE8 },
                { RawFileOffset: 0x012C946A, Address: 0x01EDAE00, Value: 0xAABBCCDD },
                { RawFileOffset: 0x012C946D, Address: 0x01EDADF4, Value: 0x01EDADE8 },
                { RawFileOffset: 0x012C9470, Address: 0x01EDADF0, Value: 0x01EDADE8 }
            ]
            && aliasing.Writes.Length == 4
            && aliasing.CounterReadOccursAfterAllFourStores
            && aliasing.CounterWriteOccursAfterCounterRead
            && aliasing.CounterValueIsIndependentFreshCapture
            && aliasing.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C945C,
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C942F,
                    0x012C942F
                })
            && aliasing.OrderedMemoryEffects[1].Address == 0x01EDADE8
            && aliasing.OrderedMemoryEffects[1].Value == 0x01EDADE8
            && aliasing.OrderedMemoryEffects[5].Address == 0x01EDADE8
            && aliasing.OrderedMemoryEffects[5].Value == 1
            && aliasing.OrderedMemoryEffects[6].Value == 0
            && aliasing.CleanupCounterGate.CounterAccesses[0].Address == 0x01EDADE8
            && aliasing.CleanupCounterGate.CounterAccesses[0].Value == 1
            && aliasing.CleanupCounterGate.CounterBefore == 1
            && aliasing.CleanupCounterGate.CounterAfter == 0
            && aliasing.CleanupCounterGate.CounterAccesses[0].Kind
                == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
            && aliasing.CleanupCounterGate.CounterAccesses[1].Kind
                == CapacityFinalCalleeCleanupCounterGateAccessKind.Write
            && aliasing.CleanupCounterGate.CounterAccesses
                .All(access => access.Address == 0x01EDADE8));

        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan noGlobalRead =
            ProjectComplete(1);
        check("The zero-path immediate is not read as global memory and no destination or call is entered",
            noGlobalRead.CleanupCounterGate.CounterAccesses
                .All(access => access.Address == 0x01EDADE8)
            && noGlobalRead.CleanupCounterGate.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C942F, 0x012C9435, 0x012C9437 })
            && CapacityFinalCalleeCleanupCounterGateProjection.Instructions
                .All(instruction => instruction.PeVirtualAddressOperand != 0x01EDADC8)
            && CapacityFinalCalleeCleanupCounterGateProjection.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x012C9437)
                .ImmediateValue == 0x01EDADC8
            && noGlobalRead.CleanupCounterGate.EcxValue == 0x01EDADC8
            && !noGlobalRead.NonzeroBranchTargetEntered
            && !noGlobalRead.DirectCallReached
            && !noGlobalRead.CalleeEntered
            && noGlobalRead.CleanupCounterGate.ExactBoundedGateCompletion
            && noGlobalRead.StopAtRawFileOffsetExclusive == 0x012C943C);
    }

    private static CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan ProjectComplete(
        uint counter) =>
        CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.Project(new(
            ParentEntryECX: 0x1000,
            FreshGlobalValue: 0x1000,
            LateEntryESI: 0x2000,
            LateEntryEDX: 0x3000,
            FreshCounterDword: counter));
}
