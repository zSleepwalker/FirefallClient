namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestTeleport (message metadata ID 210).
/// The populated constructor copies an opaque 64-bit value to +0x28.
/// Its semantic type and per-class wire encoding remain unknown.
/// </summary>
public readonly record struct RequestTeleport(ulong Target, ulong Value)
{
    public const ushort MessageId = 210;
    public const int ValueOffset = 0x28;
}
