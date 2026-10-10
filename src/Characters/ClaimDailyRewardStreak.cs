namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ClaimDailyRewardStreak, metadata ID 223.
/// The subclass field is retained positionally; its meaning is unconfirmed.
/// </summary>
public readonly record struct ClaimDailyRewardStreak(ulong Target, uint Value28)
{
    public const ushort MessageId = 223;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
