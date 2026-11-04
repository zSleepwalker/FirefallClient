using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetAppendChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Reset append capture has the pinned sample identity, raw bounds, stack locals, and exact bytes",
            UiShaderSourcePoolResetAppendProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetAppendProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetAppendProjectionRules.ResetFunctionRawFileOffset == 0x12CDC50
            && UiShaderSourcePoolResetAppendProjectionRules.AppendBranchStartRawFileOffset == 0x12CDCE6
            && UiShaderSourcePoolResetAppendProjectionRules.CapturedBytesStartRawFileOffset == 0x12CDCE3
            && UiShaderSourcePoolResetAppendProjectionRules.CapturedBytesEndRawFileOffsetExclusive
                == 0x12CDD42
            && UiShaderSourcePoolResetAppendProjectionRules.CapturedBytesEndRawFileOffsetExclusive
                - UiShaderSourcePoolResetAppendProjectionRules.CapturedBytesStartRawFileOffset == 0x5F
            && UiShaderSourcePoolResetAppendProjectionRules.DescriptorBeginStackOffset == -0x34
            && UiShaderSourcePoolResetAppendProjectionRules.DescriptorEndStackOffset == -0x30
            && UiShaderSourcePoolResetAppendProjectionRules.DescriptorStorageEndStackOffset == -0x2C
            && UiShaderSourcePoolResetAppendProjectionRules.SavedLocalPrimaryPopStackOffset == -0x14
            && UiShaderSourcePoolResetAppendProjectionRules.CapturedAppendBytes
                == "8D 45 EC 89 7D EC 3B C6 73 32 8B C8 8B 45 CC 3B "
                    + "C1 77 29 8B F9 2B F8 C1 FF 02 3B F3 75 13 6A 01 "
                    + "8D 4D CC E8 F5 77 3D FF 8B 5D D4 8B 75 D0 8B 45 "
                    + "CC 85 F6 74 21 8B 04 B8 89 06 EB 1A 3B F3 75 10 "
                    + "6A 01 8D 4D CC E8 D3 77 3D FF 8B 5D D4 8B 75 D0 "
                    + "85 F6 74 02 89 3E 8B 4D E4 83 C6 04 89 75 D0"
            && UiShaderSourcePoolResetAppendProjectionRules.CapturedAppendBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x5F);

        check("Unsigned source-address bounds choose alias only for begin <= source < end",
            UiShaderSourcePoolResetAppendProjectionRules.SourceAddressCaptureRawFileOffset
                == 0x12CDCE3
            && UiShaderSourcePoolResetAppendProjectionRules.SavePrimaryPopValueRawFileOffset
                == 0x12CDCE6
            && UiShaderSourcePoolResetAppendProjectionRules.SourceAtOrAboveEndCompareRawFileOffset
                == 0x12CDCE9
            && UiShaderSourcePoolResetAppendProjectionRules.SourceAtOrAboveEndBranchRawFileOffset
                == 0x12CDCEB
            && UiShaderSourcePoolResetAppendProjectionRules.BeginAboveSourceCompareRawFileOffset
                == 0x12CDCF2
            && UiShaderSourcePoolResetAppendProjectionRules.BeginAboveSourceBranchRawFileOffset
                == 0x12CDCF4
            && UiShaderSourcePoolResetAppendProjectionRules.AliasIndexComputeRawFileOffset
                == 0x12CDCF6
            && HasByteSlice(0x12CDCE3, 0x12CDCED, "8D 45 EC 89 7D EC 3B C6 73 32")
            && HasByteSlice(0x12CDCEF, 0x12CDCF6, "8B 45 CC 3B C1 77 29")
            && HasByteSlice(0x12CDCF6, 0x12CDCFD, "8B F9 2B F8 C1 FF 02"));

        check("Both full-capacity edges call the same growth target with descriptor and argument one",
            UiShaderSourcePoolResetAppendProjectionRules.EndVersusStorageEndCompareRawFileOffset
                == 0x12CDCFD
            && UiShaderSourcePoolResetAppendProjectionRules.AliasGrowthCallRawFileOffset == 0x12CDD06
            && UiShaderSourcePoolResetAppendProjectionRules.ExternalGrowthCallRawFileOffset == 0x12CDD28
            && UiShaderSourcePoolResetAppendProjectionRules.GrowthCallTargetRawFileOffset == 0x006A5500
            && UiShaderSourcePoolResetAppendProjectionRules.GrowthCallArgument == 1
            && UiShaderSourcePoolResetAppendProjectionRules.GrowthCallTargetRawFileOffset
                + UiShaderSourcePoolResetAppendProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00AA6100
            && HasByteSlice(0x12CDCFD, 0x12CDD0B,
                "3B F3 75 13 6A 01 8D 4D CC E8 F5 77 3D FF")
            && HasByteSlice(0x12CDD21, 0x12CDD33,
                "75 10 6A 01 8D 4D CC E8 D3 77 3D FF 8B 5D D4 8B 75 D0")
            && CallTarget(0x12CDD0B, 0xFF3D77F5) == 0x006A5500
            && CallTarget(0x12CDD2D, 0xFF3D77D3) == 0x006A5500);

        check("Nonfull alias append reads the indexed value, stores it at end, and advances end by four",
            UiShaderSourcePoolResetAppendProjectionRules.AliasSourceLoadRawFileOffset == 0x12CDD18
            && UiShaderSourcePoolResetAppendProjectionRules.AliasSourceStoreRawFileOffset == 0x12CDD1B
            && UiShaderSourcePoolResetAppendProjectionRules.EndPointerAdvanceRawFileOffset == 0x12CDD3C
            && UiShaderSourcePoolResetAppendProjectionRules.EndPointerStoreRawFileOffset == 0x12CDD3F
            && HasByteSlice(0x12CDD14, 0x12CDD1D, "85 F6 74 21 8B 04 B8 89 06")
            && HasByteSlice(0x12CDD39, 0x12CDD42, "8B 4D E4 83 C6 04 89 75 D0"));

        check("External append stores the saved primary-pop value and advances the same end pointer",
            UiShaderSourcePoolResetAppendProjectionRules.ExternalSourceStoreRawFileOffset == 0x12CDD37
            && HasByteSlice(0x12CDD33, 0x12CDD42,
                "85 F6 74 02 89 3E 8B 4D E4 83 C6 04 89 75 D0"));

        var external = UiShaderSourcePoolResetAppendProjectionRules.Project(
            new UiShaderSourcePoolResetAppendCapture(
                DescriptorAddress: 0x7000,
                LocalSourceAddress: 0x9000,
                SavedLocalPrimaryPopValue: 0xAABBCCDD,
                DescriptorBeginPointer: 0x1000,
                DescriptorEndPointer: 0x1010,
                DescriptorStorageEndPointer: 0x1018));
        check("External nonfull append uses saved local value without growth and advances the captured end",
            external.SourceKind == UiShaderSourcePoolResetAppendSourceKind.ExternalSavedLocalValue
            && external.Boundary == UiShaderSourcePoolResetAppendBoundary.EndAdvanced
            && !external.GrowthCallReached
            && external.GrowthCallDescriptorAddress is null
            && external.AppendDestinationEndPointer == 0x1010
            && external.StoredValue == 0xAABBCCDD
            && external.AppendStoreAttempted
            && external.AdvancedDescriptorEndPointer == 0x1014);

        var alias = UiShaderSourcePoolResetAppendProjectionRules.Project(
            new UiShaderSourcePoolResetAppendCapture(
                DescriptorAddress: 0x7000,
                LocalSourceAddress: 0x1008,
                SavedLocalPrimaryPopValue: 0xAABBCCDD,
                DescriptorBeginPointer: 0x1000,
                DescriptorEndPointer: 0x1010,
                DescriptorStorageEndPointer: 0x1018,
                AliasedSourceValueBeforeGrowth: 0x11223344));
        check("Aliased nonfull append computes the DWORD index and uses a separate captured element value",
            alias.SourceKind == UiShaderSourcePoolResetAppendSourceKind.AliasedDescriptorElement
            && alias.AliasedElementIndex == 2
            && alias.AliasedSourceAddressRead == 0x1008
            && alias.StoredValue == 0x11223344
            && alias.StoredValue != alias.Capture.SavedLocalPrimaryPopValue
            && alias.AppendDestinationEndPointer == 0x1010
            && alias.AdvancedDescriptorEndPointer == 0x1014);

        check("Missing aliased source value is rejected when the nonfull alias load is reached",
            ThrowsArgumentException(() =>
                UiShaderSourcePoolResetAppendProjectionRules.Project(
                    new UiShaderSourcePoolResetAppendCapture(
                        DescriptorAddress: 0x7000,
                        LocalSourceAddress: 0x1008,
                        SavedLocalPrimaryPopValue: 0xAABBCCDD,
                        DescriptorBeginPointer: 0x1000,
                        DescriptorEndPointer: 0x1010,
                        DescriptorStorageEndPointer: 0x1018))));

        check("A supplied pre-growth alias value is rejected on external and zero-end paths",
            ThrowsArgumentException(() =>
                UiShaderSourcePoolResetAppendProjectionRules.Project(
                    new UiShaderSourcePoolResetAppendCapture(
                        DescriptorAddress: 0x7000,
                        LocalSourceAddress: 0x9000,
                        SavedLocalPrimaryPopValue: 0xAABBCCDD,
                        DescriptorBeginPointer: 0x1000,
                        DescriptorEndPointer: 0x1010,
                        DescriptorStorageEndPointer: 0x1018,
                        AliasedSourceValueBeforeGrowth: 0x11223344)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetAppendProjectionRules.Project(
                    new UiShaderSourcePoolResetAppendCapture(
                        DescriptorAddress: 0x7000,
                        LocalSourceAddress: 0x9000,
                        SavedLocalPrimaryPopValue: 0xAABBCCDD,
                        DescriptorBeginPointer: 0x1000,
                        DescriptorEndPointer: 0,
                        DescriptorStorageEndPointer: 4,
                        AliasedSourceValueBeforeGrowth: 0x11223344))));

        check("A supplied pre-growth alias value is rejected at the full-capacity growth boundary",
            ThrowsArgumentException(() =>
                UiShaderSourcePoolResetAppendProjectionRules.Project(
                    new UiShaderSourcePoolResetAppendCapture(
                        DescriptorAddress: 0x7000,
                        LocalSourceAddress: 0x1008,
                        SavedLocalPrimaryPopValue: 0xAABBCCDD,
                        DescriptorBeginPointer: 0x1000,
                        DescriptorEndPointer: 0x1020,
                        DescriptorStorageEndPointer: 0x1020,
                        AliasedSourceValueBeforeGrowth: 0x11223344))));

        var fullAtCall = UiShaderSourcePoolResetAppendProjectionRules.Project(
            new UiShaderSourcePoolResetAppendCapture(
                DescriptorAddress: 0x7000,
                LocalSourceAddress: 0x1004,
                SavedLocalPrimaryPopValue: 0x55,
                DescriptorBeginPointer: 0x1000,
                DescriptorEndPointer: 0x1020,
                DescriptorStorageEndPointer: 0x1020));
        check("Full alias path stops at the growth call without predicting new pointers or values",
            fullAtCall.SourceKind == UiShaderSourcePoolResetAppendSourceKind.AliasedDescriptorElement
            && fullAtCall.GrowthCallReached
            && fullAtCall.Boundary == UiShaderSourcePoolResetAppendBoundary.GrowthCallBoundary
            && fullAtCall.GrowthCallDescriptorAddress == 0x7000
            && fullAtCall.GrowthCallArgument == 1
            && fullAtCall.AppendDestinationEndPointer is null
            && fullAtCall.StoredValue is null
            && !fullAtCall.AppendStoreAttempted
            && fullAtCall.AdvancedDescriptorEndPointer is null);

        var fullAliasReturn = UiShaderSourcePoolResetAppendProjectionRules.Project(
            new UiShaderSourcePoolResetAppendCapture(
                DescriptorAddress: 0x7000,
                LocalSourceAddress: 0x1004,
                SavedLocalPrimaryPopValue: 0x55,
                DescriptorBeginPointer: 0x1000,
                DescriptorEndPointer: 0x1020,
                DescriptorStorageEndPointer: 0x1020,
                GrowthReturn: new UiShaderSourcePoolResetAppendGrowthReturnCapture(
                    DescriptorEndPointer: 0x2008,
                    DescriptorStorageEndPointer: 0x2010,
                    DescriptorBeginPointer: 0x2000,
                    AliasedSourceValue: 0x66778899)));
        check("A returned growth helper uses independent later pointers and the separately captured rebased alias value",
            fullAliasReturn.GrowthCallReached
            && fullAliasReturn.Boundary == UiShaderSourcePoolResetAppendBoundary.EndAdvanced
            && fullAliasReturn.Capture.GrowthReturn?.DescriptorBeginPointer == 0x2000
            && fullAliasReturn.AliasedElementIndex == 1
            && fullAliasReturn.AliasedSourceAddressRead == 0x2004
            && fullAliasReturn.AppendDestinationEndPointer == 0x2008
            && fullAliasReturn.StoredValue == 0x66778899
            && fullAliasReturn.AdvancedDescriptorEndPointer == 0x200C);

        var fullExternalReturn = UiShaderSourcePoolResetAppendProjectionRules.Project(
            new UiShaderSourcePoolResetAppendCapture(
                DescriptorAddress: 0x7000,
                LocalSourceAddress: 0x9000,
                SavedLocalPrimaryPopValue: 0xAABBCCDD,
                DescriptorBeginPointer: 0x1000,
                DescriptorEndPointer: 0x1020,
                DescriptorStorageEndPointer: 0x1020,
                GrowthReturn: new UiShaderSourcePoolResetAppendGrowthReturnCapture(
                    DescriptorEndPointer: 0x3000,
                    DescriptorStorageEndPointer: 0x3000,
                    DescriptorBeginPointer: null,
                    AliasedSourceValue: null)));
        check("A returned external growth helper stores the saved value without inventing a later begin-pointer read",
            fullExternalReturn.GrowthCallReached
            && fullExternalReturn.AppendDestinationEndPointer == 0x3000
            && fullExternalReturn.StoredValue == 0xAABBCCDD
            && fullExternalReturn.AliasedSourceAddressRead is null
            && fullExternalReturn.AdvancedDescriptorEndPointer == 0x3004);

        var nullEnd = UiShaderSourcePoolResetAppendProjectionRules.Project(
            new UiShaderSourcePoolResetAppendCapture(
                DescriptorAddress: 0x7000,
                LocalSourceAddress: 0x9000,
                SavedLocalPrimaryPopValue: 0xAABBCCDD,
                DescriptorBeginPointer: 0x1000,
                DescriptorEndPointer: 0x1020,
                DescriptorStorageEndPointer: 0x1020,
                GrowthReturn: new UiShaderSourcePoolResetAppendGrowthReturnCapture(
                    DescriptorEndPointer: 0,
                    DescriptorStorageEndPointer: 0,
                    DescriptorBeginPointer: null,
                    AliasedSourceValue: null)));
        check("Null post-growth end skips the source store but still follows the captured four-byte end advance",
            nullEnd.GrowthCallReached
            && !nullEnd.AppendStoreAttempted
            && nullEnd.StoredValue is null
            && nullEnd.AppendDestinationEndPointer == 0
            && nullEnd.AdvancedDescriptorEndPointer == 4);
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private static uint CallTarget(uint nextInstructionRawOffset, uint displacement)
    {
        return unchecked(nextInstructionRawOffset + displacement);
    }

    private static bool HasByteSlice(uint rawStart, uint rawEndExclusive, string expected)
    {
        string[] bytes =
            UiShaderSourcePoolResetAppendProjectionRules.CapturedAppendBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        uint captureStart = UiShaderSourcePoolResetAppendProjectionRules.CapturedBytesStartRawFileOffset;
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
