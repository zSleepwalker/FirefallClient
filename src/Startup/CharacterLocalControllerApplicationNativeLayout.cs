namespace Firefall.Reconstruction.Startup;

public readonly record struct CharacterLocalControllerCallbackSnapshot(
    int MessageId,
    int VtableSlotOffset,
    uint DispatchCaseFileOffset,
    uint CallbackFileOffset);

public static class CharacterLocalControllerApplicationNativeLayout
{
    public const uint RawTextOffsetToVirtualAddressDelta = 0x400C00;

    public const int AdapterAllocationSize = 0x228;
    public const uint AdapterConstructorFileOffset = 0x4C66D0;
    public const int AdapterContextPointerOffset = 0x170;
    public const int AdapterKeyedReceiverPointerOffset = 0x174;
    public const int AdapterConstructorZeroedDwordOffset = 0x16C;
    public const uint AdapterConstructorContextStoreFileOffset = 0x4C6714;
    public const uint AdapterConstructorVtableStoreFileOffset = 0x4C671D;
    public const uint AdapterConstructorZeroDwordStoreFileOffset = 0x4C6723;
    public const uint AdapterConstructorReceiverStoreFileOffset = 0x4C672D;
    public const uint AdapterVtableVirtualAddress = 0x1C64648;
    public const uint AdapterVtableFileOffset = 0x1863248;
    public const string AdapterRttiName = ".?AVBaseNetController@tfLocalCharacter@@";

    public const int PrivateContextAdapterPointerOffset = 0x18;
    public const uint PrivateContextAdapterPointerInstallCallFileOffset = 0x4A1867;
    public const uint PrivateContextAdapterPointerSetterFileOffset = 0x4DB1E0;
    public const uint AdapterInitialBindCallFileOffset = 0x4A18D6;
    public const uint AdapterContextRefreshCallFileOffset = 0x4A2EE5;
    public const uint AdapterBindingMethodFileOffset = 0x4C7360;
    public const int KeyedReceiverCallbackObjectPointerOffset = 0x2C;
    public const int KeyedReceiverContextPointerOffset = 0x24;
    public const uint AdapterReceiverPointerLoadFileOffset = 0x4C738D;
    public const uint AdapterCallbackObjectStoreFileOffset = 0x4C7393;
    public const uint AdapterContextPointerRefreshFileOffset = 0x4C7396;
    public const uint AdapterContextPointerStoreFileOffset = 0x4C73A5;
    public const int PrivateContextPointerSourceOffset = 4;

    public static IReadOnlyList<CharacterLocalControllerCallbackSnapshot> Callbacks { get; } =
        Array.AsReadOnly(new[]
        {
            Callback(0x77, 0x10, 0x540446, 0x4C82E0),
            Callback(0x78, 0x14, 0x54046F, 0x4C8300),
            Callback(0x79, 0x18, 0x540498, 0x4C9B80)
        });

    public const uint Message77UnhandledLoggerFileOffset = 0x2FC4F0;
    public const int Message78SharedTextFieldOffset = 0x28;
    public const uint Message78EventSinkGlobalVirtualAddress = 0x1E002B8;
    public const int Message78EventSinkVtableOffset = 0xAC;
    public const int Message78NumericEventId = 285;
    public const uint Message78EventDispatchFileOffset = 0x4C83BA;

    public const uint Message79RegistryAccessorFileOffset = 0x35E700;
    public const int Message79RegistryMapOffsetFromAccessorResult = 0x54;
    public const uint Message79RegistryLookupFileOffset = 0x4093E0;
    public const int Message79KeyFirstDwordOffset = 0x30;
    public const uint Message79KeyFirstDwordMask = 0xFFFFFF00;
    public const int Message79KeySecondDwordOffset = 0x34;
    public const int Message79OpaqueMessageFieldOffset = 0x28;
    public const int Message79LookupValueFieldOffset = 0x38;
    public const int Message79RecipientGateByteOffset = 8;
    public const int Message79RegistryEntryRecipientPointerOffset = 0x10;
    public const uint Message79KeyedUpdateFileOffset = 0x466660;
    public const uint Message79RecipientFallbackFileOffset = 0x439A50;
    public const int Message79RecipientConditionalStatePresenceByteOffset = 0x4CA8;
    public const int Message79RecipientConditionalStateFirstDwordOffset = 0x4CB0;
    public const int Message79RecipientConditionalStateLastDwordOffset = 0x4CC4;
    public const int Message79RecipientHelperFieldOffset = 0x4CFC;
    public const int Message79RecipientDescriptorDwordFirstOffset = 0x4D00;
    public const int Message79RecipientDescriptorDwordLastOffset = 0x4D0C;
    public const int Message79RecipientAuxiliaryByteOffset = 0x4D10;
    public const int Message79RecipientArgumentOffsetPassedToAuxiliarySink = 0x58A0;
    public const int Message79RecipientObjectPointerLoadOffset = 0x46A4;

    private static CharacterLocalControllerCallbackSnapshot Callback(
        int messageId,
        int vtableSlotOffset,
        uint dispatchCaseFileOffset,
        uint callbackFileOffset) =>
        new(messageId, vtableSlotOffset, dispatchCaseFileOffset, callbackFileOffset);
}
