namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x5C construction metadata. Its shared body reader establishes
/// layout reuse, not semantic equivalence with selector 0x5B.
/// </summary>
public static class CharacterPrivateCombatLogNativeLayout
{
    public const int InnerSelector = 0x5C;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CF81;
    public const uint CaseLocalDwordValue = 0x64;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x525480;
    public const uint ConstructorHelperFileOffset = 0x5A0810;
    public const uint BodyReaderFileOffset = CharacterPublicCombatLogNativeLayout.BodyReaderFileOffset;
    public const uint SharedBodyReaderSetupFileOffset = 0x81CF5B;
    public const uint VtableVirtualAddress = 0x1C671E8;
    public const string RttiName = ".?AVPrivateCombatLog@Character@firefall@@";

    public const int ConstructorZeroedByteFieldOffset = 0x28;
    public const int EmbeddedBlobFieldOffset = 0x2C;
    public const uint EmbeddedBlobVtableVirtualAddress = 0x1C80CC4;
    public const int BodyReaderThisObjectOffset = CharacterPublicCombatLogNativeLayout.BodyReaderThisObjectOffset;
    public const int BlobDestinationOffsetFromReaderThis =
        CharacterPublicCombatLogNativeLayout.BlobDestinationOffsetFromReaderThis;
}

public static class CharacterPrivateCombatLogConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterPrivateCombatLogNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0xF4;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECC460;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACB860;
    public const uint ConsumerFunctionFileOffset = 0xACB860;

    public const int MessageByteFieldOffset = 0x28;
    public const int MessageEmbeddedBlobFieldOffset = 0x2C;
    public const int MessageBlobDataPointerFieldOffset = 0x30;
    public const int MessageWordFieldOffset = 0x34;
    public const int MessageTrailingByteFieldOffset = 0x36;
    public const uint EmbeddedBlobLookupHelperFileOffset = 0x391470;
    public const uint EmbeddedBlobFallbackHelperFileOffset = 0xC24210;
    public const uint SharedTextAppendHelperFileOffset = 0xC23FE0;
    public const uint BlobDataVtableCallFileOffset = 0xACB93A;
    public const uint ByteFieldLookupCallFileOffset = 0xACB9C3;
    public const uint WordFieldConversionHelperFileOffset = 0xC242F0;
    public const uint TrailingByteConversionHelperFileOffset = 0xC243E0;
    public const int ApplicationEventSinkVtableSlotOffset = 0xAC;
    public const uint ApplicationEventDispatchCallFileOffset = 0xACBA78;
    public const int ApplicationEventCode = 122;
}
