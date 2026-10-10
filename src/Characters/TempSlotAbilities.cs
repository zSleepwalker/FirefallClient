namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::TempSlotAbilities, metadata ID 112.
/// The observed constructor initializes four DWORDs to zero and a trailing sentinel to all ones.
/// </summary>
public readonly record struct TempSlotAbilities(ulong Target)
{
    public const ushort MessageId = 112;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
    public const int SentinelOffset = 0x38;
    public const uint SentinelValue = uint.MaxValue;

    public uint Value28 => 0;
    public uint Value2C => 0;
    public uint Value30 => 0;
    public uint Value34 => 0;
    public uint Sentinel => SentinelValue;
}
