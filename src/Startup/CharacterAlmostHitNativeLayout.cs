namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x6A raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterAlmostHitNativeLayout
{
    public const int InnerSelector = 0x6A;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D344;
    public const uint CaseLocalDwordValue = 0x72;
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x525DD0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0E50;
    public const int BaseConstructorArgument = 0x6A;
    public const uint AllocationFailureContinuationFileOffset = 0x81F72C;
    public const uint BodyReaderContinuationFileOffset = 0x81F72E;
    public const uint BodyReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C67108;
    public const string RttiName = ".?AVAlmostHit@Character@firefall@@";

    public const int QwordFieldOffset = 0x28;
    public const int NominalInputByteCount = 8;
}
