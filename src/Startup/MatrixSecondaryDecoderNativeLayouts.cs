namespace Firefall.Reconstruction.Startup;

public enum MatrixSecondaryDecoderTargetKind
{
    Unresolved,
    MessageCase,
    SharedTarget,
    ZeroReturn
}

public readonly record struct MatrixSecondaryDecoderEntrySnapshot(
    byte Selector,
    int TableIndex,
    uint TargetVirtualAddress,
    uint TargetFileOffset,
    MatrixSecondaryDecoderTargetKind TargetKind);

public static class MatrixSecondaryDecoderTableNativeLayout
{
    public const uint ParserFileOffset = 0x81CC50;
    public const uint SelectorComparisonFileOffset = 0x81F3A8;
    public const uint SelectorJumpFileOffset = 0x81F3B4;
    public const uint TableVirtualAddress = 0xC22630;
    public const uint TableFileOffset = 0x821A30;
    public const uint CaseSetupHelperFileOffset = 0x3571B0;
    public const int SelectorMinimum = 0x20;
    public const int SelectorMaximum = 0x7A;
    public const int EntryCount = 0x5B;
    public const uint DefaultZeroReturnFileOffset = 0x8217D9;
    public const uint SharedCaseSetupArgument3VirtualAddress = 0x1E60904;
    public const uint RemainingInputBlobVtableVirtualAddress = 0x1C80CC4;
    public const uint RemainingInputBlobReaderMethodVirtualAddress = 0x16CC5D0;
    public const uint RemainingInputBlobReaderMethodFileOffset = 0x12CB9D0;
    public const uint RemainingInputBlobResizeHelperFileOffset = 0x12CBCB0;
    public const uint RemainingInputBlobCopyRoutineFileOffset = 0x159D8F4;

    public static IReadOnlyList<MatrixSecondaryDecoderEntrySnapshot> VerifiedEntries { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixSecondaryDecoderEntrySnapshot(0x20, 0, 0xC1FFBB, 0x81F3BB,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x21, 1, 0xC20023, 0x81F423,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x22, 2, 0xC2008B, 0x81F48B,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x23, 3, 0xC200F3, 0x81F4F3,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x24, 4, 0xC2015D, 0x81F55D,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x25, 5, 0xC201C5, 0x81F5C5,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x26, 6, 0xC2022F, 0x81F62F,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x27, 7, 0xC20283, 0x81F683,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x28, 8, 0xC202EB, 0x81F6EB,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x29, 9, 0xC20356, 0x81F756,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x2A, 10, 0xC203BE, 0x81F7BE,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x2B, 11, 0xC20426, 0x81F826,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x2C, 12, 0xC2048E, 0x81F88E,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x2D, 13, 0xC20502, 0x81F902,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x2E, 14, 0xC2054A, 0x81F94A,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x2F, 15, 0xC205BE, 0x81F9BE,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x30, 16, 0xC20628, 0x81FA28,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x31, 17, 0xC20690, 0x81FA90,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x32, 18, 0xC206E4, 0x81FAE4,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x33, 19, 0xC2074E, 0x81FB4E,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x34, 20, 0xC207B6, 0x81FBB6,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x35, 21, 0xC207FE, 0x81FBFE,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x36, 22, 0xC20866, 0x81FC66,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x37, 23, 0xC208CE, 0x81FCCE,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x38, 24, 0xC20936, 0x81FD36,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x39, 25, 0xC2099E, 0x81FD9E,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x3A, 26, 0xC20A06, 0x81FE06,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x3B, 27, 0xC20A6E, 0x81FE6E,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x3C, 28, 0xC20AD8, 0x81FED8,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x3D, 29, 0xC20B4C, 0x81FF4C,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x3E, 30, 0xC20BB4, 0x81FFB4,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x3F, 31, 0xC20C1E, 0x82001E,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x40, 32, 0xC20C88, 0x820088,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x41, 33, 0xC20CF0, 0x8200F0,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x42, 34, 0xC20D58, 0x820158,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x43, 35, 0xC20DC0, 0x8201C0,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x44, 36, 0xC20E28, 0x820228,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x45, 37, 0xC20E90, 0x820290,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x46, 38, 0xC20EF8, 0x8202F8,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x47, 39, 0xC20F60, 0x820360,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x48, 40, 0xC20FC8, 0x8203C8,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x49, 41, 0xC21030, 0x820430,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x4A, 42, 0xC21098, 0x820498,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x4B, 43, 0xC21100, 0x820500,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x4C, 44, 0xC21168, 0x820568,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x4D, 45, 0xC211D0, 0x8205D0,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x4E, 46, 0xC21238, 0x820638,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x4F, 47, 0xC212A0, 0x8206A0,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x50, 48, 0xC2130B, 0x82070B,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x51, 49, 0xC21375, 0x820775,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x52, 50, 0xC213BD, 0x8207BD,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x53, 51, 0xC21405, 0x820805,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x54, 52, 0xC21479, 0x820879,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x55, 53, 0xC214E1, 0x8208E1,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x56, 54, 0xC21529, 0x820929,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x57, 55, 0xC21593, 0x820993,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x58, 56, 0xC215FD, 0x8209FD,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x59, 57, 0xC21645, 0x820A45,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x5A, 58, 0xC21699, 0x820A99,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x5B, 59, 0xC216E1, 0x820AE1,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x5C, 60, 0xC21749, 0x820B49,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x5D, 61, 0xC217B1, 0x820BB1,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x5E, 62, 0xC21819, 0x820C19,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x5F, 63, 0xC21881, 0x820C81,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x60, 64, 0xC218E9, 0x820CE9,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x61, 65, 0xC21951, 0x820D51,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x62, 66, 0xC219B9, 0x820DB9,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x63, 67, 0xC21A21, 0x820E21,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x64, 68, 0xC21A89, 0x820E89,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x65, 69, 0xC21AF1, 0x820EF1,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x66, 70, 0xC21B59, 0x820F59,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x67, 71, 0xC21BA1, 0x820FA1,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x68, 72, 0xC21C0C, 0x82100C,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x69, 73, 0xC21C74, 0x821074,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x6A, 74, 0xC21CDE, 0x8210DE,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x6B, 75, 0xC21D46, 0x821146,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x6C, 76, 0xC21DAE, 0x8211AE,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x6D, 77, 0xC21E16, 0x821216,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x6E, 78, 0xC21E7E, 0x82127E,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x6F, 79, 0xC21EE6, 0x8212E6,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x70, 80, 0xC21F4E, 0x82134E,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x71, 81, 0xC21FB6, 0x8213B6,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x72, 82, 0xC2201E, 0x82141E,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x73, 83, 0xC22088, 0x821488,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x74, 84, 0xC220F0, 0x8214F0,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x75, 85, 0xC22158, 0x821558,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x76, 86, 0xC221CC, 0x8215CC,
                MatrixSecondaryDecoderTargetKind.MessageCase),
            new MatrixSecondaryDecoderEntrySnapshot(0x77, 87, 0xC22236, 0x821636,
                MatrixSecondaryDecoderTargetKind.MessageCase)
        });

    public static int? GetTableIndex(uint selector) =>
        selector >= SelectorMinimum && selector <= SelectorMaximum
            ? checked((int)(selector - SelectorMinimum))
            : null;

    public static MatrixSecondaryDecoderEntrySnapshot? GetVerifiedEntry(uint selector)
    {
        foreach (var entry in VerifiedEntries)
            if (entry.Selector == selector)
                return entry;
        return null;
    }
}

public static class MatrixSecondaryMessageHeaderNativeLayout
{
    public const int PrimaryVtablePointerOffset = 0x00;
    public const int BaseMetadataFlagOffset = 0x08;
    public const uint BaseMetadataFlagValue = 1;
    public const int DescriptorAnchorPointerOffset = 0x0C;
    public const int NativeTypeIdOffset = 0x10;
    public const int ParserInputFirstDwordCopyOffset = 0x18;
    public const int ParserInputSecondDwordCopyOffset = 0x1C;
    public const int FirstZeroDwordOffset = 0x20;
    public const int SecondZeroDwordOffset = 0x24;
    public const int BodyOffset = 0x28;
}

public static class EncounterToUIMessageNativeLayout
{
    public const byte Selector = 0x20;
    public const uint CaseVirtualAddress = 0xC1FFBB;
    public const uint CaseFileOffset = 0x81F3BB;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x813400;
    public const uint BaseMetadataConstructorFileOffset = 0x823020;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CDC;
    public const uint BodyReaderFileOffset = 0x853B10;
    public const uint VtableVirtualAddress = 0x1C6E258;
    public const string RttiName = ".?AVEncounterToUIMessage@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int FirstSharedTextFieldOffset = 0x30;
    public const int SecondSharedTextFieldOffset = 0x34;
}

public static class VotekickInitiatedNativeLayout
{
    public const byte Selector = 0x21;
    public const uint CaseVirtualAddress = 0xC20023;
    public const uint CaseFileOffset = 0x81F423;
    public const int AllocationSize = 0x50;
    public const uint ConstructorFileOffset = 0x813630;
    public const uint BaseMetadataConstructorFileOffset = 0x823090;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CE4;
    public const uint BodyReaderFileOffset = 0x853A20;
    public const uint VtableVirtualAddress = 0x1C6E288;
    public const string RttiName = ".?AVVotekickInitiated@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int FirstSharedTextFieldOffset = 0x30;
    public const int SecondSharedTextFieldOffset = 0x34;
    public const int DwordFieldOffset = 0x38;
    public const int SecondRawQwordFieldOffset = 0x40;
    public const int ByteFieldOffset = 0x48;
}

public static class VotekickRespondedNativeLayout
{
    public const byte Selector = 0x22;
    public const uint CaseVirtualAddress = 0xC2008B;
    public const uint CaseFileOffset = 0x81F48B;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x8138A0;
    public const uint BaseMetadataConstructorFileOffset = 0x823100;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CCC;
    public const uint BodyReaderFileOffset = 0x8538D0;
    public const uint VtableVirtualAddress = 0x1C6E278;
    public const string RttiName = ".?AVVotekickResponded@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int FirstSharedTextFieldOffset = 0x30;
    public const int SecondSharedTextFieldOffset = 0x34;
    public const int FirstByteFieldOffset = 0x38;
    public const int FirstWordFieldOffset = 0x3A;
    public const int SecondWordFieldOffset = 0x3C;
}

public static class ScoreBoardEnableNativeLayout
{
    public const byte Selector = 0x23;
    public const uint CaseVirtualAddress = 0xC200F3;
    public const uint CaseFileOffset = 0x81F4F3;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x813AF0;
    public const uint BaseMetadataConstructorFileOffset = 0x823170;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CD4;
    public const uint BodyReaderFileOffset = 0x356CF0;
    public const uint VtableVirtualAddress = 0x1C6E228;
    public const string RttiName = ".?AVScoreBoardEnable@firefall@@";
    public const int ByteFieldOffset = 0x28;
}

public static class ScoreBoardInitNativeLayout
{
    public const byte Selector = 0x24;
    public const uint CaseVirtualAddress = 0xC2015D;
    public const uint CaseFileOffset = 0x81F55D;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x813BA0;
    public const uint BaseMetadataConstructorFileOffset = 0x8231E0;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CBC;
    public const uint BodyReaderFileOffset = 0x853720;
    public const uint CollectionReaderFileOffset = 0x8532D0;
    public const uint RecordReaderFileOffset = 0x852B70;
    public const uint NestedFourDwordGroupReaderFileOffset = 0x852100;
    public const uint NestedRecordVectorReaderFileOffset = 0x852A10;
    public const uint NestedKeyedEntriesReaderFileOffset = 0x851920;
    public const uint VtableVirtualAddress = 0x1C6E248;
    public const string RttiName = ".?AVScoreBoardInit@firefall@@";
    public const int RecordVectorBeginPointerOffset = 0x28;
    public const int RecordVectorEndPointerOffset = 0x2C;
    public const int RecordVectorCapacityPointerOffset = 0x30;
    public const int FirstByteFieldOffset = 0x34;
    public const int SecondByteFieldOffset = 0x35;
    public const int ThirdByteFieldOffset = 0x38;
    public const int FourthByteFieldOffset = 0x39;
    public const int SharedTextFieldOffset = 0x3C;
    public const int RecordStride = 0xA8;
    public const int NestedRecordStride = 0x5C;
    public const int CountedFourDwordItemStride = 0x10;
    public const int CountedGroupContinuationByte = 0xFF;
}

public static class ScoreBoardInitRecordNativeLayout
{
    public const int Size = 0xA8;
    public const int FirstRawQwordOffset = 0x00;
    public const int SecondRawQwordOffset = 0x08;
    public const int SharedTextOffset = 0x10;
    public const int FirstDwordOffset = 0x14;
    public const int FirstByteOffset = 0x18;
    public const int SecondDwordOffset = 0x1C;
    public const int SecondByteOffset = 0x20;
    public const int ThirdDwordOffset = 0x24;
    public const int ThirdByteOffset = 0x28;
    public const int FourthByteOffset = 0x29;
    public const int NestedDwordSequenceOffset = 0x2C;
    public const int NestedDwordSequenceCount = 18;
    public const int CountedFourDwordItemsOffset = 0x74;
    public const int FinalDwordOffset = 0x80;
    public const int FinalByteOffset = 0x84;
    public const int CountedRecordVectorOffset = 0x88;
    public const int CountedKeyedEntriesOffset = 0x94;
    public const int CountedKeyedEntryStride = 0x0C;
    public const int TrailingDwordOffset = 0x9C;
    public const int TrailingByteOffset = 0xA0;
}

public static class ScoreBoardSetWinnerNativeLayout
{
    public const byte Selector = 0x25;
    public const uint CaseVirtualAddress = 0xC201C5;
    public const uint CaseFileOffset = 0x81F5C5;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x813EA0;
    public const uint BaseMetadataConstructorFileOffset = 0x823250;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CC4;
    public const uint BodyReaderFileOffset = 0x337180;
    public const uint VtableVirtualAddress = 0x1C6E238;
    public const string RttiName = ".?AVScoreBoardSetWinner@firefall@@";
    public const int ByteFieldOffset = 0x28;
}

