using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveHighCountSuffixChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed suffix is exactly raw [BE5BF1, BE5C0F) with the pinned mapping",
            UnreliableReceiveHighCountSuffixProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveHighCountSuffixProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveHighCountSuffixProjection.EntryRawFileOffset == 0x00BE5BF1
            && UnreliableReceiveHighCountSuffixProjection.EntryVirtualAddress == 0x00FE67F1
            && UnreliableReceiveHighCountSuffixProjection.CallbackBoundaryRawFileOffset
                == 0x00BE5BFE
            && UnreliableReceiveHighCountSuffixProjection.CounterJoinRawFileOffset == 0x00BE5C0F
            && UnreliableReceiveHighCountSuffixProjection.EndRawFileOffsetExclusive == 0x00BE5C0F
            && UnreliableReceiveHighCountSuffixProjection.EndVirtualAddressExclusive == 0x00FE680F
            && UnreliableReceiveHighCountSuffixProjection.RawFileOffsetToVirtualAddress(
                0x00BE5BF1) == 0x00FE67F1
            && UnreliableReceiveHighCountSuffixProjection.RawFileOffsetToVirtualAddress(
                0x00BE5C0F) == 0x00FE680F
            && UnreliableReceiveHighCountSuffixProjection.CapturedWindowBytes
                == UnreliableReceiveHighCountCallbackProjection.CapturedWindowBytes
                    + " "
                    + UnreliableReceiveHighCountCounterProjection.CapturedWindowBytes
            && UnreliableReceiveHighCountSuffixProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 30
            && UnreliableReceiveHighCountSuffixProjection.Instructions.Length == 11
            && UnreliableReceiveHighCountSuffixProjection.Instructions[0].RawFileOffset
                == 0x00BE5BF1
            && UnreliableReceiveHighCountSuffixProjection.Instructions[^1].RawFileOffset
                == 0x00BE5C0C
            && UnreliableReceiveHighCountSuffixProjection.Instructions
                .All(instruction =>
                    instruction.VirtualAddress
                        == UnreliableReceiveHighCountSuffixProjection
                            .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)));

        UnreliableReceiveHighCountSuffixPlan cleanZeroBranch =
            UnreliableReceiveHighCountSuffixProjection.Project(
                new(
                    new(true, 0, 0, null, null, null),
                    new(true, 0x9000, 1, null)));
        check("A clean zero-first-loaded-pointer branch reaches the explicit counter receiver and completes",
            cleanZeroBranch.Outcome
                == UnreliableReceiveHighCountSuffixOutcome.Counter4CDecrementedAndCounter48Zeroed
            && cleanZeroBranch.CallbackPlan.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.ZeroObjectBranch
            && cleanZeroBranch.CallbackPlan.NormalBoundaryAdmitted
            && cleanZeroBranch.CounterEntryReached
            && cleanZeroBranch.CounterPlan.HasValue
            && cleanZeroBranch.CounterPlan.Value.Counter4CRead
            && cleanZeroBranch.CounterPlan.Value.Counter4COldValue == 1
            && cleanZeroBranch.CounterPlan.Value.Counter4CNewValue == 0
            && cleanZeroBranch.CounterPlan.Value.Counter48ZeroStoreProjected
            && cleanZeroBranch.CounterJoinAdmitted
            && cleanZeroBranch.Complete
            && cleanZeroBranch.CounterPlan?.Capture.ReceiverEdi == 0x9000
            && cleanZeroBranch.SelectedDestinationRawFileOffset == 0x00BE5C0F
            && cleanZeroBranch.StopAtRawFileOffsetExclusive == 0x00BE5C0F);

        UnreliableReceiveHighCountSuffixPlan zeroBranchWithUnreached48 =
            UnreliableReceiveHighCountSuffixProjection.Project(
                new(
                    new(true, 0, 0, null, null, null),
                    new(true, 0x9000, 1, 0x1234)));
        check("A zero DEC preserves its write and zero-store but rejects an unreached 48 reload as completion",
            zeroBranchWithUnreached48.Outcome
                == UnreliableReceiveHighCountSuffixOutcome.UnreachedCounterCaptureProvided
            && zeroBranchWithUnreached48.CounterEntryReached
            && zeroBranchWithUnreached48.CounterPlan?.Outcome
                == UnreliableReceiveHighCountCounterOutcome
                    .Counter4CDecrementedAndCounter48Zeroed
            && zeroBranchWithUnreached48.CounterPlan?.Counter4CNewValue == 0
            && zeroBranchWithUnreached48.CounterPlan?.Counter4CWriteProjected == true
            && zeroBranchWithUnreached48.CounterPlan?.Counter48ZeroStoreProjected == true
            && zeroBranchWithUnreached48.CounterPlan?.UnreachedCounter48CaptureProvided == true
            && zeroBranchWithUnreached48.UnreachedCounter48CaptureProvided
            && !zeroBranchWithUnreached48.CounterJoinAdmitted
            && !zeroBranchWithUnreached48.Complete
            && zeroBranchWithUnreached48.StopAtRawFileOffsetExclusive == 0x00BE5C0F);

        UnreliableReceiveHighCountSuffixPlan normalCallbackMissing48 =
            UnreliableReceiveHighCountSuffixProjection.Project(
                new(
                    new(true, 0x1000, 0x2000, 0x3000, 0x4000, true),
                    new(true, 0x9000, 2, null)));
        check("A normal indirect return preserves PUSH 1 and the 4C DEC when the reached 48 reload is missing",
            normalCallbackMissing48.Outcome
                == UnreliableReceiveHighCountSuffixOutcome.MissingCounter48ForNonzeroPostDecrement
            && normalCallbackMissing48.CallbackPlan.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.CallbackReturnedNormally
            && normalCallbackMissing48.CallbackPlan.StackPushesInInstructionOrder
                .SequenceEqual(new uint[] { 1 })
            && normalCallbackMissing48.CounterEntryReached
            && normalCallbackMissing48.CounterPlan?.Counter4CNewValue == 1
            && normalCallbackMissing48.CounterPlan?.Counter4CWriteProjected == true
            && normalCallbackMissing48.CounterPlan?.NonzeroPostDecrementBranchTaken == true
            && normalCallbackMissing48.CounterPlan?.Counter48Read == false
            && !normalCallbackMissing48.CounterJoinAdmitted
            && !normalCallbackMissing48.Complete
            && normalCallbackMissing48.StopAtRawFileOffsetExclusive == 0x00BE5C0C);

        UnreliableReceiveHighCountSuffixPlan nonreturningCallback =
            UnreliableReceiveHighCountSuffixProjection.Project(
                new(
                    new(true, 0x1000, 0x2000, 0x3000, 0x4000, false),
                    new(true, 0x9000, 1, null)));
        UnreliableReceiveHighCountSuffixPlan invalidCallback =
            UnreliableReceiveHighCountSuffixProjection.Project(
                new(
                    new(false, null, null, null, null, null),
                    new(true, 0x9000, 1, null)));
        check("Neither a failed callback nor an unvalidated prior boundary reaches the counter through BFE",
            nonreturningCallback.Outcome
                == UnreliableReceiveHighCountSuffixOutcome.UnreachedCounterCaptureProvided
            && nonreturningCallback.CallbackPlan.SelectedDestinationRawFileOffset is null
            && nonreturningCallback.CallbackPlan.StopAtRawFileOffsetExclusive == 0x00BE5BFB
            && !nonreturningCallback.CounterEntryReached
            && nonreturningCallback.CounterPlan is null
            && nonreturningCallback.UnreachedCounterEntryValidationProvided
            && nonreturningCallback.UnreachedIndependentCounter4CReloadCaptureProvided
            && !nonreturningCallback.CounterJoinAdmitted
            && !nonreturningCallback.Complete
            && invalidCallback.CallbackPlan.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.PriorBE6440DidNotReturnNormally
            && invalidCallback.CallbackPlan.SelectedDestinationRawFileOffset is null
            && !invalidCallback.CounterEntryReached
            && invalidCallback.CounterPlan is null
            && !invalidCallback.CounterJoinAdmitted
            && !invalidCallback.Complete);

        UnreliableReceiveHighCountSuffixPlan invalidCounterEntry =
            UnreliableReceiveHighCountSuffixProjection.Project(
                new(
                    new(true, 0, 0, null, null, null),
                    new(false, 0x9000, null, null)));
        check("The counter's own validated-entry gate remains required before DEC",
            invalidCounterEntry.Outcome
                == UnreliableReceiveHighCountSuffixOutcome.InvalidCounterEntry
            && invalidCounterEntry.CounterEntryReached
            && invalidCounterEntry.CounterPlan?.Outcome
                == UnreliableReceiveHighCountCounterOutcome.InvalidEntry
            && invalidCounterEntry.CounterPlan?.Counter4CRead == false
            && invalidCounterEntry.CounterPlan?.Counter4CWriteProjected == false
            && !invalidCounterEntry.CounterJoinAdmitted
            && !invalidCounterEntry.Complete);
    }
}
