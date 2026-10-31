namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x67 raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterRespawnedNativeLayout
{
    public const int InnerSelector = 0x67;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D271;
    public const uint CaseLocalDwordValue = 0x6F;
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x525BF0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0D00;
    public const int BaseConstructorArgument = 0x67;
    public const uint AllocationFailureContinuationFileOffset = 0x81D29B;
    public const uint BodyReaderContinuationFileOffset = 0x81D29D;
    public const uint BodyReaderFileOffset = 0x5F1550;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint FirstByteReaderFileOffset = 0x356CF0;
    public const uint SecondByteReaderFileOffset = 0x337180;
    public const uint VtableVirtualAddress = 0x1C67138;
    public const string RttiName = ".?AVRespawned@Character@firefall@@";

    public const int WordFieldOffset = 0x28;
    public const int FirstByteFieldOffset = 0x2A;
    public const int SecondByteFieldOffset = 0x2B;
    public const int NominalInputByteCount = 4;
}
