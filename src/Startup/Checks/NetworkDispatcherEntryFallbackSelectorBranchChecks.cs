using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackSelectorBranchChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackSelectorBranchCapture Capture(
            uint? freshComparedDword = 2,
            byte? fallbackArgument = 0x80,
            uint? freshCounterDword = 0,
            uint? freshPostFallbackESI = 0x1000) =>
            new(new NetworkDispatcherEntryFallbackSelectorCompareCapture(
                CreateZeroBranchCapture(
                    fallbackArgument,
                    freshCounterDword,
                    freshPostFallbackESI),
                freshComparedDword));

        static NetworkDispatcherEntryFallbackSelectorBranchPlan Project(
            uint? freshComparedDword = 2,
            byte? fallbackArgument = 0x80,
            uint? freshCounterDword = 0,
            uint? freshPostFallbackESI = 0x1000) =>
            NetworkDispatcherEntryFallbackSelectorBranchProjection.Project(Capture(
                freshComparedDword,
                fallbackArgument,
                freshCounterDword,
                freshPostFallbackESI));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("IDA confirms the short JG encoding and its exact target/fall-through addresses",
            NetworkDispatcherEntryFallbackSelectorBranchProjection.BytesIndependentlyVerified
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.EntryRawFileOffset
                == 0x00BBDE15
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.EntryCodeVirtualAddress
                == 0x00FBEA15
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.BranchInstructionByteLength
                == 2
            && Normalize(NetworkDispatcherEntryFallbackSelectorBranchProjection
                    .BranchInstructionBytes) == "7F14"
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.SignedRelativeDisplacement
                == 0x14
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.FallThroughRawFileOffset
                == 0x00BBDE17
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.FallThroughCodeVirtualAddress
                == 0x00FBEA17
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.BranchTargetRawFileOffset
                == 0x00BBDE2B
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.BranchTargetCodeVirtualAddress
                == 0x00FBEA2B
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.FallThroughRawFileOffset
                == NetworkDispatcherEntryFallbackSelectorBranchProjection.EntryRawFileOffset
                    + NetworkDispatcherEntryFallbackSelectorBranchProjection
                        .BranchInstructionByteLength
            && NetworkDispatcherEntryFallbackSelectorBranchProjection.BranchTargetRawFileOffset
                == NetworkDispatcherEntryFallbackSelectorBranchProjection.FallThroughRawFileOffset
                    + (uint)NetworkDispatcherEntryFallbackSelectorBranchProjection
                        .SignedRelativeDisplacement
            && NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00400C00
            && !NetworkDispatcherEntryFallbackSelectorBranchProjection.ExecutesNativeInstructions);

        bool signedGreaterCasesExact = true;
        bool sawTaken = false;
        bool sawNotTaken = false;
        foreach (uint value in new uint[] {
            0,
            1,
            2,
            0x7FFFFFFF,
            0x80000000,
            uint.MaxValue
        })
        {
            NetworkDispatcherEntryFallbackSelectorBranchPlan plan = Project(value);
            bool taken = unchecked((int)value) > 1;
            uint expectedDestination = taken
                ? NetworkDispatcherEntryFallbackSelectorBranchProjection.BranchTargetRawFileOffset
                : NetworkDispatcherEntryFallbackSelectorBranchProjection.FallThroughRawFileOffset;
            NetworkDispatcherLockGateFlags expectedFlags = plan.ParentPlan.CompareFlags
                ?? throw new InvalidOperationException("The parent CMP flags were not projected.");
            signedGreaterCasesExact &= plan.ParentSelectorCompareAccepted
                && plan.ParentPlan.Outcome
                    == NetworkDispatcherEntryFallbackSelectorCompareOutcome.Compared
                && plan.ParentPlan.StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackSelectorCompareProjection
                        .BoundaryRawFileOffsetExclusive
                && plan.ParentPlan.CompareRead is {
                    InstructionRawFileOffset: 0x00BBDE0E,
                    OperandVirtualAddress: 0x01E68590,
                    Width: 4,
                    Attempted: true,
                    Captured: true,
                    Committed: false
                }
                && plan.ParentPlan.CompareRead?.Value == value
                && plan.ComparedDword == value
                && plan.CompareRead == plan.ParentPlan.CompareRead
                && plan.CompareFlags == expectedFlags
                && plan.FlagsAtStop == expectedFlags
                && plan.Outcome == (taken
                    ? NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgTakenToBBDE2B
                    : NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgNotTakenToBBDE17)
                && plan.ProjectionEntered
                && plan.JgProjected
                && plan.JgTaken == taken
                && plan.JgNotTaken == !taken
                && plan.SelectedDestinationRawFileOffset == expectedDestination
                && plan.StopAtRawFileOffsetExclusive == expectedDestination
                && plan.BranchTargetRawFileOffset
                    == NetworkDispatcherEntryFallbackSelectorBranchProjection
                        .BranchTargetRawFileOffset
                && plan.FallThroughRawFileOffset
                    == NetworkDispatcherEntryFallbackSelectorBranchProjection
                        .FallThroughRawFileOffset
                && plan.Instructions.Length == 1
                && plan.Instructions[0] is {
                    InstructionRawFileOffset: 0x00BBDE15,
                    InstructionCodeVirtualAddress: 0x00FBEA15,
                    InstructionByteLength: 2,
                    InstructionBytes: "7F14",
                    SignedRelativeDisplacement: 0x14,
                    BranchTargetCodeVirtualAddress: 0x00FBEA2B,
                    BranchTargetRawFileOffset: 0x00BBDE2B,
                    Attempted: true,
                    Reached: true,
                    Committed: true
                }
                && plan.Instructions[0].Taken == taken
                && plan.Instructions[0].SelectedDestinationRawFileOffset == expectedDestination
                && plan.Instructions[0].FlagsBefore == expectedFlags
                && plan.Instructions[0].FlagsAfter == expectedFlags
                && plan.FlagsPreserved
                && plan.CompletedInstructionOffsets.SequenceEqual(
                    plan.ParentPlan.CompletedInstructionOffsets.Add(0x00BBDE15))
                && plan.CompletedInstructionOffsets[^1] == 0x00BBDE15
                && plan.ExactBoundedCompletion
                && plan.ExactCleanCompletion
                && plan.OrderedSemanticPrefixPreserved
                && !plan.BranchExecuted
                && !plan.BranchTargetInstructionExecuted
                && !plan.FallThroughInstructionExecuted
                && !plan.NativeInstructionsExecuted
                && !plan.NativeMemoryReadsExecuted
                && !plan.NativeMemoryWritesExecuted
                && !plan.ParentInstructionsExecuted;
            sawTaken |= taken && plan.StopAtRawFileOffsetExclusive == 0x00BBDE2B;
            sawNotTaken |= !taken && plan.StopAtRawFileOffsetExclusive == 0x00BBDE17;
        }

        check("JG uses signed greater-than semantics from preserved CMP flags and stops before either destination",
            signedGreaterCasesExact && sawTaken && sawNotTaken);

        NetworkDispatcherEntryFallbackSelectorBranchCapture immutableCapture = Capture(0x80000000);
        NetworkDispatcherEntryFallbackSelectorBranchPlan first =
            NetworkDispatcherEntryFallbackSelectorBranchProjection.Project(immutableCapture);
        NetworkDispatcherEntryFallbackSelectorBranchPlan replay =
            NetworkDispatcherEntryFallbackSelectorBranchProjection.Project(immutableCapture);
        check("The immutable capture reprojects the parent CMP value, read evidence, flags, and ordered prefix",
            first.Capture == replay.Capture
            && first.ParentPlan.Capture == replay.ParentPlan.Capture
            && first.ParentPlan.ComparedDwordCaptureState
                == replay.ParentPlan.ComparedDwordCaptureState
            && first.ParentPlan.CompareRead == replay.ParentPlan.CompareRead
            && first.ParentPlan.CompareFlags == replay.ParentPlan.CompareFlags
            && first.ParentPlan.FlagsAtStop == replay.ParentPlan.FlagsAtStop
            && first.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                replay.ParentPlan.CompletedInstructionOffsets)
            && first.CompareRead == replay.CompareRead
            && first.Instructions.SequenceEqual(replay.Instructions)
            && first.CompletedInstructionOffsets.SequenceEqual(replay.CompletedInstructionOffsets)
            && first.Outcome == replay.Outcome
            && first.StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && first.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && first.ExactCleanCompletion
            && replay.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackSelectorBranchPlan zeroArgument =
            Project(freshComparedDword: 2, fallbackArgument: 0);
        check("The parent's zero-argument JZ bypass is rejected without replacing its actual target bound",
            zeroArgument.ParentPlan.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackZeroBranchOutcome.JzTakenToBBDECA
            && zeroArgument.ParentPlan.ParentPlan.StopAtRawFileOffsetExclusive == 0x00BBDECA
            && zeroArgument.Outcome
                == NetworkDispatcherEntryFallbackSelectorBranchOutcome
                    .ParentSelectorCompareNotAccepted
            && zeroArgument.ParentPlan.ComparedDwordCaptureState
                == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.SuppliedUnused
            && zeroArgument.ParentPlan.CompareRead is null
            && zeroArgument.StopAtRawFileOffsetExclusive
                == zeroArgument.ParentPlan.StopAtRawFileOffsetExclusive
            && zeroArgument.FlagsAtStop == zeroArgument.ParentPlan.FlagsAtStop
            && zeroArgument.CompletedInstructionOffsets.SequenceEqual(
                zeroArgument.ParentPlan.CompletedInstructionOffsets)
            && !zeroArgument.ParentSelectorCompareAccepted
            && !zeroArgument.JgProjected
            && !zeroArgument.ProjectionEntered
            && !zeroArgument.ExactBoundedCompletion
            && !zeroArgument.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackSelectorBranchPlan missingComparedDword =
            Project(freshComparedDword: null);
        check("An incomplete CMP retains its attempted-read evidence and exact pre-CMP stop",
            missingComparedDword.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackSelectorCompareOutcome.MissingFreshComparedDword
            && missingComparedDword.ParentPlan.CompareRead is {
                InstructionRawFileOffset: 0x00BBDE0E,
                OperandVirtualAddress: 0x01E68590,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }
            && missingComparedDword.Outcome
                == NetworkDispatcherEntryFallbackSelectorBranchOutcome
                    .ParentSelectorCompareNotAccepted
            && missingComparedDword.CompareRead == missingComparedDword.ParentPlan.CompareRead
            && missingComparedDword.ComparedDword is null
            && missingComparedDword.CompareFlags is null
            && missingComparedDword.StopAtRawFileOffsetExclusive == 0x00BBDE0E
            && missingComparedDword.FlagsAtStop
                == missingComparedDword.ParentPlan.FlagsAtStop
            && missingComparedDword.CompletedInstructionOffsets.SequenceEqual(
                missingComparedDword.ParentPlan.CompletedInstructionOffsets)
            && !missingComparedDword.JgProjected
            && !missingComparedDword.ExactBoundedCompletion
            && !missingComparedDword.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackSelectorBranchPlan missingCounter =
            Project(freshComparedDword: 2, freshCounterDword: null);
        NetworkDispatcherEntryFallbackSelectorBranchPlan missingArgument =
            Project(freshComparedDword: 2, fallbackArgument: null);
        check("Earlier incomplete parents retain their own stops and do not project JG",
            missingCounter.ParentPlan.ParentPlan.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && missingCounter.StopAtRawFileOffsetExclusive
                == missingCounter.ParentPlan.StopAtRawFileOffsetExclusive
            && missingCounter.FlagsAtStop == missingCounter.ParentPlan.FlagsAtStop
            && missingCounter.ParentPlan.ComparedDwordCaptureState
                == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.SuppliedUnused
            && missingCounter.ParentPlan.CompareRead is null
            && !missingCounter.JgProjected
            && missingArgument.ParentPlan.ParentPlan.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackArgumentCompareProjection.CompareRawFileOffset
            && missingArgument.StopAtRawFileOffsetExclusive
                == missingArgument.ParentPlan.StopAtRawFileOffsetExclusive
            && missingArgument.FlagsAtStop == missingArgument.ParentPlan.FlagsAtStop
            && missingArgument.ParentPlan.ComparedDwordCaptureState
                == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.SuppliedUnused
            && missingArgument.ParentPlan.CompareRead is null
            && !missingArgument.JgProjected
            && !missingArgument.ExactCleanCompletion);
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
