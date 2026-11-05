using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup.CapacityFinalCalleeCommonTraversalZeroSlotLifecycleProjection;

internal static class CapacityFinalCalleeCommonTraversalZeroSlotLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition stops at the 945C threshold boundary after one traversal",
            Projection.EntryRawFileOffset == 0x012C93D4
            && Projection.ChildEntryRawFileOffset == 0x012C93E4
            && Projection.ZeroChildBoundaryRawFileOffsetExclusive == 0x012C945C
            && Projection.ThresholdBoundaryRawFileOffsetExclusive == 0x012C9468);

        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan msbClearIndex =
            Project(0, 0x40000000, 0xFFFFFFF0, 0x100, 0, null, 1);
        check("The 16-byte child slot wraps and supplies the threshold ECX from the 93EE LEA",
            msbClearIndex.TraversalOutcome
                == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ChildZeroTo945C
            && msbClearIndex.TraversalPlan.ParentUnequalHandoffExact
            && msbClearIndex.ChildPlan?.ChildFieldOffset == 16
            && msbClearIndex.ChildFieldAddressAt93EE == 0
            && msbClearIndex.ThresholdEntryECX == 0
            && msbClearIndex.ThresholdEntryECXIsDerivedFromChildFieldAddress
            && msbClearIndex.ThresholdPlan?.Capture.EntryECX == 0
            && msbClearIndex.ExactParentZeroHandoff
            && msbClearIndex.ChildZeroBranchReached
            && msbClearIndex.ChildZeroBranchTaken
            && msbClearIndex.ChildZeroBranchDestinationRawFileOffset == 0x012C945C
            && msbClearIndex.ChildZeroStopAt945C
            && msbClearIndex.ChildZero945CEntered
            && !msbClearIndex.ChildZero945CBoundaryTargetEntered
            && !msbClearIndex.ChildPlus4ReadReached
            && msbClearIndex.Internal945CEntered
            && msbClearIndex.ThresholdCompareReached
            && msbClearIndex.ThresholdBranchTaken == true
            && msbClearIndex.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && !msbClearIndex.Outer8E76Entered
            && !msbClearIndex.Outer9468Entered
            && msbClearIndex.ExactCleanCompletion
            && msbClearIndex.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan msbSetIndex =
            Project(0, 0x80000000, 0xFFFFFFF8, 0x100, 0, null, 0xD);
        check("The 20-byte child slot independently wraps and becomes the fresh threshold ECX",
            msbSetIndex.ChildPlan?.ChildFieldOffset == 20
            && msbSetIndex.ChildFieldAddressAt93EE == 0xC
            && msbSetIndex.ThresholdEntryECX == 0xC
            && msbSetIndex.ThresholdEntryECXIsDerivedFromChildFieldAddress
            && msbSetIndex.ThresholdPlan?.CompareLeft == 0xC
            && msbSetIndex.ThresholdPlan?.CompareRight == 0xD
            && msbSetIndex.ThresholdPlan?.BranchTaken == true
            && msbSetIndex.ExactParentZeroHandoff);

        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan missingGlobal =
            Project(0, 0x40000000, 0x100, 0x200, 0, null, null);
        check("A missing fresh global preserves zero-child TEST flags and the threshold read attempt",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C
            && missingGlobal.ExactParentZeroHandoff
            && missingGlobal.Internal945CEntered
            && missingGlobal.ChildZero945CEntered
            && !missingGlobal.ChildZero945CBoundaryTargetEntered
            && missingGlobal.ThresholdPlan?.GlobalReadReached == true
            && missingGlobal.ThresholdPlan?.GlobalAddress == 0x01EDADD8
            && missingGlobal.ThresholdPlan?.CompareLeft == 0x110
            && missingGlobal.ThresholdPlan?.CompareRight is null
            && missingGlobal.ThresholdPlan?.CompareFlags is null
            && missingGlobal.ThresholdPlan?.BranchReached == false
            && missingGlobal.FlagsAtStop == missingGlobal.ChildPlan?.ChildTestFlags
            && missingGlobal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingGlobal.OrderedReads.Select(read => read.Phase).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleReadPhase.ParentGate,
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleReadPhase.ChildIteration,
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleReadPhase.ZeroSlotThreshold
            })
            && missingGlobal.OrderedReads[^1] is
            {
                RawFileOffset: 0x012C945C,
                Address: 0x01EDADD8,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null,
                ReadReached: true
            }
            && !missingGlobal.ExactCleanCompletion
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan dirtyChildPlus4 =
            Project(0, 0x40000000, 0x100, 0x200, 0, 0xDEADBEEF, 1);
        check("A supplied child plus-four makes the zero handoff inexact and blocks threshold entry",
            dirtyChildPlus4.ChildPlan?.UnreachedEntryEDICaptureProvided == true
            && dirtyChildPlus4.TraversalPlan.ParentPlan.EntryEDIOperandConsumed
            && dirtyChildPlus4.Outcome
                == CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C
            && dirtyChildPlus4.ParentZeroChildOutcome
            && dirtyChildPlus4.ChildZero945CSelected
            && !dirtyChildPlus4.ChildZero945CEntered
            && !dirtyChildPlus4.ChildZero945CBoundaryTargetEntered
            && dirtyChildPlus4.ThresholdPlan is null
            && !dirtyChildPlus4.Internal945CEntered
            && dirtyChildPlus4.FreshGlobalValueCaptureProvidedButUnused
            && !dirtyChildPlus4.ChildPlus4ReadReached
            && dirtyChildPlus4.LateFreshChildPlus4CaptureProvidedButUnused
            && dirtyChildPlus4.StopAtRawFileOffsetExclusive == 0x012C945C
            && !dirtyChildPlus4.ExactParentZeroHandoff
            && !dirtyChildPlus4.ExactCleanCompletion);

        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan equalBypass =
            Project(0, 0x12345678, 0x100, 0x1234567F, null, null, 0xFFFFFFFF);
        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan loopBack =
            Project(0, 0x80001000, 0x100, 0x1234567F, 0x100, 0x11111111, 0xFFFFFFFF);
        check("Equal and loop-back edges bypass the late threshold global as unused data",
            equalBypass.TraversalOutcome
                == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ParentEqualBypassesChildTo9403
            && equalBypass.ThresholdPlan is null
            && equalBypass.FreshGlobalValueCaptureProvidedButUnused
            && equalBypass.OrderedReads.Length == 1
            && equalBypass.FlagsAtStop == equalBypass.TraversalPlan.FlagsAtStop
            && !equalBypass.ExactCleanCompletion
            && loopBack.TraversalOutcome
                == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4
            && loopBack.TraversalPlan.ChildLoopBack93E4Selected
            && !loopBack.TraversalPlan.ChildLoopBack93E4Entered
            && loopBack.ThresholdPlan is null
            && loopBack.FreshGlobalValueCaptureProvidedButUnused
            && loopBack.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint?[] { 0x012C93D8, 0x012C93F1, 0x012C93F9 })
            && loopBack.FlagsAtStop == loopBack.TraversalPlan.FlagsAtStop
            && !loopBack.ExactCleanCompletion);

        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan parentStopped =
            Project(null, 0x40000000, 0x100, 0x200, 0, null, 0x1234);
        check("A missing parent operand stops before child and threshold reads while retaining parent state",
            parentStopped.TraversalOutcome
                == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ParentGateStopped
            && parentStopped.TraversalPlan.ParentOutcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEAX
            && parentStopped.ThresholdPlan is null
            && parentStopped.FreshGlobalValueCaptureProvidedButUnused
            && parentStopped.OrderedReads.Length
                == parentStopped.TraversalPlan.OrderedReads.Length
            && parentStopped.FlagsAtStop == parentStopped.TraversalPlan.FlagsAtStop
            && parentStopped.StopAtRawFileOffsetExclusive == 0x012C93D4
            && !parentStopped.ExactCleanCompletion);

        uint aliasAddress = 0x01EDADD8;
        uint aliasEntryEDX = aliasAddress - 16;
        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan below =
            Project(0, 0x40000000, aliasEntryEDX, 0x100, 0, null, aliasAddress + 1);
        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan equal =
            Project(0, 0x40000000, aliasEntryEDX, 0x100, 0, null, aliasAddress);
        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan above =
            Project(0, 0x40000000, aliasEntryEDX, 0x100, 0, null, aliasAddress - 1);
        check("The aliased child/global reads remain fresh and ordered before unsigned below/equal/above gates",
            below.ThresholdEntryECX == aliasAddress
            && below.ThresholdPlan?.BranchTaken == true
            && below.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.ThresholdPlan?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && below.OrderedReads.Select(read => read.Address).SequenceEqual(new uint?[]
            {
                aliasEntryEDX + 4,
                aliasAddress,
                aliasAddress
            })
            && below.OrderedReads.Select(read => read.Value).SequenceEqual(new uint?[]
            {
                0x100,
                0,
                aliasAddress + 1
            })
            && below.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(new uint?[]
            {
                0x012C93D8,
                0x012C93F1,
                0x012C945C
            })
            && below.ExactParentZeroHandoff
            && below.ExactCleanCompletion
            && equal.ThresholdPlan?.CompareResult == 0
            && equal.ThresholdPlan?.BranchTaken == false
            && equal.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C9468
            && equal.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && above.ThresholdPlan?.CompareResult == 1
            && above.ThresholdPlan?.BranchTaken == false
            && above.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C9468
            && above.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && below.OrderedInstructions.Count(instruction =>
                instruction.RawFileOffset == 0x012C93E4) == 1
            && below.OrderedInstructions.Count(instruction =>
                instruction.RawFileOffset == 0x012C945C) == 1
            && below.OrderedInstructions.All(instruction =>
                instruction.RawFileOffset != 0x012C8E76
                && instruction.RawFileOffset != 0x012C9468)
            && !below.Outer8E76Entered
            && !equal.Outer9468Entered
            && !above.Outer9468Entered);
    }

    private static CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan Project(
        uint? entryEAX,
        uint? entryEDI,
        uint? entryEDX,
        uint? freshEDXPlus4,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? freshGlobalValue) =>
        CapacityFinalCalleeCommonTraversalZeroSlotLifecycleProjection.Project(new(
            entryEAX,
            entryEDI,
            entryEDX,
            freshEDXPlus4,
            freshChildDWORD,
            lateFreshChildPlus4,
            freshGlobalValue));
}
