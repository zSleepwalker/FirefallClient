using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleProjection;

internal static class
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The exact outer five-store DEC gate joins the shared 9441 epilogue",
            Projection.EntryRawFileOffset
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection
                    .EntryRawFileOffset
            && Projection.ChildEntryRawFileOffset == 0x012C9441
            && Projection.ZeroCounterStopRawFileOffset == 0x012C943C
            && Projection.ZeroCounterCallRawFileOffset == 0x012C943C
            && Projection.CleanupCallTargetRawFileOffset == 0x012C78A0
            && Projection.CleanupCallTargetVirtualAddress == 0x016C84A0
            && Projection.ReturnBoundaryRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan nonzero =
            Project(
                counter: 9,
                testedByte: 0,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                stackPopEbp: 4,
                returnTarget: 5);
        check("Five committed outer-equality stores admit nonzero DEC/JNZ while retaining its flags",
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
            && nonzero.ParentPlan.CounterJnzTaken == true
            && nonzero.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C9441
            && !nonzero.ParentPlan.CounterNonzeroTargetEntered
            && nonzero.Internal9441Entered
            && nonzero.EpiloguePlan?.TestReadReached == true
            && CapacityFinalCalleeGatedEpilogueProjection.TestMask == 0x02
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C945C
            && nonzero.ChildReturnReached
            && nonzero.ExactBoundedCompletion
            && nonzero.ExactCleanCompletion
            && nonzero.ParentFlagsAtChildEntry is { Carry: false }
            && nonzero.ParentFlagsPreservedUntilChildTest == false);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan zeroCallNormal =
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
        check("The zero branch calls the opaque helper before entering the shared bit-1 TEST",
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
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                    .CleanupCall
            && zeroCallNormal.OrderedEffects[zeroCallNormal.ParentMemoryEffectCount + 1].Kind
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan zeroCallMissing =
            Project(counter: 1);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan zeroCallFalse =
            Project(counter: 1, cleanupCallReturnedNormally: false);
        check("Missing or false direct-call status preserves DEC flags and stops at 943C",
            zeroCallMissing.CleanupCallReached
            && zeroCallMissing.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                    .MissingCleanupCallReturnStatus
            && zeroCallMissing.EpiloguePlan is null
            && !zeroCallMissing.Internal9441Entered
            && zeroCallMissing.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroCallMissing.ParentBoundedExitAt943C
            && !zeroCallMissing.ExactBoundedCompletion
            && !zeroCallMissing.ExactCleanCompletion
            && !zeroCallMissing.CleanupCallReturnStatusCaptured
            && zeroCallMissing.FlagsAtStop == ConvertFlags(zeroCallMissing.ParentPlan.FlagsAtStop)
            && zeroCallMissing.FlagsAtStop is { Carry: false }
            && zeroCallMissing.OrderedEffects[zeroCallMissing.ParentMemoryEffectCount] is
            {
                Kind: CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                    .CleanupCall,
                ValueCaptured: true,
                BooleanValue: null
            }
            && zeroCallFalse.CleanupCallReached
            && zeroCallFalse.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                    .CleanupCallDidNotReturnNormally
            && zeroCallFalse.EpiloguePlan is null
            && zeroCallFalse.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroCallFalse.ParentBoundedExitAt943C
            && !zeroCallFalse.ExactBoundedCompletion
            && !zeroCallFalse.ExactCleanCompletion
            && zeroCallFalse.FlagsAtStop == ConvertFlags(zeroCallFalse.ParentPlan.FlagsAtStop));

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan zeroCallMissingEsp =
            Project(counter: 1, cleanupCallReturnedNormally: true, childEsp: null);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan nonzeroMissingEsp =
            Project(counter: 9, childEsp: null);
        check("Normal direct-call return with missing ESP is unbounded and flags remain path-correct",
            zeroCallMissingEsp.ParentExactZeroCleanupCallHandoff
            && zeroCallMissingEsp.CleanupCallReturnedNormally == true
            && zeroCallMissingEsp.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEsp
            && zeroCallMissingEsp.StopAtRawFileOffsetExclusive == 0x012C9441
            && !zeroCallMissingEsp.ExactBoundedCompletion
            && !zeroCallMissingEsp.ExactCleanCompletion
            && zeroCallMissingEsp.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && nonzeroMissingEsp.ParentExactNonzeroEntryHandoff
            && nonzeroMissingEsp.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEsp
            && nonzeroMissingEsp.StopAtRawFileOffsetExclusive == 0x012C9441
            && !nonzeroMissingEsp.ExactBoundedCompletion
            && !nonzeroMissingEsp.ExactCleanCompletion
            && nonzeroMissingEsp.FlagsAtStop == ConvertFlags(nonzeroMissingEsp.ParentPlan.FlagsAtStop)
            && nonzeroMissingEsp.FlagsAtStop is { Carry: false }
            && nonzeroMissingEsp.ParentFlagsPreservedUntilChildTest);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan dirtyNonzero =
            Project(
                counter: 9,
                cleanupCallReturnedNormally: true,
                testedByte: 0,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                stackPopEbp: 4,
                returnTarget: 5);
        check("An unused zero-path call status dirties clean completion but keeps the bounded RET",
            dirtyNonzero.ParentExactNonzeroEntryHandoff
            && dirtyNonzero.UnreachedCleanupCallReturnStatusProvided
            && dirtyNonzero.ChildReturnReached
            && dirtyNonzero.StopAtRawFileOffsetExclusive == 0x012C945C
            && dirtyNonzero.ExactBoundedCompletion
            && !dirtyNonzero.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan pendingCell =
            Project(
                counter: 2,
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan dirtyCellBypass =
            Project(
                counter: 2,
                testedByte: 0,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellReturnedNormally: true,
                stackPopEbp: 4,
                returnTarget: 5);
        check("Pending cell calls retain TEST flags and the bit-clear branch bypasses the cell",
            pendingCell.ChildCallReached
            && pendingCell.FlagsAtStop is
            { Carry: false, Parity: false, Auxiliary: null, Zero: false, Sign: false, Overflow: false }
            && !pendingCell.ChildBoundedExitReached
            && dirtyCellBypass.ChildBoundedExitReached
            && dirtyCellBypass.ExactBoundedCompletion
            && !dirtyCellBypass.ExactCleanCompletion
            && dirtyCellBypass.FlagsAtStop is
            { Carry: false, Parity: true, Auxiliary: null, Zero: true, Sign: false, Overflow: false });

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan dirtyBypass =
            Project(
                counter: 9,
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 0x300,
                freshThresholdEDXPlus8: 0x400,
                testedByte: 2);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan cleanBypass =
            Project(
                counter: null,
                childEsp: null,
                childEbp: null,
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 0x300,
                freshThresholdEDXPlus8: 0x400);
        check("Threshold bypass retains its parent exit and unused captures cannot create a child",
            dirtyBypass.ParentPlan.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing
            && dirtyBypass.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C8E76
            && dirtyBypass.ParentPlan.ExactBoundedCompletion
            && dirtyBypass.ParentPlan.FreshCounterCaptureProvidedButUnused
            && !dirtyBypass.ParentPlan.CounterReadAttempted
            && dirtyBypass.EpiloguePlan is null
            && dirtyBypass.StopAtRawFileOffsetExclusive == 0x012C8E76
            && dirtyBypass.ExactBoundedCompletion
            && !dirtyBypass.ExactCleanCompletion
            && dirtyBypass.UnreachedTestedByteCaptureProvided
            && cleanBypass.ParentPlan.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing
            && cleanBypass.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C8E76
            && cleanBypass.ParentPlan.ExactCleanCompletion
            && cleanBypass.ParentPlan.FreshCounterCaptureProvidedButUnused == false
            && !cleanBypass.ParentPlan.CounterReadAttempted
            && cleanBypass.ParentPlan.ParentPlan.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead)
            && cleanBypass.ParentPlan.ParentPlan.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshEDXPlus8Read)
            && cleanBypass.EpiloguePlan is null
            && cleanBypass.StopAtRawFileOffsetExclusive == 0x012C8E76
            && cleanBypass.ExactBoundedCompletion
            && cleanBypass.ExactCleanCompletion
            && !cleanBypass.LateChildEspProvidedButUnused
            && !cleanBypass.LateChildEbpProvidedButUnused);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan missingCounter =
            Project(counter: null, testedByte: 0);
        check("A missing fresh counter blocks child projection at DEC without duplicating parent validation",
            missingCounter.ParentPlan.ExactParentStoreCompletion
            && missingCounter.ParentCleanupCounterPlan?.CounterReadReached == true
            && missingCounter.ParentCleanupCounterPlan?.FreshCounterDwordCaptured == false
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
    }

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan
        Project(
            uint? counter,
            bool? cleanupCallReturnedNormally = null,
            uint? childEsp = 0x1000,
            uint? childEbp = 0x2000,
            byte? testedByte = null,
            uint? stackPopEdi = null,
            uint? stackPopEsi = null,
            uint? stackPopEbx = null,
            uint? callCellTarget = null,
            bool? callCellReturnedNormally = null,
            uint? postCallEbp = null,
            uint? stackPopEbp = null,
            uint? returnTarget = null,
            uint? indexedDWORD = 0x200,
            uint? tagDWORD = 0x100,
            uint? freshThresholdGlobal = 1,
            uint? freshThresholdEDXPlus8 = 1,
            uint? rootESI = 0x3000,
            uint size = 0x100,
            uint mask = 1) =>
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleProjection.Project(
            new(
                ParentCapture: new(
                    ParentCapture: new(
                        Parent: new(
                            Parent: new(
                                EntryEDI: size,
                                LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                                EntryESI: rootESI,
                                LateFreshGlobalValue: mask),
                            FreshIndexedDWORD: indexedDWORD,
                            FreshEDXPlus4: tagDWORD),
                        FreshThresholdGlobalValue: freshThresholdGlobal,
                        FreshThresholdEDXPlus8: freshThresholdEDXPlus8),
                    FreshCounterDword: counter),
                CleanupCallReturnedNormally: cleanupCallReturnedNormally,
                LateChildEsp: childEsp,
                LateChildEbp: childEbp,
                TestedByte: testedByte,
                StackPopEdiValue: stackPopEdi,
                StackPopEsiValue: stackPopEsi,
                StackPopEbxValue: stackPopEbx,
                CallCellTargetVirtualAddress: callCellTarget,
                CallCellReturnedNormally: callCellReturnedNormally,
                PostCallEbp: postCallEbp,
                StackPopEbpValue: stackPopEbp,
                ReturnTargetVirtualAddress: returnTarget));

    private static
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleFlags?
            flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;
}