public static class ScoreBoardClearNativeLayout
{
    public const byte Selector = 0x26;
    public const uint CaseVirtualAddress = 0xC2022F;
    public const uint CaseFileOffset = 0x81F62F;
    public const int AllocationSize = 0x28;
    public const uint ConstructorFileOffset = 0x813F50;
    public const uint BaseMetadataConstructorFileOffset = 0x8232C0;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CAC;
    public const uint AllocationFailureZeroReturnFileOffset = 0x8217D9;
    public const uint VtableVirtualAddress = 0x1C6E1F8;
    public const string RttiName = ".?AVScoreBoardClear@firefall@@";
    public const bool HasBodyReader = false;
}

public static class ScoreBoardAddPlayerNativeLayout
{
    public const byte Selector = 0x27;
    public const uint CaseVirtualAddress = 0xC20283;
    public const uint CaseFileOffset = 0x81F683;
    public const int AllocationSize = 0x58;
    public const uint ConstructorFileOffset = 0x813FC0;
    public const uint BaseMetadataConstructorFileOffset = 0x823330;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CB4;
    public const uint BodyReaderFileOffset = 0x851490;
    public const uint VtableVirtualAddress = 0x1C6E218;
    public const string RttiName = ".?AVScoreBoardAddPlayer@firefall@@";
    public const int FirstRawQwordFieldOffset = 0x28;
    public const int SecondRawQwordFieldOffset = 0x30;
    public const int SharedTextFieldOffset = 0x38;
    public const int FirstDwordFieldOffset = 0x3C;
    public const int FirstByteFieldOffset = 0x40;
    public const int SecondDwordFieldOffset = 0x44;
    public const int SecondByteFieldOffset = 0x48;
    public const int ThirdByteFieldOffset = 0x4C;
    public const int FourthByteFieldOffset = 0x51;
    public const int FifthByteFieldOffset = 0x50;
}

public static class ScoreBoardRemovePlayerNativeLayout
{
    public const byte Selector = 0x28;
    public const uint CaseVirtualAddress = 0xC202EB;
    public const uint CaseFileOffset = 0x81F6EB;
    public const uint ParserLocalDwordValue = 0x08;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x814170;
    public const uint BaseMetadataConstructorFileOffset = 0x8233C0;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89D1C;
    public const uint BodyReaderFileOffset = 0x356F80;
    public const uint BodyReaderCallSiteFileOffset = 0x81F73C;
    public const uint VtableVirtualAddress = 0x1C6E208;
    public const string RttiName = ".?AVScoreBoardRemovePlayer@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
}

public static class ScoreBoardUpdatePlayerStatsNativeLayout
{
    public const byte Selector = 0x29;
    public const uint CaseVirtualAddress = 0xC20356;
    public const uint CaseFileOffset = 0x81F756;
    public const uint ParserLocalDwordValue = 0x09;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x814230;
    public const uint BaseMetadataConstructorFileOffset = 0x823430;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89D24;
    public const uint BodyReaderFileOffset = 0x851410;
    public const uint BodyReaderCallSiteFileOffset = 0x81F7A4;
    public const uint ConditionalBlobReaderFileOffset = 0x5F3350;
    public const uint WordLengthPrefixedBlobReaderFileOffset = 0x12EE170;
    public const uint VtableVirtualAddress = 0x1C6E1D8;
    public const string RttiName = ".?AVScoreBoardUpdatePlayerStats@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int ConditionalBlobOwnerOffset = 0x30;
    public const int ConditionalBlobOffset = 0x34;
    public const int ConditionalBlobGateWireWidth = 4;
    public const uint ConditionalBlobGateValue = 1;
    public const int BlobLengthPrefixWidth = 2;
    public const uint ConstructorDefaultBlobValue = 0x1C80CC4;
}

public static class ScoreBoardUpdatePlayerStatsFromStatsNativeLayout
{
    public const byte Selector = 0x2A;
    public const uint CaseVirtualAddress = 0xC203BE;
    public const uint CaseFileOffset = 0x81F7BE;
    public const uint ParserLocalDwordValue = 0x0A;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x814390;
    public const uint BaseMetadataConstructorFileOffset = 0x8234A0;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89D0C;
    public const uint BodyReaderFileOffset = 0x851370;
    public const uint BodyReaderCallSiteFileOffset = 0x81F80C;
    public const uint CollectionReaderFileOffset = 0x851040;
    public const uint NestedVariantReaderFileOffset = 0x12F49C0;
    public const uint VtableVirtualAddress = 0x1C6E1E8;
    public const string RttiName = ".?AVScoreBoardUpdatePlayerStatsFromStats@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int CollectionBeginPointerOffset = 0x30;
    public const int CollectionEndPointerOffset = 0x34;
    public const int CollectionCapacityPointerOffset = 0x38;
    public const int CollectionEntryStride = 0x10;
    public const int CollectionCountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int NestedEntryCountWireWidth = 4;
    public const int NestedItemTagWireWidth = 4;
    public const uint NestedItemTagMinimum = 1;
    public const uint NestedItemTagMaximum = 16;
}

public static class ScoreBoardUpdatePlayerStatusNativeLayout
{
    public const byte Selector = 0x2B;
    public const uint CaseVirtualAddress = 0xC20426;
    public const uint CaseFileOffset = 0x81F826;
    public const uint ParserLocalDwordValue = 0x0B;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x814500;
    public const uint BaseMetadataConstructorFileOffset = 0x823510;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89D14;
    public const uint BodyReaderFileOffset = 0x8506B0;
    public const uint BodyReaderCallSiteFileOffset = 0x81F874;
    public const uint VtableVirtualAddress = 0x1C6E1B8;
    public const string RttiName = ".?AVScoreBoardUpdatePlayerStatus@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int FirstByteFieldOffset = 0x30;
    public const int DwordFieldOffset = 0x34;
    public const int SecondByteFieldOffset = 0x38;
    public const int ThirdByteFieldOffset = 0x3C;
    public const int FourthByteFieldOffset = 0x41;
    public const int FifthByteFieldOffset = 0x40;
    public const int ConstructorDefaultWordOffset = 0x40;
    public const ushort ConstructorDefaultWordValue = 1;
}

public static class MatchLoadingStateNativeLayout
{
    public const byte Selector = 0x2C;
    public const uint CaseVirtualAddress = 0xC2048E;
    public const uint CaseFileOffset = 0x81F88E;
    public const uint ParserLocalDwordValue = 0x0C;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x8145F0;
    public const uint BaseMetadataConstructorFileOffset = 0x823580;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CFC;
    public const uint FirstBodyReaderFileOffset = 0x356CF0;
    public const uint FirstBodyReaderCallSiteFileOffset = 0x81F8DE;
    public const uint CollectionReaderFileOffset = 0x8503E0;
    public const uint CollectionReaderCallSiteFileOffset = 0x81F8E8;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint VtableVirtualAddress = 0x1C6E1A8;
    public const string RttiName = ".?AVMatchLoadingState@firefall@@";
    public const int FirstByteFieldOffset = 0x28;
    public const int CollectionBeginPointerOffset = 0x2C;
    public const int CollectionEndPointerOffset = 0x30;
    public const int CollectionCapacityPointerOffset = 0x34;
    public const int CountByteWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int EntryStride = 0x10;
    public const int EntryRawQwordOffset = 0x00;
    public const int EntrySharedTextOffset = 0x08;
    public const int EntryByteOffset = 0x0C;
}

public static class MatchEndAckNativeLayout
{
    public const byte Selector = 0x2D;
    public const uint CaseVirtualAddress = 0xC20502;
    public const uint CaseFileOffset = 0x81F902;
    public const uint ParserLocalDwordValue = 0x0D;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x814780;
    public const uint BaseMetadataConstructorFileOffset = 0x8236F0;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89D04;
    public const uint BodyReaderFileOffset = ScoreBoardRemovePlayerNativeLayout.BodyReaderFileOffset;
    public const uint SharedBodyReaderCallSiteFileOffset =
        ScoreBoardRemovePlayerNativeLayout.BodyReaderCallSiteFileOffset;
    public const uint SharedBodyContinuationFileOffset = 0x81F72E;
    public const uint AllocationFailureZeroTargetFileOffset = 0x81F72C;
    public const uint VtableVirtualAddress = 0x1C6E1C8;
    public const string RttiName = ".?AVMatchEndAck@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
}

public static class ServerProfilerSendNamesNativeLayout
{
    public const byte Selector = 0x2E;
    public const uint CaseVirtualAddress = 0xC2054A;
    public const uint CaseFileOffset = 0x81F94A;
    public const uint ParserLocalDwordValue = 0x0E;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x814840;
    public const uint BaseMetadataConstructorFileOffset = 0x823760;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CEC;
    public const uint CollectionReaderFileOffset = 0x34E700;
    public const uint FirstCollectionReaderCallSiteFileOffset = 0x81F99A;
    public const uint SecondCollectionReaderCallSiteFileOffset = 0x81F9A4;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint VtableVirtualAddress = 0x1C6E188;
    public const string RttiName = ".?AVServerProfiler_SendNames@firefall@@";
    public const int FirstCollectionBeginPointerOffset = 0x28;
    public const int FirstCollectionEndPointerOffset = 0x2C;
    public const int FirstCollectionCapacityPointerOffset = 0x30;
    public const int SecondCollectionBeginPointerOffset = 0x34;
    public const int SecondCollectionEndPointerOffset = 0x38;
    public const int SecondCollectionCapacityPointerOffset = 0x3C;
    public const int CountByteWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int EntryStride = 0x08;
    public const int EntryWordOffset = 0x00;
    public const int EntrySharedTextOffset = 0x04;
}

public static class ServerProfilerSendFrameNativeLayout
{
    public const byte Selector = 0x2F;
    public const uint CaseVirtualAddress = 0xC205BE;
    public const uint CaseFileOffset = 0x81F9BE;
    public const uint ParserLocalDwordValue = 0x0F;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x8149D0;
    public const uint BaseMetadataConstructorFileOffset = 0x8237D0;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CF4;
    public const uint BodyReaderFileOffset = 0x34D6D0;
    public const uint BodyReaderCallSiteFileOffset = 0x81FA0E;
    public const uint RecursiveCollectionReaderFileOffset = 0x34BE80;
    public const uint VtableVirtualAddress = 0x1C6E178;
    public const string RttiName = ".?AVServerProfiler_SendFrame@firefall@@";
    public const int RootCollectionBeginPointerOffset = 0x28;
    public const int RootCollectionEndPointerOffset = 0x2C;
    public const int RootCollectionCapacityPointerOffset = 0x30;
    public const int CountByteWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int RootEntryStride = 0x10;
    public const int RootEntryWordOffset = 0x00;
    public const int RootEntryChildrenVectorOffset = 0x04;
    public const int RecursiveEntryStride = 0x20;
    public const int RecursiveEntryWordOffset = 0x00;
    public const int RecursiveEntryChildrenVectorOffset = 0x04;
    public const int RecursiveEntryTrailingWordSequenceOffset = 0x10;
    public const int RecursiveEntryTrailingWordCount = 8;
}

public static class TempConsoleMessageNativeLayout
{
    public const byte Selector = 0x30;
    public const uint CaseVirtualAddress = 0xC20628;
    public const uint CaseFileOffset = 0x81FA28;
    public const uint CaseLocalDwordValue = 0x10;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C744;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x814B50;
    public const uint BaseMetadataConstructorFileOffset = 0x823840;
    public const uint ConstructorNativeTypeId = 0x30;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C5C;
    public const uint BodyReaderFileOffset = 0x84F7F0;
    public const uint BodyReaderCallSiteFileOffset = 0x81FA76;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint WordLengthPrefixedBlobReaderFileOffset = 0x12EE170;
    public const uint VtableVirtualAddress = 0x1C6E198;
    public const string RttiName = ".?AVTempConsoleMessage@firefall@@";
    public const int FirstSharedTextFieldOffset = 0x28;
    public const int SecondSharedTextFieldOffset = 0x2C;
    public const int ThirdSharedTextFieldOffset = 0x30;
    public const int FourthSharedTextFieldOffset = 0x34;
    public const int BlobFieldOffset = 0x38;
    public const int BlobLengthPrefixWidth = 2;
    public const uint ConstructorDefaultBlobValue = 0x1C80CC4;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FA69;
}

public static class ReloadStaticDataNativeLayout
{
    public const byte Selector = 0x31;
    public const uint CaseVirtualAddress = 0xC20690;
    public const uint CaseFileOffset = 0x81FA90;
    public const uint CaseLocalDwordValue = 0x11;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C75C;
    public const int AllocationSize = 0x28;
    public const uint ConstructorFileOffset = 0x814EF0;
    public const uint BaseMetadataConstructorFileOffset = 0x8238B0;
    public const uint ConstructorNativeTypeId = 0x31;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C64;
    public const bool HasBodyReader = false;
    public const uint AllocationFailureZeroReturnFileOffset = 0x8217D9;
    public const uint VtableVirtualAddress = 0x1C6E148;
    public const string RttiName = ".?AVReloadStaticData@firefall@@";
}

public static class EncDebugChatMessageNativeLayout
{
    public const byte Selector = 0x32;
    public const uint CaseVirtualAddress = 0xC206E4;
    public const uint CaseFileOffset = 0x81FAE4;
    public const uint CaseLocalDwordValue = 0x12;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C774;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x814F60;
    public const uint BaseMetadataConstructorFileOffset = 0x823920;
    public const uint ConstructorNativeTypeId = 0x32;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C4C;
    public const uint BodyReaderFileOffset = 0x12EE010;
    public const uint BodyReaderCallSiteFileOffset = 0x81FB34;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint VtableVirtualAddress = 0x1C6E138;
    public const string RttiName = ".?AVEncDebugChatMessage@firefall@@";
    public const int SharedTextFieldOffset = 0x28;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FB25;
}

public static class SendRadioMessageNativeLayout
{
    public const byte Selector = 0x33;
    public const uint CaseVirtualAddress = 0xC2074E;
    public const uint CaseFileOffset = 0x81FB4E;
    public const uint CaseLocalDwordValue = 0x13;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C78C;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x8150F0;
    public const uint BaseMetadataConstructorFileOffset = 0x823990;
    public const uint ConstructorNativeTypeId = 0x33;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C54;
    public const uint BodyReaderFileOffset = 0x84F650;
    public const uint BodyReaderCallSiteFileOffset = 0x81FB9C;
    public const uint FirstDwordReaderFileOffset = 0x3370A0;
    public const uint SecondDwordReaderFileOffset = 0x353310;
    public const uint VtableVirtualAddress = 0x1C6E168;
    public const string RttiName = ".?AVSendRadioMessage@firefall@@";
    public const int FirstDwordFieldOffset = 0x28;
    public const int SecondDwordFieldOffset = 0x2C;
    public const int DwordWireWidth = 4;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FB8F;
}

