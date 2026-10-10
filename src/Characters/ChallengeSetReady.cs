namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ChallengeSetReady, metadata ID 100.
/// The two DWORDs and trailing byte are retained positionally.
/// </summary>
public readonly record struct ChallengeSetReady(
    ulong Target,
    uint Value28,
    uint Value2C,
    byte Value30)
{
    public const ushort MessageId = 100;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
}
