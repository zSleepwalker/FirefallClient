namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x65 raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterJumpActionedNativeLayout
{
    public const int InnerSelector = 0x65;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D1F3;
    public const uint CaseLocalDwordValue = 0x6D;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x525AF0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0C20;
    public const int BaseConstructorArgument = 0x65;
    public const uint AllocationFailureContinuationFileOffset = 0x81D21D;
    public const uint BodyReaderContinuationFileOffset = 0x81D21F;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint VtableVirtualAddress = 0x1C67168;
    public const string RttiName = ".?AVJumpActioned@Character@firefall@@";

    public const int WordFieldOffset = 0x28;
    public const int NominalInputByteCount = 2;
}