public static class NpcBehaviorInfoNativeLayout
{
    public const byte Selector = 0x34;
    public const uint CaseVirtualAddress = 0xC207B6;
    public const uint CaseFileOffset = 0x81FBB6;
    public const uint CaseLocalDwordValue = 0x14;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C7A4;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x8151B0;
    public const uint BaseMetadataConstructorFileOffset = 0x823A00;
    public const uint ConstructorNativeTypeId = 0x34;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C3C;
    public const uint BodyReaderFileOffset = EncDebugChatMessageNativeLayout.BodyReaderFileOffset;
    public const uint SharedBodyReaderCallSiteFileOffset =
        EncDebugChatMessageNativeLayout.BodyReaderCallSiteFileOffset;
    public const uint SharedBodyContinuationFileOffset = 0x81FB27;
    public const uint VtableVirtualAddress = 0x1C6E158;
    public const string RttiName = ".?AVNpcBehaviorInfo@firefall@@";
    public const int SharedTextFieldOffset = 0x28;
    public const uint AllocationFailureZeroObjectFileOffset =
        EncDebugChatMessageNativeLayout.AllocationFailureZeroObjectFileOffset;
}

public static class NpcMonitoringLogNativeLayout
{
    public const byte Selector = 0x35;
    public const uint CaseVirtualAddress = 0xC207FE;
    public const uint CaseFileOffset = 0x81FBFE;
    public const uint CaseLocalDwordValue = 0x15;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C93C;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x815340;
    public const uint BaseMetadataConstructorFileOffset = 0x823A70;
    public const uint ConstructorNativeTypeId = 0x35;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C44;
    public const uint BodyReaderFileOffset = 0x84F550;
    public const uint BodyReaderCallSiteFileOffset = 0x81FC4C;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint VtableVirtualAddress = 0x1C6E108;
    public const string RttiName = ".?AVNpcMonitoringLog@firefall@@";
    public const int FirstSharedTextFieldOffset = 0x28;
    public const int ByteFieldOffset = 0x2C;
    public const int SecondSharedTextFieldOffset = 0x30;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FC3F;
}

public static class NpcNavigationInfoNativeLayout
{
    public const byte Selector = 0x36;
    public const uint CaseVirtualAddress = 0xC20866;
    public const uint CaseFileOffset = 0x81FC66;
    public const uint CaseLocalDwordValue = 0x16;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C954;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x815560;
    public const uint BaseMetadataConstructorFileOffset = 0x823AE0;
    public const uint ConstructorNativeTypeId = 0x36;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C2C;
    public const uint BodyReaderFileOffset = 0x84F490;
    public const uint BodyReaderCallSiteFileOffset = 0x81FCB4;
    public const uint RawQwordReaderFileOffset = 0x356F80;
    public const uint ThreeDwordVectorReaderFileOffset = 0x84E230;
    public const uint ConditionalRecordVectorReaderFileOffset = 0x84EC00;
    public const uint ThreeDwordVectorAppendFileOffset = 0x3BD830;
    public const uint VtableVirtualAddress = 0x1C6E128;
    public const string RttiName = ".?AVNpcNavigationInfo@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int ThreeDwordVectorBeginPointerOffset = 0x30;
    public const int ThreeDwordVectorEndPointerOffset = 0x34;
    public const int ThreeDwordVectorCapacityPointerOffset = 0x38;
    public const int ThreeDwordVectorCountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int ThreeDwordVectorItemDwordCount = 3;
    public const int ThreeDwordVectorItemWireWidth = 0x0C;
    public const int ThreeDwordVectorNativeStride = 0x0C;
    public const int ConditionalVectorBeginPointerOffset = 0x3C;
    public const int ConditionalVectorEndPointerOffset = 0x40;
    public const int ConditionalVectorCapacityPointerOffset = 0x44;
    public const int ConditionalVectorCountWireWidth = 1;
    public const int ConditionalVectorEntryPrefixDwordCount = 7;
    public const int ConditionalVectorEntryTailDwordCount = 3;
    public const int ConditionalVectorEntryTailByteCount = 1;
    public const int ConditionalVectorNativeStride = 0x70;
    public const uint ConditionalVectorBranchFileOffset = 0x84EF54;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FCA7;
}

public static class NpcHostilityDebugInfoNativeLayout
{
    public const byte Selector = 0x37;
    public const uint CaseVirtualAddress = 0xC208CE;
    public const uint CaseFileOffset = 0x81FCCE;
    public const uint CaseLocalDwordValue = 0x17;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C96C;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x815730;
    public const uint BaseMetadataConstructorFileOffset = 0x823C50;
    public const uint ConstructorNativeTypeId = 0x37;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C34;
    public const uint BodyReaderFileOffset = 0x84E200;
    public const uint BodyReaderCallSiteFileOffset = 0x81FD1C;
    public const uint RawQwordReaderFileOffset = 0x356F80;
    public const uint NestedVectorReaderFileOffset = 0x84DF20;
    public const uint VtableVirtualAddress = 0x1C6E118;
    public const string RttiName = ".?AVNpcHostilityDebugInfo@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int NestedVectorBeginPointerOffset = 0x30;
    public const int NestedVectorEndPointerOffset = 0x34;
    public const int NestedVectorCapacityPointerOffset = 0x38;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int EntryQwordOffset = 0x00;
    public const int EntryDwordOffset = 0x08;
    public const int EntryWireWidth = 0x0C;
    public const int EntryNativeStride = 0x10;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FD0F;
}

public static class NpcPositionalDebugInfoNativeLayout
{
    public const byte Selector = 0x38;
    public const uint CaseVirtualAddress = 0xC20936;
    public const uint CaseFileOffset = 0x81FD36;
    public const uint CaseLocalDwordValue = 0x18;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C984;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x815890;
    public const uint BaseMetadataConstructorFileOffset = 0x823DB0;
    public const uint ConstructorNativeTypeId = 0x38;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C9C;
    public const uint BodyReaderFileOffset = 0x84D830;
    public const uint BodyReaderCallSiteFileOffset = 0x81FD84;
    public const uint RecordVectorReaderFileOffset = 0x84D3D0;
    public const uint RecordAppendFileOffset = 0x84D2D0;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint RawQwordReaderFileOffset = 0x356F80;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint VtableVirtualAddress = 0x1C6E0D8;
    public const string RttiName = ".?AVNpcPositionalDebugInfo@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int SharedTextFieldOffset = 0x30;
    public const int RecordVectorBeginPointerOffset = 0x34;
    public const int RecordVectorEndPointerOffset = 0x38;
    public const int RecordVectorCapacityPointerOffset = 0x3C;
    public const int RecordCountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int RecordQwordWireWidth = 8;
    public const int RecordDwordCount = 8;
    public const int RecordWireWidth = 0x28;
    public const int RecordNativeStride = 0x28;
    public const int TrailingByteFieldOffset = 0x40;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FD77;
}

public static class NpcShapeDebugInfoNativeLayout
{
    public const byte Selector = 0x39;
    public const uint CaseVirtualAddress = 0xC2099E;
    public const uint CaseFileOffset = 0x81FD9E;
    public const uint CaseLocalDwordValue = 0x19;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C99C;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x815A80;
    public const uint BaseMetadataConstructorFileOffset = 0x823F20;
    public const uint ConstructorNativeTypeId = 0x39;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89CA4;
    public const uint BodyReaderFileOffset = 0x84C960;
    public const uint BodyReaderCallSiteFileOffset = 0x81FDEC;
    public const uint RecordVectorReaderFileOffset = 0x84C600;
    public const uint RecordAppendFileOffset = 0x84C570;
    public const uint FixedDwordBlockReaderFileOffset = 0x6087E0;
    public const uint RawQwordReaderFileOffset = 0x356F80;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint VtableVirtualAddress = 0x1C6E0F8;
    public const string RttiName = ".?AVNpcShapeDebugInfo@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int SharedTextFieldOffset = 0x30;
    public const int RecordVectorBeginPointerOffset = 0x34;
    public const int RecordVectorEndPointerOffset = 0x38;
    public const int RecordVectorCapacityPointerOffset = 0x3C;
    public const int RecordCountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int RecordLeadingByteOffset = 0;
    public const int RecordFixedDwordBlockOffset = 4;
    public const int RecordFixedDwordBlockCount = 16;
    public const int RecordTrailingDwordBlockOffset = 0x44;
    public const int RecordTrailingDwordCount = 4;
    public const int RecordTrailingByteOffset = 0x54;
    public const int RecordWireFixedWidth = 0x52;
    public const int RecordNativeStride = 0x58;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FDDF;
}

public static class NpcVoxelInfoNativeLayout
{
    public const byte Selector = 0x3A;
    public const uint CaseVirtualAddress = 0xC20A06;
    public const uint CaseFileOffset = 0x81FE06;
    public const uint CaseLocalDwordValue = 0x1A;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C9B4;
    public const int AllocationSize = 0x50;
    public const uint ConstructorFileOffset = 0x815C70;
    public const uint BaseMetadataConstructorFileOffset = 0x824090;
    public const uint ConstructorNativeTypeId = 0x3A;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C8C;
    public const uint BodyReaderFileOffset = 0x84BC10;
    public const uint BodyReaderCallSiteFileOffset = 0x81FE54;
    public const uint ThreeDwordVectorReaderFileOffset = 0x84E230;
    public const uint DwordVectorReaderFileOffset = 0x6095C0;
    public const uint RawQwordReaderFileOffset = 0x356F80;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint ThreeDwordVectorAppendFileOffset = 0x3BD830;
    public const uint DwordVectorAppendFileOffset = 0x3C8EE0;
    public const uint VtableVirtualAddress = 0x1C6E0E8;
    public const string RttiName = ".?AVNpcVoxelInfo@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int ThreeDwordVectorBeginPointerOffset = 0x30;
    public const int ThreeDwordVectorEndPointerOffset = 0x34;
    public const int ThreeDwordVectorCapacityPointerOffset = 0x38;
    public const int DwordVectorBeginPointerOffset = 0x3C;
    public const int DwordVectorEndPointerOffset = 0x40;
    public const int DwordVectorCapacityPointerOffset = 0x44;
    public const int TrailingByteFieldOffset = 0x48;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int ThreeDwordItemWireWidth = 0x0C;
    public const int ThreeDwordItemNativeStride = 0x0C;
    public const int DwordItemWireWidth = 4;
    public const int DwordItemNativeStride = 4;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FE47;
}

public static class DebugDrawInfoNativeLayout
{
    public const byte Selector = 0x3B;
    public const uint CaseVirtualAddress = 0xC20A6E;
    public const uint CaseFileOffset = 0x81FE6E;
    public const uint CaseLocalDwordValue = 0x1B;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C9CC;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x815E40;
    public const uint BaseMetadataConstructorFileOffset = 0x824100;
    public const uint ConstructorNativeTypeId = 0x3B;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C94;
    public const uint BodyReaderFileOffset = 0x609E50;
    public const uint BodyReaderCallSiteFileOffset = 0x81FEBE;
    public const uint VtableVirtualAddress = 0x1C6E0B8;
    public const string RttiName = ".?AVDebugDrawInfo@firefall@@";
    public const int ByteVectorBeginPointerOffset = 0x28;
    public const int ByteVectorEndPointerOffset = 0x2C;
    public const int ByteVectorCapacityPointerOffset = 0x30;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int ItemWireWidth = 1;
    public const int ItemNativeStride = 1;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FEAF;
}

public static class NpcDevCmdResponseNativeLayout
{
    public const byte Selector = 0x3C;
    public const uint CaseVirtualAddress = 0xC20AD8;
    public const uint CaseFileOffset = 0x81FED8;
    public const uint CaseLocalDwordValue = 0x1C;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C9E4;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x815F90;
    public const uint BaseMetadataConstructorFileOffset = 0x824170;
    public const uint ConstructorNativeTypeId = 0x3C;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C7C;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint FirstSharedTextReaderCallSiteFileOffset = 0x81FF28;
    public const uint SecondSharedTextReaderCallSiteFileOffset = 0x81FF32;
    public const uint VtableVirtualAddress = 0x1C6E0A8;
    public const string RttiName = ".?AVNpcDevCmdResponse@firefall@@";
    public const int FirstSharedTextFieldOffset = 0x28;
    public const int SecondSharedTextFieldOffset = 0x2C;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FF19;
}

public static class DevRequestObjectPositionsNativeLayout
{
    public const byte Selector = 0x3D;
    public const uint CaseVirtualAddress = 0xC20B4C;
    public const uint CaseFileOffset = 0x81FF4C;
    public const uint CaseLocalDwordValue = 0x1D;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C87C;
    public const int AllocationSize = 0x60;
    public const uint ConstructorFileOffset = 0x8161A0;
    public const uint BaseMetadataConstructorFileOffset = 0x8241E0;
    public const uint ConstructorNativeTypeId = 0x3D;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C84;
    public const uint BodyReaderFileOffset = 0x84BA20;
    public const uint BodyReaderCallSiteFileOffset = 0x81FF9A;
    public const uint ThreeDwordVectorReaderFileOffset = 0x84E230;
    public const uint DwordVectorReaderFileOffset = 0x5FEF50;
    public const uint SecondDwordVectorReaderFileOffset = 0x6095C0;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint ThreeDwordVectorAppendFileOffset = 0x3BD830;
    public const uint VtableVirtualAddress = 0x1C6E0C8;
    public const string RttiName = ".?AVDevRequestObjectPositions@firefall@@";
    public const int FirstByteFieldOffset = 0x28;
    public const int SecondByteFieldOffset = 0x29;
    public const int ThreeDwordVectorBeginPointerOffset = 0x2C;
    public const int ThreeDwordVectorEndPointerOffset = 0x30;
    public const int ThreeDwordVectorCapacityPointerOffset = 0x34;
    public const int FirstDwordVectorBeginPointerOffset = 0x38;
    public const int FirstDwordVectorEndPointerOffset = 0x3C;
    public const int FirstDwordVectorCapacityPointerOffset = 0x40;
    public const int SecondDwordVectorBeginPointerOffset = 0x44;
    public const int SecondDwordVectorEndPointerOffset = 0x48;
    public const int SecondDwordVectorCapacityPointerOffset = 0x4C;
    public const int ThirdDwordVectorBeginPointerOffset = 0x50;
    public const int ThirdDwordVectorEndPointerOffset = 0x54;
    public const int ThirdDwordVectorCapacityPointerOffset = 0x58;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int ThreeDwordItemWireWidth = 0x0C;
    public const int ThreeDwordItemNativeStride = 0x0C;
    public const int DwordItemWireWidth = 4;
    public const int DwordItemNativeStride = 4;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FF8D;
}

