namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::PerformEmote, metadata ID 142.
/// The u32 and u16 payload fields are retained positionally.
/// </summary>
public readonly record struct PerformEmote(ulong Target, uint Value28, short Value2C)
{
    public const ushort MessageId = 142;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
