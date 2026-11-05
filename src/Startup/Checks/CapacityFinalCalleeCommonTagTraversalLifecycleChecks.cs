using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonTagTraversalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition uses the exact parent and one-iteration child gates",
            CapacityFinalCalleeCommonTagTraversalLifecycleProjection.EntryRawFileOffset
                == 0x012C93D4
            && CapacityFinalCalleeCommonTagTraversalLifecycleProjection.ChildEntryRawFileOffset
                == 0x012C93E4
            && CapacityFinalCalleeCommonTagTraversalLifecycleProjection
                .EqualBoundaryRawFileOffsetExclusive == 0x012C9403
            && CapacityFinalCalleeCommonTagTraversalLifecycleProjection
                .ZeroChildBoundaryRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonTagTraversalLifecyclePlan missingParentEAX =
            Project(null, 0x12345678, 0x100, 0x12345678, 0, 0x4444);
        check("A missing parent count retains dirty later captures but never enters the child",
            missingParentEAX.ParentOutcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEAX
            && missingParentEAX.ParentPlan.OrderedEvents.Length == 1
            && missingParentEAX.ParentPlan.FlagsAtStop is null
            && missingParentEAX.ChildPlan is null
            && missingParentEAX.FreshChildDWORDCaptureProvidedButUnused
            && missingParentEAX.LateFreshChildPlus4CaptureProvidedButUnused
            && !missingParentEAX.ParentUnequalHandoffExact
            && !missingParentEAX.ExactCleanCompletion
            && missingParentEAX.StopAtRawFileOffsetExclusive == 0x012C93D4);

        CapacityFinalCalleeCommonTagTraversalLifecyclePlan missingParentEDI =
            Project(1, null, 0x100, 0, 0, 0x4444);
        CapacityFinalCalleeCommonTagTraversalLifecyclePlan missingParentRead =
            Project(31, 0x44, 0x100, null, 0x20, 0x4444);
        check("Partial parent operands and fresh reads remain intact when handoff is blocked",
            missingParentEDI.ParentOutcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEDI
            && missingParentEDI.ParentPlan.ECXValue == 1
            && missingParentEDI.ParentPlan.EntryEAXOperandConsumed
            && missingParentEDI.ParentPlan.FreshEDXPlus4CaptureProvidedButUnused
            && missingParentEDI.ChildPlan is null
            && missingParentEDI.StopAtRawFileOffsetExclusive == 0x012C93D6
            && missingParentRead.ParentOutcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingFreshEDXPlus4
            && missingParentRead.ParentPlan.EAXValue == 31
            && missingParentRead.ParentPlan.EBXValue == 0x44
            && missingParentRead.ParentPlan.ReadAttempts is
                [{
                    RawFileOffset: 0x012C93D8,
                    AbsoluteAddress: 0x104,
                    ReadReached: true,
                    ValueCaptured: false,
                    Value: null
                }]
            && missingParentRead.FreshChildDWORDCaptureProvidedButUnused
            && missingParentRead.LateFreshChildPlus4CaptureProvidedButUnused
            && missingParentRead.ChildPlan is null
            && missingParentRead.StopAtRawFileOffsetExclusive == 0x012C93D8);

        CapacityFinalCalleeCommonTagTraversalLifecyclePlan equal =
            Project(0, 0x12345678, 0x100, 0x1234567F, null, null);
        CapacityFinalCalleeCommonTagTraversalLifecyclePlan equalWithDirtyChildCaptures =
            Project(0, 0x12345678, 0x100, 0x1234567F, 0x200, 0x12345678);
        check("The outer equal edge bypasses traversal and extra child captures spoil clean evidence",
            equal.ParentOutcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.EqualToBounded9403
            && equal.ParentEqual9403Selected
            && equal.ParentPlan.ExactCleanBoundedDestination
            && equal.ParentEqual9403Entered == false
            && equal.ChildPlan is null
            && !equal.Internal93E4HandoffEntered
            && equal.StopAtRawFileOffsetExclusive == 0x012C9403
            && equal.ExactCleanCompletion
            && equalWithDirtyChildCaptures.ParentEqual9403Selected
            && equalWithDirtyChildCaptures.FreshChildDWORDCaptureProvidedButUnused
            && equalWithDirtyChildCaptures.LateFreshChildPlus4CaptureProvidedButUnused
            && !equalWithDirtyChildCaptures.ExactCleanCompletion
            && equalWithDirtyChildCaptures.ChildPlan is null);

        CapacityFinalCalleeCommonTagTraversalLifecyclePlan msbClearIndex =
            Project(0, 0x40000000, 0x100, 0x200, null, null);
        CapacityFinalCalleeCommonTagTraversalLifecyclePlan count31Index =
            Project(31, 1, 0x100, 0x200, null, null);
        check("The parent CL shift supplies child EBX and its MSB chooses byte offsets 16 or 20",
            msbClearIndex.ParentUnequal93E4Selected
            && msbClearIndex.ParentUnequalHandoffExact
            && msbClearIndex.Internal93E4HandoffEntered
            && msbClearIndex.ChildEntryOperandsAreParentDerived
            && msbClearIndex.ParentPlan.ShiftCountMasked == 0
            && msbClearIndex.ParentShiftedEBX == 0x40000000
            && msbClearIndex.ChildEntryEBX == 0x40000000
            && msbClearIndex.ChildPlan?.ChildFieldOffset == 16
            && msbClearIndex.ChildPlan?.ChildFieldAddress == 0x110
            && count31Index.ParentUnequalHandoffExact
            && count31Index.ParentPlan.ShiftCountMasked == 31
            && count31Index.ParentShiftedEBX == 0x80000000
            && count31Index.ChildEntryEBX == 0x80000000
            && count31Index.ChildPlan?.ChildFieldOffset == 20
            && count31Index.ChildPlan?.ChildFieldAddress == 0x114
            && msbClearIndex.ChildPlan!.FreshChildReadReached
            && count31Index.ChildPlan!.FreshChildReadReached
            && msbClearIndex.StopAtRawFileOffsetExclusive == 0x012C93F1
            && count31Index.StopAtRawFileOffsetExclusive == 0x012C93F1);

        CapacityFinalCalleeCommonTagTraversalLifecyclePlan missingChildSlot =
            Project(31, 1, 0x100, 0x200, null, 0x4444);
        check("A missing child slot preserves derived arithmetic, the read address, and flags",
            missingChildSlot.ParentUnequalHandoffExact
            && missingChildSlot.ChildEntryEBX == 0x80000000
            && missingChildSlot.ChildPlan?.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MissingFreshChildDWORDAt93F1
            && missingChildSlot.ChildPlan?.ChildFieldOffset == 20
            && missingChildSlot.ChildPlan?.ChildFieldAddress == 0x114
            && missingChildSlot.OrderedReads is
                [{
                    Phase: CapacityFinalCalleeCommonTagTraversalLifecycleReadPhase.ParentGate,
                    RawFileOffset: 0x012C93D8,
                    Address: 0x104,
                    ValueCaptured: true,
                    Value: 0x200
                }, {
                    Phase: CapacityFinalCalleeCommonTagTraversalLifecycleReadPhase.ChildIteration,
                    RawFileOffset: 0x012C93F1,
                    Address: 0x114,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null
                }]
            && missingChildSlot.LateFreshChildPlus4CaptureProvidedButUnused
            && missingChildSlot.ParentFlagsAtStop == missingChildSlot.ParentPlan.CompareFlags
            && missingChildSlot.ChildFlagsAtEntry is
            {
                Carry: false,
                Zero: false
            }
            && missingChildSlot.ChildEntryMovePreservesIncomingFlags
            && missingChildSlot.ChildFlagsAfterEntryMove
                == missingChildSlot.ChildFlagsAtEntry
            && missingChildSlot.ChildPlan?.FlagsAtStop
                == missingChildSlot.ChildPlan?.AddFourFlags
            && missingChildSlot.FlagsAtStop == missingChildSlot.ChildPlan?.AddFourFlags
            && !missingChildSlot.ExactCleanCompletion);

        CapacityFinalCalleeCommonTagTraversalLifecyclePlan zeroChild =
            Project(0, 0x12345678, 0x100, 0x87654321, 0, null);
        CapacityFinalCalleeCommonTagTraversalLifecyclePlan zeroChildWithUnusedPlus4 =
            Project(0, 0x12345678, 0x100, 0x87654321, 0, 0xDEADBEEF);
        check("The zero child branches to 945C without reading child plus four",
            zeroChild.ParentUnequalHandoffExact
            && zeroChild.ChildPlan?.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C
            && zeroChild.ChildPlan?.ChildZeroBranchTaken == true
            && zeroChild.ChildZero945CSelected
            && !zeroChild.ChildZero945CEntered
            && zeroChild.ChildPlan?.Reads.Length == 1
            && zeroChild.ChildPlan?.FreshChildDWORDCaptured == true
            && zeroChild.ChildPlan?.FreshChildDWORD == 0
            && zeroChild.LateFreshChildPlus4CaptureProvidedButUnused == false
            && zeroChild.ExactSingleIterationEvidence
            && zeroChild.ExactCleanCompletion
            && zeroChild.StopAtRawFileOffsetExclusive == 0x012C945C
            && zeroChildWithUnusedPlus4.ChildZero945CSelected
            && zeroChildWithUnusedPlus4.LateFreshChildPlus4CaptureProvidedButUnused
            && !zeroChildWithUnusedPlus4.ExactSingleIterationEvidence
            && !zeroChildWithUnusedPlus4.ExactCleanCompletion);

        CapacityFinalCalleeCommonTagTraversalLifecyclePlan childEqual =
            Project(0, 0x12345678, 0x100, 0x87654321, 0x120, 0x1234567F);
        check("The child independently masks its fresh plus-four value and reaches equal 9403",
            childEqual.ParentUnequalHandoffExact
            && childEqual.Internal93E4HandoffEntered
            && childEqual.ChildPlan?.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildEqualsEntryEDIAt9403
            && childEqual.ChildPlan?.EDXAfterChildMove == 0x120
            && childEqual.ChildPlan?.FreshChildPlus4Address == 0x124
            && childEqual.ChildPlan?.FreshChildPlus4 == 0x1234567F
            && childEqual.ChildPlan?.MaskedChildDWORD == 0x12345678
            && childEqual.ChildPlan?.CompareRight == 0x12345678
            && childEqual.ChildPlan?.LoopBranchTaken == false
            && childEqual.ChildBoundary9403Entered == false
            && childEqual.ExactSingleIterationEvidence
            && childEqual.ExactCleanCompletion
            && childEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && childEqual.ParentPlan.ReadAttempts[0].Value == 0x87654321
            && childEqual.ChildPlan!.Reads[0].Value == 0x120
            && childEqual.ChildPlan!.Reads[1].Value == 0x1234567F);

        CapacityFinalCalleeCommonTagTraversalLifecyclePlan loopBack =
            Project(0, 0x80001000, 0x100, 0x1234567F, 0x100, 0x11111111);
        check("Aliased parent and child reads stay ordered and independent with one bounded iteration",
            loopBack.ParentUnequalHandoffExact
            && loopBack.ChildPlan?.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4
            && loopBack.OrderedReads.Select(read => read.RawFileOffset).SequenceEqual(new uint?[]
            {
                0x012C93D8, 0x012C93F1, 0x012C93F9
            })
            && loopBack.OrderedReads.Select(read => read.Address).SequenceEqual(new uint?[]
            {
                0x104, 0x114, 0x104
            })
            && loopBack.OrderedReads.Select(read => read.Value).SequenceEqual(new uint?[]
            {
                0x1234567F, 0x100, 0x11111111
            })
            && loopBack.ChildPlan?.FreshChildPlus4Address == loopBack.ParentPlan.FreshReadAddress
            && loopBack.ParentPlan.ReadAttempts[0].Value
                != loopBack.ChildPlan?.Reads[1].Value
            && loopBack.OrderedInstructions.Select(item => item.RawFileOffset).SequenceEqual(new uint[]
            {
                0x012C93D4u, 0x012C93D6u, 0x012C93D8u, 0x012C93DBu,
                0x012C93DEu, 0x012C93E0u, 0x012C93E2u, 0x012C93E4u,
                0x012C93E6u, 0x012C93E8u, 0x012C93EBu, 0x012C93EEu,
                0x012C93F1u, 0x012C93F3u, 0x012C93F5u, 0x012C93F7u,
                0x012C93F9u, 0x012C93FCu, 0x012C93FFu, 0x012C9401u
            })
            && loopBack.OrderedInstructions.Count(item =>
                item.RawFileOffset == 0x012C93E4) == 1
            && loopBack.ParentUnequal93E4Selected
            && loopBack.Internal93E4HandoffEntered
            && loopBack.ChildLoopBack93E4Selected
            && !loopBack.ChildLoopBack93E4Entered
            && loopBack.ChildPlan?.LoopBranchSelectedDestinationRawFileOffset == 0x012C93E4
            && loopBack.ChildPlan?.Events.Count(item =>
                item.Kind
                    == CapacityFinalCalleeCommonTraversalIterationEventKind.MoveEBXToEAX) == 1
            && loopBack.ParentFlagsAtStop == loopBack.ParentPlan.CompareFlags
            && loopBack.ChildFlagsAtEntry is
            {
                Zero: false,
                Carry: true
            }
            && loopBack.ChildPlan?.FlagsAtStop == loopBack.ChildPlan?.CompareFlags
            && loopBack.ExactSingleIterationEvidence
            && loopBack.ExactCleanCompletion
            && loopBack.StopAtRawFileOffsetExclusive == 0x012C93E4
            && !loopBack.TargetEntered);
    }

    private static CapacityFinalCalleeCommonTagTraversalLifecyclePlan Project(
        uint? entryEAX,
        uint? entryEDI,
        uint? entryEDX,
        uint? freshEDXPlus4,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4) =>
        CapacityFinalCalleeCommonTagTraversalLifecycleProjection.Project(new(
            entryEAX,
            entryEDI,
            entryEDX,
            freshEDXPlus4,
            freshChildDWORD,
            lateFreshChildPlus4));
}
