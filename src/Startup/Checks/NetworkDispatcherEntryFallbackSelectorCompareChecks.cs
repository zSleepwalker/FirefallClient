using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackSelectorCompareChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackSelectorCompareCapture Capture(
            uint? freshComparedDword = 2,
            byte? fallbackArgument = 0x80,
            uint? freshCounterDword = 0,
            uint? freshPostFallbackESI = 0x1000)
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
            var zeroBranchCapture =
                new NetworkDispatcherEntryFallbackZeroBranchCapture(clearStoresCapture);
            return new(zeroBranchCapture, freshComparedDword);
        }

        static NetworkDispatcherEntryFallbackSelectorComparePlan Project(
            uint? freshComparedDword = 2,
            byte? fallbackArgument = 0x80,
            uint? freshCounterDword = 0,
            uint? freshPostFallbackESI = 0x1000) =>
            NetworkDispatcherEntryFallbackSelectorCompareProjection.Project(Capture(
                freshComparedDword,
                fallbackArgument,
                freshCounterDword,
                freshPostFallbackESI));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("IDA confirms the scalar DWORD CMP, absolute operand, and stop boundary before the following branch",
            NetworkDispatcherEntryFallbackSelectorCompareProjection.EntryRawFileOffset
                == 0x00BBDE0E
            && NetworkDispatcherEntryFallbackSelectorCompareProjection.CompareRawFileOffset
                == 0x00BBDE0E
            && NetworkDispatcherEntryFallbackSelectorCompareProjection.EntryCodeVirtualAddress
                == 0x00FBEA0E
            && NetworkDispatcherEntryFallbackSelectorCompareProjection.CompareCodeVirtualAddress
                == 0x00FBEA0E
            && NetworkDispatcherEntryFallbackSelectorCompareProjection.ComparedMemoryOperandVirtualAddress
                == 0x01E68590
            && NetworkDispatcherEntryFallbackSelectorCompareProjection.ComparedDwordWidth == 4
            && NetworkDispatcherEntryFallbackSelectorCompareProjection.ComparedImmediate == 1
            && NetworkDispatcherEntryFallbackSelectorCompareProjection.CompareInstructionByteLength
                == 7
            && NetworkDispatcherEntryFallbackSelectorCompareProjection.BoundaryRawFileOffsetExclusive
                == 0x00BBDE15
            && NetworkDispatcherEntryFallbackSelectorCompareProjection.CompareRawFileOffset
                + NetworkDispatcherEntryFallbackSelectorCompareProjection
                    .CompareInstructionByteLength
                == NetworkDispatcherEntryFallbackSelectorCompareProjection
                    .BoundaryRawFileOffsetExclusive
            && NetworkDispatcherEntryFallbackSelectorCompareProjection
                .RawFileOffsetToCodeVirtualAddress(
                    NetworkDispatcherEntryFallbackSelectorCompareProjection.EntryRawFileOffset)
                == NetworkDispatcherEntryFallbackSelectorCompareProjection.EntryCodeVirtualAddress
            && NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta == 0x00400C00
            && Normalize(NetworkDispatcherEntryFallbackSelectorCompareProjection
                    .CompareInstructionBytes) == "833D9085E60101"
            && Normalize(NetworkDispatcherEntryFallbackSelectorCompareProjection
                    .CapturedWindowBytes) == "833D9085E601017F14"
            && !NetworkDispatcherEntryFallbackSelectorCompareProjection.ExecutesNativeInstructions);

        bool arithmeticCasesExact = true;
        foreach (uint value in new uint[] {
            0,
            1,
            2,
            0x7FFFFFFF,
            0x80000000,
            uint.MaxValue
        })
        {
            NetworkDispatcherLockGateFlags expectedFlags = value switch
            {
                0 => new(true, true, true, false, true, false),
                1 => new(false, true, false, true, false, false),
                2 => new(false, false, false, false, false, false),
                0x7FFFFFFF => new(false, false, false, false, false, false),
                0x80000000 => new(false, true, true, false, false, true),
                uint.MaxValue => new(false, false, false, false, true, false),
                _ => throw new InvalidOperationException("Unexpected selector comparison fixture.")
            };
            NetworkDispatcherEntryFallbackSelectorComparePlan plan = Project(value);
            arithmeticCasesExact &= plan.ParentZeroBranchAccepted
                && plan.ParentPlan.Outcome
                    == NetworkDispatcherEntryFallbackZeroBranchOutcome.JzNotTakenToBBDE0E
                && plan.ParentPlan.StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackSelectorCompareProjection.EntryRawFileOffset
                && plan.ParentPlan.FlagsAtStop is { Zero: false }
                && plan.Outcome == NetworkDispatcherEntryFallbackSelectorCompareOutcome.Compared
                && plan.ComparedDwordCaptureState
                    == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.Captured
                && plan.ComparedDword == value
                && plan.CompareRead is {
                    InstructionRawFileOffset: 0x00BBDE0E,
                    OperandVirtualAddress: 0x01E68590,
                    Width: 4,
                    Attempted: true,
                    Captured: true,
                    Committed: false
                }
                && plan.CompareRead?.Value == value
                && plan.CompareFlags == expectedFlags
                && plan.FlagsAtStop == expectedFlags
                && plan.CompareReached
                && plan.CompletedInstructionOffsets.SequenceEqual(
                    plan.ParentPlan.CompletedInstructionOffsets.Add(0x00BBDE0E))
                && plan.CompletedInstructionOffsets[^1] == 0x00BBDE0E
                && plan.StopAtRawFileOffsetExclusive == 0x00BBDE15
                && plan.ExactBoundedCompletion
                && plan.ExactCleanCompletion
                && plan.OrderedSemanticPrefixPreserved
                && !plan.NextBranchProjected
                && !plan.NextBranchExecuted
                && !plan.NativeInstructionsExecuted
                && !plan.NativeMemoryReadsExecuted
                && !plan.NativeMemoryWritesExecuted;
        }

        check("Captured DWORD values compute all CMP flags with wraparound and stop after exactly this CMP",
            arithmeticCasesExact);

        NetworkDispatcherEntryFallbackSelectorComparePlan missingValue =
            Project(freshComparedDword: null);
        check("A missing fresh DWORD records the attempted scalar read but completes no instruction or flag update",
            missingValue.ParentZeroBranchAccepted
            && missingValue.Outcome
                == NetworkDispatcherEntryFallbackSelectorCompareOutcome.MissingFreshComparedDword
            && missingValue.ComparedDwordCaptureState
                == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.Missing
            && missingValue.CompareRead is {
                InstructionRawFileOffset: 0x00BBDE0E,
                OperandVirtualAddress: 0x01E68590,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }
            && missingValue.CompareFlags is null
            && missingValue.FlagsAtStop == missingValue.ParentPlan.FlagsAtStop
            && missingValue.StopAtRawFileOffsetExclusive == 0x00BBDE0E
            && missingValue.CompletedInstructionOffsets.SequenceEqual(
                missingValue.ParentPlan.CompletedInstructionOffsets)
            && !missingValue.CompareReached
            && !missingValue.ExactBoundedCompletion
            && !missingValue.ExactCleanCompletion
            && !missingValue.NextBranchProjected
            && !missingValue.NextBranchExecuted);

        NetworkDispatcherEntryFallbackSelectorComparePlan zeroArgument =
            Project(freshComparedDword: 2, fallbackArgument: 0);
        check("A zero fallback argument selects the parent's taken JZ and rejects the selector CMP",
            zeroArgument.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackZeroBranchOutcome.JzTakenToBBDECA
            && zeroArgument.ParentPlan.JzTaken
            && zeroArgument.ParentPlan.StopAtRawFileOffsetExclusive == 0x00BBDECA
            && zeroArgument.Outcome
                == NetworkDispatcherEntryFallbackSelectorCompareOutcome
                    .ParentFallbackZeroBranchNotAccepted
            && zeroArgument.ComparedDwordCaptureState
                == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.SuppliedUnused
            && zeroArgument.CompareRead is null
            && zeroArgument.CompareFlags is null
            && zeroArgument.FlagsAtStop == zeroArgument.ParentPlan.FlagsAtStop
            && zeroArgument.StopAtRawFileOffsetExclusive
                == zeroArgument.ParentPlan.StopAtRawFileOffsetExclusive
            && zeroArgument.CompletedInstructionOffsets.SequenceEqual(
                zeroArgument.ParentPlan.CompletedInstructionOffsets)
            && !zeroArgument.ParentZeroBranchAccepted
            && !zeroArgument.CompareReached
            && !zeroArgument.ProjectionEntered
            && !zeroArgument.ExactBoundedCompletion
            && !zeroArgument.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackSelectorComparePlan missingParentCounter =
            Project(freshComparedDword: 2, freshCounterDword: null);
        NetworkDispatcherEntryFallbackSelectorComparePlan missingParentArgument =
            Project(freshComparedDword: 2, fallbackArgument: null);
        check("Incomplete parents preserve their own stops and flags without consuming the child scalar",
            missingParentCounter.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackZeroBranchOutcome.ParentClearStoresNotAccepted
            && missingParentCounter.ParentPlan.StopAtRawFileOffsetExclusive == 0x00BBDDF6
            && missingParentCounter.Outcome
                == NetworkDispatcherEntryFallbackSelectorCompareOutcome
                    .ParentFallbackZeroBranchNotAccepted
            && missingParentCounter.StopAtRawFileOffsetExclusive
                == missingParentCounter.ParentPlan.StopAtRawFileOffsetExclusive
            && missingParentCounter.FlagsAtStop == missingParentCounter.ParentPlan.FlagsAtStop
            && missingParentCounter.ComparedDwordCaptureState
                == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.SuppliedUnused
            && missingParentCounter.CompareRead is null
            && missingParentCounter.CompletedInstructionOffsets.SequenceEqual(
                missingParentCounter.ParentPlan.CompletedInstructionOffsets)
            && !missingParentCounter.ExactBoundedCompletion
            && missingParentArgument.ParentPlan.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackArgumentCompareProjection.CompareRawFileOffset
            && missingParentArgument.StopAtRawFileOffsetExclusive
                == missingParentArgument.ParentPlan.StopAtRawFileOffsetExclusive
            && missingParentArgument.FlagsAtStop == missingParentArgument.ParentPlan.FlagsAtStop
            && missingParentArgument.ComparedDwordCaptureState
                == NetworkDispatcherEntryFallbackSelectorCompareCaptureState.SuppliedUnused
            && missingParentArgument.CompareRead is null
            && !missingParentArgument.CompareReached
            && !missingParentArgument.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackSelectorCompareCapture immutableCapture = Capture(0x80000000);
        NetworkDispatcherEntryFallbackSelectorComparePlan first =
            NetworkDispatcherEntryFallbackSelectorCompareProjection.Project(immutableCapture);
        NetworkDispatcherEntryFallbackSelectorComparePlan replay =
            NetworkDispatcherEntryFallbackSelectorCompareProjection.Project(immutableCapture);
        check("An immutable parent capture reprojects the complete accepted prefix and CMP evidence",
            first.Capture == replay.Capture
            && first.ParentPlan.Capture == replay.ParentPlan.Capture
            && first.ParentPlan.Instructions.SequenceEqual(replay.ParentPlan.Instructions)
            && first.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                replay.ParentPlan.CompletedInstructionOffsets)
            && first.ParentPlan.FlagsAtStop == replay.ParentPlan.FlagsAtStop
            && first.CompareRead == replay.CompareRead
            && first.CompletedInstructionOffsets.SequenceEqual(replay.CompletedInstructionOffsets)
            && first.CompareFlags == replay.CompareFlags
            && first.FlagsAtStop == replay.FlagsAtStop
            && first.Outcome == replay.Outcome
            && first.StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && first.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && first.ExactCleanCompletion
            && replay.ExactCleanCompletion);
    }
}
