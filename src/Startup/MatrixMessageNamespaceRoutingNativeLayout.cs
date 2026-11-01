namespace Firefall.Reconstruction.Startup;

public enum MatrixMessageCallbackDispatchPath
{
    EnvelopeKeyRegistry,
    NestedMessageChildren,
    ProtocolHandlerPrimaryVtable,
    ProtocolHandlerUnavailable
}

public readonly record struct MatrixMessageNamespaceSnapshot(
    int ParserRouteIndex,
    uint RouteDispatcherFileOffset,
    uint RouteSelectorTableVirtualAddress,
    uint ParserCaseFileOffset,
    int ParserSelector,
    int AllocationSize,
    uint ParserStackLocalDwordFileOffset,
    uint ParserStackLocalDwordValue,
    uint ConstructorFileOffset,
    uint BaseConstructorHelperFileOffset,
    uint BaseMetadataStringVirtualAddress,
    uint BaseHelperFinalVtableVirtualAddress,
    uint MessageVtableVirtualAddress,
    string MessageRttiName);

public readonly record struct MatrixMessageEnvelopeRegistrySnapshot(
    uint CallbackFileOffset,
    uint EnvelopeDispatcherFileOffset,
    uint RegistryLookupFileOffset,
    int MessageKeyLowDwordFieldOffset,
    int MessageKeyHighDwordFieldOffset,
    int FirstRegistryFieldOffset,
    int SecondRegistryFieldOffset,
    int RegistryNodeKeyLowDwordFieldOffset,
    int RegistryNodeKeyHighDwordFieldOffset,
    int RegistryNodeValuePointerFieldOffset,
    uint RegistryTreeConstructorFileOffset,
    uint RegistrySearchFileOffset,
    uint RegistryEntryConstructorFileOffset,
    uint RegistryInsertFileOffset,
    uint DynamicFactoryFileOffset,
    int DynamicFactoryFirstByteMinimum,
    int DynamicFactoryFirstByteMaximum,
    uint DirectProtocolHandlerDispatcherFileOffset);

/// <summary>
/// Records parser-family collisions and the conditional callback routing
/// performed by tfGameSocketHandler. The envelope key's application meaning
/// and the runtime values supplied by the server remain unknown.
/// </summary>
public static class MatrixMessageNamespaceRoutingNativeLayout
{
    public const uint StreamEnvelopeQwordReaderVirtualAddress = 0x757B80;
    public const uint StreamCompactSelectorReaderVirtualAddress = 0x74AC20;
    public const uint RawTextOffsetToVirtualAddressDelta = 0x400C00;
    public const uint StreamParserFileOffset = 0x821BA0;
    public const uint StreamEnvelopeQwordReadCallFileOffset = 0x821CBA;
    public const uint StreamCompactSelectorReadCallFileOffset = 0x821CC6;
    public const uint StreamParserDispatchCallFileOffset = 0x821D87;
    public const uint ParserDispatcherFileOffset = 0x81CC50;

    public const int MessagePrimaryVtableFieldOffset = 0;
    public const int MessageReferenceCountFieldOffset = 0x08;
    public const int MessageMetadataPointerFieldOffset = 0x0C;
    public const int MessageSelectorFieldOffset = 0x10;
    public const int MessageFieldOffset14 = 0x14;
    public const int MessageEnvelopeKeyLowDwordFieldOffset = 0x18;
    public const int MessageEnvelopeKeyHighDwordFieldOffset = 0x1C;
    public const int MessageClearedDwordFieldOffset1 = 0x20;
    public const int MessageClearedDwordFieldOffset2 = 0x24;
    public const int MessageBodyStartOffset = 0x28;
    public const int BaseConstructorReferenceCountValue = 1;
    public const int CollisionSelector = 0x53;
    public const int EnvelopeKeyGateByteOffset = MessageEnvelopeKeyLowDwordFieldOffset;

