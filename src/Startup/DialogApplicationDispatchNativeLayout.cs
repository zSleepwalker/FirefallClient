namespace Firefall.Reconstruction.Startup;

public enum DialogApplicationPayloadValueKind
{
    Int32,
    BooleanByte,
    Int32Pair,
    NullTerminatedText
}

public readonly record struct DialogApplicationPayloadFieldSnapshot(
    string Key,
    int ReceiverInputOffset,
    DialogApplicationPayloadValueKind ValueKind);

public readonly record struct DialogApplicationEventDispatchSnapshot(
    int MessageDiscriminator,
    uint ReceiverMethodFileOffset,
    uint ApplicationHelperFileOffset,
    uint EventCodeFileOffset,
    int EventCode,
    bool PassesPayloadRecord);

public static class DialogApplicationDispatchNativeLayout
{
    public const uint UiManagerGlobalPointerFileOffset = 0x19FE6B8;
    public const uint UiManagerVtableFileOffset = 0x1872ABC;
    public const uint UiManagerEventDispatchSlotFileOffset = 0x1872B68;
    public const uint UiManagerEventDispatchMethodVirtualAddress = 0xFFA980;
    public const uint UiManagerEventDispatchMethodFileOffset = 0xBF9D80;
    public const int UiManagerEventDispatchVtableSlotOffset = 0xAC;
    public const int UiManagerEventNameMapOffset = 0x6E8;
    public const int UiManagerEventRegistryOffset = 0x6F0;
    public const int UiManagerEventNameArrayBeginOffset = 0x71C;
    public const int UiManagerEventNameArrayEndOffset = 0x720;
    public const int UiManagerEventDeliveryQueueOffset = 0x5EC;
    public const int UiManagerListenerEventMapOffset = 0x168;
    public const int UiManagerListenerPendingDispatchOffset = 0x4C4;
    public const uint UiManagerEventNameToIdMethodFileOffset = 0xBF9AB0;
    public const uint UiManagerEventListenerRegisterMethodFileOffset = 0xBF9B40;
    public const uint UiManagerEventListenerDispatchMethodFileOffset = 0xBF9C40;
    public const uint UiManagerEventListenerCallbackCallSiteFileOffset = 0xBF9D5B;
    public const uint UiManagerEventCallbackRouteMethodFileOffset = 0xC38400;
    public const uint UiManagerEventCallbackLookupCallSiteFileOffset = 0xC38438;
    public const uint UiManagerEventListenerMapLookupMethodFileOffset = 0xC41BC0;
    public const uint UiManagerEventListenerDispatchInsertCallSiteFileOffset = 0xC3847C;
    public const uint UiManagerEventListenerDispatchInsertMethodFileOffset = 0xC42180;
    public const uint UiManagerEventDeliveryQueueInsertCallSiteFileOffset = 0xC384D3;
    public const uint UiManagerComponentEventsParserFileOffset = 0xC35740;
    public const uint UiManagerComponentEventNameToIdCallFileOffset = 0xC358AF;
    public const uint UiManagerComponentListenerRegisterCallFileOffset = 0xC35AD8;
    public const uint UiManagerNamedSubscriptionMethodFileOffset = 0xC35BB0;

    public const uint OpenMovieDialogHandlerFileOffset = 0xACE2E0;
    public const uint OpenMovieDialogHandlerArgumentSetupFileOffset = 0xACE2E6;
    public const uint OpenMovieDialogHandlerCallFileOffset = 0xACE2F3;
    public const uint OpenMovieDialogApplicationHelperFileOffset = 0x991320;
    public const uint OpenMovieDialogPayloadFieldCallFileOffset = 0x99146A;
    public const uint OpenMovieDialogEventCodeFileOffset = 0x99147C;
    public const int OpenMovieDialogEventCode = 228;
    public const int OpenMovieDialogMessageIdOffset = 0x28;
    public const int OpenMovieDialogMessageTextOffset = 0x30;

    public const uint PrivateDialogReceiverThunkFileOffset = 0xACE300;
    public const uint PrivateDialogApplicationHelperFileOffset = 0x991510;
    public const uint PrivateDialogSuccessFieldCallFileOffset = 0x9915E5;
    public const uint PrivateDialogVendorFieldCallFileOffset = 0x9915F6;
    public const uint PrivateDialogProductFieldsCallFileOffset = 0x99160A;
    public const uint PrivateDialogPriceFieldsCallFileOffset = 0x99161E;
    public const uint PrivateDialogCodeFieldCallFileOffset = 0x991649;
    public const uint PrivateDialogEventCodeFileOffset = 0x99165B;
    public const int PrivateDialogEventCode = 229;
    public const string PrivateDialogCodeFallback = "PURCHASE_SUCCESSFUL";

