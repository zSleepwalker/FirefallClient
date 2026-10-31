namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x6F raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterConfirmedPoseUpdateNativeLayout
{
    public const int InnerSelector = 0x6F;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D4A6;
    public const uint CaseLocalDwordValue = 0x77;
    public const int AllocationSize = 0xC0;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x5260B0;
    public const uint BaseConstructorHelperFileOffset = 0x5A1090;
    public const int BaseConstructorArgument = 0x6F;
    public const uint AllocationFailureContinuationFileOffset = 0x81D4D3;
    public const uint BodyReaderContinuationFileOffset = 0x81D4D5;
    public const uint BodyReaderFileOffset = 0x5EFAB0;
    public const uint ConditionalRecordReaderFileOffset = 0x5EF8D0;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint DwordReaderFileOffset = 0x3568E0;
    public const uint FourDwordBlockReaderFileOffset = 0x384B40;
    public const uint AlternateWordReaderFileOffset = 0x6097B0;
    public const uint NestedFieldReaderFileOffset = 0x5FFB00;
    public const uint VtableVirtualAddress = 0x1C670B8;
    public const string RttiName = ".?AVConfirmedPoseUpdate@Character@firefall@@";

    public const int InitialWordFieldOffset = 0xB4;
    public const int FirstGateByteOffset = 0xA6;
    public const int SecondGateByteOffset = 0xA7;
    public const int FirstConditionalDwordBlockOffset = 0x2C;
    public const int ConditionalSixteenByteBlockOffset = 0x78;
    public const int ConditionalFollowingWordOffset = 0xA4;
    public const int AlwaysDwordBlockOffset = 0x98;
    public const int SecondConditionalDwordBlockOffset = 0x8C;
    public const int TrailingWordBlockOffset = 0xA8;
    public const int NestedFieldOffset = 0xB0;
    public const int FinalWordFieldOffset = 0xB8;
    public const int ConstructorInitializedWordOffset = 0x88;
    public const int ConstructorInitializedWordValue = 0x0101;
    public const int ConstructorInitializedDwordOffset = 0xB0;
    public const int ConstructorInitializedDwordValue = 0;
}
