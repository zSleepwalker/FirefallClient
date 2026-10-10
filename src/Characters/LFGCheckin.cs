namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::LFGCheckin, metadata ID 107.
/// The two subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct LFGCheckin(ulong Target, uint Value28, uint Value2C)
{
    public const ushort MessageId = 107;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
