using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition uses the exact cleanup-to-epilogue and exclusive RET boundaries",
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.EntryRawFileOffset
                == 0x012C9403
            && CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection
                .ChildEntryRawFileOffset == 0x012C9441
            && CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection
                .ZeroCounterStopRawFileOffset == 0x012C943C
            && CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection
                .ReturnBoundaryRawFileOffsetExclusive == 0x012C945C
            && CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset == 0x012C945B
            && CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive
                == 0x012C945C);

        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan cleanSkip =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 0));
        check("An exact nonzero DEC/JNZ handoff validates the child and cleanly skips its opaque call",
            cleanSkip.ParentOutcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.DecrementedNonzero
            && cleanSkip.ParentExactNonzeroEntryHandoff
            && cleanSkip.ParentPlan.ParentRingBoundaryReached
            && !cleanSkip.ParentPlan.ParentRingBoundaryInstructionEntered
            && cleanSkip.ParentPlan.CleanupCounterPlan?.JnzReached == true
            && cleanSkip.ParentPlan.CleanupCounterPlan?.JnzTaken == true
            && cleanSkip.ParentPlan.CleanupCounterPlan?.SelectedDestinationRawFileOffset
                == 0x012C9441
            && cleanSkip.ParentPlan.CleanupCounterPlan?.StopAtRawFileOffsetExclusive
                == 0x012C9441
            && cleanSkip.ParentPlan.ParentPlan.Writes.Length == 5
            && cleanSkip.ParentPlan.ParentPlan.Writes.All(write => write.Committed)
            && cleanSkip.ParentPlan.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C9403,
                    0x012C9409,
                    0x012C941C,
                    0x012C941F,
                    0x012C9422,
                    0x012C9425,
                    0x012C9428,
                    0x012C942F,
                    0x012C942F
                })
            && cleanSkip.ChildEntryValidated
            && cleanSkip.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && cleanSkip.EpiloguePlan?.TestedByteCaptured == true
            && cleanSkip.EpiloguePlan?.BranchTaken == true
            && !cleanSkip.ChildCallReached
            && !cleanSkip.OpaqueCallTargetCaptured
            && !cleanSkip.OpaqueCallExecuted
            && cleanSkip.ChildReturnReached
            && cleanSkip.StopAtRawFileOffsetExclusive == 0x012C945C
            && cleanSkip.ChildExactCleanExitAdmitted
            && cleanSkip.ExactCleanCompletion
            && cleanSkip.ParentFlagsAtChildEntry is { Carry: false }
            && cleanSkip.FlagsAtStop is { Carry: false, Zero: true }
            && !cleanSkip.ParentFlagsPreservedUntilChildTest
            && cleanSkip.ChildStackAccesses.Select(access => access.Kind).SequenceEqual(
                new[]
                {
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEdi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEsi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbx,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbp,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.ReturnTarget
                }));

        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan cleanCallReturn =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(
                    counter: 2,
                    testedByte: 2,
                    callTarget: 0x12345678,
                    callReturnedNormally: true,
                    postCallEbp: 0x5000));
        check("A set TEST bit keeps stack and post-call EBP captures independent and observes, but never invokes, the opaque call",
            cleanCallReturn.ParentExactNonzeroEntryHandoff
            && cleanCallReturn.ChildEntryValidated
            && cleanCallReturn.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally
            && cleanCallReturn.EpiloguePlan?.TestedByte == 2
            && cleanCallReturn.EpiloguePlan?.StackPopEdiAddress == 0x1000
            && cleanCallReturn.EpiloguePlan?.StackPopEsiAddress == 0x1004
            && cleanCallReturn.EpiloguePlan?.StackPopEbxAddress == 0x1008
            && cleanCallReturn.EpiloguePlan?.PushImmediateAddress == 0x1008
            && cleanCallReturn.OpaqueCallTargetCaptured
            && cleanCallReturn.EpiloguePlan?.CallCellTargetVirtualAddress == 0x12345678
            && cleanCallReturn.OpaqueCallReturnedNormally == true
            && cleanCallReturn.FlagsAtStop is
            {
                Carry: null, Parity: null, Auxiliary: null,
                Zero: null, Sign: null, Overflow: null
            }
            && !cleanCallReturn.OpaqueCallExecuted
            && cleanCallReturn.EpiloguePlan?.PostCallEbp == 0x5000
            && cleanCallReturn.EpiloguePlan?.MovEspSourceEbp == 0x5000
            && cleanCallReturn.EpiloguePlan?.StackPopEbpAddress == 0x5000
            && cleanCallReturn.EpiloguePlan?.ReturnTargetStackAddress == 0x5004
            && cleanCallReturn.ChildStackAccesses.Select(access => access.Kind).SequenceEqual(
                new[]
                {
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEdi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEsi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbx,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PushImmediate,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbp,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.ReturnTarget
                })
            && cleanCallReturn.ChildExactCleanExitAdmitted
            && cleanCallReturn.StopAtRawFileOffsetExclusive == 0x012C945C
            && cleanCallReturn.ExactCleanCompletion);

        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan zeroCounter =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 1, testedByte: 2, callTarget: 0x1234,
                    callReturnedNormally: true, postCallEbp: 0x5000));
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan cleanZeroCounter =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 1, childEsp: null, childEbp: null, testedByte: null,
                    popEdi: null, popEsi: null, popEbx: null, popEbp: null,
                    returnTarget: null));
        check("A zero DEC result follows the pre-CALL 943C path and exposes all later captures as unused",
            zeroCounter.ParentOutcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.DecrementedToZero
            && zeroCounter.ParentExactZeroBypass
            && zeroCounter.ParentPlan.CleanupCounterPlan?.JnzTaken == false
            && zeroCounter.ParentPlan.CleanupCounterPlan?.SelectedDestinationRawFileOffset
                == CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
            && zeroCounter.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroCounter.EpiloguePlan is null
            && !zeroCounter.ChildEntryValidated
            && !zeroCounter.ChildTestReadReached
            && !zeroCounter.ChildCallReached
            && !zeroCounter.OpaqueCallExecuted
            && zeroCounter.UnreachedTestedByteCaptureProvided
            && zeroCounter.UnreachedStackPopEdiCaptureProvided
            && zeroCounter.UnreachedCallCellTargetCaptureProvided
            && zeroCounter.UnreachedCallReturnStatusProvided
            && zeroCounter.UnreachedPostCallEbpCaptureProvided
            && zeroCounter.UnreachedReturnTargetCaptureProvided
            && !zeroCounter.ExactCleanCompletion
            && cleanZeroCounter.ParentExactZeroBypass
            && cleanZeroCounter.EpiloguePlan is null
            && cleanZeroCounter.StopAtRawFileOffsetExclusive == 0x012C943C
            && cleanZeroCounter.ExactCleanCompletion);

        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan missingParent =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(global: null, counter: 2, testedByte: 0,
                    callTarget: 0x1234, callReturnedNormally: true));
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan borrowedParent =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(global: 0x200, parentEDX: 0x100, freshEDXPlus8: 0x100,
                    counter: 2, testedByte: 0, popEdi: 1, popEsi: 2, popEbx: 3));
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan missingCounter =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: null, testedByte: 0, callTarget: 0x1234));
        check("Missing or borrowed parent routes never validate the child and retain supplied epilogue captures as unused",
            missingParent.ParentOutcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.MissingFreshGlobalValue
            && missingParent.EpiloguePlan is null
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C9403
            && missingParent.UnreachedTestedByteCaptureProvided
            && missingParent.UnreachedCallCellTargetCaptureProvided
            && !missingParent.ChildEntryValidated
            && borrowedParent.ParentOutcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .FirstUnsignedBelowBypassesChild
            && borrowedParent.EpiloguePlan is null
            && borrowedParent.ParentPlan.ParentPlan.ParentPlan.FirstJBTaken == true
            && borrowedParent.UnreachedTestedByteCaptureProvided
            && borrowedParent.UnreachedStackPopEdiCaptureProvided
            && !borrowedParent.ChildEntryValidated
            && missingCounter.ParentOutcome
                == CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.MissingFreshCounterDword
            && missingCounter.ParentPlan.ParentRingBoundaryReached
            && missingCounter.EpiloguePlan is null
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F
            && missingCounter.UnreachedTestedByteCaptureProvided
            && missingCounter.UnreachedCallCellTargetCaptureProvided);

        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan missingEsp =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, childEsp: null, childEbp: 0x5000,
                    testedByte: 0, popEdi: 1));
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan missingEbp =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, childEsp: 0x1000, childEbp: null,
                    testedByte: 0, popEdi: 1));
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan missingBothEntryRegisters =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, childEsp: null, childEbp: null,
                    testedByte: 0, popEdi: 1));
        check("Missing late ESP or EBP stops at 9441 without fabricating zero and retains the known other operand",
            missingEsp.Outcome
                == CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEsp
            && missingEsp.ParentExactNonzeroEntryHandoff
            && missingEsp.Capture.LateChildEsp is null
            && missingEsp.Capture.LateChildEbp == 0x5000
            && missingEsp.EpiloguePlan is null
            && missingEsp.ChildEntryPending
            && missingEsp.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingEsp.LateChildEbpProvidedButUnused
            && missingEsp.UnreachedTestedByteCaptureProvided
            && missingEsp.UnreachedStackPopEdiCaptureProvided
            && missingEbp.Outcome
                == CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEbp
            && missingEbp.Capture.LateChildEsp == 0x1000
            && missingEbp.Capture.LateChildEbp is null
            && missingEbp.EpiloguePlan is null
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingEbp.LateChildEspProvidedButUnused
            && missingEbp.UnreachedTestedByteCaptureProvided
            && missingBothEntryRegisters.Outcome
                == CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEspAndEbp
            && missingBothEntryRegisters.EpiloguePlan is null
            && missingBothEntryRegisters.ChildEntryPending
            && missingBothEntryRegisters.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingBothEntryRegisters.UnreachedTestedByteCaptureProvided);

        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan missingTestByte =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: null,
                    popEdi: 1, popEsi: 2, popEbx: 3, popEbp: 4, returnTarget: 5));
        check("A reached but uncaptured fresh TEST byte leaves parent DEC flags at 9441",
            missingTestByte.ChildEntryValidated
            && missingTestByte.ChildTestReadReached
            && missingTestByte.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte
            && missingTestByte.EpiloguePlan?.TestedByteCaptured == false
            && missingTestByte.ParentFlagsAtChildEntry is { Carry: false }
            && missingTestByte.FlagsAtStop is
            {
                Carry: false, Parity: false, Auxiliary: false,
                Zero: false, Sign: false, Overflow: false
            }
            && missingTestByte.ParentFlagsPreservedUntilChildTest
            && missingTestByte.UnreachedStackPopEdiCaptureProvided
            && missingTestByte.UnreachedStackPopEbpCaptureProvided
            && missingTestByte.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan missingFirstPop =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, popEdi: null,
                    popEsi: 2, popEbx: 3, callTarget: 0x1234,
                    callReturnedNormally: true, postCallEbp: 0x5000));
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan missingSecondPop =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, popEdi: 1,
                    popEsi: null, popEbx: 3, callTarget: 0x1234));
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan missingThirdPop =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, popEdi: 1,
                    popEsi: 2, popEbx: null, callTarget: 0x1234));
        check("A missing ordered stack pop stops at its own instruction without consuming later captures",
            missingFirstPop.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEdiValue
            && missingFirstPop.EpiloguePlan?.StackPopEdiReached == true
            && missingFirstPop.EpiloguePlan?.StackPopEdiAddress == 0x1000
            && missingFirstPop.EpiloguePlan?.StackAccesses.Length == 1
            && missingFirstPop.EpiloguePlan?.StackAccesses[0].Value is null
            && missingFirstPop.UnreachedStackPopEsiCaptureProvided
            && missingFirstPop.UnreachedStackPopEbxCaptureProvided
            && missingFirstPop.UnreachedCallCellTargetCaptureProvided
            && missingFirstPop.UnreachedCallReturnStatusProvided
            && missingFirstPop.UnreachedPostCallEbpCaptureProvided
            && missingFirstPop.StopAtRawFileOffsetExclusive == 0x012C9448
            && missingSecondPop.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEsiValue
            && missingSecondPop.EpiloguePlan?.PoppedEdi == 1
            && missingSecondPop.EpiloguePlan?.StackPopEsiAddress == 0x1004
            && missingSecondPop.UnreachedStackPopEbxCaptureProvided
            && missingSecondPop.UnreachedCallCellTargetCaptureProvided
            && missingSecondPop.StopAtRawFileOffsetExclusive == 0x012C9449
            && missingThirdPop.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbxValue
            && missingThirdPop.EpiloguePlan?.PoppedEdi == 1
            && missingThirdPop.EpiloguePlan?.PoppedEsi == 2
            && missingThirdPop.EpiloguePlan?.StackPopEbxAddress == 0x1008
            && missingThirdPop.UnreachedCallCellTargetCaptureProvided
            && missingThirdPop.StopAtRawFileOffsetExclusive == 0x012C944A);

        CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan opaqueNoReturn =
            CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection.Project(
                Capture(counter: 2, testedByte: 2, callTarget: 0xCAFEBABE,
                    callReturnedNormally: false, postCallEbp: 0x5000));
        check("A captured opaque call with an observed nonreturning path stops at CALL and does not execute its target",
            opaqueNoReturn.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally
            && opaqueNoReturn.ChildCallReached
            && opaqueNoReturn.OpaqueCallTargetCaptured
            && opaqueNoReturn.EpiloguePlan?.CallCellTargetVirtualAddress == 0xCAFEBABE
            && opaqueNoReturn.OpaqueCallReturnedNormally == false
            && !opaqueNoReturn.OpaqueCallExecuted
            && opaqueNoReturn.EpiloguePlan?.PushImmediateReached == true
            && opaqueNoReturn.UnreachedPostCallEbpCaptureProvided
            && opaqueNoReturn.StopAtRawFileOffsetExclusive == 0x012C9452
            && !opaqueNoReturn.ChildExactCleanExitAdmitted);
    }

    private static CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleCapture Capture(
        uint? global = 0x80,
        uint? parentEDX = 0x100,
        uint? freshEDXPlus8 = 0x100,
        uint? lateESI = 0x200,
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
            global,
            parentEDX,
            freshEDXPlus8,
            lateESI,
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
