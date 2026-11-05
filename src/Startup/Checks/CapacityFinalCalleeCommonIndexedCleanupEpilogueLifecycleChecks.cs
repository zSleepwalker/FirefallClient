using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleProjection;

internal static class CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The indexed cleanup epilogue keeps the exact parent entry, DEC/JNZ, zero-call boundary, and RET boundary",
            Projection.EntryRawFileOffset == 0x012C93C0
            && Projection.ChildEntryRawFileOffset == 0x012C9441
            && Projection.ZeroCounterStopRawFileOffset == 0x012C943C
            && Projection.ReturnBoundaryRawFileOffsetExclusive == 0x012C945C
            && CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset == 0x012C945B);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan count31Slot16 =
            Project(parent: Parent(counter: 2),
                testedByte: 0,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                stackPopEbp: 4,
                returnTarget: 5);
        check("Count 31 with a 16-byte slot forwards the original EDX through four committed stores and DEC/JNZ",
            count31Slot16.ParentExactNonzeroEntryHandoff
            && count31Slot16.ParentPlan.ParentLifecycle.CountPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && count31Slot16.ParentPlan.ParentLifecycle.ChildPlan?.ChildFieldOffset == 16
            && count31Slot16.ParentPlan.ParentLifecycle.StoreTailLateEntryEDX == 0x100
            && count31Slot16.ParentPlan.ParentLifecycle.StoreTailLateEntryEDXIsOriginalParentDerivedTagEDX
            && count31Slot16.ParentPlan.StoreWrites.Length == 4
            && count31Slot16.ParentPlan.FourParentStoresCommitted
            && count31Slot16.ParentPlan.OrderedReads.Length == 4
            && count31Slot16.ParentPlan.OrderedMemoryEffects.Length == 10
            && count31Slot16.ParentPlan.OrderedInstructions.TakeLast(7)
                .Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C9473,
                    0x012C942F,
                    0x012C9435
                })
            && count31Slot16.OrderedEffects.Take(10).Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.CleanupCounterRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.CleanupCounterWrite
                })
            && count31Slot16.OrderedEffects.Take(4).All(effect =>
                effect.Attempted && effect.ValueCaptured && !effect.Committed)
            && count31Slot16.OrderedEffects.Skip(4).Take(4).All(effect =>
                effect.Attempted && effect.ValueCaptured && effect.Committed)
            && count31Slot16.OrderedEffects[8] is
            {
                Attempted: true,
                ValueCaptured: true,
                Committed: false
            }
            && count31Slot16.OrderedEffects[9] is
            {
                Attempted: true,
                ValueCaptured: true,
                Committed: true
            }
            && count31Slot16.OrderedEffects[10] is
            {
                Kind: CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.TestByteRead,
                RawFileOffset: 0x012C9441,
                Attempted: true,
                ValueCaptured: true,
                Committed: false
            }
            && count31Slot16.OrderedEffects.Length == 19
            && count31Slot16.OrderedEffects.Skip(10).Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.TestByteRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEdi,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEsi,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEbx,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.TestBranch,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.MovEspFromEbp,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEbp,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ReturnTargetRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ReturnInstruction
                })
            && count31Slot16.ParentMemoryEffectsCompleteBeforeChildTest
            && count31Slot16.Internal9441Entered
            && count31Slot16.EpiloguePlan?.Outcome == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && count31Slot16.ChildReturnReached
            && count31Slot16.StopAtRawFileOffsetExclusive == 0x012C945C
            && count31Slot16.ExactBoundedCompletion
            && count31Slot16.ExactCleanCompletion);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan count30Slot20 =
            Project(parent: Parent(
                entryEBX: 0x94,
                freshIndexedDWORD: 0xFFFFFFF8,
                lateEntryECX: 30,
                lateEntryEDI: 0x00200000,
                freshEDXPlus4: 8,
                freshChildDWORD: 0,
                freshGlobalValue: 0xC,
                lateEntryESI: 0x180,
                counter: 2),
                testedByte: 2,
                stackPopEdi: 0x1111,
                stackPopEsi: 0x2222,
                stackPopEbx: 0x3333,
                callCellTarget: 0x12345678,
                callReturnedNormally: true,
                postCallEbp: 0x5000,
                stackPopEbp: 0x6000,
                returnTarget: 0x7000);
        check("Count 30 uses the wrapping 20-byte child slot and keeps the opaque call and RET bounded",
            count30Slot20.ParentPlan.ParentLifecycle.ChildPlan?.ChildFieldOffset == 20
            && count30Slot20.ParentPlan.ParentLifecycle.ChildFieldAddressAt93EE == 0xC
            && count30Slot20.ParentPlan.ParentLifecycle.StoreTailEntryECX == 0xC
            && count30Slot20.ParentPlan.ParentLifecycle.StoreTailLateEntryEDX == 0xFFFFFFF8
            && count30Slot20.ParentPlan.StoreWrites[0].Address == 0xC
            && count30Slot20.ParentPlan.StoreWrites[1].Value == 0xFFFFFFF8
            && count30Slot20.ParentExactNonzeroEntryHandoff
            && count30Slot20.ChildCallReached
            && count30Slot20.OpaqueCallTargetCaptured
            && count30Slot20.OpaqueCallReturnedNormally == true
            && !count30Slot20.OpaqueCallExecuted
            && count30Slot20.ChildReturnReached
            && count30Slot20.ChildBoundedExitReached
            && count30Slot20.ExactBoundedCompletion
            && count30Slot20.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            });

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan aliasedCounter =
            Project(parent: Parent(
                entryEBX: 0xB0,
                freshIndexedDWORD: 0x01EDADD8,
                lateEntryECX: 31,
                lateEntryEDI: 0,
                freshEDXPlus4: 8,
                freshChildDWORD: 0,
                freshGlobalValue: 0x01EDADE8,
                lateEntryESI: 0x01EDADD0,
                counter: 5),
                testedByte: 0);
        check("Aliased parent stores do not substitute for the independent cleanup-counter read",
            aliasedCounter.ParentExactNonzeroEntryHandoff
            && aliasedCounter.ParentPlan.StoreWrites[0].Address == 0x01EDADE8
            && aliasedCounter.ParentPlan.StoreWrites[1].Address == 0x01EDADE8
            && aliasedCounter.ParentPlan.CleanupCounterPlan?.CounterBefore == 5
            && aliasedCounter.ParentPlan.CleanupCounterPlan?.CounterAfter == 4
            && aliasedCounter.ParentPlan.CleanupCounterPlan?.CounterAccesses[0].Value == 5
            && aliasedCounter.ParentPlan.OrderedMemoryEffects[8].Kind
                == CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead
            && aliasedCounter.ParentPlan.OrderedMemoryEffects[9].Kind
                == CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .CleanupCounterWrite);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan zeroCounter =
            Project(parent: Parent(counter: 1), testedByte: 0, callCellTarget: 0x1234,
                callReturnedNormally: true, postCallEbp: 0x5000);
        check("DEC zero executes only the MOV ECX immediate and stops before the 943C call",
            zeroCounter.Outcome
                == CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
                    .ParentDecrementedToZeroBeforeCall
            && zeroCounter.ParentExactZeroBypass
            && zeroCounter.ParentPlan.CleanupCounterPlan?.MovEcxReached == true
            && zeroCounter.ParentPlan.CleanupCounterPlan?.EcxValue == 0x01EDADC8
            && zeroCounter.ParentPlan.CleanupCounterPlan?.CallReached == false
            && zeroCounter.ParentPlan.CleanupCounterPlan?.CalleeEntered == false
            && zeroCounter.ZeroPathFallthroughEntered
            && !zeroCounter.DirectCallReached
            && !zeroCounter.CalleeEntered
            && zeroCounter.EpiloguePlan is null
            && !zeroCounter.Internal9441Entered
            && zeroCounter.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroCounter.ExactBoundedCompletion
            && !zeroCounter.ExactCleanCompletion
            && zeroCounter.UnreachedTestedByteCaptureProvided
            && zeroCounter.UnreachedCallCellTargetCaptureProvided
            && zeroCounter.UnreachedCallReturnStatusProvided
            && zeroCounter.UnreachedPostCallEbpCaptureProvided);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan cleanZeroCounter =
            Project(parent: Parent(counter: 1), childEsp: null, childEbp: null);
        check("The zero bypass is clean only when no unreached epilogue inputs were supplied",
            cleanZeroCounter.ParentExactZeroBypass
            && cleanZeroCounter.ExactBoundedCompletion
            && cleanZeroCounter.ExactCleanCompletion
            && cleanZeroCounter.EpiloguePlan is null
            && cleanZeroCounter.StopAtRawFileOffsetExclusive == 0x012C943C
            && AllUnreachedCaptureFlagsMatchInputs(cleanZeroCounter));

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan equalBypass =
            Project(parent: Parent(
                entryEBX: 0xDC,
                freshIndexedDWORD: 0x100,
                lateEntryECX: 31,
                lateEntryEDI: 0,
                freshEDXPlus4: 0,
                freshChildDWORD: null,
                freshGlobalValue: 0xFFFFFFFF,
                lateEntryESI: 0x180,
                counter: 4),
                testedByte: 1);
        check("The exact equal-parent bypass retains its own 9403 bound and leaves child captures dirty",
            equalBypass.ParentOutcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ParentEqualBypassesChildTo9403
            && equalBypass.EpiloguePlan is null
            && equalBypass.StopAtRawFileOffsetExclusive == 0x012C9403
            && equalBypass.ExactBoundedCompletion
            && !equalBypass.ExactCleanCompletion
            && equalBypass.UnreachedTestedByteCaptureProvided);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan belowThreshold =
            Project(parent: Parent(
                entryEBX: 0xD0,
                freshIndexedDWORD: 0x100,
                lateEntryECX: 31,
                lateEntryEDI: 0,
                freshEDXPlus4: 8,
                freshChildDWORD: 0,
                freshGlobalValue: 0x111,
                lateEntryESI: 0x180,
                counter: 4),
                testedByte: 1,
                stackPopEdi: 1);
        check("An unsigned-below parent threshold exit remains bounded without entering the child",
            belowThreshold.ParentPlan.ParentLifecycle.Outcome
                == CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget
            && belowThreshold.EpiloguePlan is null
            && belowThreshold.StopAtRawFileOffsetExclusive == 0x012C8E76
            && belowThreshold.ExactBoundedCompletion
            && !belowThreshold.ExactCleanCompletion
            && belowThreshold.UnreachedTestedByteCaptureProvided
            && belowThreshold.UnreachedStackPopEdiCaptureProvided);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingCounter =
            Project(parent: Parent(counter: null), testedByte: 1, callCellTarget: 0x1234);
        check("A missing counter value attempts the DEC read but never invents a child entry",
            missingCounter.ParentPlan.CleanupCounterPlan?.CounterReadReached == true
            && missingCounter.ParentPlan.CleanupCounterPlan?.CounterAccesses.Length == 1
            && missingCounter.ParentPlan.CleanupCounterPlan?.CounterAccesses[0].ValueCaptured == false
            && missingCounter.EpiloguePlan is null
            && !missingCounter.Internal9441Entered
            && missingCounter.UnreachedTestedByteCaptureProvided
            && missingCounter.UnreachedCallCellTargetCaptureProvided
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingESI =
            Project(parent: Parent(lateEntryESI: null, counter: 2), testedByte: 1);
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingGlobal =
            Project(parent: Parent(freshGlobalValue: null, counter: 2), testedByte: 1);
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan dirtyUnusedChildPlus4 =
            Project(parent: Parent(lateFreshChildPlus4: 0x200, counter: 2), testedByte: 1);
        check("Missing ESI/global input and a dirty unused child-plus-four all block the counter-to-child handoff",
            missingESI.EpiloguePlan is null
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9468
            && missingESI.UnreachedTestedByteCaptureProvided
            && missingGlobal.EpiloguePlan is null
            && missingGlobal.ParentPlan.CleanupCounterPlan is null
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C
            && missingGlobal.UnreachedTestedByteCaptureProvided
            && dirtyUnusedChildPlus4.EpiloguePlan is null
            && dirtyUnusedChildPlus4.ParentPlan.CleanupCounterPlan is null
            && dirtyUnusedChildPlus4.StopAtRawFileOffsetExclusive == 0x012C945C
            && dirtyUnusedChildPlus4.UnreachedTestedByteCaptureProvided);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingEsp =
            Project(parent: Parent(counter: 2), childEsp: null, childEbp: 0x5000,
                testedByte: 0, stackPopEdi: 1);
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingEbp =
            Project(parent: Parent(counter: 2), childEsp: 0x1000, childEbp: null,
                testedByte: 0, stackPopEdi: 1);
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingBoth =
            Project(parent: Parent(counter: 2), childEsp: null, childEbp: null,
                testedByte: 0);
        check("Late ESP/EBP remain independent, nullable captures and stop at 9441",
            missingEsp.Outcome
                == CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEsp
            && missingEsp.ParentExactNonzeroEntryHandoff
            && missingEsp.Capture.LateChildEbp == 0x5000
            && missingEsp.ChildEntryPending
            && missingEsp.EpiloguePlan is null
            && missingEsp.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingEsp.LateChildEbpProvidedButUnused
            && missingEsp.UnreachedTestedByteCaptureProvided
            && missingEbp.Outcome
                == CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEbp
            && missingEbp.Capture.LateChildEsp == 0x1000
            && missingEbp.EpiloguePlan is null
            && missingEbp.LateChildEspProvidedButUnused
            && missingBoth.Outcome
                == CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
                    .MissingLateChildEspAndEbp
            && missingBoth.EpiloguePlan is null
            && missingBoth.ParentFlagsPreservedUntilChildTest
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingTest =
            Project(parent: Parent(counter: 2),
                testedByte: null,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x1234,
                callReturnedNormally: true,
                postCallEbp: 0x5000,
                stackPopEbp: 4,
                returnTarget: 5);
        check("9441 TEST attempts with a missing byte and preserves all six DEC flags",
            missingTest.ChildEntryValidated
            && missingTest.Internal9441Entered
            && missingTest.ChildTestReadReached
            && missingTest.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte
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
            && missingTest.OrderedEffects.Length == 11
            && missingTest.OrderedEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.TestByteRead,
                Attempted: true,
                ValueCaptured: false,
                Committed: false
            }
            && AllUnreachedCaptureFlagsMatchInputs(missingTest));

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingPop =
            Project(parent: Parent(counter: 2), testedByte: 2, stackPopEsi: 2);
        check("MOVs and POPs before the opaque CALL preserve all six TEST flags",
            missingPop.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEdiValue
            && missingPop.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingPop.ParentFlagsAtChildEntry is { Carry: false, Zero: false }
            && !missingPop.ChildCallReached
            && missingPop.EpiloguePlan?.StackPopEdiReached == true
            && missingPop.EpiloguePlan?.StackPopEsiReached == false
            && missingPop.UnreachedStackPopEsiCaptureProvided);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingPopEsi =
            Project(parent: Parent(counter: 2), testedByte: 2, stackPopEdi: 1);
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingPopEbx =
            Project(parent: Parent(counter: 2), testedByte: 2,
                stackPopEdi: 1, stackPopEsi: 2);
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingPopEbp =
            Project(parent: Parent(counter: 2), testedByte: 0,
                stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3,
                returnTarget: 5);
        check("Missing ESI, EBX, and EBP pops stop at their reached instruction without losing prior flags",
            missingPopEsi.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEsiValue
            && missingPopEsi.EpiloguePlan?.StackPopEsiReached == true
            && missingPopEsi.StopAtRawFileOffsetExclusive == 0x012C9449
            && missingPopEsi.FlagsAtStop is { Carry: false, Zero: false }
            && missingPopEbx.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbxValue
            && missingPopEbx.EpiloguePlan?.StackPopEbxReached == true
            && missingPopEbx.StopAtRawFileOffsetExclusive == 0x012C944A
            && missingPopEbx.FlagsAtStop is { Carry: false, Zero: false }
            && missingPopEbp.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbpValue
            && missingPopEbp.EpiloguePlan?.StackPopEbpReached == true
            && missingPopEbp.StopAtRawFileOffsetExclusive == 0x012C945A
            && missingPopEbp.FlagsAtStop is { Carry: false, Zero: true }
            && missingPopEbp.UnreachedReturnTargetCaptureProvided);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan wrappedStack =
            Project(parent: Parent(counter: 2),
                childEsp: 0xFFFFFFFC,
                childEbp: 0x76543210,
                testedByte: 2,
                stackPopEdi: 0x1111,
                stackPopEsi: 0x2222,
                stackPopEbx: 0x3333,
                callCellTarget: 0x12345678,
                callReturnedNormally: true,
                postCallEbp: 0xFFFFFFFC,
                stackPopEbp: 0x4444,
                returnTarget: 0x5555);
        check("Wrapped ESP arithmetic and late EBP capture remain independent stack inputs",
            wrappedStack.EpiloguePlan?.Capture.EntryEsp == 0xFFFFFFFC
            && wrappedStack.EpiloguePlan?.Capture.EntryEbp == 0x76543210
            && wrappedStack.EpiloguePlan?.StackPopEdiAddress == 0xFFFFFFFC
            && wrappedStack.EpiloguePlan?.StackPopEsiAddress == 0
            && wrappedStack.EpiloguePlan?.StackPopEbxAddress == 4
            && wrappedStack.EpiloguePlan?.PushImmediateAddress == 4
            && wrappedStack.EpiloguePlan?.PostCallEbp == 0xFFFFFFFC
            && wrappedStack.EpiloguePlan?.StackPopEbpAddress == 0xFFFFFFFC
            && wrappedStack.EpiloguePlan?.ReturnTargetStackAddress == 0
            && wrappedStack.EpiloguePlan?.EspAfterReturn == 4
            && wrappedStack.ChildReturnReached
            && wrappedStack.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingCallCell =
            Project(parent: Parent(counter: 2),
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callReturnedNormally: true);
        check("An indirect call-cell read attempt remains distinct from call entry",
            missingCallCell.EpiloguePlan?.CallCellReadReached == true
            && missingCallCell.EpiloguePlan?.CallCellTargetCaptured == false
            && !missingCallCell.ChildCallReached
            && !missingCallCell.OpaqueCallExecuted
            && missingCallCell.FlagsAtStop is { Carry: false, Parity: false, Zero: false }
            && !missingCallCell.UnreachedCallCellTargetCaptureProvided
            && missingCallCell.UnreachedCallReturnStatusProvided);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingCallStatus =
            Project(parent: Parent(counter: 2),
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678,
                postCallEbp: 0x5000);
        check("The opaque call is entered only after a captured call cell and an independent return-status input",
            missingCallStatus.ChildCallReached
            && missingCallStatus.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallReturnStatus
            && missingCallStatus.OpaqueCallTargetCaptured
            && !missingCallStatus.OpaqueCallExecuted
            && missingCallStatus.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingCallStatus.UnreachedCallReturnStatusProvided == false
            && missingCallStatus.UnreachedPostCallEbpCaptureProvided);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan nonreturningCall =
            Project(parent: Parent(counter: 2),
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678,
                callReturnedNormally: false,
                postCallEbp: 0x5000);
        check("A nonreturning opaque call stops at the call without executing the callee",
            nonreturningCall.ChildCallReached
            && nonreturningCall.OpaqueCallReturnedNormally == false
            && nonreturningCall.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally
            && !nonreturningCall.OpaqueCallExecuted
            && nonreturningCall.StopAtRawFileOffsetExclusive == 0x012C9452
            && nonreturningCall.UnreachedPostCallEbpCaptureProvided
            && AllUnreachedCaptureFlagsMatchInputs(nonreturningCall));

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan normalCallMissingPostEbp =
            Project(parent: Parent(counter: 2),
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678,
                callReturnedNormally: true,
                postCallEbp: null,
                stackPopEbp: 4,
                returnTarget: 5);
        check("A normal opaque RETURN makes all six flags unknown even when post-call EBP is absent",
            normalCallMissingPostEbp.ChildCallReached
            && normalCallMissingPostEbp.OpaqueCallReturnedNormally == true
            && normalCallMissingPostEbp.EpiloguePlan?.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingPostCallEbp
            && normalCallMissingPostEbp.EpiloguePlan?.MovEspFromEbpReached == true
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
            && !normalCallMissingPostEbp.OpaqueCallExecuted
            && !normalCallMissingPostEbp.UnreachedPostCallEbpCaptureProvided);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan fullReturn =
            Project(parent: Parent(counter: 2),
                testedByte: 2,
                stackPopEdi: 0x1111,
                stackPopEsi: 0x2222,
                stackPopEbx: 0x3333,
                callCellTarget: 0x12345678,
                callReturnedNormally: true,
                postCallEbp: 0x5000,
                stackPopEbp: 0x6000,
                returnTarget: 0x7000);
        check("Only the captured actual RET reaches the complete 945C boundary",
            fullReturn.ChildReturnReached
            && fullReturn.ChildBoundedExitReached
            && fullReturn.ChildExactCleanExitAdmitted
            && fullReturn.ExactBoundedCompletion
            && fullReturn.ExactCleanCompletion
            && fullReturn.StopAtRawFileOffsetExclusive == 0x012C945C
            && fullReturn.OrderedEffects.Take(10).All(effect =>
                effect.Kind is CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
                    .ParentRead
                    or CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
                        .ParentStore
                    or CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
                        .CleanupCounterRead
                    or CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
                        .CleanupCounterWrite)
            && fullReturn.OrderedEffects.Skip(10).Select(effect => effect.Kind).SequenceEqual(
                new[]
                {
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.TestByteRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEdi,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEsi,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEbx,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.TestBranch,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.PushImmediate,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.CallCellRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
                        .OpaqueCallReturnStatus,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
                        .PostCallEbpCapture,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.MovEspFromEbp,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEbp,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ReturnTargetRead,
                    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ReturnInstruction
                })
            && !fullReturn.OpaqueCallExecuted);

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan missingReturnTarget =
            Project(parent: Parent(counter: 2),
                testedByte: 2,
                stackPopEdi: 1,
                stackPopEsi: 2,
                stackPopEbx: 3,
                callCellTarget: 0x12345678,
                callReturnedNormally: true,
                postCallEbp: 0x5000,
                stackPopEbp: 4,
                returnTarget: null);
        check("A missing RET target is a read attempt and not a bounded completion",
            missingReturnTarget.EpiloguePlan?.ReturnTargetReadReached == true
            && missingReturnTarget.EpiloguePlan?.ReturnTargetCaptured == false
            && !missingReturnTarget.ChildReturnReached
            && !missingReturnTarget.ChildBoundedExitReached
            && !missingReturnTarget.ExactBoundedCompletion
            && !missingReturnTarget.ExactCleanCompletion
            && missingReturnTarget.StopAtRawFileOffsetExclusive == 0x012C945B);
    }

    private static bool AllUnreachedCaptureFlagsMatchInputs(
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan plan) =>
        plan.LateChildEspProvidedButUnused
            == (!plan.ChildEntryValidated && plan.Capture.LateChildEsp.HasValue)
        && plan.LateChildEbpProvidedButUnused
            == (!plan.ChildEntryValidated && plan.Capture.LateChildEbp.HasValue)
        && plan.UnreachedTestedByteCaptureProvided
            == (!plan.ChildTestReadReached && plan.Capture.TestedByte.HasValue)
        && plan.UnreachedStackPopEdiCaptureProvided
            == (plan.EpiloguePlan?.StackPopEdiReached != true
                && plan.Capture.StackPopEdiValue.HasValue)
        && plan.UnreachedStackPopEsiCaptureProvided
            == (plan.EpiloguePlan?.StackPopEsiReached != true
                && plan.Capture.StackPopEsiValue.HasValue)
        && plan.UnreachedStackPopEbxCaptureProvided
            == (plan.EpiloguePlan?.StackPopEbxReached != true
                && plan.Capture.StackPopEbxValue.HasValue)
        && plan.UnreachedCallCellTargetCaptureProvided
            == (plan.EpiloguePlan?.CallCellReadReached != true
                && plan.Capture.CallCellTargetVirtualAddress.HasValue)
        && plan.UnreachedCallReturnStatusProvided
            == (plan.EpiloguePlan?.CallReached != true
                && plan.Capture.CallReturnedNormally.HasValue)
        && plan.UnreachedPostCallEbpCaptureProvided
            == (plan.EpiloguePlan?.PostCallEbpCaptureReached != true
                && plan.Capture.PostCallEbp.HasValue)
        && plan.UnreachedStackPopEbpCaptureProvided
            == (plan.EpiloguePlan?.StackPopEbpReached != true
                && plan.Capture.StackPopEbpValue.HasValue)
        && plan.UnreachedReturnTargetCaptureProvided
            == (plan.EpiloguePlan?.ReturnTargetReadReached != true
                && plan.Capture.ReturnTargetVirtualAddress.HasValue);

    private static CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan Project(
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleCapture? parent = null,
        uint? childEsp = 0x1000,
        uint? childEbp = 0x2000,
        byte? testedByte = null,
        uint? stackPopEdi = null,
        uint? stackPopEsi = null,
        uint? stackPopEbx = null,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null,
        uint? postCallEbp = null,
        uint? stackPopEbp = null,
        uint? returnTarget = null) =>
        Projection.Project(new(
            parent ?? Parent(),
            childEsp,
            childEbp,
            testedByte,
            stackPopEdi,
            stackPopEsi,
            stackPopEbx,
            callCellTarget,
            callReturnedNormally,
            postCallEbp,
            stackPopEbp,
            returnTarget));

    private static CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleCapture Parent(
        uint? entryEBX = 0x80,
        uint? freshIndexedDWORD = 0x100,
        uint? lateEntryECX = 31,
        uint? lateEntryEDI = 0,
        uint? freshEDXPlus4 = 8,
        uint? freshChildDWORD = 0,
        uint? lateFreshChildPlus4 = null,
        uint? freshGlobalValue = 0x110,
        uint? lateEntryESI = 0x180,
        uint? counter = 2)
    {
        var countCapture =
            new CapacityFinalCalleeCommonIndexedCountClassificationCapture(
                entryEBX,
                freshIndexedDWORD,
                lateEntryECX);
        var tagCapture = new CapacityFinalCalleeCommonIndexedTagTraversalLifecycleCapture(
            countCapture,
            lateEntryEDI,
            freshEDXPlus4,
            freshChildDWORD,
            lateFreshChildPlus4);
        var zeroSlotCapture =
            new CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleCapture(
                tagCapture,
                freshGlobalValue);
        var storesCapture =
            new CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleCapture(
                zeroSlotCapture,
                lateEntryESI);
        return new(storesCapture, counter);
    }
}
