namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x68 raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterCalledForHelpNativeLayout
{
    public const int InnerSelector = 0x68;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D2C2;
    public const uint CaseLocalDwordValue = 0x70;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x525C70;
    public const uint BaseConstructorHelperFileOffset = 0x5A0D70;
    public const int BaseConstructorArgument = 0x68;
    public const uint AllocationFailureContinuationFileOffset = 0x81D21D;
    public const uint BodyReaderContinuationFileOffset = 0x81D21F;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint VtableVirtualAddress = 0x1C67118;
    public const string RttiName = ".?AVCalledForHelp@Character@firefall@@";

    public const int WordFieldOffset = 0x28;
    public const int NominalInputByteCount = 2;
}

public static class CharacterCalledForHelpConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterCalledForHelpNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0x124;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECE040;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACD440;
    public const uint ConsumerFunctionFileOffset = 0xACD440;

    public const uint KeyedRegistryVirtualAddress = 0x1E5A830;
    public const uint KeyedLookupRoutineFileOffset = 0x327890;
    public const int MessageKeyFieldOffset = 0x28;
    public const int MessageAuxiliaryByteFieldOffset = 0x2C;
    public const int MessageAuxiliaryDwordFieldOffset = 0x30;
    public const int AllocationSize = CharacterCalledForHelpNativeLayout.AllocationSize;
    public const int MessageAuxiliaryDwordPastAllocationByteCount =
        MessageAuxiliaryDwordFieldOffset + 4 - AllocationSize;

    public const uint UnhandledMessageLoggerFileOffset = 0x2FC4F0;
    public const int DispatchedEventId = 176;
}
