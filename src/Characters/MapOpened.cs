namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::MapOpened, metadata ID 109.
/// The observed constructor initializes no subclass fields.
/// </summary>
public readonly record struct MapOpened(ulong Target)
{
    public const ushort MessageId = 109;
    public const int TargetOffset = 0x18;
}
