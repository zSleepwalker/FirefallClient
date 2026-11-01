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
    public const int SelectorMinimum = 0x20;
    public const int SelectorMaximum = 0x7A;
    public const int EntryCount = 0x5B;
    public const uint DefaultZeroReturnFileOffset = 0x8217D9;

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
