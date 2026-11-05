using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackDiagnosticZeroBranchChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("Coordinator byte anchor covers JZ, target LEA, opaque CALL, and return continuation",
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .BytesIndependentlyVerified
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .BranchRawFileOffset == 0x00BBDE3E
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .FallThroughRawFileOffset == 0x00BBDE40
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .BranchTargetRawFileOffset == 0x00BBDE83
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .LeaEBXRawFileOffset == 0x00BBDE83
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .CallBoundaryRawFileOffset == 0x00BBDE86
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .NormalReturnRawFileOffset == 0x00BBDE8C
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .CallBoundaryRawFileOffset
                + NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .CallBoundaryInstructionByteLength
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .NormalReturnInstructionBoundaryRawFileOffset
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .NormalReturnInstructionBoundaryRawFileOffset
                == 0x00BBDE8C
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .BranchInstructionBytes) == "7443"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .CoordinatorProvidedBytes) == "8D5E6CFF15244DAC018945F0"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .LeaEBXInstructionBytes) == "8D5E6C"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .CallBoundaryInstructionBytes) == "FF15244DAC01"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .NormalReturnInstructionBytes) == "8945F0"
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .CoordinatorProvidedBytes.Length / 2 == 12
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE3E) == 0x00FBEA3E
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE83) == 0x00FBEA83
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE86) == 0x00FBEA86
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE8C) == 0x00FBEA8C
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE3E)
                - 0x00BBDE3E
                == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .RawToCodeVirtualAddressDelta
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .OpaqueCallTargetSlotVirtualAddress == 0x01AC4D24
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .OpaqueCallTargetSlotVirtualAddress
                == NetworkIoCompletionClockGateProjection.ClockIatVirtualAddress
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .OpaqueCallTargetSlotRawFileOffset == 0x016C3924
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .OpaqueCallTargetSlotRawFileOffset
                == NetworkIoCompletionClockGateProjection.ClockIatRawFileOffset
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .OpaqueCallTargetSlotImportName == "timeGetTime"
            && !NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .OpaqueCallTargetAddressResolved
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .CalculateRelativeTargetCodeVirtualAddress(
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .BranchCodeVirtualAddress,
                    NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                        .SignedRelativeDisplacement)
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                    .BranchTargetCodeVirtualAddress
            && NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection
                .OpaqueCellPointerVirtualAddress == 0x01AC4D24);

        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan taken =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                CreateCapture(
                    dwordAtA8: 0x12345678,
                    dwordAtA4: 0x12345678,
                    opaqueCallReturnedNormally: true));
        check("Zero TEST path models the normal opaque-call return boundary",
            taken.ParentPlan.ExactCleanCompletion
            && taken.ParentPlan.EAX == 0
            && taken.ParentPlan.FlagsAtStop is { Zero: true }
            && taken.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                    .CompletedAtNormalReturnBoundaryBBDE8C
            && taken.BranchConditionCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && taken.TargetESICaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && taken.Jz is { Taken: true, Attempted: true, Reached: true, Committed: true }
            && taken.Jz.Value.TestedEAX == 0
            && taken.Jz.Value.SelectedDestinationRawFileOffset == 0x00BBDE83
            && taken.AddressCalculation is { Reached: true, Committed: true }
            && taken.AddressCalculation.Value.ESIValue == 0x5000
            && taken.AddressCalculation.Value.EBXBefore == taken.ParentPlan.EBX
            && taken.AddressCalculation.Value.EBXAfter == 0x506C
            && taken.AddressCalculation.Value.FlagsBefore == taken.ParentPlan.FlagsAtStop
            && taken.AddressCalculation.Value.FlagsAfter == taken.ParentPlan.FlagsAtStop
            && !taken.AddressCalculation.Value.FlagsWritten
            && !taken.AddressCalculation.Value.NativeMemoryReadExecuted
            && !taken.AddressCalculation.Value.NativeMemoryWriteExecuted
            && taken.CallBoundary is {
                Reached: true,
                CallExecuted: false,
                PointerReadExecuted: false,
                CalleeContinuationIdentified: false
            }
            && taken.CallBoundary.Value.IndirectPointerAddress == 0x01AC4D24
            && taken.EBXAtCallBoundary == 0x506C
            && taken.OpaqueCallAttempted
            && taken.OpaqueCallCompleted
            && taken.OpaqueCallReturnedNormally == true
            && taken.NormalReturnBoundaryReached
            && taken.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && taken.OpaqueCallReturn is {
                InstructionRawFileOffset: 0x00BBDE86,
                InstructionCodeVirtualAddress: 0x00FBEA86,
                InstructionByteLength: 6,
                InstructionBytes: "FF15244DAC01",
                IndirectPointerAddress: 0x01AC4D24,
                NextInstructionRawFileOffset: 0x00BBDE8C,
                NextInstructionCodeVirtualAddress: 0x00FBEA8C,
                NextInstructionByteLength: 3,
                NextInstructionBytes: "8945F0",
                BoundaryReached: true,
                Attempted: true,
                ReturnedNormally: true,
                Completed: true,
                NormalReturnBoundaryReached: true,
                PointerReadExecuted: false,
                TargetExecuted: false,
                NextInstructionExecuted: false
            }
            && taken.OpaqueCallReturn.Value.FlagsBefore == taken.ParentPlan.FlagsAtStop
            && taken.OpaqueCallReturn.Value.FlagsAfter is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && taken.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && taken.PreCallESI == 0x5000
            && taken.PreCallEAX == 0
            && taken.EBXBeforeCall == 0x506C
            && taken.ESI is null
            && taken.EAX is null
            && taken.EBX is null
            && taken.StopAtRawFileOffsetExclusive == 0x00BBDE8C
            && taken.LastReachedRawFileOffset == 0x00BBDE86
            && taken.CompletedInstructionOffsets.SequenceEqual(
                taken.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE3E)
                    .Add(0x00BBDE83)
                    .Add(0x00BBDE86))
            && taken.BranchConditionFlagsPreserved
            && !taken.ConditionFlagsPreserved
            && !taken.PostCallFlagsForwarded
            && !taken.PostCallRegistersForwarded
            && taken.PostCallEAX is null
            && taken.PostCallEBX is null
            && taken.ExactBoundedCompletion
            && taken.ExactCleanCompletion
            && !taken.BranchExecuted
            && !taken.TargetInstructionExecuted
            && !taken.CallExecuted
            && !taken.OpaqueCellReadExecuted
            && !taken.CalleeContinuationIdentified
            && !taken.NativeInstructionsExecuted);

        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan notTaken =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                CreateCapture());
        check("Nonzero TEST result selects JZ fall-through without target effects",
            notTaken.ParentPlan.ExactCleanCompletion
            && notTaken.ParentPlan.EAX == 1
            && notTaken.ParentPlan.FlagsAtStop is { Zero: false }
            && notTaken.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                    .JzNotTakenToBBDE40
            && notTaken.BranchConditionCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && notTaken.TargetESICaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.SuppliedUnused
            && notTaken.Jz is { Taken: false, Attempted: true, Reached: true, Committed: true }
            && notTaken.Jz.Value.SelectedDestinationRawFileOffset == 0x00BBDE40
            && notTaken.AddressCalculation is null
            && notTaken.CallBoundary is null
            && notTaken.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached
            && notTaken.StopAtRawFileOffsetExclusive == 0x00BBDE40
            && notTaken.LastReachedRawFileOffset == 0x00BBDE3E
            && notTaken.CompletedInstructionOffsets.SequenceEqual(
                notTaken.ParentPlan.CompletedInstructionOffsets.Add(0x00BBDE3E))
            && notTaken.ConditionFlagsPreserved
            && notTaken.ExactBoundedCompletion
            && !notTaken.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan callCaptureUnused =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                CreateCapture(opaqueCallReturnedNormally: true));
        check("Normal-return capture is supplied but unused when the parent JZ is not taken",
            callCaptureUnused.ParentPlan.ExactCleanCompletion
            && callCaptureUnused.Jz is { Taken: false }
            && callCaptureUnused.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.SuppliedUnused
            && callCaptureUnused.OpaqueCallReturn is null
            && callCaptureUnused.StopAtRawFileOffsetExclusive == 0x00BBDE40
            && !callCaptureUnused.OpaqueCallAttempted);

        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan missingCallReturn =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                CreateCapture(dwordAtA8: 0x12345678, dwordAtA4: 0x12345678));
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan nonreturningCall =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                CreateCapture(
                    dwordAtA8: 0x12345678,
                    dwordAtA4: 0x12345678,
                    opaqueCallReturnedNormally: false));
        check("Missing and negative normal-return observations stop at the CALL boundary",
            missingCallReturn.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                    .MissingOpaqueCallReturnStatus
            && missingCallReturn.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing
            && missingCallReturn.OpaqueCallReturn is {
                BoundaryReached: true,
                Attempted: true,
                ReturnedNormally: null,
                Completed: false,
                NormalReturnBoundaryReached: false
            }
            && missingCallReturn.CompletedInstructionOffsets.SequenceEqual(
                missingCallReturn.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE3E)
                    .Add(0x00BBDE83))
            && missingCallReturn.StopAtRawFileOffsetExclusive == 0x00BBDE86
            && missingCallReturn.LastReachedRawFileOffset == 0x00BBDE83
            && missingCallReturn.FlagsAtStop == missingCallReturn.ParentPlan.FlagsAtStop
            && !missingCallReturn.ExactBoundedCompletion
            && missingCallReturn.OrderedSemanticPrefixPreserved
            && nonreturningCall.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome
                    .OpaqueCallDidNotReturnNormally
            && nonreturningCall.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Captured
            && nonreturningCall.OpaqueCallReturnedNormally == false
            && !nonreturningCall.OpaqueCallCompleted
            && !nonreturningCall.NormalReturnBoundaryReached
            && nonreturningCall.StopAtRawFileOffsetExclusive == 0x00BBDE86);

        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan missingESI =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                CreateCapture(freshPostCallESI: null));
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan missingA8 =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                CreateCapture(dwordAtA8: null));
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan missingA4 =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                CreateCapture(dwordAtA4: null));
        check("Missing parent captures stop at inherited prefix endpoints",
            IsPreservedMissingParentStop(missingESI)
                && missingESI.PostCallESICaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing
                && missingA8.DWORDAtA8CaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing
                && IsPreservedMissingParentStop(missingA8)
                && missingA4.DWORDAtA4CaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Missing
                && IsPreservedMissingParentStop(missingA4));

        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan rejectedParent =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                CreateCapture(
                    parentCallReturnedNormally: false,
                    opaqueCallReturnedNormally: true));
        check("Rejected parent mismatch preserves its endpoint and leaves call capture unused",
            !rejectedParent.ParentPlan.ExactCleanCompletion
            && !rejectedParent.ParentContinuationReached
            && rejectedParent.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome.ParentPrefixNotAccepted
            && rejectedParent.Jz is null
            && rejectedParent.AddressCalculation is null
            && rejectedParent.CallBoundary is null
            && rejectedParent.CompletedInstructionOffsets.SequenceEqual(
                rejectedParent.ParentPlan.CompletedInstructionOffsets)
            && rejectedParent.StopAtRawFileOffsetExclusive
                == rejectedParent.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedParent.LastReachedRawFileOffset
                == (rejectedParent.ParentPlan.CompletedInstructionOffsets.IsEmpty
                    ? null
                    : rejectedParent.ParentPlan.CompletedInstructionOffsets[^1])
            && rejectedParent.FlagsAtStop == rejectedParent.ParentPlan.FlagsAtStop
            && rejectedParent.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.SuppliedUnused
            && rejectedParent.OpaqueCallReturn is null
            && rejectedParent.ExactBoundedCompletion
                == rejectedParent.ParentPlan.ExactBoundedCompletion);

        NetworkDispatcherEntryFallbackDiagnosticZeroBranchCapture immutableCapture =
            CreateCapture(
                dwordAtA8: 0x12345678,
                dwordAtA4: 0x12345678,
                opaqueCallReturnedNormally: true);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan first =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                immutableCapture);
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan replay =
            NetworkDispatcherEntryFallbackDiagnosticZeroBranchProjection.Project(
                immutableCapture);
        check("Fresh reprojection reproduces parent and taken-path evidence",
            first.Capture == replay.Capture
            && first.ParentPlan.Capture == replay.ParentPlan.Capture
            && first.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                replay.ParentPlan.CompletedInstructionOffsets)
            && first.ParentPlan.StopAtRawFileOffsetExclusive
                == replay.ParentPlan.StopAtRawFileOffsetExclusive
            && first.Jz == replay.Jz
            && first.AddressCalculation == replay.AddressCalculation
            && first.CallBoundary == replay.CallBoundary
            && first.OpaqueCallReturn == replay.OpaqueCallReturn
            && first.NormalReturnCaptureState == replay.NormalReturnCaptureState
            && first.CompletedInstructionOffsets.SequenceEqual(
                replay.CompletedInstructionOffsets)
            && first.StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && first.LastReachedRawFileOffset == replay.LastReachedRawFileOffset
            && first.FlagsAtStop == replay.FlagsAtStop
            && first.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && first.ExactCleanCompletion
            && replay.ExactCleanCompletion);
    }

    private static bool IsPreservedMissingParentStop(
        NetworkDispatcherEntryFallbackDiagnosticZeroBranchPlan plan) =>
        !plan.ParentContinuationReached
        && plan.Outcome
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchOutcome.ParentPrefixNotAccepted
        && plan.Jz is null
        && plan.AddressCalculation is null
        && plan.CallBoundary is null
        && plan.OpaqueCallReturn is null
        && plan.NormalReturnCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticZeroBranchCaptureState.Unreached
        && plan.CompletedInstructionOffsets.SequenceEqual(
            plan.ParentPlan.CompletedInstructionOffsets)
        && plan.StopAtRawFileOffsetExclusive
            == plan.ParentPlan.StopAtRawFileOffsetExclusive
        && plan.LastReachedRawFileOffset
            == (plan.ParentPlan.CompletedInstructionOffsets.IsEmpty
                ? null
                : plan.ParentPlan.CompletedInstructionOffsets[^1])
        && plan.ExactBoundedCompletion == plan.ParentPlan.ExactBoundedCompletion;

    private static NetworkDispatcherEntryFallbackDiagnosticZeroBranchCapture CreateCapture(
        uint? freshPostCallESI = 0x5000,
        uint? dwordAtA8 = 0x80000003,
        uint? dwordAtA4 = 0x7FFFFFFF,
        bool? parentCallReturnedNormally = true,
        uint? comparedDword = 0,
        uint? entryESP = 0x5008,
        uint? entryECX = 0x12345678,
        bool? opaqueCallReturnedNormally = null) =>
        new(
            new NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture(
                CreateReturnCapture(
                    parentCallReturnedNormally,
                    comparedDword,
                    entryESP,
                    entryECX),
                freshPostCallESI,
                dwordAtA8,
                dwordAtA4),
            opaqueCallReturnedNormally);

    private static NetworkDispatcherEntryFallbackDiagnosticReturnCapture CreateReturnCapture(
        bool? normalReturnObserved,
        uint? comparedDword,
        uint? entryESP,
        uint? entryECX) =>
        new(
            CreatePreparationCapture(comparedDword, entryESP, entryECX),
            normalReturnObserved);

    private static NetworkDispatcherEntryFallbackDiagnosticPreparationCapture
        CreatePreparationCapture(
            uint? comparedDword,
            uint? entryESP,
            uint? entryECX)
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
        return new(
            new NetworkDispatcherEntryFallbackSelectorFallthroughCapture(
                new NetworkDispatcherEntryFallbackSelectorBranchCapture(
                    new NetworkDispatcherEntryFallbackSelectorCompareCapture(
                        new NetworkDispatcherEntryFallbackZeroBranchCapture(
                            clearStoresCapture),
                        comparedDword)),
                entryESP,
                entryECX));
    }
}
