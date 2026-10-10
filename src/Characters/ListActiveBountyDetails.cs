namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ListActiveBountyDetails, metadata ID 159.
/// Both subclass values are retained positionally.
/// </summary>
public readonly record struct ListActiveBountyDetails(ulong Target, uint Value, byte Flag)
{
    public const ushort MessageId = 159;
    public const int ValueOffset = 0x28;
    public const int FlagOffset = 0x2C;
}
