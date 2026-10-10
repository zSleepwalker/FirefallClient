namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestAchievementStatus, metadata ID 169.
/// The sole subclass DWORD is preserved without assigning an unverified
/// domain meaning.
/// </summary>
public readonly record struct RequestAchievementStatus(ulong Target, uint Value)
{
    public const ushort MessageId = 169;
    public const int ValueOffset = 0x28;
}