public static class DevRequestSpawnTablesNativeLayout
{
    public const byte Selector = 0x3E;
    public const uint CaseVirtualAddress = 0xC20BB4;
    public const uint CaseFileOffset = 0x81FFB4;
    public const uint CaseLocalDwordValue = 0x1E;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C894;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x8163A0;
    public const uint BaseMetadataConstructorFileOffset = 0x824250;
    public const uint ConstructorNativeTypeId = 0x3E;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C6C;
    public const uint BodyReaderFileOffset = 0x84B470;
    public const uint BodyReaderCallSiteFileOffset = 0x820004;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint VtableVirtualAddress = 0x1C6E088;
    public const string RttiName = ".?AVDevRequestSpawnTables@firefall@@";
    public const int RecordVectorBeginPointerOffset = 0x28;
    public const int RecordVectorEndPointerOffset = 0x2C;
    public const int RecordVectorCapacityPointerOffset = 0x30;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int RecordFirstSharedTextOffset = 0;
    public const int RecordFirstDwordOffset = 4;
    public const int RecordDwordCount = 5;
    public const int RecordByteOffset = 0x18;
    public const int RecordSecondSharedTextOffset = 0x1C;
    public const int RecordNativeStride = 0x20;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FFF5;
}

public static class DevRequestResourceNodeDebugNativeLayout
{
    public const byte Selector = 0x3F;
    public const uint CaseVirtualAddress = 0xC20C1E;
    public const uint CaseFileOffset = 0x82001E;
    public const uint CaseLocalDwordValue = 0x1F;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C8AC;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x8164E0;
    public const uint BaseMetadataConstructorFileOffset = 0x8243C0;
    public const uint ConstructorNativeTypeId = 0x3F;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C74;
    public const uint BodyReaderFileOffset = 0x84E230;
    public const uint BodyReaderCallSiteFileOffset = 0x82006E;
    public const uint RecordAppendFileOffset = 0x3BD830;
    public const uint VtableVirtualAddress = 0x1C6E078;
    public const string RttiName = ".?AVDevRequestResourceNodeDebug@firefall@@";
    public const int RecordVectorBeginPointerOffset = 0x28;
    public const int RecordVectorEndPointerOffset = 0x2C;
    public const int RecordVectorCapacityPointerOffset = 0x30;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int RecordDwordCount = 3;
    public const int RecordWireWidth = 0x0C;
    public const int RecordNativeStride = 0x0C;
    public const uint AllocationFailureZeroObjectFileOffset = 0x82005F;
}

public static class MissionObjectiveUpdatedNativeLayout
{
    public const byte Selector = 0x40;
    public const uint CaseVirtualAddress = 0xC20C88;
    public const uint CaseFileOffset = 0x820088;
    public const uint CaseLocalDwordValue = 0x20;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C8C4;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x816630;
    public const uint BaseMetadataConstructorFileOffset = 0x824430;
    public const uint ConstructorNativeTypeId = 0x40;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BDC;
    public const uint BodyReaderFileOffset = 0x84A7F0;
    public const uint BodyReaderCallSiteFileOffset = 0x8200D6;
    public const uint VtableVirtualAddress = 0x1C6E098;
    public const string RttiName = ".?AVMissionObjectiveUpdated@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int DwordFieldOffset = 0x30;
    public const int FirstSharedTextFieldOffset = 0x34;
    public const int SecondSharedTextFieldOffset = 0x38;
    public const int ByteFieldOffset = 0x3C;
    public const int TrailingDwordFieldOffset = 0x40;
}

public static class MissionStatusChangedNativeLayout
{
    public const byte Selector = 0x41;
    public const uint CaseVirtualAddress = 0xC20CF0;
    public const uint CaseFileOffset = 0x8200F0;
    public const uint CaseLocalDwordValue = 0x21;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C8DC;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x816880;
    public const uint BaseMetadataConstructorFileOffset = 0x8244A0;
    public const uint ConstructorNativeTypeId = 0x41;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BE4;
    public const uint BodyReaderFileOffset = 0x84A640;
    public const uint BodyReaderCallSiteFileOffset = 0x82013E;
    public const uint VtableVirtualAddress = 0x1C6E048;
    public const string RttiName = ".?AVMissionStatusChanged@firefall@@";
    public const int RawQwordFieldOffset = 0x28;
    public const int ByteFieldOffset = 0x30;
}

public static class MissionReturnToChangedNativeLayout
{
    public const byte Selector = 0x42;
    public const uint CaseVirtualAddress = 0xC20D58;
    public const uint CaseFileOffset = 0x820158;
    public const uint CaseLocalDwordValue = 0x22;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C8F4;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x816960;
    public const uint BaseMetadataConstructorFileOffset = 0x824510;
    public const uint ConstructorNativeTypeId = 0x42;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BCC;
    public const uint BodyReaderFileOffset = 0x84A520;
    public const uint BodyReaderCallSiteFileOffset = 0x8201A6;
    public const uint VtableVirtualAddress = 0x1C6E038;
    public const string RttiName = ".?AVMissionReturnToChanged@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int ByteFieldOffset = 0x2C;
}

public static class MissionsAvailableNativeLayout
{
    public const byte Selector = 0x43;
    public const uint CaseVirtualAddress = 0xC20DC0;
    public const uint CaseFileOffset = 0x8201C0;
    public const uint CaseLocalDwordValue = 0x23;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C90C;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x816A20;
    public const uint BaseMetadataConstructorFileOffset = 0x824580;
    public const uint ConstructorNativeTypeId = 0x43;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BD4;
    public const uint BodyReaderFileOffset = 0x84A480;
    public const uint BodyReaderCallSiteFileOffset = 0x82020E;
    public const uint RecordVectorReaderFileOffset = 0x84A1A0;
    public const uint RawQwordReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C6E068;
    public const string RttiName = ".?AVMissionsAvailable@firefall@@";
    public const int RecordVectorBeginPointerOffset = 0x28;
    public const int RecordVectorEndPointerOffset = 0x2C;
    public const int RecordVectorCapacityPointerOffset = 0x30;
    public const int RecordDwordOffset = 0;
    public const int RecordByteOffset = 4;
    public const int RecordWireWidth = 5;
    public const int RecordNativeStride = 8;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
    public const int RawQwordFieldOffset = 0x38;
}

public static class BountyStatusChangedNativeLayout
{
    public const byte Selector = 0x45;
    public const uint CaseVirtualAddress = 0xC20E90;
    public const uint CaseFileOffset = 0x820290;
    public const uint CaseLocalDwordValue = 0x25;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CABC;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x816C40;
    public const uint BaseMetadataConstructorFileOffset = 0x824750;
    public const uint ConstructorNativeTypeId = 0x45;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BC4;
    public const uint BodyReaderFileOffset = 0x8499F0;
    public const uint BodyReaderCallSiteFileOffset = 0x8202DE;
    public const uint VtableVirtualAddress = 0x1C6E008;
    public const string RttiName = ".?AVBountyStatusChanged@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int ByteFieldOffset = 0x2C;
}

public static class MissionActivationAckNativeLayout
{
    public const byte Selector = 0x44;
    public const uint CaseVirtualAddress = 0xC20E28;
    public const uint CaseFileOffset = 0x820228;
    public const uint CaseLocalDwordValue = 0x24;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C924;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x816B80;
    public const uint BaseMetadataConstructorFileOffset = 0x8246E0;
    public const uint ConstructorNativeTypeId = 0x44;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BBC;
    public const uint BodyReaderFileOffset = 0x849AB0;
    public const uint BodyReaderCallSiteFileOffset = 0x820276;
    public const uint VtableVirtualAddress = 0x1C6E058;
    public const string RttiName = ".?AVMissionActivationAck@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int FirstByteFieldOffset = 0x2C;
    public const int SecondByteFieldOffset = 0x2D;
}

public static class BountyAbortAckNativeLayout
{
    public const byte Selector = 0x46;
    public const uint CaseVirtualAddress = 0xC20EF8;
    public const uint CaseFileOffset = 0x8202F8;
    public const uint CaseLocalDwordValue = 0x26;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CAD4;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x816D00;
    public const uint BaseMetadataConstructorFileOffset = 0x8247C0;
    public const uint ConstructorNativeTypeId = 0x46;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BAC;
    public const uint BodyReaderFileOffset = 0x8498D0;
    public const uint BodyReaderCallSiteFileOffset = 0x820346;
    public const uint VtableVirtualAddress = 0x1C6E028;
    public const string RttiName = ".?AVBountyAbortAck@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int FirstByteFieldOffset = 0x2C;
    public const int SecondByteFieldOffset = 0x2D;
}

public static class BountyActivationAckNativeLayout
{
    public const byte Selector = 0x47;
    public const uint CaseVirtualAddress = 0xC20F60;
    public const uint CaseFileOffset = 0x820360;
    public const uint CaseLocalDwordValue = 0x27;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CAEC;
    public const int AllocationSize = 0x50;
    public const uint ConstructorFileOffset = 0x816DC0;
    public const uint BaseMetadataConstructorFileOffset = 0x824830;
    public const uint ConstructorNativeTypeId = 0x47;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BB4;
    public const uint BodyReaderFileOffset = 0x849790;
    public const uint BodyReaderCallSiteFileOffset = 0x8203AE;
    public const uint QwordReaderFileOffset = 0x356850;
    public const uint VtableVirtualAddress = 0x1C6E018;
    public const string RttiName = ".?AVBountyActivationAck@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int FirstByteFieldOffset = 0x2C;
    public const int SecondByteFieldOffset = 0x2D;
    public const int ThirdByteFieldOffset = 0x2E;
    public const int FirstTrailingDwordFieldOffset = 0x30;
    public const int SecondTrailingDwordFieldOffset = 0x34;
    public const int TrailingByteFieldOffset = 0x38;
    public const int QwordFieldOffset = 0x40;
    public const int SharedTextFieldOffset = 0x48;
}

public static class BountyListActiveAckNativeLayout
{
    public const byte Selector = 0x48;
    public const uint CaseVirtualAddress = 0xC20FC8;
    public const uint CaseFileOffset = 0x8203C8;
    public const uint CaseLocalDwordValue = 0x28;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CB04;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x816FC0;
    public const uint BaseMetadataConstructorFileOffset = 0x8248A0;
    public const uint ConstructorNativeTypeId = 0x48;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C1C;
    public const uint BodyReaderFileOffset = 0x849760;
    public const uint BodyReaderCallSiteFileOffset = 0x820416;
    public const uint VtableVirtualAddress = 0x1C6DFD8;
    public const string RttiName = ".?AVBountyListActiveAck@firefall@@";
    public const int FirstByteFieldOffset = 0x28;
    public const int ByteWireValueDwordOffset = 0x2C;
    public const int EmbeddedBlobObjectOffset = 0x30;
}

public static class BountyListActiveDetailsAckNativeLayout
{
    public const byte Selector = 0x49;
    public const uint CaseVirtualAddress = 0xC21030;
    public const uint CaseFileOffset = 0x820430;
    public const uint CaseLocalDwordValue = 0x29;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CB1C;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x817120;
    public const uint BaseMetadataConstructorFileOffset = 0x824910;
    public const uint ConstructorNativeTypeId = 0x49;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C24;
    public const uint BodyReaderFileOffset = 0x8496A0;
    public const uint BodyReaderCallSiteFileOffset = 0x82047E;
    public const uint VtableVirtualAddress = 0x1C6DFF8;
    public const string RttiName = ".?AVBountyListActiveDetailsAck@firefall@@";
    public const int ByteFieldOffset = 0x28;
    public const int EmbeddedBlobObjectOffset = 0x2C;
}

public static class BountyListAvailableAckNativeLayout
{
    public const byte Selector = 0x4A;
    public const uint CaseVirtualAddress = 0xC21098;
    public const uint CaseFileOffset = 0x820498;
    public const uint CaseLocalDwordValue = 0x2A;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CB34;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x817260;
    public const uint BaseMetadataConstructorFileOffset = 0x824980;
    public const uint ConstructorNativeTypeId = 0x4A;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C0C;
    public const uint BodyReaderFileOffset = 0x849600;
    public const uint BodyReaderCallSiteFileOffset = 0x8204E6;
    public const uint VtableVirtualAddress = 0x1C6DFE8;
    public const string RttiName = ".?AVBountyListAvailableAck@firefall@@";
    public const int ByteFieldOffset = 0x28;
    public const int EmbeddedBlobObjectOffset = 0x2C;
}

public static class BountyClearAckNativeLayout
{
    public const byte Selector = 0x4B;
    public const uint CaseVirtualAddress = 0xC21100;
    public const uint CaseFileOffset = 0x820500;
    public const uint CaseLocalDwordValue = 0x2B;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CB4C;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x8173A0;
    public const uint BaseMetadataConstructorFileOffset = 0x8249F0;
    public const uint ConstructorNativeTypeId = 0x4B;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C14;
    public const uint BodyReaderFileOffset = 0x849560;
    public const uint BodyReaderCallSiteFileOffset = 0x82054E;
    public const uint VtableVirtualAddress = 0x1C6DFB8;
    public const string RttiName = ".?AVBountyClearAck@firefall@@";
    public const int FirstByteFieldOffset = 0x28;
    public const int ByteWireValueDwordOffset = 0x2C;
}

public static class BountyClearPreviousAckNativeLayout
{
    public const byte Selector = 0x4C;
    public const uint CaseVirtualAddress = 0xC21168;
    public const uint CaseFileOffset = 0x820568;
    public const uint CaseLocalDwordValue = 0x2C;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CB64;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x817460;
    public const uint BaseMetadataConstructorFileOffset = 0x824A60;
    public const uint ConstructorNativeTypeId = 0x4C;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BFC;
    public const uint BodyReaderFileOffset = 0x8494C0;
    public const uint BodyReaderCallSiteFileOffset = 0x8205B6;
    public const uint VtableVirtualAddress = 0x1C6DFA8;
    public const string RttiName = ".?AVBountyClearPreviousAck@firefall@@";
    public const int FirstByteFieldOffset = 0x28;
    public const int ByteWireValueDwordOffset = 0x2C;
}

