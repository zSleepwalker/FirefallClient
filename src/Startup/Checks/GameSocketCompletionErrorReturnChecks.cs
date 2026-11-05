using Firefall.Reconstruction.Startup;

internal static class GameSocketCompletionErrorReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The exact RET 4 bytes, raw boundary, and VA match the epilogue",
            GameSocketCompletionErrorReturnProjection.NativeRetInstructionByteLength == 3
            && GameSocketCompletionErrorReturnProjection.NativeRetRawFileOffset == 0xBD2E29
            && GameSocketCompletionErrorReturnProjection.AfterNativeRetRawFileOffset == 0xBD2E2C
            && GameSocketCompletionErrorReturnProjection.NativeRetVirtualAddress == 0xFD3A29
            && GameSocketCompletionErrorReturnProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && GameSocketCompletionErrorReturnProjection.ReturnTargetStackOffset == 4
            && GameSocketCompletionErrorReturnProjection.ReturnAddressBytes == 4
            && GameSocketCompletionErrorReturnProjection.ArgumentBytes == 4
            && GameSocketCompletionErrorReturnProjection.NativeRetInstructionBytes.SequenceEqual(
                new byte[] { 0xC2, 0x04, 0x00 }));

        GameSocketCompletionErrorReturnPlan zeroTarget = Project(
            postQueueEbp: 0x2000,
            previousSeh: 0x11111111,
            firstPopEsp: 0x3000,
            poppedEcx: 0x22222222,
            poppedEdi: 0x33333333,
            poppedEsi: 0x44444444,
            savedEbp: 0x55555555,
            returnTarget: 0);
        check("A captured zero is a valid scalar RET target and advances ESP by RET 4",
            zeroTarget.ParentEpilogue.IsCleanEpilogue
            && zeroTarget.Outcome == GameSocketCompletionErrorReturnOutcome.BoundedAfterNativeRet
            && zeroTarget.Disposition == GameSocketCompletionErrorReturnDisposition.BoundedAfterNativeRet
            && zeroTarget.ProjectionEntered
            && zeroTarget.IsCleanReturn
            && zeroTarget.ExactBoundedCompletion
            && zeroTarget.ReturnTargetCaptureState == GameSocketCompletionErrorReturnCaptureState.Captured
            && zeroTarget.EspBeforeRet == 0x2004
            && zeroTarget.ReturnTargetReadAddress == 0x2004
            && zeroTarget.TargetVirtualAddress == 0
            && zeroTarget.EspAfterRet == 0x200C
            && zeroTarget.LastReachedRawFileOffset == 0xBD2E29
            && zeroTarget.StopBeforeRawFileOffset == 0xBD2E2C
            && zeroTarget.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] { 0xBD2E29 })
            && zeroTarget.MemoryEffects.Length == 1
            && zeroTarget.MemoryEffects[0] is
                { Kind: GameSocketCompletionErrorReturnMemoryEffectKind.ReturnTargetRead,
                  Address: 0x2004, Value: 0, Attempted: true, Captured: true, Committed: false,
                  InstructionRawFileOffset: 0xBD2E29 }
            && zeroTarget.NativeInstructionsExecuted == false
            && zeroTarget.NativeRetExecuted == false
            && zeroTarget.TargetEntryExecuted == false
            && zeroTarget.CallerReturnAddressMappingKnown == false
            && zeroTarget.FlagsAtStop is
                { Carry: null, Parity: null, AuxiliaryCarry: null, Zero: null, Sign: null, Overflow: null });

        GameSocketCompletionErrorReturnPlan nonzeroTarget = Project(
            postQueueEbp: 0x2000,
            previousSeh: 1,
            firstPopEsp: 0x3000,
            poppedEcx: 2,
            poppedEdi: 3,
            poppedEsi: 4,
            savedEbp: 5,
            returnTarget: 0xC20400);
        check("A nonzero target is captured without entering it or asserting a PE mapping",
            nonzeroTarget.TargetVirtualAddress == 0xC20400
            && nonzeroTarget.EspAfterRet == 0x200C
            && !nonzeroTarget.TargetEntryExecuted
            && !nonzeroTarget.CallerReturnAddressMappingKnown
            && nonzeroTarget.Outcome == GameSocketCompletionErrorReturnOutcome.BoundedAfterNativeRet);

        GameSocketCompletionErrorReturnPlan missingTarget = Project(
            postQueueEbp: 0x2000,
            previousSeh: 1,
            firstPopEsp: 0x3000,
            poppedEcx: 2,
            poppedEdi: 3,
            poppedEsi: 4,
            savedEbp: 5,
            returnTarget: null);
        check("A missing fresh return target stops before RET and does not claim exact completion",
            missingTarget.ParentEpilogue.IsCleanEpilogue
            && missingTarget.ProjectionEntered
            && missingTarget.Outcome == GameSocketCompletionErrorReturnOutcome.MissingReturnTarget
            && missingTarget.ReturnTargetCaptureState == GameSocketCompletionErrorReturnCaptureState.Missing
            && missingTarget.MemoryEffects.Length == 1
            && missingTarget.MemoryEffects[0] is
                { Address: 0x2004, Value: null, Attempted: true, Captured: false, Committed: false }
            && missingTarget.ReachedInstructionRawFileOffsets.IsEmpty
            && missingTarget.LastReachedRawFileOffset == 0xBD2E28
            && missingTarget.StopBeforeRawFileOffset == 0xBD2E29
            && !missingTarget.ExactBoundedCompletion
            && missingTarget.EspAfterRet is null
            && missingTarget.TargetVirtualAddress is null
            && missingTarget.FlagsAtStop is
                { Carry: null, Parity: null, AuxiliaryCarry: null, Zero: null, Sign: null, Overflow: null });

        GameSocketCompletionErrorReturnPlan wrapFromF8 = Project(
            postQueueEbp: 0xFFFFFFF8,
            previousSeh: 0,
            firstPopEsp: 0x1000,
            poppedEcx: 1,
            poppedEdi: 2,
            poppedEsi: 3,
            savedEbp: 4,
            returnTarget: 0xABCDEF01);
        GameSocketCompletionErrorReturnPlan wrapFromFC = Project(
            postQueueEbp: 0xFFFFFFFC,
            previousSeh: 0,
            firstPopEsp: 0x1000,
            poppedEcx: 1,
            poppedEdi: 2,
            poppedEsi: 3,
            savedEbp: 4,
            returnTarget: 0);
        check("RET stack arithmetic wraps as DWORDs for both boundary cases",
            wrapFromF8.EspBeforeRet == 0xFFFFFFFC
            && wrapFromF8.ReturnTargetReadAddress == 0xFFFFFFFC
            && wrapFromF8.EspAfterRet == 4
            && wrapFromFC.EspBeforeRet == 0
            && wrapFromFC.ReturnTargetReadAddress == 0
            && wrapFromFC.EspAfterRet == 8);

        GameSocketCompletionErrorReturnPlan aliasedCaptures = Project(
            postQueueEbp: 0x2000,
            previousSeh: 0xA1A1A1A1,
            firstPopEsp: 0x2004,
            poppedEcx: 0xB2B2B2B2,
            poppedEdi: 0xC3C3C3C3,
            poppedEsi: 0xD4D4D4D4,
            savedEbp: 0xE5E5E5E5,
            returnTarget: 0xF6F6F6F6);
        check("Aliased stack locations retain independent fresh return, POP EBP, and SEH captures",
            aliasedCaptures.ParentEpilogue.PreviousSehReadAddress == 0x1FF4
            && aliasedCaptures.ParentEpilogue.SavedEbpAddress == 0x2000
            && aliasedCaptures.ParentEpilogue.PoppedEcxAddress == aliasedCaptures.ReturnTargetReadAddress
            && aliasedCaptures.ParentEpilogue.MemoryEffects[0].Value == 0xA1A1A1A1
            && aliasedCaptures.ParentEpilogue.MemoryEffects[^1].Value == 0xE5E5E5E5
            && aliasedCaptures.ParentEpilogue.MemoryEffects[2].Value == 0xB2B2B2B2
            && aliasedCaptures.MemoryEffects[0].Value == 0xF6F6F6F6
            && aliasedCaptures.TargetVirtualAddress == 0xF6F6F6F6);

        GameSocketCompletionErrorReturnCaptures originalCaptures = Captures(
            postQueueEbp: 0x4000,
            previousSeh: 7,
            firstPopEsp: 0x5000,
            poppedEcx: 8,
            poppedEdi: 9,
            poppedEsi: 10,
            savedEbp: 11,
            returnTarget: 0x12345678);
        GameSocketCompletionErrorReturnPlan original =
            GameSocketCompletionErrorReturnProjection.Project(originalCaptures);
        GameSocketCompletionErrorReturnPlan reprojected =
            GameSocketCompletionErrorReturnProjection.Project(originalCaptures with { ReturnTarget = 0x87654321 });
        check("Immutable captures can be reprojected without mutating an earlier return plan",
            original.TargetVirtualAddress == 0x12345678
            && reprojected.TargetVirtualAddress == 0x87654321
            && original.MemoryEffects[0].Value == 0x12345678
            && reprojected.MemoryEffects[0].Value == 0x87654321
            && original.ParentEpilogue.OrderedTrace.SequenceEqual(reprojected.ParentEpilogue.OrderedTrace)
            && original.EspAfterRet == reprojected.EspAfterRet);

        check("Ordered trace preserves the epilogue prefix, appends the return read, then transfer metadata",
            zeroTarget.OrderedTrace.Length == zeroTarget.ParentEpilogue.OrderedTrace.Length + 2
            && zeroTarget.OrderedTrace.Take(zeroTarget.ParentEpilogue.OrderedTrace.Length)
                .Select(step => step.ParentEpilogueTraceStep)
                .SequenceEqual(zeroTarget.ParentEpilogue.OrderedTrace.Select(step =>
                    (GameSocketCompletionErrorEpilogueTraceStep?)step))
            && zeroTarget.OrderedTrace.Skip(zeroTarget.ParentEpilogue.OrderedTrace.Length)
                .Select(step => step.Kind)
                .SequenceEqual(new[]
                {
                    GameSocketCompletionErrorReturnTraceKind.ReturnTargetRead,
                    GameSocketCompletionErrorReturnTraceKind.LogicalTransfer
                })
            && zeroTarget.OrderedTrace[^2].ReturnMemoryEffect == zeroTarget.MemoryEffects[0]
            && zeroTarget.OrderedTrace[^1].Transfer is
                { TargetVirtualAddress: 0, EspBeforeRet: 0x2004, EspAfterRet: 0x200C,
                  ReturnAddressBytes: 4, ArgumentBytes: 4, InstructionRawFileOffset: 0xBD2E29 }
            && zeroTarget.OrderedTrace.Select(step => step.Sequence)
                .SequenceEqual(Enumerable.Range(0, zeroTarget.OrderedTrace.Length))
            && zeroTarget.MemoryEffects.All(effect =>
                effect.Kind != GameSocketCompletionErrorReturnMemoryEffectKind.ReturnTargetRead
                || effect.Committed != true));

        GameSocketCompletionErrorReturnPlan dirtyParent = Project(
            postQueueEbp: 0x6000,
            previousSeh: 1,
            firstPopEsp: 0x7000,
            poppedEcx: 2,
            poppedEdi: 3,
            poppedEsi: 4,
            savedEbp: 5,
            returnTarget: 0xC20400,
            parentAtomicObserved: 1,
            parentFreshField: 0xABCD);
        check("A rejected dirty parent retains its endpoint and leaves a supplied target unused",
            dirtyParent.ParentEpilogue.ExactBoundedCompletion
            && !dirtyParent.ParentEpilogue.IsCleanEpilogue
            && dirtyParent.Outcome == GameSocketCompletionErrorReturnOutcome.ParentBoundary
            && !dirtyParent.ProjectionEntered
            && dirtyParent.MemoryEffects.IsEmpty
            && dirtyParent.ExactBoundedCompletion
            && dirtyParent.LastReachedRawFileOffset == dirtyParent.ParentEpilogue.LastReachedRawFileOffset
            && dirtyParent.StopBeforeRawFileOffset == dirtyParent.ParentEpilogue.StopBeforeRawFileOffset
            && dirtyParent.FlagsAtStop == dirtyParent.ParentEpilogue.FlagsAtStop
            && dirtyParent.ReturnTargetCaptureState
                == GameSocketCompletionErrorReturnCaptureState.SuppliedButUnused);

        GameSocketCompletionErrorReturnPlan missingParent = Project(
            postQueueEbp: 0x6000,
            previousSeh: 1,
            firstPopEsp: 0x7000,
            poppedEcx: 2,
            poppedEdi: 3,
            poppedEsi: 4,
            savedEbp: null,
            returnTarget: 0xC20400);
        check("An incomplete parent is not entered and preserves its own exact-bound state",
            !missingParent.ParentEpilogue.IsCleanEpilogue
            && missingParent.Outcome == GameSocketCompletionErrorReturnOutcome.ParentBoundary
            && !missingParent.ProjectionEntered
            && missingParent.MemoryEffects.IsEmpty
            && missingParent.ReturnTargetCaptureState
                == GameSocketCompletionErrorReturnCaptureState.SuppliedButUnused
            && missingParent.ExactBoundedCompletion
                == missingParent.ParentEpilogue.ExactBoundedCompletion
            && missingParent.StopBeforeRawFileOffset == missingParent.ParentEpilogue.StopBeforeRawFileOffset);
    }

    private static GameSocketCompletionErrorReturnPlan Project(
        uint? postQueueEbp,
        uint? previousSeh,
        uint? firstPopEsp,
        uint? poppedEcx,
        uint? poppedEdi,
        uint? poppedEsi,
        uint? savedEbp,
        uint? returnTarget,
        uint? parentAtomicObserved = 0,
        uint? parentFreshField = 0x44,
        bool? parentQueueReturnedNormally = true) =>
        GameSocketCompletionErrorReturnProjection.Project(Captures(
            postQueueEbp,
            previousSeh,
            firstPopEsp,
            poppedEcx,
            poppedEdi,
            poppedEsi,
            savedEbp,
            returnTarget,
            parentAtomicObserved,
            parentFreshField,
            parentQueueReturnedNormally));

    private static GameSocketCompletionErrorReturnCaptures Captures(
        uint? postQueueEbp,
        uint? previousSeh,
        uint? firstPopEsp,
        uint? poppedEcx,
        uint? poppedEdi,
        uint? poppedEsi,
        uint? savedEbp,
        uint? returnTarget,
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
        var epilogue = new GameSocketCompletionErrorEpilogueCaptures(
            parent,
            postQueueEbp,
            previousSeh,
            firstPopEsp,
            poppedEcx,
            poppedEdi,
            poppedEsi,
            savedEbp);
        return new(epilogue, returnTarget);
    }
}