    public const uint PublicDialogHandlerFileOffset = 0xACE390;
    public const uint PublicDialogContextFactoryCallFileOffset = 0xACE3A8;
    public const uint PublicDialogApplicationHelperCallFileOffset = 0xACE3AF;
    public const uint PublicDialogApplicationHelperFileOffset = 0x4D6C10;
    public const uint PublicDialogActionFieldCallFileOffset = 0x4D6D23;
    public const uint PublicDialogActivatedFieldCallFileOffset = 0x4D6D34;
    public const uint PublicDialogEventCodeFileOffset = 0x4D6D43;
    public const int PublicDialogEventCode = 357;
    public const int PublicDialogActionIdOffset = 0x30;
    public const int PublicDialogActivatedFlagOffset = 0x84;
    public const bool PublicDialogDispatchPassesPayloadRecord = false;
    public const uint PayloadInt32FieldWriterMethodFileOffset = 0xC24210;
    public const uint PayloadBooleanFieldWriterMethodFileOffset = 0xC243E0;
    public const uint PayloadInt32PairFieldWriterMethodFileOffset = 0xC245A0;
    public const uint PayloadInt32FieldWriterFileOffset = 0xC24254;
    public const uint PayloadBooleanFieldWriterFileOffset = 0xC24424;
    public const uint PayloadInt32PairFieldWriterFileOffset = 0xC245E4;
    public const uint PayloadTextFieldWriterFileOffset = 0xC240F0;

    public const uint VendorIdKeyVirtualAddress = 0x1B99B1C;
    public const uint SuccessKeyVirtualAddress = 0x1B99B08;
    public const uint ProductIdKeyVirtualAddress = 0x1B99A54;
    public const uint PriceIdKeyVirtualAddress = 0x1B99A60;
    public const uint CodeKeyVirtualAddress = 0x1B99A08;
    public const uint PurchaseSuccessfulFallbackVirtualAddress = 0x1B99B28;
    public const uint ActionIdKeyVirtualAddress = 0x1B5AC5C;
    public const uint ActivatedKeyVirtualAddress = 0x1B5AB7C;

    public static IReadOnlyList<DialogApplicationEventDispatchSnapshot> Dispatches { get; } =
        Array.AsReadOnly(new[]
        {
            new DialogApplicationEventDispatchSnapshot(
                0x78,
                OpenMovieDialogHandlerFileOffset,
                OpenMovieDialogApplicationHelperFileOffset,
                OpenMovieDialogEventCodeFileOffset,
                OpenMovieDialogEventCode,
                true),
            new DialogApplicationEventDispatchSnapshot(
                0x79,
                PrivateDialogReceiverThunkFileOffset,
                PrivateDialogApplicationHelperFileOffset,
                PrivateDialogEventCodeFileOffset,
                PrivateDialogEventCode,
                true),
            new DialogApplicationEventDispatchSnapshot(
                0x7A,
                PublicDialogHandlerFileOffset,
                PublicDialogApplicationHelperFileOffset,
                PublicDialogEventCodeFileOffset,
                PublicDialogEventCode,
                PublicDialogDispatchPassesPayloadRecord)
        });

    public static IReadOnlyList<DialogApplicationPayloadFieldSnapshot> OpenMoviePayloadFields { get; } =
        Array.AsReadOnly(new[]
        {
            new DialogApplicationPayloadFieldSnapshot(
                "vendor_id",
                OpenMovieDialogMessageIdOffset,
                DialogApplicationPayloadValueKind.Int32)
        });

    public static IReadOnlyList<DialogApplicationPayloadFieldSnapshot> PrivateDialogPayloadFields { get; } =
        Array.AsReadOnly(new[]
        {
            new DialogApplicationPayloadFieldSnapshot(
                "success",
                0x28,
                DialogApplicationPayloadValueKind.BooleanByte),
            new DialogApplicationPayloadFieldSnapshot(
                "vendor_id",
                0x40,
                DialogApplicationPayloadValueKind.Int32),
            new DialogApplicationPayloadFieldSnapshot(
                "product_id",
                0x30,
                DialogApplicationPayloadValueKind.Int32Pair),
            new DialogApplicationPayloadFieldSnapshot(
                "price_id",
                0x38,
                DialogApplicationPayloadValueKind.Int32Pair),
            new DialogApplicationPayloadFieldSnapshot(
                "code",
                0x44,
                DialogApplicationPayloadValueKind.NullTerminatedText)
        });

    public static IReadOnlyList<DialogApplicationPayloadFieldSnapshot> PublicDialogPreparedFields { get; } =
        Array.AsReadOnly(new[]
        {
            new DialogApplicationPayloadFieldSnapshot(
                "action_id",
                PublicDialogActionIdOffset,
                DialogApplicationPayloadValueKind.Int32),
            new DialogApplicationPayloadFieldSnapshot(
                "activated",
                PublicDialogActivatedFlagOffset,
                DialogApplicationPayloadValueKind.BooleanByte)
        });
}
