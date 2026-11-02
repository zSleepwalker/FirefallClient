namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x6D raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterWarnLockTargetedNativeLayout
{
    public const int InnerSelector = 0x6D;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D417;
    public const uint CaseLocalDwordValue = 0x75;
    public const uint ConstructorCallSiteFileOffset = 0x81D43C;
    public const uint SuccessPathReturnEpilogueFileOffset = 0x81D441;
    public const int AllocationSize = 0x28;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x525FB0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0FA0;
    public const int BaseConstructorArgument = 0x6D;
    public const int CommonQwordFirstDwordFieldOffset = 0x18;
    public const int CommonQwordSecondDwordFieldOffset = 0x1C;
    public const int ConstructorInitializedEndOffsetExclusive = 0x28;
    public const uint AllocationFailureContinuationFileOffset = 0x8217D9;
    public const uint BodyReaderContinuationFileOffset = 0;
    public const uint BodyReaderFileOffset = 0;
    public const uint VtableVirtualAddress = 0x1C670C8;
    public const string RttiName = ".?AVWarnLockTargeted@Character@firefall@@";
}

public static class CharacterWarnLockTargetedConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterWarnLockTargetedNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0x138;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECEBC0;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACDFC0;
    public const uint ConsumerFunctionFileOffset = 0xACDFC0;
    public const uint ConsumerSwitchTargetVirtualAddress = 0xC1D45F;
    public const uint ConsumerSwitchTargetFileOffset = 0x81C85F;
    public const uint ConsumerVtableCallFileOffset = 0x81C87E;

    public const uint ManagerGlobalAddress = 0x1DFFFFC;
    public const int MessageTargetFieldOffset = 0x28;
    public const int MessageBooleanFieldOffset = 0x38;
    public const int AllocationSize = CharacterWarnLockTargetedNativeLayout.AllocationSize;
    public const int MessageTargetPastAllocationByteCount =
        MessageTargetFieldOffset + sizeof(uint) - AllocationSize;
    public const int MessageBooleanPastAllocationByteCount =
        MessageBooleanFieldOffset + sizeof(byte) - AllocationSize;
    public const uint StateUpdateHelperFileOffset = 0x465D40;
}
