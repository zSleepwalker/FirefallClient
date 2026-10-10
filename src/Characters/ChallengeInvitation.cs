namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ChallengeInvitation, metadata ID 96.
/// The shared-text pointer is opaque and process-local.
/// </summary>
public readonly record struct ChallengeInvitation(
    ulong Target,
    uint Value28,
    uint Value2C,
    uint SharedTextReference,
    byte Value34,
    uint Value38)
{
    public const ushort MessageId = 96;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int SharedTextReferenceOffset = 0x30;
    public const int Value34Offset = 0x34;
    public const int Value38Offset = 0x38;
}
