using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveHighCountCallbackChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE maps the exact 13-byte raw window and excludes the receiver decrement",
            UnreliableReceiveHighCountCallbackProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveHighCountCallbackProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveHighCountCallbackProjection.EntryRawFileOffset == 0x00BE5BF1
            && UnreliableReceiveHighCountCallbackProjection.EntryVirtualAddress == 0x00FE67F1
            && UnreliableReceiveHighCountCallbackProjection.PriorBE6440NormalBoundaryRawFileOffset
                == 0x00BE6440
            && UnreliableReceiveHighCountCallbackProjection.ZeroObjectBranchRawFileOffset
                == 0x00BE5BF5
            && UnreliableReceiveHighCountCallbackProjection.SecondReadRawFileOffset
                == 0x00BE5BF7
            && UnreliableReceiveHighCountCallbackProjection.PushOneRawFileOffset
                == 0x00BE5BF9
            && UnreliableReceiveHighCountCallbackProjection.IndirectCallRawFileOffset
                == 0x00BE5BFB
            && UnreliableReceiveHighCountCallbackProjection.NormalBoundaryRawFileOffset
                == 0x00BE5BFE
            && UnreliableReceiveHighCountCallbackProjection.NormalBoundaryVirtualAddress
                == 0x00FE67FE
            && UnreliableReceiveHighCountCallbackProjection.EndRawFileOffsetExclusive
                == 0x00BE5BFE
            && UnreliableReceiveHighCountCallbackProjection.RawFileOffsetToVirtualAddress(
                0x00BE5BF1) == 0x00FE67F1
            && UnreliableReceiveHighCountCallbackProjection.RawFileOffsetToVirtualAddress(
                0x00BE5BFE) == 0x00FE67FE
            && UnreliableReceiveHighCountCallbackProjection.CapturedWindowBytes
                == "8B 08 85 C9 74 07 8B 01 6A 01 FF 50 08"
            && UnreliableReceiveHighCountCallbackProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 13
            && InstructionsMatch());

        UnreliableReceiveHighCountCallbackPlan missingPrior =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(null, 0, 0, 0, 0, true));
        UnreliableReceiveHighCountCallbackPlan priorDidNotReturn =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(false, null, null, null, null, null));
        check("Only a normally returned prior boundary admits the helper-return address capture",
            missingPrior.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
            && !missingPrior.PriorBoundaryValidated
            && missingPrior.UnreachedHelperReturnEaxCaptureProvided
            && missingPrior.UnreachedFirstLoadedDwordCaptureProvided
            && missingPrior.UnreachedSecondLoadedDwordCaptureProvided
            && missingPrior.UnreachedIndirectTargetDwordCaptureProvided
            && missingPrior.UnreachedCallbackNormalCaptureProvided
            && !missingPrior.NormalBoundaryAdmitted
            && missingPrior.SelectedDestinationRawFileOffset is null
            && missingPrior.StopAtRawFileOffsetExclusive == 0x00BE5BF1
            && priorDidNotReturn.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.PriorBE6440DidNotReturnNormally
            && !priorDidNotReturn.FirstReadAddressCaptured
            && !priorDidNotReturn.NormalBoundaryAdmitted);

        UnreliableReceiveHighCountCallbackPlan zeroObject =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0, 0, null, null, null));
        UnreliableReceiveHighCountCallbackPlan zeroObjectWithLaterCaptures =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0, 0, 0, 0, true));
        check("A zero helper-return EAX is a valid first-read address; only a clean zero-object branch admits the boundary",
            zeroObject.Outcome == UnreliableReceiveHighCountCallbackOutcome.ZeroObjectBranch
            && zeroObject.PriorBoundaryValidated
            && zeroObject.FirstReadAddressCaptured
            && zeroObject.FirstReadAddress == 0
            && zeroObject.FirstLoadedDwordRead
            && zeroObject.CapturedFirstLoadedDword == 0
            && zeroObject.ZeroObjectBranchTaken
            && !zeroObject.SecondReadReached
            && !zeroObject.PushOneReached
            && zeroObject.StackPushesInInstructionOrder.IsEmpty
            && !zeroObject.IndirectCallReached
            && zeroObject.NormalBoundaryAdmitted
            && zeroObject.SelectedDestinationRawFileOffset == 0x00BE5BFE
            && zeroObject.StopAtRawFileOffsetExclusive == 0x00BE5BFE
            && zeroObjectWithLaterCaptures.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
            && zeroObjectWithLaterCaptures.ZeroObjectBranchTaken
            && zeroObjectWithLaterCaptures.UnreachedSecondLoadedDwordCaptureProvided
            && zeroObjectWithLaterCaptures.UnreachedIndirectTargetDwordCaptureProvided
            && zeroObjectWithLaterCaptures.UnreachedCallbackNormalCaptureProvided
            && !zeroObjectWithLaterCaptures.NormalBoundaryAdmitted
            && zeroObjectWithLaterCaptures.SelectedDestinationRawFileOffset is null);

        UnreliableReceiveHighCountCallbackPlan missingFirstLoaded =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0x1000, null, null, null, null));
        UnreliableReceiveHighCountCallbackPlan laterAfterMissingFirst =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0x1000, null, 0, 0, true));
        check("Missing first-read data preserves its captured address but blocks every later capture",
            missingFirstLoaded.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.MissingFirstLoadedDword
            && missingFirstLoaded.FirstReadAddressCaptured
            && missingFirstLoaded.FirstReadAddress == 0x1000
            && !missingFirstLoaded.FirstLoadedDwordRead
            && !missingFirstLoaded.SecondReadReached
            && laterAfterMissingFirst.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
            && laterAfterMissingFirst.UnreachedSecondLoadedDwordCaptureProvided
            && laterAfterMissingFirst.UnreachedIndirectTargetDwordCaptureProvided
            && laterAfterMissingFirst.UnreachedCallbackNormalCaptureProvided
            && !laterAfterMissingFirst.NormalBoundaryAdmitted);

        UnreliableReceiveHighCountCallbackPlan missingSecondLoaded =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0x1000, 0x2000, null, null, null));
        UnreliableReceiveHighCountCallbackPlan laterAfterMissingSecond =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0x1000, 0x2000, null, 0, true));
        check("A nonzero first DWORD requires the second read and blocks later captures when it is missing",
            missingSecondLoaded.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.MissingSecondLoadedDword
            && missingSecondLoaded.FirstReadAddress == 0x1000
            && missingSecondLoaded.CapturedFirstLoadedDword == 0x2000
            && missingSecondLoaded.SecondReadReached
            && missingSecondLoaded.SecondReadAddress == 0x2000
            && !missingSecondLoaded.SecondLoadedDwordRead
            && !missingSecondLoaded.PushOneReached
            && laterAfterMissingSecond.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
            && laterAfterMissingSecond.UnreachedIndirectTargetDwordCaptureProvided
            && laterAfterMissingSecond.UnreachedCallbackNormalCaptureProvided);

        UnreliableReceiveHighCountCallbackPlan missingTarget =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0, 0x2000, 0, null, null));
        UnreliableReceiveHighCountCallbackPlan laterAfterMissingTarget =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0, 0x2000, 0, null, true));
        check("A zero second-read base is captured normally, and PUSH 1 survives a missing target read",
            missingTarget.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.MissingIndirectTargetDword
            && missingTarget.SecondReadReached
            && missingTarget.SecondReadAddress == 0x2000
            && missingTarget.SecondLoadedDwordRead
            && missingTarget.CapturedSecondLoadedDword == 0
            && missingTarget.PushOneReached
            && missingTarget.StackPushesInInstructionOrder.SequenceEqual(new uint[] { 1 })
            && missingTarget.IndirectTargetAddress == 8
            && !missingTarget.IndirectTargetRead
            && !missingTarget.IndirectCallReached
            && laterAfterMissingTarget.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
            && laterAfterMissingTarget.UnreachedCallbackNormalCaptureProvided);

        UnreliableReceiveHighCountCallbackPlan missingCallback =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0, 0x2000, 0, 0, null));
        UnreliableReceiveHighCountCallbackPlan nonreturningCallback =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0, 0x2000, 0, 0, false));
        UnreliableReceiveHighCountCallbackPlan normalCallback =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, 0, 0x2000, 0, 0, true));
        check("The target read follows PUSH 1, and only a normal callback admits the common boundary",
            missingCallback.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.MissingCallbackNormal
            && missingCallback.PushOneReached
            && missingCallback.StackPushesInInstructionOrder.SequenceEqual(new uint[] { 1 })
            && missingCallback.IndirectTargetAddress == 8
            && missingCallback.CapturedIndirectTargetDword == 0
            && missingCallback.IndirectCallReached
            && !missingCallback.NormalBoundaryAdmitted
            && nonreturningCallback.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.CallbackDidNotReturnNormally
            && !nonreturningCallback.NormalBoundaryAdmitted
            && nonreturningCallback.SelectedDestinationRawFileOffset is null
            && normalCallback.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.CallbackReturnedNormally
            && normalCallback.CallbackReturnedNormally == true
            && normalCallback.NormalBoundaryAdmitted
            && normalCallback.SelectedDestinationRawFileOffset == 0x00BE5BFE);

        UnreliableReceiveHighCountCallbackPlan wrappedTargetAddress =
            UnreliableReceiveHighCountCallbackProjection.Project(
                new(true, uint.MaxValue, 0x2000, uint.MaxValue, null, null));
        check("The indirect target read address wraps at DWORD width",
            wrappedTargetAddress.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.MissingIndirectTargetDword
            && wrappedTargetAddress.FirstReadAddress == uint.MaxValue
            && wrappedTargetAddress.SecondReadAddress == 0x2000
            && wrappedTargetAddress.IndirectTargetAddress == 7
            && UnreliableReceiveHighCountCallbackProjection.IndirectTargetAddress(
                uint.MaxValue) == 7);
    }

    private static bool InstructionsMatch()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE5BF1, 0x00FE67F1, "8B 08"),
            (0x00BE5BF3, 0x00FE67F3, "85 C9"),
            (0x00BE5BF5, 0x00FE67F5, "74 07"),
            (0x00BE5BF7, 0x00FE67F7, "8B 01"),
            (0x00BE5BF9, 0x00FE67F9, "6A 01"),
            (0x00BE5BFB, 0x00FE67FB, "FF 50 08")
        };
        return UnreliableReceiveHighCountCallbackProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected)
            && UnreliableReceiveHighCountCallbackProjection.RawFileOffsetToVirtualAddress(
                0x00BE5BF1) == 0x00FE67F1
            && UnreliableReceiveHighCountCallbackProjection.RawFileOffsetToVirtualAddress(
                0x00BE5BFE) == 0x00FE67FE
            && UnreliableReceiveHighCountCallbackProjection.IndirectTargetAddress(
                0xFFFFFFF8) == 0;
    }
}
