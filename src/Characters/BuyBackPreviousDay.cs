namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::BuyBackPreviousDay, metadata ID 224.
/// The subclass field is retained positionally; its meaning is unconfirmed.
/// </summary>
public readonly record struct BuyBackPreviousDay(ulong Target, uint Value28)
{
    public const ushort MessageId = 224;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
