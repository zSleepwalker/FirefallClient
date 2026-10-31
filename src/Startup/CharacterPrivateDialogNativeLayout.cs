namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x79 raw field layout. Field meanings remain unknown.
/// </summary>
public static class CharacterPrivateDialogNativeLayout
{
    public const int InnerSelector = 0x79;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint InnerSelectorTableEntryOffset = 0x98;
    public const uint CaseVirtualAddress = 0xC1E39E;
    public const uint CaseFileOffset = 0x81D79E;
    public const uint CaseLocalDwordValue = 0x81;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x526840;
    public const uint ConstructorCallSiteFileOffset = 0x81D7BF;
    public const uint BaseConstructorHelperFileOffset = 0x5A1510;
    public const int BaseConstructorArgument = 0x79;
    public const uint AllocationFailureContinuationFileOffset = 0x81D7C8;
    public const uint BodyReaderCallSiteFileOffset = 0x81D7D5;
    public const uint BodyReaderFileOffset = 0x5EE900;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C67018;
    public const string RttiName = ".?AVPrivateDialog@Character@firefall@@";

    public const int FirstDwordFieldOffset = 0x28;
    public const int QwordFieldOffset = 0x30;
    public const int SecondDwordFieldOffset = 0x38;
}
