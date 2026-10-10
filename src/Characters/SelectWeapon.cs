namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SelectWeapon, metadata ID 125.
/// Field meanings are retained positionally.
/// </summary>
public readonly record struct SelectWeapon(
    ulong Target, uint Value28, byte Value2C, byte Value2D)
{
    public const ushort MessageId = 125;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value2DOffset = 0x2D;
}
