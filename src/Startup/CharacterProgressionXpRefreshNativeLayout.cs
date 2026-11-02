namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x59 vector-reader layout. Record field meanings remain unknown.
/// </summary>
public static class CharacterProgressionXpRefreshNativeLayout
{
    public const int InnerSelector = 0x59;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CE89;
    public const uint CaseLocalDwordValue = 0x61;
    public const int AllocationSize = 0x38;
    public const uint ConstructorFileOffset = 0x5251B0;
    public const uint ConstructorHelperFileOffset = 0x5A06A0;
    public const uint BodyReaderFileOffset = 0x5F4170;
    public const uint RecordAppendFileOffset = 0x5200F0;
    public const uint VtableVirtualAddress = 0x1C67228;
    public const string RttiName = ".?AVProgressionXpRefresh@Character@firefall@@";

    public const int VectorBeginOffset = 0x28;
    public const int VectorEndOffset = 0x2C;
    public const int VectorCapacityOffset = 0x30;
    public const int RecordDwordCount = 5;
    public const int RecordByteCount = 0x14;
    public const int CountFieldByteCount = 1;
    public const int CountBlockRepeatMarker = 0xFF;
}

public static class CharacterProgressionXpRefreshConsumerNativeLayout
{
    public const int MessageDiscriminator = 0x59;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0xE8;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECB5D0;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACA9D0;
    public const uint ConsumerFunctionFileOffset = 0xACA9D0;
    public const uint OwnerStatePointerOffset = 0x43C8;
    public const uint OwnerTreeSentinelPointerOffset = 0;
    public const uint OwnerTreeSentinelSizeOffset = 4;
    public const uint OwnerStateFlagOffset = 8;
    public const uint TreeClearHelperFileOffset = 0x90A7B0;
    public const int MissingOwnerDiagnosticCode = 1651;
}
