namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::LoseWeaponTarget, metadata ID 129.
/// The positional fields' gameplay meanings are unknown.
/// </summary>
public readonly record struct LoseWeaponTarget(ulong Target, uint Value28, uint Value30, uint Value34)
{
    public const ushort MessageId = 129;
    public const int Value28Offset = 0x28;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
}
