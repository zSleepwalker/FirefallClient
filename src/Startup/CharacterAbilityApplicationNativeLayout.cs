namespace Firefall.Reconstruction.Startup;

public readonly record struct CharacterAbilityReceiverHandlerSnapshot(
    int MessageDiscriminator,
    int VtableSlotOffset,
    uint MethodVirtualAddress,
    uint MethodFileOffset);

/// <summary>
/// Records observed tfGameProtocolHandler receiver mappings and application
/// calls for Character selectors 0x74 through 0x77 without assigning names to
/// numeric application-event codes or opaque message fields.
/// </summary>
public static class CharacterAbilityApplicationNativeLayout
{
    public const string ReceiverRttiName = ".?AVtfGameProtocolHandler@@";
    public const uint ReceiverConstructorFileOffset = 0xAC8CA0;
    public const uint ReceiverConstructorVtableStoreFileOffset = 0xAC8CE2;
    public const uint ReceiverVtableVirtualAddress = 0x1C71EC0;
    public const uint ReceiverVtableFileOffset = 0x1870AC0;

    public const uint AbilityActivatedHandlerFileOffset = 0xACDD40;
    public const uint AbilityActivatedSingletonAccessorFileOffset = 0x49DCC0;
    public const int AbilityActivatedOwnerStateFieldOffset = 0x43C8;
    public const int AbilityActivatedMessageKeyOffset = 0x28;
    public const int AbilityActivatedStateFirstDwordOffset = 0x30;
    public const int AbilityActivatedStateSecondDwordOffset = 0x34;
    public const uint AbilityActivatedApplicationHelperFileOffset = 0x909B40;
    public const uint AbilityActivatedEventPayloadHelperFileOffset = 0xC24210;
    public const int AbilityActivatedEventPayloadTag = 1;
    public const int AbilityActivatedNumericApplicationEventCode = 230;

    public const uint AbilityFailedHandlerFileOffset = 0xACDDE0;
    public const uint AbilityFailedSingletonAccessorFileOffset = 0x49DCC0;
    public const int AbilityFailedOwnerStateFieldOffset = 0x43C8;
    public const int AbilityFailedFirstArgumentAddressOffset = 0x28;
    public const int AbilityFailedSecondArgumentAddressOffset = 0x30;
    public const uint AbilityFailedApplicationHelperFileOffset = 0x909C60;
    public const uint AbilityFailedSourceTreeRangeCopyFileOffset = 0x90C580;
    public const uint AbilityFailedSourceTreeEntryCloneFileOffset = 0x82C2E0;
    public const uint AbilityFailedOwnerTreeInsertFileOffset = 0x8410B0;

    public const uint AbilityCooldownsHandlerFileOffset = 0xACE210;
    public const int AbilityCooldownsBeginOffset = 0x28;
    public const int AbilityCooldownsEndOffset = 0x2C;
    public const int AbilityCooldownsObservedRecordStride = 0x40;
    public const int AbilityCooldownsObservedStringOffset1 = 0x08;
    public const int AbilityCooldownsObservedStringOffset2 = 0x0C;
    public const int AbilityCooldownsObservedByteOffset1 = 0x10;
    public const int AbilityCooldownsObservedByteOffset2 = 0x11;
    public const int AbilityCooldownsObservedByteOffset3 = 0x12;
    public const int AbilityCooldownsObservedFlagsOffset = 0x14;
    public const int AbilityCooldownsObservedOptionalBranchOffset = 0x14;
    public const int AbilityCooldownsObservedOptionalDataOffset = 0x18;
    public const int AbilityCooldownsObservedAuthorIdOffset = 0x00;
    public const int AbilityCooldownsObservedAuthorTextOffset = 0x08;
    public const int AbilityCooldownsObservedMessageTextOffset = 0x0C;
    public const int AbilityCooldownsObservedNativeChannelOffset = 0x10;
    public const int AbilityCooldownsObservedDisplayFlagsOffset = 0x11;
    public const int AbilityCooldownsObservedTemplateSelectorOffset = 0x12;
    public const uint AbilityCooldownsChatProcessorFileOffset = 0x6E2590;
    public const uint AbilityCooldownsFormattedMessageHelperFileOffset = 0x6E3670;
    public const uint AbilityCooldownsDisplayNotificationFileOffset = 0x6E3460;
    public const int AbilityCooldownsDisplayNotificationMessageId = 335;

    public const uint NpcInteractionHandlerFileOffset = 0xACE2A0;
    public const uint NpcInteractionRootAccessorFileOffset = 0x49DDD0;
    public const uint NpcInteractionSingletonAccessorFileOffset = 0x49DCC0;
    public const int NpcInteractionRootStatePointerOffset = 0x4D20;
    public const uint NpcInteractionMessagePointerLoadFileOffset = 0xACE2B8;
    public const int NpcInteractionMessageKeyOffset = 0x28;
    public const int NpcInteractionMessageStateOffset = 0x30;
    public const uint NpcInteractionApplicationHelperFileOffset = 0x4C9250;
    public const uint NpcInteractionApplicationHelperCallFileOffset = 0xACE2CC;
    public const uint NpcInteractionMessageStateArgumentLoadFileOffset = 0x4C927E;
    public const uint NpcInteractionMessageStateArgumentPushFileOffset = 0x4C92B9;
    public const uint NpcInteractionMessageStateSwapCallFileOffset = 0x4C92BD;
    public const int NpcInteractionOwnerMapOffset = 0x4338;
    public const uint NpcInteractionMessageStateSwapHelperFileOffset = 0x51C9D0;
    public const int NpcInteractionMessageStateSwapByteCount = 0x20;
    public const int NpcInteractionMessageStateEndOffsetExclusive =
        NpcInteractionMessageStateOffset + NpcInteractionMessageStateSwapByteCount;
    public const uint NpcInteractionEventPayloadHelperFileOffset = 0xC245A0;
    public const int NpcInteractionNumericApplicationEventCode = 356;
    public const int NpcInteractionEventPayloadTag = 5;

    public const uint UiManagerGlobalVirtualAddress = 0x1E002B8;
    public const int UiManagerDispatchVtableSlotOffset = 0xAC;

    public static IReadOnlyList<CharacterAbilityReceiverHandlerSnapshot> ReceiverHandlers { get; } =
        Array.AsReadOnly(new[]
        {
            new CharacterAbilityReceiverHandlerSnapshot(0x74, 0x154, 0xECE940, 0xACDD40),
            new CharacterAbilityReceiverHandlerSnapshot(0x75, 0x158, 0xECE9E0, 0xACDDE0),
            new CharacterAbilityReceiverHandlerSnapshot(0x76, 0x15C, 0xECEE10, 0xACE210),
            new CharacterAbilityReceiverHandlerSnapshot(0x77, 0x160, 0xECEEA0, 0xACE2A0)
        });
}
