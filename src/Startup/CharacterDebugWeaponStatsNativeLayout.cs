namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x57 object layout. Word fields are kept offset-only because
/// their semantic meanings are unknown.
/// </summary>
public static class CharacterDebugWeaponStatsNativeLayout
{
    public const int InnerSelector = 0x57;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CDE7;
    public const uint CaseLocalDwordValue = 0x5F;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x5250A0;
    public const uint ConstructorHelperFileOffset = 0x5A05C0;
    public const uint BodyReaderFileOffset = 0x5F4A10;
    public const uint VtableVirtualAddress = 0x1C67238;
    public const string RttiName = ".?AVDebugWeaponStats@Character@firefall@@";
    public const int FirstWordFieldOffset = 0x28;
    public const int WordFieldCount = 8;
    public static IReadOnlyList<int> WordFieldOffsets { get; } =
        Array.AsReadOnly(new[] { 0x28, 0x2A, 0x2C, 0x2E, 0x30, 0x32, 0x34, 0x36 });
}
