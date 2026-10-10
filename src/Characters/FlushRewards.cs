namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FlushRewards, metadata ID 226.
/// The observed constructor initializes no subclass fields.
/// </summary>
public readonly record struct FlushRewards(ulong Target)
{
    public const ushort MessageId = 226;
    public const int TargetOffset = 0x18;
}
