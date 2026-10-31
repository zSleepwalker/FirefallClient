namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x69 raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterTookHitNativeLayout
{
    public const int InnerSelector = 0x69;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D2F3;
    public const uint CaseLocalDwordValue = 0x71;
    public const int AllocationSize = 0x60;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x525CF0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0DE0;
    public const int BaseConstructorArgument = 0x69;
    public const uint AllocationFailureContinuationFileOffset = 0x81D31D;
    public const uint BodyReaderContinuationFileOffset = 0x81D31F;
    public const uint BodyReaderFileOffset = 0x5F0FC0;
    public const uint ConditionalRecordReaderFileOffset = 0x5F0CF0;
    public const uint ConditionalQwordReaderFileOffset = 0x5FDA00;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint DwordReaderFileOffset = 0x353310;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint VtableVirtualAddress = 0x1C67128;
    public const string RttiName = ".?AVTookHit@Character@firefall@@";

    public const int ConditionalRecordGateByteOffset = 0x28;
    public const int ConditionalRecordFirstQwordOffset = 0x30;
    public const int ConditionalRecordNestedGateByteOffset = 0x38;
    public const int ConditionalRecordSecondQwordOffset = 0x40;
    public const int ConditionalRecordDwordOffset = 0x48;
    public const int ConditionalRecordByteOffset = 0x4C;
    public const int FirstTrailingByteFieldOffset = 0x50;
    public const int SecondTrailingByteFieldOffset = 0x51;
    public const int TrailingWordFieldOffset = 0x58;
    public const int FinalByteFieldOffset = 0x5A;
    public const int MinimumInputByteCount = 6;
}
