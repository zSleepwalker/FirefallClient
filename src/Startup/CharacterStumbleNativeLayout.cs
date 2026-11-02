namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x62 raw field layout. Semantic meanings remain unknown.
/// </summary>
public static class CharacterStumbleNativeLayout
{
    public const int InnerSelector = 0x62;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D120;
    public const uint CaseLocalDwordValue = 0x6A;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x5258C0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0AD0;
    public const int BaseConstructorArgument = 0x62;
    public const uint BodyReaderFileOffset = 0x5F1ED0;
    public const uint VtableVirtualAddress = 0x1C67188;
    public const string RttiName = ".?AVStumble@Character@firefall@@";

    public const int WordField0Offset = 0x28;
    public const int WordField1Offset = 0x2A;
    public const int ByteFieldOffset = 0x2C;
    public const int NominalInputByteCount = 5;
}

public static class CharacterStumbleConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterStumbleNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0x10C;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECC250;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACB650;
    public const uint ConsumerFunctionFileOffset = 0xACB650;

    public const uint MessageStateGateGlobalVirtualAddress = 0x1E68354;
    public const int MessageStateGateByteOffset = 0x40;
    public const int MessageKeyDwordOffset = 0x28;
    public const int MessageStatusByteOffset = 0x2C;
    public const int MessageVectorBeginPointerOffset = 0x30;
    public const int MessageVectorEndPointerOffset = 0x34;
    public const int VectorEntrySizeBytes = 12;
    public const int VectorHeaderReadEndOffsetExclusive = 0x38;
    public const int VectorHeaderReadPastAllocationByteCount =
        VectorHeaderReadEndOffsetExclusive - CharacterStumbleNativeLayout.AllocationSize;

    public const int StatusValueZero = 0;
    public const int StatusValueOne = 1;
    public const int StatusValueOther = 2;
    public const uint VectorCopyHelperFileOffset = 0x611A30;
    public const uint SubsystemAccessorFileOffset = 0x60FB60;
    public const uint SubsystemMessageHandlerFileOffset = 0x60FBD0;
}
