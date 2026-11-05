using Firefall.Reconstruction.Startup;
using System.Linq;

internal static class CapacityFinalCalleeCommonIndexedTagTraversalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition begins at the indexed count gate and shares existing exact boundaries",
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection.EntryRawFileOffset
                == 0x012C93C0
            && CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
                .ParentTagEntryRawFileOffset == 0x012C93D4
            && CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
                .TraversalEntryRawFileOffset == 0x012C93E4
            && CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
                .EqualBoundaryRawFileOffsetExclusive == 0x012C9403
            && CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
                .ZeroChildBoundaryRawFileOffsetExclusive == 0x012C945C
            && CapacityFinalCalleeCommonIndexedCountClassificationProjection
                .BoundaryRawFileOffsetExclusive
                == CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection
                    .ParentHandoffRawFileOffsetExclusive
            && CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection
                .ChildEntryRawFileOffset
                == CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.EntryRawFileOffset
            && CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                .FallThroughDestinationRawFileOffset
                == CapacityFinalCalleeCommonTraversalIterationProjection.EntryRawFileOffset
            && CapacityFinalCalleeCommonTraversalIterationProjection
                .BoundaryRawFileOffsetExclusive
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
                    .EqualBoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan missingParentPointer =
            Project(null, 0x200, 31, 0, 8, 0, 0x100);
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan missingParentCount =
            Project(0x20, 0x200, null, 0, 8, 0, 0x100);
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan missingParentTag =
            Project(0x24, 0x300, 30, 0x10, null, 0x100, 0x200);
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan missingLateEDI =
            Project(0x28, 0x400, 31, null, 8, 0x100, 0x200);
        check("Missing parent operands and tag inputs stop at their reached instruction boundaries",
            missingParentPointer.Outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome.ParentBlocked
            && missingParentPointer.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.MissingEntryEBX
            && missingParentPointer.OrderedReads.IsEmpty
            && missingParentPointer.TagPlan is null
            && missingParentPointer.TraversalPlan is null
            && missingParentPointer.FreshChildDWORDCaptureProvidedButUnused
            && missingParentPointer.LateFreshChildPlus4CaptureProvidedButUnused
            && missingParentPointer.StopAtRawFileOffsetExclusive == 0x012C93C0
            && missingParentCount.Outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome.ParentBlocked
            && missingParentCount.OrderedReads is
            [{
                Stage: CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
                    .ParentIndexedDWORDAt93C0,
                Address: 0x20,
                AddressCaptured: true,
                ReadReached: true,
                ValueCaptured: true,
                Value: 0x200
            }]
            && missingParentCount.StopAtRawFileOffsetExclusive == 0x012C93C2
            && missingParentCount.TagPlan is null
            && missingParentCount.TraversalPlan is null
            && missingParentCount.OrderedInstructions.Select(item => item.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C93C0, 0x012C93C2 })
            && missingParentTag.Outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ParentTagGateStopped
            && missingParentTag.ParentPlan.ExactParentHandoffCompleted
            && missingParentTag.TagOutcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome
                    .MissingFreshEDXPlus4
            && missingParentTag.OrderedReads is
            [{
                Stage: CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
                    .ParentIndexedDWORDAt93C0,
                Address: 0x24,
                ValueCaptured: true
            }, {
                Stage: CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
                    .ParentTagDWORDAt93D8,
                RawFileOffset: 0x012C93D8,
                Address: 0x304,
                AddressCaptured: true,
                ReadReached: true,
                ValueCaptured: false,
                Value: null
            }]
            && missingParentTag.StopAtRawFileOffsetExclusive == 0x012C93D8
            && missingParentTag.FreshChildDWORDCaptureProvidedButUnused
            && missingParentTag.LateFreshChildPlus4CaptureProvidedButUnused
            && missingParentTag.TraversalPlan is null
            && missingLateEDI.Outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ParentTagGateStopped
            && missingLateEDI.TagOutcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEDI
            && missingLateEDI.OrderedReads.Length == 1
            && missingLateEDI.StopAtRawFileOffsetExclusive == 0x012C93D6
            && missingLateEDI.TraversalPlan is null);

        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan count31ZeroChild =
            Project(0x80, 0x100, 31, 0, 8, 0, null);
        check("The equal-31 branch derives child count zero and hands one fresh zero child to 945C",
            count31ZeroChild.ParentPlan.ExactParentHandoffCompleted
            && count31ZeroChild.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && count31ZeroChild.ParentDerivedCountEAX == 0
            && count31ZeroChild.ParentDerivedTagEDX == 0x100
            && count31ZeroChild.TagOutcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome
                    .UnequalToFallThrough93E4
            && count31ZeroChild.ParentUnequal93E4Selected
            && count31ZeroChild.ParentUnequalHandoffExact
            && count31ZeroChild.TraversalEntry93E4Entered
            && count31ZeroChild.TraversalOperandsAreParentDerived
            && count31ZeroChild.TraversalEntryEBX == 0
            && count31ZeroChild.TraversalPlan?.ChildFieldOffset == 16
            && count31ZeroChild.TraversalPlan?.ChildFieldAddress == 0x110
            && count31ZeroChild.TraversalPlan?.FreshChildDWORD == 0
            && count31ZeroChild.ChildZero945CSelected
            && !count31ZeroChild.ChildZero945CEntered
            && !count31ZeroChild.ChildLoopBack93E4Entered
            && !count31ZeroChild.ChildBoundary9403Entered
            && count31ZeroChild.TraversalInheritedEDIAlreadyConsumed
            && count31ZeroChild.ExactSingleIterationEvidence
            && count31ZeroChild.ExactCleanCompletion
            && count31ZeroChild.StopAtRawFileOffsetExclusive == 0x012C945C
            && count31ZeroChild.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C93C0, 0x012C93D8, 0x012C93F1 })
            && count31ZeroChild.OrderedInstructions.Count(item =>
                item.Phase
                    == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase
                        .TraversalIteration
                && item.RawFileOffset == 0x012C93E4) == 1);

        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan equalParent =
            Project(0x84, 0x200, 31, 0, 0, null, null);
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan equalParentWithDirtyChildren =
            Project(0x84, 0x200, 31, 0, 0, 0x100, 0x200);
        check("Parent equality stops at unentered 9403 and unused traversal captures spoil cleanliness",
            equalParent.Outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ParentEqualBypassesTraversalTo9403
            && equalParent.ParentEqual9403Selected
            && !equalParent.ParentEqual9403Entered
            && equalParent.TraversalPlan is null
            && equalParent.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C93C0, 0x012C93D8 })
            && equalParent.StopAtRawFileOffsetExclusive == 0x012C9403
            && equalParent.ExactCleanCompletion
            && equalParentWithDirtyChildren.ParentEqual9403Selected
            && equalParentWithDirtyChildren.FreshChildDWORDCaptureProvidedButUnused
            && equalParentWithDirtyChildren.LateFreshChildPlus4CaptureProvidedButUnused
            && !equalParentWithDirtyChildren.ExactCleanCompletion
            && equalParentWithDirtyChildren.TraversalPlan is null);

        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan missingChildRead =
            Project(0x88, 0x100, 31, 0, 8, null, 0x4444);
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan missingChildPlus4 =
            Project(0x8A, 0x100, 31, 0, 8, 0x120, null);
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan zeroChildWithDirtyPlus4 =
            Project(0x8C, 0x100, 31, 0, 8, 0, 0xDEADBEEF);
        check("A missing child read preserves its computed address and a zero child never reads plus four",
            missingChildRead.Outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1
            && missingChildRead.TraversalPlan?.ChildFieldAddress == 0x110
            && missingChildRead.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C93C0, 0x012C93D8, 0x012C93F1 })
            && missingChildRead.OrderedReads[^1].Address == 0x110
            && !missingChildRead.OrderedReads[^1].ValueCaptured
            && missingChildRead.LateFreshChildPlus4CaptureProvidedButUnused
            && missingChildRead.StopAtRawFileOffsetExclusive == 0x012C93F1
            && !missingChildRead.ExactCleanCompletion
            && missingChildPlus4.Outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9
            && missingChildPlus4.TraversalPlan?.FreshChildPlus4ReadReached == true
            && missingChildPlus4.TraversalPlan?.FreshChildPlus4Address == 0x124
            && missingChildPlus4.OrderedReads[^1] is
            {
                RawFileOffset: 0x012C93F9,
                Address: 0x124,
                ReadReached: true,
                ValueCaptured: false,
                Value: null
            }
            && missingChildPlus4.StopAtRawFileOffsetExclusive == 0x012C93F9
            && !missingChildPlus4.ExactCleanCompletion
            && zeroChildWithDirtyPlus4.ChildZero945CSelected
            && zeroChildWithDirtyPlus4.LateFreshChildPlus4CaptureProvidedButUnused
            && zeroChildWithDirtyPlus4.TraversalPlan?.FreshChildPlus4ReadReached == false
            && !zeroChildWithDirtyPlus4.ExactSingleIterationEvidence
            && !zeroChildWithDirtyPlus4.ExactCleanCompletion
            && zeroChildWithDirtyPlus4.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan count30ShiftedSlot16 =
            Project(0x90, 0x100, 30, 0x40000000, 8, 0, null);
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan count30ShiftedSlot20 =
            Project(0x94, 0xFFFFFFF8, 30, 0x00200000, 8, 0xFFFFFFFC, 0x00200007);
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan maskedShiftCount =
            Project(0x98, 0x100, 0xFFFFFFFF, 0x40, 8, 0, null);
        check("Non-31 count derivation, masked shifts, high-bit slots, and DWORD wrapping remain exact",
            count30ShiftedSlot16.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.NonEqualAt93D4
            && count30ShiftedSlot16.ParentDerivedCountEAX == 10
            && count30ShiftedSlot16.TagPlan?.ShiftCountMasked == 10
            && count30ShiftedSlot16.TagPlan?.ShiftResult == 0
            && count30ShiftedSlot16.TraversalPlan?.ChildFieldOffset == 16
            && count30ShiftedSlot16.TraversalPlan?.ChildFieldAddress == 0x110
            && count30ShiftedSlot16.ChildZero945CSelected
            && count30ShiftedSlot16.ExactCleanCompletion
            && maskedShiftCount.ParentDerivedCountEAX == 0x8000001A
            && maskedShiftCount.TagPlan?.ShiftCountMasked == 26
            && maskedShiftCount.TagPlan?.ShiftResult == 0
            && maskedShiftCount.TraversalPlan?.ChildFieldOffset == 16
            && maskedShiftCount.ExactCleanCompletion
            && count30ShiftedSlot20.CountPlan.SubtractResult == 10
            && count30ShiftedSlot20.TagPlan?.ShiftCountMasked == 10
            && count30ShiftedSlot20.TagPlan?.ShiftResult == 0x80000000
            && count30ShiftedSlot20.TraversalPlan?.ChildFieldOffset == 20
            && count30ShiftedSlot20.TraversalPlan?.ChildFieldAddress == 12
            && count30ShiftedSlot20.TraversalPlan?.FreshChildPlus4Address == 0
            && count30ShiftedSlot20.TraversalPlan?.MaskedChildDWORD == 0x00200000
            && count30ShiftedSlot20.TraversalPlan?.LoopBranchTaken == false
            && count30ShiftedSlot20.Outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403
            && count30ShiftedSlot20.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C93C0, 0x012C93D8, 0x012C93F1, 0x012C93F9 })
            && count30ShiftedSlot20.OrderedReads.Select(read => read.Address)
                .SequenceEqual(new uint?[] { 0x94, 0xFFFFFFFC, 12, 0 })
            && count30ShiftedSlot20.ExactSingleIterationEvidence
            && count30ShiftedSlot20.ExactCleanCompletion
            && count30ShiftedSlot20.StopAtRawFileOffsetExclusive == 0x012C9403
            && !count30ShiftedSlot20.ChildBoundary9403Entered);

        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan loopBack =
            Project(0x9C, 0x100, 31, 0, 8, 0x120, 0x11111111);
        check("A nonzero unequal child reads plus four once and selects but never enters the loop edge",
            loopBack.ParentUnequalHandoffExact
            && loopBack.Outcome
                == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4
            && loopBack.TraversalPlan?.FreshChildPlus4Address == 0x124
            && loopBack.TraversalPlan?.LoopBranchTaken == true
            && loopBack.ChildLoopBack93E4Selected
            && !loopBack.ChildLoopBack93E4Entered
            && !loopBack.TraversalPlan!.TargetEntered
            && loopBack.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C93C0, 0x012C93D8, 0x012C93F1, 0x012C93F9 })
            && loopBack.OrderedInstructions.Count(item =>
                item.RawFileOffset == 0x012C93E4) == 1
            && loopBack.ExactSingleIterationEvidence
            && loopBack.ExactCleanCompletion
            && loopBack.StopAtRawFileOffsetExclusive == 0x012C93E4);

        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan flagsAndOrder =
            Project(0xA0, 0x100, 31, 0, 8, 0, null);
        var iterationFlags = flagsAndOrder.TraversalPlan!.DoubleEBXFlags;
        check("Flags pass through the inherited EDI gate and entry MOV before iteration arithmetic",
            flagsAndOrder.ParentFlagsAtStop == flagsAndOrder.TagPlan!.CompareFlags
            && flagsAndOrder.TraversalFlagsAtEntry is
            {
                Carry: false,
                Zero: false
            }
            && flagsAndOrder.TraversalFlagsAfterEntryMove
                == flagsAndOrder.TraversalFlagsAtEntry
            && iterationFlags is
            {
                Carry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && flagsAndOrder.TraversalPlan.Events[0].Kind
                == CapacityFinalCalleeCommonTraversalIterationEventKind.MoveEBXToEAX
            && flagsAndOrder.TraversalPlan.Events[0].Flags is null
            && flagsAndOrder.TraversalPlan.Events[1].Flags == iterationFlags
            && flagsAndOrder.TraversalPlan.FlagsAtStop
                == flagsAndOrder.TraversalPlan.ChildTestFlags
            && flagsAndOrder.FlagsAtStop == flagsAndOrder.TraversalPlan.FlagsAtStop
            && flagsAndOrder.OrderedInstructions.Select(item => item.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C93C0, 0x012C93C2, 0x012C93C5, 0x012C93C7, 0x012C93C9,
                    0x012C93D4, 0x012C93D6, 0x012C93D8, 0x012C93DB, 0x012C93DE,
                    0x012C93E0, 0x012C93E2, 0x012C93E4, 0x012C93E6, 0x012C93E8,
                    0x012C93EB, 0x012C93EE, 0x012C93F1, 0x012C93F3, 0x012C93F5
                })
            && flagsAndOrder.OrderedReads.Count(read =>
                read.RawFileOffset == 0x012C93D8) == 1
            && !flagsAndOrder.TargetEntered
            && !flagsAndOrder.ChildZero945CEntered
            && !flagsAndOrder.ChildLoopBack93E4Entered
            && !flagsAndOrder.ChildBoundary9403Entered);
    }

    private static CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan Project(
        uint? entryEBX,
        uint? freshIndexedDWORD,
        uint? lateEntryECX,
        uint? lateEntryEDI,
        uint? freshEDXPlus4,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4) =>
        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection.Project(new(
            Parent: new(entryEBX, freshIndexedDWORD, lateEntryECX),
            LateEntryEDI: lateEntryEDI,
            FreshEDXPlus4: freshEDXPlus4,
            FreshChildDWORD: freshChildDWORD,
            LateFreshChildPlus4: lateFreshChildPlus4));
}
