namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::Fabrication_ApplyAction, metadata ID 244.
/// The subclass DWORDs remain positional because their meanings are unknown.
/// </summary>
public readonly record struct Fabrication_ApplyAction(
    ulong Target,
    uint Value28,
    uint Value2C,
    uint Value30)
{
    public const ushort MessageId = 244;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
}
