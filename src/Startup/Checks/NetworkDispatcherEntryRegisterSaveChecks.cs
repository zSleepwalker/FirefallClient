using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryRegisterSaveChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The entry register-save projection is bounded before the cookie global read",
            NetworkDispatcherEntryRegisterSaveProjection.EntryRawFileOffset == 0x00BBDDC4
            && NetworkDispatcherEntryRegisterSaveProjection.SaveEBXRawFileOffset == 0x00BBDDC4
            && NetworkDispatcherEntryRegisterSaveProjection.SaveESIRawFileOffset == 0x00BBDDC5
            && NetworkDispatcherEntryRegisterSaveProjection.SaveEDIRawFileOffset == 0x00BBDDC6
            && NetworkDispatcherEntryRegisterSaveProjection.CookieGlobalReadRawFileOffset == 0x00BBDDC7
            && NetworkDispatcherEntryRegisterSaveProjection.PrefixEndRawFileOffsetExclusive == 0x00BBDDC7
            && Normalize(NetworkDispatcherEntryRegisterSaveProjection.CapturedWindowBytes)
                == "558BEC6AFF68183CA40164A1000000005083EC0C535657A10050DC01"
            && !NetworkDispatcherEntryRegisterSaveProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryFramePrefixCapture parentCapture = ParentCapture();
        NetworkDispatcherEntryFramePrefixPlan parentPlan =
            NetworkDispatcherEntryFramePrefixProjection.Project(parentCapture);
        NetworkDispatcherEntryRegisterSavePlan complete =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                parentCapture,
                parentPlan,
                entryEBX: 0,
                entryESI: uint.MaxValue,
                entryEDI: 0x12345678));

        check("The accepted parent prefix flows into three ordered register saves without reading the cookie or rewriting FS",
            complete.ParentPlanMatchesCapture
            && complete.ParentPlan.ExactPrefixProjected
            && complete.ParentPlan.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFramePrefixProjection.PrefixEndRawFileOffsetExclusive
            && complete.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.RegisterSavesProjected
            && complete.CompletedInstructionOffsets.SequenceEqual(new uint[]
            {
                0x00BBDDC4, 0x00BBDDC5, 0x00BBDDC6
            })
            && complete.Pushes.Select(push => push.RawFileOffset).SequenceEqual(new uint[]
            {
                0x00BBDDC4, 0x00BBDDC5, 0x00BBDDC6
            })
            && complete.Pushes.Select(push => push.Value).SequenceEqual(new uint[]
            {
                0, uint.MaxValue, 0x12345678
            })
            && complete.Pushes.Select(push => push.StackAddress).SequenceEqual(new uint?[]
            {
                0x1FE0, 0x1FDC, 0x1FD8
            })
            && complete.Pushes.All(push =>
                push.StackAddressKnown && push.Attempted && push.Committed)
            && complete.EntryEBP == 0x1FFC
            && complete.CurrentESPAtStop == 0x1FD8
            && complete.FlagsAtStop == parentPlan.FlagsAtStop
            && complete.FreshEntryEBXCaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Captured
            && complete.FreshEntryESICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Captured
            && complete.FreshEntryEDICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Captured
            && complete.ExactRegisterSavesProjected
            && complete.StopAtRawFileOffsetExclusive == 0x00BBDDC7
            && !complete.CookieGlobalReadReached);

        NetworkDispatcherEntryRegisterSavePlan missingEBX =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                parentCapture, parentPlan, entryEBX: null,
                entryESI: 0, entryEDI: uint.MaxValue));
        NetworkDispatcherEntryRegisterSavePlan missingESI =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                parentCapture, parentPlan, entryEBX: uint.MaxValue,
                entryESI: null, entryEDI: 0));
        NetworkDispatcherEntryRegisterSavePlan missingEDI =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                parentCapture, parentPlan, entryEBX: 0,
                entryESI: uint.MaxValue, entryEDI: null));
        NetworkDispatcherEntryRegisterSavePlan missingEBXWithUnreachedRegisters =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                parentCapture, parentPlan, entryEBX: null,
                entryESI: null, entryEDI: null));
        NetworkDispatcherEntryRegisterSavePlan missingESIWithUnreachedEDI =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                parentCapture, parentPlan, entryEBX: 0,
                entryESI: null, entryEDI: null));
        check("A missing fresh register stops at its own push after retaining only earlier saves",
            missingEBX.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryEBX
            && missingEBX.Pushes.IsEmpty
            && missingEBX.CurrentESPAtStop == parentPlan.CurrentESPAtStop
            && missingEBX.StopAtRawFileOffsetExclusive == 0x00BBDDC4
            && missingEBX.FreshEntryEBXCaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Missing
            && missingEBX.FreshEntryESICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.SuppliedUnused
            && missingEBX.FreshEntryEDICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.SuppliedUnused
            && missingEBXWithUnreachedRegisters.FreshEntryESICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Unreached
            && missingEBXWithUnreachedRegisters.FreshEntryEDICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Unreached
            && missingESI.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryESI
            && missingESI.CompletedInstructionOffsets.SequenceEqual(new uint[]
            {
                0x00BBDDC4
            })
            && missingESI.Pushes is [{ Value: uint.MaxValue, StackAddress: 0x1FE0 }]
            && missingESI.CurrentESPAtStop == 0x1FE0
            && missingESI.StopAtRawFileOffsetExclusive == 0x00BBDDC5
            && missingESI.FreshEntryESICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Missing
            && missingESI.FreshEntryEDICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.SuppliedUnused
            && missingESIWithUnreachedEDI.FreshEntryEDICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Unreached
            && missingEDI.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryEDI
            && missingEDI.CompletedInstructionOffsets.SequenceEqual(new uint[]
            {
                0x00BBDDC4, 0x00BBDDC5
            })
            && missingEDI.Pushes.Select(push => push.Value).SequenceEqual(new uint[]
            {
                0, uint.MaxValue
            })
            && missingEDI.Pushes.Select(push => push.StackAddress).SequenceEqual(new uint?[]
            {
                0x1FE0, 0x1FDC
            })
            && missingEDI.CurrentESPAtStop == 0x1FDC
            && missingEDI.StopAtRawFileOffsetExclusive == 0x00BBDDC6
            && missingEDI.FreshEntryEDICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Missing);

        NetworkDispatcherEntryRegisterSavePlan wrapped =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                parentCapture,
                parentPlan,
                entryEBX: uint.MaxValue,
                entryESI: 0,
                entryEDI: uint.MaxValue));
        check("Register pushes wrap as DWORDs and do not alter any of the six parent flags",
            NetworkDispatcherEntryRegisterSaveProjection.StackAddressAfterPush(0) == 0xFFFFFFFC
            && wrapped.Pushes.Select(push => push.StackAddress).SequenceEqual(new uint?[]
            {
                0x1FE0, 0x1FDC, 0x1FD8
            })
            && wrapped.Pushes.Select(push => push.Value).SequenceEqual(new uint[]
            {
                uint.MaxValue, 0, uint.MaxValue
            })
            && wrapped.FlagsAtStop == parentPlan.FlagsAtStop
            && wrapped.FlagsAtStop == new NetworkIoCompletionClockGateFlags(
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: false));

        NetworkDispatcherEntryFramePrefixCapture zeroStackParentCapture =
            ParentCapture(entryESP: 0x1C, entryEBP: uint.MaxValue, fsOffset0DWORD: 0);
        NetworkDispatcherEntryFramePrefixPlan zeroStackParentPlan =
            NetworkDispatcherEntryFramePrefixProjection.Project(zeroStackParentCapture);
        NetworkDispatcherEntryRegisterSavePlan zeroStack =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                zeroStackParentCapture,
                zeroStackParentPlan,
                entryEBX: 0,
                entryESI: uint.MaxValue,
                entryEDI: 0));
        NetworkDispatcherEntryRegisterSavePlan zeroStackMissingESI =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                zeroStackParentCapture,
                zeroStackParentPlan,
                entryEBX: 0,
                entryESI: null,
                entryEDI: uint.MaxValue));
        NetworkDispatcherEntryRegisterSavePlan zeroStackMissingEDI =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                zeroStackParentCapture,
                zeroStackParentPlan,
                entryEBX: 0,
                entryESI: uint.MaxValue,
                entryEDI: null));
        check("A zero parent ESP wraps each committed push, including partial missing-register prefixes",
            zeroStackParentPlan.CurrentESPAtStop == 0
            && zeroStack.EntryEBP == 0x18
            && zeroStack.Pushes.Select(push => push.StackAddress).SequenceEqual(new uint?[]
            {
                0xFFFFFFFC, 0xFFFFFFF8, 0xFFFFFFF4
            })
            && zeroStack.CurrentESPAtStop == 0xFFFFFFF4
            && zeroStackMissingESI.CurrentESPAtStop == 0xFFFFFFFC
            && zeroStackMissingESI.StopAtRawFileOffsetExclusive == 0x00BBDDC5
            && zeroStackMissingEDI.CurrentESPAtStop == 0xFFFFFFF8
            && zeroStackMissingEDI.StopAtRawFileOffsetExclusive == 0x00BBDDC6);

        NetworkDispatcherEntryFramePrefixCapture rejectedParentCapture =
            ParentCapture(callerCapture: CallerCapture(
                freshClockField: 500,
                clockReturnEAX: 999));
        NetworkDispatcherEntryFramePrefixPlan rejectedParentPlan =
            NetworkDispatcherEntryFramePrefixProjection.Project(rejectedParentCapture);
        NetworkDispatcherEntryRegisterSavePlan rejectedParent =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                rejectedParentCapture,
                rejectedParentPlan,
                entryEBX: 0,
                entryESI: uint.MaxValue,
                entryEDI: 0));
        check("A rejected parent forwards its actual endpoint and caller outcome without forcing entry at C4",
            rejectedParent.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.ParentPrefixNotAccepted
            && rejectedParent.ParentPlanMatchesCapture
            && rejectedParent.ParentPlan.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.CallerDispatcherCallNotReached
            && !rejectedParent.ParentPlan.CallerDispatcherCallReached
            && rejectedParent.ParentPlan.CallerClockGatePlan.Outcome
                == NetworkIoCompletionClockGateOutcome.BelowThreshold
            && rejectedParent.StopAtRawFileOffsetExclusive
                == rejectedParent.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedParent.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.BoundedContinuationRawFileOffset
            && rejectedParent.CompletedInstructionOffsets.IsEmpty
            && rejectedParent.Pushes.IsEmpty
            && rejectedParent.FreshEntryEBXCaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.SuppliedUnused
            && rejectedParent.FreshEntryESICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.SuppliedUnused
            && rejectedParent.FreshEntryEDICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.SuppliedUnused);

        NetworkDispatcherEntryFramePrefixCapture missingFSParentCapture =
            ParentCapture(entryFlags: new NetworkIoCompletionClockGateFlags(
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false),
                fsOffset0DWORD: null);
        NetworkDispatcherEntryFramePrefixPlan missingFSParentPlan =
            NetworkDispatcherEntryFramePrefixProjection.Project(missingFSParentCapture);
        NetworkDispatcherEntryRegisterSavePlan missingFSParent =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                missingFSParentCapture,
                missingFSParentPlan,
                entryEBX: 1,
                entryESI: 2,
                entryEDI: 3));
        NetworkDispatcherEntryFramePrefixCapture returnedCallerCapture =
            ParentCapture(callerCapture: CallerCapture(dispatcherReturnedNormally: true));
        NetworkDispatcherEntryFramePrefixPlan returnedCallerParentPlan =
            NetworkDispatcherEntryFramePrefixProjection.Project(returnedCallerCapture);
        NetworkDispatcherEntryRegisterSavePlan returnedCaller =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                returnedCallerCapture,
                returnedCallerParentPlan,
                entryEBX: 1,
                entryESI: 2,
                entryEDI: 3));
        NetworkDispatcherEntryFramePrefixCapture failedCallerCapture =
            ParentCapture(callerCapture: CallerCapture(dispatcherReturnedNormally: false));
        NetworkDispatcherEntryFramePrefixPlan failedCallerParentPlan =
            NetworkDispatcherEntryFramePrefixProjection.Project(failedCallerCapture);
        NetworkDispatcherEntryRegisterSavePlan failedCaller =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                failedCallerCapture,
                failedCallerParentPlan,
                entryEBX: 1,
                entryESI: 2,
                entryEDI: 3));
        check("Missing parent FS capture and returned caller outcomes keep their own endpoints and untouched lineage",
            missingFSParent.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.ParentPrefixNotAccepted
            && missingFSParent.ParentPlan.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.MissingFreshFSOffset0DWORD
            && missingFSParent.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFramePrefixProjection.ReadFSOffset0RawFileOffset
            && missingFSParent.ParentPlan.CompletedInstructionOffsets.SequenceEqual(new uint[]
            {
                0x00BBDDB0, 0x00BBDDB1, 0x00BBDDB3, 0x00BBDDB5
            })
            && missingFSParent.CompletedInstructionOffsets.IsEmpty
            && missingFSParent.Pushes.IsEmpty
            && missingFSParent.EntryEBP == missingFSParentPlan.EntryEBPAfterMov
            && missingFSParent.CurrentESPAtStop == missingFSParentPlan.CurrentESPAtStop
            && missingFSParent.FlagsAtStop == missingFSParentPlan.FlagsAtStop
            && returnedCaller.ParentPlan.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.CallerDispatcherReturnAlreadyObserved
            && returnedCaller.ParentPlan.CallerClockGatePlan.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherReturnedNormally
            && returnedCaller.StopAtRawFileOffsetExclusive
                == returnedCallerParentPlan.StopAtRawFileOffsetExclusive
            && failedCaller.ParentPlan.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.CallerDispatcherReturnAlreadyObserved
            && failedCaller.ParentPlan.CallerClockGatePlan.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherDidNotReturnNormally
            && failedCaller.StopAtRawFileOffsetExclusive
                == failedCallerParentPlan.StopAtRawFileOffsetExclusive
            && returnedCaller.Pushes.IsEmpty
            && failedCaller.Pushes.IsEmpty);

        NetworkDispatcherEntryFramePrefixPlan independentlyProjectedParent =
            NetworkDispatcherEntryFramePrefixProjection.Project(parentCapture);
        NetworkDispatcherEntryRegisterSavePlan reprojected =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                parentCapture,
                independentlyProjectedParent,
                entryEBX: 0,
                entryESI: uint.MaxValue,
                entryEDI: 0x12345678));
        check("Equivalent parent projections with distinct immutable-array instances are accepted",
            !parentPlan.CompletedInstructionOffsets.Equals(
                independentlyProjectedParent.CompletedInstructionOffsets)
            && reprojected.ParentPlanMatchesCapture
            && reprojected.CompletedInstructionOffsets.SequenceEqual(
                complete.CompletedInstructionOffsets)
            && reprojected.Pushes.SequenceEqual(complete.Pushes));

        NetworkDispatcherEntryRegisterSavePlan mismatchedParent =
            NetworkDispatcherEntryRegisterSaveProjection.Project(Capture(
                parentCapture,
                parentPlan with { StopAtRawFileOffsetExclusive = 0x00BBDDC3 },
                entryEBX: 1,
                entryESI: 2,
                entryEDI: 3));
        check("A forged parent endpoint is rejected in favor of the original reprojected endpoint",
            mismatchedParent.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.ParentPlanMismatch
            && !mismatchedParent.ParentPlanMatchesCapture
            && mismatchedParent.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFramePrefixProjection.PrefixEndRawFileOffsetExclusive
            && mismatchedParent.Pushes.IsEmpty
            && mismatchedParent.FreshEntryEBXCaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.SuppliedUnused);
    }

    private static NetworkDispatcherEntryRegisterSaveCapture Capture(
        NetworkDispatcherEntryFramePrefixCapture parentCapture,
        NetworkDispatcherEntryFramePrefixPlan parentPlan,
        uint? entryEBX,
        uint? entryESI,
        uint? entryEDI) =>
        new(parentCapture, parentPlan, entryEBX, entryESI, entryEDI);

    private static NetworkDispatcherEntryFramePrefixCapture ParentCapture(
        uint? entryESP = 0x2000,
        uint? entryEBP = 0x1234,
        NetworkIoCompletionClockGateFlags? entryFlags = null,
        uint? fsOffset0DWORD = 0xAABBCCDD,
        NetworkIoCompletionClockGateCapture? callerCapture = null) =>
        new(
            callerCapture ?? CallerCapture(),
            FreshEntryESP: entryESP,
            FreshEntryEBP: entryEBP,
            FreshEntryFlags: entryFlags,
            FreshFSOffset0DWORD: fsOffset0DWORD);

    private static NetworkIoCompletionClockGateCapture CallerCapture(
        bool? dispatcherReturnedNormally = null,
        uint? freshClockField = 500,
        uint? clockReturnEAX = 1000) =>
        new(
            EntryValidated: true,
            EntryEBP: 0x2000,
            EntryFlags: null,
            FreshGlobalPointer: 0x1000,
            FreshClockField: freshClockField,
            CachedClockTargetVirtualAddress: 0x12345678,
            ClockReturnedNormally: true,
            ClockReturnEAX: clockReturnEAX,
            FreshSavedPointerRead: 0x2222,
            DispatcherReturnedNormally: dispatcherReturnedNormally);

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Where(Uri.IsHexDigit)).ToUpperInvariant();
}
