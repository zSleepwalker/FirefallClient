namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::MatchAccept, metadata ID 93.
/// Payload fields remain positional; no packet order is inferred.
/// </summary>
public sealed class MatchAccept
{
    public const ushort MessageId = 93;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
    public const int Value38Offset = 0x38;

    public MatchAccept(ulong target, uint value28, uint value30, uint value34, byte value38)
    {
        Target = target;
        Value28 = value28;
        Value30 = value30;
        Value34 = value34;
        Value38 = value38;
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public uint Value30 { get; }
    public uint Value34 { get; }
    public byte Value38 { get; }
}
