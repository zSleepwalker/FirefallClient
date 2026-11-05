using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleProjection;

internal static class
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The five-store DEC gate and shared epilogue retain their observed boundaries",
            Projection.EntryRawFileOffset == 0x012C9354
            && Projection.ChildEntryRawFileOffset == 0x012C9441
            && Projection.ZeroCounterStopRawFileOffset == 0x012C943C
            && Projection.ZeroCounterCallRawFileOffset == 0x012C943C
            && Projection.CleanupCallTargetRawFileOffset == 0x012C78A0
            && Projection.CleanupCallTargetVirtualAddress == 0x016C84A0
            && Projection.ReturnBoundaryRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan nonzero =
            Project(
                counter: 9,
                testedByte: 0,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                stackPopEbp: 4,
                returnTarget: 5);
        check("Five committed ring stores admit DEC/JNZ to unentered 9441 with parent flags retained",
            nonzero.ParentPlan.ExactParentStoreCompletion
            && nonzero.ParentPlan.ParentPlan.ThresholdRingPlan?.Writes is
            [
                { RawFileOffset: 0x012C941C, Attempted: true, Committed: true },
                { RawFileOffset: 0x012C941F, Attempted: true, Committed: true },
                { RawFileOffset: 0x012C9422, Attempted: true, Committed: true },
                { RawFileOffset: 0x012C9425, Attempted: true, Committed: true },
                { RawFileOffset: 0x012C9428, Attempted: true, Committed: true }
            ]
            && nonzero.ParentExactNonzeroEntryHandoff
            && nonzero.ParentPlan.CounterBefore == 9
            && nonzero.ParentPlan.CounterAfter == 8
            && nonzero.ParentCleanupCounterPlan?.Capture.IncomingCarryFlag
                == nonzero.ParentPlan.ParentPlan.FlagsAtStop?.Carry
            && nonzero.ParentCleanupCounterPlan?.UnreachedIncomingCarryFlagCaptureProvided == false
            && nonzero.ParentPlan.CounterJnzTaken == true
            && nonzero.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C9441
            && !nonzero.ParentPlan.CounterNonzeroTargetEntered
            && nonzero.Internal9441Entered
            && nonzero.EpiloguePlan?.TestReadReached == true
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C945C
            && nonzero.ChildReturnReached
            && nonzero.ExactBoundedCompletion
            && nonzero.ExactCleanCompletion
            && nonzero.ParentFlagsAtChildEntry == ConvertFlags(nonzero.ParentPlan.FlagsAtStop)
            && nonzero.ParentFlagsAtChildEntry is { Carry: false }
            && nonzero.ParentFlagsPreservedUntilChildTest == false);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan zeroCallNormal =
            Project(
                counter: 1,
                cleanupCallReturnedNormally: true,
                testedByte: 2,
                stackPopEdi: 0x1111,
                stackPopEsi: 0x2222,
                stackPopEbx: 0x3333,
                callCellTarget: 0x12345678,
                callCellReturnedNormally: true,
                postCallEbp: 0x5000,
                stackPopEbp: 4,
                returnTarget: 5);
        check("DEC zero follows MOV ECX and a normal opaque helper return before shared TEST",
            zeroCallNormal.ParentExactZeroCleanupCallHandoff
            && zeroCallNormal.ParentPlan.ZeroPathMovEcxReached
            && zeroCallNormal.CleanupCallReached
            && zeroCallNormal.CleanupCallReturnStatusCaptured
            && zeroCallNormal.CleanupCallReturnedNormally == true
            && zeroCallNormal.CleanupCallRawFileOffset == 0x012C943C
            && zeroCallNormal.CleanupCallTargetRawFileOffset == 0x012C78A0
            && zeroCallNormal.CleanupCallTargetVirtualAddress == 0x016C84A0
            && !zeroCallNormal.CleanupCallExecuted
            && !zeroCallNormal.CleanupHelperEntered
            && zeroCallNormal.Internal9441Entered
            && zeroCallNormal.EpiloguePlan?.TestReadReached == true
            && zeroCallNormal.EpiloguePlan?.TestedBitIsSet == true
            && zeroCallNormal.EpiloguePlan?.BranchTaken == false
            && zeroCallNormal.ChildReturnReached
            && zeroCallNormal.StopAtRawFileOffsetExclusive == 0x012C945C
            && zeroCallNormal.ExactBoundedCompletion
            && zeroCallNormal.ExactCleanCompletion
            && zeroCallNormal.ParentFlagsAtChildEntry is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && zeroCallNormal.OrderedEffects[zeroCallNormal.ParentMemoryEffectCount].Kind
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .CleanupCall
            && zeroCallNormal.OrderedEffects[zeroCallNormal.ParentMemoryEffectCount + 1].Kind
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .TestByteRead
            && zeroCallNormal.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            });

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan zeroCallMissing =
            Project(counter: 1);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan zeroCallFalse =
            Project(counter: 1, cleanupCallReturnedNormally: false);
        check("Missing or false helper-return evidence stops at 943C without entering 9441",
            zeroCallMissing.CleanupCallReached
            && zeroCallMissing.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                    .MissingCleanupCallReturnStatus
            && zeroCallMissing.EpiloguePlan is null
            && !zeroCallMissing.Internal9441Entered
            && zeroCallMissing.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroCallMissing.ExactBoundedCompletion
            && !zeroCallMissing.ExactCleanCompletion
            && !zeroCallMissing.CleanupCallReturnStatusCaptured
            && zeroCallMissing.LateChildEspProvidedButUnused
            && zeroCallMissing.LateChildEbpProvidedButUnused
            && zeroCallMissing.FlagsAtStop == ConvertFlags(zeroCallMissing.ParentPlan.FlagsAtStop)
            && zeroCallMissing.FlagsAtStop is { Carry: false }
            && zeroCallMissing.OrderedEffects[zeroCallMissing.ParentMemoryEffectCount] is
            {
                Kind: CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .CleanupCall,
                ValueCaptured: true,
                BooleanValue: null
            }
            && zeroCallFalse.CleanupCallReached
            && zeroCallFalse.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                    .CleanupCallDidNotReturnNormally
            && zeroCallFalse.EpiloguePlan is null
            && zeroCallFalse.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroCallFalse.ExactBoundedCompletion
            && !zeroCallFalse.ExactCleanCompletion
            && zeroCallFalse.FlagsAtStop == ConvertFlags(zeroCallFalse.ParentPlan.FlagsAtStop));

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan dirtyNonzero =
            Project(
                counter: 9,
                cleanupCallReturnedNormally: true,
                testedByte: 0,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                stackPopEbp: 4,
                returnTarget: 5);
        check("Unused cleanup-return evidence dirties clean completion but preserves a bounded RET",
            dirtyNonzero.ParentExactNonzeroEntryHandoff
            && dirtyNonzero.UnreachedCleanupCallReturnStatusProvided
            && dirtyNonzero.ChildReturnReached
            && dirtyNonzero.StopAtRawFileOffsetExclusive == 0x012C945C
            && dirtyNonzero.ExactBoundedCompletion
            && !dirtyNonzero.ExactCleanCompletion);

        var pendingCell = Project(
            counter: 2, testedByte: 2,
            stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3,
            callCellTarget: 0x12345678);
        var dirtyCellBypass = Project(
            counter: 2, testedByte: 0,
            stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3,
            callCellReturnedNormally: true, stackPopEbp: 4, returnTarget: 5);
        check("Pending indirect return retains TEST flags and unused return evidence preserves the bounded bypass",
            pendingCell.ChildCallReached
            && pendingCell.FlagsAtStop is
            { Carry: false, Parity: false, Auxiliary: null, Zero: false, Sign: false, Overflow: false }
            && !pendingCell.ChildBoundedExitReached
            && dirtyCellBypass.ChildBoundedExitReached
            && dirtyCellBypass.ExactBoundedCompletion
            && !dirtyCellBypass.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan byteZero =
            Project(counter: 2,
                testedByte: 0xFD,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                stackPopEbp: 4,
                returnTarget: 5);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan byteNonzero =
            Project(counter: 2,
                testedByte: 0xFE,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678,
                callCellReturnedNormally: true,
                postCallEbp: 0x2000,
                stackPopEbp: 4,
                returnTarget: 5);
        check("TEST uses only bit 1 and restores the captured frame before the bounded RET",
            byteZero.EpiloguePlan?.TestResult == 0
            && byteZero.EpiloguePlan?.TestedBitIsSet == false
            && byteZero.EpiloguePlan?.BranchTaken == true
            && byteZero.EpiloguePlan?.MovEspSourceEbp == 0x2000
            && byteZero.EpiloguePlan?.StackPopEbpAddress == 0x2000
            && byteZero.EpiloguePlan?.ReturnTargetStackAddress == 0x2004
            && byteZero.EpiloguePlan?.EspAfterReturn == 0x2008
            && byteZero.ChildReturnReached
            && byteZero.ExactBoundedCompletion
            && byteZero.ExactCleanCompletion
            && byteNonzero.EpiloguePlan?.TestResult == 2
            && byteNonzero.EpiloguePlan?.TestedBitIsSet == true
            && byteNonzero.EpiloguePlan?.BranchTaken == false
            && byteNonzero.EpiloguePlan?.CallReached == true
            && byteNonzero.EpiloguePlan?.MovEspSourceEbp == 0x2000
            && byteNonzero.ChildReturnReached
            && byteNonzero.StopAtRawFileOffsetExclusive == 0x012C945C
            && byteNonzero.ExactBoundedCompletion);

        const uint counterAddress = 0x01EDADE8;
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan aliasedCounter =
            Project(counter: 9, freshChildEDXPlus8: counterAddress - 0x0C,
                testedByte: 0,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                stackPopEbp: 4,
                returnTarget: 5);
        check("The DEC reads fresh counter state even when a ring store aliases its address",
            aliasedCounter.ParentExactNonzeroEntryHandoff
            && aliasedCounter.ParentPlan.ParentPlan.ThresholdRingPlan?.Writes.Any(write =>
                write.Address == counterAddress
                && write.Committed
                && write.Value != aliasedCounter.ParentPlan.Capture.FreshCounterDword) == true
            && aliasedCounter.ParentCleanupCounterPlan?.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: counterAddress,
                ValueCaptured: true,
                Value: 9
            }
            && aliasedCounter.ParentPlan.CounterBefore == 9
            && aliasedCounter.ParentPlan.CounterAfter == 8
            && aliasedCounter.OrderedEffects.Take(aliasedCounter.ParentMemoryEffectCount)
                .Select(effect => effect.Kind)
                .SequenceEqual(aliasedCounter.ParentPlan.OrderedMemoryEffects.Select(effect =>
                    effect.Kind switch
                    {
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .RingWriteAttempt =>
                            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                                .RingWrite,
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .CleanupCounterRead =>
                            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                                .CleanupCounterRead,
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .CleanupCounterWrite =>
                            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                                .CleanupCounterWrite,
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .ParentWrite =>
                            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                                .ParentWrite,
                        _ =>
                            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                                .ParentRead
                    })));

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan missingCounter =
            Project(counter: null, testedByte: 0);
        check("A missing fresh counter blocks the child and preserves the exact parent stop",
            missingCounter.ParentPlan.ExactParentStoreCompletion
            && missingCounter.ParentPlan.CleanupCounterPlan?.CounterReadReached == true
            && missingCounter.ParentPlan.CleanupCounterPlan?.FreshCounterDwordCaptured == false
            && !missingCounter.ParentExactNonzeroEntryHandoff
            && !missingCounter.ParentExactZeroCleanupCallHandoff
            && !missingCounter.CleanupCallReached
            && missingCounter.EpiloguePlan is null
            && !missingCounter.Internal9441Entered
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F
            && !missingCounter.ExactBoundedCompletion
            && !missingCounter.ExactCleanCompletion
            && missingCounter.LateChildEspProvidedButUnused
            && missingCounter.LateChildEbpProvidedButUnused
            && missingCounter.UnreachedTestedByteCaptureProvided
            && missingCounter.FlagsAtStop == ConvertFlags(missingCounter.ParentPlan.FlagsAtStop));

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan missingChildEbp =
            Project(counter: 2, childEsp: 0x1000, childEbp: null, testedByte: 0);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan missingTestedByte =
            Project(counter: 2,
                testedByte: null,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3);
        check("Missing frame and TEST captures retain their distinct reached and stop states",
            missingChildEbp.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEbp
            && missingChildEbp.StopAtRawFileOffsetExclusive == 0x012C9441
            && !missingChildEbp.ChildEntryValidated
            && missingChildEbp.LateChildEspProvidedButUnused
            && !missingChildEbp.Internal9441Entered
            && missingTestedByte.ChildEntryValidated
            && missingTestedByte.EpiloguePlan?.TestReadReached == true
            && missingTestedByte.EpiloguePlan?.TestedByteCaptured == false
            && missingTestedByte.ParentFlagsPreservedUntilChildTest
            && missingTestedByte.StopAtRawFileOffsetExclusive == 0x012C9441
            && !missingTestedByte.ExactBoundedCompletion);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan wrappedStack =
            Project(
                counter: 2,
                childEsp: 0xFFFFFFFC,
                childEbp: 0xFFFFFFFC,
                testedByte: 2,
                stackPopEdi: 0x1111,
                stackPopEsi: 0x2222,
                stackPopEbx: 0x3333,
                callCellTarget: 0x12345678,
                callCellReturnedNormally: true,
                postCallEbp: 0xFFFFFFFC,
                stackPopEbp: 0x4444,
                returnTarget: 0x5555);
        check("The shared epilogue wraps 32-bit stack arithmetic without conflating call captures",
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
    }

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleFlags? flags) =>
            flags is { } value
                ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
                : null;

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan
        Project(
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
            uint? freshIndexedDWORD = 0x300,
            uint? freshTag = 0,
            uint? freshChildDWORD = 0x5000,
            uint? freshChildPlus4 = 0x107,
            uint? freshChildGlobalValue = 0x1000,
            uint? freshChildEDXPlus8 = 0x3000,
            uint? entryESI = 0x3000)
    {
        var parentCapture =
            new CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleCapture(
                new(
                    new(
                        new(
                            new(
                                EntryEDI: size,
                                LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                                EntryESI: entryESI,
                                LateFreshGlobalValue: 1),
                            FreshIndexedDWORD: freshIndexedDWORD,
                            FreshEDXPlus4: freshTag),
                        FreshChildDWORD: freshChildDWORD,
                        LateFreshChildPlus4: freshChildPlus4),
                    FreshChildGlobalValue: freshChildGlobalValue,
                    FreshChildEDXPlus8: freshChildEDXPlus8),
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
