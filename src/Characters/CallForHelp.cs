namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::CallForHelp, metadata ID 151.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct CallForHelp(ulong Target, uint Value)
{
    public const ushort MessageId = 151;
    public const int ValueOffset = 0x28;
}
