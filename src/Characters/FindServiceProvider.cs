namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FindServiceProvider, metadata ID 195.
/// The byte's meaning is unknown.
/// </summary>
public readonly record struct FindServiceProvider(ulong Target, byte Value28)
{
    public const ushort MessageId = 195;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
