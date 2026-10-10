namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::LFGLeave, metadata ID 108.
/// The two subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct LFGLeave(ulong Target, uint Value28, uint Value2C)
{
    public const ushort MessageId = 108;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
