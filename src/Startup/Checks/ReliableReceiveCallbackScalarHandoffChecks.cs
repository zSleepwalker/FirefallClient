using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveCallbackScalarHandoffChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample, mapped range, and exact twelve-byte window match",
            ReliableReceiveCallbackScalarHandoffProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveCallbackScalarHandoffProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveCallbackScalarHandoffProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveCallbackScalarHandoffProjection.EntryRawFileOffset
                == 0x00BE53DD
            && ReliableReceiveCallbackScalarHandoffProjection.EntryVirtualAddress
                == 0x00FE5FDD
            && ReliableReceiveCallbackScalarHandoffProjection.HelperCallRawFileOffset
                == 0x00BE53E1
            && ReliableReceiveCallbackScalarHandoffProjection.HelperCallVirtualAddress
                == 0x00FE5FE1
            && ReliableReceiveCallbackScalarHandoffProjection.HelperCallTargetAddressFromIda
                == 0x00BD58E0
            && ReliableReceiveCallbackScalarHandoffProjection.ExplicitPushCount == 0
            && ReliableReceiveCallbackScalarHandoffProjection.HelperCallContinuationRawFileOffset
                == 0x00BE53E6
            && ReliableReceiveCallbackScalarHandoffProjection.HelperCallContinuationVirtualAddress
                == 0x00FE5FE6
            && ReliableReceiveCallbackScalarHandoffProjection.EndRawFileOffsetExclusive
                == 0x00BE53E9
            && ReliableReceiveCallbackScalarHandoffProjection.EndVirtualAddressExclusive
                == 0x00FE5FE9
            && ReliableReceiveCallbackScalarHandoffProjection.StackCleanupAmount == 4
            && ReliableReceiveCallbackScalarHandoffProjection.CapturedWindowBytes
                == "8B D0 8A CB E8 FA 04 FF FF 83 C4 04"
            && ReliableReceiveCallbackScalarHandoffProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 12
            && InstructionsMatch());

        ReliableReceiveCallbackScalarHandoffPlan normal =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(true, 0x12345678, 0xABCDEF00, 0x5A, true));
        check("A normal helper handoff passes full EDX and only replaces ECX's low byte",
            normal.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.HelperReturnedNormally
            && normal.PriorCallbackReturnedNormally == true
            && normal.CapturedEdx == 0x12345678
            && normal.CapturedCl == 0x5A
            && normal.ProjectedEcx == 0xABCDEF5A
            && normal.HelperCallReached
            && normal.HelperReturnedNormally == true
            && normal.StackCleanupBytes == 4
            && normal.SelectedDestinationRawFileOffset == 0x00BE53E9
            && normal.StopAtRawFileOffsetExclusive == 0x00BE53E9);

        ReliableReceiveCallbackScalarHandoffPlan unknownEcxUpper =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(true, 0, null, 0xA7, true));
        check("Unknown ECX upper bits do not block the call while CL remains known",
            unknownEcxUpper.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.HelperReturnedNormally
            && unknownEcxUpper.CapturedEdx == 0
            && unknownEcxUpper.CapturedCl == 0xA7
            && unknownEcxUpper.ProjectedEcx is null
            && unknownEcxUpper.HelperCallReached
            && unknownEcxUpper.StackCleanupBytes == 4);

        ReliableReceiveCallbackScalarHandoffPlan helperDidNotReturn =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(true, 0xFFFFFFFF, 0x12345600, 0xFF, false));
        check("A nonreturning opaque helper stops before cleanup",
            helperDidNotReturn.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.HelperDidNotReturnNormally
            && helperDidNotReturn.CapturedEdx == uint.MaxValue
            && helperDidNotReturn.CapturedCl == 0xFF
            && helperDidNotReturn.ProjectedEcx == 0x123456FF
            && helperDidNotReturn.HelperCallReached
            && helperDidNotReturn.HelperReturnedNormally == false
            && helperDidNotReturn.StackCleanupBytes is null
            && helperDidNotReturn.SelectedDestinationRawFileOffset is null
            && helperDidNotReturn.StopAtRawFileOffsetExclusive == 0x00BE53E6);

        ReliableReceiveCallbackScalarHandoffPlan missingEax =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(true, null, null, null, null));
        ReliableReceiveCallbackScalarHandoffPlan missingBl =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(true, 7, null, null, null));
        ReliableReceiveCallbackScalarHandoffPlan missingHelperReturn =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(true, 7, null, 2, null));
        check("Missing captures stop at their first unresolved instruction",
            missingEax.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.MissingPriorCallbackEax
            && missingEax.CapturedEdx is null
            && missingEax.StopAtRawFileOffsetExclusive == 0x00BE53DD
            && missingBl.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.MissingRetainedBl
            && missingBl.CapturedEdx == 7
            && missingBl.CapturedCl is null
            && missingBl.StopAtRawFileOffsetExclusive == 0x00BE53DF
            && missingHelperReturn.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.MissingHelperReturn
            && missingHelperReturn.HelperCallReached
            && missingHelperReturn.StackCleanupBytes is null
            && missingHelperReturn.StopAtRawFileOffsetExclusive == 0x00BE53E6);

        ReliableReceiveCallbackScalarHandoffPlan missingPrior =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(null, 0, 0xFFFFFFFF, 0, true));
        ReliableReceiveCallbackScalarHandoffPlan priorDidNotReturn =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(false, null, null, null, null));
        ReliableReceiveCallbackScalarHandoffPlan unreachedNativeZero =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(false, 0, 0, 0, true));
        check("Prior callback normal return gates all captures, including native zero",
            missingPrior.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.UnreachedCaptureProvided
            && missingPrior.CapturedEdx is null
            && !missingPrior.HelperCallReached
            && missingPrior.StopAtRawFileOffsetExclusive == 0x00BE53DD
            && priorDidNotReturn.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.PriorCallbackDidNotReturnNormally
            && priorDidNotReturn.CapturedEdx is null
            && unreachedNativeZero.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.UnreachedCaptureProvided
            && unreachedNativeZero.CapturedEdx is null
            && !unreachedNativeZero.HelperCallReached
            && unreachedNativeZero.StackCleanupBytes is null);

        ReliableReceiveCallbackScalarHandoffPlan zeroEax =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(true, 0, 0xFFFFFFFF, 0x34, true));
        ReliableReceiveCallbackScalarHandoffPlan unreachedHelperReturn =
            ReliableReceiveCallbackScalarHandoffProjection.Project(
                new(true, 0, null, null, true));
        check("A reached native zero is passed through unchanged, while an unreached helper result is rejected",
            zeroEax.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.HelperReturnedNormally
            && zeroEax.CapturedEdx == 0
            && zeroEax.CapturedCl == 0x34
            && zeroEax.ProjectedEcx == 0xFFFFFF34
            && unreachedHelperReturn.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.UnreachedCaptureProvided
            && unreachedHelperReturn.CapturedEdx == 0
            && !unreachedHelperReturn.HelperCallReached
            && unreachedHelperReturn.StackCleanupBytes is null
            && unreachedHelperReturn.StopAtRawFileOffsetExclusive == 0x00BE53DF);
    }

    private static bool InstructionsMatch()
    {
        var expected =
            new (uint Raw, uint Virtual, string Bytes)[]
            {
                (0x00BE53DD, 0x00FE5FDD, "8B D0"),
                (0x00BE53DF, 0x00FE5FDF, "8A CB"),
                (0x00BE53E1, 0x00FE5FE1, "E8 FA 04 FF FF"),
                (0x00BE53E6, 0x00FE5FE6, "83 C4 04")
            };
        return ReliableReceiveCallbackScalarHandoffProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected)
            && ReliableReceiveCallbackScalarHandoffProjection.RawFileOffsetToVirtualAddress(
                0x00BE53E9) == 0x00FE5FE9
            && ReliableReceiveCallbackScalarHandoffProjection.ReplaceLowByte(
                0xABCDEF00, 0x5A) == 0xABCDEF5A;
    }
}
