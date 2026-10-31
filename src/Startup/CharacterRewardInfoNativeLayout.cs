namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x58 object layout. Field meanings remain unknown.
/// </summary>
public static class CharacterRewardInfoNativeLayout
{
    public const int InnerSelector = 0x58;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CE38;
    public const uint CaseLocalDwordValue = 0x60;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x525120;
    public const uint ConstructorHelperFileOffset = 0x5A0630;
    public const uint BodyReaderFileOffset = 0x5F47C0;
    public const uint VtableVirtualAddress = 0x1C67218;
    public const string RttiName = ".?AVRewardInfo@Character@firefall@@";

    public const int FirstByteFieldOffset = 0x28;
    public const int SecondByteFieldOffset = 0x29;
    public const int RawQwordFieldOffset = 0x30;
    public static IReadOnlyList<int> RawDwordFieldOffsets { get; } =
        Array.AsReadOnly(new[] { 0x38, 0x3C, 0x40 });
    public const int NominalInputByteCount = 0x16;
    public const int ReaderLastWrittenFieldEndOffset = 0x44;

    public const int ConstructorZeroedDwordField1Offset = 0x30;
    public const int ConstructorZeroedDwordField2Offset = 0x34;
}
