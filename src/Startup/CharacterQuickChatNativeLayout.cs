namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x63 raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterQuickChatNativeLayout
{
    public const int InnerSelector = 0x63;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D171;
    public const uint CaseLocalDwordValue = 0x6B;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x525940;
    public const uint BaseConstructorHelperFileOffset = 0x5A0B40;
    public const int BaseConstructorArgument = 0x63;
    public const uint AllocationFailureContinuationFileOffset = 0x8206E1;
    public const uint BodyReaderContinuationFileOffset = 0x8206E3;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint VtableVirtualAddress = 0x1C67178;
    public const string RttiName = ".?AVQuickChat@Character@firefall@@";

    public const int DwordFieldOffset = 0x28;
    public const int NominalInputByteCount = 4;
}

public static class CharacterQuickChatConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterQuickChatNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0x110;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECD7E0;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACCBE0;
    public const uint ConsumerFunctionFileOffset = 0xACCBE0;

    public const int MessageLogDwordOffset = 0x28;
    public const int MessageTextPointerOffset = 0x30;
    public const int MessageTypePointerOffset = 0x34;
    public const int ReceiverPointerReadEndOffsetExclusive = 0x38;
    public const int DeclaredAllocationReadPastEndByteCount =
        ReceiverPointerReadEndOffsetExclusive - CharacterQuickChatNativeLayout.AllocationSize;

    public const uint ChatGetterFileOffset = 0x6F4540;
    public const uint NotificationMethodFileOffset = 0x6E3460;
    public const uint LoggerFileOffset = 0x325A90;
}
