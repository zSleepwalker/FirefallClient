namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::TimedDailyRewardRequest, metadata ID 201.
/// The subclass byte is retained positionally; its meaning is unconfirmed.
/// </summary>
public readonly record struct TimedDailyRewardRequest(ulong Target, byte Value28)
{
    public const ushort MessageId = 201;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
