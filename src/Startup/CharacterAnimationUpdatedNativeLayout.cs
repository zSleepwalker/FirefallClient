namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x5D object layout. Field meanings remain unknown.
/// </summary>
public static class CharacterAnimationUpdatedNativeLayout
{
    public const int InnerSelector = 0x5D;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CFAB;
    public const uint CaseLocalDwordValue = 0x65;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x525570;
    public const uint ConstructorHelperFileOffset = 0x5A0880;
    public const uint BodyReaderFileOffset = 0x5F30F0;
    public const uint VtableVirtualAddress = 0x1C671D8;
    public const string RttiName = ".?AVAnimationUpdated@Character@firefall@@";

    public const int RawWordFieldOffset = 0x28;
    public const int RawByteFieldOffset = 0x2A;
    public const int NominalInputByteCount = 3;
}

public static class CharacterAnimationUpdatedConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterAnimationUpdatedNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0xF8;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECCB40;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACBF40;
    public const uint ConsumerFunctionFileOffset = 0xACBF40;

    public const int HandlerContextPointerOffset = 8;
    public const int ContextCounterPointerOffset = 8;
    public const int ContextCounterValueOffset = 8;
    public const int MessageFirstWordOffset = 0x28;
    public const int MessageSecondWordOffset = 0x2A;
    public const int MessageQueueIndexByteOffset = 0x2C;
    public const int MessageModeByteOffset = 0x2D;
    public const uint CounterWrapModulus = 0x10000;
    public const uint CounterWrapHalfRange = 0x8000;
    public const uint RelayHelperFileOffset = 0x3343B0;
    public const uint RelayRecordQueueFileOffset = 0x334460;
    public const int RelayMaximumQueueIndex = 10;
    public const int RelayMaximumMode = 3;
    public const int RelayQueueFlagsOffset = 5;
    public const int RelayQueueAcceptFlagOffset = 6;
    public const int RelayRecordSize = 0x10;
    public const int RelayRecordArrayBaseOffset = 0x50;
    public const int RelayRecordIndexBias = 5;
    public const uint RelayQueueCapacity = 0xBB8;
}
