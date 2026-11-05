using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleProjection;

internal static class CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition uses the exact traversal cleanup target, internal TEST entry, and RET boundary",
            Projection.EntryRawFileOffset == 0x012C93D4
            && Projection.ChildEntryRawFileOffset == 0x012C9441
            && Projection.ZeroCounterStopRawFileOffset == 0x012C943C
            && Projection.ReturnBoundaryRawFileOffsetExclusive == 0x012C945C
            && CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset == 0x012C945B
            && CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive
                == 0x012C945C);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan nonzero16 =
            Project(counter: 7, entryEDI: 0x40000000, entryEDX: 0xFFFFFFF0,
                freshEDXPlus4: 0x100, freshGlobal: 0, lateEntryESI: 0xFFFFFFF8,
                childEsp: null, childEbp: null, testedByte: null,
                popEdi: null, popEsi: null, popEbx: null,
                callTarget: null, callReturnedNormally: null, postCallEbp: null,
                popEbp: null, returnTarget: null);
        check("The 16-byte slot's exact four stores and two counter effects hand off at JNZ 9441 without entering it",
            nonzero16.ParentExactNonzeroEntryHandoff
            && nonzero16.ParentPlan.StoreWrites.Length == 4
            && nonzero16.ParentPlan.StoreWrites.All(write =>
                write.Attempted && write.Committed)
            && nonzero16.ParentPlan.StoreWrites[0].Address == 0
            && nonzero16.ParentPlan.StoreWrites[1].Address == 0x10
            && nonzero16.ParentPlan.StoreWrites[1].Value
                == nonzero16.Capture.StoreCleanupCapture.StoreCapture.TraversalCapture.EntryEDX
            && nonzero16.ParentPlan.StoreWrites[2].Address == 4
            && nonzero16.ParentPlan.StoreWrites[3].Address == 0
            && nonzero16.ParentPlan.OrderedMemoryEffects.Length == 9
            && nonzero16.ParentPlan.OrderedMemoryEffects.TakeLast(2).Select(effect =>
                effect.Kind).SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                })
            && nonzero16.ParentPlan.CleanupCounterPlan?.CounterBefore == 7
            && nonzero16.ParentPlan.CleanupCounterPlan?.CounterAfter == 6
            && nonzero16.ParentPlan.CleanupCounterPlan?.JnzReached == true
            && nonzero16.ParentPlan.CleanupCounterPlan?.JnzTaken == true
            && nonzero16.ParentPlan.CleanupCounterPlan?.SelectedDestinationRawFileOffset
                == 0x012C9441
            && nonzero16.ParentPlan.OrderedInstructions.TakeLast(7).Select(instruction =>
                instruction.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C9473,
                    0x012C942F,
                    0x012C9435
                })
            && nonzero16.ParentPlan.OrderedInstructions.All(instruction =>
                instruction.Reached && instruction.Executed)
            && nonzero16.StopAtRawFileOffsetExclusive == 0x012C9441
            && nonzero16.EpiloguePlan is null
            && !nonzero16.NonzeroBranchTargetEntered
            && !nonzero16.Internal9441Entered
            && nonzero16.ChildEntryPending
            && !nonzero16.ExactBoundedCompletion
            && !nonzero16.ExactCleanCompletion
            && !nonzero16.DestinationEntered);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan nonzero20 =
            Project(counter: 1, entryEDI: 0x80000000, entryEDX: 0xFFFFFFF8,
                freshEDXPlus4: 0x100, freshGlobal: 0xC, lateEntryESI: 0xFFFFFFF8,
                childEsp: null, childEbp: null, testedByte: null);
        check("The 20-byte slot wraps store addresses and DEC-to-zero remains before the call at 943C",
            nonzero20.ParentPlan.ParentLifecycle.ChildFieldAddressAt93EE == 0xC
            && nonzero20.ParentPlan.StoreWrites.Length == 4
            && nonzero20.ParentPlan.StoreWrites[0].Address == 0xC
            && nonzero20.ParentPlan.StoreWrites[1].Address == 0x10
            && nonzero20.ParentPlan.StoreWrites[1].Value == 0xFFFFFFF8
            && nonzero20.ParentPlan.CleanupCounterPlan?.CounterAfter == 0
            && nonzero20.ParentPlan.CleanupCounterPlan?.JnzTaken == false
            && nonzero20.ParentPlan.CleanupCounterPlan?.SelectedDestinationRawFileOffset
                == 0x012C9437
            && nonzero20.ParentPlan.OrderedInstructions.TakeLast(8).Select(instruction =>
                instruction.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C9473,
                    0x012C942F,
                    0x012C9435,
                    0x012C9437
                })
            && nonzero20.ParentExactZeroBypass
            && nonzero20.EpiloguePlan is null
            && !nonzero20.Internal9441Entered
            && nonzero20.StopAtRawFileOffsetExclusive == 0x012C943C
            && nonzero20.ExactBoundedCompletion
            && !nonzero20.ExactCleanCompletion);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan cleanZero =
            Project(counter: 1, entryEDI: 0x40000000, entryEDX: 0xFFFFFFF0,
                freshEDXPlus4: 0x100, freshGlobal: 0, lateEntryESI: 0xFFFFFFF8,
                childEsp: null, childEbp: null, testedByte: null,
                popEdi: null, popEsi: null, popEbx: null,
                callTarget: null, callReturnedNormally: null, postCallEbp: null,
                popEbp: null, returnTarget: null);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan dirtyZero =
            Project(counter: 1, entryEDI: 0x40000000, entryEDX: 0xFFFFFFF0,
                freshEDXPlus4: 0x100, freshGlobal: 0, lateEntryESI: 0xFFFFFFF8,
                childEsp: 0x1000, childEbp: 0x2000, testedByte: 0);
        check("A zero DEC remains clean only with no late child captures, while its bounded stop remains 943C",
            cleanZero.ParentExactZeroBypass
            && cleanZero.ExactBoundedCompletion
            && cleanZero.ExactCleanCompletion
            && cleanZero.EpiloguePlan is null
            && cleanZero.StopAtRawFileOffsetExclusive == 0x012C943C
            && !cleanZero.DirectCallReached
            && !cleanZero.CalleeEntered
            && dirtyZero.ParentExactZeroBypass
            && dirtyZero.ExactBoundedCompletion
            && !dirtyZero.ExactCleanCompletion
            && dirtyZero.LateChildEspProvidedButUnused
            && dirtyZero.LateChildEbpProvidedButUnused
            && dirtyZero.UnreachedTestedByteCaptureProvided
            && !dirtyZero.ChildEntryTargetEntered);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan otherParentBypass =
            Project(entryEDI: 0x12345678, entryEDX: 0x100,
                freshEDXPlus4: 0x1234567F, freshChildDWORD: null,
                freshGlobal: null, lateEntryESI: null, counter: null,
                childEsp: null, childEbp: null, testedByte: null,
                popEdi: null, popEsi: null, popEbx: null,
                callTarget: null, callReturnedNormally: null, postCallEbp: null,
                popEbp: null, returnTarget: null);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan dirtyParentBypass =
            Project(entryEDI: 0x12345678, entryEDX: 0x100,
                freshEDXPlus4: 0x1234567F, freshChildDWORD: null,
                freshGlobal: null, lateEntryESI: null, counter: null,
                childEsp: null, childEbp: null, testedByte: 0,
                popEdi: null, popEsi: null, popEbx: null,
                callTarget: null, callReturnedNormally: null, postCallEbp: null,
                popEbp: null, returnTarget: null);
        check("A nonzero parent bypass remains bounded without consuming child captures, while dirty captures only defeat cleanliness",
            otherParentBypass.ParentOutcome
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ParentEqualBypassesChildTo9403
            && otherParentBypass.EpiloguePlan is null
            && otherParentBypass.StopAtRawFileOffsetExclusive == 0x012C9403
            && otherParentBypass.ExactBoundedCompletion
            && otherParentBypass.ExactCleanCompletion
            && !otherParentBypass.Internal9441Entered
            && dirtyParentBypass.EpiloguePlan is null
            && dirtyParentBypass.ExactBoundedCompletion
            && !dirtyParentBypass.ExactCleanCompletion
            && dirtyParentBypass.UnreachedTestedByteCaptureProvided
            && dirtyParentBypass.StopAtRawFileOffsetExclusive == 0x012C9403);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingESI =
            Project(counter: 5, entryEDI: 0x40000000, entryEDX: 0x100,
                freshEDXPlus4: 0x200, freshGlobal: 0, lateEntryESI: null,
                childEsp: 0x1000, childEbp: 0x2000, testedByte: 0);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingGlobal =
            Project(counter: 5, entryEDI: 0x40000000, entryEDX: 0x100,
                freshEDXPlus4: 0x200, freshGlobal: null, lateEntryESI: 0x3000,
                childEsp: 0x1000, childEbp: 0x2000, testedByte: 0);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan dirtyChildPlus4 =
            Project(counter: 5, entryEDI: 0x40000000, entryEDX: 0x100,
                freshEDXPlus4: 0x200, freshGlobal: 0, lateEntryESI: 0x3000,
                lateFreshChildPlus4: 0xDEADBEEF,
                childEsp: 0x1000, childEbp: 0x2000, testedByte: 0);
        check("Missing ESI/global and a dirty child-plus-four stop in the parent and leave late child registers unused",
            missingESI.ParentOutcome
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .MissingLateEntryESIForFirstStore
            && missingESI.ParentPlan.StoreWrites.Length == 1
            && missingESI.ParentPlan.StoreWrites[0].Attempted
            && !missingESI.ParentPlan.StoreWrites[0].Committed
            && missingESI.EpiloguePlan is null
            && missingESI.LateChildEspProvidedButUnused
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9468
            && missingGlobal.ParentPlan.ParentLifecycle.ParentLifecycle.ThresholdPlan
                ?.GlobalReadReached == true
            && missingGlobal.ParentPlan.StoreWrites.IsEmpty
            && missingGlobal.EpiloguePlan is null
            && missingGlobal.LateChildEbpProvidedButUnused
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C
            && dirtyChildPlus4.ParentPlan.ParentLifecycle.ParentLifecycle
                .LateFreshChildPlus4CaptureProvidedButUnused
            && !dirtyChildPlus4.ParentExactNonzeroEntryHandoff
            && dirtyChildPlus4.EpiloguePlan is null
            && dirtyChildPlus4.LateChildEspProvidedButUnused
            && dirtyChildPlus4.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingCounter =
            Project(counter: null, entryEDI: 0x40000000, entryEDX: 0xFFFFFFF0,
                freshEDXPlus4: 0x100, freshGlobal: 0, lateEntryESI: 0xFFFFFFF8,
                childEsp: 0x1000, childEbp: 0x2000, testedByte: 0,
                popEdi: 1, popEsi: 2, popEbx: 3, callTarget: 0x1234,
                callReturnedNormally: true, postCallEbp: 0x3000,
                popEbp: 4, returnTarget: 5);
        check("A missing fresh counter attempts only the 942F read and leaves all later captures unused",
            missingCounter.ParentPlan.FourParentStoresCommitted
            && missingCounter.ParentPlan.CounterReadAttempted
            && !missingCounter.ParentPlan.CounterValueCaptured
            && !missingCounter.ParentPlan.CounterWriteCommitted
            && missingCounter.ParentPlan.OrderedMemoryEffects.Length == 8
            && missingCounter.EpiloguePlan is null
            && !missingCounter.Internal9441Entered
            && missingCounter.UnreachedTestedByteCaptureProvided
            && missingCounter.UnreachedCallCellTargetCaptureProvided
            && missingCounter.UnreachedCallReturnStatusProvided
            && missingCounter.UnreachedPostCallEbpCaptureProvided
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan wrappedCounter =
            Project(counter: 0, childEsp: null, childEbp: null,
                testedByte: null, popEdi: null, popEsi: null, popEbx: null,
                callTarget: null, callReturnedNormally: null, postCallEbp: null,
                popEbp: null, returnTarget: null);
        check("DEC wraps zero to UINT_MAX and takes the exact nonzero JNZ edge to 9441",
            wrappedCounter.ParentPlan.CounterAfter == uint.MaxValue
            && wrappedCounter.ParentPlan.CleanupFlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && wrappedCounter.ParentExactNonzeroEntryHandoff
            && wrappedCounter.ParentPlan.CounterJnzTaken == true
            && wrappedCounter.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingChildEsp =
            Project(counter: 2, childEsp: null, childEbp: 0x5000,
                testedByte: 2, popEdi: 1, popEsi: 2, popEbx: 3);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingChildEbp =
            Project(counter: 2, childEsp: 0x4000, childEbp: null,
                testedByte: 2, popEdi: 1, popEsi: 2, popEbx: 3);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingChildRegisters =
            Project(counter: 2, childEsp: null, childEbp: null, testedByte: 2);
        check("Neither late child stack register defaults to zero or permits TEST when missing",
            missingChildEsp.ParentExactNonzeroEntryHandoff
            && missingChildEsp.Outcome
                == CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEsp
            && missingChildEsp.EpiloguePlan is null
            && missingChildEsp.LateChildEbpProvidedButUnused
            && missingChildEsp.StopAtRawFileOffsetExclusive == 0x012C9441
            && !missingChildEsp.Internal9441Entered
            && !missingChildEsp.ExactBoundedCompletion
            && missingChildEbp.Outcome
                == CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEbp
            && missingChildEbp.LateChildEspProvidedButUnused
            && missingChildEbp.StopAtRawFileOffsetExclusive == 0x012C9441
            && !missingChildEbp.Internal9441Entered
            && !missingChildEbp.ExactBoundedCompletion
            && missingChildRegisters.Outcome
                == CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEspAndEbp
            && missingChildRegisters.EpiloguePlan is null
            && missingChildRegisters.StopAtRawFileOffsetExclusive == 0x012C9441
            && !missingChildRegisters.ExactBoundedCompletion);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingTest =
            Project(counter: 2, testedByte: null,
                popEdi: 1, popEsi: 2, popEbx: 3, popEbp: 4, returnTarget: 5);
        check("A missing TEST value still records the internal read attempt and preserves all six DEC flags",
            missingTest.ChildEntryValidated
            && missingTest.Internal9441Entered
            && missingTest.ChildTestReadReached
            && missingTest.ChildOutcome == CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte
            && missingTest.ParentFlagsAtChildEntry is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingTest.FlagsAtStop == missingTest.ParentFlagsAtChildEntry
            && missingTest.ParentFlagsPreservedUntilChildTest
            && missingTest.UnreachedStackPopEdiCaptureProvided
            && missingTest.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingTest.OrderedEffects.Length == 10
            && missingTest.OrderedEffects.Take(9).Select(effect => effect.Kind)
                .SequenceEqual(missingTest.ParentOrderedMemoryEffects.Select(effect =>
                    effect.Kind switch
                    {
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentRead =>
                            CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                                .ParentRead,
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentStore =>
                            CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                                .ParentStore,
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                            .CleanupCounterRead =>
                            CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                                .CleanupCounterRead,
                        _ =>
                            CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                                .CleanupCounterWrite
                    }))
            && missingTest.OrderedEffects[9] is
            {
                Kind: CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                    .TestByteRead,
                RawFileOffset: 0x012C9441,
                Address: CapacityFinalCalleeGatedEpilogueProjection.TestByteVirtualAddress,
                Attempted: true,
                ValueCaptured: false
            }
            && !missingTest.ExactCleanCompletion);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingPopEdi =
            Project(counter: 2, testedByte: 2, popEdi: null, popEsi: 2,
                popEbx: 3, callTarget: 0x1234);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingPopEsi =
            Project(counter: 2, testedByte: 2, popEdi: 1, popEsi: null,
                popEbx: 3, callTarget: 0x1234);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingPopEbx =
            Project(counter: 2, testedByte: 2, popEdi: 1, popEsi: 2,
                popEbx: null, callTarget: 0x1234, callReturnedNormally: true);
        check("Ordered EDI/ESI/EBX pops stop at their own missing values and do not consume later captures",
            missingPopEdi.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEdiValue
            && missingPopEdi.ChildStackAccesses.Length == 1
            && missingPopEdi.ChildStackAccesses[0].Address == 0x1000
            && missingPopEdi.UnreachedStackPopEsiCaptureProvided
            && missingPopEdi.UnreachedCallCellTargetCaptureProvided
            && missingPopEdi.StopAtRawFileOffsetExclusive == 0x012C9448
            && missingPopEsi.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEsiValue
            && missingPopEsi.ChildStackAccesses.Select(access => access.Address)
                .SequenceEqual(new uint[] { 0x1000, 0x1004 })
            && missingPopEsi.UnreachedStackPopEbxCaptureProvided
            && missingPopEsi.StopAtRawFileOffsetExclusive == 0x012C9449
            && missingPopEbx.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbxValue
            && missingPopEbx.ChildStackAccesses.Select(access => access.Address)
                .SequenceEqual(new uint[] { 0x1000, 0x1004, 0x1008 })
            && missingPopEbx.UnreachedCallCellTargetCaptureProvided
            && missingPopEbx.UnreachedCallReturnStatusProvided
            && missingPopEbx.StopAtRawFileOffsetExclusive == 0x012C944A);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan skippedCallWrapped =
            Project(counter: 2, childEsp: 0xFFFFFFFC, childEbp: 0xFFFFFFF8,
                testedByte: 0, popEdi: 1, popEsi: 2, popEbx: 3,
                popEbp: 4, returnTarget: 5);
        check("TEST-zero skips the opaque call, uses independent EBP, wraps stack arithmetic, and stops after RET",
            skippedCallWrapped.ChildOutcome == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && skippedCallWrapped.ChildStackAccesses.Select(access => access.Address)
                .SequenceEqual(new uint[]
                {
                    0xFFFFFFFC,
                    0,
                    4,
                    0xFFFFFFF8,
                    0xFFFFFFFC
                })
            && skippedCallWrapped.ChildReturnReached
            && skippedCallWrapped.ChildReturnInstructionRawFileOffset == 0x012C945B
            && skippedCallWrapped.StopAtRawFileOffsetExclusive == 0x012C945C
            && skippedCallWrapped.ChildExactCleanExitAdmitted
            && skippedCallWrapped.ExactCleanCompletion
            && skippedCallWrapped.ExactBoundedCompletion
            && !skippedCallWrapped.DestinationEntered
            && !skippedCallWrapped.ChildCallReached
            && !skippedCallWrapped.OpaqueCallTargetCaptured
            && !skippedCallWrapped.OpaqueCallExecuted
            && skippedCallWrapped.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && skippedCallWrapped.OrderedEffects.TakeLast(9).Select(effect =>
                effect.Kind).SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .TestByteRead,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .StackPopEdi,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .StackPopEsi,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .StackPopEbx,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .TestBranch,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .MovEspFromEbp,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .StackPopEbp,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .ReturnTargetRead,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .ReturnInstruction
                }));

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan opaqueCallReturned =
            Project(counter: 2, childEsp: 0x1000, childEbp: 0x2000,
                testedByte: 2, popEdi: 1, popEsi: 2, popEbx: 3,
                callTarget: 0x00405000, callReturnedNormally: true,
                postCallEbp: 0x3000, popEbp: 4, returnTarget: 0x00401234);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan opaqueNoReturn =
            Project(counter: 2, childEsp: 0x1000, childEbp: 0x2000,
                testedByte: 2, popEdi: 1, popEsi: 2, popEbx: 3,
                callTarget: 0xCAFEBABE, callReturnedNormally: false,
                postCallEbp: null, popEbp: null, returnTarget: null);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingCallCell =
            Project(counter: 2, testedByte: 2, popEdi: 1, popEsi: 2,
                popEbx: 3, callTarget: null, callReturnedNormally: null);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingCallStatus =
            Project(counter: 2, testedByte: 2, popEdi: 1, popEsi: 2,
                popEbx: 3, callTarget: 0x1234, callReturnedNormally: null,
                postCallEbp: 0x3000);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingPostCallEbp =
            Project(counter: 2, testedByte: 2, popEdi: 1, popEsi: 2,
                popEbx: 3, callTarget: 0x1234, callReturnedNormally: true,
                postCallEbp: null);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingPopEbp =
            Project(counter: 2, testedByte: 0, popEdi: 1, popEsi: 2,
                popEbx: 3, popEbp: null, returnTarget: 0x5555);
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan missingReturnTarget =
            Project(counter: 2, testedByte: 0, popEdi: 1, popEsi: 2,
                popEbx: 3, popEbp: 4, returnTarget: null);
        check("Opaque-call skip, missing observations, nonreturn, and normal return preserve their distinct evidence",
            opaqueCallReturned.ChildCallReached
            && opaqueCallReturned.OpaqueCallTargetCaptured
            && opaqueCallReturned.OpaqueCallReturnedNormally == true
            && !opaqueCallReturned.OpaqueCallExecuted
            && opaqueCallReturned.ChildReturnReached
            && opaqueCallReturned.EpiloguePlan?.PostCallEbp == 0x3000
            && opaqueCallReturned.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && opaqueCallReturned.ExactCleanCompletion
            && opaqueCallReturned.StopAtRawFileOffsetExclusive == 0x012C945C
            && opaqueCallReturned.ExactBoundedCompletion
            && opaqueCallReturned.OrderedEffects.TakeLast(13).Select(effect =>
                effect.Kind).SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .TestByteRead,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .StackPopEdi,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .StackPopEsi,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .StackPopEbx,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .TestBranch,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .PushImmediate,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .CallCellRead,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .OpaqueCallReturnStatus,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .PostCallEbpCapture,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .MovEspFromEbp,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .StackPopEbp,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .ReturnTargetRead,
                    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                        .ReturnInstruction
                })
            && opaqueNoReturn.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally
            && opaqueNoReturn.OpaqueCallTargetCaptured
            && opaqueNoReturn.OpaqueCallReturnedNormally == false
            && !opaqueNoReturn.OpaqueCallExecuted
            && !opaqueNoReturn.ChildReturnReached
            && opaqueNoReturn.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && opaqueNoReturn.StopAtRawFileOffsetExclusive == 0x012C9452
            && missingCallCell.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallCellTarget
            && missingCallCell.EpiloguePlan?.CallCellReadReached == true
            && !missingCallCell.ChildCallReached
            && missingCallCell.FlagsAtStop is { Carry: false, Parity: false, Zero: false }
            && missingCallCell.OrderedEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                    .CallCellRead,
                Attempted: true,
                ValueCaptured: false
            }
            && missingCallCell.StopAtRawFileOffsetExclusive == 0x012C9452
            && missingCallStatus.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallReturnStatus
            && missingCallStatus.ChildCallReached
            && missingCallStatus.OpaqueCallReturnedNormally is null
            && missingCallStatus.OrderedEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                    .OpaqueCallReturnStatus,
                Attempted: true,
                ValueCaptured: false
            }
            && missingCallStatus.UnreachedPostCallEbpCaptureProvided
            && missingCallStatus.FlagsAtStop is { Carry: false, Parity: false, Zero: false }
            && missingCallStatus.StopAtRawFileOffsetExclusive == 0x012C9452
            && missingPostCallEbp.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingPostCallEbp
            && missingPostCallEbp.ChildCallReached
            && missingPostCallEbp.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingPostCallEbp.StopAtRawFileOffsetExclusive == 0x012C9458
            && !missingPostCallEbp.ExactBoundedCompletion
            && missingPopEbp.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbpValue
            && missingPopEbp.StopAtRawFileOffsetExclusive == 0x012C945A
            && missingReturnTarget.ChildOutcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingReturnTarget
            && missingReturnTarget.EpiloguePlan?.ReturnTargetReadReached == true
            && missingReturnTarget.StopAtRawFileOffsetExclusive == 0x012C945B);

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan aliasedStores =
            Project(counter: 7, entryEDI: 0x40000000,
                entryEDX: CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress - 16,
                freshEDXPlus4: 0x100, freshGlobal: 0,
                lateEntryESI: CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress - 0x18,
                childEsp: CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress,
                childEbp: CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress,
                testedByte: 0);
        check("Aliased stores do not forward into the independent fresh counter or child-stack captures",
            aliasedStores.ParentExactNonzeroEntryHandoff
            && aliasedStores.ParentPlan.StoreWrites[1].Address
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress
            && aliasedStores.ParentPlan.StoreWrites[1].Value
                == aliasedStores.Capture.StoreCleanupCapture.StoreCapture.TraversalCapture.EntryEDX
            && aliasedStores.ParentPlan.CleanupCounterPlan?.CounterBefore == 7
            && aliasedStores.ParentPlan.CleanupCounterPlan?.CounterAfter == 6
            && aliasedStores.EpiloguePlan?.Capture.EntryEsp
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress
            && aliasedStores.EpiloguePlan?.Capture.EntryEbp
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress
            && aliasedStores.ChildStackAccesses[0].Address
                == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress
            && aliasedStores.ParentMemoryEffectsCompleteBeforeChildTest);
    }

    private static CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan Project(
        uint? counter = 2,
        uint entryEDI = 0x40000000,
        uint entryEDX = 0xFFFFFFF0,
        uint freshEDXPlus4 = 0x100,
        uint? freshChildDWORD = 0,
        uint? freshGlobal = 0,
        uint? lateEntryESI = 0xFFFFFFF8,
        uint? lateFreshChildPlus4 = null,
        uint? childEsp = 0x1000,
        uint? childEbp = 0x2000,
        byte? testedByte = 0,
        uint? popEdi = 0x11111111,
        uint? popEsi = 0x22222222,
        uint? popEbx = 0x33333333,
        uint? callTarget = null,
        bool? callReturnedNormally = null,
        uint? postCallEbp = null,
        uint? popEbp = 0x44444444,
        uint? returnTarget = 0x55555555) =>
        Projection.Project(new(
            StoreCleanupCapture: new(
                StoreCapture: new(
                    TraversalCapture: new(
                        EntryEAX: 0,
                        EntryEDI: entryEDI,
                        EntryEDX: entryEDX,
                        FreshEDXPlus4: freshEDXPlus4,
                        FreshChildDWORD: freshChildDWORD,
                        LateFreshChildPlus4: lateFreshChildPlus4,
                        FreshGlobalValue: freshGlobal),
                    LateEntryESI: lateEntryESI),
                FreshCounterDword: counter),
            LateChildEsp: childEsp,
            LateChildEbp: childEbp,
            TestedByte: testedByte,
            StackPopEdiValue: popEdi,
            StackPopEsiValue: popEsi,
            StackPopEbxValue: popEbx,
            CallCellTargetVirtualAddress: callTarget,
            CallReturnedNormally: callReturnedNormally,
            PostCallEbp: postCallEbp,
            StackPopEbpValue: popEbp,
            ReturnTargetVirtualAddress: returnTarget));
}