public static class BountyListPreviousAckNativeLayout
{
    public const byte Selector = 0x4D;
    public const uint CaseVirtualAddress = 0xC211D0;
    public const uint CaseFileOffset = 0x8205D0;
    public const uint CaseLocalDwordValue = 0x2D;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8C9FC;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x817520;
    public const uint BaseMetadataConstructorFileOffset = 0x824AD0;
    public const uint ConstructorNativeTypeId = 0x4D;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89C04;
    public const uint BodyReaderFileOffset = 0x849420;
    public const uint BodyReaderCallSiteFileOffset = 0x82061E;
    public const uint VtableVirtualAddress = 0x1C6DFC8;
    public const string RttiName = ".?AVBountyListPreviousAck@firefall@@";
    public const int ByteFieldOffset = 0x28;
    public const int EmbeddedBlobObjectOffset = 0x2C;
}

public static class BountyRerollResponseNativeLayout
{
    public const byte Selector = 0x4E;
    public const uint CaseVirtualAddress = 0xC21238;
    public const uint CaseFileOffset = 0x820638;
    public const uint CaseLocalDwordValue = 0x2E;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CA14;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x817660;
    public const uint BaseMetadataConstructorFileOffset = 0x824B40;
    public const uint ConstructorNativeTypeId = 0x4E;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BEC;
    public const uint BodyReaderFileOffset = 0x849380;
    public const uint BodyReaderCallSiteFileOffset = 0x820686;
    public const uint VtableVirtualAddress = 0x1C6DF88;
    public const string RttiName = ".?AVBountyRerollResponse@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int ByteFieldOffset = 0x2C;
    public const int SharedTextFieldOffset = 0x30;
}

public static class DisplayUiTrackBountyNativeLayout
{
    public const byte Selector = 0x4F;
    public const uint CaseVirtualAddress = 0xC212A0;
    public const uint CaseFileOffset = 0x8206A0;
    public const uint CaseLocalDwordValue = 0x2F;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CA2C;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x817800;
    public const uint BaseMetadataConstructorFileOffset = 0x824BB0;
    public const uint ConstructorNativeTypeId = 0x4F;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BF4;
    public const uint BodyReaderFileOffset = 0x3370A0;
    public const uint BodyReaderCallSiteFileOffset = 0x8206F1;
    public const uint VtableVirtualAddress = 0x1C6DF78;
    public const string RttiName = ".?AVDisplayUiTrackBounty@firefall@@";
    public const int DwordFieldOffset = 0x28;
}

public static class AchievementsNativeLayout
{
    public const byte Selector = 0x50;
    public const uint CaseVirtualAddress = 0xC2130B;
    public const uint CaseFileOffset = 0x82070B;
    public const uint CaseLocalDwordValue = 0x30;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CA44;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x8178C0;
    public const uint BaseMetadataConstructorFileOffset = 0x824C20;
    public const uint ConstructorNativeTypeId = 0x50;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B5C;
    public const uint BodyReaderFileOffset = 0x848FC0;
    public const uint BodyReaderCallSiteFileOffset = 0x82075B;
    public const uint RecordAppenderFileOffset = 0x848CC0;
    public const uint VtableVirtualAddress = 0x1C6DF98;
    public const string RttiName = ".?AVAchievements@firefall@@";
    public const int RecordVectorBeginPointerOffset = 0x28;
    public const int RecordVectorEndPointerOffset = 0x2C;
    public const int RecordVectorCapacityPointerOffset = 0x30;
    public const int RecordDwordWireOffset = 0;
    public const int RecordByteWireOffset = 4;
    public const int RecordWireWidth = 5;
    public const int RecordNativeStride = 8;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
}

public static class AchievementUnlockedNativeLayout
{
    public const byte Selector = 0x51;
    public const uint CaseVirtualAddress = 0xC21375;
    public const uint CaseFileOffset = 0x820775;
    public const uint CaseLocalDwordValue = 0x31;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CA5C;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x817A10;
    public const uint BaseMetadataConstructorFileOffset = 0x824D80;
    public const uint ConstructorNativeTypeId = 0x51;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B64;
    public const uint BodyReaderFileOffset = 0x356F80;
    public const uint BodyReaderCallSiteFileOffset = 0x81F73C;
    public const uint SharedBodyContinuationFileOffset = 0x81F72E;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81F72C;
    public const uint VtableVirtualAddress = 0x1C6DF48;
    public const string RttiName = ".?AVAchievementUnlocked@firefall@@";
    public const int QwordFieldOffset = 0x28;
    public const int QwordWireWidth = 8;
}

public static class TotalAchievementPointsNativeLayout
{
    public const byte Selector = 0x52;
    public const uint CaseVirtualAddress = 0xC213BD;
    public const uint CaseFileOffset = 0x8207BD;
    public const uint CaseLocalDwordValue = 0x32;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CA74;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x817AD0;
    public const uint BaseMetadataConstructorFileOffset = 0x824DF0;
    public const uint ConstructorNativeTypeId = 0x52;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B4C;
    public const uint BodyReaderFileOffset = 0x3370A0;
    public const uint BodyReaderCallSiteFileOffset = 0x8206F1;
    public const uint SharedBodyContinuationFileOffset = 0x8206E3;
    public const uint AllocationFailureZeroObjectFileOffset = 0x8206E1;
    public const uint VtableVirtualAddress = 0x1C6DF38;
    public const string RttiName = ".?AVTotalAchievementPoints@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int DwordWireWidth = 4;
}

public static class MissionCompletionCountsNativeLayout
{
    public const byte Selector = 0x53;
    public const uint CaseVirtualAddress = 0xC21405;
    public const uint CaseFileOffset = 0x820805;
    public const uint CaseLocalDwordValue = 0x33;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CA8C;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x817B90;
    public const uint BaseMetadataConstructorFileOffset = 0x824E60;
    public const uint ConstructorNativeTypeId = 0x53;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B54;
    public const uint BodyReaderFileOffset = 0x848610;
    public const uint BodyReaderCallSiteFileOffset = 0x820855;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint SharedTextReaderCallSiteFileOffset = 0x82085F;
    public const uint RecordAppenderFileOffset = 0x848570;
    public const uint VtableVirtualAddress = 0x1C6DF68;
    public const string RttiName = ".?AVMissionCompletionCounts@firefall@@";
    public const int RecordVectorBeginPointerOffset = 0x28;
    public const int RecordVectorEndPointerOffset = 0x2C;
    public const int RecordVectorCapacityPointerOffset = 0x30;
    public const int SharedTextFieldOffset = 0x34;
    public const int RecordDwordWireOffset = 0;
    public const int RecordQwordWireOffset = 4;
    public const int RecordTrailingDwordWireOffset = 12;
    public const int RecordWireWidth = 16;
    public const int RecordNativeStride = 24;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
}

public static class ContentUnlockedNativeLayout
{
    public const byte Selector = 0x54;
    public const uint CaseVirtualAddress = 0xC21479;
    public const uint CaseFileOffset = 0x820879;
    public const uint CaseLocalDwordValue = 0x34;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CAA4;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x817D60;
    public const uint BaseMetadataConstructorFileOffset = 0x824FD0;
    public const uint ConstructorNativeTypeId = 0x54;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B3C;
    public const uint BodyReaderFileOffset = 0x847DC0;
    public const uint BodyReaderCallSiteFileOffset = 0x8208C7;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C6DF58;
    public const string RttiName = ".?AVContentUnlocked@firefall@@";
    public const int SharedTextFieldOffset = 0x28;
    public const int DwordFieldOffset = 0x2C;
    public const int QwordFieldOffset = 0x30;
}

public static class ClientUIActionNativeLayout
{
    public const byte Selector = 0x55;
    public const uint CaseVirtualAddress = 0xC214E1;
    public const uint CaseFileOffset = 0x8208E1;
    public const uint CaseLocalDwordValue = 0x35;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CC3C;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x817F20;
    public const uint BaseMetadataConstructorFileOffset = 0x825040;
    public const uint ConstructorNativeTypeId = 0x55;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B44;
    public const uint BodyReaderFileOffset = 0x12EE010;
    public const uint BodyReaderCallSiteFileOffset = 0x81FB34;
    public const uint SharedBodyContinuationFileOffset = 0x81FB27;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81FB25;
    public const uint VtableVirtualAddress = 0x1C6DF08;
    public const string RttiName = ".?AVClientUIAction@firefall@@";
    public const int SharedTextFieldOffset = 0x28;
}

public static class ArcCompletionHistoryUpdateNativeLayout
{
    public const byte Selector = 0x56;
    public const uint CaseVirtualAddress = 0xC21529;
    public const uint CaseFileOffset = 0x820929;
    public const uint CaseLocalDwordValue = 0x36;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CC54;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x8180B0;
    public const uint BaseMetadataConstructorFileOffset = 0x8250B0;
    public const uint ConstructorNativeTypeId = 0x56;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B2C;
    public const uint BodyReaderFileOffset = 0x847A70;
    public const uint BodyReaderCallSiteFileOffset = 0x820979;
    public const uint TreeResetHelperFileOffset = 0x846C10;
    public const uint TreeInsertHelperFileOffset = 0x8479F0;
    public const uint SentinelFactoryFileOffset = 0x82A6B0;
    public const uint VtableVirtualAddress = 0x1C6DF28;
    public const string RttiName = ".?AVArcCompletionHistoryUpdate@firefall@@";
    public const int TreePointerOffset = 0x28;
    public const int TreeSentinelAllocationSize = 0x20;
    public const int TreeSentinelParentPointerOffset = 0;
    public const int TreeSentinelLeftPointerOffset = 4;
    public const int TreeSentinelRightPointerOffset = 8;
    public const int TreeSentinelMarkerWordOffset = 0x0C;
    public const ushort TreeSentinelMarkerWordValue = 0x0101;
    public const int RecordKeyDwordWireOffset = 0;
    public const int RecordQwordWireOffset = 4;
    public const int RecordTrailingDwordWireOffset = 12;
    public const int RecordWireWidth = 16;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
}

public static class JobLedgerEntriesUpdateNativeLayout
{
    public const byte Selector = 0x57;
    public const uint CaseVirtualAddress = 0xC21593;
    public const uint CaseFileOffset = 0x820993;
    public const uint CaseLocalDwordValue = 0x37;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CC6C;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x818220;
    public const uint BaseMetadataConstructorFileOffset = 0x825120;
    public const uint ConstructorNativeTypeId = 0x57;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B34;
    public const uint BodyReaderFileOffset = 0x6095C0;
    public const uint BodyReaderCallSiteFileOffset = 0x8209E3;
    public const uint RecordAppenderFileOffset = 0x3C8EE0;
    public const uint VtableVirtualAddress = 0x1C6DF18;
    public const string RttiName = ".?AVJobLedgerEntriesUpdate@firefall@@";
    public const int RecordVectorBeginPointerOffset = 0x28;
    public const int RecordVectorEndPointerOffset = 0x2C;
    public const int RecordVectorCapacityPointerOffset = 0x30;
    public const int RecordDwordWireOffset = 0;
    public const int RecordWireWidth = 4;
    public const int RecordNativeStride = 4;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
}

public static class TrackRecipeNativeLayout
{
    public const byte Selector = 0x58;
    public const uint CaseVirtualAddress = 0xC215FD;
    public const uint CaseFileOffset = 0x8209FD;
    public const uint CaseLocalDwordValue = 0x38;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CC84;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x818370;
    public const uint BaseMetadataConstructorFileOffset = 0x825190;
    public const uint ConstructorNativeTypeId = 0x58;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B9C;
    public const uint BodyReaderFileOffset = 0x3370A0;
    public const uint BodyReaderCallSiteFileOffset = 0x8206F1;
    public const uint SharedBodyContinuationFileOffset = 0x8206E3;
    public const uint AllocationFailureZeroObjectFileOffset = 0x8206E1;
    public const uint VtableVirtualAddress = 0x1C6DED8;
    public const string RttiName = ".?AVTrackRecipe@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int DwordWireWidth = 4;
}

public static class ClearTrackedRecipeNativeLayout
{
    public const byte Selector = 0x59;
    public const uint CaseVirtualAddress = 0xC21645;
    public const uint CaseFileOffset = 0x820A45;
    public const uint CaseLocalDwordValue = 0x39;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CC9C;
    public const int AllocationSize = 0x28;
    public const uint ConstructorFileOffset = 0x818430;
    public const uint BaseMetadataConstructorFileOffset = 0x825200;
    public const uint ConstructorNativeTypeId = 0x59;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89BA4;
    public const uint AllocationFailureZeroReturnFileOffset = 0x8217D9;
    public const uint SuccessfulConstructionReturnFileOffset = 0x820A7A;
    public const uint VtableVirtualAddress = 0x1C6DEF8;
    public const string RttiName = ".?AVClearTrackedRecipe@firefall@@";
    public const bool HasBodyReader = false;
}

public static class SlotTechNativeLayout
{
    public const byte Selector = 0x5A;
    public const uint CaseVirtualAddress = 0xC21699;
    public const uint CaseFileOffset = 0x820A99;
    public const uint CaseLocalDwordValue = 0x3A;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CCB4;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x8184A0;
    public const uint BaseMetadataConstructorFileOffset = 0x825270;
    public const uint ConstructorNativeTypeId = 0x5A;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B8C;
    public const uint BodyReaderFileOffset = 0x3370A0;
    public const uint BodyReaderCallSiteFileOffset = 0x8206F1;
    public const uint SharedBodyContinuationFileOffset = 0x8206E3;
    public const uint AllocationFailureZeroObjectFileOffset = 0x8206E1;
    public const uint VtableVirtualAddress = 0x1C6DEE8;
    public const string RttiName = ".?AVSlotTech@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int DwordWireWidth = 4;
}

public static class InteractableStatusChangedNativeLayout
{
    public const byte Selector = 0x5B;
    public const uint CaseVirtualAddress = 0xC216E1;
    public const uint CaseFileOffset = 0x820AE1;
    public const uint CaseLocalDwordValue = 0x3B;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CCCC;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x818560;
    public const uint BaseMetadataConstructorFileOffset = 0x8252E0;
    public const uint ConstructorNativeTypeId = 0x5B;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B94;
    public const uint BodyReaderFileOffset = 0x846A50;
    public const uint BodyReaderCallSiteFileOffset = 0x820B2F;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820B22;
    public const uint VtableVirtualAddress = 0x1C6DEB8;
    public const string RttiName = ".?AVInteractableStatusChanged@firefall@@";
    public const int ByteFieldOffset = 0x28;
    public const int DwordFieldOffset = 0x2C;
    public const int QwordFieldOffset = 0x30;
    public const int TrailingByteFieldOffset = 0x38;
    public const int ByteWireWidth = 1;
    public const int DwordWireWidth = 4;
    public const int QwordWireWidth = 8;
    public const int TrailingByteWireWidth = 1;
}

