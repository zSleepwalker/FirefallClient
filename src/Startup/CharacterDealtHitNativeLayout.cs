namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x6B raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterDealtHitNativeLayout
{
    public const int InnerSelector = 0x6B;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D375;
    public const uint CaseLocalDwordValue = 0x73;
    public const int AllocationSize = 0x58;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x525E60;
    public const uint BaseConstructorHelperFileOffset = 0x5A0EC0;
    public const int BaseConstructorArgument = 0x6B;
    public const uint AllocationFailureContinuationFileOffset = 0x81D39F;
    public const uint BodyReaderContinuationFileOffset = 0x81D3A1;
    public const uint BodyReaderFileOffset = 0x5F0800;
    public const uint ConditionalRecordReaderFileOffset = 0x5F0CF0;
    public const uint ConditionalQwordReaderFileOffset = 0x5FDA00;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint DwordReaderFileOffset = 0x353310;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint VtableVirtualAddress = 0x1C670F8;
    public const string RttiName = ".?AVDealtHit@Character@firefall@@";

    public const int ConditionalRecordGateByteOffset = 0x28;
    public const int ConditionalRecordFirstQwordOffset = 0x30;
    public const int ConditionalRecordNestedGateByteOffset = 0x38;
    public const int ConditionalRecordSecondQwordOffset = 0x40;
    public const int ConditionalRecordDwordOffset = 0x48;
    public const int ConditionalRecordByteOffset = 0x4C;
    public const int FirstTrailingByteFieldOffset = 0x50;
    public const int SecondTrailingByteFieldOffset = 0x51;
    public const int MinimumInputByteCount = 3;
}

public static class CharacterDealtHitConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterDealtHitNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0x130;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECEB70;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACDF70;
    public const uint ConsumerFunctionFileOffset = 0xACDF70;
    public const uint ConsumerSwitchTargetVirtualAddress = 0xC1D409;
    public const uint ConsumerSwitchTargetFileOffset = 0x81C809;
    public const uint ConsumerVtableCallFileOffset = 0x81C828;

    public const int MessageSignedValueFieldOffset = 0x28;
    public const int MessagePositionArgumentOffset = 0x30;
    public const int PositionComponentCount = 3;
    public const uint PositionDistanceOriginXVirtualAddress = 0x1E62C44;
    public const uint PositionDistanceOriginYVirtualAddress = 0x1E62C48;
    public const uint PositionDistanceOriginZVirtualAddress = 0x1E62C4C;
    public const int PositionUpdateGateByteOffset = 0xE1;
    public const uint PositionUpdateHelperFileOffset = 0x100F7A0;
    public const uint PositionDistanceCallbackFileOffset = 0x100AE90;
    public const uint MessageValueHandlerFileOffset = 0x369080;
    public const int MessageValueHandlerTypeArgument = 12;
    public const int FollowupDurationSeconds = 0x3C;
    public const int FollowupDurationMillisecondsMultiplier = 1000;
    public const uint FollowupTimerRegistrationFileOffset = 0x100C5D0;
    public const uint SharedDispatchHelperFileOffset = 0x368EA0;
}
