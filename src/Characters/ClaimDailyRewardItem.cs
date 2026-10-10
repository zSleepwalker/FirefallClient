namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ClaimDailyRewardItem, metadata ID 222.
/// Observed constructors zero the subclass field; no populated form was found.
/// </summary>
public readonly record struct ClaimDailyRewardItem(ulong Target)
{
    public const ushort MessageId = 222;
    public const int TargetOffset = 0x18;
    public const int ZeroedValue28Offset = 0x28;
}
