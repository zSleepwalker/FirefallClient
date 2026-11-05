using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetCallbackChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Reset callback source identity, raw offsets, mapping, bounds, and dispatch bytes are exact",
            UiShaderSourcePoolResetCallbackProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetCallbackProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetCallbackProjectionRules.FunctionRawFileOffset == 0x12CDC50
            && UiShaderSourcePoolResetCallbackProjectionRules.IterationStartRawFileOffset == 0x12CDDB2
            && UiShaderSourcePoolResetCallbackProjectionRules.IterationEndRawFileOffsetExclusive
                == 0x12CDDE6
            && UiShaderSourcePoolResetCallbackProjectionRules.InitialIndexZeroRawFileOffset
                == 0x12CDDA8
            && UiShaderSourcePoolResetCallbackProjectionRules.BoundCompareRawFileOffset == 0x12CDDAA
            && UiShaderSourcePoolResetCallbackProjectionRules.BoundBranchRawFileOffset == 0x12CDDAD
            && UiShaderSourcePoolResetCallbackProjectionRules.BoundBranchTargetRawFileOffset
                == 0x12CDDE6
            && UiShaderSourcePoolResetCallbackProjectionRules.BoundBranchRawFileOffset + 2 + 0x37
                == UiShaderSourcePoolResetCallbackProjectionRules.BoundBranchTargetRawFileOffset
            && UiShaderSourcePoolResetCallbackProjectionRules.IterationStartRawFileOffset
                + UiShaderSourcePoolResetCallbackProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CE9B2
            && UiShaderSourcePoolResetCallbackProjectionRules.IterationEndRawFileOffsetExclusive
                - UiShaderSourcePoolResetCallbackProjectionRules.IterationStartRawFileOffset
                == 0x34
            && HasByteSlice(0x12CDDA8, 0x12CDDAA, "33 FF")
            && HasByteSlice(0x12CDDAA, 0x12CDDAD, "39 7B 20")
            && HasByteSlice(0x12CDDAD, 0x12CDDAF, "76 37")
            && UiShaderSourcePoolResetCallbackProjectionRules.IterationTailBoundCompareRawFileOffset
                == 0x12CDDE1
            && UiShaderSourcePoolResetCallbackProjectionRules.IterationTailBoundBranchRawFileOffset
                == 0x12CDDE4
            && HasByteSlice(0x12CDDE1, 0x12CDDE4, "3B 7B 20")
            && HasByteSlice(0x12CDDE4, 0x12CDDE6, "72 CC")
            && UiShaderSourcePoolResetCallbackProjectionRules.CapturedDispatchBytes
                == "33 FF 39 7B 20 76 37 8B 55 CC 8B 4B 1C 8B C2 03 "
                    + "4B 14 0F AF CF C6 45 F3 00 03 4D E4 3B D6 74 0B "
                    + "39 08 74 78 83 C0 04 3B C6 75 F5 51 FF 55 08 8B "
                    + "55 CC 83 C4 04 FF 45 E8 47 3B 7B 20 72 CC"
            && UiShaderSourcePoolResetCallbackProjectionRules.CapturedDispatchBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x3E);

        check("Candidate formula loads padding, adds size, then multiplies by index and adds aligned base",
            UiShaderSourcePoolResetCallbackProjectionRules.PaddingLoadRawFileOffset
                == 0x12CDDB2
            && UiShaderSourcePoolResetCallbackProjectionRules.SizeAddRawFileOffset == 0x12CDDB7
            && UiShaderSourcePoolResetCallbackProjectionRules.IndexMultiplyRawFileOffset == 0x12CDDBA
            && UiShaderSourcePoolResetCallbackProjectionRules.AlignedBaseAddRawFileOffset
                == 0x12CDDC1
            && UiShaderSourcePoolResetCallbackProjectionRules.Field14Offset == 0x14
            && UiShaderSourcePoolResetCallbackProjectionRules.Field1COffset == 0x1C
            && UiShaderSourcePoolResetCallbackProjectionRules.LoopBoundOffset == 0x20
            && UiShaderSourcePoolResetCallbackProjectionRules.AlignedBaseStackOffset == -0x1C
            && UiShaderSourcePoolResetCallbackProjectionRules.ScanBeginStackOffset == -0x34
            && HasByteSlice(0x12CDDB2, 0x12CDDB5, "8B 4B 1C")
            && HasByteSlice(0x12CDDB7, 0x12CDDBA, "03 4B 14")
            && HasByteSlice(0x12CDDBA, 0x12CDDBD, "0F AF CF")
            && HasByteSlice(0x12CDDC1, 0x12CDDC4, "03 4D E4")
            && HasByteSlice(0x12CDDB2, 0x12CDDC4,
                "8B 4B 1C 8B C2 03 4B 14 0F AF CF C6 45 F3 00 03 4D E4"));

        check("First equality selects the later branch without modeling any subsequent callback",
            UiShaderSourcePoolResetCallbackProjectionRules.ScanRangeEmptyCompareRawFileOffset
                == 0x12CDDC4
            && UiShaderSourcePoolResetCallbackProjectionRules.MatchCandidateCompareRawFileOffset
                == 0x12CDDC8
            && UiShaderSourcePoolResetCallbackProjectionRules.MatchBranchRawFileOffset
                == 0x12CDDCA
            && UiShaderSourcePoolResetCallbackProjectionRules.MatchBranchTargetRawFileOffset
                == 0x12CDE44
            && UiShaderSourcePoolResetCallbackProjectionRules.MatchBranchRawFileOffset + 2 + 0x78
                == UiShaderSourcePoolResetCallbackProjectionRules.MatchBranchTargetRawFileOffset
            && HasByteSlice(0x12CDDC8, 0x12CDDCA, "39 08")
            && HasByteSlice(0x12CDDCA, 0x12CDDCC, "74 78")
            && HasByteSlice(0x12CDDC4, 0x12CDDD1,
                "3B D6 74 0B 39 08 74 78 83 C0 04 3B C6"));

        check("Callback dispatch pushes the candidate and calls the cdecl function pointer from the one-argument stack slot",
            UiShaderSourcePoolResetCallbackProjectionRules.CallbackArgumentPushRawFileOffset
                == 0x12CDDD3
            && UiShaderSourcePoolResetCallbackProjectionRules.CallbackCallRawFileOffset
                == 0x12CDDD4
            && UiShaderSourcePoolResetCallbackProjectionRules.CallbackCallArgumentCount == 1
            && UiShaderSourcePoolResetCallbackProjectionRules.CallbackCallingConvention == "cdecl"
            && !UiShaderSourcePoolResetCallbackProjectionRules.CallbackSuppliesThis
            && !UiShaderSourcePoolResetCallbackProjectionRules.CallbackReturnValueUsed
            && HasByteSlice(0x12CDDD3, 0x12CDDD4, "51")
            && HasByteSlice(0x12CDDD4, 0x12CDDD7, "FF 55 08")
            && HasByteSlice(0x12CDDD3, 0x12CDDD7, "51 FF 55 08"));

        var dispatch = UiShaderSourcePoolResetCallbackProjectionRules.Project(
            index: 3,
            loopBound: 4,
            field1CValue: 8,
            field14Value: 4,
            alignedBase: 0x1000,
            callbackPointer: 0x76543210,
            capturedScanValues: new uint[] { 0x2000, 0x3000 });
        check("A bounded unmatched candidate reaches the callback with exactly the computed candidate argument",
            dispatch.Outcome == UiShaderSourcePoolResetCallbackOutcome.CallbackCallReached
            && dispatch.Candidate == 0x1024
            && dispatch.FirstMatchingScanValueIndex is null
            && dispatch.CallbackCallReached
            && dispatch.CallbackArgumentPointer == 0x1024
            && dispatch.CallbackTargetPointer == 0x76543210
            && dispatch.BranchTargetRawFileOffset is null);

        var matchingValues = new uint[] { 0x1024, 0x1024, 0xFFFF };
        var match = UiShaderSourcePoolResetCallbackProjectionRules.Project(
            index: 3,
            loopBound: 4,
            field1CValue: 8,
            field14Value: 4,
            alignedBase: 0x1000,
            callbackPointer: 0x76543210,
            capturedScanValues: matchingValues);
        matchingValues[0] = 0;
        check("An equality match selects the first duplicate in order and bypasses callback dispatch",
            match.Outcome == UiShaderSourcePoolResetCallbackOutcome.ExistingCandidateMatched
            && match.Candidate == 0x1024
            && match.FirstMatchingScanValueIndex == 0
            && !match.CallbackCallReached
            && match.CallbackArgumentPointer is null
            && match.BranchTargetRawFileOffset
                == UiShaderSourcePoolResetCallbackProjectionRules.MatchBranchTargetRawFileOffset
            && match.CapturedScanValues.SequenceEqual([0x1024u, 0x1024u, 0xFFFFu]));

        var emptyScan = UiShaderSourcePoolResetCallbackProjectionRules.Project(
            index: 0,
            loopBound: 1,
            field1CValue: 5,
            field14Value: 2,
            alignedBase: 7,
            callbackPointer: 0x100,
            capturedScanValues: Array.Empty<uint>());
        check("An empty captured scan follows the no-match callback path",
            emptyScan.Outcome == UiShaderSourcePoolResetCallbackOutcome.CallbackCallReached
            && emptyScan.Candidate == 7
            && emptyScan.CapturedScanValues.IsEmpty
            && emptyScan.CallbackArgumentPointer == 7);

        var nullCallback = UiShaderSourcePoolResetCallbackProjectionRules.Project(
            index: 0,
            loopBound: 1,
            field1CValue: 1,
            field14Value: 2,
            alignedBase: 0x3000,
            callbackPointer: 0,
            capturedScanValues: Array.Empty<uint>());
        check("A null callback is still represented as a reached indirect call rather than a clean no-op",
            nullCallback.CallbackCallReached
            && nullCallback.CallbackTargetPointer == 0
            && nullCallback.CallbackArgumentPointer == 0x3000);

        var wrapped = UiShaderSourcePoolResetCallbackProjectionRules.Project(
            index: 2,
            loopBound: 3,
            field1CValue: 2,
            field14Value: uint.MaxValue,
            alignedBase: 0xFFFFFFFE,
            callbackPointer: 1,
            capturedScanValues: new uint[] { 1, 0, 0 });
        check("DWORD stride, multiplication, and aligned-base addition wrap before first-match testing",
            wrapped.Candidate == 0
            && wrapped.FirstMatchingScanValueIndex == 1
            && wrapped.Outcome == UiShaderSourcePoolResetCallbackOutcome.ExistingCandidateMatched);

        var outsideBound = UiShaderSourcePoolResetCallbackProjectionRules.Project(
            index: uint.MaxValue,
            loopBound: uint.MaxValue,
            field1CValue: 9,
            field14Value: 3,
            alignedBase: 0x3000,
            callbackPointer: 0x100,
            capturedScanValues: new uint[] { 0x3000 });
        check("Unsigned index-at-bound exits before candidate calculation or equality dispatch",
            outsideBound.Outcome == UiShaderSourcePoolResetCallbackOutcome.IndexOutsideBound
            && outsideBound.Candidate is null
            && outsideBound.FirstMatchingScanValueIndex is null
            && !outsideBound.CallbackCallReached
            && outsideBound.BranchTargetRawFileOffset
                == UiShaderSourcePoolResetCallbackProjectionRules.BoundBranchTargetRawFileOffset);
    }

    private static bool HasByteSlice(uint rawStart, uint rawEndExclusive, string expected)
    {
        string[] bytes =
            UiShaderSourcePoolResetCallbackProjectionRules.CapturedDispatchBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        uint captureStart = 0x12CDDA8;
        if (rawStart < captureStart || rawEndExclusive < rawStart)
        {
            return false;
        }

        int start = checked((int)(rawStart - captureStart));
        int length = checked((int)(rawEndExclusive - rawStart));
        return start + length <= bytes.Length
            && string.Join(" ", bytes.Skip(start).Take(length)) == expected;
    }
}
