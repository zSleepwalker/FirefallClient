namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ReloadWeapon, metadata ID 126.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct ReloadWeapon(ulong Target, uint Value)
{
    public const ushort MessageId = 126;
    public const int ValueOffset = 0x28;
}
