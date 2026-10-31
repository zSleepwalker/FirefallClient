namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Observed selector-0x55 object and reader layout. Field meanings remain
/// unknown; object offsets are relative to the allocated message object.
/// </summary>
public static class CharacterTookDebugWeaponHitPublicNativeLayout
{
    public const int InnerSelector = 0x55;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CD65;
    public const uint CaseLocalDwordValue = 0x5D;
    public const int AllocationSize = 0xA8;
    public const uint ConstructorFileOffset = 0x524EE0;
    public const uint ConstructorHelperFileOffset = 0x5A04E0;
    public const uint BodyReaderFileOffset = 0x5F60F0;
    public const uint VtableVirtualAddress = 0x1C67258;
    public const string RttiName = ".?AVTookDebugWeaponHitPublic@Character@firefall@@";

    public const int FirstByteFieldOffset = 0;
    public const int RawWordFieldOffset = 0x0C;
    public static IReadOnlyList<int> RawDwordFieldOffsets { get; } =
        Array.AsReadOnly(new[] { 0x04, 0x08, 0x10, 0x14, 0x18, 0x1C, 0x20, 0x24, 0x28 });
    public const int FixedPrefixByteCount = 0x27;

    public const int FirstConditionalTailMarkerOffset = 0x30;
    public const int FirstConditionalTailRawQwordOffset = 0x38;
    public const int FirstConditionalTailRawDwordGroupOffset = 0x40;
    public const int FirstConditionalTailFourFloatGroupOffset = 0x4C;
    public const int FirstConditionalTailTrailingBytesOffset = 0x5C;
    public const int FirstConditionalTailPayloadByteCount = 0x26;
    public const uint FirstConditionalTailReaderFileOffset = 0x5F4FD0;
    public const uint FirstConditionalTailPayloadReaderFileOffset = 0x5F4D60;

    public const int SecondConditionalTailMarkerOffset = 0x60;
    public const int SecondConditionalTailCopiedPrefixOffset = 0x68;
    public const int SecondConditionalTailCopiedPrefixDwordCount = 3;
    public const int SecondConditionalTailVectorBeginOffset = 0x74;
    public const int SecondConditionalTailVectorEndOffset = 0x78;
    public const int SecondConditionalTailVectorCapacityOffset = 0x7C;
    public const int SecondConditionalTailRecordByteCount = 0x40;
    public const int SecondConditionalTailRecordDwordCount = 16;
    public const int SecondConditionalTailCountRepeatMarker = 0xFF;
    public const uint SecondConditionalTailReaderFileOffset = 0x5F5FB0;
    public const int SecondConditionalTailReaderByteCount = 0x121;
    public const uint SecondConditionalTailRecordReaderFileOffset = 0x6087E0;

    public const int ConstructorZeroedByteField1Offset = 0x58;
    public const int ConstructorZeroedByteField2Offset = 0x88;
}
