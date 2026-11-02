namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x65 raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterJumpActionedNativeLayout
{
    public const int InnerSelector = 0x65;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D1F3;
    public const uint CaseLocalDwordValue = 0x6D;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x525AF0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0C20;
    public const int BaseConstructorArgument = 0x65;
    public const uint AllocationFailureContinuationFileOffset = 0x81D21D;
    public const uint BodyReaderContinuationFileOffset = 0x81D21F;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint VtableVirtualAddress = 0x1C67168;
    public const string RttiName = ".?AVJumpActioned@Character@firefall@@";

    public const int WordFieldOffset = 0x28;
    public const int NominalInputByteCount = 2;
}

public static class CharacterJumpActionedConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterJumpActionedNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0x118;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECD970;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACCD70;
    public const uint ConsumerFunctionFileOffset = 0xACCD70;

    public const int TransportContextFieldOffset = 0x10;
    public const int MessageWordFieldOffset = CharacterJumpActionedNativeLayout.WordFieldOffset;
    public const int FirstSubobjectAddressOffset = 0x30;
    public const int SecondSubobjectAddressOffset = 0x40;
    public const int AllocationSize = CharacterJumpActionedNativeLayout.AllocationSize;
    public const int SecondSubobjectAddressPastAllocationByteCount =
        SecondSubobjectAddressOffset - AllocationSize;
    public const int SubobjectVtableMethodSlotOffset = 0x0C;

    public const int MissingTransportDiagnosticCode = 91;
    public const uint HandoffRoutineFileOffset = 0x977750;
    public const int HandoffEventIdForRepeatedItems = 360;
    public const int HandoffEventIdForNonemptyTail = 359;
}
