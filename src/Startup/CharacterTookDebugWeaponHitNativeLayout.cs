namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x56 construction metadata. Its body reader is shared with the
/// selector-0x55 layout; RTTI and field semantics are not interchangeable.
/// </summary>
public static class CharacterTookDebugWeaponHitNativeLayout
{
    public const int InnerSelector = 0x56;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CDBA;
    public const uint CaseLocalDwordValue = 0x5E;
    public const int AllocationSize = 0xA8;
    public const uint ConstructorFileOffset = 0x524FC0;
    public const uint ConstructorHelperFileOffset = 0x5A0550;
    public const uint BodyReaderFileOffset =
        CharacterTookDebugWeaponHitPublicNativeLayout.BodyReaderFileOffset;
    public const uint VtableVirtualAddress = 0x1C67248;
    public const string RttiName = ".?AVTookDebugWeaponHit@Character@firefall@@";
    public const uint SharedBodyReaderSetupFileOffset = 0x81CD94;
    public const int ConstructorZeroedByteField1Offset = 0x58;
    public const int ConstructorZeroedByteField2Offset = 0x88;
}

public static class CharacterTookDebugWeaponHitConsumerNativeLayout
{
    public const int MessageDiscriminator = 0x56;
    public const int ProtocolHandlerVtableSlotOffset = 0xDC;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECEA30;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACDE30;
    public const uint ConsumerFunctionFileOffset = 0xACDE30;
    public const uint OwnerStatePointerOffset = 0x43D0;
    public const uint OwnerTreeOffset = 0x30;
    public const int InputCollectionPointerOffset = 0x28;
    public const int InputCollectionCountOffset = 0x2C;
    public const uint CollectionMergeHelperFileOffset = 0x927F60;
    public const int EventSinkVtableSlotOffset = 0xAC;
    public const int ApplicationEventCode = 222;
}
