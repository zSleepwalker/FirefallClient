namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x62 raw field layout. Semantic meanings remain unknown.
/// </summary>
public static class CharacterStumbleNativeLayout
{
    public const int InnerSelector = 0x62;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D120;
    public const uint CaseLocalDwordValue = 0x6A;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x5258C0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0AD0;
    public const int BaseConstructorArgument = 0x62;
    public const uint BodyReaderFileOffset = 0x5F1ED0;
    public const uint VtableVirtualAddress = 0x1C67188;
    public const string RttiName = ".?AVStumble@Character@firefall@@";

    public const int WordField0Offset = 0x28;
    public const int WordField1Offset = 0x2A;
    public const int ByteFieldOffset = 0x2C;
    public const int NominalInputByteCount = 5;
}
