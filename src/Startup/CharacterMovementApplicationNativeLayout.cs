namespace Firefall.Reconstruction.Startup;

public readonly record struct CharacterMovementReceiverHandlerSnapshot(
    int MessageSelector,
    uint DispatcherCaseVirtualAddress,
    uint DispatcherCaseFileOffset,
    int VtableSlotOffset,
    uint MethodVirtualAddress,
    uint MethodFileOffset,
    uint ApplicationHelperFileOffset,
    int? NumericApplicationEventCode);

/// <summary>
/// Records the concrete tfGameProtocolHandler slots and application paths for
/// Character movement selectors 0x70 through 0x73, including observed
/// producer-consumer layout discrepancies without assigning field semantics.
/// </summary>
public static class CharacterMovementApplicationNativeLayout
{
    public const string ReceiverRttiName = ".?AVtfGameProtocolHandler@@";
    public const uint ReceiverConstructorFileOffset = 0xAC8CA0;
    public const uint ReceiverConstructorVtableStoreFileOffset = 0xAC8CE2;
    public const uint ReceiverVtableVirtualAddress = 0x1C71EC0;
    public const uint ReceiverVtableFileOffset = 0x1870AC0;
    public const uint MessageDispatcherFileOffset = 0x81BB90;
    public const uint MessageDispatcherSelectorTableVirtualAddress = 0xC1D6DC;
    public const uint MessageDispatcherSelectorTableFileOffset = 0x81CADC;
    public const int FirstMessageSelector = 0x20;
    public const int VtableSlotBaseOffset = 4;
    public const int VtableEntrySize = 4;
    public const uint UiManagerGlobalVirtualAddress = 0x1E002B8;
    public const int UiManagerDispatchVtableSlotOffset = 0xAC;

    public const uint ReceiverPreDispatchGuardHelperFileOffset = 0x322820;

    public const uint PublicDebugMovementHandlerFileOffset = 0xACDFF0;
    public const uint PublicDebugMovementPayloadHelperFileOffset = 0xC245A0;
    public const uint PublicDebugMovementConditionalPayloadHelperFileOffset = 0xC24780;
    public const int PublicDebugMovementFirstReaderFieldOffset = 0x28;
    public const int PublicDebugMovementFirstReaderFieldWidth = 2;
    public const int PublicDebugMovementFirstHandlerReadOffset = 0x28;
    public const int PublicDebugMovementFirstHandlerReadWidth = 4;
    public const int PublicDebugMovementFirstRecordStateOffset = 0x30;
    public const int PublicDebugMovementStateProbeVtableSlotOffset = 0x0C;
    public const int PublicDebugMovementStateActionVtableSlotOffset = 0x18;
    public const int PublicDebugMovementOptionalGateOffset = 0x74;
    public const uint PublicDebugMovementOptionalGateReaderFileOffset = 0x5FF760;
    public const uint PublicDebugMovementOptionalValueReaderFileOffset = 0x5FF520;
    public const int PublicDebugMovementNumericApplicationEventCode = 363;

    public const uint ForcedMovementHandlerFileOffset = 0xACE130;
    public const uint ForcedMovementPayloadHelperFileOffset = 0xC24780;
    public const int ForcedMovementNestedStateOffset = 0x28;
    public const int ForcedMovementNestedStateFirstDwordValue = 0;
    public const int ForcedMovementNestedReaderFirstWrittenDwordOffset = 0x2C;
    public const int ForcedMovementDiscriminatorOffset = 0x30;
    public const int ForcedMovementConditionalPairOffset = 0x138;
    public const int ForcedMovementTrailingWordOffset = 0x1E0;
    public const int ForcedMovementNumericApplicationEventCode = 365;

    public const uint ForcedMovementCancelledHandlerFileOffset = 0xACAA70;
    public const uint ForcedMovementCancelledSingletonAccessorFileOffset = 0x49DCC0;
    public const int ForcedMovementCancelledOwnerStateFieldOffset = 0x43C8;
    public const uint ForcedMovementCancelledListUpdaterFileOffset = 0x9090A0;
    public const uint ForcedMovementCancelledEntryUpdaterFileOffset = 0x909120;
    public const int ForcedMovementCancelledInputListPointerOffset = 0x28;
    public const int ForcedMovementCancelledIgnoredWordOffset = 0x2C;
    public const int ForcedMovementCancelledMissingStateDiagnostic = 1651;

    public const uint GrappleClimbPermissionHandlerFileOffset = 0xACAAC0;
    public const uint GrappleClimbPermissionSingletonAccessorFileOffset = 0x49DCC0;
    public const int GrappleClimbPermissionOwnerStateFieldOffset = 0x43B8;
    public const uint GrappleClimbPermissionApplicationHelperFileOffset = 0x8EDD10;
    public const int GrappleClimbPermissionMessageValueOffset = 0x28;
    public const int GrappleClimbPermissionMessageListPointerOffset = 0x2C;
    public const int GrappleClimbPermissionNestedStateFirstDwordValue = 0;
    public const int GrappleClimbPermissionNestedReaderFirstWrittenDwordOffset = 0x2C;
    public const int GrappleClimbPermissionManagedRecordStride = 20;
    public const int GrappleClimbPermissionGeneratedRecordStride = 84;
    public const int GrappleClimbPermissionMissingStateDiagnostic = 1646;
    public const int GrappleClimbPermissionNumericApplicationEventCode = 213;

    public static IReadOnlyList<CharacterMovementReceiverHandlerSnapshot> ReceiverHandlers { get; } =
        Array.AsReadOnly(new[]
        {
            new CharacterMovementReceiverHandlerSnapshot(
                0x70, 0xC1D4E0, 0x81C8E0, 0x144, 0xECEBF0, 0xACDFF0,
                PublicDebugMovementPayloadHelperFileOffset, PublicDebugMovementNumericApplicationEventCode),
            new CharacterMovementReceiverHandlerSnapshot(
                0x71, 0xC1D50B, 0x81C90B, 0x148, 0xECED30, 0xACE130,
                ForcedMovementPayloadHelperFileOffset, ForcedMovementNumericApplicationEventCode),
            new CharacterMovementReceiverHandlerSnapshot(
                0x72, 0xC1D536, 0x81C936, 0x14C, 0xECB670, 0xACAA70,
                ForcedMovementCancelledListUpdaterFileOffset, null),
            new CharacterMovementReceiverHandlerSnapshot(
                0x73, 0xC1D561, 0x81C961, 0x150, 0xECB6C0, 0xACAAC0,
                GrappleClimbPermissionApplicationHelperFileOffset, GrappleClimbPermissionNumericApplicationEventCode)
        });
}
