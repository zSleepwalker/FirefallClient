namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::EquipExperimentalLoadout, metadata ID 236.
/// Its subclass DWORD remains positional because its meaning is unknown.
/// </summary>
public readonly record struct EquipExperimentalLoadout(ulong Target, uint Value28)
{
    public const ushort MessageId = 236;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
