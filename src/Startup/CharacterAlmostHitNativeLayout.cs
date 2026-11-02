namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x6A raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterAlmostHitNativeLayout
{
    public const int InnerSelector = 0x6A;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D344;
    public const uint CaseLocalDwordValue = 0x72;
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x525DD0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0E50;
    public const int BaseConstructorArgument = 0x6A;
    public const uint AllocationFailureContinuationFileOffset = 0x81F72C;
    public const uint BodyReaderContinuationFileOffset = 0x81F72E;
    public const uint BodyReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C67108;
    public const string RttiName = ".?AVAlmostHit@Character@firefall@@";

    public const int QwordFieldOffset = 0x28;
    public const int NominalInputByteCount = 8;
}

public static class CharacterAlmostHitConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterAlmostHitNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0x12C;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECEAD0;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACDED0;
    public const uint ConsumerFunctionFileOffset = 0xACDED0;

    public const int MessageSignedValueFieldOffset = 0x28;
    public const int MessageLookupKeyFieldOffset = 0x30;
    public const int LookupKeyByteMask = unchecked((int)0xFFFFFF00);
    public const ulong InvalidLookupKeyUpperSentinel = 0xFFFFFFFFFFFFFF00;
    public const int AllocationSize = CharacterAlmostHitNativeLayout.AllocationSize;
    public const int LookupKeyPastAllocationByteCount =
        MessageLookupKeyFieldOffset + sizeof(ulong) - AllocationSize;
    public const uint LookupTableAccessorFileOffset = 0x35E700;
    public const int LookupTableBucketsFieldOffset = 0x54;
    public const uint LookupRoutineFileOffset = 0x4093E0;
    public const int LookupEntryTargetPointerOffset = 0x10;
    public const int LookupTargetVirtualMethodSlotOffset = 0x70;
    public const uint SignedValueHandlerFileOffset = 0x369080;
    public const int SignedValueHandlerTypeArgument = 12;
    public const uint ManagerGlobalAddress = 0x1DFFFE8;
    public const int ManagerAvailabilityFieldOffset = 0x28;
}
