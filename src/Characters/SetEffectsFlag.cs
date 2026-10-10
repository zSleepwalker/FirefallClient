namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SetEffectsFlag, metadata ID 148.
/// Both subclass bytes are retained positionally.
/// </summary>
public readonly record struct SetEffectsFlag(ulong Target, byte Value28, byte Value29)
{
    public const ushort MessageId = 148;
    public const int Value28Offset = 0x28;
    public const int Value29Offset = 0x29;
}
