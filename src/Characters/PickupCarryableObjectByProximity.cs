namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::PickupCarryableObjectByProximity, metadata ID 189.
/// Its payload is an opaque 64-bit object identifier.
/// </summary>
public readonly record struct PickupCarryableObjectByProximity(ulong Target, ulong ObjectIdentifier)
{
    public const ushort MessageId = 189;
    public const int TargetOffset = 0x18;
    public const int ObjectIdentifierOffset = 0x28;
    public const int ObjectIdentifierSize = 8;
}
