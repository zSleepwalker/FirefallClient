namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::TutorialEventTriggeredCmd, metadata ID 239.
/// Fields are positional; their semantic roles are not established.
/// </summary>
public readonly record struct TutorialEventTriggeredCmd(
    ulong Target,
    uint Value28,
    uint Value30,
    uint Value34,
    byte Value38)
{
    public const ushort MessageId = 239;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
    public const int Value38Offset = 0x38;
}