    public const uint CallbackDispatcherFileOffset = 0x7FDC60;
    public const uint CallbackMessagePointerLoadFileOffset = 0x7FDC6E;
    public const uint CallbackEnvelopeKeyLowLoadFileOffset = 0x7FDC70;
    public const uint CallbackEnvelopeKeyHighLoadFileOffset = 0x7FDC75;
    public const uint CallbackEnvelopeKeyGateTestFileOffset = 0x7FDC86;
    public const uint CallbackEnvelopeKeyDispatchCallFileOffset = 0x7FDD59;
    public const uint EnvelopeKeyDispatcherFileOffset = 0x800420;
    public const uint EnvelopeKeyRegistryLookupFileOffset = 0x800070;
    public const int EnvelopeKeyRegistryFieldOffset1 = 0x24;
    public const int EnvelopeKeyRegistryFieldOffset2 = 0x2C;
    public const int EnvelopeKeyReceiverVirtualSlotOffset = 0x20;
    public const int EnvelopeRegistryNodeKeyLowDwordFieldOffset = 0x10;
    public const int EnvelopeRegistryNodeKeyHighDwordFieldOffset = 0x14;
    public const int EnvelopeRegistryNodeValuePointerFieldOffset = 0x18;
    public const uint EnvelopeRegistryTreeConstructorFileOffset = 0x805130;
    public const uint EnvelopeRegistrySearchFileOffset = 0x8013B0;
    public const uint EnvelopeRegistryEntryConstructorFileOffset = 0x806910;
    public const uint EnvelopeRegistryInsertFileOffset = 0x8064C0;
    public const uint EnvelopeRegistryDynamicFactoryFileOffset = 0x81B1B0;
    public const uint EnvelopeRegistryDynamicFactoryVirtualAddress = 0xC1BDB0;
    public const uint EnvelopeRegistryDynamicFactoryByteDispatchFileOffset = 0x81B1DC;
    public const int EnvelopeRegistryDynamicFactoryFirstByteMinimum = 2;
    public const int EnvelopeRegistryDynamicFactoryFirstByteMaximum = 0x35;
    public const uint EnvelopeRegistrySecondMapUpdateFileOffset = 0x800300;
    public const uint EnvelopeRegistrySecondMapInsertCallFileOffset = 0x8003FF;
    public const uint CallbackSelectorLoadFileOffset = 0x7FDC8E;
    public const uint CallbackNestedSentinelCompareFileOffset = 0x7FDC91;
    public const uint CallbackProtocolReceiverLoadFileOffset = 0x7FDD47;
    public const int CallbackProtocolReceiverFieldOffset = 8;
    public const uint ProtocolHandlerDispatcherFileOffset = 0x81BB90;

    public const uint ProtocolHandlerConstructorFileOffset = 0xAC8CA0;
    public const uint ProtocolHandlerVtableStoreFileOffset = 0xAC8CE2;
    public const uint ProtocolHandlerVtableVirtualAddress = 0x1C71EC0;
    public const uint ProtocolHandlerCompleteObjectLocatorVirtualAddress = 0x1CCA404;
    public const uint ProtocolHandlerTypeDescriptorVirtualAddress = 0x1E29DF8;
    public const uint ProtocolHandlerClassHierarchyDescriptorVirtualAddress = 0x1CCA3D4;
    public const string ProtocolHandlerRttiName = ".?AVtfGameProtocolHandler@@";
    public const string ProtocolHandlerSecondBaseRttiName = ".?AVMessageHandler@firefall@@";
    public const int ProtocolHandlerCompleteObjectOffset = 0;
    public const int ProtocolHandlerSecondBaseSubobjectOffset = 0;
    public const int ProtocolHandlerObservedVtableGroupCount = 1;
    public const int ProtocolHandlerFirstSelector = 0x20;
    public const int ProtocolHandlerLastSelector = 0x7A;
    public const int ProtocolHandlerVtableSlotBaseOffset = 4;
    public const int ProtocolHandlerVtableEntrySize = 4;
    public const int Selector53ProtocolHandlerVtableSlotOffset = 0xD0;
    public const uint Selector53ProtocolHandlerMethodVirtualAddress = 0xECB180;
    public const uint Selector53ProtocolHandlerMethodFileOffset = 0xACA580;
    public const int Selector53ProtocolHandlerObservedMessageReadOffset = 0x34;

