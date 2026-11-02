namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x5E shared-text field layout. The text meaning and encoding
/// remain unknown.
/// </summary>
public static class CharacterRaiaNpcDebuggingNativeLayout
{
    public const int InnerSelector = 0x5E;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CFFC;
    public const uint CaseLocalDwordValue = 0x66;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x5255F0;
    public const uint ConstructorHelperFileOffset = 0x5A08F0;
    public const uint SharedTextReaderSetupFileOffset = 0x81FB27;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint VtableVirtualAddress = 0x1C671B8;
    public const string RttiName = ".?AVRaiaNPCDebugging@Character@firefall@@";

    public const int SharedTextFieldOffset = 0x28;
    public const int SharedTextChunkByteLimit = 0x100;
    public const int SharedTextTemporaryBufferByteCount = 0x104;
    public const int SharedTextTerminatorByte = 0;
}

public static class CharacterRaiaNpcDebuggingConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterRaiaNpcDebuggingNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0xFC;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECCBE0;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACBFE0;
    public const uint ConsumerFunctionFileOffset = 0xACBFE0;

    public const int ContextPointerOffset = 8;
    public const int ContextCounterPointerOffset = 8;
    public const int ContextCounterValueOffset = 8;
    public const int FirstMessageWordOffset = 0x28;
    public const int MessageWordWidth = 2;
    public const int MessageWordReadCount = 5;
    public const int LastMessageWordOffset = 0x30;
    public const int ReceiverReadEndOffsetExclusive = 0x32;
    public const uint MetricStateGlobalVirtualAddress = 0x1E0004C;
    public const uint MetricAppenderFileOffset = 0x481430;
    public const int MetricAppenderCount = 5;
}
