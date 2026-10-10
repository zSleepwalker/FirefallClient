namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::Fabrication_Start, metadata ID 243.
/// The subclass values are positional because their meanings are unknown.
/// </summary>
public readonly record struct Fabrication_Start(ulong Target, uint Value28, byte Value2C)
{
    public const ushort MessageId = 243;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
