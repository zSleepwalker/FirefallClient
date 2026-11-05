using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleProjection;

internal static class CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The node-indexed cleanup helper and shared epilogue retain observed boundaries",
            Projection.EntryRawFileOffset == 0x012C9354
            && Projection.ChildEntryRawFileOffset == 0x012C9441
            && Projection.ZeroCounterStopRawFileOffset == 0x012C943C
            && Projection.ZeroCounterCallRawFileOffset == 0x012C943C
            && Projection.CleanupCallTargetRawFileOffset == 0x012C78A0
            && Projection.CleanupCallTargetVirtualAddress == 0x016C84A0
            && Projection.ReturnBoundaryRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan skippedCall =
            Project(counter: 2,
                testedByte: 0,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                stackPopEbp: 4,
                returnTarget: 5);
        int actualParentEffectCount = skippedCall.ParentPlan.OrderedMemoryEffects.Length;
        check("A nonzero DEC goes straight to 9441 and can return through the TEST-zero path",
            skippedCall.ParentExactNonzeroEntryHandoff
            && skippedCall.ParentPlan.ParentStorePlan.Writes.Length == 4
            && skippedCall.ParentPlan.ParentStorePlan.Writes.All(write =>
                write.Attempted && write.Committed)
            && skippedCall.ParentPlan.OrderedMemoryEffects
                .TakeLast(2)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                })
            && !skippedCall.CleanupCallReached
            && !skippedCall.ParentCleanupCallReached
            && skippedCall.Internal9441Entered
            && skippedCall.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && skippedCall.ChildReturnReached
            && skippedCall.StopAtRawFileOffsetExclusive == 0x012C945C
            && skippedCall.ExactBoundedCompletion
            && skippedCall.ExactCleanCompletion
            && skippedCall.ParentMemoryEffectCount == actualParentEffectCount
            && skippedCall.ChildEffectCount
                == skippedCall.OrderedEffects.Length - actualParentEffectCount
            && skippedCall.OrderedEffects.Length == actualParentEffectCount + 9
            && skippedCall.OrderedEffects[actualParentEffectCount].Kind
                == CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .TestByteRead
            && skippedCall.ParentMemoryEffectsCompleteBeforeChildTest
            && skippedCall.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            });

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan takenCall =
            Project(counter: 2,
                testedByte: 2,
                stackPopEdi: 0x1111,
                stackPopEsi: 0x2222,
                stackPopEbx: 0x3333,
                callCellTarget: 0x12345678,
                callCellReturnedNormally: true,
                postCallEbp: null,
                stackPopEbp: 4,
                returnTarget: 5);
        check("A TEST-nonzero child records the gated indirect call without executing its target",
            takenCall.ParentExactNonzeroEntryHandoff
            && takenCall.EpiloguePlan?.BranchTaken == false
            && takenCall.ChildCallReached
            && takenCall.OpaqueCallTargetCaptured
            && takenCall.OpaqueCallReturnedNormally == true
            && !takenCall.OpaqueCallExecuted
            && takenCall.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingPostCallEbp
            && takenCall.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && takenCall.UnreachedStackPopEbpCaptureProvided
            && !takenCall.UnreachedPostCallEbpCaptureProvided);

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan zeroCallReturn =
            Project(counter: 1,
                cleanupCallReturnedNormally: true,
                testedByte: null,
                stackPopEdi: 1,
                callCellTarget: 0x12345678);
        check("DEC zero attempts the direct helper call then reaches 9441 with unknown flags",
            zeroCallReturn.ParentExactZeroCleanupCallHandoff
            && zeroCallReturn.ParentCleanupMovEcxReached
            && !zeroCallReturn.ParentCleanupCallReached
            && zeroCallReturn.CleanupCallReached
            && zeroCallReturn.CleanupCallAttempted
            && zeroCallReturn.CleanupCallRawFileOffset == 0x012C943C
            && zeroCallReturn.CleanupCallTargetRawFileOffset == 0x012C78A0
            && zeroCallReturn.CleanupCallTargetVirtualAddress == 0x016C84A0
            && zeroCallReturn.CleanupCallReturnedNormally == true
            && !zeroCallReturn.CleanupCallExecuted
            && !zeroCallReturn.CleanupHelperEntered
            && zeroCallReturn.Internal9441Entered
            && zeroCallReturn.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte
            && zeroCallReturn.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && zeroCallReturn.OrderedEffects[zeroCallReturn.ParentMemoryEffectCount].Kind
                == CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .CleanupCall
            && zeroCallReturn.OrderedEffects[zeroCallReturn.ParentMemoryEffectCount + 1].Kind
                == CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .TestByteRead);

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan fullZeroCallSkip =
            Project(counter: 1,
                cleanupCallReturnedNormally: true,
                testedByte: 0,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                stackPopEbp: 4,
                returnTarget: 5);
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan fullZeroCallReturn =
            Project(counter: 1,
                cleanupCallReturnedNormally: true,
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678,
                callCellReturnedNormally: true,
                postCallEbp: 0x5000,
                stackPopEbp: 4,
                returnTarget: 5);
        check("A normal zero-path helper return can traverse the complete gated epilogue to RET",
            fullZeroCallSkip.CleanupCallReached
            && fullZeroCallSkip.CleanupCallReturnStatusCaptured
            && fullZeroCallSkip.ChildReturnReached
            && fullZeroCallSkip.ChildBoundedExitReached
            && fullZeroCallSkip.ExactBoundedCompletion
            && fullZeroCallSkip.ExactCleanCompletion
            && fullZeroCallSkip.StopAtRawFileOffsetExclusive == 0x012C945C
            && fullZeroCallSkip.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && fullZeroCallReturn.CleanupCallReached
            && fullZeroCallReturn.ChildReturnReached
            && fullZeroCallReturn.ExactBoundedCompletion
            && fullZeroCallReturn.ExactCleanCompletion
            && fullZeroCallReturn.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            });

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan zeroCallUnknown =
            Project(counter: 1,
                childEsp: null,
                childEbp: null,
                cleanupCallReturnedNormally: null,
                testedByte: null);
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan zeroCallFalse =
            Project(counter: 1,
                childEsp: null,
                childEbp: null,
                cleanupCallReturnedNormally: false,
                testedByte: null);
        check("Missing or non-normal cleanup-call return stops at 943C without entering 9441",
            zeroCallUnknown.CleanupCallReached
            && zeroCallUnknown.Outcome
                == CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                    .MissingCleanupCallReturnStatus
            && zeroCallUnknown.EpiloguePlan is null
            && !zeroCallUnknown.Internal9441Entered
            && zeroCallUnknown.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroCallUnknown.ExactBoundedCompletion
            && !zeroCallUnknown.ExactCleanCompletion
            && !zeroCallUnknown.CleanupCallReturnStatusCaptured
            && zeroCallUnknown.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroCallUnknown.OrderedEffects[zeroCallUnknown.ParentMemoryEffectCount] is
            {
                Kind: CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .CleanupCall,
                ValueCaptured: true,
                BooleanValue: null
            }
            && zeroCallFalse.CleanupCallReached
            && zeroCallFalse.Outcome
                == CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                    .CleanupCallDidNotReturnNormally
            && !zeroCallFalse.Internal9441Entered
            && zeroCallFalse.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroCallFalse.ExactBoundedCompletion
            && !zeroCallFalse.ExactCleanCompletion
            && !zeroCallFalse.CleanupCallExecuted
            && zeroCallFalse.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroCallFalse.OrderedEffects[zeroCallFalse.ParentMemoryEffectCount] is
            {
                Kind: CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .CleanupCall,
                ValueCaptured: true,
                BooleanValue: false
            }
            && !zeroCallFalse.UnreachedTestedByteCaptureProvided);

        var dirtySkippedCall = Projection.Project(skippedCall.Capture with
        {
            CleanupCallReturnedNormally = true
        });
        check("Unused cleanup return evidence spoils clean admission without erasing a bounded child return",
            dirtySkippedCall.UnreachedCleanupCallReturnStatusProvided
            && dirtySkippedCall.ChildBoundedExitReached
            && dirtySkippedCall.ExactBoundedCompletion
            && !dirtySkippedCall.ExactCleanCompletion
            && dirtySkippedCall.StopAtRawFileOffsetExclusive == 0x012C945C);

        const uint cleanupCounterAddress = 0x01EDADE8;
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan aliasedCounter =
            Project(counter: 9,
                freshIndexedDWORD: cleanupCounterAddress - 0x10,
                freshGlobalValue: cleanupCounterAddress,
                entryESI: cleanupCounterAddress - 0x18,
                testedByte: 0);
        check("An earlier parent store alias does not replace the fresh DEC counter read",
            aliasedCounter.ParentExactNonzeroEntryHandoff
            && aliasedCounter.ParentPlan.ParentStorePlan.Writes.Any(write =>
                write.Address == cleanupCounterAddress && write.Committed)
            && aliasedCounter.ParentCleanupCounterPlan?.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: cleanupCounterAddress,
                ValueCaptured: true,
                Value: 9
            }
            && aliasedCounter.ParentCleanupCounterPlan?.CounterBefore == 9
            && aliasedCounter.ParentCleanupCounterPlan?.CounterAfter == 8
            && aliasedCounter.ParentPlan.OrderedMemoryEffects.TakeLast(2)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                })
            && aliasedCounter.ParentMemoryEffectsCompleteBeforeChildTest);

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan missingChildEsp =
            Project(counter: 1,
                cleanupCallReturnedNormally: true,
                childEsp: null,
                childEbp: 0x2000,
                testedByte: 0);
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan missingChildEbp =
            Project(counter: 2,
                childEsp: 0x1000,
                childEbp: null,
                testedByte: 0);
        check("The child is entered only after validated ESP and EBP are both captured",
            missingChildEsp.Outcome
                == CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEsp
            && missingChildEsp.CleanupCallReached
            && missingChildEsp.CleanupCallReturnedNormally == true
            && !missingChildEsp.ChildEntryValidated
            && !missingChildEsp.Internal9441Entered
            && missingChildEsp.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingChildEsp.LateChildEbpProvidedButUnused
            && missingChildEbp.Outcome
                == CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEbp
            && missingChildEbp.ParentExactNonzeroEntryHandoff
            && !missingChildEbp.Internal9441Entered
            && missingChildEbp.LateChildEspProvidedButUnused
            && missingChildEbp.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            });

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan missingTest =
            Project(counter: 2, testedByte: null,
                stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3,
                callCellTarget: 0x1234, callCellReturnedNormally: true,
                postCallEbp: 0x5000, stackPopEbp: 4, returnTarget: 5);
        check("A missing 9441 TEST byte leaves all six DEC flags unchanged",
            missingTest.Internal9441Entered
            && missingTest.EpiloguePlan?.TestReadReached == true
            && missingTest.EpiloguePlan?.TestedByteCaptured == false
            && missingTest.ParentFlagsPreservedUntilChildTest
            && missingTest.FlagsAtStop == missingTest.ParentFlagsAtChildEntry
            && missingTest.OrderedEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .TestByteRead,
                Attempted: true,
                ValueCaptured: false,
                Committed: false
            });

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan missingCallCell =
            Project(counter: 2,
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellReturnedNormally: true);
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan gatedCallUnknown =
            Project(counter: 2,
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678);
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan gatedCallFalse =
            Project(counter: 2,
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678,
                callCellReturnedNormally: false);
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan normalCallMissingPostEbp =
            Project(counter: 2,
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678,
                callCellReturnedNormally: true,
                postCallEbp: null,
                stackPopEbp: 4,
                returnTarget: 5);
        check("The indirect cell gate and normal opaque return retain exact call and flag boundaries",
            missingCallCell.EpiloguePlan?.CallCellReadReached == true
            && missingCallCell.EpiloguePlan?.CallCellTargetCaptured == false
            && !missingCallCell.ChildCallReached
            && missingCallCell.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingCallCell.UnreachedCallCellReturnStatusProvided
            && gatedCallUnknown.ChildCallReached
            && gatedCallUnknown.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallReturnStatus
            && gatedCallUnknown.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && gatedCallFalse.ChildCallReached
            && gatedCallFalse.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally
            && gatedCallFalse.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && normalCallMissingPostEbp.ChildCallReached
            && normalCallMissingPostEbp.OpaqueCallReturnedNormally == true
            && normalCallMissingPostEbp.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingPostCallEbp
            && normalCallMissingPostEbp.EpiloguePlan?.PostCallEbpCaptureReached == true
            && normalCallMissingPostEbp.EpiloguePlan?.PostCallEbpCaptured == false
            && normalCallMissingPostEbp.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && fullZeroCallReturn.ChildBoundedExitReached
            && fullZeroCallReturn.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            });

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan wrappedStack =
            Project(counter: 2,
                childEsp: 0xFFFFFFFC,
                childEbp: 0x76543210,
                testedByte: 2,
                stackPopEdi: 0x1111,
                stackPopEsi: 0x2222,
                stackPopEbx: 0x3333,
                callCellTarget: 0x12345678,
                callCellReturnedNormally: true,
                postCallEbp: 0xFFFFFFFC,
                stackPopEbp: 0x4444,
                returnTarget: 0x5555);
        check("The 9441 stack path wraps at 32 bits and keeps the helper and gated call captures distinct",
            wrappedStack.EpiloguePlan?.StackPopEdiAddress == 0xFFFFFFFC
            && wrappedStack.EpiloguePlan?.StackPopEsiAddress == 0
            && wrappedStack.EpiloguePlan?.StackPopEbxAddress == 4
            && wrappedStack.EpiloguePlan?.PushImmediateAddress == 4
            && wrappedStack.EpiloguePlan?.PostCallEbp == 0xFFFFFFFC
            && wrappedStack.EpiloguePlan?.StackPopEbpAddress == 0xFFFFFFFC
            && wrappedStack.EpiloguePlan?.ReturnTargetStackAddress == 0
            && wrappedStack.EpiloguePlan?.EspAfterReturn == 4
            && wrappedStack.ChildReturnReached
            && wrappedStack.StopAtRawFileOffsetExclusive == 0x012C945C
            && !wrappedStack.CleanupCallReached
            && wrappedStack.CleanupCallTargetRawFileOffset is null);

        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan dirtyParent =
            Project(counter: 7, entryESI: null, testedByte: 1);
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan parentBypass =
            Project(counter: 7,
                size: 0x100,
                mask: 1,
                freshIndexedDWORD: 0x300,
                freshTag: 0,
                freshChildDWORD: 0,
                freshGlobalValue: 0x311,
                entryESI: 0x3000,
                testedByte: 1);
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan equalParentBypass =
            Project(counter: 7,
                size: 0x100,
                mask: 1,
                freshIndexedDWORD: 0x200,
                freshTag: 0x100,
                freshChildDWORD: 0,
                freshGlobalValue: 0xFFFFFFFF,
                entryESI: 0x3000,
                testedByte: 1);
        check("Dirty parent inputs and outer bypasses preserve their actual stop without entering the child",
            dirtyParent.ParentPlan.FreshCounterCaptureProvidedButUnused
            && !dirtyParent.ParentPlan.CleanupCounterGateEntered
            && !dirtyParent.CleanupCallReached
            && !dirtyParent.Internal9441Entered
            && dirtyParent.StopAtRawFileOffsetExclusive
                == dirtyParent.ParentPlan.StopAtRawFileOffsetExclusive
            && dirtyParent.ExactBoundedCompletion
                == dirtyParent.ParentPlan.ExactBoundedCompletion
            && parentBypass.ParentPlan.ParentOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget
            && parentBypass.StopAtRawFileOffsetExclusive == 0x012C8E76
            && parentBypass.ExactBoundedCompletion
                == parentBypass.ParentPlan.ExactBoundedCompletion
            && !parentBypass.Internal9441Entered
            && parentBypass.UnreachedTestedByteCaptureProvided
            && equalParentBypass.ParentPlan.ParentOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .ParentOtherBoundedExit
            && equalParentBypass.StopAtRawFileOffsetExclusive == 0x012C9403
            && equalParentBypass.ExactBoundedCompletion
                == equalParentBypass.ParentPlan.ExactBoundedCompletion
            && !equalParentBypass.Internal9441Entered
            && equalParentBypass.UnreachedTestedByteCaptureProvided);
    }

    private static CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan Project(
        uint? counter,
        uint? childEsp = 0x1000,
        uint? childEbp = 0x2000,
        bool? cleanupCallReturnedNormally = null,
        byte? testedByte = null,
        uint? stackPopEdi = null,
        uint? stackPopEsi = null,
        uint? stackPopEbx = null,
        uint? callCellTarget = null,
        bool? callCellReturnedNormally = null,
        uint? postCallEbp = null,
        uint? stackPopEbp = null,
        uint? returnTarget = null,
        uint size = 0x100,
        uint mask = 1,
        uint? freshIndexedDWORD = 0x300,
        uint? freshTag = 0,
        uint? freshChildDWORD = 0,
        uint? lateFreshChildPlus4 = null,
        uint? freshGlobalValue = 0x310,
        uint? entryESI = 0x3000)
    {
        var storeCapture =
            new CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleCapture(
                new(
                    new(
                        new(
                            new(
                                EntryEDI: size,
                                LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                                EntryESI: entryESI,
                                LateFreshGlobalValue: mask),
                            FreshIndexedDWORD: freshIndexedDWORD,
                            FreshEDXPlus4: freshTag),
                        FreshChildDWORD: freshChildDWORD,
                        LateFreshChildPlus4: lateFreshChildPlus4),
                    FreshGlobalValue: freshGlobalValue));
        var parentCapture =
            new CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleCapture(
                storeCapture,
                counter);
        return Projection.Project(new(
            parentCapture,
            childEsp,
            childEbp,
            cleanupCallReturnedNormally,
            testedByte,
            stackPopEdi,
            stackPopEsi,
            stackPopEbx,
            callCellTarget,
            callCellReturnedNormally,
            postCallEbp,
            stackPopEbp,
            returnTarget));
    }
}
