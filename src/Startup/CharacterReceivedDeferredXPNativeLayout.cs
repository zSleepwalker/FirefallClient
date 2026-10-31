namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x5A count-blocked vector layout. Record field meanings remain
/// unknown.
/// </summary>
public static class CharacterReceivedDeferredXPNativeLayout
{
    public const int InnerSelector = 0x5A;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CEDC;
    public const uint CaseLocalDwordValue = 0x62;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x5252A0;
    public const uint ConstructorHelperFileOffset = 0x5A0710;
    public const uint BodyReaderFileOffset = 0x5F3A80;
    public const uint VectorGrowthHelperFileOffset = 0x5F3880;
    public const uint VtableVirtualAddress = 0x1C67208;
    public const string RttiName = ".?AVReceivedDeferredXP@Character@firefall@@";

    public const int VectorBeginOffset = 0x28;
    public const int VectorEndOffset = 0x2C;
    public const int VectorCapacityOffset = 0x30;
    public const int RecordDwordCount = 4;
    public const int RecordByteCount = 0x10;
    public const int CountFieldByteCount = 1;
    public const int CountBlockRepeatMarker = 0xFF;
}
