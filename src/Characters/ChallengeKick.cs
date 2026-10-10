namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ChallengeKick, metadata ID 99.
/// The shared-text pointer is opaque and process-local.
/// </summary>
public readonly record struct ChallengeKick(
    ulong Target,
    uint Value28,
    uint Value2C,
    uint SharedTextReference)
{
    public const ushort MessageId = 99;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int SharedTextReferenceOffset = 0x30;
}
