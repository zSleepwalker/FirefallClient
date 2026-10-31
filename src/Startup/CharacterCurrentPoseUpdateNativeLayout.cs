namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x6E raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterCurrentPoseUpdateNativeLayout
{
    public const int InnerSelector = 0x6E;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D454;
    public const uint CaseLocalDwordValue = 0x76;
    public const int AllocationSize = 0x58;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x526030;
    public const uint BaseConstructorHelperFileOffset = 0x5A1010;
    public const int BaseConstructorArgument = 0x6E;
    public const uint AllocationFailureContinuationFileOffset = 0x81D47E;
    public const uint BodyReaderContinuationFileOffset = 0x81D480;
    public const uint BodyReaderFileOffset = 0x5EFE30;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint DwordReaderFileOffset = 0x3568E0;
    public const uint ThreeByteReaderFileOffset = 0x336B50;
    public const uint AlternateWordReaderFileOffset = 0x6097B0;
    public const uint AlternateByteReaderFileOffset = 0x356CF0;
    public const uint VtableVirtualAddress = 0x1C670D8;
    public const string RttiName = ".?AVCurrentPoseUpdate@Character@firefall@@";

    public const int GateByteOffset = 0x28;
    public const int FirstConditionalByteOffset = 0x29;
    public const int FirstConditionalWordOffset = 0x2A;
    public const int AlwaysByteOffset = 0x2C;
    public const int SecondConditionalWordOffset = 0x2E;
    public const int ConditionalThreeByteBlockOffset = 0x30;
    public const int ConditionalDwordBlockOffset = 0x34;
    public const int MidModeSingleByteOffset = 0x40;
    public const int MidModeThreeByteBlockOffset = 0x41;
    public const int MidModeFourWordBlockOffset = 0x44;
    public const int TailModeThreeByteBlockOffset = 0x4C;
    public const int TailModeThreeWordBlockOffset = 0x50;
}
