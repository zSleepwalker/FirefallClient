namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x78 raw field layout. Field meanings remain unknown.
/// </summary>
public static class CharacterOpenMovieDialogNativeLayout
{
    public const int InnerSelector = 0x78;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint InnerSelectorTableEntryOffset = 0x94;
    public const uint CaseVirtualAddress = 0xC1E36D;
    public const uint CaseFileOffset = 0x81D76D;
    public const uint CaseLocalDwordValue = 0x80;
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x526730;
    public const uint ConstructorCallSiteFileOffset = 0x81D792;
    public const uint BaseConstructorHelperFileOffset = 0x5A14A0;
    public const int BaseConstructorArgument = 0x78;
    public const uint AllocationFailureContinuationFileOffset = 0x81FB25;
    public const uint BodyReaderSetupFileOffset = 0x81FB27;
    public const uint BodyReaderCallSiteFileOffset = 0x81FB34;
    public const uint BodyReaderFileOffset = 0x12EE010;
    public const uint VtableVirtualAddress = 0x1C67028;
    public const string RttiName = ".?AVOpenMovieDialog@Character@firefall@@";

    public const int FirstTextFieldOffset = 0x28;
}
