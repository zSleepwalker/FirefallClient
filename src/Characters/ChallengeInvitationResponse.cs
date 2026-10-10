namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ChallengeInvitationResponse, metadata ID 97.
/// The two DWORDs and trailing byte are retained positionally.
/// </summary>
public readonly record struct ChallengeInvitationResponse(
    ulong Target,
    uint Value28,
    uint Value2C,
    byte Value30)
{
    public const ushort MessageId = 97;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
}
