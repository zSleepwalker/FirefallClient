namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ChallengeSetRoleAndTeam, metadata ID 103.
/// Six subclass values are retained positionally.
/// </summary>
public readonly record struct ChallengeSetRoleAndTeam(
    ulong Target,
    uint Value28,
    uint Value2C,
    uint Value30,
    uint Value34,
    byte Value38,
    uint Value3C)
{
    public const ushort MessageId = 103;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
    public const int Value38Offset = 0x38;
    public const int Value3COffset = 0x3C;
}
