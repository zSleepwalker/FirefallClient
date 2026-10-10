namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::NPCApplyEffect, metadata ID 130.
/// The three DWORD fields are retained positionally.
/// </summary>
public readonly record struct NpcApplyEffect(
    ulong Target, uint Value28, uint Value30, uint Value34)
{
    public const ushort MessageId = 130;
    public const int Value28Offset = 0x28;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
}
