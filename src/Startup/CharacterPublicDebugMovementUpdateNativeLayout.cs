namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x70 raw field layout. Field meanings remain unknown.
/// </summary>
public static class CharacterPublicDebugMovementUpdateNativeLayout
{
    public const int InnerSelector = 0x70;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint InnerSelectorTableEntryOffset = 0x74;
    public const uint CaseVirtualAddress = 0xC1E0FA;
    public const uint CaseFileOffset = 0x81D4FA;
    public const uint CaseLocalDwordValue = 0x78;
    public const int AllocationSize = 0x5A0;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x526180;
    public const uint ConstructorCallSiteFileOffset = 0x81D51E;
    public const uint BaseConstructorHelperFileOffset = 0x5A1100;
    public const int BaseConstructorArgument = 0x70;
    public const uint ArrayInitializerFileOffset = 0x60E6A0;
    public const uint AllocationFailureContinuationFileOffset = 0x81D527;
    public const uint BodyReaderCallSiteFileOffset = 0x81D535;
    public const uint BodyReaderFileOffset = 0x5FF970;
    public const uint NestedRecordReaderFileOffset = 0x608370;
    public const uint TaggedFieldReaderFileOffset = 0x5FF760;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint DwordReaderFileOffset = 0x3568E0;
    public const uint FourDwordBlockReaderFileOffset = 0x384B40;
    public const uint VtableVirtualAddress = 0x1C670A8;
    public const string RttiName = ".?AVPublicDebugMovementUpdate@Character@firefall@@";

    public const int ArrayOffset = 0x28;
    public const int ArrayElementCount = 7;
    public const int ArrayElementStride = 0xC8;
    public const int ElementFirstWordOffset = 0;
    public const int ElementNestedRecordOffset = 4;
    public const int ElementSecondWordOffset = 0x70;
    public const int ElementTaggedByteOffset = 0x74;
    public const int NestedRecordFourDwordBlockOffset = 0x50;
    public const int NestedRecordFourDwordBlockSize = 0x10;
    public const int NestedRecordPresenceWordOffset = 0x60;
    public const int NestedRecordTrailingDwordsOffset = 0x64;
    public const int NestedRecordTrailingDwordCount = 3;
    public const int ConstructorInitializedWordValue = 0x0101;
    public const int ReaderWrittenPresenceWordValue = 1;

    public static readonly int[] ConstructorInitializedWordOffsets =
        [0x88, 0x150, 0x218, 0x2E0, 0x3A8, 0x470, 0x538];

    public static readonly int[] ConstructorInitializedByteOffsets =
        [0x9C, 0x164, 0x22C, 0x2F4, 0x3BC, 0x484, 0x54C];

    public const uint FirstWordReaderCallSiteFileOffset = 0x5FF982;
    public const uint FirstNestedRecordReaderCallSiteFileOffset = 0x5FF98C;
    public const uint FirstSecondWordReaderCallSiteFileOffset = 0x5FF997;
    public const uint FirstTaggedFieldReaderCallSiteFileOffset = 0x5FF9A1;
    public const int ReaderCallSiteStride = 0x2D;
}
