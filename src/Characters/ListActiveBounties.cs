namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ListActiveBounties, metadata ID 158.
/// Both subclass values are retained positionally.
/// </summary>
public readonly record struct ListActiveBounties(ulong Target, uint Value, byte Flag)
{
    public const ushort MessageId = 158;
    public const int ValueOffset = 0x28;
    public const int FlagOffset = 0x2C;
}
