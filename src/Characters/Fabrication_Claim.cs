namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::Fabrication_Claim, metadata ID 247.
/// The two subclass DWORDs remain positional because their meanings are unknown.
/// </summary>
public readonly record struct Fabrication_Claim(ulong Target, uint Value28, uint Value2C)
{
    public const ushort MessageId = 247;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
