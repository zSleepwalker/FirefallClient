namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::QueueUnstuck, metadata ID 178.
/// Its constructor initializes no subclass payload fields.
/// </summary>
public readonly record struct QueueUnstuck(ulong Target)
{
    public const ushort MessageId = 178;
    public const int TargetOffset = 0x18;
}
