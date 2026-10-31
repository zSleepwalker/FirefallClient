namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x6B raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterDealtHitNativeLayout
{
    public const int InnerSelector = 0x6B;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D375;
    public const uint CaseLocalDwordValue = 0x73;
    public const int AllocationSize = 0x58;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x525E60;
    public const uint BaseConstructorHelperFileOffset = 0x5A0EC0;
    public const int BaseConstructorArgument = 0x6B;
    public const uint AllocationFailureContinuationFileOffset = 0x81D39F;
    public const uint BodyReaderContinuationFileOffset = 0x81D3A1;
    public const uint BodyReaderFileOffset = 0x5F0800;
    public const uint ConditionalRecordReaderFileOffset = 0x5F0CF0;
    public const uint ConditionalQwordReaderFileOffset = 0x5FDA00;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint DwordReaderFileOffset = 0x353310;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint VtableVirtualAddress = 0x1C670F8;
    public const string RttiName = ".?AVDealtHit@Character@firefall@@";

    public const int ConditionalRecordGateByteOffset = 0x28;
    public const int ConditionalRecordFirstQwordOffset = 0x30;
    public const int ConditionalRecordNestedGateByteOffset = 0x38;
    public const int ConditionalRecordSecondQwordOffset = 0x40;
    public const int ConditionalRecordDwordOffset = 0x48;
    public const int ConditionalRecordByteOffset = 0x4C;
    public const int FirstTrailingByteFieldOffset = 0x50;
    public const int SecondTrailingByteFieldOffset = 0x51;
    public const int MinimumInputByteCount = 3;
}
