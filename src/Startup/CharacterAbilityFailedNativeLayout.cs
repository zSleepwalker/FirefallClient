namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x75 raw field layout. Field meanings remain unknown.
/// </summary>
public static class CharacterAbilityFailedNativeLayout
{
    public const int InnerSelector = 0x75;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint InnerSelectorTableEntryOffset = 0x88;
    public const uint CaseVirtualAddress = 0xC1E29A;
    public const uint CaseFileOffset = 0x81D69A;
    public const uint CaseLocalDwordValue = 0x7D;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x5264C0;
    public const uint ConstructorCallSiteFileOffset = 0x81D6BB;
    public const uint BaseConstructorHelperFileOffset = 0x5A1330;
    public const int BaseConstructorArgument = 0x75;
    public const uint AllocationFailureContinuationFileOffset = 0x81D6C4;
    public const uint BodyReaderCallSiteFileOffset = 0x81D6D1;
    public const uint BodyReaderFileOffset = 0x5EF090;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint VtableVirtualAddress = 0x1C67058;
    public const string RttiName = ".?AVAbilityFailed@Character@firefall@@";

    public const int FirstReaderFieldOffset = 0x28;
    public const int SecondReaderFieldOffset = 0x2C;
    public const int PayloadOffset = 0x30;
    public const int PayloadStateSize = 0x10;
    public const uint PayloadStateInitializer = 0x1C80CC4;
}
