using Firefall.Reconstruction.Startup;

internal static class FrameHandoffHelperEntryChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE, raw/VA boundaries, full instruction bytes, ordering, and branches are exact",
            FrameHandoffHelperEntryProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffHelperEntryProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffHelperEntryProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffHelperEntryProjection.EntryRawFileOffset == 0x00BE64A0
            && FrameHandoffHelperEntryProjection.EntryVirtualAddress == 0x00FE70A0
            && FrameHandoffHelperEntryProjection.EndRawFileOffsetExclusive == 0x00BE64C5
            && FrameHandoffHelperEntryProjection.RawFileOffsetToVirtualAddress(0x00BE64C5)
                == 0x00FE70C5
            && FrameHandoffHelperEntryProjection.CapturedWindowBytes
                == "55 8B EC 56 8B F1 8B 46 0C 8B 4E 10 03 C1 A8 03 75 13 "
                    + "8D 41 04 C1 E8 02 39 46 08 77 08 51 8B CE E8 8B 06 00 00"
            && FrameHandoffHelperEntryProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x25
            && InstructionEvidenceMatches());

        check("Both short branches and the relative call target the verified raw boundaries",
            ShortBranchTarget(
                FrameHandoffHelperEntryProjection.FirstBranchRawFileOffset,
                0x13) == FrameHandoffHelperEntryProjection.FirstBranchDestinationRawFileOffset
            && ShortBranchTarget(
                FrameHandoffHelperEntryProjection.SecondBranchRawFileOffset,
                0x08) == FrameHandoffHelperEntryProjection.SecondBranchDestinationRawFileOffset
            && FrameHandoffHelperEntryProjection.RelativeCallTarget(
                FrameHandoffHelperEntryProjection.HelperCallRawFileOffset,
                0x68B) == FrameHandoffHelperEntryProjection.HelperCallTargetRawFileOffset
            && FrameHandoffHelperEntryProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffHelperEntryProjection.HelperCallTargetRawFileOffset) == 0x00FE7750
            && FrameHandoffHelperEntryProjection.ReceiverAddress(0, 0x0C) == 0x0C
            && FrameHandoffHelperEntryProjection.ReceiverAddress(uint.MaxValue - 0x0B, 0x0C) == 0);

        FrameHandoffHelperEntryPlan missingFirst =
            FrameHandoffHelperEntryProjection.Project(new(0, 0, null, 0, 0, true));
        FrameHandoffHelperEntryPlan missingSecond =
            FrameHandoffHelperEntryProjection.Project(new(0x1234, 0x5678, 0, null, 0, true));
        check("Missing first reads preserve the independent captures and block later-stage values",
            missingFirst.Outcome
                == FrameHandoffHelperEntryOutcome.MissingReceiverPlus0CCapture
            && missingFirst.Capture.EntryFrameEbp == 0
            && missingFirst.Capture.EntryReceiverEcx == 0
            && missingFirst.Capture.ReceiverPlus10 == 0
            && missingFirst.Capture.ReceiverPlus08 == 0
            && missingFirst.ReceiverPlus0CReadAddress == 0x0C
            && missingFirst.ReceiverPlus0CReadValue is null
            && missingFirst.ReceiverPlus0CReadReached
            && !missingFirst.ReceiverPlus0CCaptured
            && missingFirst.ReceiverPlus10ReadAddress is null
            && missingFirst.ReceiverPlus10ReadValue is null
            && !missingFirst.ReceiverPlus10ReadReached
            && !missingFirst.ReceiverPlus10Captured
            && missingFirst.UnreachedReceiverPlus10CaptureProvided
            && missingFirst.ReceiverPlus08ReadAddress is null
            && missingFirst.ReceiverPlus08ReadValue is null
            && missingFirst.WrappingSum is null
            && !missingFirst.ReceiverPlus08ReadReached
            && !missingFirst.ReceiverPlus08Captured
            && missingFirst.UnreachedReceiverPlus08CaptureProvided
            && missingFirst.UnreachedHelperReturnedNormallyCaptureProvided
            && missingFirst.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperEntryProjection.FirstReceiverReadRawFileOffset
            && missingSecond.Outcome
                == FrameHandoffHelperEntryOutcome.MissingReceiverPlus10Capture
            && missingSecond.Capture.EntryFrameEbp == 0x1234
            && missingSecond.Capture.EntryReceiverEcx == 0x5678
            && missingSecond.Capture.ReceiverPlus0C == 0
            && missingSecond.ReceiverPlus0CReadAddress == 0x5684
            && missingSecond.ReceiverPlus0CReadValue == 0
            && missingSecond.ReceiverPlus0CReadReached
            && missingSecond.ReceiverPlus0CCaptured
            && missingSecond.ReceiverPlus10ReadAddress == 0x5688
            && missingSecond.ReceiverPlus10ReadValue is null
            && missingSecond.ReceiverPlus10ReadReached
            && !missingSecond.ReceiverPlus10Captured
            && !missingSecond.UnreachedReceiverPlus10CaptureProvided
            && !missingSecond.ReceiverPlus08ReadReached
            && missingSecond.WrappingSum is null
            && missingSecond.UnreachedReceiverPlus08CaptureProvided
            && missingSecond.UnreachedHelperReturnedNormallyCaptureProvided
            && missingSecond.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperEntryProjection.SecondReceiverReadRawFileOffset);

        FrameHandoffHelperEntryPlan earlyTestBranch =
            FrameHandoffHelperEntryProjection.Project(new(0, 0, 1, 0, 0xCAFE, true));
        check("A nonzero low-two-bit test branches before the +8 read, push, receiver reload, and call",
            earlyTestBranch.Outcome
                == FrameHandoffHelperEntryOutcome.LowTwoSumBitsNonzero
            && earlyTestBranch.WrappingSum == 1
            && earlyTestBranch.LowTwoSumBits == 1
            && earlyTestBranch.ReceiverPlus0CReadReached
            && earlyTestBranch.ReceiverPlus0CCaptured
            && earlyTestBranch.ReceiverPlus0CReadValue == 1
            && earlyTestBranch.ReceiverPlus10ReadReached
            && earlyTestBranch.ReceiverPlus10Captured
            && earlyTestBranch.ReceiverPlus10ReadValue == 0
            && !earlyTestBranch.ReceiverPlus08ReadReached
            && !earlyTestBranch.ReceiverPlus08Captured
            && earlyTestBranch.UnreachedReceiverPlus08CaptureProvided
            && earlyTestBranch.UnreachedHelperReturnedNormallyCaptureProvided
            && !earlyTestBranch.PayloadValuePushed
            && !earlyTestBranch.ReceiverRestoredToEcx
            && !earlyTestBranch.HelperCallReached
            && earlyTestBranch.SelectedDestinationRawFileOffset
                == FrameHandoffHelperEntryProjection.FirstBranchDestinationRawFileOffset
            && earlyTestBranch.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperEntryProjection.FirstBranchDestinationRawFileOffset);

        FrameHandoffHelperEntryPlan missingPlus08 =
            FrameHandoffHelperEntryProjection.Project(new(0, 0, uint.MaxValue, 1, null, true));
        FrameHandoffHelperEntryPlan unsignedGreater =
            FrameHandoffHelperEntryProjection.Project(new(0, 0, uint.MaxValue, 1, 2, true));
        check("DWORD sum wraps; the rounded length is (captured+4) wrapping then logical SHR 2",
            missingPlus08.Outcome
                == FrameHandoffHelperEntryOutcome.MissingReceiverPlus08Capture
            && missingPlus08.WrappingSum == 0
            && missingPlus08.LowTwoSumBits == 0
            && missingPlus08.WrappingPlusFour == 5
            && missingPlus08.RoundedLength == 1
            && missingPlus08.ReceiverPlus0CReadReached
            && missingPlus08.ReceiverPlus0CCaptured
            && missingPlus08.ReceiverPlus0CReadAddress == 0x0C
            && missingPlus08.ReceiverPlus0CReadValue == uint.MaxValue
            && missingPlus08.ReceiverPlus10ReadReached
            && missingPlus08.ReceiverPlus10Captured
            && missingPlus08.ReceiverPlus10ReadAddress == 0x10
            && missingPlus08.ReceiverPlus10ReadValue == 1
            && missingPlus08.ReceiverPlus08ReadReached
            && !missingPlus08.ReceiverPlus08Captured
            && missingPlus08.ReceiverPlus08ReadAddress == 0x08
            && missingPlus08.ReceiverPlus08ReadValue is null
            && missingPlus08.UnreachedHelperReturnedNormallyCaptureProvided
            && missingPlus08.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperEntryProjection.ReceiverPlus08CompareRawFileOffset
            && unsignedGreater.Outcome
                == FrameHandoffHelperEntryOutcome.ReceiverPlus08GreaterThanRoundedLength
            && unsignedGreater.WrappingSum == 0
            && unsignedGreater.ReceiverPlus08ReadReached
            && unsignedGreater.ReceiverPlus08Captured
            && unsignedGreater.ReceiverPlus08ReadValue == 2
            && unsignedGreater.UnreachedHelperReturnedNormallyCaptureProvided
            && !unsignedGreater.PayloadValuePushed
            && !unsignedGreater.HelperCallReached
            && unsignedGreater.SelectedDestinationRawFileOffset
                == FrameHandoffHelperEntryProjection.SecondBranchDestinationRawFileOffset
            && unsignedGreater.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperEntryProjection.SecondBranchDestinationRawFileOffset);

        FrameHandoffHelperEntryPlan missingCallReturn =
            FrameHandoffHelperEntryProjection.Project(new(0x1111, 0, uint.MaxValue, 1, 1, null));
        FrameHandoffHelperEntryPlan nonReturningCall =
            FrameHandoffHelperEntryProjection.Project(new(0x2222, 0, uint.MaxValue, 1, 1, false));
        FrameHandoffHelperEntryPlan normalCall =
            FrameHandoffHelperEntryProjection.Project(new(0, 0, uint.MaxValue, 1, 1, true));
        check("Only the fallthrough pushes captured +10, restores entry ECX, and reaches the helper call",
            missingCallReturn.Outcome
                == FrameHandoffHelperEntryOutcome.MissingHelperReturnCapture
            && missingCallReturn.ReceiverPlus08ReadReached
                && missingCallReturn.ReceiverPlus08Captured
                && !missingCallReturn.UnreachedReceiverPlus08CaptureProvided
            && !missingCallReturn.UnreachedHelperReturnedNormallyCaptureProvided
            && missingCallReturn.PayloadValuePushed
            && missingCallReturn.PushedReceiverPlus10Value == 1
            && missingCallReturn.ReceiverRestoredToEcx
            && missingCallReturn.ReceiverPassedInEcx == 0
            && missingCallReturn.HelperCallReached
            && missingCallReturn.SelectedDestinationRawFileOffset is null
            && missingCallReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperEntryProjection.HelperCallRawFileOffset
            && nonReturningCall.Outcome
                == FrameHandoffHelperEntryOutcome.HelperDidNotReturnNormally
            && nonReturningCall.PayloadValuePushed
            && nonReturningCall.PushedReceiverPlus10Value == 1
            && nonReturningCall.ReceiverPassedInEcx == 0
            && nonReturningCall.HelperCallReached
            && nonReturningCall.HelperReturnedNormally == false
            && nonReturningCall.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperEntryProjection.HelperCallRawFileOffset
            && normalCall.Outcome == FrameHandoffHelperEntryOutcome.HelperReturnedNormally
            && normalCall.Capture.EntryFrameEbp == 0
            && normalCall.PayloadValuePushed
            && normalCall.PushedReceiverPlus10Value == 1
            && normalCall.ReceiverRestoredToEcx
            && normalCall.ReceiverPassedInEcx == 0
            && normalCall.HelperCallReached
            && normalCall.HelperReturnedNormally == true
            && normalCall.SelectedDestinationRawFileOffset
                == FrameHandoffHelperEntryProjection.EndRawFileOffsetExclusive
            && normalCall.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperEntryProjection.EndRawFileOffsetExclusive);

        FrameHandoffHelperEntryPlan zeroValues =
            FrameHandoffHelperEntryProjection.Project(new(0, 0, 0, 0, 0, true));
        FrameHandoffHelperEntryPlan wrappingIncrement =
            FrameHandoffHelperEntryProjection.Project(new(0, 0, 4, 0xFFFFFFFC, 0, true));
        check("Zero addresses are valid and the unsigned capacity comparison includes equality",
            zeroValues.Outcome == FrameHandoffHelperEntryOutcome.HelperReturnedNormally
            && zeroValues.WrappingSum == 0
            && zeroValues.RoundedLength == 1
            && zeroValues.ReceiverPlus08ReadReached
            && zeroValues.ReceiverPlus08Captured
            && zeroValues.HelperCallReached
            && zeroValues.ReceiverPassedInEcx == 0
            && wrappingIncrement.Outcome
                == FrameHandoffHelperEntryOutcome.HelperReturnedNormally
            && wrappingIncrement.WrappingPlusFour == 0
            && wrappingIncrement.RoundedLength == 0
            && wrappingIncrement.ReceiverPlus08ReadReached
            && !wrappingIncrement.UnreachedHelperReturnedNormallyCaptureProvided);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE64A0, 0x00FE70A0, "55 8B EC 56 8B F1", null, null),
            (0x00BE64A6, 0x00FE70A6, "8B 46 0C", null, null),
            (0x00BE64A9, 0x00FE70A9, "8B 4E 10", null, null),
            (0x00BE64AC, 0x00FE70AC, "03 C1 A8 03", null, null),
            (0x00BE64B0, 0x00FE70B0, "75 13", 0x00BE64C5, 0x00FE70C5),
            (0x00BE64B2, 0x00FE70B2, "8D 41 04 C1 E8 02", null, null),
            (0x00BE64B8, 0x00FE70B8, "39 46 08", null, null),
            (0x00BE64BB, 0x00FE70BB, "77 08", 0x00BE64C5, 0x00FE70C5),
            (0x00BE64BD, 0x00FE70BD, "51", null, null),
            (0x00BE64BE, 0x00FE70BE, "8B CE", null, null),
            (0x00BE64C0, 0x00FE70C0, "E8 8B 06 00 00", 0x00BE6B50, 0x00FE7750)
        };
        return FrameHandoffHelperEntryProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