    public static MatrixMessageEnvelopeRegistrySnapshot EnvelopeKeyRegistry { get; } = new(
        CallbackDispatcherFileOffset,
        EnvelopeKeyDispatcherFileOffset,
        EnvelopeKeyRegistryLookupFileOffset,
        MessageEnvelopeKeyLowDwordFieldOffset,
        MessageEnvelopeKeyHighDwordFieldOffset,
        EnvelopeKeyRegistryFieldOffset1,
        EnvelopeKeyRegistryFieldOffset2,
        EnvelopeRegistryNodeKeyLowDwordFieldOffset,
        EnvelopeRegistryNodeKeyHighDwordFieldOffset,
        EnvelopeRegistryNodeValuePointerFieldOffset,
        EnvelopeRegistryTreeConstructorFileOffset,
        EnvelopeRegistrySearchFileOffset,
        EnvelopeRegistryEntryConstructorFileOffset,
        EnvelopeRegistryInsertFileOffset,
        EnvelopeRegistryDynamicFactoryFileOffset,
        EnvelopeRegistryDynamicFactoryFirstByteMinimum,
        EnvelopeRegistryDynamicFactoryFirstByteMaximum,
        ProtocolHandlerDispatcherFileOffset);

    public static IReadOnlyList<MatrixMessageNamespaceSnapshot> Selector53Namespaces { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixMessageNamespaceSnapshot(
                0,
                0x81CCAD,
                0xC22444,
                0x81CCC3,
                0x53,
                0x48,
                0x81CCD1,
                0x5B,
                0x524C30,
                0x5A0400,
                0x1B5BEAC,
                0x1C65CF0,
                0x1C67278,
                ".?AVMarketRequestComplete@Character@firefall@@"),
            new MatrixMessageNamespaceSnapshot(
                2,
                0x81EE63,
                0,
                0x81EF1C,
                0x53,
                0x40,
                0x81EF2A,
                0xCD,
                0xADBAA0,
                0xB07660,
                0x1BB0224,
                0x1C72204,
                0x1C723C0,
                ".?AVLootObjectCollected@AreaVisualData@firefall@@"),
            new MatrixMessageNamespaceSnapshot(
                3,
                0x81EF6D,
                0xC2260C,
                0x81EF83,
                0x53,
                0x30,
                0x81EF91,
                0xD0,
                0xB093C0,
                0xB474B0,
                0x1BB04E0,
                0x1C72554,
                0x1C72998,
                ".?AVAbilityActivated@Vehicle@firefall@@"),
            new MatrixMessageNamespaceSnapshot(
                4,
                0x81F206,
                0,
                0x81F29E,
                0x53,
                0x60,
                0x81F2AC,
                0xD9,
                0xB27E80,
                0xB47C40,
                0x1BB03C4,
                0x1C72464,
                0x1C727A8,
                ".?AVTookHit@Deployable@firefall@@"),
            new MatrixMessageNamespaceSnapshot(
                5,
                0x81F2EF,
                0,
                0x81F2F9,
                0x53,
                0x48,
                0x81F307,
                0xDC,
                0xB36520,
                0xB47D90,
                0x1BB033C,
                0x1C72494,
                0x1C726E8,
                ".?AVWeaponProjectileFired@Turret@firefall@@"),
            new MatrixMessageNamespaceSnapshot(
                6,
                0x81F34A,
                0,
                0x81F354,
                0x53,
                0x40,
                0x81F362,
                0xDD,
                0xB43A70,
                0xB48040,
                0x1BB0280,
                0x1C72444,
                0x1C72608,
                ".?AVLootObjectCollected@LootStoreExtension@firefall@@")
        });

    public static MatrixMessageCallbackDispatchPath ClassifyCallbackPath(
        byte envelopeKeyFirstByte,
        bool matchesNestedMessageSentinel,
        bool protocolHandlerAvailable)
    {
        if (envelopeKeyFirstByte != 0)
            return MatrixMessageCallbackDispatchPath.EnvelopeKeyRegistry;
        if (matchesNestedMessageSentinel)
            return MatrixMessageCallbackDispatchPath.NestedMessageChildren;
        return protocolHandlerAvailable
            ? MatrixMessageCallbackDispatchPath.ProtocolHandlerPrimaryVtable
            : MatrixMessageCallbackDispatchPath.ProtocolHandlerUnavailable;
    }

    public static int GetProtocolHandlerVtableSlotOffset(int messageSelector)
    {
        if (messageSelector < ProtocolHandlerFirstSelector
            || messageSelector > ProtocolHandlerLastSelector)
            throw new ArgumentOutOfRangeException(nameof(messageSelector));

        return ProtocolHandlerVtableSlotBaseOffset
            + ProtocolHandlerVtableEntrySize * (messageSelector - ProtocolHandlerFirstSelector);
    }
}
