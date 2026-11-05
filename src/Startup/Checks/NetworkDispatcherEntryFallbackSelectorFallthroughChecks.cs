using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackSelectorFallthroughChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackSelectorFallthroughCapture Capture(
            uint? freshComparedDword = 0,
            uint? freshEntryESP = 0x5000,
            uint? freshEntryECX = 0x12345678,
            byte? fallbackArgument = 0x80,
            uint? freshCounterDword = 0,
            uint? freshPostFallbackESI = 0x1000) =>
            new(
                new NetworkDispatcherEntryFallbackSelectorBranchCapture(
                    new NetworkDispatcherEntryFallbackSelectorCompareCapture(
                        CreateZeroBranchCapture(
                            fallbackArgument,
                            freshCounterDword,
                            freshPostFallbackESI),
                        freshComparedDword)),
                freshEntryESP,
                freshEntryECX);

        static NetworkDispatcherEntryFallbackSelectorFallthroughPlan Project(
            uint? freshComparedDword = 0,
            uint? freshEntryESP = 0x5000,
            uint? freshEntryECX = 0x12345678,
            byte? fallbackArgument = 0x80,
            uint? freshCounterDword = 0,
            uint? freshPostFallbackESI = 0x1000) =>
            NetworkDispatcherEntryFallbackSelectorFallthroughProjection.Project(Capture(
                freshComparedDword,
                freshEntryESP,
                freshEntryECX,
                fallbackArgument,
                freshCounterDword,
                freshPostFallbackESI));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("IDA confirms the BBDE17 fallthrough bytes and exact two-instruction scalar prefix",
            NetworkDispatcherEntryFallbackSelectorFallthroughProjection.BytesIndependentlyVerified
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection.EntryRawFileOffset
                == 0x00BBDE17
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection.EntryCodeVirtualAddress
                == 0x00FBEA17
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection.PushECXInstructionByteLength
                == 1
            && Normalize(NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .PushECXInstructionBytes) == "51"
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection.SecondPushRawFileOffset
                == 0x00BBDE18
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection.SecondPushCodeVirtualAddress
                == 0x00FBEA18
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                .SecondPushInstructionByteLength == 2
            && Normalize(NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .SecondPushInstructionBytes) == "6A00"
            && Normalize(NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .CapturedWindowBytes) == "516A00683C4FBB01"
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                .BoundaryRawFileOffsetExclusive == 0x00BBDE1A
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                .SecondPushRawFileOffset
                + NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .SecondPushInstructionByteLength
                == NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .BoundaryRawFileOffsetExclusive
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .RawFileOffsetToCodeVirtualAddress(
                        NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                            .EntryRawFileOffset)
                == NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                    .EntryCodeVirtualAddress
            && NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00400C00
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection.PushDwordWidth == 4
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection.StackPointerDecrement == 4
            && NetworkDispatcherEntryFallbackSelectorFallthroughProjection.MaximumProjectedPushCount
                == 2
            && !NetworkDispatcherEntryFallbackSelectorFallthroughProjection
                .ExecutesNativeInstructions);

        bool prefixCasesExact = true;
        bool sawZeroResult = false;
        bool sawNonzeroResult = false;
        foreach ((uint comparedDword, uint entryESP, uint entryECX) in new[]
        {
            (0u, 0x00000003u, 0x12345678u),
            (1u, 0x00000004u, 0xFFFFFFFFu)
        })
        {
            NetworkDispatcherEntryFallbackSelectorFallthroughPlan plan =
                Project(comparedDword, entryESP, entryECX);
            uint firstESP = unchecked(entryESP - 4);
            uint secondESP = unchecked(entryESP - 8);
            NetworkDispatcherLockGateFlags expectedFlags = plan.ParentPlan.FlagsAtStop
                ?? throw new InvalidOperationException("The accepted JG has no flags.");
            prefixCasesExact &= plan.ParentJgNotTakenAccepted
                && plan.ParentPlan.Outcome
                    == NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgNotTakenToBBDE17
                && plan.ParentPlan.SelectedDestinationRawFileOffset == 0x00BBDE17
                && plan.ParentPlan.StopAtRawFileOffsetExclusive == 0x00BBDE17
                && plan.ParentPlan.FlagsAtStop == plan.ParentPlan.CompareFlags
                && plan.Outcome
                    == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
                        .ProjectedTwoPushPrefix
                && plan.FreshEntryESPCaptureState
                    == NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.Captured
                && plan.FreshEntryECXCaptureState
                    == NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.Captured
                && plan.RegisterReads.Length == 2
                && plan.RegisterReads[0] is {
                    InstructionRawFileOffset: 0x00BBDE17,
                    Register: NetworkDispatcherEntryFallbackSelectorFallthroughRegister.ESP,
                    Width: 4,
                    Attempted: true,
                    Captured: true
                }
                && plan.RegisterReads[0].Value == entryESP
                && plan.RegisterReads[1] is {
                    InstructionRawFileOffset: 0x00BBDE17,
                    Register: NetworkDispatcherEntryFallbackSelectorFallthroughRegister.ECX,
                    Width: 4,
                    Attempted: true,
                    Captured: true
                }
                && plan.RegisterReads[1].Value == entryECX
                && plan.Pushes.Length == 2
                && plan.Pushes[0] is {
                    InstructionRawFileOffset: 0x00BBDE17,
                    InstructionCodeVirtualAddress: 0x00FBEA17,
                    InstructionByteLength: 1,
                    InstructionBytes: "51",
                    Width: 4,
                    Attempted: true,
                    Reached: true,
                    Committed: true
                }
                && plan.Pushes[0].StackPointerBefore == entryESP
                && plan.Pushes[0].StackAddress == firstESP
                && plan.Pushes[0].StackPointerAfter == firstESP
                && plan.Pushes[0].Value == entryECX
                && plan.Pushes[0].FlagsBefore == expectedFlags
                && plan.Pushes[0].FlagsAfter == expectedFlags
                && plan.Pushes[1] is {
                    InstructionRawFileOffset: 0x00BBDE18,
                    InstructionCodeVirtualAddress: 0x00FBEA18,
                    InstructionByteLength: 2,
                    InstructionBytes: "6A00",
                    Width: 4,
                    Attempted: true,
                    Reached: true,
                    Committed: true
                }
                && plan.Pushes[1].StackPointerBefore == firstESP
                && plan.Pushes[1].StackAddress == secondESP
                && plan.Pushes[1].StackPointerAfter == secondESP
                && plan.Pushes[1].Value == 0
                && plan.Pushes[1].FlagsBefore == expectedFlags
                && plan.Pushes[1].FlagsAfter == expectedFlags
                && plan.FinalESP == secondESP
                && plan.FlagsAtStop == expectedFlags
                && plan.FlagsPreserved
                && plan.CompletedInstructionOffsets.SequenceEqual(
                    plan.ParentPlan.CompletedInstructionOffsets
                        .Add(0x00BBDE17)
                        .Add(0x00BBDE18))
                && plan.CompletedInstructionOffsets[^1] == 0x00BBDE18
                && plan.StopAtRawFileOffsetExclusive == 0x00BBDE1A
                && plan.ExactBoundedCompletion
                && plan.ExactCleanCompletion
                && plan.OrderedSemanticPrefixPreserved
                && !plan.NativeInstructionsExecuted
                && !plan.NativeMemoryReadsExecuted
                && !plan.NativeMemoryWritesExecuted
                && !plan.ParentInstructionsExecuted;
            sawZeroResult |= plan.ParentPlan.CompareFlags is { Zero: true };
            sawNonzeroResult |= plan.ParentPlan.CompareFlags is { Zero: false };
        }

        check("Two PUSHes use independent fresh ESP and ECX reads, DWORD wraparound, and preserve flags",
            prefixCasesExact && sawZeroResult && sawNonzeroResult);

        NetworkDispatcherEntryFallbackSelectorFallthroughPlan missingECX =
            Project(freshEntryECX: null);
        NetworkDispatcherEntryFallbackSelectorFallthroughPlan missingESP =
            Project(freshEntryESP: null);
        NetworkDispatcherEntryFallbackSelectorFallthroughPlan missingBoth =
            Project(freshEntryESP: null, freshEntryECX: null);
        check("Missing first-PUSH inputs remain independent attempted reads and stop before BBDE17",
            missingECX.Outcome
                == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome.MissingFreshEntryECX
            && missingECX.RegisterReads.Length == 2
            && missingECX.RegisterReads[0].Captured
            && !missingECX.RegisterReads[1].Captured
            && missingECX.RegisterReads.All(read => read.Attempted)
            && missingECX.Pushes.IsEmpty
            && missingECX.CompletedInstructionOffsets.SequenceEqual(
                missingECX.ParentPlan.CompletedInstructionOffsets)
            && missingECX.StopAtRawFileOffsetExclusive == 0x00BBDE17
            && missingECX.FlagsAtStop == missingECX.ParentPlan.FlagsAtStop
            && missingESP.Outcome
                == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome.MissingFreshEntryESP
            && !missingESP.RegisterReads[0].Captured
            && missingESP.RegisterReads[1].Captured
            && missingESP.Pushes.IsEmpty
            && missingESP.StopAtRawFileOffsetExclusive == 0x00BBDE17
            && missingESP.FlagsAtStop == missingESP.ParentPlan.FlagsAtStop
            && missingBoth.Outcome
                == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
                    .MissingFreshEntryESPAndECX
            && missingBoth.RegisterReads.All(read => read.Attempted && !read.Captured)
            && missingBoth.Pushes.IsEmpty
            && missingBoth.StopAtRawFileOffsetExclusive == 0x00BBDE17
            && !missingECX.ExactBoundedCompletion
            && !missingESP.ExactCleanCompletion
            && !missingBoth.ExactBoundedCompletion);

        NetworkDispatcherEntryFallbackSelectorFallthroughPlan takenJg =
            Project(freshComparedDword: 2);
        check("A taken JG is rejected without replacing its BBDE2B bound or CMP flags",
            takenJg.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgTakenToBBDE2B
            && takenJg.ParentPlan.StopAtRawFileOffsetExclusive == 0x00BBDE2B
            && takenJg.ParentPlan.FlagsAtStop == takenJg.ParentPlan.CompareFlags
            && takenJg.Outcome
                == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome.JgTakenRejected
            && !takenJg.ParentJgNotTakenAccepted
            && !takenJg.ProjectionEntered
            && takenJg.RegisterReads.IsEmpty
            && takenJg.Pushes.IsEmpty
            && takenJg.CompletedInstructionOffsets.SequenceEqual(
                takenJg.ParentPlan.CompletedInstructionOffsets)
            && takenJg.StopAtRawFileOffsetExclusive == takenJg.ParentPlan.StopAtRawFileOffsetExclusive
            && takenJg.FlagsAtStop == takenJg.ParentPlan.FlagsAtStop
            && !takenJg.ExactBoundedCompletion
            && !takenJg.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackSelectorFallthroughPlan missingCompare =
            Project(freshComparedDword: null);
        check("Incomplete ancestry retains the CMP boundary, flags, and unused child captures",
            missingCompare.ParentPlan.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackSelectorCompareOutcome.MissingFreshComparedDword
            && missingCompare.ParentPlan.ParentPlan.StopAtRawFileOffsetExclusive == 0x00BBDE0E
            && missingCompare.Outcome
                == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
                    .ParentSelectorBranchNotAccepted
            && !missingCompare.ParentJgNotTakenAccepted
            && !missingCompare.ProjectionEntered
            && missingCompare.RegisterReads.IsEmpty
            && missingCompare.Pushes.IsEmpty
            && missingCompare.CompletedInstructionOffsets.SequenceEqual(
                missingCompare.ParentPlan.CompletedInstructionOffsets)
            && missingCompare.StopAtRawFileOffsetExclusive
                == missingCompare.ParentPlan.StopAtRawFileOffsetExclusive
            && missingCompare.FlagsAtStop == missingCompare.ParentPlan.FlagsAtStop
            && missingCompare.FreshEntryESPCaptureState
                == NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.SuppliedUnused
            && missingCompare.FreshEntryECXCaptureState
                == NetworkDispatcherEntryFallbackSelectorFallthroughCaptureState.SuppliedUnused
            && !missingCompare.ExactBoundedCompletion
            && !missingCompare.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackSelectorFallthroughCapture immutableCapture =
            Capture(freshComparedDword: 0, freshEntryESP: 0);
        NetworkDispatcherEntryFallbackSelectorFallthroughPlan first =
            NetworkDispatcherEntryFallbackSelectorFallthroughProjection.Project(immutableCapture);
        NetworkDispatcherEntryFallbackSelectorFallthroughPlan replay =
            NetworkDispatcherEntryFallbackSelectorFallthroughProjection.Project(immutableCapture);
        check("An immutable capture reprojects the same branch, reads, ordered PUSH prefix, and Lazy predicates",
            first.Capture == replay.Capture
            && first.ParentPlan.Capture == replay.ParentPlan.Capture
            && first.ParentPlan.Instructions.SequenceEqual(replay.ParentPlan.Instructions)
            && first.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                replay.ParentPlan.CompletedInstructionOffsets)
            && first.ParentPlan.FlagsAtStop == replay.ParentPlan.FlagsAtStop
            && first.RegisterReads.SequenceEqual(replay.RegisterReads)
            && first.Pushes.SequenceEqual(replay.Pushes)
            && first.CompletedInstructionOffsets.SequenceEqual(replay.CompletedInstructionOffsets)
            && first.Outcome == replay.Outcome
            && first.StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && first.FinalESP == replay.FinalESP
            && first.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && first.ExactBoundedCompletion
            && replay.ExactBoundedCompletion
            && first.ExactCleanCompletion
            && replay.ExactCleanCompletion);
    }

    private static NetworkDispatcherEntryFallbackZeroBranchCapture CreateZeroBranchCapture(
        byte? fallbackArgument,
        uint? freshCounterDword,
        uint? freshPostFallbackESI)
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
            FreshCounterDword: freshCounterDword);
        var argumentCompareCapture = new NetworkDispatcherEntryFallbackArgumentCompareCapture(
            fallbackCounterCapture,
            FreshPostFallbackEBP: 0x2000,
            FreshArgumentByte: fallbackArgument);
        var clearStoresCapture = new NetworkDispatcherEntryFallbackClearStoresCapture(
            argumentCompareCapture,
            freshPostFallbackESI);
        return new(clearStoresCapture);
    }
}
