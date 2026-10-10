namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::DeactivateAbility, metadata ID 137.
/// Both subclass fields are retained positionally.
/// </summary>
public readonly record struct DeactivateAbility(ulong Target, uint Value28, byte Value2C)
{
    public const ushort MessageId = 137;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
