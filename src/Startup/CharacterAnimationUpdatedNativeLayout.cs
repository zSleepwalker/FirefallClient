namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x5D object layout. Field meanings remain unknown.
/// </summary>
public static class CharacterAnimationUpdatedNativeLayout
{
    public const int InnerSelector = 0x5D;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CFAB;
    public const uint CaseLocalDwordValue = 0x65;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x525570;
    public const uint ConstructorHelperFileOffset = 0x5A0880;
    public const uint BodyReaderFileOffset = 0x5F30F0;
    public const uint VtableVirtualAddress = 0x1C671D8;
    public const string RttiName = ".?AVAnimationUpdated@Character@firefall@@";

    public const int RawWordFieldOffset = 0x28;
    public const int RawByteFieldOffset = 0x2A;
    public const int NominalInputByteCount = 3;
}
