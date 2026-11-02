namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x5B construction and body-reader layout. The raw discriminator
/// and blob contents do not have established semantic names.
/// </summary>
public static class CharacterPublicCombatLogNativeLayout
{
    public const int InnerSelector = 0x5B;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CF2F;
    public const uint CaseLocalDwordValue = 0x63;
    public const int AllocationSize = 0x40;
    public const uint ConstructorFileOffset = 0x525390;
    public const uint ConstructorHelperFileOffset = 0x5A07A0;
    public const uint BodyReaderFileOffset = 0x5F3350;
    public const uint WordLengthPrefixedBlobReaderFileOffset = 0x12EE170;
    public const uint VtableVirtualAddress = 0x1C671F8;
    public const string RttiName = ".?AVPublicCombatLog@Character@firefall@@";

    public const int ConstructorZeroedByteFieldOffset = 0x28;
    public const int EmbeddedBlobFieldOffset = 0x2C;
    public const uint EmbeddedBlobVtableVirtualAddress = 0x1C80CC4;
    public const int BodyReaderThisObjectOffset = 0x28;
    public const int BlobDestinationOffsetFromReaderThis = 4;
    public const int RequiredRawDwordValue = 1;
    public const int RawDwordWidth = 4;
    public const int BlobLengthPrefixByteCount = 2;
    public const int MinimumSuccessBodyPrefixByteCount = 6;
}

public static class CharacterPublicCombatLogConsumerNativeLayout
{
    public const int MessageDiscriminator = 0x5B;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0xF0;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECB260;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACA660;
    public const uint ConsumerFunctionFileOffset = 0xACA660;
    public const int MessageEmbeddedQwordOffset = 0x30;
    public const int MessageModeByteOffset = 0x38;
    public const uint OwnerStatePointerOffset = 0x43BC;
    public const uint OwnerMapSubobjectOffset = 8;
    public const uint ContextAccessorFileOffset = 0x49DCC0;
    public const uint EmptyOrSentinelFlaggedHelperFileOffset = 0xB901F0;
    public const uint EmptyOrSentinelUnflaggedHelperFileOffset = 0xB903B0;
    public const uint QwordFlaggedHelperFileOffset = 0xB90A30;
    public const uint QwordUnflaggedSearchHelperFileOffset = 0xB90810;
    public const uint QwordUnflaggedRangeUpdateHelperFileOffset = 0x36F480;
    public const uint SentinelLowDwordMask = 0xFFFFFF00;
    public const uint SentinelLowDwordMaskedValue = 0xFFFFFF00;
    public const uint SentinelHighDwordValue = 0xFFFFFFFF;
    public const int MissingOwnerDiagnosticCode = 1647;
}
