namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ClientQueryInteractionStatus, metadata ID 186.
/// The pair and byte meanings are unknown.
/// </summary>
public readonly record struct ClientQueryInteractionStatus(
    ulong Target,
    ulong Value28,
    byte Value30)
{
    public const ushort MessageId = 186;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value30Offset = 0x30;
}
