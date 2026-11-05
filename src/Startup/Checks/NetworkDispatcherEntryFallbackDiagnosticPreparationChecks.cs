using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackDiagnosticPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackDiagnosticPreparationCapture Capture(
            uint? freshComparedDword = 0,
            uint? freshEntryESP = 0x5008,
            uint? freshEntryECX = 0x12345678) =>
            new(
                new NetworkDispatcherEntryFallbackSelectorFallthroughCapture(
                    new NetworkDispatcherEntryFallbackSelectorBranchCapture(
                        new NetworkDispatcherEntryFallbackSelectorCompareCapture(
                            CreateZeroBranchCapture(),
                            freshComparedDword)),
                    freshEntryESP,
                    freshEntryECX));

        static NetworkDispatcherEntryFallbackDiagnosticPreparationPlan Project(
            uint? freshComparedDword = 0,
            uint? freshEntryESP = 0x5008,
            uint? freshEntryECX = 0x12345678) =>
            NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.Project(
                Capture(freshComparedDword, freshEntryESP, freshEntryECX));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("IDA byte evidence bounds the immediate preparation prefix and first CALL boundary",
            NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .BytesIndependentlyVerified
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.EntryRawFileOffset
                == 0x00BBDE1A
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.EntryCodeVirtualAddress
                == 0x00FBEA1A
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.SecondPushRawFileOffset
                == 0x00BBDE1F
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.SecondPushCodeVirtualAddress
                == 0x00FBEA1F
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.MoveEcxRawFileOffset
                == 0x00BBDE21
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.MoveEcxCodeVirtualAddress
                == 0x00FBEA21
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .FirstCallBoundaryRawFileOffset == 0x00BBDE26
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE26) == 0x00FBEA26
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .PushImmediateDwordInstructionByteLength == 5
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .PushImmediateDwordInstructionBytes) == "683C4FBB01"
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .PushImmediateValue == 0x01BB4F3C
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .PushSignExtendedImmediateByteInstructionByteLength == 2
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .PushSignExtendedImmediateByteInstructionBytes) == "6A01"
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .SecondPushImmediateValue == 1
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .MoveEcxImmediateDwordInstructionByteLength == 5
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .MoveEcxImmediateDwordInstructionBytes) == "B98085E601"
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .EcxImmediateValue == 0x01E68580
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .CallBoundaryInstructionByteLength == 5
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .CallBoundaryInstructionBytes) == "E805427000"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .CapturedPreparationAndCallBoundaryBytes)
                == "683C4FBB016A01B98085E601E805427000"
            && !NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .CallBoundaryIsIndirect
            && NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .MaximumProjectedInstructionCount == 3
            && NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00400C00
            && !NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .ExecutesNativeInstructions);

        bool prefixCasesExact = true;
        bool sawWrappedStack = false;
        bool sawNonWrappedStack = false;
        foreach ((uint entryESP, uint entryECX) in new[]
        {
            (0x00000003u, 0x12345678u),
            (0x00005008u, 0xFFFFFFFFu)
        })
        {
            NetworkDispatcherEntryFallbackDiagnosticPreparationPlan plan =
                Project(freshEntryESP: entryESP, freshEntryECX: entryECX);
            NetworkDispatcherLockGateFlags expectedFlags = plan.ParentPlan.FlagsAtStop
                ?? throw new InvalidOperationException("The accepted parent prefix has no flags.");
            uint parentFinalESP = unchecked(entryESP - 8);
            uint firstESP = unchecked(parentFinalESP - 4);
            uint secondESP = unchecked(firstESP - 4);

            prefixCasesExact &= plan.ParentFallthroughAccepted
                && plan.ParentPlan.Outcome
                    == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
                        .ProjectedTwoPushPrefix
                && plan.ParentPlan.FinalESP == parentFinalESP
                && plan.ParentPlan.Capture.FreshEntryECX == entryECX
                && plan.Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome
                        .ProjectedThreeInstructionPrefix
                && plan.ProjectionEntered
                && plan.Instructions.Length == 3
                && plan.Instructions[0] is {
                    InstructionRawFileOffset: 0x00BBDE1A,
                    InstructionCodeVirtualAddress: 0x00FBEA1A,
                    InstructionByteLength: 5,
                    InstructionBytes: "683C4FBB01",
                    Kind: NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
                        .PushImmediateDword,
                    StackAddress: not null,
                    OperandValue: 0x01BB4F3C,
                    Attempted: true,
                    Reached: true,
                    Committed: true
                }
                && plan.Instructions[0].StackPointerBefore == parentFinalESP
                && plan.Instructions[0].StackAddress == firstESP
                && plan.Instructions[0].StackPointerAfter == firstESP
                && plan.Instructions[0].FlagsBefore == expectedFlags
                && plan.Instructions[0].FlagsAfter == expectedFlags
                && plan.Instructions[1] is {
                    InstructionRawFileOffset: 0x00BBDE1F,
                    InstructionCodeVirtualAddress: 0x00FBEA1F,
                    InstructionByteLength: 2,
                    InstructionBytes: "6A01",
                    Kind: NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
                        .PushSignExtendedImmediateByte,
                    OperandValue: 1,
                    Attempted: true,
                    Reached: true,
                    Committed: true
                }
                && plan.Instructions[1].StackPointerBefore == firstESP
                && plan.Instructions[1].StackAddress == secondESP
                && plan.Instructions[1].StackPointerAfter == secondESP
                && plan.Instructions[1].FlagsBefore == expectedFlags
                && plan.Instructions[1].FlagsAfter == expectedFlags
                && plan.Instructions[2] is {
                    InstructionRawFileOffset: 0x00BBDE21,
                    InstructionCodeVirtualAddress: 0x00FBEA21,
                    InstructionByteLength: 5,
                    InstructionBytes: "B98085E601",
                    Kind: NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
                        .MoveEcxImmediateDword,
                    StackAddress: null,
                    OperandValue: 0x01E68580,
                    Attempted: true,
                    Reached: true,
                    Committed: true
                }
                && plan.Instructions[2].StackPointerBefore == secondESP
                && plan.Instructions[2].StackPointerAfter == secondESP
                && plan.Instructions[2].FlagsBefore == expectedFlags
                && plan.Instructions[2].FlagsAfter == expectedFlags
                && plan.FinalESP == secondESP
                && plan.ECXAfterMove == 0x01E68580
                && plan.CompletedInstructionOffsets.SequenceEqual(
                    plan.ParentPlan.CompletedInstructionOffsets
                        .Add(0x00BBDE1A)
                        .Add(0x00BBDE1F)
                        .Add(0x00BBDE21))
                && plan.CompletedInstructionOffsets[^1] == 0x00BBDE21
                && plan.StopAtRawFileOffsetExclusive == 0x00BBDE26
                && plan.CallBoundaryReached
                && !plan.CallExecutionAttempted
                && plan.FlagsAtStop == expectedFlags
                && plan.OrderedSemanticPrefixPreserved
                && plan.ExactBoundedCompletion
                && plan.ExactCleanCompletion
                && !plan.NativeInstructionsExecuted
                && !plan.NativeMemoryReadsExecuted
                && !plan.NativeMemoryWritesExecuted
                && !plan.ParentInstructionsExecuted;
            sawWrappedStack |= entryESP == 3
                && parentFinalESP == 0xFFFFFFFB
                && secondESP == 0xFFFFFFF3;
            sawNonWrappedStack |= entryESP == 0x5008 && secondESP == 0x00004FF8;
        }

        check("Three immediate preparation instructions preserve inherited flags and derived ESP",
            prefixCasesExact && sawWrappedStack && sawNonWrappedStack);

        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan takenJg =
            Project(freshComparedDword: 2);
        check("Taken JG is rejected while preserving the original BBDE2B endpoint",
            takenJg.ParentPlan.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgTakenToBBDE2B
            && takenJg.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome.JgTakenRejected
            && !takenJg.ParentFallthroughAccepted
            && !takenJg.ProjectionEntered
            && takenJg.Instructions.IsEmpty
            && takenJg.CompletedInstructionOffsets.SequenceEqual(
                takenJg.ParentPlan.CompletedInstructionOffsets)
            && takenJg.StopAtRawFileOffsetExclusive == 0x00BBDE2B
            && takenJg.StopAtRawFileOffsetExclusive
                == takenJg.ParentPlan.StopAtRawFileOffsetExclusive
            && takenJg.FlagsAtStop == takenJg.ParentPlan.FlagsAtStop
            && takenJg.FinalESP == takenJg.ParentPlan.FinalESP
            && takenJg.ECXAfterMove is null
            && !takenJg.CallBoundaryReached
            && !takenJg.ExactBoundedCompletion
            && !takenJg.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan missingCompare =
            Project(freshComparedDword: null);
        check("Incomplete comparison ancestry retains its CMP endpoint without entering the child",
            missingCompare.ParentPlan.ParentPlan.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackSelectorCompareOutcome.MissingFreshComparedDword
            && missingCompare.ParentPlan.ParentPlan.ParentPlan.StopAtRawFileOffsetExclusive
                == 0x00BBDE0E
            && missingCompare.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome
                    .ParentFallthroughNotAccepted
            && !missingCompare.ParentFallthroughAccepted
            && !missingCompare.ProjectionEntered
            && missingCompare.Instructions.IsEmpty
            && missingCompare.CompletedInstructionOffsets.SequenceEqual(
                missingCompare.ParentPlan.CompletedInstructionOffsets)
            && missingCompare.StopAtRawFileOffsetExclusive
                == missingCompare.ParentPlan.StopAtRawFileOffsetExclusive
            && missingCompare.FlagsAtStop == missingCompare.ParentPlan.FlagsAtStop
            && missingCompare.FinalESP == missingCompare.ParentPlan.FinalESP
            && missingCompare.ECXAfterMove is null
            && !missingCompare.CallBoundaryReached
            && !missingCompare.ExactBoundedCompletion
            && !missingCompare.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan missingESP =
            Project(freshEntryESP: null);
        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan missingECX =
            Project(freshEntryECX: null);
        check("Missing earlier PUSH inputs preserve the BBDE17 endpoint and do not enter preparation",
            missingESP.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome.MissingFreshEntryESP
            && missingECX.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome.MissingFreshEntryECX
            && missingESP.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome
                    .ParentFallthroughNotAccepted
            && missingECX.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome
                    .ParentFallthroughNotAccepted
            && missingESP.CompletedInstructionOffsets.SequenceEqual(
                missingESP.ParentPlan.CompletedInstructionOffsets)
            && missingECX.CompletedInstructionOffsets.SequenceEqual(
                missingECX.ParentPlan.CompletedInstructionOffsets)
            && missingESP.StopAtRawFileOffsetExclusive == 0x00BBDE17
            && missingECX.StopAtRawFileOffsetExclusive == 0x00BBDE17
            && missingESP.FlagsAtStop == missingESP.ParentPlan.FlagsAtStop
            && missingECX.FlagsAtStop == missingECX.ParentPlan.FlagsAtStop
            && missingESP.Instructions.IsEmpty
            && missingECX.Instructions.IsEmpty
            && !missingESP.CallBoundaryReached
            && !missingECX.CallBoundaryReached
            && !missingESP.ExactBoundedCompletion
            && !missingECX.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackDiagnosticPreparationCapture immutableCapture =
            Capture(freshEntryESP: 0);
        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan first =
            NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.Project(
                immutableCapture);
        NetworkDispatcherEntryFallbackDiagnosticPreparationPlan replay =
            NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.Project(
                immutableCapture);
        check("An immutable capture reprojects the same bounded prefix without executing the CALL",
            first.Capture == replay.Capture
            && first.ParentPlan.Capture == replay.ParentPlan.Capture
            && first.ParentPlan.Pushes.SequenceEqual(replay.ParentPlan.Pushes)
            && first.Instructions.SequenceEqual(replay.Instructions)
            && first.CompletedInstructionOffsets.SequenceEqual(replay.CompletedInstructionOffsets)
            && first.Outcome == replay.Outcome
            && first.StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && first.FinalESP == replay.FinalESP
            && first.ECXAfterMove == replay.ECXAfterMove
            && first.CallBoundaryReached
            && replay.CallBoundaryReached
            && !first.CallExecutionAttempted
            && !replay.CallExecutionAttempted
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
