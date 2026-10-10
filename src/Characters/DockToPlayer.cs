namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::DockToPlayer, metadata ID 132.
/// Five payload DWORDs are preserved positionally.
/// </summary>
public readonly record struct DockToPlayer(
    ulong Target, uint Value28, uint Value2C, uint Value30, uint Value34, uint Value38)
{
    public const ushort MessageId = 132;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
    public const int Value38Offset = 0x38;
}
