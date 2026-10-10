namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::Fabrication_Finalize, metadata ID 246.
/// The subclass values are positional because their meanings are unknown.
/// </summary>
public readonly record struct Fabrication_Finalize(ulong Target, uint Value28, byte Value2C)
{
    public const ushort MessageId = 246;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
