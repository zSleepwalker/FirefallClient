namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x61 raw field layout. Semantic meanings remain unknown.
/// </summary>
public static class CharacterProjectileHitReportedNativeLayout
{
    public const int InnerSelector = 0x61;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D0CF;
    public const uint CaseLocalDwordValue = 0x69;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x525840;
    public const uint BaseConstructorHelperFileOffset = 0x5A0A60;
    public const int BaseConstructorArgument = 0x61;
    public const uint BodyReaderFileOffset = 0x5F20E0;
    public const uint VtableVirtualAddress = 0x1C67198;
    public const string RttiName = ".?AVProjectileHitReported@Character@firefall@@";

    public const int WordField0Offset = 0x28;
    public const int WordField1Offset = 0x2A;
    public const int ByteField0Offset = 0x2C;
    public const int ByteField1Offset = 0x2D;
    public const int NominalInputByteCount = 6;
}
