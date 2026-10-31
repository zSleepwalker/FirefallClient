namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x5C construction metadata. Its shared body reader establishes
/// layout reuse, not semantic equivalence with selector 0x5B.
/// </summary>
public static class CharacterPrivateCombatLogNativeLayout
{
    public const int InnerSelector = 0x5C;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CF81;
    public const uint CaseLocalDwordValue = 0x64;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x525480;
    public const uint ConstructorHelperFileOffset = 0x5A0810;
    public const uint BodyReaderFileOffset = CharacterPublicCombatLogNativeLayout.BodyReaderFileOffset;
    public const uint SharedBodyReaderSetupFileOffset = 0x81CF5B;
    public const uint VtableVirtualAddress = 0x1C671E8;
    public const string RttiName = ".?AVPrivateCombatLog@Character@firefall@@";

    public const int ConstructorZeroedByteFieldOffset = 0x28;
    public const int EmbeddedBlobFieldOffset = 0x2C;
    public const uint EmbeddedBlobVtableVirtualAddress = 0x1C80CC4;
    public const int BodyReaderThisObjectOffset = CharacterPublicCombatLogNativeLayout.BodyReaderThisObjectOffset;
    public const int BlobDestinationOffsetFromReaderThis =
        CharacterPublicCombatLogNativeLayout.BlobDestinationOffsetFromReaderThis;
}
