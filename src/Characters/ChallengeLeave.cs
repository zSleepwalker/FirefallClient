namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ChallengeLeave, metadata ID 95.
/// The two subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct ChallengeLeave(ulong Target, uint Value28, uint Value2C)
{
    public const ushort MessageId = 95;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
