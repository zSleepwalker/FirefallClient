namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x72 raw field layout. Field meanings remain unknown.
/// </summary>
public static class CharacterForcedMovementCancelledNativeLayout
{
    public const int InnerSelector = 0x72;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint InnerSelectorTableEntryOffset = 0x7C;
    public const uint CaseVirtualAddress = 0xC1E1A3;
    public const uint CaseFileOffset = 0x81D5A3;
    public const uint CaseLocalDwordValue = 0x7A;
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x5262C0;
    public const uint ConstructorCallSiteFileOffset = 0x81D5C4;
    public const uint BaseConstructorHelperFileOffset = 0x5A11E0;
    public const int BaseConstructorArgument = 0x72;
    public const uint AllocationFailureContinuationFileOffset = 0x81D5CD;
    public const uint BodyReaderCallSiteFileOffset = 0x81D5DA;
    public const uint BodyReaderFileOffset = 0x5EF490;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint VtableVirtualAddress = 0x1C67078;
    public const string RttiName = ".?AVForcedMovementCancelled@Character@firefall@@";

    public const int FirstReaderFieldOffset = 0x28;
    public const int SecondReaderFieldOffset = 0x2C;
}
