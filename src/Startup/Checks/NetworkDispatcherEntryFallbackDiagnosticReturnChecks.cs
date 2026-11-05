using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackDiagnosticReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackDiagnosticReturnCapture Capture(
            bool? normalReturnObserved = true,
            uint? freshComparedDword = 0,
            uint? freshEntryESP = 0x5008,
            uint? freshEntryECX = 0x12345678) =>
            new(
                new NetworkDispatcherEntryFallbackDiagnosticPreparationCapture(
                    new NetworkDispatcherEntryFallbackSelectorFallthroughCapture(
                        new NetworkDispatcherEntryFallbackSelectorBranchCapture(
                            new NetworkDispatcherEntryFallbackSelectorCompareCapture(
                                CreateZeroBranchCapture(),
                                freshComparedDword)),
                        freshEntryESP,
                        freshEntryECX)),
                normalReturnObserved);

        static NetworkDispatcherEntryFallbackDiagnosticReturnPlan Project(
            bool? normalReturnObserved = true,
            uint? freshComparedDword = 0,
            uint? freshEntryESP = 0x5008,
            uint? freshEntryECX = 0x12345678) =>
            NetworkDispatcherEntryFallbackDiagnosticReturnProjection.Project(
                Capture(
                    normalReturnObserved,
                    freshComparedDword,
                    freshEntryESP,
                    freshEntryECX));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("IDA confirms the direct CALL bytes, next-IP boundary, and mapped code target",
            NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .BytesIndependentlyVerified
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallIsDirect
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection.EntryRawFileOffset
                == 0x00BBDE26
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallRawFileOffset
                == 0x00BBDE26
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallCodeVirtualAddress
                == 0x00FBEA26
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallInstructionByteLength
                == 5
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .CallInstructionBytes) == "E805427000"
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallOpcode == 0xE8
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .CallRelativeDisplacement == 0x00704205
            && unchecked(
                NetworkDispatcherEntryFallbackDiagnosticReturnProjection.CallRawFileOffset
                + NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .CallInstructionByteLength
                + (uint)NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .CallRelativeDisplacement)
                == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .TargetRawFileOffset
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .NextInstructionRawFileOffset == 0x00BBDE2B
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .NextInstructionBytes) == "8B86A8000000"
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .CallRawFileOffset
                + NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .CallInstructionByteLength
                == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .NextInstructionRawFileOffset
            && unchecked(
                NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .CallCodeVirtualAddress
                + NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .CallInstructionByteLength
                + (uint)NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .CallRelativeDisplacement)
                == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .TargetCodeVirtualAddress
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection.TargetRawFileOffset
                == 0x012C2030
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection.TargetCodeVirtualAddress
                == 0x016C2C30
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection.RawToCodeVirtualAddressDelta
                == 0x00400C00
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .TargetCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .TargetRawFileOffset
                    + NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                        .RawToCodeVirtualAddressDelta
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .TargetEntryInstructionBytes) == "558BEC"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .CapturedPreparationAndCallBytes)
                == "683C4FBB016A01B98085E601E805427000"
            && !NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .ExecutesNativeInstructions);

        NetworkDispatcherEntryFallbackDiagnosticReturnPlan normal = Project();
        NetworkDispatcherLockGateFlags unknownFlags = new(
            Carry: null,
            Parity: null,
            Auxiliary: null,
            Zero: null,
            Sign: null,
            Overflow: null);
        check("Observed normal return advances to BBDE2B and invalidates prior flags/registers",
            normal.ParentPlan.ExactCleanCompletion
            && normal.ExactBoundedCompletion
            && normal.ExactCleanCompletion
            && normal.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome.BoundedAfterDirectCall
            && normal.Call is {
                InstructionRawFileOffset: 0x00BBDE26,
                InstructionCodeVirtualAddress: 0x00FBEA26,
                InstructionByteLength: 5,
                InstructionBytes: "E805427000",
                RelativeDisplacement: 0x00704205,
                TargetRawFileOffset: 0x012C2030,
                TargetCodeVirtualAddress: 0x016C2C30,
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: true,
                Completed: true
            }
            && normal.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.Captured
            && normal.CallBoundaryReached
            && normal.NextInstructionBoundaryReached
            && normal.CompletedInstructionOffsets.SequenceEqual(
                normal.ParentPlan.CompletedInstructionOffsets.Add(0x00BBDE26))
            && normal.CompletedInstructionOffsets[^1] == 0x00BBDE26
            && normal.StopAtRawFileOffsetExclusive == 0x00BBDE2B
            && normal.FlagsAtStop == unknownFlags
            && normal.FlagsInvalidatedOnNormalReturn
            && !normal.PostCallFlagsForwarded
            && !normal.PostCallRegistersForwarded
            && !normal.PostCallESPForwarded
            && !normal.PostCallEAXForwarded
            && !normal.PostCallECXForwarded
            && !normal.PostCallEBXForwarded
            && !normal.PostCallESIForwarded
            && !normal.PostCallEDIForwarded
            && !normal.PostCallEBPForwarded
            && !normal.NativeInstructionsExecuted
            && !normal.NativeCallExecuted
            && !normal.NativeMemoryReadsExecuted
            && !normal.NativeMemoryWritesExecuted
            && !normal.ParentInstructionsExecuted);

        NetworkDispatcherEntryFallbackDiagnosticReturnPlan pending = Project(
            normalReturnObserved: null);
        NetworkDispatcherEntryFallbackDiagnosticReturnPlan didNotReturn = Project(
            normalReturnObserved: false);
        check("Pending and non-normal observations preserve exact before-CALL bounds and flags",
            pending.ParentPlan.ExactCleanCompletion
            && didNotReturn.ParentPlan.ExactCleanCompletion
            && pending.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome.NormalReturnNotObserved
            && didNotReturn.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome.CallDidNotReturnNormally
            && pending.Call is {
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: null,
                Completed: false
            }
            && didNotReturn.Call is {
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: false,
                Completed: false
            }
            && pending.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.Pending
            && didNotReturn.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.Captured
            && pending.CompletedInstructionOffsets.SequenceEqual(
                pending.ParentPlan.CompletedInstructionOffsets)
            && didNotReturn.CompletedInstructionOffsets.SequenceEqual(
                didNotReturn.ParentPlan.CompletedInstructionOffsets)
            && pending.StopAtRawFileOffsetExclusive
                == pending.ParentPlan.StopAtRawFileOffsetExclusive
            && didNotReturn.StopAtRawFileOffsetExclusive
                == didNotReturn.ParentPlan.StopAtRawFileOffsetExclusive
            && pending.StopAtRawFileOffsetExclusive == 0x00BBDE26
            && didNotReturn.StopAtRawFileOffsetExclusive == 0x00BBDE26
            && pending.FlagsAtStop == pending.ParentPlan.FlagsAtStop
            && didNotReturn.FlagsAtStop == didNotReturn.ParentPlan.FlagsAtStop
            && pending.CallBoundaryReached
            && didNotReturn.CallBoundaryReached
            && !pending.NextInstructionBoundaryReached
            && !didNotReturn.NextInstructionBoundaryReached
            && !pending.FlagsInvalidatedOnNormalReturn
            && !didNotReturn.FlagsInvalidatedOnNormalReturn
            && !pending.ExactBoundedCompletion
            && !didNotReturn.ExactBoundedCompletion
            && !pending.ExactCleanCompletion
            && !didNotReturn.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackDiagnosticReturnPlan takenJg =
            Project(freshComparedDword: 2);
        NetworkDispatcherEntryFallbackDiagnosticReturnPlan missingComparedDword =
            Project(freshComparedDword: null, normalReturnObserved: true);
        check("Rejected or incomplete parent prefixes retain their original bounds without a child call",
            !takenJg.ProjectionEntered
            && !missingComparedDword.ProjectionEntered
            && takenJg.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome
                    .ParentPreparationNotAccepted
            && missingComparedDword.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome
                    .ParentPreparationNotAccepted
            && takenJg.Call is null
            && missingComparedDword.Call is null
            && takenJg.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.SuppliedUnused
            && missingComparedDword.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticReturnCaptureState.SuppliedUnused
            && takenJg.CompletedInstructionOffsets.SequenceEqual(
                takenJg.ParentPlan.CompletedInstructionOffsets)
            && missingComparedDword.CompletedInstructionOffsets.SequenceEqual(
                missingComparedDword.ParentPlan.CompletedInstructionOffsets)
            && takenJg.StopAtRawFileOffsetExclusive
                == takenJg.ParentPlan.StopAtRawFileOffsetExclusive
            && missingComparedDword.StopAtRawFileOffsetExclusive
                == missingComparedDword.ParentPlan.StopAtRawFileOffsetExclusive
            && takenJg.FlagsAtStop == takenJg.ParentPlan.FlagsAtStop
            && missingComparedDword.FlagsAtStop == missingComparedDword.ParentPlan.FlagsAtStop
            && takenJg.StopAtRawFileOffsetExclusive == 0x00BBDE2B
            && missingComparedDword.StopAtRawFileOffsetExclusive == 0x00BBDE0E
            && !takenJg.CallBoundaryReached
            && !missingComparedDword.CallBoundaryReached
            && !takenJg.ExactBoundedCompletion
            && !missingComparedDword.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackDiagnosticReturnCapture immutableCapture = Capture();
        NetworkDispatcherEntryFallbackDiagnosticReturnPlan first =
            NetworkDispatcherEntryFallbackDiagnosticReturnProjection.Project(immutableCapture);
        NetworkDispatcherEntryFallbackDiagnosticReturnPlan replay =
            NetworkDispatcherEntryFallbackDiagnosticReturnProjection.Project(immutableCapture);
        check("Fresh reprojection of an immutable capture preserves the call observation and bounds",
            first.Capture == replay.Capture
            && first.ParentPlan.Capture == replay.ParentPlan.Capture
            && first.ParentPlan.Instructions.SequenceEqual(replay.ParentPlan.Instructions)
            && first.Call == replay.Call
            && first.Outcome == replay.Outcome
            && first.CompletedInstructionOffsets.SequenceEqual(
                replay.CompletedInstructionOffsets)
            && first.StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && first.FlagsAtStop == replay.FlagsAtStop
            && first.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && first.ExactCleanCompletion
            && replay.ExactCleanCompletion);
    }

    private static NetworkDispatcherEntryFallbackZeroBranchCapture CreateZeroBranchCapture()
    {
        var callerCapture = new NetworkIoCompletionClockGateCapture(
            EntryValidated: true,
            EntryEBP: 0x2000,
            EntryFlags: null,
            FreshGlobalPointer: 0x1000,
            FreshClockField: 500,
            CachedClockTargetVirtualAddress: 0x12345678,
            ClockReturnedNormally: true,
            ClockReturnEAX: 1000,
            FreshSavedPointerRead: 0x2222,
            DispatcherReturnedNormally: null);
        var frameCapture = new NetworkDispatcherEntryFramePrefixCapture(
            callerCapture,
            0x2000,
            0x2000,
            new NetworkIoCompletionClockGateFlags(
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false),
            0xAABBCCDD);
        NetworkDispatcherEntryFramePrefixPlan framePlan =
            NetworkDispatcherEntryFramePrefixProjection.Project(frameCapture);
        var registerCapture = new NetworkDispatcherEntryRegisterSaveCapture(
            frameCapture,
            framePlan,
            FreshEntryEBX: 0x11111111,
            FreshEntryESI: 0x123,
            FreshEntryEDI: 0x33333333);
        var cookieCapture = new NetworkDispatcherEntryCookiePreparationCapture(
            registerCapture,
            FreshCookieGlobalDWORD: 0);
        var lockCapture = new NetworkDispatcherEntryLockPreparationCapture(
            new NetworkDispatcherEntryFsPublicationCapture(cookieCapture),
            FreshEntryECX: 0x123);
        var entryCallCapture = new NetworkDispatcherEntryImportReturnCapture(
            lockCapture,
            FreshOperationTargetDWORD: 0x12345678,
            NormalReturnObserved: true,
            FreshReturnedEAX: 0);
        var fallbackPushCapture = new NetworkDispatcherEntryFallbackPushCapture(
            new NetworkDispatcherEntryReturnedTestBranchCapture(entryCallCapture),
            FreshPostCallESP: 0x5000,
            FreshPostCallEBX: 0xA5A5);
        var fallbackImportCapture = new NetworkDispatcherEntryFallbackImportReturnCapture(
            fallbackPushCapture,
            FreshOperationTargetDWORD: 0x12345678,
            NormalReturnObserved: true);
        var fallbackCounterCapture = new NetworkDispatcherEntryFallbackCounterCapture(
            fallbackImportCapture,
            FreshPostFallbackEBX: 0x1000,
            FreshCounterDword: 0);
        var argumentCompareCapture = new NetworkDispatcherEntryFallbackArgumentCompareCapture(
            fallbackCounterCapture,
            FreshPostFallbackEBP: 0x2000,
            FreshArgumentByte: 0x80);
        var clearStoresCapture = new NetworkDispatcherEntryFallbackClearStoresCapture(
            argumentCompareCapture,
            FreshPostFallbackESI: 0x1000);
        return new(clearStoresCapture);
    }
}
