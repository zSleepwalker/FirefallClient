using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonZeroSlotStoresLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan missingParent =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: null,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000));
        check("Missing parent ECX reads the threshold global but bypasses both late pointers",
            missingParent.ThresholdGate.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingEntryECX
            && missingParent.ThresholdGate.CompareInstructionReached
            && missingParent.ThresholdGate.GlobalReadReached
            && missingParent.ThresholdGate.Reads.Length == 1
            && missingParent.ThresholdGate.Reads[0].Address == 0x01EDADD8
            && missingParent.ThresholdGate.Reads[0].Value == 0x1000
            && missingParent.ThresholdGate.Reads[0].ValueCaptured
            && !missingParent.ThresholdGate.CompareReached
            && !missingParent.ThresholdGate.BranchReached
            && missingParent.StoreTail is null
            && !missingParent.ChildReached
            && missingParent.LateEntryESICaptureVisible
            && missingParent.LateEntryEDXCaptureVisible
            && missingParent.LateEntryESIUnusedBecauseThresholdBypassed
            && missingParent.LateEntryEDXUnusedBecauseThresholdBypassed
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan missingGlobal =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0x1100,
                FreshGlobalValue: null,
                LateEntryESI: 0x2200,
                LateEntryEDX: 0x3300));
        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan bothOperandsMissing =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: null,
                FreshGlobalValue: null,
                LateEntryESI: 0x2200,
                LateEntryEDX: 0x3300));
        check("Missing either or both CMP operands prevents child entry and keeps late captures unused",
            missingGlobal.ThresholdGate.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingFreshGlobalValue
            && missingGlobal.ThresholdGate.CompareLeft == 0x1100
            && missingGlobal.ThresholdGate.CompareRight is null
            && missingGlobal.ThresholdGate.Reads[0].AddressCaptured
            && !missingGlobal.ThresholdGate.Reads[0].ValueCaptured
            && missingGlobal.StoreTail is null
            && missingGlobal.LateEntryESIUnusedBecauseThresholdBypassed
            && missingGlobal.LateEntryEDXUnusedBecauseThresholdBypassed
            && bothOperandsMissing.ThresholdGate.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                    .MissingBothCompareOperands
            && bothOperandsMissing.StoreTail is null
            && bothOperandsMissing.LateEntryESIUnusedBecauseThresholdBypassed
            && bothOperandsMissing.LateEntryEDXUnusedBecauseThresholdBypassed);

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan below =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0x0FFF,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000));
        check("Unsigned-below branch exits at 8E76 without entering the child or consuming dirty pointers",
            below.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                    .UnsignedBelowBranchTarget
            && below.ThresholdGate.CompareLeft == 0x0FFF
            && below.ThresholdGate.CompareRight == 0x1000
            && below.ThresholdGate.CompareReached
            && below.ThresholdGate.BranchReached
            && below.ThresholdGate.BranchTaken == true
            && below.ThresholdGate.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.ThresholdGate.StopAtRawFileOffsetExclusive == 0x012C8E76
            && below.ThresholdGate.FlagsAtStop == below.ThresholdGate.CompareFlags
            && below.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && !below.ThresholdGate.TargetEntered
            && below.StoreTail is null
            && !below.ChildReached
            && below.LateEntryESICaptureVisible
            && below.LateEntryEDXCaptureVisible
            && below.LateEntryESIUnusedBecauseThresholdBypassed
            && below.LateEntryEDXUnusedBecauseThresholdBypassed
            && below.Writes.IsEmpty);
        check("Unused late pointers prevent clean completion on the bounded borrow exit",
            below.ExactBoundedCompletion && !below.ExactCleanCompletion
            && !below.DestinationEntered);

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan equal =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000));
        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan above =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0x1001,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000));
        check("Equal and unsigned-above comparisons take the 9468 fallthrough with all CMP flags",
            equal.ThresholdGate.CompareResult == 0
            && equal.ThresholdGate.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.ThresholdGate.BranchTaken == false
            && equal.ThresholdGate.BranchSelectedDestinationRawFileOffset == 0x012C9468
            && equal.ThresholdGate.StopAtRawFileOffsetExclusive == 0x012C9468
            && !equal.ThresholdGate.TargetEntered
            && equal.ChildReached
            && equal.ChildEntryECX == 0x1000
            && equal.StoreTailEntryInstructionEntered
            && above.ThresholdGate.CompareResult == 1
            && above.ThresholdGate.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && above.ThresholdGate.BranchTaken == false
            && above.ChildEntryECX == 0x1001
            && above.StoreTailEntryInstructionEntered);

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan missingESI =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: null,
                LateEntryEDX: 0x3000));
        check("Missing late ESI attempts the first store with the derived ECX address and leaves EDX unused",
            missingESI.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                    .MissingLateEntryESIForFirstStore
            && missingESI.ChildEntryECX == 0x1000
            && missingESI.ChildReached
            && missingESI.StoreTailEntryInstructionEntered
            && missingESI.FirstStoreAttempted
            && !missingESI.FirstStoreCommitted
            && missingESI.Writes is
            [
                {
                    RawFileOffset: 0x012C9468,
                    Address: 0x1000,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Attempted: true,
                    Committed: false
                }
            ]
            && !missingESI.StoreTail!.LateEntryEDXReadReached
            && missingESI.LateEntryEDXUnusedBecauseFirstStoreBlocked
            && !missingESI.LateEntryEDXUnusedBecauseThresholdBypassed
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9468);

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan missingEDX =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: null));
        check("Missing late EDX retains the committed first store and attempted second-store address",
            missingEDX.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                    .MissingLateEntryEDXForSecondStore
            && missingEDX.Writes is
            [
                {
                    RawFileOffset: 0x012C9468,
                    Address: 0x1000,
                    Value: 0x2000,
                    Attempted: true,
                    Committed: true
                },
                {
                    RawFileOffset: 0x012C946A,
                    Address: 0x2018,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Attempted: true,
                    Committed: false
                }
            ]
            && missingEDX.FirstStoreCommitted
            && missingEDX.SecondStoreAttempted
            && !missingEDX.SecondStoreCommitted
            && missingEDX.StoreTail!.LateEntryEDXReadReached
            && !missingEDX.ThirdStoreAttempted
            && !missingEDX.FourthStoreAttempted
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C946A);

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan zeroPointers =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0,
                FreshGlobalValue: 0,
                LateEntryESI: 0,
                LateEntryEDX: 0));
        check("Zero pointers are captured values and commit all four ordered zero-address stores",
            zeroPointers.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome.CompletedToCleanupJump
            && zeroPointers.Writes is
            [
                { RawFileOffset: 0x012C9468, Address: 0, Value: 0, Committed: true },
                { RawFileOffset: 0x012C946A, Address: 0x18, Value: 0, Committed: true },
                { RawFileOffset: 0x012C946D, Address: 0x0C, Value: 0, Committed: true },
                { RawFileOffset: 0x012C9470, Address: 8, Value: 0, Committed: true }
            ]
            && zeroPointers.Writes.Length == 4
            && zeroPointers.CleanupJumpReached
            && !zeroPointers.CleanupJumpTargetEntered);

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan aliasing =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0x1008,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x1000,
                LateEntryEDX: 0x1008));
        check("Aliased store destinations remain four separate writes in instruction order",
            aliasing.Writes is
            [
                { RawFileOffset: 0x012C9468, Address: 0x1008, Value: 0x1000 },
                { RawFileOffset: 0x012C946A, Address: 0x1018, Value: 0x1008 },
                { RawFileOffset: 0x012C946D, Address: 0x100C, Value: 0x1000 },
                { RawFileOffset: 0x012C9470, Address: 0x1008, Value: 0x1000 }
            ]
            && aliasing.Writes.Length == 4
            && aliasing.Writes[0].Address == aliasing.Writes[3].Address
            && aliasing.Writes
                .Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470
                }));

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan wrapped =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0xFFFFFFFC,
                FreshGlobalValue: 0xFFFFFFFC,
                LateEntryESI: 0xFFFFFFF0,
                LateEntryEDX: 0xAABBCCDD));
        check("Derived ESI store addresses wrap as unsigned DWORD values",
            wrapped.Writes is
            [
                { Address: 0xFFFFFFFC, Value: 0xFFFFFFF0 },
                { Address: 8, Value: 0xAABBCCDD },
                { Address: 0xFFFFFFFC, Value: 0xFFFFFFF0 },
                { Address: 0xFFFFFFF8, Value: 0xFFFFFFF0 }
            ]
            && wrapped.Writes.Length == 4
            && wrapped.CleanupJumpReached);

        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan complete =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000));
        check("The exact not-below gate hands off at 9468 and the ordered stores jump to unentered 942F",
            complete.ThresholdGate.CompareInstructionReached
            && complete.ThresholdGate.GlobalReadReached
            && complete.ThresholdGate.Reads.Length == 1
            && complete.ThresholdGate.Reads[0] is
            {
                RawFileOffset: 0x012C945C,
                Address: 0x01EDADD8,
                AddressCaptured: true,
                ValueCaptured: true,
                Value: 0x1000
            }
            && complete.ThresholdGate.CompareReached
            && complete.ThresholdGate.CompareLeft == 0x1000
            && complete.ThresholdGate.CompareRight == 0x1000
            && complete.ThresholdGate.BranchReached
            && complete.ThresholdGate.BranchTaken == false
            && complete.ThresholdGate.BranchFlags == complete.ThresholdGate.CompareFlags
            && complete.ThresholdGate.BranchSelectedDestinationRawFileOffset == 0x012C9468
            && complete.ThresholdGate.StopAtRawFileOffsetExclusive == 0x012C9468
            && !complete.ThresholdGate.TargetEntered
            && complete.ChildReached
            && complete.ChildEntryECX == complete.ThresholdGate.CompareLeft
            && complete.StoreTailEntryInstructionEntered
            && complete.ChildFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && complete.ChildFlagsDerivedFromParentCompare
            && !complete.ChildFlagsUserProvided
            && complete.ParentCompareFlagsPreservedThroughChild
            && complete.Writes.Length == 4
            && complete.FirstStoreCommitted
            && complete.SecondStoreCommitted
            && complete.ThirdStoreCommitted
            && complete.FourthStoreCommitted
            && complete.CleanupJumpReached
            && complete.CleanupJumpTargetRawFileOffset == 0x012C942F
            && complete.CleanupJumpTargetVirtualAddress == 0x016CA02F
            && !complete.CleanupJumpTargetEntered
            && complete.ExactBoundedCompletion
            && complete.ExactCleanCompletion
            && !complete.DestinationEntered
            && complete.StopAtRawFileOffsetExclusive == 0x012C942F
            && CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection
                .CapturedWindowBytes
                == "3B 0D D8 AD ED 01 0F 82 0E FA FF FF 89 31 89 56 18 89 76 0C 89 76 08 EB BA");
    }
}
