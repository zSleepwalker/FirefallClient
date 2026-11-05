using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture Capture(
            uint? freshPostCallESI = 0x5000,
            uint? dwordAtA8 = 0x80000003,
            uint? dwordAtA4 = 0x7FFFFFFF,
            bool? parentCallReturnedNormally = true,
            uint? comparedDword = 0,
            uint? entryESP = 0x5008,
            uint? entryECX = 0x12345678) =>
            new(
                CreateReturnCapture(
                    parentCallReturnedNormally,
                    comparedDword,
                    entryESP,
                    entryECX),
                freshPostCallESI,
                dwordAtA8,
                dwordAtA4);

        static NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan Project(
            uint? freshPostCallESI = 0x5000,
            uint? dwordAtA8 = 0x80000003,
            uint? dwordAtA4 = 0x7FFFFFFF,
            bool? parentCallReturnedNormally = true,
            uint? comparedDword = 0,
            uint? entryESP = 0x5008,
            uint? entryECX = 0x12345678) =>
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.Project(
                Capture(
                    freshPostCallESI,
                    dwordAtA8,
                    dwordAtA4,
                    parentCallReturnedNormally,
                    comparedDword,
                    entryESP,
                    entryECX));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("Coordinator byte anchor covers the five-instruction prefix and its JZ boundary",
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BytesIndependentlyVerified
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .EntryRawFileOffset == 0x00BBDE2B
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .MoveEaxRawFileOffset == 0x00BBDE2B
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .XorEbxRawFileOffset == 0x00BBDE31
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .SubEaxRawFileOffset == 0x00BBDE33
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .SarEaxRawFileOffset == 0x00BBDE39
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .TestEaxRawFileOffset == 0x00BBDE3C
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BranchRawFileOffset == 0x00BBDE3E
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BranchNextRawFileOffset == 0x00BBDE40
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BranchRawFileOffset
                + NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .BranchInstructionByteLength
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .BranchNextRawFileOffset
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BranchTargetRawFileOffset == 0x00BBDE83
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .CoordinatorProvidedByteLength == 21
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .CoordinatorProvidedBytes)
                == "8B86A800000033DB2B86A4000000C1F80285C07443"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .CoordinatorProvidedBytes).Length / 2
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .CoordinatorProvidedByteLength
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .MoveEaxInstructionBytes) == "8B86A8000000"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .XorEbxInstructionBytes) == "33DB"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .SubEaxInstructionBytes) == "2B86A4000000"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .SarEaxInstructionBytes) == "C1F802"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .TestEaxInstructionBytes) == "85C0"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BranchInstructionBytes) == "7443"
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE2B) == 0x00FBEA2B
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE3E) == 0x00FBEA3E
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE3E)
                - NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .RawFileOffsetToCodeVirtualAddress(0x00BBDE2B)
                == 0x13
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BranchRawFileOffset
                - NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .EntryRawFileOffset
                == 0x13
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE2B)
                - 0x00BBDE2B
                == NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                    .RawToCodeVirtualAddressDelta
            && NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .NextInstructionRawFileOffset
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .EntryRawFileOffset
            && unchecked(
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .BranchNextRawFileOffset
                + 0x43)
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .BranchTargetRawFileOffset
            && !NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .ExecutesNativeInstructions);

        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan success = Project();
        check("Fresh captures project MOV, XOR, SUB, SAR, and TEST through the exact JZ boundary",
            success.ParentPlan.ExactCleanCompletion
            && success.ExactBoundedCompletion
            && success.ExactCleanCompletion
            && success.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .BoundedBeforeJz
            && success.ProjectionEntered
            && success.ESI == 0x5000
            && success.EAX == 1
            && success.EBX == 0
            && success.Reads.Length == 2
            && success.Reads[0] is {
                InstructionRawFileOffset: 0x00BBDE2B,
                Address: 0x50A8,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0x80000003
            }
            && success.Reads[1] is {
                InstructionRawFileOffset: 0x00BBDE33,
                Address: 0x50A4,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0x7FFFFFFF
            }
            && success.Instructions.Length == 5
            && success.Instructions[0] is {
                InstructionRawFileOffset: 0x00BBDE2B,
                InstructionCodeVirtualAddress: 0x00FBEA2B,
                InstructionByteLength: 6,
                InstructionBytes: "8B86A8000000",
                Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .MoveEaxFromEsiPlusA8,
                EAXBefore: null,
                EAXAfter: 0x80000003,
                Attempted: true,
                Reached: true,
                Committed: true
            }
            && success.Instructions[0].FlagsBefore == success.ParentPlan.FlagsAtStop
            && success.Instructions[0].FlagsAfter == success.ParentPlan.FlagsAtStop
            && success.Instructions[1] is {
                InstructionRawFileOffset: 0x00BBDE31,
                InstructionByteLength: 2,
                InstructionBytes: "33DB",
                Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .XorEbxEbx,
                EAXBefore: 0x80000003,
                EAXAfter: 0x80000003,
                EBXBefore: null,
                EBXAfter: 0,
                Committed: true
            }
            && success.Instructions[1].FlagsAfter is {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && success.Instructions[2] is {
                InstructionRawFileOffset: 0x00BBDE33,
                InstructionByteLength: 6,
                InstructionBytes: "2B86A4000000",
                Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .SubEaxFromEsiPlusA4,
                EAXBefore: 0x80000003,
                EAXAfter: 4,
                EBXBefore: 0,
                EBXAfter: 0,
                Committed: true
            }
            && success.Instructions[2].FlagsAfter is {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && success.Instructions[3] is {
                InstructionRawFileOffset: 0x00BBDE39,
                InstructionByteLength: 3,
                InstructionBytes: "C1F802",
                Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .SarEaxByTwo,
                EAXBefore: 4,
                EAXAfter: 1,
                Committed: true
            }
            && success.Instructions[3].FlagsAfter is {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && success.Instructions[4] is {
                InstructionRawFileOffset: 0x00BBDE3C,
                InstructionByteLength: 2,
                InstructionBytes: "85C0",
                Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .TestEaxEax,
                EAXBefore: 1,
                EAXAfter: 1,
                EBXBefore: 0,
                EBXAfter: 0,
                Committed: true
            }
            && success.FlagsAtStop is {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && success.CompletedInstructionOffsets.SequenceEqual(
                success.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE2B)
                    .Add(0x00BBDE31)
                    .Add(0x00BBDE33)
                    .Add(0x00BBDE39)
                    .Add(0x00BBDE3C))
            && success.CompletedInstructionOffsets[^1] == 0x00BBDE3C
            && success.StopAtRawFileOffsetExclusive == 0x00BBDE3E
            && success.BranchBoundaryReached
            && !success.ConditionalBranchProjected
            && !success.NativeInstructionsExecuted
            && !success.NativeMemoryReadsExecuted
            && !success.NativeMemoryWritesExecuted
            && !success.ParentInstructionsExecuted);

        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan zeroTest = Project(
            dwordAtA8: 0x100,
            dwordAtA4: 0x100);
        check("TEST zero state is retained at the boundary without projecting the JZ",
            zeroTest.ExactCleanCompletion
            && zeroTest.EAX == 0
            && zeroTest.FlagsAtStop is { Zero: true }
            && zeroTest.StopAtRawFileOffsetExclusive == 0x00BBDE3E
            && zeroTest.BranchBoundaryReached
            && !zeroTest.ConditionalBranchProjected);

        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan missingESI =
            Project(freshPostCallESI: null);
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan missingA8 =
            Project(dwordAtA8: null);
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan missingA4 =
            Project(dwordAtA4: null);
        check("Missing captures stop at their first dependent instruction with later inputs unused",
            missingESI.ParentPlan.ExactCleanCompletion
            && missingA8.ParentPlan.ExactCleanCompletion
            && missingA4.ParentPlan.ExactCleanCompletion
            && missingESI.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .MissingFreshPostCallESI
            && missingA8.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .MissingFreshDWORDAtESIPlusA8
            && missingA4.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .MissingFreshDWORDAtESIPlusA4
            && missingESI.EsiCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Missing
            && missingESI.DWORDAtA8CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                    .SuppliedUnused
            && missingA8.DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                    .SuppliedUnused
            && missingA4.DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Missing
            && missingESI.Reads.IsEmpty
            && missingESI.Instructions is [{ Committed: false }]
            && missingA8.Reads is [{ Address: 0x50A8, Captured: false, Value: null }]
            && missingA8.CompletedInstructionOffsets.SequenceEqual(
                missingA8.ParentPlan.CompletedInstructionOffsets)
            && missingA4.Reads is [
                { Address: 0x50A8, Captured: true, Value: 0x80000003 },
                { Address: 0x50A4, Captured: false, Value: null }
            ]
            && missingA4.Instructions is [
                { InstructionRawFileOffset: 0x00BBDE2B, Committed: true },
                { InstructionRawFileOffset: 0x00BBDE31, Committed: true },
                { InstructionRawFileOffset: 0x00BBDE33, Committed: false }
            ]
            && missingA4.CompletedInstructionOffsets.SequenceEqual(
                missingA4.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE2B)
                    .Add(0x00BBDE31))
            && missingA4.FlagsAtStop == missingA4.Instructions[^1].FlagsAfter
            && missingESI.StopAtRawFileOffsetExclusive == 0x00BBDE2B
            && missingA8.StopAtRawFileOffsetExclusive == 0x00BBDE2B
            && missingA4.StopAtRawFileOffsetExclusive == 0x00BBDE33
            && !missingESI.ExactCleanCompletion
            && !missingA8.ExactCleanCompletion
            && !missingA4.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan pendingParent =
            Project(parentCallReturnedNormally: null);
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan rejectedParent =
            Project(parentCallReturnedNormally: false);
        check("Pending and rejected call-return parents preserve their bounds and unused child captures",
            pendingParent.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome.NormalReturnNotObserved
            && rejectedParent.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome.CallDidNotReturnNormally
            && pendingParent.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .ParentCallReturnNotAccepted
            && rejectedParent.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .ParentCallReturnNotAccepted
            && !pendingParent.ProjectionEntered
            && !rejectedParent.ProjectionEntered
            && pendingParent.Instructions.IsEmpty
            && rejectedParent.Instructions.IsEmpty
            && pendingParent.Reads.IsEmpty
            && rejectedParent.Reads.IsEmpty
            && pendingParent.CompletedInstructionOffsets.SequenceEqual(
                pendingParent.ParentPlan.CompletedInstructionOffsets)
            && rejectedParent.CompletedInstructionOffsets.SequenceEqual(
                rejectedParent.ParentPlan.CompletedInstructionOffsets)
            && pendingParent.StopAtRawFileOffsetExclusive
                == pendingParent.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedParent.StopAtRawFileOffsetExclusive
                == rejectedParent.ParentPlan.StopAtRawFileOffsetExclusive
            && pendingParent.FlagsAtStop == pendingParent.ParentPlan.FlagsAtStop
            && rejectedParent.FlagsAtStop == rejectedParent.ParentPlan.FlagsAtStop
            && pendingParent.StopAtRawFileOffsetExclusive == 0x00BBDE26
            && rejectedParent.StopAtRawFileOffsetExclusive == 0x00BBDE26
            && pendingParent.EsiCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                    .SuppliedUnused
            && pendingParent.DWORDAtA8CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                    .SuppliedUnused
            && pendingParent.DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                    .SuppliedUnused);

        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture immutableCapture =
            Capture();
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan first =
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.Project(
                immutableCapture);
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan replay =
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.Project(
                immutableCapture);
        check("Fresh projection of an immutable capture reproduces parent and suffix evidence",
            first.Capture == replay.Capture
            && first.ParentPlan.Capture == replay.ParentPlan.Capture
            && first.ParentPlan.Call == replay.ParentPlan.Call
            && first.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                replay.ParentPlan.CompletedInstructionOffsets)
            && first.ParentPlan.StopAtRawFileOffsetExclusive
                == replay.ParentPlan.StopAtRawFileOffsetExclusive
            && first.Reads.SequenceEqual(replay.Reads)
            && first.Instructions.SequenceEqual(replay.Instructions)
            && first.CompletedInstructionOffsets.SequenceEqual(
                replay.CompletedInstructionOffsets)
            && first.StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && first.FlagsAtStop == replay.FlagsAtStop
            && first.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && first.ExactCleanCompletion
            && replay.ExactCleanCompletion);
    }

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
