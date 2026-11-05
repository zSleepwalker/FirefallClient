using Firefall.Reconstruction.Startup;

internal static class GameSocketCompletionErrorEpilogueChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The small epilogue bytes, offsets, and VA use the verified raw-text mapping",
            GameSocketCompletionErrorEpilogueProjection.InstructionSequenceByteLength == 19
            && GameSocketCompletionErrorEpilogueProjection.StartRawFileOffset == 0xBD2E19
            && GameSocketCompletionErrorEpilogueProjection.StartVirtualAddress == 0xFD3A19
            && GameSocketCompletionErrorEpilogueProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && GameSocketCompletionErrorEpilogueProjection.PreviousSehLoadRawFileOffset == 0xBD2E19
            && GameSocketCompletionErrorEpilogueProjection.FsPreviousSehWriteRawFileOffset == 0xBD2E1C
            && GameSocketCompletionErrorEpilogueProjection.FirstPopRawFileOffset == 0xBD2E23
            && GameSocketCompletionErrorEpilogueProjection.PopEdiRawFileOffset == 0xBD2E24
            && GameSocketCompletionErrorEpilogueProjection.PopEsiRawFileOffset == 0xBD2E25
            && GameSocketCompletionErrorEpilogueProjection.RestoreEspFromEbpRawFileOffset == 0xBD2E26
            && GameSocketCompletionErrorEpilogueProjection.PopSavedEbpRawFileOffset == 0xBD2E28
            && GameSocketCompletionErrorEpilogueProjection.NativeRetRawFileOffset == 0xBD2E29
            && GameSocketCompletionErrorEpilogueProjection.InstructionBytes.SequenceEqual(
                new byte[]
                {
                    0x8B, 0x4D, 0xF4, 0x64, 0x89, 0x0D, 0x00, 0x00, 0x00, 0x00,
                    0x59, 0x5F, 0x5E, 0x8B, 0xE5, 0x5D, 0xC2, 0x04, 0x00
                }));

        GameSocketCompletionErrorEpiloguePlan normal = Project(
            postQueueEbp: 0x2000,
            previousSeh: 0x11223344,
            firstPopEsp: 0xFFFFFFFC,
            poppedEcx: 0,
            poppedEdi: 0,
            poppedEsi: 0,
            savedEbp: 0x55667788);
        check("A clean parent admits all fresh epilogue captures and stops before RET",
            normal.ParentPlan.IsCleanCompletion
            && normal.ParentPlan.CaptureStates.QueueAppendReturnedNormally
                == GameSocketCompletionErrorStateCaptureState.Captured
            && normal.ProjectionEntered
            && normal.Outcome == GameSocketCompletionErrorEpilogueOutcome.BoundedBeforeNativeRet
            && normal.Disposition == GameSocketCompletionErrorEpilogueDisposition.BoundedBeforeNativeRet
            && normal.IsCleanEpilogue
            && normal.ExactBoundedCompletion
            && normal.CaptureStates.PostQueueEbp == GameSocketCompletionErrorEpilogueCaptureState.Captured
            && normal.CaptureStates.PreviousSeh == GameSocketCompletionErrorEpilogueCaptureState.Captured
            && normal.CaptureStates.FirstPopEsp == GameSocketCompletionErrorEpilogueCaptureState.Captured
            && normal.CaptureStates.PoppedEcx == GameSocketCompletionErrorEpilogueCaptureState.Captured
            && normal.CaptureStates.PoppedEdi == GameSocketCompletionErrorEpilogueCaptureState.Captured
            && normal.CaptureStates.PoppedEsi == GameSocketCompletionErrorEpilogueCaptureState.Captured
            && normal.CaptureStates.SavedEbp == GameSocketCompletionErrorEpilogueCaptureState.Captured
            && normal.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[]
            {
                0xBD2E19, 0xBD2E1C, 0xBD2E23, 0xBD2E24, 0xBD2E25, 0xBD2E26, 0xBD2E28
            })
            && normal.LastReachedRawFileOffset == 0xBD2E28
            && normal.StopBeforeRawFileOffset == 0xBD2E29
            && !normal.ReachedInstructionRawFileOffsets.Contains(0xBD2E29)
            && normal.NativeInstructionsExecuted == false
            && normal.NativeRetExecuted == false
            && normal.NativeRetTargetCaptured == false
            && normal.EaxReturnValueModeled == false
            && normal.RestoredEbp == 0x55667788
            && normal.EspBeforeNativeRet == 0x2004
            && normal.UnusedCaptures.IsEmpty);

        check("The epilogue commits only the explicit FS write and preserves unknown flags",
            normal.MemoryEffects.Length == 6
            && normal.MemoryEffects[0] is
                { Kind: GameSocketCompletionErrorEpilogueMemoryEffectKind.PreviousSehRead,
                  Address: 0x1FF4, Value: 0x11223344, Attempted: true, Captured: true,
                  Committed: false, InstructionRawFileOffset: 0xBD2E19 }
            && normal.MemoryEffects[1] is
                { Kind: GameSocketCompletionErrorEpilogueMemoryEffectKind.SegmentWrite,
                  Address: null, Value: 0x11223344, Attempted: true, Captured: true,
                  Committed: true, InstructionRawFileOffset: 0xBD2E1C,
                  Segment: "FS", SegmentOffset: 0 }
            && normal.MemoryEffects[2] is
                { Kind: GameSocketCompletionErrorEpilogueMemoryEffectKind.StackRead,
                  Address: 0xFFFFFFFC, Value: 0, Attempted: true, Captured: true,
                  Committed: false, InstructionRawFileOffset: 0xBD2E23 }
            && normal.MemoryEffects[3] is
                { Kind: GameSocketCompletionErrorEpilogueMemoryEffectKind.StackRead,
                  Address: 0, Value: 0, Attempted: true, Captured: true,
                  Committed: false, InstructionRawFileOffset: 0xBD2E24 }
            && normal.MemoryEffects[4] is
                { Kind: GameSocketCompletionErrorEpilogueMemoryEffectKind.StackRead,
                  Address: 4, Value: 0, Attempted: true, Captured: true,
                  Committed: false, InstructionRawFileOffset: 0xBD2E25 }
            && normal.MemoryEffects[5] is
                { Kind: GameSocketCompletionErrorEpilogueMemoryEffectKind.StackRead,
                  Address: 0x2000, Value: 0x55667788, Attempted: true, Captured: true,
                  Committed: false, InstructionRawFileOffset: 0xBD2E28 }
            && normal.OrderedTrace.Length
                == normal.ParentPlan.OrderedMemoryEffects.Length + normal.MemoryEffects.Length
            && normal.OrderedTrace.Take(normal.ParentPlan.OrderedMemoryEffects.Length)
                .Select(step => step.ParentMemoryEffect)
                .SequenceEqual(normal.ParentPlan.OrderedMemoryEffects.Select(effect =>
                    (GameSocketCompletionErrorStateMemoryEffect?)effect))
            && normal.OrderedTrace.Skip(normal.ParentPlan.OrderedMemoryEffects.Length)
                .Select(step => step.EpilogueMemoryEffect)
                .SequenceEqual(normal.MemoryEffects.Select(effect =>
                    (GameSocketCompletionErrorEpilogueMemoryEffect?)effect))
            && normal.OrderedTrace.Select(step => step.Sequence)
                .SequenceEqual(Enumerable.Range(0, normal.OrderedTrace.Length))
            && normal.FlagsAtStop is
                { Carry: null, Parity: null, AuxiliaryCarry: null, Zero: null, Sign: null, Overflow: null });

        GameSocketCompletionErrorEpiloguePlan missingEbp = Project(
            postQueueEbp: null,
            previousSeh: 0,
            firstPopEsp: 0x3000,
            poppedEcx: 1,
            poppedEdi: 2,
            poppedEsi: 3,
            savedEbp: 4);
        check("A missing post-call EBP stops at the attempted previous-SEH read",
            missingEbp.Outcome == GameSocketCompletionErrorEpilogueOutcome.MissingPostQueueEbp
            && missingEbp.CaptureStates.PostQueueEbp == GameSocketCompletionErrorEpilogueCaptureState.Missing
            && missingEbp.CaptureStates.PreviousSeh
                == GameSocketCompletionErrorEpilogueCaptureState.SuppliedButUnused
            && missingEbp.MemoryEffects.Length == 1
            && missingEbp.MemoryEffects[0] is
                { Address: null, Value: null, Attempted: true, Captured: false,
                  InstructionRawFileOffset: 0xBD2E19 }
            && missingEbp.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] { 0xBD2E19 })
            && missingEbp.LastReachedRawFileOffset == 0xBD2E19
            && missingEbp.StopBeforeRawFileOffset == 0xBD2E19
            && !missingEbp.ExactBoundedCompletion
            && missingEbp.UnusedCaptures.Length == 6);

        GameSocketCompletionErrorEpiloguePlan missingSeh = Project(
            postQueueEbp: 0x3000,
            previousSeh: null,
            firstPopEsp: 0x3000,
            poppedEcx: 1,
            poppedEdi: 2,
            poppedEsi: 3,
            savedEbp: 4);
        check("A missing previous-SEH value does not reach or commit the FS segment write",
            missingSeh.Outcome == GameSocketCompletionErrorEpilogueOutcome.MissingPreviousSeh
            && missingSeh.CaptureStates.PreviousSeh == GameSocketCompletionErrorEpilogueCaptureState.Missing
            && missingSeh.MemoryEffects.Length == 1
            && missingSeh.MemoryEffects[0] is
                { Address: 0x2FF4, Value: null, Attempted: true, Captured: false }
            && !missingSeh.MemoryEffects.Any(effect =>
                effect.Kind == GameSocketCompletionErrorEpilogueMemoryEffectKind.SegmentWrite)
            && missingSeh.StopBeforeRawFileOffset == 0xBD2E19
            && !missingSeh.ExactBoundedCompletion
            && missingSeh.UnusedCaptures.Length == 5);

        GameSocketCompletionErrorEpiloguePlan missingEsp = Project(
            postQueueEbp: 0x4000,
            previousSeh: 0,
            firstPopEsp: null,
            poppedEcx: 0,
            poppedEdi: 1,
            poppedEsi: 2,
            savedEbp: 3);
        check("A missing independent first-pop ESP follows the committed FS write and attempts POP ECX",
            missingEsp.Outcome == GameSocketCompletionErrorEpilogueOutcome.MissingFirstPopEsp
            && missingEsp.CaptureStates.FirstPopEsp == GameSocketCompletionErrorEpilogueCaptureState.Missing
            && missingEsp.CaptureStates.PoppedEcx
                == GameSocketCompletionErrorEpilogueCaptureState.SuppliedButUnused
            && missingEsp.MemoryEffects.Length == 3
            && missingEsp.MemoryEffects[1] is
                { Kind: GameSocketCompletionErrorEpilogueMemoryEffectKind.SegmentWrite,
                  Address: null, Value: 0, Committed: true, Segment: "FS", SegmentOffset: 0 }
            && missingEsp.MemoryEffects[2] is
                { Kind: GameSocketCompletionErrorEpilogueMemoryEffectKind.StackRead,
                  Address: null, Value: null, Attempted: true, Captured: false,
                  InstructionRawFileOffset: 0xBD2E23 }
            && missingEsp.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[]
                { 0xBD2E19, 0xBD2E1C, 0xBD2E23 })
            && !missingEsp.ExactBoundedCompletion
            && missingEsp.UnusedCaptures.Length == 4);

        GameSocketCompletionErrorEpiloguePlan missingEcx = Project(
            postQueueEbp: 0x4000,
            previousSeh: 0,
            firstPopEsp: 0x5000,
            poppedEcx: null,
            poppedEdi: 1,
            poppedEsi: 2,
            savedEbp: 3);
        check("A missing POP ECX value stops at that read and leaves later stack captures unused",
            missingEcx.Outcome == GameSocketCompletionErrorEpilogueOutcome.MissingPoppedEcx
            && missingEcx.CaptureStates.PoppedEcx == GameSocketCompletionErrorEpilogueCaptureState.Missing
            && missingEcx.MemoryEffects[^1] is
                { Address: 0x5000, Value: null, Attempted: true, Captured: false,
                  InstructionRawFileOffset: 0xBD2E23 }
            && missingEcx.LastReachedRawFileOffset == 0xBD2E23
            && missingEcx.StopBeforeRawFileOffset == 0xBD2E23
            && !missingEcx.ExactBoundedCompletion
            && missingEcx.UnusedCaptures.Length == 3);

        GameSocketCompletionErrorEpiloguePlan missingEdi = Project(
            postQueueEbp: 0x4000,
            previousSeh: 0,
            firstPopEsp: 0x5000,
            poppedEcx: 0,
            poppedEdi: null,
            poppedEsi: 2,
            savedEbp: 3);
        check("A missing POP EDI stops after the independently captured ECX pop",
            missingEdi.Outcome == GameSocketCompletionErrorEpilogueOutcome.MissingPoppedEdi
            && missingEdi.CaptureStates.PoppedEdi == GameSocketCompletionErrorEpilogueCaptureState.Missing
            && missingEdi.MemoryEffects[^1] is
                { Address: 0x5004, Value: null, Attempted: true, Captured: false,
                  InstructionRawFileOffset: 0xBD2E24 }
            && missingEdi.LastReachedRawFileOffset == 0xBD2E24
            && missingEdi.StopBeforeRawFileOffset == 0xBD2E24
            && !missingEdi.ExactBoundedCompletion
            && missingEdi.UnusedCaptures.Length == 2);

        GameSocketCompletionErrorEpiloguePlan missingEsi = Project(
            postQueueEbp: 0x4000,
            previousSeh: 0,
            firstPopEsp: 0x5000,
            poppedEcx: 0,
            poppedEdi: 0,
            poppedEsi: null,
            savedEbp: 3);
        check("A missing POP ESI stops before ESP is restored from EBP",
            missingEsi.Outcome == GameSocketCompletionErrorEpilogueOutcome.MissingPoppedEsi
            && missingEsi.CaptureStates.PoppedEsi == GameSocketCompletionErrorEpilogueCaptureState.Missing
            && missingEsi.MemoryEffects[^1] is
                { Address: 0x5008, Value: null, Attempted: true, Captured: false,
                  InstructionRawFileOffset: 0xBD2E25 }
            && !missingEsi.ReachedInstructionRawFileOffsets.Contains(0xBD2E26)
            && missingEsi.LastReachedRawFileOffset == 0xBD2E25
            && missingEsi.StopBeforeRawFileOffset == 0xBD2E25
            && !missingEsi.ExactBoundedCompletion
            && missingEsi.UnusedCaptures.Length == 1);

        GameSocketCompletionErrorEpiloguePlan missingSavedEbp = Project(
            postQueueEbp: 0x4000,
            previousSeh: 0,
            firstPopEsp: 0x5000,
            poppedEcx: 0,
            poppedEdi: 0,
            poppedEsi: 0,
            savedEbp: null);
        check("A missing saved EBP stops at POP EBP after ESP was restored",
            missingSavedEbp.Outcome == GameSocketCompletionErrorEpilogueOutcome.MissingSavedEbp
            && missingSavedEbp.CaptureStates.SavedEbp
                == GameSocketCompletionErrorEpilogueCaptureState.Missing
            && missingSavedEbp.MemoryEffects[^1] is
                { Address: 0x4000, Value: null, Attempted: true, Captured: false,
                  InstructionRawFileOffset: 0xBD2E28 }
            && missingSavedEbp.ReachedInstructionRawFileOffsets.Contains(0xBD2E26)
            && missingSavedEbp.StopBeforeRawFileOffset == 0xBD2E28
            && !missingSavedEbp.ExactBoundedCompletion
            && missingSavedEbp.RestoredEbp is null
            && missingSavedEbp.EspBeforeNativeRet is null);

        GameSocketCompletionErrorEpiloguePlan wrap = Project(
            postQueueEbp: 8,
            previousSeh: 0,
            firstPopEsp: 0xFFFFFFFC,
            poppedEcx: 0,
            poppedEdi: 0,
            poppedEsi: 0,
            savedEbp: 0);
        check("EBP displacement and independent POP ESP additions wrap as unsigned DWORDs",
            wrap.PreviousSehReadAddress == 0xFFFFFFFC
            && wrap.PoppedEcxAddress == 0xFFFFFFFC
            && wrap.PoppedEdiAddress == 0
            && wrap.PoppedEsiAddress == 4
            && wrap.SavedEbpAddress == 8
            && wrap.EspBeforeNativeRet == 12);

        GameSocketCompletionErrorEpiloguePlan aliasedReads = Project(
            postQueueEbp: 0x100,
            previousSeh: 0,
            firstPopEsp: 0xF4,
            poppedEcx: 0x12345678,
            poppedEdi: 0,
            poppedEsi: 0,
            savedEbp: 0);
        check("Aliased SEH and stack addresses retain independent captured values",
            aliasedReads.MemoryEffects[0].Address == aliasedReads.MemoryEffects[2].Address
            && aliasedReads.MemoryEffects[0].Value == 0
            && aliasedReads.MemoryEffects[2].Value == 0x12345678
            && aliasedReads.MemoryEffects[1].Value == 0
            && aliasedReads.RestoredEbp == 0);

        GameSocketCompletionErrorEpiloguePlan dirtyParent = Project(
            postQueueEbp: 0x6000,
            previousSeh: 1,
            firstPopEsp: 0x7000,
            poppedEcx: 2,
            poppedEdi: 3,
            poppedEsi: 4,
            savedEbp: 5,
            parentAtomicObserved: 1,
            parentFreshField: 0xABCD);
        check("A dirty parent is rejected even when its bounded endpoint and return flag are true",
            dirtyParent.ParentPlan.HasActualBoundedCompletion
            && dirtyParent.ParentPlan.QueueAppendReturnedNormally == true
            && dirtyParent.ParentPlan.StopBeforeRawFileOffset == 0xBD2E19
            && !dirtyParent.ParentPlan.IsCleanCompletion
            && dirtyParent.ExactBoundedCompletion
            && dirtyParent.Outcome == GameSocketCompletionErrorEpilogueOutcome.ParentBoundary
            && !dirtyParent.ProjectionEntered
            && dirtyParent.ReachedInstructionRawFileOffsets.IsEmpty
            && dirtyParent.MemoryEffects.IsEmpty
            && dirtyParent.StopBeforeRawFileOffset == dirtyParent.ParentPlan.StopBeforeRawFileOffset
            && dirtyParent.LastReachedRawFileOffset == dirtyParent.ParentPlan.LastReachedRawFileOffset
            && dirtyParent.FlagsAtStop == dirtyParent.ParentPlan.FlagsAtStop
            && dirtyParent.CaptureStates.PostQueueEbp
                == GameSocketCompletionErrorEpilogueCaptureState.SuppliedButUnused
            && dirtyParent.UnusedCaptures.Length == 7);

        GameSocketCompletionErrorEpiloguePlan missingParentReturn = Project(
            postQueueEbp: 0x6000,
            previousSeh: 1,
            firstPopEsp: 0x7000,
            poppedEcx: 2,
            poppedEdi: 3,
            poppedEsi: 4,
            savedEbp: 5,
            parentQueueReturnedNormally: null);
        check("A parent stopped at the queue call retains its exact bound and flags",
            missingParentReturn.ParentPlan.QueueAppendCallSiteReached
            && missingParentReturn.ParentPlan.LastReachedRawFileOffset == 0xBD2E14
            && missingParentReturn.ParentPlan.StopBeforeRawFileOffset == 0xBD2E19
            && missingParentReturn.ParentPlan.FlagsAtStop == missingParentReturn.ParentPlan.FlagsAfterTestEax
            && missingParentReturn.Outcome == GameSocketCompletionErrorEpilogueOutcome.ParentBoundary
            && missingParentReturn.StopBeforeRawFileOffset == 0xBD2E19
            && missingParentReturn.LastReachedRawFileOffset == 0xBD2E14
            && missingParentReturn.FlagsAtStop == missingParentReturn.ParentPlan.FlagsAtStop
            && !missingParentReturn.ExactBoundedCompletion
            && !missingParentReturn.ProjectionEntered
            && missingParentReturn.UnusedCaptures.Length == 7);

        GameSocketCompletionErrorEpiloguePlan missingParentAtomic = Project(
            postQueueEbp: 0x6000,
            previousSeh: 1,
            firstPopEsp: 0x7000,
            poppedEcx: 2,
            poppedEdi: 3,
            poppedEsi: 4,
            savedEbp: 5,
            parentAtomicObserved: null);
        check("A parent stopped at its own missing atomic read is not advanced into the epilogue",
            missingParentAtomic.ParentPlan.Outcome
                == GameSocketCompletionErrorStateOutcome.MissingAtomicObservation
            && missingParentAtomic.ParentPlan.LastReachedRawFileOffset == 0xBD2DF8
            && missingParentAtomic.ParentPlan.StopBeforeRawFileOffset == 0xBD2DF8
            && missingParentAtomic.Outcome == GameSocketCompletionErrorEpilogueOutcome.ParentBoundary
            && missingParentAtomic.StopBeforeRawFileOffset == 0xBD2DF8
            && missingParentAtomic.FlagsAtStop == missingParentAtomic.ParentPlan.FlagsAtStop
            && !missingParentAtomic.ExactBoundedCompletion
            && missingParentAtomic.ReachedInstructionRawFileOffsets.IsEmpty);

        GameSocketCompletionErrorEpiloguePlan abnormalParentReturn = Project(
            postQueueEbp: 0x6000,
            previousSeh: 1,
            firstPopEsp: 0x7000,
            poppedEcx: 2,
            poppedEdi: 3,
            poppedEsi: 4,
            savedEbp: 5,
            parentQueueReturnedNormally: false);
        check("A non-normal parent return retains its boundary without claiming exact completion",
            abnormalParentReturn.ParentPlan.QueueAppendReturnedNormally == false
            && abnormalParentReturn.ParentPlan.StopBeforeRawFileOffset == 0xBD2E19
            && abnormalParentReturn.ParentPlan.FlagsAtStop
                == abnormalParentReturn.ParentPlan.FlagsAfterTestEax
            && !abnormalParentReturn.ParentPlan.HasActualBoundedCompletion
            && abnormalParentReturn.Outcome == GameSocketCompletionErrorEpilogueOutcome.ParentBoundary
            && abnormalParentReturn.StopBeforeRawFileOffset == 0xBD2E19
            && abnormalParentReturn.FlagsAtStop == abnormalParentReturn.ParentPlan.FlagsAtStop
            && !abnormalParentReturn.ExactBoundedCompletion);
    }

    private static GameSocketCompletionErrorEpiloguePlan Project(
        uint? postQueueEbp,
        uint? previousSeh,
        uint? firstPopEsp,
        uint? poppedEcx,
        uint? poppedEdi,
        uint? poppedEsi,
        uint? savedEbp,
        uint? parentAtomicObserved = 0,
        uint? parentFreshField = 0x44,
        bool? parentQueueReturnedNormally = true)
    {
        var parent = new GameSocketCompletionErrorStateCapture(
            true,
            0x1000,
            parentAtomicObserved,
            parentFreshField,
            parentQueueReturnedNormally);
        return GameSocketCompletionErrorEpilogueProjection.Project(new(
            parent,
            postQueueEbp,
            previousSeh,
            firstPopEsp,
            poppedEcx,
            poppedEdi,
            poppedEsi,
            savedEbp));
    }
}
