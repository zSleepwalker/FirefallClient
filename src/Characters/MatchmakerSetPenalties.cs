namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::MatchmakerSetPenalties, metadata ID 92.
/// Payload field meanings and packet order are unknown.
/// </summary>
public sealed class MatchmakerSetPenalties
{
    public const ushort MessageId = 92;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;

    public MatchmakerSetPenalties(
        ulong target,
        uint value28,
        byte value2C,
        uint value30,
        byte value34)
    {
        Target = target;
        Value28 = value28;
        Value2C = value2C;
        Value30 = value30;
        Value34 = value34;
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public byte Value2C { get; }
    public uint Value30 { get; }
    public byte Value34 { get; }
}
