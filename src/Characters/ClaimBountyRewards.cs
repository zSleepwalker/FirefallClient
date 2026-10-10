namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ClaimBountyRewards, metadata ID 250.
/// The only found constructor initializes the inherited target.
/// </summary>
public readonly record struct ClaimBountyRewards(ulong Target)
{
    public const ushort MessageId = 250;
    public const int TargetOffset = 0x18;
}
