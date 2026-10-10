namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::NPCRemoveEffect, metadata ID 131.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct NpcRemoveEffect(ulong Target, uint Value)
{
    public const ushort MessageId = 131;
    public const int ValueOffset = 0x28;
}
