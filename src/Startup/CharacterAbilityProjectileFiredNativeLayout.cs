namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x60 raw field layout. Semantic meanings remain unknown.
/// </summary>
public static class CharacterAbilityProjectileFiredNativeLayout
{
    public const int InnerSelector = 0x60;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D07E;
    public const uint CaseLocalDwordValue = 0x68;
    public const int AllocationSize = 0x68;
    public const uint ConstructorFileOffset = 0x5257A0;
    public const uint BaseConstructorHelperFileOffset = 0x5A09F0;
    public const int BaseConstructorArgument = 0x60;
    public const uint BodyReaderFileOffset = 0x5F22A0;
    public const uint OptionalQwordReaderFileOffset = 0x5FDA00;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C671A8;
    public const string RttiName = ".?AVAbilityProjectileFired@Character@firefall@@";

    public const int WordField0Offset = 0x28;
    public const int WordField1Offset = 0x2A;
    public const int WordField2Offset = 0x2C;
    public const int WordField3Offset = 0x2E;
    public const int WordField4Offset = 0x30;
    public const int WordField5Offset = 0x32;
    public const int WordField6Offset = 0x34;
    public const int WordField7Offset = 0x36;
    public const int DwordField0Offset = 0x38;
    public const int DwordField1Offset = 0x3C;
    public const int ByteField0Offset = 0x40;
    public const int DwordField2Offset = 0x44;
    public const int ByteField1Offset = 0x48;
    public const int DwordField3Offset = 0x4C;
    public const int DwordField4Offset = 0x50;
    public const int OptionalQwordPresenceFieldOffset = 0x58;
    public const int OptionalQwordFieldOffset = 0x60;
    public const int OptionalQwordInputGateOffset = 0x26;
    public const int OptionalQwordByteCount = 8;
    public const int InputByteCountWithoutOptionalQword = 0x27;
    public const int InputByteCountWithOptionalQword = 0x2F;
}
