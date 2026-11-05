using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup.CapacityFinalCalleeCommonTraversalSlotStoresLifecycleProjection;

internal static class CapacityFinalCalleeCommonTraversalSlotStoresLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition joins the released 945C lifecycle to the 9468 store tail",
            Projection.EntryRawFileOffset == 0x012C93D4
            && Projection.ThresholdEntryRawFileOffset == 0x012C945C
            && Projection.StoreTailEntryRawFileOffset == 0x012C9468
            && Projection.BelowBranchTargetRawFileOffset == 0x012C8E76
            && Projection.CleanupJumpTargetRawFileOffset == 0x012C942F
            && Projection.CleanupJumpBoundaryRawFileOffsetExclusive == 0x012C9475);

        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan msbClearIndex =
            Project(0, 0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, 0xFFFFFFF8);
        check("The clear-MSB child offset wraps and feeds its derived address into exact CMP handoff",
            msbClearIndex.TraversalOutcome
                == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ChildZeroTo945C
            && msbClearIndex.ParentUnequalHandoffExact
            && msbClearIndex.ParentLifecycle.ParentZeroChildOutcome
            && msbClearIndex.ParentLifecycle.ChildZero945CSelected
            && msbClearIndex.ParentLifecycle.ChildZero945CEntered
            && !msbClearIndex.ParentLifecycle.ChildZero945CBoundaryTargetEntered
            && msbClearIndex.ChildPlan?.ChildFieldOffset == 16
            && msbClearIndex.ChildFieldAddressAt93EE == 0
            && msbClearIndex.ParentLifecycle.ThresholdEntryECXIsDerivedFromChildFieldAddress
            && msbClearIndex.ThresholdEntryECX == msbClearIndex.ChildFieldAddressAt93EE
            && msbClearIndex.ThresholdPlan?.CompareLeft == msbClearIndex.ChildFieldAddressAt93EE
            && msbClearIndex.ThresholdPlan?.CompareRight == 0
            && msbClearIndex.ExactParentZeroHandoff
            && msbClearIndex.ParentLifecycle.ExactCleanCompletion
            && msbClearIndex.ParentLifecycle.Internal945CEntered
            && msbClearIndex.ParentLifecycle.ThresholdCompareReached
            && msbClearIndex.ParentLifecycle.ThresholdBranchTaken == false
            && msbClearIndex.ParentLifecycle.ThresholdBranchSelectedDestinationRawFileOffset
                == 0x012C9468
            && msbClearIndex.ParentLifecycle.StopAtRawFileOffsetExclusive == 0x012C9468
            && msbClearIndex.ParentLifecycle.Outcome
                == CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildZeroUnsignedNotBelowFallthrough
            && !msbClearIndex.ParentLifecycle.FreshGlobalValueCaptureProvidedButUnused
            && msbClearIndex.ChildZeroBranchReached
            && msbClearIndex.ChildZeroBranchTaken
            && msbClearIndex.ChildZeroBranchDestinationRawFileOffset == 0x012C945C
            && msbClearIndex.ChildZeroStopAt945C
            && msbClearIndex.ChildPlan?.MoveChildToEDXReached == false
            && msbClearIndex.ParentLifecycle.TraversalPlan.ParentPlan
                .EntryEDXOperandConsumed
            && msbClearIndex.ParentLifecycle.TraversalPlan.ChildEntryOperandsAreParentDerived
            && msbClearIndex.ChildPlan?.Capture.LateEntryEDX == 0xFFFFFFF0
            && msbClearIndex.Capture.TraversalCapture.EntryEDX.HasValue
            && !msbClearIndex.ChildPlus4ReadReached
            && msbClearIndex.StoreTailEntryReached
            && msbClearIndex.StoreTailEntryInstructionEntered
            && msbClearIndex.Internal9468Entered == msbClearIndex.FirstStoreAttempted
            && msbClearIndex.StoreTailEntryECX == msbClearIndex.ChildFieldAddressAt93EE
            && msbClearIndex.StoreTailEntryECXIsDerivedFromThresholdLeft
            && msbClearIndex.StoreTailLateEntryEDX == 0xFFFFFFF0
            && msbClearIndex.StoreTailLateEntryEDXIsOriginalTraversalParentPointer
            && !msbClearIndex.StoreTailLateEntryEDXDerivedFromChild
            && msbClearIndex.StoreTailEntryESI == 0xFFFFFFF8);

        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan msbSetIndex =
            Project(0, 0x80000000, 0xFFFFFFF8, 0x100, 0, null, 0xC, 0x1000);
        check("The set-MSB child offset independently wraps to C and supplies zero-child ECX",
            msbSetIndex.ChildPlan?.ChildFieldOffset == 20
            && msbSetIndex.ChildFieldAddressAt93EE == 0xC
            && msbSetIndex.ThresholdEntryECX == 0xC
            && msbSetIndex.ParentLifecycle.ThresholdEntryECXIsDerivedFromChildFieldAddress
            && msbSetIndex.ThresholdPlan?.CompareLeft == 0xC
            && msbSetIndex.ThresholdPlan?.CompareRight == 0xC
            && msbSetIndex.ThresholdBranchTaken == false
            && msbSetIndex.StoreTailLateEntryEDX == 0xFFFFFFF8
            && msbSetIndex.StoreTailLateEntryEDXIsOriginalTraversalParentPointer
            && msbSetIndex.ExactCleanCompletion);

        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan missingESI =
            Project(0, 0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, null);
        check("Missing late ESI leaves the first derived store address known and attempts 9468",
            missingESI.ExactParentZeroHandoff
            && missingESI.ThresholdPlan?.CompareLeft == 0
            && missingESI.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C9468
            && missingESI.StoreTailEntryReached
            && missingESI.StoreTailEntryInstructionEntered
            && missingESI.Internal9468Entered
            && missingESI.Internal9468Entered == missingESI.FirstStoreAttempted
            && missingESI.Outcome
                == CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .MissingLateEntryESIForFirstStore
            && missingESI.Writes is
            [
                {
                    RawFileOffset: 0x012C9468,
                    Address: 0,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Attempted: true,
                    Committed: false
                }
            ]
            && missingESI.StoreTail?.LateEntryEDXReadReached == false
            && missingESI.StoreTail?.LateEntryEDXUnusedBecauseFirstStoreBlocked == true
            && missingESI.OrderedInstructions.TakeLast(1).Select(instruction =>
                instruction.RawFileOffset).SequenceEqual(new uint[] { 0x012C9468 })
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9468
            && !missingESI.ExactCleanCompletion);

        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan parentMissing =
            Project(null, 0x40000000, 0x100, 0x200, 0, null, 0x1234, 0x5678);
        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan dirtyChildPlus4 =
            Project(0, 0x40000000, 0x100, 0x200, 0, 0xDEADBEEF, 1, 0x5678);
        check("Missing parent input and dirty unreached child plus-four both block late ESI",
            parentMissing.TraversalOutcome
                == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ParentGateStopped
            && parentMissing.ParentLifecycle.ThresholdPlan is null
            && parentMissing.StoreTail is null
            && parentMissing.LateEntryESIUnusedBecauseThresholdBypassed
            && parentMissing.ParentLifecycle.FreshGlobalValueCaptureProvidedButUnused
            && parentMissing.StopAtRawFileOffsetExclusive == 0x012C93D4
            && dirtyChildPlus4.ChildZero945CSelected
            && dirtyChildPlus4.ParentLifecycle.Outcome
                == CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C
            && dirtyChildPlus4.Outcome
                == CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C
            && dirtyChildPlus4.ParentLifecycle.ThresholdPlan is null
            && dirtyChildPlus4.ParentLifecycle.FreshGlobalValueCaptureProvidedButUnused
            && dirtyChildPlus4.ParentLifecycle.StopAtRawFileOffsetExclusive == 0x012C945C
            && dirtyChildPlus4.StoreTail is null
            && dirtyChildPlus4.LateFreshChildPlus4CaptureProvidedButUnused
            && !dirtyChildPlus4.ExactParentZeroHandoff
            && dirtyChildPlus4.LateEntryESIUnusedBecauseThresholdBypassed
            && dirtyChildPlus4.LateEntryESIUnused
            && !dirtyChildPlus4.ExactCleanCompletion);

        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan equalBypass =
            Project(0, 0x12345678, 0x100, 0x1234567F, null, null, 0xFFFFFFFF, 0xAABBCCDD);
        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan loopBack =
            Project(0, 0x80001000, 0x100, 0x1234567F, 0x100, 0x11111111, 0xFFFFFFFF,
                0xAABBCCDD);
        check("Parent equality, loop-back, and late-global bypasses never enter the store child",
            equalBypass.TraversalOutcome
                == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ParentEqualBypassesChildTo9403
            && equalBypass.ParentLifecycle.ThresholdPlan is null
            && equalBypass.ParentLifecycle.FreshGlobalValueCaptureProvidedButUnused
            && equalBypass.StoreTail is null
            && equalBypass.LateEntryESIUnusedBecauseThresholdBypassed
            && equalBypass.OrderedReads.Length == 1
            && equalBypass.StopAtRawFileOffsetExclusive == 0x012C9403
            && loopBack.TraversalOutcome
                == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4
            && loopBack.TraversalPlan.ChildLoopBack93E4Selected
            && !loopBack.TraversalPlan.ChildLoopBack93E4Entered
            && loopBack.ParentLifecycle.ThresholdPlan is null
            && loopBack.ParentLifecycle.FreshGlobalValueCaptureProvidedButUnused
            && loopBack.StoreTail is null
            && loopBack.LateEntryESIUnusedBecauseThresholdBypassed
            && loopBack.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint?[] { 0x012C93D8, 0x012C93F1, 0x012C93F9 })
            && loopBack.StopAtRawFileOffsetExclusive == 0x012C93E4);

        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan below =
            Project(0, 0x40000000, 0xFFFFFFF0, 0x100, 0, null, 1, 0x2000);
        check("The unsigned-below exit stops at 8E76 and explicitly leaves late ESI unused",
            below.ExactParentZeroHandoff
            && below.Internal945CEntered
            && below.ThresholdCompareReached
            && below.ThresholdPlan?.CompareLeft == below.ChildFieldAddressAt93EE
            && below.ThresholdPlan?.CompareRight == 1
            && below.ThresholdBranchTaken == true
            && below.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.ThresholdPlan?.StopAtRawFileOffsetExclusive == 0x012C8E76
            && below.StoreTail is null
            && !below.Internal9468Entered
            && below.LateEntryESIUnusedBecauseExactStoreHandoffBlocked
            && below.LateEntryESIUnused
            && below.ExactBoundedCompletion
            && !below.ExactCleanCompletion
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan missingGlobal =
            Project(0, 0x40000000, 0x100, 0x200, 0, null, null, 0x2000);
        check("Missing threshold global blocks the child after its read attempt and leaves ESI unused",
            missingGlobal.ExactParentZeroHandoff
            && missingGlobal.Internal945CEntered
            && missingGlobal.ThresholdPlan?.GlobalReadReached == true
            && missingGlobal.ThresholdPlan?.GlobalAddress == 0x01EDADD8
            && missingGlobal.ThresholdPlan?.CompareLeft == 0x110
            && missingGlobal.ThresholdPlan?.CompareRight is null
            && missingGlobal.ThresholdPlan?.BranchReached == false
            && missingGlobal.StoreTail is null
            && missingGlobal.LateEntryESIUnusedBecauseExactStoreHandoffBlocked
            && missingGlobal.LateEntryESIUnused
            && !missingGlobal.ParentLifecycle.FreshGlobalValueCaptureProvidedButUnused
            && missingGlobal.FlagsAtStop
                == new CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags(
                    false, true, null, true, false, false)
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C);

        uint aliasAddress = 0x01EDADD8;
        uint aliasEntryEDX = aliasAddress - 16;
        uint aliasEntryESI = aliasAddress - 0x18;
        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan aliasing =
            Project(0, 0x40000000, aliasEntryEDX, 0x100, 0, null, aliasAddress,
                aliasEntryESI);
        check("Aliased reads and writes preserve their instruction order and original pointer values",
            aliasing.ExactParentZeroHandoff
            && aliasing.ThresholdPlan?.CompareLeft == aliasAddress
            && aliasing.ThresholdPlan?.CompareRight == aliasAddress
            && aliasing.ThresholdBranchTaken == false
            && aliasing.OrderedReads.Select(read => read.Address).SequenceEqual(new uint?[]
            {
                aliasEntryEDX + 4,
                aliasAddress,
                aliasAddress
            })
            && aliasing.OrderedReads.Select(read => read.Value).SequenceEqual(new uint?[]
            {
                0x100,
                0,
                aliasAddress
            })
            && aliasing.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint?[] { 0x012C93D8, 0x012C93F1, 0x012C945C })
            && aliasing.Writes.Length == 4
            && aliasing.Writes[0] is
            {
                RawFileOffset: 0x012C9468,
                Address: var firstAddress,
                Value: var firstValue,
                Committed: true
            }
            && firstAddress == aliasAddress
            && firstValue == aliasEntryESI
            && aliasing.Writes[1] is
            {
                RawFileOffset: 0x012C946A,
                Address: var secondAddress,
                Value: var secondValue,
                Committed: true
            }
            && secondAddress == aliasAddress
            && secondValue == aliasEntryEDX
            && aliasing.Writes[2] is
            {
                RawFileOffset: 0x012C946D,
                Address: var thirdAddress,
                Value: var thirdValue,
                Committed: true
            }
            && thirdAddress == aliasEntryESI + 0x0C
            && thirdValue == aliasEntryESI
            && aliasing.Writes[3] is
            {
                RawFileOffset: 0x012C9470,
                Address: var fourthAddress,
                Value: var fourthValue,
                Committed: true
            }
            && fourthAddress == aliasEntryESI + 8
            && fourthValue == aliasEntryESI
            && aliasing.Writes[0].Address == aliasing.Writes[1].Address
            && aliasing.Writes.Select(write => write.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C9468, 0x012C946A, 0x012C946D, 0x012C9470 }));

        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan complete =
            Project(0, 0x40000000, 0xFFFFFFF0, 0x100, 0, null, 0, 0xFFFFFFF8);
        CapacityFinalCalleeCommonZeroSlotStoreTailFlags equalFlags = new(
            Carry: false,
            Parity: true,
            AuxiliaryCarry: false,
            Zero: true,
            Sign: false,
            Overflow: false);
        CapacityFinalCalleeCommonZeroSlotThresholdGateFlags thresholdEqualFlags = new(
            Carry: false,
            Parity: true,
            AuxiliaryCarry: false,
            Zero: true,
            Sign: false,
            Overflow: false);
        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags equalLifecycleFlags = new(
            Carry: false,
            Parity: true,
            AuxiliaryCarry: false,
            Zero: true,
            Sign: false,
            Overflow: false);
        check("CMP flags survive the ordered stores and final JMP boundary with four commits",
            complete.Outcome
                == CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .CompletedToCleanupJump
            && complete.ThresholdPlan?.CompareResult == 0
            && complete.ThresholdPlan?.CompareFlags == thresholdEqualFlags
            && complete.ChildFlags == equalFlags
            && complete.ChildFlagsDerivedFromThresholdCompare
            && !complete.ChildFlagsUserProvided
            && complete.StoreTail?.MoveInstructionsPreserveFlags == true
            && complete.StoreTail?.JumpPreservesFlags == true
            && complete.ThresholdCompareFlagsPreservedThroughStoreTail
            && complete.FlagsAtStop == equalLifecycleFlags
            && complete.Writes.Length == 4
            && complete.OrderedInstructions.TakeLast(5).Select(instruction =>
                instruction.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C9473
                })
            && complete.OrderedInstructions.TakeLast(5).All(instruction =>
                instruction.Phase
                    == CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
                        .StoreTail)
            && complete.FirstStoreCommitted
            && complete.SecondStoreCommitted
            && complete.ThirdStoreCommitted
            && complete.FourthStoreCommitted
            && complete.CleanupJumpReached
            && complete.CleanupJumpTargetRawFileOffset == 0x012C942F
            && complete.CleanupJumpTargetVirtualAddress == 0x016CA02F
            && !complete.CleanupJumpTargetEntered
            && complete.CleanupJumpBoundaryReached
            && !complete.CleanupJumpBoundaryInstructionEntered
            && CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpRawFileOffset
                == 0x012C9473
            && complete.StoreTail?.BoundaryRawFileOffsetExclusive == 0x012C9475
            && complete.ExactBoundedCompletion
            && complete.ExactCleanCompletion
            && !complete.DestinationEntered
            && complete.StopAtRawFileOffsetExclusive == 0x012C942F
            && complete.Capture.LateEntryESI == 0xFFFFFFF8
            && complete.StoreTail?.Capture.LateEntryEDX == 0xFFFFFFF0);
    }

    private static CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan Project(
        uint? entryEAX,
        uint? entryEDI,
        uint? entryEDX,
        uint? freshEDXPlus4,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? freshGlobalValue,
        uint? lateEntryESI) =>
        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleProjection.Project(new(
            new(
                entryEAX,
                entryEDI,
                entryEDX,
                freshEDXPlus4,
                freshChildDWORD,
                lateFreshChildPlus4,
                freshGlobalValue),
            lateEntryESI));
}
