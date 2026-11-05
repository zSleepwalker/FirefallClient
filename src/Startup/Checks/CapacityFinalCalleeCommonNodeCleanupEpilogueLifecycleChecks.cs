using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The node cleanup composition uses the exact internal entry, zero stop, and RET boundary",
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.EntryRawFileOffset
                == 0x012C9354
            && CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection
                .ChildEntryRawFileOffset == 0x012C9441
            && CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection
                .ZeroCounterStopRawFileOffset == 0x012C943C
            && CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection
                .ReturnBoundaryRawFileOffsetExclusive == 0x012C945C
            && CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset == 0x012C945B
            && CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive
                == 0x012C945C);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan zeroIndex =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(edi: 0x100, lateEntryEbp: 0x2000, counter: 2, testedByte: 0));
        check("Index-zero reaches the exact five-store cleanup handoff and bounded TEST-skip RET",
            zeroIndex.ParentOutcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .CounterDecrementedNonzeroTo9441
            && zeroIndex.ParentPlan.ParentChild.Parent.ParentLifecycle?.DerivedECX == 0
            && zeroIndex.ParentPlan.ParentChild.ParentDerivedNodeEBX == 0x01EDAEF8
            && zeroIndex.ParentPlan.ParentChild.ChildEntryEDX == 1
            && zeroIndex.ParentPlan.ExactParentWrites
            && zeroIndex.ParentPlan.ExactFiveChildWrites
            && zeroIndex.ParentPlan.ExactParentChildCompletion
            && zeroIndex.ParentPlan.OrderedMemoryEffects.Count(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                        .ChildPointerStore) == 4
            && zeroIndex.ParentPlan.OrderedMemoryEffects.Count(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                        .ChildGlobalWrite) == 1
            && zeroIndex.ParentPlan.CounterGate.CounterAccesses.Length == 2
            && zeroIndex.ParentPlan.CounterGate.CounterAccesses[0].Kind
                == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
            && zeroIndex.ParentPlan.CounterGate.CounterAccesses[1].Kind
                == CapacityFinalCalleeCleanupCounterGateAccessKind.Write
            && zeroIndex.ParentPlan.CounterGate.CounterBefore == 2
            && zeroIndex.ParentPlan.CounterGate.CounterAfter == 1
            && zeroIndex.ParentPlan.JnzTaken == true
            && zeroIndex.ParentPlan.SelectedDestinationRawFileOffset == 0x012C9441
            && zeroIndex.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C9441
            && zeroIndex.ParentExactNonzeroEntryHandoff
            && zeroIndex.ChildEntryValidated
            && zeroIndex.Internal9441Entered
            && zeroIndex.ChildTestReadReached
            && zeroIndex.EpiloguePlan?.Outcome == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && zeroIndex.EpiloguePlan?.TestedByte == 0
            && zeroIndex.EpiloguePlan?.BranchTaken == true
            && !zeroIndex.ChildCallReached
            && !zeroIndex.OpaqueCallExecuted
            && zeroIndex.ChildReturnReached
            && zeroIndex.StopAtRawFileOffsetExclusive == 0x012C945C
            && zeroIndex.ChildExactCleanExitAdmitted
            && zeroIndex.ExactCleanCompletion
            && zeroIndex.ExactBoundedExitAdmitted
            && zeroIndex.ParentFlagsAtChildEntry is { Carry: false }
            && zeroIndex.FlagsAtStop is { Carry: false, Zero: true }
            && !zeroIndex.ParentFlagsPreservedUntilChildTest
            && !zeroIndex.DestinationEntered);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan index31 =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(edi: 0x00FFFF00, lateEntryEbp: 0x2000, counter: 2, testedByte: 0));
        check("Index-thirty-one carries the high-bit node mask through DEC and into 9441",
            index31.ParentPlan.ParentChild.Parent.ParentLifecycle?.DerivedECX == 31
            && index31.ParentPlan.ParentChild.ParentDerivedNodeEBX == 0x01EDAF74
            && index31.ParentPlan.ParentChild.Parent.FreshMaskGate?.ShiftResult == 0x80000000
            && index31.ParentPlan.ParentChild.ChildEntryEDX == 0x80000000
            && index31.ParentPlan.ParentChild.ChildORFlags?.Carry == false
            && index31.ParentPlan.CounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && index31.ParentExactNonzeroEntryHandoff
            && index31.ChildEntryValidated
            && index31.Internal9441Entered
            && index31.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan zeroCounter =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 1, testedByte: 0, callTarget: 0x1234,
                    callReturnedNormally: true, postCallEbp: 0x5000));
        check("A zero DEC stops before the 943C call and never enters the internal TEST",
            zeroCounter.Outcome
                == CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
                    .ParentDecrementedToZeroBeforeCall
            && zeroCounter.ParentExactZeroBypass
            && zeroCounter.ParentPlan.JnzTaken == false
            && zeroCounter.ParentPlan.SelectedDestinationRawFileOffset == 0x012C9437
            && zeroCounter.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroCounter.EpiloguePlan is null
            && !zeroCounter.ChildEntryValidated
            && !zeroCounter.Internal9441Entered
            && !zeroCounter.ChildCallReached
            && !zeroCounter.OpaqueCallExecuted
            && zeroCounter.UnreachedTestedByteCaptureProvided
            && zeroCounter.UnreachedCallCellTargetCaptureProvided
            && zeroCounter.UnreachedCallReturnStatusProvided
            && zeroCounter.UnreachedPostCallEbpCaptureProvided
            && !zeroCounter.ExactCleanCompletion
            && zeroCounter.ExactBoundedExitAdmitted);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan cleanZeroCounter =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 1, childEsp: null, childEbp: null, testedByte: null,
                    popEdi: null, popEsi: null, popEbx: null, callTarget: null,
                    callReturnedNormally: null, postCallEbp: null, popEbp: null,
                    returnTarget: null));
        check("A zero DEC is clean only when all 9441 and later captures are absent",
            cleanZeroCounter.ParentExactZeroBypass
            && cleanZeroCounter.ExactCleanCompletion
            && cleanZeroCounter.ExactBoundedExitAdmitted
            && cleanZeroCounter.EpiloguePlan is null
            && cleanZeroCounter.StopAtRawFileOffsetExclusive == 0x012C943C);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingCounter =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: null, testedByte: 0, callTarget: 0x1234));
        check("A missing fresh counter attempts only its read at 942F and leaves child captures unused",
            missingCounter.ParentOutcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .MissingFreshCounterDwordAt942F
            && missingCounter.ParentPlan.Internal942FEntryReached
            && missingCounter.ParentPlan.CleanupCounterReadAttempted
            && !missingCounter.ParentPlan.CleanupCounterDecrementExecuted
            && missingCounter.ParentPlan.CounterGate.CounterAccesses.Length == 1
            && missingCounter.ParentPlan.CounterGate.CounterAccesses[0].ValueCaptured == false
            && missingCounter.ParentPlan.ParentORFlagsPreservedWhenCounterUnavailable
            && missingCounter.EpiloguePlan is null
            && !missingCounter.Internal9441Entered
            && missingCounter.UnreachedTestedByteCaptureProvided
            && missingCounter.UnreachedCallCellTargetCaptureProvided
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan dirtyUnusedEBP =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(lateEntryEbp: 0x2000, counter: 2, testedByte: 0));
        check("A dirty unused parent EBP blocks fresh-mask and counter admission",
            dirtyUnusedEBP.ParentOutcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome.ParentNotCleanAt93A1
            && dirtyUnusedEBP.ParentPlan.ParentChild.Parent.ParentLifecycle
                ?.LateEntryEBPProvidedButUnused == true
            && !dirtyUnusedEBP.ParentPlan.CleanupCounterReadAttempted
            && dirtyUnusedEBP.EpiloguePlan is null
            && dirtyUnusedEBP.UnreachedTestedByteCaptureProvided
            && dirtyUnusedEBP.StopAtRawFileOffsetExclusive == 0x012C93A1);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingESP =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, childEsp: null, childEbp: 0x5000,
                    testedByte: 0, popEdi: 1));
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingEBP =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, childEsp: 0x1000, childEbp: null,
                    testedByte: 0, popEdi: 1));
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingBoth =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, childEsp: null, childEbp: null, testedByte: 0));
        check("Missing late ESP and EBP stop at 9441, preserve parent flags, and retain the other operand",
            missingESP.Outcome
                == CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEsp
            && missingESP.ParentExactNonzeroEntryHandoff
            && missingESP.Capture.LateChildEsp is null
            && missingESP.Capture.LateChildEbp == 0x5000
            && missingESP.EpiloguePlan is null
            && missingESP.ChildEntryPending
            && missingESP.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingESP.LateChildEbpProvidedButUnused
            && missingESP.UnreachedTestedByteCaptureProvided
            && missingESP.ParentFlagsPreservedUntilChildTest
            && missingEBP.Outcome
                == CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEbp
            && missingEBP.Capture.LateChildEsp == 0x1000
            && missingEBP.Capture.LateChildEbp is null
            && missingEBP.EpiloguePlan is null
            && missingEBP.LateChildEspProvidedButUnused
            && missingEBP.ParentFlagsPreservedUntilChildTest
            && missingBoth.Outcome
                == CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEspAndEbp
            && missingBoth.ChildEntryPending
            && missingBoth.EpiloguePlan is null
            && missingBoth.ParentFlagsPreservedUntilChildTest
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingTest =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: null, popEdi: 1, popEsi: 2,
                    popEbx: 3, popEbp: 4, returnTarget: 5));
        check("The internal 9441 entry is counted at a TEST read attempt and missing TEST data preserves DEC flags",
            missingTest.ChildEntryValidated
            && missingTest.Internal9441Entered
            && missingTest.ChildTestReadReached
            && missingTest.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte
            && missingTest.ParentFlagsAtChildEntry is
            {
                Carry: false, Parity: false, Auxiliary: false,
                Zero: false, Sign: false, Overflow: false
            }
            && missingTest.FlagsAtStop == missingTest.ParentFlagsAtChildEntry
            && missingTest.ParentFlagsPreservedUntilChildTest
            && missingTest.UnreachedStackPopEdiCaptureProvided
            && missingTest.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingFirstPop =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, popEdi: null, popEsi: 2,
                    popEbx: 3, callTarget: 0x1234));
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingSecondPop =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, popEdi: 1, popEsi: null,
                    popEbx: 3, callTarget: 0x1234));
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingThirdPop =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, popEdi: 1, popEsi: 2,
                    popEbx: null, callTarget: 0x1234));
        check("Ordered stack pops stop at the missing instruction without consuming later captures",
            missingFirstPop.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEdiValue
            && missingFirstPop.EpiloguePlan?.StackPopEdiAddress == 0x1000
            && missingFirstPop.EpiloguePlan?.StackAccesses.Length == 1
            && missingFirstPop.UnreachedStackPopEsiCaptureProvided
            && missingFirstPop.UnreachedCallCellTargetCaptureProvided
            && missingFirstPop.StopAtRawFileOffsetExclusive == 0x012C9448
            && missingSecondPop.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEsiValue
            && missingSecondPop.EpiloguePlan?.PoppedEdi == 1
            && missingSecondPop.EpiloguePlan?.StackPopEsiAddress == 0x1004
            && missingSecondPop.UnreachedStackPopEbxCaptureProvided
            && missingSecondPop.StopAtRawFileOffsetExclusive == 0x012C9449
            && missingThirdPop.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbxValue
            && missingThirdPop.EpiloguePlan?.PoppedEsi == 2
            && missingThirdPop.EpiloguePlan?.StackPopEbxAddress == 0x1008
            && missingThirdPop.UnreachedCallCellTargetCaptureProvided
            && missingThirdPop.StopAtRawFileOffsetExclusive == 0x012C944A);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingCallTarget =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, callTarget: null,
                    callReturnedNormally: true, postCallEbp: 0x5000));
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingCallStatus =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, callTarget: 0x12345678,
                    callReturnedNormally: null, postCallEbp: 0x5000));
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan opaqueNoReturn =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, callTarget: 0xCAFEBABE,
                    callReturnedNormally: false, postCallEbp: 0x5000));
        check("Opaque-call target and return observations are independent and the target is never invoked",
            missingCallTarget.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallCellTarget
            && missingCallTarget.EpiloguePlan?.CallCellReadReached == true
            && !missingCallTarget.ChildCallReached
            && missingCallTarget.UnreachedCallReturnStatusProvided
            && missingCallTarget.StopAtRawFileOffsetExclusive == 0x012C9452
            && missingCallStatus.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallReturnStatus
            && missingCallStatus.OpaqueCallTargetCaptured
            && missingCallStatus.ChildCallReached
            && missingCallStatus.OpaqueCallReturnedNormally is null
            && missingCallStatus.UnreachedPostCallEbpCaptureProvided
            && missingCallStatus.StopAtRawFileOffsetExclusive == 0x012C9452
            && opaqueNoReturn.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally
            && opaqueNoReturn.OpaqueCallTargetCaptured
            && opaqueNoReturn.OpaqueCallReturnedNormally == false
            && opaqueNoReturn.UnreachedPostCallEbpCaptureProvided
            && !opaqueNoReturn.OpaqueCallExecuted
            && !opaqueNoReturn.ChildExactCleanExitAdmitted);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan opaqueReturn =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, callTarget: 0x12345678,
                    callReturnedNormally: true, postCallEbp: 0x5000));
        check("A normally observed opaque return makes all six flags unknown and reaches RET 945B only",
            opaqueReturn.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally
            && opaqueReturn.ChildCallReached
            && opaqueReturn.OpaqueCallTargetCaptured
            && opaqueReturn.OpaqueCallReturnedNormally == true
            && !opaqueReturn.OpaqueCallExecuted
            && opaqueReturn.EpiloguePlan?.PostCallEbp == 0x5000
            && opaqueReturn.EpiloguePlan?.MovEspSourceEbp == 0x5000
            && opaqueReturn.EpiloguePlan?.ReturnTargetStackAddress == 0x5004
            && opaqueReturn.ChildReturnReached
            && opaqueReturn.EpiloguePlan?.ReturnTargetVirtualAddress == 0x5555
            && opaqueReturn.EpiloguePlan?.StopAtRawFileOffsetExclusive == 0x012C945C
            && opaqueReturn.StopAtRawFileOffsetExclusive == 0x012C945C
            && opaqueReturn.FlagsAtStop is
            {
                Carry: null, Parity: null, Auxiliary: null,
                Zero: null, Sign: null, Overflow: null
            }
            && opaqueReturn.ChildExactCleanExitAdmitted
            && opaqueReturn.ExactCleanCompletion
            && !opaqueReturn.DestinationEntered);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan nonzeroParentBypass =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(edi: 0x01000000, lateFreshGlobal: 0x80000000,
                    counter: null, childEsp: null, childEbp: null, testedByte: null,
                    popEdi: null, popEsi: null, popEbx: null, callTarget: null,
                    callReturnedNormally: null, postCallEbp: null, popEbp: null,
                    returnTarget: null));
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan dirtyNonzeroParentBypass =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(edi: 0x01000000, lateFreshGlobal: 0x80000000,
                    counter: null, childEsp: null, childEbp: null, testedByte: 0));
        check("A nonzero parent TEST bypasses cleanup and keeps bounded completion distinct from unused child captures",
            nonzeroParentBypass.ParentOutcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .NonzeroTestResultBypassesCleanupAt93C0
            && nonzeroParentBypass.ParentPlan.ParentChild.Parent.FreshMaskGate?.TestResult
                == 0x80000000
            && nonzeroParentBypass.ParentPlan.SelectedDestinationRawFileOffset == 0x012C93C0
            && nonzeroParentBypass.ParentExactNonzeroTestBypass
            && nonzeroParentBypass.EpiloguePlan is null
            && !nonzeroParentBypass.Internal9441Entered
            && nonzeroParentBypass.ExactBoundedExitAdmitted
            && nonzeroParentBypass.ExactCleanCompletion
            && !nonzeroParentBypass.DestinationEntered
            && dirtyNonzeroParentBypass.ParentExactNonzeroTestBypass
            && dirtyNonzeroParentBypass.ExactBoundedExitAdmitted
            && !dirtyNonzeroParentBypass.ExactCleanCompletion
            && dirtyNonzeroParentBypass.UnreachedTestedByteCaptureProvided
            && dirtyNonzeroParentBypass.ChildEntryPending == false);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan missingParent =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(edi: null, testedByte: 0, callTarget: 0x1234));
        check("Missing parent input never admits the internal epilogue and leaves later captures unused",
            missingParent.ParentOutcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome.MissingEntryEDIAt9354
            && missingParent.EpiloguePlan is null
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C9354
            && missingParent.UnreachedTestedByteCaptureProvided
            && missingParent.UnreachedCallCellTargetCaptureProvided
            && !missingParent.ChildEntryValidated
            && !missingParent.Internal9441Entered);

        uint counterAddress =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.CounterVirtualAddress;
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan aliasedCounter =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(
                    edi: 0x00FFFF00,
                    lateEntryEbp: 0x2000,
                    esi: counterAddress - 0x1C,
                    lateFreshGlobal: 1,
                    counter: 2,
                    testedByte: 0));
        check("An aliased parent node write is followed by an independent fresh counter read without forwarding",
            aliasedCounter.ParentPlan.ParentChild.Parent.Writes.Any(write =>
                write.Address == counterAddress && write.Value == 31)
            && aliasedCounter.ParentPlan.CounterGate.CounterAccesses[0].Address == counterAddress
            && aliasedCounter.ParentPlan.CounterGate.CounterAccesses[0].Value == 2
            && aliasedCounter.ParentPlan.FreshCleanupCounterForwardedFromPriorWrite == false
            && aliasedCounter.ParentPlan.OrderedMemoryEffects.All(effect =>
                !effect.ForwardedFromPriorWrite)
            && aliasedCounter.ParentPlan.CounterReadAfterParentMemoryEffects
            && aliasedCounter.ParentExactNonzeroEntryHandoff
            && aliasedCounter.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan wrappedStack =
            CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, childEsp: 0xFFFFFFFC, childEbp: 0xFFFFFFFC,
                    testedByte: 0, popEdi: 1, popEsi: 2, popEbx: 3,
                    popEbp: 4, returnTarget: 5));
        check("Stack-slot arithmetic wraps at 32 bits and the bounded RET ends before 945C",
            wrappedStack.EpiloguePlan?.StackPopEdiAddress == 0xFFFFFFFC
            && wrappedStack.EpiloguePlan?.StackPopEsiAddress == 0
            && wrappedStack.EpiloguePlan?.StackPopEbxAddress == 4
            && wrappedStack.EpiloguePlan?.StackPopEbpAddress == 0xFFFFFFFC
            && wrappedStack.EpiloguePlan?.ReturnTargetStackAddress == 0
            && wrappedStack.ChildReturnReached
            && wrappedStack.StopAtRawFileOffsetExclusive == 0x012C945C
            && !wrappedStack.DestinationEntered);
    }

    private static CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleCapture Capture(
        uint? edi = 0,
        uint? lateEntryEbp = null,
        uint? esi = 0x3000,
        uint? lateFreshGlobal = 0,
        uint? counter = 2,
        uint? childEsp = 0x1000,
        uint? childEbp = 0x4000,
        byte? testedByte = 0,
        uint? popEdi = 0x1111,
        uint? popEsi = 0x2222,
        uint? popEbx = 0x3333,
        uint? callTarget = null,
        bool? callReturnedNormally = null,
        uint? postCallEbp = null,
        uint? popEbp = 0x4444,
        uint? returnTarget = 0x5555) =>
        new(
            edi,
            lateEntryEbp,
            esi,
            lateFreshGlobal,
            counter,
            childEsp,
            childEbp,
            testedByte,
            popEdi,
            popEsi,
            popEbx,
            callTarget,
            callReturnedNormally,
            postCallEbp,
            popEbp,
            returnTarget);
}
