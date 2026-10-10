namespace Firefall.Characters;

public readonly record struct BagInventorySettingsPayload(
    uint ReferencedObject,
    uint ReferenceControlBlock,
    uint Value38);

/// <summary>
/// Native Character::Commands::BagInventorySettings, metadata ID 234.
/// Optional nested values are retained without assigning gameplay meaning.
/// </summary>
public readonly record struct BagInventorySettings(
    ulong Target,
    BagInventorySettingsPayload? Settings)
{
    public const ushort MessageId = 234;
    public const int TargetOffset = 0x18;
    public const int HasSettingsOffset = 0x28;
    public const int SettingsSubobjectOffset = 0x2C;
    public const int ReferencedObjectOffset = 0x30;
    public const int ReferenceControlBlockOffset = 0x34;
    public const int Value38Offset = 0x38;
    public const uint NativeSettingsVTableAddress = 0x1C80CC4;

    public bool HasSettings => Settings.HasValue;
}