public static class SendTipMessageNativeLayout
{
    public const byte Selector = 0x5C;
    public const uint CaseVirtualAddress = 0xC21749;
    public const uint CaseFileOffset = 0x820B49;
    public const uint CaseLocalDwordValue = 0x3C;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CCE4;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x818640;
    public const uint BaseMetadataConstructorFileOffset = 0x825350;
    public const uint ConstructorNativeTypeId = 0x5C;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B7C;
    public const uint BodyReaderFileOffset = 0x846960;
    public const uint BodyReaderCallSiteFileOffset = 0x820B97;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820B8A;
    public const uint VtableVirtualAddress = 0x1C6DEA8;
    public const string RttiName = ".?AVSendTipMessage@firefall@@";
    public const int ByteFieldOffset = 0x28;
    public const int FirstDwordFieldOffset = 0x2C;
    public const int SecondDwordFieldOffset = 0x30;
    public const int WordFieldOffset = 0x34;
    public const int TrailingByteFieldOffset = 0x36;
    public const int ByteWireWidth = 1;
    public const int DwordWireWidth = 4;
    public const int WordWireWidth = 2;
}

public static class DebugEventSampleNativeLayout
{
    public const byte Selector = 0x5D;
    public const uint CaseVirtualAddress = 0xC217B1;
    public const uint CaseFileOffset = 0x820BB1;
    public const uint CaseLocalDwordValue = 0x3D;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CB7C;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x818730;
    public const uint BaseMetadataConstructorFileOffset = 0x8253C0;
    public const uint ConstructorNativeTypeId = 0x5D;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B84;
    public const uint BodyReaderFileOffset = 0x846850;
    public const uint BodyReaderCallSiteFileOffset = 0x820BFF;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820BF2;
    public const uint VtableVirtualAddress = 0x1C6DEC8;
    public const string RttiName = ".?AVDebugEventSample@firefall@@";
    public const int FirstWordFieldOffset = 0x28;
    public const int SecondWordFieldOffset = 0x2A;
    public const int FirstByteFieldOffset = 0x2C;
    public const int SecondByteFieldOffset = 0x2D;
    public const int WordWireWidth = 2;
    public const int ByteWireWidth = 1;
}

public static class DebugLagPlayerSampleNativeLayout
{
    public const byte Selector = 0x5E;
    public const uint CaseVirtualAddress = 0xC21819;
    public const uint CaseFileOffset = 0x820C19;
    public const uint CaseLocalDwordValue = 0x3E;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CB94;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x818800;
    public const uint BaseMetadataConstructorFileOffset = 0x825430;
    public const uint ConstructorNativeTypeId = 0x5E;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B6C;
    public const uint BodyReaderFileOffset = 0x846760;
    public const uint BodyReaderCallSiteFileOffset = 0x820C67;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820C5A;
    public const uint VtableVirtualAddress = 0x1C6DE88;
    public const string RttiName = ".?AVDebugLagPlayerSample@firefall@@";
    public const int FirstWordFieldOffset = 0x28;
    public const int SecondWordFieldOffset = 0x2A;
    public const int ThirdWordFieldOffset = 0x2C;
    public const int FourthWordFieldOffset = 0x2E;
    public const int FifthWordFieldOffset = 0x30;
    public const int WordFieldCount = 5;
    public const int WordFieldStride = 2;
    public const int WordWireWidth = 2;
}

public static class DebugLagSimulationSampleNativeLayout
{
    public const byte Selector = 0x5F;
    public const uint CaseVirtualAddress = 0xC21881;
    public const uint CaseFileOffset = 0x820C81;
    public const uint CaseLocalDwordValue = 0x3F;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CBAC;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x8188E0;
    public const uint BaseMetadataConstructorFileOffset = 0x8254A0;
    public const uint ConstructorNativeTypeId = 0x5F;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B74;
    public const uint BodyReaderFileOffset = 0x8465C0;
    public const uint BodyReaderCallSiteFileOffset = 0x820CCF;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820CC2;
    public const uint VtableVirtualAddress = 0x1C6DE78;
    public const string RttiName = ".?AVDebugLagSimulationSample@firefall@@";
    public const int FirstWordFieldOffset = 0x28;
    public const int LastWordFieldOffset = 0x40;
    public const int WordFieldCount = 13;
    public const int WordFieldStride = 2;
    public const int WordWireWidth = 2;
}

public static class DebugLagRaiaSampleNativeLayout
{
    public const byte Selector = 0x60;
    public const uint CaseVirtualAddress = 0xC218E9;
    public const uint CaseFileOffset = 0x820CE9;
    public const uint CaseLocalDwordValue = 0x40;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CBC4;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x818A10;
    public const uint BaseMetadataConstructorFileOffset = 0x825510;
    public const uint ConstructorNativeTypeId = 0x60;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89ADC;
    public const uint BodyReaderFileOffset = 0x846580;
    public const uint BodyReaderCallSiteFileOffset = 0x820D37;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820D2A;
    public const uint VtableVirtualAddress = 0x1C6DE98;
    public const string RttiName = ".?AVDebugLagRaiaSample@firefall@@";
    public const int FirstWordFieldOffset = 0x28;
    public const int WordFieldCount = 4;
    public const int WordFieldStride = 2;
    public const int WordWireWidth = 2;
}

public static class DebugEncounterVolumesNativeLayout
{
    public const byte Selector = 0x61;
    public const uint CaseVirtualAddress = 0xC21951;
    public const uint CaseFileOffset = 0x820D51;
    public const uint CaseLocalDwordValue = 0x41;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CBDC;
    public const int AllocationSize = 0x58;
    public const uint ConstructorFileOffset = 0x818AE0;
    public const uint BaseMetadataConstructorFileOffset = 0x825580;
    public const uint ConstructorNativeTypeId = 0x61;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89AE4;
    public const uint BodyReaderFileOffset = 0x846480;
    public const uint BodyReaderCallSiteFileOffset = 0x820D9F;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820D92;
    public const uint VtableVirtualAddress = 0x1C6DE48;
    public const string RttiName = ".?AVDebugEncounterVolumes@firefall@@";
    public const int FirstVectorBeginOffset = 0x28;
    public const int FirstVectorEndOffset = 0x2C;
    public const int FirstVectorCapacityOffset = 0x30;
    public const int FirstRecordFloatCount = 4;
    public const int FirstRecordWireWidth = 0x10;
    public const int FirstRecordNativeStride = 0x10;
    public const int SecondVectorBeginOffset = 0x34;
    public const int SecondVectorEndOffset = 0x38;
    public const int SecondVectorCapacityOffset = 0x3C;
    public const int SecondRecordWireWidth = 0x40;
    public const int SecondRecordNativeStride = 0x40;
    public const int ThirdVectorBeginOffset = 0x40;
    public const int ThirdVectorEndOffset = 0x44;
    public const int ThirdVectorCapacityOffset = 0x48;
    public const int ThirdRecordWireWidth = 0x40;
    public const int ThirdRecordNativeStride = 0x40;
    public const int FourthVectorBeginOffset = 0x4C;
    public const int FourthVectorEndOffset = 0x50;
    public const int FourthVectorCapacityOffset = 0x54;
    public const int FourthRecordFixedWireWidth = 0x40;
    public const int FourthRecordNativeStride = 0x44;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
}

public static class TrailNativeLayout
{
    public const byte Selector = 0x62;
    public const uint CaseVirtualAddress = 0xC219B9;
    public const uint CaseFileOffset = 0x820DB9;
    public const uint CaseLocalDwordValue = 0x42;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CBF4;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x818CE0;
    public const uint BaseMetadataConstructorFileOffset = 0x825700;
    public const uint ConstructorNativeTypeId = 0x62;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89ACC;
    public const uint BodyReaderFileOffset = 0x844AD0;
    public const uint BodyReaderCallSiteFileOffset = 0x820E07;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820DFA;
    public const uint VtableVirtualAddress = 0x1C6DE38;
    public const string RttiName = ".?AVTrail@firefall@@";
    public const int DwordFieldOffset = 0x28;
    public const int ByteFieldOffset = 0x2C;
    public const int RecordVectorBeginOffset = 0x30;
    public const int RecordVectorEndOffset = 0x34;
    public const int RecordVectorCapacityOffset = 0x38;
    public const int RecordFloatCount = 3;
    public const int RecordWireWidth = 0x0C;
    public const int RecordNativeStride = 0x0C;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
}

public static class EncounterDebugNotificationNativeLayout
{
    public const byte Selector = 0x63;
    public const uint CaseVirtualAddress = 0xC21A21;
    public const uint CaseFileOffset = 0x820E21;
    public const uint CaseLocalDwordValue = 0x43;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CC0C;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x818E40;
    public const uint BaseMetadataConstructorFileOffset = 0x825770;
    public const uint ConstructorNativeTypeId = 0x63;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89AD4;
    public const uint BodyReaderFileOffset = 0x844A10;
    public const uint BodyReaderCallSiteFileOffset = 0x820E6F;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820E62;
    public const uint VtableVirtualAddress = 0x1C6DE68;
    public const string RttiName = ".?AVEncounterDebugNotification@firefall@@";
    public const int QwordFieldOffset = 0x28;
    public const int FirstTextFieldOffset = 0x30;
    public const int SecondTextFieldOffset = 0x34;
    public const int QwordWireWidth = 8;
    public const int TextTerminatorWireWidth = 1;
}

public static class EncounterUIScopeInNativeLayout
{
    public const byte Selector = 0x64;
    public const uint CaseVirtualAddress = 0xC21A89;
    public const uint CaseFileOffset = 0x820E89;
    public const uint CaseLocalDwordValue = 0x44;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CC24;
    public const int AllocationSize = 0x60;
    public const uint ConstructorFileOffset = 0x819070;
    public const uint BaseMetadataConstructorFileOffset = 0x8257E0;
    public const uint ConstructorNativeTypeId = 0x64;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89ABC;
    public const uint BodyReaderFileOffset = 0x8448B0;
    public const uint BodyReaderCallSiteFileOffset = 0x820ED7;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820ECA;
    public const uint VtableVirtualAddress = 0x1C6DE58;
    public const string RttiName = ".?AVEncounterUIScopeIn@firefall@@";
    public const uint TaggedEntryDispatcherFileOffset = 0x609830;
    public const uint TaggedEntryDispatchTableVirtualAddress = 0xA0A630;
    public const uint TagZeroHandlerFileOffset = 0x6098AD;
    public const uint TagOneHandlerFileOffset = 0x609907;
    public const uint TagTwoAndThreeHandlerFileOffset = 0x609919;
    public const uint TagFourHandlerFileOffset = 0x6099B6;
    public const uint UnsupportedTagHandlerFileOffset = 0x609A1D;
    public const uint TagSixHandlerFileOffset = 0x60992B;
    public const uint TagSevenHandlerFileOffset = 0x60993D;
    public const uint TagEightHandlerFileOffset = 0x6099AB;
    public const uint SpecialZeroTagHandlerFileOffset = 0x609A05;
    public const uint SpecialNoPayloadTagHandlerFileOffset = 0x609912;
    public const int QwordFieldOffset = 0x28;
    public const int LeadingBlobOffset = 0x30;
    public const int LeadingBlobLengthPrefixWireWidth = 2;
    public const int WordFieldOffset = 0x40;
    public const int TreeRootPointerOffset = 0x44;
    public const int TreeCountOffset = 0x48;
    public const int TrailingBlobOffset = 0x4C;
    public const int TreeRecordKeyWireWidth = 8;
    public const int TreeRecordValueQwordWireWidth = 8;
    public const int TreeRecordValueDwordWireWidth = 4;
    public const int TaggedVectorCountWireWidth = 1;
    public const byte TaggedVectorChunkContinuationCount = 0xFF;
    public const int TaggedEntryNativeStride = 0x18;
    public const byte TagDwordAtEight = 0;
    public const byte TagDwordAtSixteen = 1;
    public const byte TagQwordAtEightA = 2;
    public const byte TagQwordAtEightB = 3;
    public const byte TagByteAtTwenty = 4;
    public const byte UnsupportedTag = 5;
    public const byte TagWordAtTwenty = 6;
    public const byte TagFlagAndConditionalPayload = 7;
    public const byte TagSetWordOne = 8;
    public const byte SpecialZeroTag = 0xFE;
    public const byte SpecialNoPayloadTag = 0xFF;
    public const int TaggedEntryTagOffset = 0x00;
    public const int TaggedEntryDwordAtEightOffset = 0x08;
    public const int TaggedEntryTagZeroClearedDwordOffset = 0x0C;
    public const int TaggedEntryQwordAtEightOffset = 0x08;
    public const int TaggedEntryDwordAtSixteenOffset = 0x10;
    public const int TaggedEntryFlagWordOffset = 0x14;
    public const int TagSevenDwordPayloadOffset = 0x10;
    public const int TagSevenQwordPayloadOffset = 0x08;
    public const byte ConditionalDwordFlagMask = 0x02;
    public const int TaggedEntryQwordWireWidth = 8;
    public const int TaggedEntryDwordWireWidth = 4;
    public const int TaggedEntryWordWireWidth = 2;
    public const int TaggedEntryFlagWireWidth = 1;
}

public static class EncounterUIUpdateNativeLayout
{
    public const byte Selector = 0x65;
    public const uint CaseVirtualAddress = 0xC21AF1;
    public const uint CaseFileOffset = 0x820EF1;
    public const uint CaseLocalDwordValue = 0x45;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CDBC;
    public const int AllocationSize = 0x50;
    public const uint ConstructorFileOffset = 0x819310;
    public const uint BaseMetadataConstructorFileOffset = 0x8258C0;
    public const uint ConstructorNativeTypeId = 0x65;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89AC4;
    public const uint BodyReaderFileOffset = 0x842940;
    public const uint BodyReaderCallSiteFileOffset = 0x820F3F;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820F32;
    public const uint VtableVirtualAddress = 0x1C6DE08;
    public const string RttiName = ".?AVEncounterUIUpdate@firefall@@";
    public const int QwordFieldOffset = 0x28;
    public const int LeadingBlobOffset = 0x30;
    public const int LeadingBlobLengthPrefixWireWidth = 2;
    public const int TrailingBlobOffset = 0x40;
}

