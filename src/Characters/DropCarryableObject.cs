namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::DropCarryableObject, metadata ID 190.
/// Its payload is an opaque 64-bit object identifier.
/// </summary>
public readonly record struct DropCarryableObject(ulong Target, ulong ObjectIdentifier)
{
    public const ushort MessageId = 190;
    public const int TargetOffset = 0x18;
    public const int ObjectIdentifierOffset = 0x28;
    public const int ObjectIdentifierSize = 8;
}
