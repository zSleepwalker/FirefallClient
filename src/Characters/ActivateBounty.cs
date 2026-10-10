namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ActivateBounty, metadata ID 157.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct ActivateBounty(ulong Target, uint Value)
{
    public const ushort MessageId = 157;
    public const int ValueOffset = 0x28;
}