public static class EncounterUIScopeOutNativeLayout
{
    public const byte Selector = 0x66;
    public const uint CaseVirtualAddress = 0xC21B59;
    public const uint CaseFileOffset = 0x820F59;
    public const uint CaseLocalDwordValue = 0x46;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CDD4;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x8194D0;
    public const uint BaseMetadataConstructorFileOffset = 0x825930;
    public const uint ConstructorNativeTypeId = 0x66;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89AAC;
    public const uint BodyReaderFileOffset = 0x356F80;
    public const uint SharedBodyContinuationFileOffset = 0x81F72E;
    public const uint AllocationFailureZeroObjectFileOffset = 0x81F72C;
    public const uint AllocationFailureBranchFileOffset = 0x820F88;
    public const uint SuccessfulConstructionBranchFileOffset = 0x820F9C;
    public const uint VtableVirtualAddress = 0x1C6DE28;
    public const string RttiName = ".?AVEncounterUIScopeOut@firefall@@";
    public const int QwordFieldOffset = 0x28;
    public const int QwordWireWidth = 8;
    public const bool HasBodyReader = true;
}

public static class DisplayUiNotificationNativeLayout
{
    public const byte Selector = 0x67;
    public const uint CaseVirtualAddress = 0xC21BA1;
    public const uint CaseFileOffset = 0x820FA1;
    public const uint CaseLocalDwordValue = 0x47;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CDEC;
    public const int AllocationSize = 0x90;
    public const uint ConstructorFileOffset = 0x819590;
    public const uint BaseMetadataConstructorFileOffset = 0x8259A0;
    public const uint ConstructorNativeTypeId = 0x67;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89AB4;
    public const uint BodyReaderFileOffset = 0x8428B0;
    public const uint BodyReaderCallSiteFileOffset = 0x820FF2;
    public const uint AllocationFailureZeroObjectFileOffset = 0x820FE5;
    public const uint VtableVirtualAddress = 0x1C6DE18;
    public const string RttiName = ".?AVDisplayUiNotification@firefall@@";
    public const int FirstDwordFieldOffset = 0x28;
    public const int SecondDwordFieldOffset = 0x2C;
    public const int RecordVectorBeginOffset = 0x30;
    public const int RecordVectorEndOffset = 0x34;
    public const int RecordVectorCapacityOffset = 0x38;
    public const int RecordEmbeddedStorageDescriptorOffset = 0x3C;
    public const int RecordEmbeddedStoragePointerOffset = 0x40;
    public const int RecordEmbeddedStorageByteCapacityOffset = 0x44;
    public const int RecordEmbeddedStorageUsedOffset = 0x48;
    public const int RecordEmbeddedStorageOffset = 0x4C;
    public const int RecordNativeStride = 0x10;
    public const int RecordLeadingByteOffset = 0x00;
    public const int RecordTextOffset = 0x04;
    public const int RecordDwordOffset = 0x08;
    public const int RecordFloatOffset = 0x0C;
    public const int RecordLeadingByteWireWidth = 1;
    public const int RecordTextTerminatorWireWidth = 1;
    public const int RecordDwordWireWidth = 4;
    public const int RecordFloatWireWidth = 4;
    public const int TrailingByteFieldOffset = 0x8C;
    public const int TrailingByteWireWidth = 1;
    public const int CountWireWidth = 1;
    public const byte ChunkContinuationCount = 0xFF;
}

public static class DisplayMoneyBombBannerNativeLayout
{
    public const byte Selector = 0x68;
    public const uint CaseVirtualAddress = 0xC21C0C;
    public const uint CaseFileOffset = 0x82100C;
    public const uint CaseLocalDwordValue = 0x48;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CE04;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x819750;
    public const uint BaseMetadataConstructorFileOffset = 0x825AA0;
    public const uint ConstructorNativeTypeId = 0x68;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B1C;
    public const uint BodyReaderFileOffset = 0x8420B0;
    public const uint BodyReaderCallSiteFileOffset = 0x82105A;
    public const uint AllocationFailureBranchFileOffset = 0x82103B;
    public const uint AllocationFailureZeroObjectFileOffset = 0x82104D;
    public const uint SuccessfulConstructionBranchFileOffset = 0x82104B;
    public const uint VtableVirtualAddress = 0x1C6DDF8;
    public const string RttiName = ".?AVDisplayMoneyBombBanner@firefall@@";
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const int FirstDwordFieldOffset = 0x28;
    public const int SharedTextFieldOffset = 0x2C;
    public const int SecondDwordFieldOffset = 0x30;
    public const int DwordWireWidth = 4;
    public const int TextTerminatorWireWidth = 1;
    public const int TextChunkSize = 0x100;
}

public static class SetPreloadPositionNativeLayout
{
    public const byte Selector = 0x69;
    public const uint CaseVirtualAddress = 0xC21C74;
    public const uint CaseFileOffset = 0x821074;
    public const uint CaseLocalDwordValue = 0x49;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CE1C;
    public const int AllocationSize = 0x68;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x819900;
    public const uint BaseMetadataConstructorFileOffset = 0x825B10;
    public const uint ConstructorNativeTypeId = 0x69;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B24;
    public const uint BodyReaderFileOffset = 0x6087E0;
    public const uint BodyReaderCallSiteFileOffset = 0x8210C4;
    public const uint AllocationFailureBranchFileOffset = 0x8210A3;
    public const uint AllocationFailureZeroObjectFileOffset = 0x8210B5;
    public const uint SuccessfulConstructionBranchFileOffset = 0x8210B3;
    public const uint VtableVirtualAddress = 0x1C6DDC8;
    public const string RttiName = ".?AVSetPreloadPosition@firefall@@";
    public const int FirstDwordFieldOffset = 0x28;
    public const int DwordFieldCount = 16;
    public const int DwordFieldStride = 4;
    public const int DwordWireWidth = 4;
    public const int DwordRows = 4;
    public const int DwordColumns = 4;
    public const int LastDwordFieldOffset = 0x64;
    public const int NominalInputByteCount = 0x40;
}

public static class PlaySoundIdNativeLayout
{
    public const byte Selector = 0x6A;
    public const uint CaseVirtualAddress = 0xC21CDE;
    public const uint CaseFileOffset = 0x8210DE;
    public const uint CaseLocalDwordValue = 0x4A;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CE34;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x8199B0;
    public const uint BaseMetadataConstructorFileOffset = 0x825B80;
    public const uint ConstructorNativeTypeId = 0x6A;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B0C;
    public const uint BodyReaderFileOffset = 0x841F80;
    public const uint BodyReaderCallSiteFileOffset = 0x82112C;
    public const uint AllocationFailureBranchFileOffset = 0x82110D;
    public const uint AllocationFailureZeroObjectFileOffset = 0x82111F;
    public const uint SuccessfulConstructionBranchFileOffset = 0x82111D;
    public const uint VtableVirtualAddress = 0x1C6DDE8;
    public const string RttiName = ".?AVPlaySoundId@firefall@@";
    public const uint FirstQwordReaderFileOffset = 0x356850;
    public const uint SecondQwordReaderFileOffset = 0x356F80;
    public const int FirstQwordFieldOffset = 0x28;
    public const int SecondQwordFieldOffset = 0x30;
    public const int QwordWireWidth = 8;
    public const int NominalInputByteCount = 0x10;
}

public static class PlaySoundIdAtLocationNativeLayout
{
    public const byte Selector = 0x6B;
    public const uint CaseVirtualAddress = 0xC21D46;
    public const uint CaseFileOffset = 0x821146;
    public const uint CaseLocalDwordValue = 0x4B;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CE4C;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x819A90;
    public const uint BaseMetadataConstructorFileOffset = 0x825BF0;
    public const uint ConstructorNativeTypeId = 0x6B;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B14;
    public const uint BodyReaderFileOffset = 0x841ED0;
    public const uint BodyReaderCallSiteFileOffset = 0x821194;
    public const uint AllocationFailureBranchFileOffset = 0x821175;
    public const uint AllocationFailureZeroObjectFileOffset = 0x821187;
    public const uint SuccessfulConstructionBranchFileOffset = 0x821185;
    public const uint VtableVirtualAddress = 0x1C6DDD8;
    public const string RttiName = ".?AVPlaySoundIdAtLocation@firefall@@";
    public const uint QwordReaderFileOffset = 0x356850;
    public const uint DwordReaderFileOffset = 0x3568E0;
    public const int QwordFieldOffset = 0x28;
    public const int FirstDwordFieldOffset = 0x30;
    public const int SecondDwordFieldOffset = 0x34;
    public const int ThirdDwordFieldOffset = 0x38;
    public const int QwordWireWidth = 8;
    public const int DwordWireWidth = 4;
    public const int NominalInputByteCount = 0x14;
}

public static class PlayDialogScriptMessageNativeLayout
{
    public const byte Selector = 0x6C;
    public const uint CaseVirtualAddress = 0xC21DAE;
    public const uint CaseFileOffset = 0x8211AE;
    public const uint CaseLocalDwordValue = 0x4C;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CE64;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x819B60;
    public const uint BaseMetadataConstructorFileOffset = 0x825C60;
    public const uint ConstructorNativeTypeId = 0x6C;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89AFC;
    public const uint BodyReaderFileOffset = 0x841DE0;
    public const uint BodyReaderCallSiteFileOffset = 0x8211FC;
    public const uint AllocationFailureBranchFileOffset = 0x8211DD;
    public const uint AllocationFailureZeroObjectFileOffset = 0x8211EF;
    public const uint SuccessfulConstructionBranchFileOffset = 0x8211ED;
    public const uint VtableVirtualAddress = 0x1C6DD98;
    public const string RttiName = ".?AVPlayDialogScriptMessage@firefall@@";
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint QwordVectorReaderFileOffset = 0x34F7D0;
    public const int DwordFieldOffset = 0x28;
    public const int DwordWireWidth = 4;
    public const int VectorBeginOffset = 0x2C;
    public const int VectorEndOffset = 0x30;
    public const int VectorCapacityOffset = 0x34;
    public const int RecordWireWidth = 8;
    public const int RecordNativeStride = 8;
    public const int CountWireWidth = 1;
    public const int ChunkContinuationCount = 0xFF;
    public const int TerminalCount = 0;
    public const int MinimumInputByteCount = 5;
}

public static class StopDialogScriptMessageNativeLayout
{
    public const byte Selector = 0x6D;
    public const uint CaseVirtualAddress = 0xC21E16;
    public const uint CaseFileOffset = 0x821216;
    public const uint CaseLocalDwordValue = 0x4D;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CCFC;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x819CC0;
    public const uint BaseMetadataConstructorFileOffset = 0x825CD0;
    public const uint ConstructorNativeTypeId = 0x6D;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89B04;
    public const uint BodyReaderFileOffset = 0x841D40;
    public const uint BodyReaderCallSiteFileOffset = 0x821264;
    public const uint AllocationFailureBranchFileOffset = 0x821245;
    public const uint AllocationFailureZeroObjectFileOffset = 0x821257;
    public const uint SuccessfulConstructionBranchFileOffset = 0x821255;
    public const uint VtableVirtualAddress = 0x1C6DDB8;
    public const string RttiName = ".?AVStopDialogScriptMessage@firefall@@";
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint QwordVectorReaderFileOffset = 0x34F7D0;
    public const uint ByteReaderFileOffset = 0x337180;
    public const int DwordFieldOffset = 0x28;
    public const int DwordWireWidth = 4;
    public const int VectorBeginOffset = 0x2C;
    public const int VectorEndOffset = 0x30;
    public const int VectorCapacityOffset = 0x34;
    public const int RecordWireWidth = 8;
    public const int RecordNativeStride = 8;
    public const int CountWireWidth = 1;
    public const int ChunkContinuationCount = 0xFF;
    public const int TerminalCount = 0;
    public const int TrailingByteFieldOffset = 0x38;
    public const int TrailingByteWireWidth = 1;
    public const int MinimumInputByteCount = 6;
}

public static class PingMapNativeLayout
{
    public const byte Selector = 0x6E;
    public const uint CaseVirtualAddress = 0xC21E7E;
    public const uint CaseFileOffset = 0x82127E;
    public const uint CaseLocalDwordValue = 0x4E;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CD14;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x819E20;
    public const uint BaseMetadataConstructorFileOffset = 0x825D40;
    public const uint ConstructorNativeTypeId = 0x6E;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89AEC;
    public const uint BodyReaderFileOffset = 0x841C60;
    public const uint BodyReaderCallSiteFileOffset = 0x8212CC;
    public const uint AllocationFailureBranchFileOffset = 0x8212AD;
    public const uint AllocationFailureZeroObjectFileOffset = 0x8212BF;
    public const uint SuccessfulConstructionBranchFileOffset = 0x8212BD;
    public const uint VtableVirtualAddress = 0x1C6DDA8;
    public const string RttiName = ".?AVPingMap@firefall@@";
    public const uint DwordReaderFileOffset = 0x3568E0;
    public const uint FinalDwordReaderFileOffset = 0x3370A0;
    public const int FirstDwordFieldOffset = 0x28;
    public const int SecondDwordFieldOffset = 0x2C;
    public const int ThirdDwordFieldOffset = 0x30;
    public const int FourthDwordFieldOffset = 0x34;
    public const int DwordFieldCount = 4;
    public const int DwordWireWidth = 4;
    public const int NominalInputByteCount = 0x10;
}

public static class PingMapMarkerNativeLayout
{
    public const byte Selector = 0x6F;
    public const uint CaseVirtualAddress = 0xC21EE6;
    public const uint CaseFileOffset = 0x8212E6;
    public const uint CaseLocalDwordValue = 0x4F;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CD2C;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x819EF0;
    public const uint BaseMetadataConstructorFileOffset = 0x825DB0;
    public const uint ConstructorNativeTypeId = 0x6F;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89AF4;
    public const uint BodyReaderFileOffset = 0x841B70;
    public const uint BodyReaderCallSiteFileOffset = 0x821334;
    public const uint AllocationFailureBranchFileOffset = 0x821315;
    public const uint AllocationFailureZeroObjectFileOffset = 0x821327;
    public const uint SuccessfulConstructionBranchFileOffset = 0x821325;
    public const uint VtableVirtualAddress = 0x1C6DD78;
    public const string RttiName = ".?AVPingMapMarker@firefall@@";
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const int QwordFieldOffset = 0x28;
    public const int DwordFieldOffset = 0x30;
    public const int QwordWireWidth = 8;
    public const int DwordWireWidth = 4;
    public const int NominalInputByteCount = 0x0C;
}

