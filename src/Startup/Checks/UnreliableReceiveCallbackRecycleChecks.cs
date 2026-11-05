using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveCallbackRecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE maps the exact 20-byte raw window through the first-call return boundary",
            UnreliableReceiveCallbackRecycleProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveCallbackRecycleProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveCallbackRecycleProjection.EntryRawFileOffset == 0x00BE5B64
            && UnreliableReceiveCallbackRecycleProjection.EntryVirtualAddress == 0x00FE6764
            && UnreliableReceiveCallbackRecycleProjection.RawFileOffsetToVirtualAddress(
                UnreliableReceiveCallbackRecycleProjection.RecycleHelperCallRawFileOffset)
                == 0x00FE6773
            && UnreliableReceiveCallbackRecycleProjection.RecycleHelperTargetRawFileOffset
                == 0x00BD5C60
            && UnreliableReceiveCallbackRecycleProjection.RecycleHelperTargetVirtualAddress
                == 0x00FD6860
            && UnreliableReceiveCallbackRecycleProjection.RawFileOffsetToVirtualAddress(
                UnreliableReceiveCallbackRecycleProjection.NormalCallBoundaryRawFileOffset)
                == 0x00FE6778
            && UnreliableReceiveCallbackRecycleProjection.EndRawFileOffsetExclusive
                == 0x00BE5B78
            && UnreliableReceiveCallbackRecycleProjection.CapturedWindowBytes
                == "8B 0D 84 FF DF 01 83 C4 10 8B F0 89 75 08 53 E8 E8 00 FF FF"
            && UnreliableReceiveCallbackRecycleProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 20
            && InstructionsMatch());

        UnreliableReceiveCallbackRecyclePlan normalZeroValues =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(true, 0xFFFFFFF8, 0xAABBCCDD, 0, 0, true));
        check("Captured native zero values remain valid and ECX survives cleanup, copy, frame write, and push",
            normalZeroValues.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.RecycleHelperReturnedNormally
            && normalZeroValues.GlobalDwordRead
            && normalZeroValues.CapturedGlobalDword == 0
            && normalZeroValues.ProjectedEcx == 0
            && normalZeroValues.StackCleanupReached
            && normalZeroValues.StackCleanupBytes == 0x10
            && normalZeroValues.CallbackEaxCopied
            && normalZeroValues.CapturedCallbackEax == 0
            && normalZeroValues.FrameDwordWriteReached
            && normalZeroValues.FrameDwordAddress == 0
            && normalZeroValues.FrameDwordWritten == 0
            && normalZeroValues.PayloadPushReached
            && normalZeroValues.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0xAABBCCDD })
            && normalZeroValues.RecycleHelperCallReached
            && normalZeroValues.CallEcx == 0
            && normalZeroValues.RecycleHelperReturnedNormally == true
            && normalZeroValues.NormalCallBoundaryReached
            && normalZeroValues.SelectedDestinationRawFileOffset == 0x00BE5B78
            && normalZeroValues.StopAtRawFileOffsetExclusive == 0x00BE5B78);

        UnreliableReceiveCallbackRecyclePlan missingCallbackEax =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(true, 0x1000, 0x12345678, 0xCAFEBABE, null, null));
        UnreliableReceiveCallbackRecyclePlan missingCallbackEaxWithLaterCapture =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(true, 0x1000, 0x12345678, 0xCAFEBABE, null, true));
        check("A missing callback EAX capture preserves the first global read and cleanup but prevents later effects",
            missingCallbackEax.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.MissingCallbackEaxAtCopy
            && missingCallbackEax.GlobalDwordRead
            && missingCallbackEax.CapturedGlobalDword == 0xCAFEBABE
            && missingCallbackEax.ProjectedEcx == 0xCAFEBABE
            && missingCallbackEax.StackCleanupReached
            && missingCallbackEax.StackCleanupBytes == 0x10
            && !missingCallbackEax.CallbackEaxCopied
            && !missingCallbackEax.FrameDwordWriteReached
            && missingCallbackEax.FrameDwordAddress is null
            && !missingCallbackEax.PayloadPushReached
            && missingCallbackEax.StackPushesInInstructionOrder.IsEmpty
            && !missingCallbackEax.RecycleHelperCallReached
            && !missingCallbackEax.NormalCallBoundaryReached
            && missingCallbackEax.StopAtRawFileOffsetExclusive == 0x00BE5B6D
            && missingCallbackEaxWithLaterCapture.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.UnreachedCaptureProvided
            && missingCallbackEaxWithLaterCapture.UnreachedRecycleHelperReturnCaptureProvided
            && missingCallbackEaxWithLaterCapture.StackCleanupReached
            && !missingCallbackEaxWithLaterCapture.FrameDwordWriteReached
            && !missingCallbackEaxWithLaterCapture.RecycleHelperCallReached);

        UnreliableReceiveCallbackRecyclePlan missingPriorGate =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(null, 0x1000, 0x2000, 0, 0, true));
        UnreliableReceiveCallbackRecyclePlan priorDidNotReturn =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(false, 0x1000, 0x2000, null, null, null));
        UnreliableReceiveCallbackRecyclePlan unreachedAfterPrior =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(false, 0x1000, 0x2000, 1, 2, true));
        check("The prior callback normal-return gate blocks the global read and every later instruction",
            missingPriorGate.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.UnreachedCaptureProvided
            && !missingPriorGate.GlobalDwordRead
            && !missingPriorGate.StackCleanupReached
            && !missingPriorGate.RecycleHelperCallReached
            && missingPriorGate.UnreachedGlobalDwordCaptureProvided
            && missingPriorGate.UnreachedCallbackEaxCaptureProvided
            && missingPriorGate.UnreachedRecycleHelperReturnCaptureProvided
            && missingPriorGate.StopAtRawFileOffsetExclusive == 0x00BE5B64
            && priorDidNotReturn.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.PriorCallbackDidNotReturnNormally
            && !priorDidNotReturn.GlobalDwordRead
            && !priorDidNotReturn.StackCleanupReached
            && unreachedAfterPrior.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.UnreachedCaptureProvided
            && unreachedAfterPrior.UnreachedGlobalDwordCaptureProvided
            && unreachedAfterPrior.UnreachedCallbackEaxCaptureProvided
            && unreachedAfterPrior.UnreachedRecycleHelperReturnCaptureProvided
            && !unreachedAfterPrior.GlobalDwordRead);

        UnreliableReceiveCallbackRecyclePlan missingGlobal =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(true, 0x1000, 0x2000, null, null, null));
        UnreliableReceiveCallbackRecyclePlan missingGlobalWithLaterCapture =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(true, 0x1000, 0x2000, null, 0, true));
        check("A missing first global read blocks cleanup and marks callback and helper captures unreached",
            missingGlobal.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.MissingGlobalDwordAtFirstRead
            && !missingGlobal.GlobalDwordRead
            && !missingGlobal.StackCleanupReached
            && !missingGlobal.CallbackEaxCopied
            && !missingGlobal.RecycleHelperCallReached
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x00BE5B64
            && missingGlobalWithLaterCapture.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.UnreachedCaptureProvided
            && missingGlobalWithLaterCapture.UnreachedCallbackEaxCaptureProvided
            && missingGlobalWithLaterCapture.UnreachedRecycleHelperReturnCaptureProvided
            && !missingGlobalWithLaterCapture.StackCleanupReached);

        UnreliableReceiveCallbackRecyclePlan missingHelperReturn =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(true, 0x2000, 0x33445566, 0x778899AA, 0x11223344, null));
        UnreliableReceiveCallbackRecyclePlan helperDidNotReturn =
            UnreliableReceiveCallbackRecycleProjection.Project(
                new(true, 0x2000, 0x33445566, 0x778899AA, 0x11223344, false));
        check("Helper status distinguishes an unreached normal-return boundary without interpreting later branch code",
            missingHelperReturn.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.MissingRecycleHelperReturn
            && missingHelperReturn.RecycleHelperCallReached
            && missingHelperReturn.CallEcx == 0x778899AA
            && missingHelperReturn.NormalCallBoundaryReached == false
            && missingHelperReturn.SelectedDestinationRawFileOffset is null
            && missingHelperReturn.StopAtRawFileOffsetExclusive == 0x00BE5B73
            && helperDidNotReturn.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.RecycleHelperDidNotReturnNormally
            && helperDidNotReturn.RecycleHelperReturnedNormally == false
            && helperDidNotReturn.RecycleHelperCallReached
            && !helperDidNotReturn.NormalCallBoundaryReached
            && helperDidNotReturn.StopAtRawFileOffsetExclusive == 0x00BE5B73);
    }

    private static bool InstructionsMatch()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE5B64, 0x00FE6764, "8B 0D 84 FF DF 01"),
            (0x00BE5B6A, 0x00FE676A, "83 C4 10"),
            (0x00BE5B6D, 0x00FE676D, "8B F0"),
            (0x00BE5B6F, 0x00FE676F, "89 75 08"),
            (0x00BE5B72, 0x00FE6772, "53"),
            (0x00BE5B73, 0x00FE6773, "E8 E8 00 FF FF")
        };
        return UnreliableReceiveCallbackRecycleProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected)
            && UnreliableReceiveCallbackRecycleProjection.RawFileOffsetToVirtualAddress(
                0x00BE5B78) == 0x00FE6778
            && UnreliableReceiveCallbackRecycleProjection.RawFileOffsetToVirtualAddress(
                0x00BD5C60) == 0x00FD6860
            && UnreliableReceiveCallbackRecycleProjection.FrameDwordAddress(
                0xFFFFFFF8) == 0;
    }
}
