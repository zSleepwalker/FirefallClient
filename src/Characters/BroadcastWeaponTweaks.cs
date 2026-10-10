namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::BroadcastWeaponTweaks, metadata ID 114.
/// Embedded reference values are retained opaquely.
/// </summary>
public readonly record struct BroadcastWeaponTweaks(
    ulong Target,
    byte Value28,
    byte Value29,
    uint ReferencedObject,
    uint ReferenceControlBlock,
    uint Value38)
{
    public const ushort MessageId = 114;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value29Offset = 0x29;
    public const int NestedSubobjectOffset = 0x2C;
    public const int ReferencedObjectOffset = 0x30;
    public const int ReferenceControlBlockOffset = 0x34;
    public const int Value38Offset = 0x38;
    public const uint NativeSubobjectVTableAddress = 0x1C80CC4;
}
