namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::PerformQuickChatCommand, metadata ID 144.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct PerformQuickChatCommand(ulong Target, uint Value)
{
    public const ushort MessageId = 144;
    public const int ValueOffset = 0x28;
}
