using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolPopChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Pool-pop anchors match the pinned sample, raw bytes, and raw-offset-to-VA mapping",
            UiShaderSourcePoolPopProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolPopProjection.EntryRawFileOffset == 0x12CDAE0
            && UiShaderSourcePoolPopProjection.EntryEndRawFileOffsetExclusive == 0x12CDB3E
            && UiShaderSourcePoolPopProjection.EntryRawFileOffset
                + UiShaderSourcePoolPopProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CE6E0
            && UiShaderSourcePoolPopProjection.EntryEndRawFileOffsetExclusive
                + UiShaderSourcePoolPopProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CE73E
            && UiShaderSourcePoolPopProjection.CapturedFunctionBytes
                == "55 8B EC 83 E4 F8 83 EC 0C 53 56 8B C1 57 89 44 24 0C "
                    + "8B 48 04 8B 30 89 74 24 10 85 F6 74 36 8B 06 33 FF "
                    + "8B D1 33 DB 0B FE 89 54 24 14 0B D8 41 8B C7 8B 74 "
                    + "24 0C F0 0F C7 0E 8B 74 24 10 3B C7 8B 44 24 0C 75 "
                    + "CC 3B 54 24 14 75 C6 8B C6 5F 5E 5B 8B E5 5D C3 "
                    + "5F 5E 33 C0 5B 8B E5 5D C3"
            && UiShaderSourcePoolPopProjection.PairHeadOffset == 0
            && UiShaderSourcePoolPopProjection.PairHighTagOffset == 4);

        check("Pool-pop evidence preserves the separate captures, locked exchange, and result comparisons",
            UiShaderSourcePoolPopProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset)[]
                {
                    (0x12CDAF2, "8B 48 04", null),
                    (0x12CDAF5, "8B 30", null),
                    (0x12CDAFB, "85 F6", null),
                    (0x12CDAFD, "74 36", 0x12CDB35),
                    (0x12CDAFF, "8B 06", null),
                    (0x12CDB03, "8B D1", null),
                    (0x12CDB07, "0B FE", null),
                    (0x12CDB0D, "0B D8", null),
                    (0x12CDB0F, "41", null),
                    (0x12CDB10, "8B C7", null),
                    (0x12CDB12, "8B 74 24 0C", null),
                    (0x12CDB16, "F0 0F C7 0E", null),
                    (0x12CDB1E, "3B C7", null),
                    (0x12CDB20, "8B 44 24 0C", null),
                    (0x12CDB24, "75 CC", 0x12CDAF2),
                    (0x12CDB26, "3B 54 24 14", null),
                    (0x12CDB2A, "75 C6", 0x12CDAF2),
                    (0x12CDB2C, "8B C6", null),
                    (0x12CDB35, "5F 5E 33 C0 5B 8B E5 5D C3", null)
                })
            && UiShaderSourcePoolPopProjection.EmptyHeadBranchRawFileOffset + 2 + 0x36
                == UiShaderSourcePoolPopProjection.EmptyHeadReturnRawFileOffset
            && UiShaderSourcePoolPopProjection.CompareReturnedHeadBranchRawFileOffset + 2 - 0x34
                == 0x12CDAF2
            && UiShaderSourcePoolPopProjection.CompareReturnedHighBranchRawFileOffset + 2 - 0x3A
                == 0x12CDAF2);

        UiShaderSourcePoolPopPlan empty =
            UiShaderSourcePoolPopProjection.Project(
                new UiShaderSourcePoolPopCapture(
                    ReceiverPairPointer: 0x10203040,
                    CapturedHighTag: 17,
                    CapturedHead: 0,
                    CapturedNextHead: null,
                    CompareExchangeReturnedHead: null,
                    CompareExchangeReturnedHighTag: null));
        check("Null captured head returns zero without an exchange and preserves the capture",
            empty.Outcome == UiShaderSourcePoolPopOutcome.EmptyCapturedHead
            && empty.Capture.CapturedHead == 0
            && empty.ReturnValue == 0
            && empty.ExpectedPair is null
            && empty.ReplacementPair is null
            && empty.CompareExchangeReturnedPair is null
            && empty.Actions.SequenceEqual(new[]
            {
                new UiShaderSourcePoolPopAction(
                    UiShaderSourcePoolPopActionKind.ReturnZeroWithoutExchange,
                    0x12CDAFD,
                    BranchTargetRawFileOffset: 0x12CDB35)
            }));

        UiShaderSourcePoolPopPlan returnedHead =
            UiShaderSourcePoolPopProjection.Project(
                new UiShaderSourcePoolPopCapture(
                    ReceiverPairPointer: 0x10203040,
                    CapturedHighTag: 0xAABBCCDD,
                    CapturedHead: 0x55667788,
                    CapturedNextHead: 0x99AABBCC,
                    CompareExchangeReturnedHead: 0x55667788,
                    CompareExchangeReturnedHighTag: 0xAABBCCDD));
        check("Matching observed compare-exchange halves return the separately captured head",
            returnedHead.Outcome == UiShaderSourcePoolPopOutcome.ReturnedCapturedHead
            && returnedHead.ExpectedPair
                == new UiShaderSourcePoolPopPair(0x55667788, 0xAABBCCDD)
            && returnedHead.ReplacementPair
                == new UiShaderSourcePoolPopPair(0x99AABBCC, 0xAABBCCDD + 1)
            && returnedHead.CompareExchangeReturnedPair == returnedHead.ExpectedPair
            && returnedHead.ReturnValue == 0x55667788
            && returnedHead.Actions.SequenceEqual(new[]
            {
                new UiShaderSourcePoolPopAction(
                    UiShaderSourcePoolPopActionKind.ReadCapturedHeadLink,
                    0x12CDAFF,
                    0x55667788),
                new UiShaderSourcePoolPopAction(
                    UiShaderSourcePoolPopActionKind.AttemptLockedCompareExchange,
                    0x12CDB16,
                    0x10203040,
                    new UiShaderSourcePoolPopPair(0x55667788, 0xAABBCCDD),
                    new UiShaderSourcePoolPopPair(0x99AABBCC, 0xAABBCCDD + 1)),
                new UiShaderSourcePoolPopAction(
                    UiShaderSourcePoolPopActionKind.ReturnCapturedHead,
                    0x12CDB2C,
                    0x55667788)
            }));

        UiShaderSourcePoolPopPlan lowHalfMismatch =
            UiShaderSourcePoolPopProjection.Project(
                new UiShaderSourcePoolPopCapture(
                    ReceiverPairPointer: 0x10203040,
                    CapturedHighTag: 4,
                    CapturedHead: 0x11112222,
                    CapturedNextHead: 0x33334444,
                    CompareExchangeReturnedHead: 0x55556666,
                    CompareExchangeReturnedHighTag: 4));
        UiShaderSourcePoolPopPlan highHalfMismatch =
            UiShaderSourcePoolPopProjection.Project(
                new UiShaderSourcePoolPopCapture(
                    ReceiverPairPointer: 0x10203040,
                    CapturedHighTag: 4,
                    CapturedHead: 0x11112222,
                    CapturedNextHead: 0x33334444,
                    CompareExchangeReturnedHead: 0x11112222,
                    CompareExchangeReturnedHighTag: 5));
        check("Either observed-half mismatch stops at the raw recapture boundary without simulating a retry",
            lowHalfMismatch.Outcome == UiShaderSourcePoolPopOutcome.RetryBoundary
            && lowHalfMismatch.ReturnValue is null
            && lowHalfMismatch.Actions[^1]
                == new UiShaderSourcePoolPopAction(
                    UiShaderSourcePoolPopActionKind.StopAtUnmodeledRetryBoundary,
                    0x12CDB24,
                    BranchTargetRawFileOffset: 0x12CDAF2)
            && highHalfMismatch.Outcome == UiShaderSourcePoolPopOutcome.RetryBoundary
            && highHalfMismatch.ReturnValue is null
            && highHalfMismatch.Actions[^1]
                == new UiShaderSourcePoolPopAction(
                    UiShaderSourcePoolPopActionKind.StopAtUnmodeledRetryBoundary,
                    0x12CDB2A,
                    BranchTargetRawFileOffset: 0x12CDAF2));

        UiShaderSourcePoolPopPlan wrappingTag =
            UiShaderSourcePoolPopProjection.Project(
                new UiShaderSourcePoolPopCapture(
                    ReceiverPairPointer: 0xFFFFFFFC,
                    CapturedHighTag: uint.MaxValue,
                    CapturedHead: 1,
                    CapturedNextHead: 0,
                    CompareExchangeReturnedHead: 1,
                    CompareExchangeReturnedHighTag: uint.MaxValue));
        check("The observed high DWORD increment wraps as unchecked 32-bit arithmetic",
            wrappingTag.ReplacementPair == new UiShaderSourcePoolPopPair(0, 0));

        bool emptyHeadWithExchangeResultRejected = false;
        try
        {
            UiShaderSourcePoolPopProjection.Project(
                new UiShaderSourcePoolPopCapture(
                    ReceiverPairPointer: 1,
                    CapturedHighTag: 0,
                    CapturedHead: 0,
                    CapturedNextHead: null,
                    CompareExchangeReturnedHead: 0,
                    CompareExchangeReturnedHighTag: null));
        }
        catch (ArgumentException exception)
        {
            emptyHeadWithExchangeResultRejected = exception.ParamName == "capture";
        }

        bool missingLinkRejected = false;
        bool missingReturnedHeadRejected = false;
        bool missingReturnedHighRejected = false;
        try
        {
            UiShaderSourcePoolPopProjection.Project(
                new UiShaderSourcePoolPopCapture(
                    ReceiverPairPointer: 1,
                    CapturedHighTag: 0,
                    CapturedHead: 1,
                    CapturedNextHead: null,
                    CompareExchangeReturnedHead: 1,
                    CompareExchangeReturnedHighTag: 0));
        }
        catch (ArgumentException exception)
        {
            missingLinkRejected = exception.ParamName == "capture";
        }

        try
        {
            UiShaderSourcePoolPopProjection.Project(
                new UiShaderSourcePoolPopCapture(
                    ReceiverPairPointer: 1,
                    CapturedHighTag: 0,
                    CapturedHead: 1,
                    CapturedNextHead: 2,
                    CompareExchangeReturnedHead: null,
                    CompareExchangeReturnedHighTag: 0));
        }
        catch (ArgumentException exception)
        {
            missingReturnedHeadRejected = exception.ParamName == "capture";
        }

        try
        {
            UiShaderSourcePoolPopProjection.Project(
                new UiShaderSourcePoolPopCapture(
                    ReceiverPairPointer: 1,
                    CapturedHighTag: 0,
                    CapturedHead: 1,
                    CapturedNextHead: 2,
                    CompareExchangeReturnedHead: 1,
                    CompareExchangeReturnedHighTag: null));
        }
        catch (ArgumentException exception)
        {
            missingReturnedHighRejected = exception.ParamName == "capture";
        }

        check("Projection rejects incomplete or incompatible captures",
            emptyHeadWithExchangeResultRejected
            && missingLinkRejected
            && missingReturnedHeadRejected
            && missingReturnedHighRejected);
    }
}