public static class EncounterPublicInfoNativeLayout
{
    public const byte Selector = 0x70;
    public const uint CaseVirtualAddress = 0xC21F4E;
    public const uint CaseFileOffset = 0x82134E;
    public const uint CaseLocalDwordValue = 0x50;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CD44;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x819FC0;
    public const uint BaseMetadataConstructorFileOffset = 0x825E20;
    public const uint ConstructorNativeTypeId = 0x70;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89A84;
    public const uint BodyReaderFileOffset = 0x841AA0;
    public const uint BodyReaderCallSiteFileOffset = 0x82139C;
    public const uint AllocationFailureBranchFileOffset = 0x82137D;
    public const uint AllocationFailureZeroObjectFileOffset = 0x82138F;
    public const uint SuccessfulConstructionBranchFileOffset = 0x82138D;
    public const uint VtableVirtualAddress = 0x1C6DD68;
    public const uint VtableFileOffset = 0x186C968;
    public const string RttiName = ".?AVEncounterPublicInfo@firefall@@";
    public const uint QwordReaderFileOffset = 0x356F80;
    public const int QwordFieldOffset = 0x28;
    public const int RemainingInputBlobFieldOffset = 0x30;
    public const int RemainingInputVirtualMethodOffset = 0x10;
    public const int QwordWireWidth = 8;
}

public static class RequestActiveEncountersResponseNativeLayout
{
    public const byte Selector = 0x71;
    public const uint CaseVirtualAddress = 0xC21FB6;
    public const uint CaseFileOffset = 0x8213B6;
    public const uint CaseLocalDwordValue = 0x51;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CD5C;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x81A120;
    public const uint BaseMetadataConstructorFileOffset = 0x825E90;
    public const uint ConstructorNativeTypeId = 0x71;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89A8C;
    public const uint BodyReaderFileOffset = 0x841950;
    public const uint BodyReaderCallSiteFileOffset = 0x821404;
    public const uint AllocationFailureBranchFileOffset = 0x8213E5;
    public const uint AllocationFailureZeroObjectFileOffset = 0x8213F7;
    public const uint SuccessfulConstructionBranchFileOffset = 0x8213F5;
    public const uint VtableVirtualAddress = 0x1C6DD88;
    public const uint VtableFileOffset = 0x186C988;
    public const string RttiName = ".?AVRequestActiveEncounters_Response@firefall@@";
    public const int RemainingInputBlobFieldOffset = 0x28;
    public const int RemainingInputVirtualMethodOffset = 0x10;
}

public static class ShoppingListInitNativeLayout
{
    public const byte Selector = 0x72;
    public const uint CaseVirtualAddress = 0xC2201E;
    public const uint CaseFileOffset = 0x82141E;
    public const uint CaseLocalDwordValue = 0x52;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CD74;
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x81A260;
    public const uint BaseMetadataConstructorFileOffset = 0x825F00;
    public const uint ConstructorNativeTypeId = 0x72;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89A74;
    public const uint BodyReaderFileOffset = 0x5B9670;
    public const uint BodyReaderCallSiteFileOffset = 0x82146E;
    public const uint AllocationFailureBranchFileOffset = 0x82144D;
    public const uint AllocationFailureZeroObjectFileOffset = 0x82145F;
    public const uint SuccessfulConstructionBranchFileOffset = 0x82145D;
    public const uint VtableVirtualAddress = 0x1C6DD48;
    public const uint VtableFileOffset = 0x186C948;
    public const string RttiName = ".?AVShoppingListInit@firefall@@";
    public const uint TreeClearFileOffset = 0x6050E0;
    public const uint TreeInsertFileOffset = 0x5B9530;
    public const int TreeFieldOffset = 0x28;
    public const int RecordFirstDwordWireWidth = 4;
    public const int RecordSecondDwordWireWidth = 4;
    public const int CountWireWidth = 1;
    public const int ChunkContinuationCount = 0xFF;
    public const int TerminalCount = 0;
}

public static class SetClientDailyInfoNativeLayout
{
    public const byte Selector = 0x73;
    public const uint CaseVirtualAddress = 0xC22088;
    public const uint CaseFileOffset = 0x821488;
    public const uint CaseLocalDwordValue = 0x53;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CD8C;
    public const int AllocationSize = 0x50;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x81A3D0;
    public const uint BaseMetadataConstructorFileOffset = 0x825F70;
    public const uint ConstructorNativeTypeId = 0x73;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89A7C;
    public const uint BodyReaderFileOffset = 0x8417D0;
    public const uint BodyReaderCallSiteFileOffset = 0x8214D6;
    public const uint AllocationFailureBranchFileOffset = 0x8214B7;
    public const uint AllocationFailureZeroObjectFileOffset = 0x8214C9;
    public const uint SuccessfulConstructionBranchFileOffset = 0x8214C7;
    public const uint VtableVirtualAddress = 0x1C6DD38;
    public const uint VtableFileOffset = 0x186C938;
    public const string RttiName = ".?AVSetClientDailyInfo@firefall@@";
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint CountedRecordReaderFileOffset = 0x70A340;
    public const uint RecordHelperFileOffset = 0x38EC20;
    public const int FirstDwordFieldOffset = 0x28;
    public const int CountedSequenceFieldOffset = 0x2C;
    public const int DwordWireWidth = 4;
    public const int CountWireWidth = 1;
    public const int ChunkContinuationCount = 0xFF;
    public const int TerminalCount = 0;
}

public static class GlobalCounterUpdateNativeLayout
{
    public const byte Selector = 0x74;
    public const uint CaseVirtualAddress = 0xC220F0;
    public const uint CaseFileOffset = 0x8214F0;
    public const uint CaseLocalDwordValue = 0x54;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CDA4;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x81A570;
    public const uint BaseMetadataConstructorFileOffset = 0x825FE0;
    public const uint ConstructorNativeTypeId = 0x74;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89A64;
    public const uint BodyReaderFileOffset = 0x841730;
    public const uint BodyReaderCallSiteFileOffset = 0x82153E;
    public const uint AllocationFailureBranchFileOffset = 0x82151F;
    public const uint AllocationFailureZeroObjectFileOffset = 0x821531;
    public const uint SuccessfulConstructionBranchFileOffset = 0x82152F;
    public const uint VtableVirtualAddress = 0x1C6DD58;
    public const uint VtableFileOffset = 0x186C958;
    public const string RttiName = ".?AVGlobalCounterUpdate@firefall@@";
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const int SharedTextFieldOffset = 0x28;
    public const int QwordFieldOffset = 0x30;
    public const int QwordWireWidth = 8;
    public const int TextTerminatorWireWidth = 1;
    public const int TextChunkSize = 0x100;
}

public static class GlobalCounterMilestoneInfoNativeLayout
{
    public const byte Selector = 0x75;
    public const uint CaseVirtualAddress = 0xC22158;
    public const uint CaseFileOffset = 0x821558;
    public const uint CaseLocalDwordValue = 0x55;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CFDC;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x81A720;
    public const uint BaseMetadataConstructorFileOffset = 0x826050;
    public const uint ConstructorNativeTypeId = 0x75;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89A6C;
    public const uint BodyReaderFileOffset = 0x841430;
    public const uint BodyReaderCallSiteFileOffset = 0x8215A8;
    public const uint SecondBodyReaderFileOffset = 0x355E50;
    public const uint SecondBodyReaderCallSiteFileOffset = 0x8215B2;
    public const uint NestedCollectionReaderFileOffset = 0x83F7D0;
    public const uint AllocationFailureBranchFileOffset = 0x821587;
    public const uint AllocationFailureZeroObjectFileOffset = 0x821599;
    public const uint SuccessfulConstructionBranchFileOffset = 0x821597;
    public const uint VtableVirtualAddress = 0x1C6DD08;
    public const uint VtableFileOffset = 0x186C908;
    public const string RttiName = ".?AVGlobalCounterMilestoneInfo@firefall@@";
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const int FirstCollectionFieldOffset = 0x28;
    public const int SecondCollectionFieldOffset = 0x30;
    public const int CountWireWidth = 1;
    public const int ChunkContinuationCount = 0xFF;
    public const int TerminalCount = 0;
    public const int DwordWireWidth = 4;
    public const int TextTerminatorWireWidth = 1;
    public const int NestedRecordFirstDwordWireWidth = 4;
    public const int NestedRecordSecondDwordWireWidth = 4;
    public const int NestedRecordTextTerminatorWireWidth = 1;
    public const int NestedRecordTrailingDwordWireWidth = 4;
}

public static class ChatMessageListNativeLayout
{
    public const byte Selector = 0x76;
    public const uint CaseVirtualAddress = 0xC221CC;
    public const uint CaseFileOffset = 0x8215CC;
    public const uint CaseLocalDwordValue = 0x56;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8CFF4;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x81A930;
    public const uint BaseMetadataConstructorFileOffset = 0x826130;
    public const uint ConstructorNativeTypeId = 0x76;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89A54;
    public const uint BodyReaderFileOffset = 0x83ECF0;
    public const uint BodyReaderCallSiteFileOffset = 0x82161C;
    public const uint AllocationFailureBranchFileOffset = 0x8215FB;
    public const uint AllocationFailureZeroObjectFileOffset = 0x82160D;
    public const uint SuccessfulConstructionBranchFileOffset = 0x82160B;
    public const uint VtableVirtualAddress = 0x1C6DCF8;
    public const uint VtableFileOffset = 0x186C8F8;
    public const string RttiName = ".?AVChatMessageList@firefall@@";
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint OptionalBlobReaderFileOffset = 0x5F19A0;
    public const uint OptionalBlobLengthReaderFileOffset = 0x12EE170;
    public const uint OptionalBlobUpdateFileOffset = 0x5F1920;
    public const uint OptionalBlobClearFileOffset = 0x49AC30;
    public const uint OptionalQwordReaderFileOffset = 0x5FDA00;
    public const uint OptionalQwordPrimitiveReaderFileOffset = 0x356F80;
    public const uint NestedRecordReaderFileOffset = 0x5E4C20;
    public const int RecordVectorFieldOffset = 0x28;
    public const int RecordNativeStride = 0x40;
    public const int RecordQwordOffset = 0x00;
    public const int RecordFirstTextOffset = 0x08;
    public const int RecordSecondTextOffset = 0x0C;
    public const int RecordFirstByteOffset = 0x10;
    public const int RecordSecondByteOffset = 0x11;
    public const int RecordThirdByteOffset = 0x12;
    public const int RecordOptionalBlobOffset = 0x14;
    public const int RecordOptionalQwordOffset = 0x28;
    public const int RecordNestedReaderOffset = 0x38;
    public const int CountWireWidth = 1;
    public const int ChunkContinuationCount = 0xFF;
    public const int TerminalCount = 0;
}

public static class CurrentLoadoutResponseNativeLayout
{
    public const byte Selector = 0x77;
    public const uint CaseVirtualAddress = 0xC22236;
    public const uint CaseFileOffset = 0x821636;
    public const uint CaseLocalDwordValue = 0x57;
    public const uint CaseSetupArgument2VirtualAddress = 0x1B8D00C;
    public const int AllocationSize = 0x50;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x81AA70;
    public const uint BaseMetadataConstructorFileOffset = 0x8262A0;
    public const uint ConstructorNativeTypeId = 0x77;
    public const uint DescriptorAnchorVirtualAddress = 0x1B89A5C;
    public const uint BodyReaderFileOffset = 0x83E000;
    public const uint BodyReaderCallSiteFileOffset = 0x821684;
    public const uint AllocationFailureBranchFileOffset = 0x821665;
    public const uint AllocationFailureZeroObjectFileOffset = 0x821677;
    public const uint SuccessfulConstructionBranchFileOffset = 0x821675;
    public const uint VtableVirtualAddress = 0x1C6DD28;
    public const uint VtableFileOffset = 0x186C928;
    public const string RttiName = ".?AVCurrentLoadoutResponse@firefall@@";
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint DwordReaderFileOffset = 0x353310;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint VectorReaderFileOffset = 0x5EA3F0;
    public const uint VectorItemReaderFileOffset = 0x5EA1B0;
    public const uint VectorInsertFileOffset = 0x5EA330;
    public const int QwordFieldOffset = 0x28;
    public const int FirstDwordFieldOffset = 0x30;
    public const int SecondDwordFieldOffset = 0x34;
    public const int FirstTextFieldOffset = 0x38;
    public const int SecondTextFieldOffset = 0x3C;
    public const int ThirdDwordFieldOffset = 0x40;
    public const int RecordVectorFieldOffset = 0x44;
    public const int RecordNativeStride = 0x2E0;
    public const int CountWireWidth = 1;
    public const int ChunkContinuationCount = 0xFF;
    public const int TerminalCount = 0;
    public const int RecordFirstDwordOffset = 0x00;
    public const int RecordTextOffset = 0x04;
    public const int RecordFirstNestedReaderOffset = 0x08;
    public const int RecordSecondNestedReaderOffset = 0x14;
    public const int RecordThirdNestedReaderOffset = 0x20;
    public const int RecordFirstTrailingDwordOffset = 0x2C;
    public const int RecordSecondTrailingDwordOffset = 0x30;
    public const int RecordOptionalBlockGateOffset = 0x34;
    public const int RecordOptionalBlockFirstReaderOffset = 0x38;
    public const int RecordOptionalBlockSecondReaderOffset = 0xF8;
    public const int RecordOptionalBlockThirdReaderOffset = 0x1D8;
    public const int RecordOptionalBlockFourthReaderOffset = 0x1E4;
    public const int RecordOptionalBlockFifthReaderOffset = 0x1F0;
    public const int RecordOptionalBlockFirstDwordOffset = 0x2D0;
    public const int RecordOptionalBlockSecondDwordOffset = 0x2D4;
    public const int RecordOptionalBlockByteOffset = 0x2D8;
    public const int RecordOptionalBlockFinalDwordOffset = 0xF4;
}

public static class MatrixSecondaryEmbeddedBlobNativeLayout
{
    public const int LengthPrefixWireWidth = 2;
    public const int ObjectCopyVirtualMethodOffset = 0x10;
    public const int ZeroLengthPayloadWidth = 0;
}
