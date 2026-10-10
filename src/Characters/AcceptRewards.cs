namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::AcceptRewards, metadata ID 225.
/// The subclass field is retained positionally; its meaning is unconfirmed.
/// </summary>
public readonly record struct AcceptRewards(ulong Target, uint Value28)
{
    public const ushort MessageId = 225;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
