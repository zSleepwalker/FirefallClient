namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x6C raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterKilledNativeLayout
{
    public const int InnerSelector = 0x6C;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D3C6;
    public const uint CaseLocalDwordValue = 0x74;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x525F20;
    public const uint BaseConstructorHelperFileOffset = 0x5A0F30;
    public const int BaseConstructorArgument = 0x6C;
    public const uint AllocationFailureContinuationFileOffset = 0x81D3F0;
    public const uint BodyReaderContinuationFileOffset = 0x81D3F2;
    public const uint BodyReaderFileOffset = 0x5F0560;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint VtableVirtualAddress = 0x1C670E8;
    public const string RttiName = ".?AVKilled@Character@firefall@@";

    public const int WordFieldOffset = 0x28;
    public const int QwordFieldOffset = 0x30;
    public const int FirstByteFieldOffset = 0x38;
    public const int SecondByteFieldOffset = 0x39;
    public const int ThirdByteFieldOffset = 0x3A;
    public const int NominalInputByteCount = 13;
}
