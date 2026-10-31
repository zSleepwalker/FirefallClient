namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x7A raw field layout. Field meanings remain unknown.
/// </summary>
public static class CharacterPublicDialogNativeLayout
{
    public const int InnerSelector = 0x7A;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint InnerSelectorTableEntryOffset = 0x9C;
    public const uint CaseVirtualAddress = 0xC1E3EF;
    public const uint CaseFileOffset = 0x81D7EF;
    public const uint CaseLocalDwordValue = 0x82;
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x5268D0;
    public const uint ConstructorCallSiteFileOffset = 0x81D810;
    public const uint BaseConstructorHelperFileOffset = 0x5A1580;
    public const int BaseConstructorArgument = 0x7A;
    public const uint AllocationFailureContinuationFileOffset = 0x81D819;
    public const uint BodyReaderCallSiteFileOffset = 0x81D826;
    public const uint BodyReaderFileOffset = 0x5EE730;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint VtableVirtualAddress = 0x1C66FF8;
    public const string RttiName = ".?AVPublicDialog@Character@firefall@@";

    public const int FirstDwordFieldOffset = 0x28;
    public const int SecondDwordFieldOffset = 0x2C;
}
