namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x5F field layout. The field meanings remain unknown.
/// </summary>
public static class CharacterWeaponProjectileFiredNativeLayout
{
    public const int InnerSelector = 0x5F;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D02D;
    public const uint CaseLocalDwordValue = 0x67;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x525700;
    public const uint ConstructorHelperFileOffset = 0x5A0960;
    public const uint BodyReaderFileOffset = 0x5F2990;
    public const uint OptionalTailReaderFileOffset = 0x5F27E0;
    public const uint WordReaderFileOffset = 0x34B740;
    public const uint VtableVirtualAddress = 0x1C671C8;
    public const string RttiName = ".?AVWeaponProjectileFired@Character@firefall@@";

    public const int PrefixWordFieldOffset = 0x28;
    public const int PrefixWordCount = 4;
    public const int OptionalTailInputGateOffset = 8;
    public const int OptionalTailMarkerFieldOffset = 0x30;
    public const int OptionalTailPayloadFieldOffset = 0x32;
    public const int OptionalTailPayloadWordCount = 3;
    public const int InputByteCountWithoutOptionalTail = 9;
    public const int InputByteCountWithOptionalTail = 15;
}
