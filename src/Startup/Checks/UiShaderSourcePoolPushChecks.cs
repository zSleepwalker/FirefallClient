using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolPushChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Pool-push anchors match the pinned sample, complete raw entry, and code VA mapping",
            UiShaderSourcePoolPushProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolPushProjection.EntryName == "VIBE_LockFreePushStackNode"
            && UiShaderSourcePoolPushProjection.EntryRawFileOffset == 0x12CDA60
            && UiShaderSourcePoolPushProjection.EntryEndRawFileOffsetExclusive == 0x12CDAD6
            && UiShaderSourcePoolPushProjection.EntryEndRawFileOffsetExclusive
                - UiShaderSourcePoolPushProjection.EntryRawFileOffset == 0x76
            && UiShaderSourcePoolPushProjection.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x76
            && UiShaderSourcePoolPushProjection.EntryRawFileOffset
                + UiShaderSourcePoolPushProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CE660
            && UiShaderSourcePoolPushProjection.EntryEndRawFileOffsetExclusive
                + UiShaderSourcePoolPushProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CE6D6
            && UiShaderSourcePoolPushProjection.CapturedFunctionBytes
                == "55 8B EC 83 E4 F8 83 EC 0C 83 7D 08 00 8B C1 53 "
                    + "56 57 89 44 24 10 75 1E 6A 00 68 B6 01 00 00 68 "
                    + "A7 41 B7 01 68 6B 41 B7 01 FF 15 A0 5E C1 01 8B "
                    + "44 24 20 83 C4 10 8B 55 08 33 FF 8B 48 04 33 DB "
                    + "8B 00 8B F1 0B DA 89 74 24 14 89 02 41 8B 02 0B "
                    + "F8 8B C7 8B D6 8B 74 24 10 F0 0F C7 0E 8B 74 24 "
                    + "14 3B C7 8B 44 24 10 75 CD 3B D6 75 C9 5F 5E 5B "
                    + "8B E5 5D C2 04 00"
            && UiShaderSourcePoolPushProjection.PairHeadOffset == 0
            && UiShaderSourcePoolPushProjection.PairHighTagOffset == 4);

        check("Pool-push instruction evidence preserves all exact offsets, bytes, and branch targets",
            UiShaderSourcePoolPushProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.IndirectPointerSlotVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    uint? BranchTargetRawFileOffset,
                    uint? IndirectPointerSlotVirtualAddress)[]
                {
                    (0x12CDA60, "55", null, null),
                    (0x12CDA61, "8B EC", null, null),
                    (0x12CDA63, "83 E4 F8", null, null),
                    (0x12CDA66, "83 EC 0C", null, null),
                    (0x12CDA69, "83 7D 08 00", null, null),
                    (0x12CDA6D, "8B C1", null, null),
                    (0x12CDA6F, "53", null, null),
                    (0x12CDA70, "56", null, null),
                    (0x12CDA71, "57", null, null),
                    (0x12CDA72, "89 44 24 10", null, null),
                    (0x12CDA76, "75 1E", 0x12CDA96, null),
                    (0x12CDA78, "6A 00", null, null),
                    (0x12CDA7A, "68 B6 01 00 00", null, null),
                    (0x12CDA7F, "68 A7 41 B7 01", null, null),
                    (0x12CDA84, "68 6B 41 B7 01", null, null),
                    (0x12CDA89, "FF 15 A0 5E C1 01", null, 0x1C15EA0),
                    (0x12CDA8F, "8B 44 24 20", null, null),
                    (0x12CDA93, "83 C4 10", null, null),
                    (0x12CDA96, "8B 55 08", null, null),
                    (0x12CDA99, "33 FF", null, null),
                    (0x12CDA9B, "8B 48 04", null, null),
                    (0x12CDA9E, "33 DB", null, null),
                    (0x12CDAA0, "8B 00", null, null),
                    (0x12CDAA2, "8B F1", null, null),
                    (0x12CDAA4, "0B DA", null, null),
                    (0x12CDAA6, "89 74 24 14", null, null),
                    (0x12CDAAA, "89 02", null, null),
                    (0x12CDAAC, "41", null, null),
                    (0x12CDAAD, "8B 02", null, null),
                    (0x12CDAAF, "0B F8", null, null),
                    (0x12CDAB1, "8B C7", null, null),
                    (0x12CDAB3, "8B D6", null, null),
                    (0x12CDAB5, "8B 74 24 10", null, null),
                    (0x12CDAB9, "F0 0F C7 0E", null, null),
                    (0x12CDABD, "8B 74 24 14", null, null),
                    (0x12CDAC1, "3B C7", null, null),
                    (0x12CDAC3, "8B 44 24 10", null, null),
                    (0x12CDAC7, "75 CD", 0x12CDA96, null),
                    (0x12CDAC9, "3B D6", null, null),
                    (0x12CDACB, "75 C9", 0x12CDA96, null),
                    (0x12CDACD, "5F", null, null),
                    (0x12CDACE, "5E", null, null),
                    (0x12CDACF, "5B", null, null),
                    (0x12CDAD0, "8B E5", null, null),
                    (0x12CDAD2, "5D", null, null),
                    (0x12CDAD3, "C2 04 00", null, null)
                })
            && UiShaderSourcePoolPushProjection.NullNodeBranchRawFileOffset + 2 + 0x1E
                == UiShaderSourcePoolPushProjection.RetryCaptureRawFileOffset
            && UiShaderSourcePoolPushProjection.LowMismatchBranchRawFileOffset + 2 - 0x33
                == UiShaderSourcePoolPushProjection.RetryCaptureRawFileOffset
            && UiShaderSourcePoolPushProjection.HighMismatchBranchRawFileOffset + 2 - 0x37
                == UiShaderSourcePoolPushProjection.RetryCaptureRawFileOffset);

        UiShaderSourcePoolPushPlan returnedReceiver =
            UiShaderSourcePoolPushProjection.Project(
                new UiShaderSourcePoolPushCapture(
                    ReceiverPairPointer: 0x10203040,
                    CapturedNodePointer: 0x55667788,
                    CapturedHighTag: 0xAABBCCDD,
                    CapturedHead: 0x11223344,
                    CapturedNodeHeadForExpectedPair: 0x99AABBCC,
                    CompareExchangeReturnedHead: 0x99AABBCC,
                    CompareExchangeReturnedHighTag: 0xAABBCCDD));
        check("Matching observed cmpxchg8b halves return the receiver after one attempt",
            returnedReceiver.Outcome == UiShaderSourcePoolPushOutcome.ReturnedReceiver
            && returnedReceiver.ExpectedPair
                == new UiShaderSourcePoolPushPair(0x99AABBCC, 0xAABBCCDD)
            && returnedReceiver.ReplacementPair
                == new UiShaderSourcePoolPushPair(0x55667788, 0xAABBCCDD + 1)
            && returnedReceiver.CompareExchangeReturnedPair == returnedReceiver.ExpectedPair
            && returnedReceiver.ReturnValue == 0x10203040
            && returnedReceiver.Actions.SequenceEqual(new[]
            {
                new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.CaptureReceiverHighTag,
                    0x12CDA9B,
                    0x10203044,
                    0xAABBCCDD),
                new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.CaptureReceiverHead,
                    0x12CDAA0,
                    0x10203040,
                    0x11223344),
                new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.AttemptNodeLinkStore,
                    0x12CDAAA,
                    0x55667788,
                    0x11223344),
                new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.CaptureNodeHeadForExpectedPair,
                    0x12CDAAD,
                    0x55667788,
                    0x99AABBCC),
                new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.AttemptLockedCompareExchange,
                    0x12CDAB9,
                    0x10203040,
                    ExpectedPair: new UiShaderSourcePoolPushPair(0x99AABBCC, 0xAABBCCDD),
                    ReplacementPair: new UiShaderSourcePoolPushPair(0x55667788, 0xAABBCCDD + 1)),
                new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.CompareReturnedHead,
                    0x12CDAC1,
                    CapturedValue: 0x99AABBCC),
                new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.LoadReceiverForReturn,
                    0x12CDAC3,
                    CapturedValue: 0x10203040),
                new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.CompareReturnedHighTag,
                    0x12CDAC9,
                    CapturedValue: 0xAABBCCDD),
                new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.ReturnReceiver,
                    0x12CDAD3,
                    CapturedValue: 0x10203040)
            }));

        UiShaderSourcePoolPushPlan lowHalfMismatch =
            UiShaderSourcePoolPushProjection.Project(
                new UiShaderSourcePoolPushCapture(
                    ReceiverPairPointer: 0x10203040,
                    CapturedNodePointer: 0x55667788,
                    CapturedHighTag: 4,
                    CapturedHead: 0x11112222,
                    CapturedNodeHeadForExpectedPair: 0x33334444,
                    CompareExchangeReturnedHead: 0x55556666,
                    CompareExchangeReturnedHighTag: 4));
        UiShaderSourcePoolPushPlan highHalfMismatch =
            UiShaderSourcePoolPushProjection.Project(
                new UiShaderSourcePoolPushCapture(
                    ReceiverPairPointer: 0x10203040,
                    CapturedNodePointer: 0x55667788,
                    CapturedHighTag: 4,
                    CapturedHead: 0x11112222,
                    CapturedNodeHeadForExpectedPair: 0x33334444,
                    CompareExchangeReturnedHead: 0x33334444,
                    CompareExchangeReturnedHighTag: 5));
        check("Each returned-half mismatch stops at its explicit retry branch without simulating another attempt",
            lowHalfMismatch.Outcome == UiShaderSourcePoolPushOutcome.RetryBoundary
            && lowHalfMismatch.ReturnValue is null
            && lowHalfMismatch.Actions[^1]
                == new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.StopAtUnmodeledRetryBoundary,
                    0x12CDAC7,
                    BranchTargetRawFileOffset: 0x12CDA96)
            && highHalfMismatch.Outcome == UiShaderSourcePoolPushOutcome.RetryBoundary
            && highHalfMismatch.ReturnValue is null
            && highHalfMismatch.Actions[^1]
                == new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.StopAtUnmodeledRetryBoundary,
                    0x12CDACB,
                    BranchTargetRawFileOffset: 0x12CDA96));

        UiShaderSourcePoolPushPlan nullNode =
            UiShaderSourcePoolPushProjection.Project(
                new UiShaderSourcePoolPushCapture(
                    ReceiverPairPointer: 0x10203040,
                    CapturedNodePointer: 0,
                    CapturedHighTag: 7,
                    CapturedHead: 0x11112222,
                    CapturedNodeHeadForExpectedPair: 0x33334444,
                    CompareExchangeReturnedHead: 0x33334444,
                    CompareExchangeReturnedHighTag: 7));
        check("A captured null node records the diagnostic and a node-store attempt, never a clean no-op or assumed write",
            nullNode.Actions[0]
                == new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.CallNullNodeDiagnostic,
                    0x12CDA89,
                    0x1C15EA0)
            && nullNode.Actions[3]
                == new UiShaderSourcePoolPushAction(
                    UiShaderSourcePoolPushActionKind.AttemptNodeLinkStore,
                    0x12CDAAA,
                    Pointer: 0,
                    CapturedValue: 0x11112222)
            && nullNode.ExpectedPair == new UiShaderSourcePoolPushPair(0x33334444, 7)
            && nullNode.Outcome == UiShaderSourcePoolPushOutcome.ReturnedReceiver
            && nullNode.ReturnValue == 0x10203040);

        UiShaderSourcePoolPushPlan wrappingTag =
            UiShaderSourcePoolPushProjection.Project(
                new UiShaderSourcePoolPushCapture(
                    ReceiverPairPointer: 0xFFFFFFFC,
                    CapturedNodePointer: 1,
                    CapturedHighTag: uint.MaxValue,
                    CapturedHead: 2,
                    CapturedNodeHeadForExpectedPair: 3,
                    CompareExchangeReturnedHead: 3,
                    CompareExchangeReturnedHighTag: uint.MaxValue));
        check("Replacement high tag uses wrapping 32-bit increment",
            wrappingTag.ReplacementPair == new UiShaderSourcePoolPushPair(1, 0));
    }
}
