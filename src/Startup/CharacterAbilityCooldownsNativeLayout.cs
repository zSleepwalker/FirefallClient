namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x76 raw field layout. Field meanings remain unknown.
/// </summary>
public static class CharacterAbilityCooldownsNativeLayout
{
    public const int InnerSelector = 0x76;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint InnerSelectorTableEntryOffset = 0x8C;
    public const uint CaseVirtualAddress = 0xC1E2EB;
    public const uint CaseFileOffset = 0x81D6EB;
    public const uint CaseLocalDwordValue = 0x7E;
    public const int AllocationSize = 0x38;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x5265B0;
    public const uint ConstructorCallSiteFileOffset = 0x81D70C;
    public const uint BaseConstructorHelperFileOffset = 0x5A13C0;
    public const int BaseConstructorArgument = 0x76;
    public const uint AllocationFailureContinuationFileOffset = 0x81D715;
    public const uint BodyReaderCallSiteFileOffset = 0x81D722;
    public const uint BodyReaderFileOffset = 0x5EEE60;
    public const uint VtableVirtualAddress = 0x1C67038;
    public const string RttiName = ".?AVAbilityCooldowns@Character@firefall@@";

    public const int PayloadOffset = 0x28;
    public const int PayloadStateSize = 0x10;
    public const uint PayloadStateInitializer = 0x1C80CC4;
}
