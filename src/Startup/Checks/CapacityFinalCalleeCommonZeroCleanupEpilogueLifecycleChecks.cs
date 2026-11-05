using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingParent =
            Project(
                ParentEntryECX: null,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                FreshCounterDword: 5,
                LateChildEsp: 0x4000,
                LateChildEbp: 0x5000,
                TestedByte: 0,
                StackPopEdiValue: 0x10,
                StackPopEsiValue: 0x20,
                StackPopEbxValue: 0x30,
                CallCellTargetVirtualAddress: 0x00405000,
                CallReturnedNormally: true,
                PostCallEbp: 0x6000,
                StackPopEbpValue: 0x7000,
                ReturnTargetVirtualAddress: 0x00401234);
        check("Missing threshold entry prevents child entry and exposes all supplied child captures as unused",
            missingParent.Outcome
                == CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                    .ParentNotAdmittedToChild
            && missingParent.ParentOutcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .ThresholdCompareOperandsMissing
            && !missingParent.ParentExactNonzeroEntryHandoff
            && !missingParent.ChildEntryTargetEntered
            && missingParent.LateChildEspProvidedButUnused
            && missingParent.LateChildEbpProvidedButUnused
            && missingParent.UnreachedTestedByteCaptureProvided
            && missingParent.UnreachedStackPopEdiCaptureProvided
            && missingParent.UnreachedStackPopEsiCaptureProvided
            && missingParent.UnreachedStackPopEbxCaptureProvided
            && missingParent.UnreachedCallCellTargetCaptureProvided
            && missingParent.UnreachedCallReturnStatusProvided
            && missingParent.UnreachedPostCallEbpCaptureProvided
            && missingParent.UnreachedStackPopEbpCaptureProvided
            && missingParent.UnreachedReturnTargetCaptureProvided
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan belowThreshold =
            Project(
                ParentEntryECX: 0x0FFF,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                FreshCounterDword: 5,
                LateChildEsp: 0x4000,
                LateChildEbp: 0x5000,
                TestedByte: null,
                StackPopEdiValue: null,
                StackPopEsiValue: null,
                StackPopEbxValue: null,
                CallCellTargetVirtualAddress: null,
                CallReturnedNormally: null,
                PostCallEbp: null,
                StackPopEbpValue: null,
                ReturnTargetVirtualAddress: null);
        check("Unsigned-below threshold exits before stores and leaves all child captures unused",
            belowThreshold.ParentOutcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .UnsignedBelowBranchTarget
            && belowThreshold.ParentPlan.ParentCompareFlags is
            {
                Carry: true,
                Zero: false
            }
            && belowThreshold.ParentPlan.Writes.IsEmpty
            && !belowThreshold.ParentPlan.CleanupCounterGateEntryReached
            && !belowThreshold.ParentExactNonzeroEntryHandoff
            && !belowThreshold.ChildEntryTargetEntered
            && belowThreshold.LateChildEspProvidedButUnused
            && belowThreshold.LateChildEbpProvidedButUnused
            && belowThreshold.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingGlobal =
            Project(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: null,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                FreshCounterDword: 5,
                LateChildEsp: 0x4000,
                LateChildEbp: 0x5000,
                TestedByte: null,
                StackPopEdiValue: null,
                StackPopEsiValue: null,
                StackPopEbxValue: null,
                CallCellTargetVirtualAddress: null,
                CallReturnedNormally: null,
                PostCallEbp: null,
                StackPopEbpValue: null,
                ReturnTargetVirtualAddress: null);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingESI =
            Project(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: null,
                LateEntryEDX: 0x3000,
                FreshCounterDword: 5,
                LateChildEsp: null,
                LateChildEbp: null,
                TestedByte: null,
                StackPopEdiValue: null,
                StackPopEsiValue: null,
                StackPopEbxValue: null,
                CallCellTargetVirtualAddress: null,
                CallReturnedNormally: null,
                PostCallEbp: null,
                StackPopEbpValue: null,
                ReturnTargetVirtualAddress: null);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingEDX =
            Project(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: null,
                FreshCounterDword: 5,
                LateChildEsp: null,
                LateChildEbp: null,
                TestedByte: null,
                StackPopEdiValue: null,
                StackPopEsiValue: null,
                StackPopEbxValue: null,
                CallCellTargetVirtualAddress: null,
                CallReturnedNormally: null,
                PostCallEbp: null,
                StackPopEbpValue: null,
                ReturnTargetVirtualAddress: null);
        check("Missing global or either ordered store operand prevents the cleanup-counter handoff",
            missingGlobal.ParentOutcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .ThresholdCompareOperandsMissing
            && missingGlobal.ParentPlan.StoresLifecycle.ThresholdGate.Reads.Length == 1
            && missingGlobal.ParentPlan.Writes.IsEmpty
            && !missingGlobal.ParentPlan.CleanupCounterGateEntryReached
            && missingESI.ParentOutcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .MissingLateEntryESIForFirstStore
            && missingESI.ParentPlan.Writes.Length == 1
            && missingESI.ParentPlan.Writes[0].Attempted
            && !missingESI.ParentPlan.Writes[0].Committed
            && missingEDX.ParentOutcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .MissingLateEntryEDXForSecondStore
            && missingEDX.ParentPlan.Writes.Length == 2
            && missingEDX.ParentPlan.Writes[0].Committed
            && missingEDX.ParentPlan.Writes[1].Attempted
            && !missingEDX.ParentPlan.Writes[1].Committed
            && !missingGlobal.ParentExactNonzeroEntryHandoff
            && !missingESI.ParentExactNonzeroEntryHandoff
            && !missingEDX.ParentExactNonzeroEntryHandoff);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingCounter =
            Project(
                ParentEntryECX: 0x1000,
                FreshGlobalValue: 0x1000,
                LateEntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                FreshCounterDword: null,
                LateChildEsp: 0x4000,
                LateChildEbp: 0x5000,
                TestedByte: 0,
                StackPopEdiValue: 1,
                StackPopEsiValue: 2,
                StackPopEbxValue: 3,
                CallCellTargetVirtualAddress: null,
                CallReturnedNormally: null,
                PostCallEbp: null,
                StackPopEbpValue: 4,
                ReturnTargetVirtualAddress: 0x00401234);
        check("A missing fresh counter is read only after four committed stores and blocks child admission",
            missingCounter.ParentOutcome
                == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .MissingFreshCounterDword
            && missingCounter.ParentPlan.AllFourStoresCommitted
            && missingCounter.ParentPlan.CounterReadAttempted
            && !missingCounter.ParentPlan.CounterReadValueCaptured
            && !missingCounter.ParentPlan.CounterWriteCommitted
            && missingCounter.ParentPlan.OrderedMemoryEffects.Select(
                effect => effect.RawFileOffset).SequenceEqual(
                new uint[]
                {
                    0x012C945C,
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C942F
                })
            && !missingCounter.ParentExactNonzeroEntryHandoff
            && !missingCounter.ChildEntryTargetEntered
            && missingCounter.LateChildEspProvidedButUnused
            && missingCounter.UnreachedTestedByteCaptureProvided
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan zeroClean =
            ProjectComplete(
                counter: 1,
                lateChildEsp: null,
                lateChildEbp: null,
                testedByte: null,
                stackPopEdiValue: null,
                stackPopEsiValue: null,
                stackPopEbxValue: null,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan zeroDirty =
            ProjectComplete(
                counter: 1,
                lateChildEsp: 0x4000,
                lateChildEbp: 0x5000,
                testedByte: 0,
                stackPopEdiValue: null,
                stackPopEsiValue: null,
                stackPopEbxValue: null,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        check("DEC-to-zero stays pre-call at 943C: clean captures complete, but dirty child captures remain unused",
            zeroClean.Outcome
                == CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                    .ParentDecrementedToZero
            && zeroClean.ParentExactZeroBypass
            && zeroClean.ExactCleanCompletion
            && zeroClean.EpiloguePlan is null
            && zeroClean.ParentPlan.CleanupCounterGate.JnzTaken == false
            && zeroClean.ParentPlan.CleanupCounterGate.SelectedDestinationRawFileOffset
                == 0x012C9437
            && zeroClean.ParentPlan.ZeroPathFallthroughEntered
            && !zeroClean.DirectCallReached
            && !zeroClean.CalleeEntered
            && !zeroClean.ChildEntryTargetEntered
            && zeroClean.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroDirty.ParentExactZeroBypass
            && zeroDirty.EpiloguePlan is null
            && zeroDirty.LateChildEspProvidedButUnused
            && zeroDirty.LateChildEbpProvidedButUnused
            && zeroDirty.UnreachedTestedByteCaptureProvided
            && !zeroDirty.ExactCleanCompletion
            && !zeroDirty.ChildEntryTargetEntered
            && zeroDirty.StopAtRawFileOffsetExclusive == 0x012C943C);
        check("The exclusive return boundary 945C is after RET at 945B",
            CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleProjection
                .ReturnBoundaryRawFileOffsetExclusive == 0x012C945C
            && zeroClean.ChildReturnInstructionRawFileOffset == 0x012C945B
            && zeroClean.StopAtRawFileOffsetExclusive
                != zeroClean.ChildReturnInstructionRawFileOffset);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan zeroCounter =
            ProjectComplete(
                counter: 0,
                lateChildEsp: 0x4000,
                lateChildEbp: 0x5000,
                testedByte: null,
                stackPopEdiValue: null,
                stackPopEsiValue: null,
                stackPopEbxValue: null,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan maxCounter =
            ProjectComplete(
                counter: uint.MaxValue,
                lateChildEsp: null,
                lateChildEbp: null,
                testedByte: null,
                stackPopEdiValue: null,
                stackPopEsiValue: null,
                stackPopEbxValue: null,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan signBitCounter =
            ProjectComplete(
                counter: 0x80000000,
                lateChildEsp: null,
                lateChildEbp: null,
                testedByte: null,
                stackPopEdiValue: null,
                stackPopEsiValue: null,
                stackPopEbxValue: null,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        check("DEC arithmetic preserves the exact nonzero JNZ boundary, including zero and sign wrap",
            zeroCounter.ParentPlan.CleanupCounterGate.CounterAfter == uint.MaxValue
            && zeroCounter.ParentPlan.CleanupCounterGate.JnzTaken == true
            && zeroCounter.ParentPlan.CleanupCounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zeroCounter.ParentExactNonzeroEntryHandoff
            && !zeroCounter.NonzeroBranchTargetEntered
            && zeroCounter.StopAtRawFileOffsetExclusive == 0x012C9441
            && maxCounter.ParentPlan.CleanupCounterGate.CounterAfter == 0xFFFFFFFE
            && maxCounter.ParentExactNonzeroEntryHandoff
            && maxCounter.StopAtRawFileOffsetExclusive == 0x012C9441
            && signBitCounter.ParentPlan.CleanupCounterGate.CounterAfter == 0x7FFFFFFF
            && signBitCounter.ParentPlan.CleanupCounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signBitCounter.ParentExactNonzeroEntryHandoff);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingBothChildRegisters =
            ProjectComplete(
                counter: 2,
                lateChildEsp: null,
                lateChildEbp: null,
                testedByte: 0,
                stackPopEdiValue: 1,
                stackPopEsiValue: 2,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingChildEsp =
            ProjectComplete(
                counter: 2,
                lateChildEsp: null,
                lateChildEbp: 0x5000,
                testedByte: 0,
                stackPopEdiValue: 1,
                stackPopEsiValue: 2,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingChildEbp =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0x4000,
                lateChildEbp: null,
                testedByte: 0,
                stackPopEdiValue: 1,
                stackPopEsiValue: 2,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        check("Late child ESP and EBP are both required before constructing the gated child",
            missingBothChildRegisters.ParentExactNonzeroEntryHandoff
            && missingBothChildRegisters.Outcome
                == CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEspAndEbp
            && missingBothChildRegisters.EpiloguePlan is null
            && missingBothChildRegisters.LateChildEspProvidedButUnused == false
            && missingBothChildRegisters.LateChildEbpProvidedButUnused == false
            && missingBothChildRegisters.UnreachedTestedByteCaptureProvided
            && !missingBothChildRegisters.ChildEntryTargetEntered
            && missingChildEsp.Outcome
                == CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEsp
            && missingChildEsp.LateChildEbpProvidedButUnused
            && missingChildEsp.LateChildEspProvidedButUnused == false
            && missingChildEsp.EpiloguePlan is null
            && !missingChildEsp.ChildEntryTargetEntered
            && missingChildEbp.Outcome
                == CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEbp
            && missingChildEbp.LateChildEspProvidedButUnused
            && missingChildEbp.LateChildEbpProvidedButUnused == false
            && missingChildEbp.EpiloguePlan is null
            && !missingChildEbp.ChildEntryTargetEntered
            && missingChildEbp.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingTest =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0x4000,
                lateChildEbp: 0x5000,
                testedByte: null,
                stackPopEdiValue: 1,
                stackPopEsiValue: 2,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        check("The internal 9441 entry is counted exactly when the first TEST read is attempted",
            missingTest.ParentExactNonzeroEntryHandoff
            && missingTest.ChildEntryValidated
            && missingTest.ChildEntryTargetEntered
            && missingTest.ChildTestReadReached
            && missingTest.ParentMemoryEffectsCompleteBeforeChildTest
            && missingTest.ChildOutcome == CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte
            && missingTest.UnreachedStackPopEdiCaptureProvided
            && missingTest.UnreachedStackPopEsiCaptureProvided
            && missingTest.UnreachedStackPopEbxCaptureProvided
            && missingTest.ParentFlagsPreservedUntilChildTest
            && missingTest.StopAtRawFileOffsetExclusive == 0x012C9441
            && !missingTest.ExactCleanCompletion);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingEdi =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0x4000,
                lateChildEbp: 0x5000,
                testedByte: 0x02,
                stackPopEdiValue: null,
                stackPopEsiValue: 2,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: 0x00405000,
                callReturnedNormally: true,
                postCallEbp: 0x6000,
                stackPopEbpValue: 0x7000,
                returnTargetVirtualAddress: 0x00401234);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingEsi =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0x4000,
                lateChildEbp: 0x5000,
                testedByte: 0x02,
                stackPopEdiValue: 1,
                stackPopEsiValue: null,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: 0x00405000,
                callReturnedNormally: true,
                postCallEbp: 0x6000,
                stackPopEbpValue: 0x7000,
                returnTargetVirtualAddress: 0x00401234);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingEbx =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0x4000,
                lateChildEbp: 0x5000,
                testedByte: 0x02,
                stackPopEdiValue: 1,
                stackPopEsiValue: 2,
                stackPopEbxValue: null,
                callCellTargetVirtualAddress: 0x00405000,
                callReturnedNormally: true,
                postCallEbp: 0x6000,
                stackPopEbpValue: 0x7000,
                returnTargetVirtualAddress: 0x00401234);
        check("Each missing pop stops at its own instruction without consuming later captures",
            missingEdi.ChildOutcome == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEdiValue
            && missingEdi.ChildStackAccesses.Length == 1
            && missingEdi.ChildStackAccesses[0].Address == 0x4000
            && missingEdi.UnreachedStackPopEsiCaptureProvided
            && missingEdi.UnreachedStackPopEbxCaptureProvided
            && missingEdi.UnreachedCallCellTargetCaptureProvided
            && missingEdi.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingEdi.StopAtRawFileOffsetExclusive == 0x012C9448
            && missingEsi.ChildOutcome == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEsiValue
            && missingEsi.ChildStackAccesses.Length == 2
            && missingEsi.ChildStackAccesses[0].Address == 0x4000
            && missingEsi.ChildStackAccesses[1].Address == 0x4004
            && missingEsi.UnreachedStackPopEbxCaptureProvided
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C9449
            && missingEbx.ChildOutcome == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbxValue
            && missingEbx.ChildStackAccesses.Length == 3
            && missingEbx.ChildStackAccesses[2].Address == 0x4008
            && missingEbx.UnreachedCallCellTargetCaptureProvided
            && missingEbx.UnreachedCallReturnStatusProvided
            && missingEbx.StopAtRawFileOffsetExclusive == 0x012C944A);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan cleanCallSkip =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0xFFFFFFFC,
                lateChildEbp: 0xFFFFFFFC,
                testedByte: 0x00,
                stackPopEdiValue: 0x11111111,
                stackPopEsiValue: 0x22222222,
                stackPopEbxValue: 0x33333333,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: 0x44444444,
                returnTargetVirtualAddress: 0x00401234);
        check("The clean zero TEST skips the opaque call, wraps stack arithmetic, and returns at the exclusive boundary",
            cleanCallSkip.ParentExactNonzeroEntryHandoff
            && cleanCallSkip.ChildOutcome == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && cleanCallSkip.ChildEntryTargetEntered
            && cleanCallSkip.ChildExactCleanExitAdmitted
            && cleanCallSkip.ExactCleanCompletion
            && !cleanCallSkip.ChildCallReached
            && !cleanCallSkip.OpaqueCallTargetCaptured
            && !cleanCallSkip.OpaqueCallExecuted
            && cleanCallSkip.ChildReturnReached
            && cleanCallSkip.ChildReturnInstructionRawFileOffset == 0x012C945B
            && cleanCallSkip.StopAtRawFileOffsetExclusive == 0x012C945C
            && cleanCallSkip.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && cleanCallSkip.ParentFlagsPreservedUntilChildTest == false
            && cleanCallSkip.ChildStackAccesses.Select(access => access.Address)
                .SequenceEqual(new uint[]
                {
                    0xFFFFFFFC,
                    0x00000000,
                    0x00000004,
                    0xFFFFFFFC,
                    0x00000000
                })
            && !cleanCallSkip.UnreachedCallCellTargetCaptureProvided
            && !cleanCallSkip.UnreachedCallReturnStatusProvided);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan opaqueCallReturned =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0x1000,
                lateChildEbp: 0x2000,
                testedByte: 0x02,
                stackPopEdiValue: 1,
                stackPopEsiValue: 2,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: 0x00405000,
                callReturnedNormally: true,
                postCallEbp: 0x3000,
                stackPopEbpValue: 4,
                returnTargetVirtualAddress: 0x00401234);
        check("A captured opaque normal return leaves every flag unknown and is never executed by the projection",
            opaqueCallReturned.ChildCallReached
            && opaqueCallReturned.OpaqueCallTargetCaptured
            && opaqueCallReturned.OpaqueCallReturnedNormally == true
            && !opaqueCallReturned.OpaqueCallExecuted
            && opaqueCallReturned.ChildReturnReached
            && opaqueCallReturned.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && opaqueCallReturned.ChildExactCleanExitAdmitted
            && opaqueCallReturned.ExactCleanCompletion
            && opaqueCallReturned.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan opaqueCallDidNotReturn =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0x1000,
                lateChildEbp: 0x2000,
                testedByte: 0x02,
                stackPopEdiValue: 1,
                stackPopEsiValue: 2,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: 0x00405000,
                callReturnedNormally: false,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        check("A no-return observation stops at CALL and retains only the pre-call TEST flags",
            opaqueCallDidNotReturn.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally
            && opaqueCallDidNotReturn.ChildCallReached
            && opaqueCallDidNotReturn.OpaqueCallTargetCaptured
            && opaqueCallDidNotReturn.OpaqueCallReturnedNormally == false
            && !opaqueCallDidNotReturn.OpaqueCallExecuted
            && !opaqueCallDidNotReturn.ChildReturnReached
            && opaqueCallDidNotReturn.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && opaqueCallDidNotReturn.StopAtRawFileOffsetExclusive == 0x012C9452
            && !opaqueCallDidNotReturn.ExactCleanCompletion);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingCallCell =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0x1000,
                lateChildEbp: 0x2000,
                testedByte: 0x02,
                stackPopEdiValue: 1,
                stackPopEsiValue: 2,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: null,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan missingCallStatus =
            ProjectComplete(
                counter: 2,
                lateChildEsp: 0x1000,
                lateChildEbp: 0x2000,
                testedByte: 0x02,
                stackPopEdiValue: 1,
                stackPopEsiValue: 2,
                stackPopEbxValue: 3,
                callCellTargetVirtualAddress: 0x00405000,
                callReturnedNormally: null,
                postCallEbp: null,
                stackPopEbpValue: null,
                returnTargetVirtualAddress: null);
        check("Call-cell and return-status captures are independently required and preserve pre-call flags",
            missingCallCell.ChildOutcome == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallCellTarget
            && missingCallCell.ChildTestReadReached
            && !missingCallCell.OpaqueCallTargetCaptured
            && !missingCallCell.ChildCallReached
            && missingCallCell.FlagsAtStop is { Carry: false, Parity: false, Zero: false }
            && missingCallCell.StopAtRawFileOffsetExclusive == 0x012C9452
            && missingCallStatus.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallReturnStatus
            && missingCallStatus.OpaqueCallTargetCaptured
            && missingCallStatus.ChildCallReached
            && missingCallStatus.OpaqueCallReturnedNormally is null
            && missingCallStatus.UnreachedPostCallEbpCaptureProvided == false
            && missingCallStatus.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingCallStatus.StopAtRawFileOffsetExclusive == 0x012C9452);

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan aliasedCounter =
            Project(
                ParentEntryECX: 0x01EDADE8,
                FreshGlobalValue: 0x01EDADE8,
                LateEntryESI: 0x01EDADE8,
                LateEntryEDX: 0xAABBCCDD,
                FreshCounterDword: 7,
                LateChildEsp: null,
                LateChildEbp: null,
                TestedByte: null,
                StackPopEdiValue: null,
                StackPopEsiValue: null,
                StackPopEbxValue: null,
                CallCellTargetVirtualAddress: null,
                CallReturnedNormally: null,
                PostCallEbp: null,
                StackPopEbpValue: null,
                ReturnTargetVirtualAddress: null);
        check("The aliased store and later counter read remain separate fresh observations in ordered parent effects",
            aliasedCounter.ParentExactNonzeroEntryHandoff
            && aliasedCounter.ParentPlan.OrderedMemoryEffects.Length == 7
            && aliasedCounter.ParentPlan.OrderedMemoryEffects[1].Address == 0x01EDADE8
            && aliasedCounter.ParentPlan.OrderedMemoryEffects[1].Value == 0x01EDADE8
            && aliasedCounter.ParentPlan.OrderedMemoryEffects[5].Address == 0x01EDADE8
            && aliasedCounter.ParentPlan.OrderedMemoryEffects[5].Value == 7
            && aliasedCounter.ParentPlan.OrderedMemoryEffects[6].Value == 6
            && aliasedCounter.ParentPlan.CounterValueIsIndependentFreshCapture
            && aliasedCounter.ParentMemoryEffectsCompleteBeforeChildTest == false
            && !aliasedCounter.ChildEntryTargetEntered
            && aliasedCounter.StopAtRawFileOffsetExclusive == 0x012C9441);
    }

    private static CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan ProjectComplete(
        uint counter,
        uint? lateChildEsp,
        uint? lateChildEbp,
        byte? testedByte,
        uint? stackPopEdiValue,
        uint? stackPopEsiValue,
        uint? stackPopEbxValue,
        uint? callCellTargetVirtualAddress,
        bool? callReturnedNormally,
        uint? postCallEbp,
        uint? stackPopEbpValue,
        uint? returnTargetVirtualAddress) =>
        Project(
            ParentEntryECX: 0x1000,
            FreshGlobalValue: 0x1000,
            LateEntryESI: 0x2000,
            LateEntryEDX: 0x3000,
            FreshCounterDword: counter,
            LateChildEsp: lateChildEsp,
            LateChildEbp: lateChildEbp,
            TestedByte: testedByte,
            StackPopEdiValue: stackPopEdiValue,
            StackPopEsiValue: stackPopEsiValue,
            StackPopEbxValue: stackPopEbxValue,
            CallCellTargetVirtualAddress: callCellTargetVirtualAddress,
            CallReturnedNormally: callReturnedNormally,
            PostCallEbp: postCallEbp,
            StackPopEbpValue: stackPopEbpValue,
            ReturnTargetVirtualAddress: returnTargetVirtualAddress);

    private static CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan Project(
        uint? ParentEntryECX,
        uint? FreshGlobalValue,
        uint? LateEntryESI,
        uint? LateEntryEDX,
        uint? FreshCounterDword,
        uint? LateChildEsp,
        uint? LateChildEbp,
        byte? TestedByte,
        uint? StackPopEdiValue,
        uint? StackPopEsiValue,
        uint? StackPopEbxValue,
        uint? CallCellTargetVirtualAddress,
        bool? CallReturnedNormally,
        uint? PostCallEbp,
        uint? StackPopEbpValue,
        uint? ReturnTargetVirtualAddress) =>
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleProjection.Project(new(
            ParentEntryECX,
            FreshGlobalValue,
            LateEntryESI,
            LateEntryEDX,
            FreshCounterDword,
            LateChildEsp,
            LateChildEbp,
            TestedByte,
            StackPopEdiValue,
            StackPopEsiValue,
            StackPopEbxValue,
            CallCellTargetVirtualAddress,
            CallReturnedNormally,
            PostCallEbp,
            StackPopEbpValue,
            ReturnTargetVirtualAddress));
}
