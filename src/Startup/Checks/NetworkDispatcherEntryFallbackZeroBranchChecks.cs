using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackZeroBranchChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackZeroBranchCapture Capture(
            byte? freshArgumentByte,
            uint? initialReturnedEAX = 0,
            uint? freshParentEBP = 0x2000,
            uint? freshPostFallbackESI = 0x1000,
            uint? freshOperationTarget = 0x12345678,
            bool? normalReturnObserved = true,
            uint? freshPostCallESP = 0x5000,
            uint? freshPostCallEBX = 0xA5A5,
            uint? freshCounterEBX = 0x1000,
            uint? freshCounterDword = 0) =>
            new(new NetworkDispatcherEntryFallbackClearStoresCapture(
                CreateArgumentCompareCapture(
                    freshParentEBP,
                    freshArgumentByte,
                    freshOperationTarget,
                    normalReturnObserved,
                    freshPostCallESP,
                    freshPostCallEBX,
                    freshCounterEBX,
                    freshCounterDword,
                    initialReturnedEAX),
                freshPostFallbackESI));

        static NetworkDispatcherEntryFallbackZeroBranchPlan Project(
            byte? freshArgumentByte,
            uint? initialReturnedEAX = 0,
            uint? freshParentEBP = 0x2000,
            uint? freshPostFallbackESI = 0x1000) =>
            NetworkDispatcherEntryFallbackZeroBranchProjection.Project(Capture(
                freshArgumentByte,
                initialReturnedEAX,
                freshParentEBP,
                freshPostFallbackESI));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("IDA and the sample independently confirm the JZ, signed target, and next instruction",
            NetworkDispatcherEntryFallbackZeroBranchProjection.BytesIndependentlyVerified
            && NetworkDispatcherEntryFallbackZeroBranchProjection.EntryRawFileOffset
                == 0x00BBDE08
            && NetworkDispatcherEntryFallbackZeroBranchProjection.EntryCodeVirtualAddress
                == 0x00FBEA08
            && NetworkDispatcherEntryFallbackZeroBranchProjection.FallThroughRawFileOffset
                == 0x00BBDE0E
            && NetworkDispatcherEntryFallbackZeroBranchProjection.FallThroughCodeVirtualAddress
                == 0x00FBEA0E
            && NetworkDispatcherEntryFallbackZeroBranchProjection.BranchInstructionByteLength == 6
            && NetworkDispatcherEntryFallbackZeroBranchProjection.SignedRelativeDisplacement == 0xBC
            && Normalize(NetworkDispatcherEntryFallbackZeroBranchProjection.BranchInstructionBytes)
                == "0F84BC000000"
            && Normalize(NetworkDispatcherEntryFallbackZeroBranchProjection
                    .CapturedBranchAndFallThroughBytes)
                == "0F84BC000000833D9085E60101"
            && Normalize(NetworkDispatcherEntryFallbackZeroBranchProjection
                    .FallThroughInstructionBytes)
                == "833D9085E60101"
            && NetworkDispatcherEntryFallbackZeroBranchProjection.FallThroughInstructionByteLength
                == 7
            && NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetCodeVirtualAddress
                == 0x00FBEACA
            && NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetRawFileOffset
                == 0x00BBDECA
            && Normalize(NetworkDispatcherEntryFallbackZeroBranchProjection
                    .BranchTargetInstructionBytes)
                == "8B86A8000000"
            && NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetInstructionByteLength
                == 6
            && NetworkDispatcherEntryFallbackZeroBranchProjection.EntryRawFileOffset
                + NetworkDispatcherEntryFallbackZeroBranchProjection.BranchInstructionByteLength
                == NetworkDispatcherEntryFallbackZeroBranchProjection.FallThroughRawFileOffset
            && NetworkDispatcherEntryFallbackZeroBranchProjection.EntryCodeVirtualAddress
                + NetworkDispatcherEntryFallbackZeroBranchProjection.BranchInstructionByteLength
                + (uint)NetworkDispatcherEntryFallbackZeroBranchProjection.SignedRelativeDisplacement
                == NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetCodeVirtualAddress
            && NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetCodeVirtualAddress
                == NetworkDispatcherEntryFallbackZeroBranchProjection.RawFileOffsetToCodeVirtualAddress(
                    NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetRawFileOffset)
            && NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00400C00
            && NetworkDispatcherEntryFallbackZeroBranchProjection
                .CalculateRelativeTargetVirtualAddress(0xFFFFFFFE, 1) == 5
            && NetworkDispatcherEntryFallbackZeroBranchProjection
                .CalculateRelativeTargetVirtualAddress(1, -4) == 3);

        bool branchCasesExact = true;
        bool sawTaken = false;
        bool sawNotTaken = false;
        foreach (byte value in new byte[] { 0, 1, 0x80, 0xFF })
        {
            NetworkDispatcherEntryFallbackZeroBranchPlan plan = Project(value);
            bool taken = value == 0;
            uint expectedDestination = taken
                ? NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetRawFileOffset
                : NetworkDispatcherEntryFallbackZeroBranchProjection.FallThroughRawFileOffset;
            NetworkDispatcherLockGateFlags expectedFlags = plan.ParentPlan.ParentPlan.CompareFlags
                ?? throw new InvalidOperationException("The parent CMP flags were not projected.");
            branchCasesExact &= plan.ParentClearStoresAccepted
                && plan.ParentPlan.Outcome
                    == NetworkDispatcherEntryFallbackClearStoresOutcome.CompletedBeforeBranch
                && plan.ParentPlan.ExactCleanCompletion
                && plan.ParentPlan.StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackZeroBranchProjection.EntryRawFileOffset
                && plan.ParentPlan.CompletedInstructionOffsets[^2]
                    == NetworkDispatcherEntryFallbackClearStoresProjection
                        .FrameDwordClearRawFileOffset
                && plan.ParentPlan.CompletedInstructionOffsets[^1]
                    == NetworkDispatcherEntryFallbackClearStoresProjection
                        .ReceiverByteClearRawFileOffset
                && plan.ParentPlan.FlagsAtStop == expectedFlags
                && expectedFlags.Zero == taken
                && plan.ParentPlan.FreshParentEBP == 0x2000
                && plan.ParentPlan.FreshPostFallbackESI == 0x1000
                && plan.ParentPlan.ParentPlan.ArgumentByte == value
                && plan.ParentPlan.FrameAddress == 0x1FFC
                && plan.ParentPlan.ReceiverAddress == 0x1064
                && plan.ParentPlan.ParentPlan.ParentFallbackCounterPlan
                    .ParentFallbackImportReturnPlan.ParentFallbackPushPlan.ParentTestBranchPlan
                    .TestBranchTaken == false
                && plan.Outcome == (taken
                    ? NetworkDispatcherEntryFallbackZeroBranchOutcome.JzTakenToBBDECA
                    : NetworkDispatcherEntryFallbackZeroBranchOutcome.JzNotTakenToBBDE0E)
                && plan.ProjectionEntered
                && plan.JzProjected
                && plan.JzTaken == taken
                && plan.JzNotTaken == !taken
                && plan.SelectedDestinationRawFileOffset == expectedDestination
                && plan.StopAtRawFileOffsetExclusive == expectedDestination
                && plan.BranchTargetRawFileOffset
                    == NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetRawFileOffset
                && plan.BranchTargetCodeVirtualAddress
                    == NetworkDispatcherEntryFallbackZeroBranchProjection.BranchTargetCodeVirtualAddress
                && plan.Instructions.Length == 1
                && plan.Instructions[0] is {
                    InstructionRawFileOffset: 0x00BBDE08,
                    InstructionCodeVirtualAddress: 0x00FBEA08,
                    InstructionByteLength: 6,
                    InstructionBytes: "0F84BC000000",
                    SignedRelativeDisplacement: 0xBC,
                    BranchTargetCodeVirtualAddress: 0x00FBEACA,
                    BranchTargetRawFileOffset: 0x00BBDECA,
                    Attempted: true,
                    Reached: true,
                    Committed: true
                }
                && plan.Instructions[0].SelectedDestinationRawFileOffset == expectedDestination
                && plan.Instructions[0].FlagsBefore == expectedFlags
                && plan.Instructions[0].FlagsAfter == expectedFlags
                && plan.FlagsAtStop == expectedFlags
                && plan.FlagsPreserved
                && plan.ExactBoundedCompletion
                && plan.ExactCleanCompletion
                && plan.OrderedSemanticPrefixPreserved
                && plan.CompletedInstructionOffsets.SequenceEqual(
                    plan.ParentPlan.CompletedInstructionOffsets
                        .Add(NetworkDispatcherEntryFallbackZeroBranchProjection.EntryRawFileOffset))
                && plan.CompletedInstructionOffsets[^1]
                    == NetworkDispatcherEntryFallbackZeroBranchProjection.EntryRawFileOffset
                && !plan.BranchExecuted
                && !plan.BranchTargetInstructionExecuted
                && !plan.FallThroughInstructionExecuted
                && !plan.NativeInstructionsExecuted
                && !plan.NativeMemoryReadsExecuted
                && !plan.NativeMemoryWritesExecuted
                && !NetworkDispatcherEntryFallbackZeroBranchProjection.ExecutesNativeInstructions;
            sawTaken |= taken && plan.StopAtRawFileOffsetExclusive == 0x00BBDECA;
            sawNotTaken |= !taken && plan.StopAtRawFileOffsetExclusive == 0x00BBDE0E;
        }

        check("CMP's preserved ZF selects the exact taken or fall-through endpoint without running either target",
            branchCasesExact && sawTaken && sawNotTaken);

        NetworkDispatcherEntryFallbackZeroBranchCapture immutableCapture = Capture(0x80);
        NetworkDispatcherEntryFallbackZeroBranchPlan first =
            NetworkDispatcherEntryFallbackZeroBranchProjection.Project(immutableCapture);
        NetworkDispatcherEntryFallbackZeroBranchPlan replay =
            NetworkDispatcherEntryFallbackZeroBranchProjection.Project(immutableCapture);
        check("The immutable accepted parent capture reprojects with its clear-store prefix and flags intact",
            first.Capture == replay.Capture
            && first.ParentPlan.Capture == replay.ParentPlan.Capture
            && first.ParentPlan.Stores.SequenceEqual(replay.ParentPlan.Stores)
            && first.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                replay.ParentPlan.CompletedInstructionOffsets)
            && first.ParentPlan.FlagsAtStop == replay.ParentPlan.FlagsAtStop
            && first.Instructions.SequenceEqual(replay.Instructions)
            && first.CompletedInstructionOffsets.SequenceEqual(replay.CompletedInstructionOffsets)
            && first.Outcome == replay.Outcome
            && first.StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && first.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && first.ExactCleanCompletion
            && replay.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackZeroBranchPlan missingESI =
            Project(freshArgumentByte: 0, freshPostFallbackESI: null);
        NetworkDispatcherEntryFallbackZeroBranchPlan missingArgument = Project(
            freshArgumentByte: null,
            freshPostFallbackESI: 0x1000);
        check("An incomplete clear-store or CMP parent preserves its own stop and projects no branch",
            missingESI.Outcome
                == NetworkDispatcherEntryFallbackZeroBranchOutcome.ParentClearStoresNotAccepted
            && missingESI.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackClearStoresOutcome.MissingFreshPostFallbackESI
            && missingESI.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackClearStoresProjection.ReceiverByteClearRawFileOffset
            && missingESI.CompletedInstructionOffsets.SequenceEqual(
                missingESI.ParentPlan.CompletedInstructionOffsets)
            && missingESI.Instructions.IsEmpty
            && !missingESI.ParentClearStoresAccepted
            && !missingESI.JzProjected
            && !missingESI.ExactBoundedCompletion
            && missingArgument.Outcome
                == NetworkDispatcherEntryFallbackZeroBranchOutcome.ParentClearStoresNotAccepted
            && missingArgument.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackClearStoresOutcome.ParentArgumentCompareNotAccepted
            && missingArgument.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackArgumentCompareProjection.CompareRawFileOffset
            && missingArgument.Instructions.IsEmpty
            && !missingArgument.JzProjected
            && !missingArgument.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackZeroBranchPlan initialImportBypass =
            Project(freshArgumentByte: 0, initialReturnedEAX: 1);
        check("A nonzero initial import return bypass stays a parent rejection, not a CMP-based JZ decision",
            initialImportBypass.Outcome
                == NetworkDispatcherEntryFallbackZeroBranchOutcome.ParentClearStoresNotAccepted
            && initialImportBypass.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackClearStoresOutcome.ParentArgumentCompareNotAccepted
            && initialImportBypass.ParentPlan.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackCounterProjection.IncrementRawFileOffset
            && initialImportBypass.ParentPlan.ParentPlan.ParentFallbackCounterPlan
                .ParentFallbackImportReturnPlan.ParentFallbackPushPlan.ParentTestBranchPlan
                .TestBranchTaken == true
            && initialImportBypass.StopAtRawFileOffsetExclusive
                == initialImportBypass.ParentPlan.StopAtRawFileOffsetExclusive
            && initialImportBypass.Instructions.IsEmpty
            && !initialImportBypass.ParentClearStoresAccepted
            && !initialImportBypass.JzProjected
            && !initialImportBypass.ExactBoundedCompletion
            && !initialImportBypass.ExactCleanCompletion);
    }

    private static NetworkDispatcherEntryFallbackArgumentCompareCapture
        CreateArgumentCompareCapture(
            uint? freshParentEBP,
            byte? freshArgumentByte,
            uint? operationTarget,
            bool? normalReturnObserved,
            uint? freshPostCallESP,
            uint? freshPostCallEBX,
            uint? freshCounterEBX,
            uint? freshCounterDword,
            uint? initialReturnedEAX)
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
        var fallbackPushCapture = new NetworkDispatcherEntryFallbackPushCapture(
            new NetworkDispatcherEntryReturnedTestBranchCapture(
                new NetworkDispatcherEntryImportReturnCapture(
                    lockCapture,
                    0x12345678,
                    true,
                    initialReturnedEAX)),
            freshPostCallESP,
            freshPostCallEBX);
        var fallbackImportCapture = new NetworkDispatcherEntryFallbackImportReturnCapture(
            fallbackPushCapture,
            operationTarget,
            normalReturnObserved);
        var fallbackCounterCapture = new NetworkDispatcherEntryFallbackCounterCapture(
            fallbackImportCapture,
            freshCounterEBX,
            freshCounterDword);
        return new(
            fallbackCounterCapture,
            freshParentEBP,
            freshArgumentByte);
    }
}
