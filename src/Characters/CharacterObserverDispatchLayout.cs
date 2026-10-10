namespace Firefall.Characters;

public readonly record struct CharacterObserverDispatchSnapshot(
    ushort MessageId,
    string MessageName,
    int GateMethodFileOffset,
    int SenderMethodFileOffset,
    bool BuilderAlsoGatesInline);

public readonly record struct CharacterObserverBuilderCallerSnapshot(
    int CallerMethodFileOffset,
    int CallSiteFileOffset);

public readonly record struct ShoppingListSourceFieldSnapshot(
    int NativeOffset,
    string ObservedShape);

/// <summary>
/// Native dispatch observations for selected remote-observer message helpers.
/// The original native class identity is not established.
/// Offsets are raw file offsets or offsets within the indicated native object.
/// </summary>
public static class CharacterObserverDispatchLayout
{
    private static readonly CharacterObserverDispatchSnapshot[] Dispatches =
    [
        new(0xAE, "LogDirectActivityRequest", 0x90CBC0, 0x90EE20, false),
        new(0xAF, "LogActivityPush", 0x90CC30, 0x90EF50, false),
        new(0xC2, "UpdateShoppingList", 0x90CF60, 0x90F260, true)
    ];

    private static readonly CharacterObserverBuilderCallerSnapshot[] BuilderCallers =
    [
        new(0x49B3D0, 0x49B40A),
        new(0x4A7980, 0x4A7EE3)
    ];

    private static readonly ShoppingListSourceFieldSnapshot[] ShoppingListStateFields =
    [
        new(0x00, "Outer ordered map head pointer"),
        new(0x04, "Outer map size"),
        new(0x08, "Dirty byte"),
        new(0x0C, "Flush interval"),
        new(0x10, "Last-send tick")
    ];

    public const int ShoppingListBuilderMethodFileOffset = 0x9095B0;
    public const int ShoppingListStateOwnerOffset = 0x43C8;
    public const int ShoppingListPersistentOrderedMapOffset = 0x00;
    public const int ShoppingListTextKeyedMapOffset = 0x2C;
    public const int ShoppingListViewTreeOffset = 0x4C;
    public const int ShoppingListAdditionalCollectionOffset = 0x24;
    public const int ShoppingListStateDestructorMethodFileOffset = 0x908D30;
    public const int ShoppingListResetOwnerMethodFileOffset = 0x4A0700;
    public const int ShoppingListComputedViewReaderMethodFileOffset = 0xA242A0;
    public const int ShoppingListScriptViewReaderMethodFileOffset = 0x9F65E0;
    public const int ShoppingListNamedValueUpdateMethodFileOffset = 0xACDD40;
    public const int ShoppingListFormatNamedValueMethodFileOffset = 0x909B40;
    public const int ShoppingListNamedValueEventId = 230;
    public const int ShoppingListOuterMapEntryFirstWordOffset = 0x10;
    public const int ShoppingListOuterMapEntrySecondWordOffset = 0x14;
    public const int ShoppingListOuterMapKeyOffset = 0x14;
    public const int ShoppingListOuterMapInnerSetHeadOffset = 0x18;
    public const int ShoppingListOuterMapInnerSetSizeOffset = 0x1C;
    public const int ShoppingListInnerElementFirstWordOffset = 0x10;
    public const int ShoppingListInnerElementByteOffset = 0x14;
    public const int ShoppingListFlushRequiresDirtyByte = 1;
    public const int ShoppingListContextKeyLowWordOffset = 0x18;
    public const int ShoppingListContextKeyHighWordOffset = 0x1C;
    public const byte ShoppingListContextKeyForcedLowByte = 4;
    public const int ShoppingListReadyEventId = 219;
    public const int ShoppingListSourceMethodFileOffset = 0x909120;
    public const int ShoppingListAddEntryMethodFileOffset = 0x9092A0;
    public const int ShoppingListResolveComponentsMethodFileOffset = 0x9D4C00;
    public const int ObserverAvailabilityFieldOffset = 0x820;
    public const int ObserverSignedThresholdFieldOffset = 0x0C;
    public const int AcceptedObserverThresholdExclusive = 3;
    public const int ObserverFlagsByteOffset = 0x14;
    public const byte OptionalLogFlagMask = 0x08;
    public const int ShoppingListMessageIdFieldOffset = 0x10;
    public const int EncodedDescriptorFirstWordOffset = 0x18;
    public const int EncodedDescriptorSecondWordOffset = 0x1C;

    public static IReadOnlyList<CharacterObserverDispatchSnapshot> DispatchesByMessageId => Dispatches;

    public static IReadOnlyList<CharacterObserverBuilderCallerSnapshot> ShoppingListBuilderCallers => BuilderCallers;

    public static IReadOnlyList<ShoppingListSourceFieldSnapshot> ShoppingListStateFieldsByOffset => ShoppingListStateFields;
}
