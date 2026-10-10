namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::AcquireWeaponTarget, metadata ID 128.
/// The positional fields' gameplay meanings are unknown.
/// </summary>
public readonly record struct AcquireWeaponTarget(
    ulong Target, uint Value28, uint Value30, uint Value34, uint Value38, byte Value3C)
{
    public const ushort MessageId = 128;
    public const int Value28Offset = 0x28;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
    public const int Value38Offset = 0x38;
    public const int Value3COffset = 0x3C;
}
