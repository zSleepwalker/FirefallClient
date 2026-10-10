namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RevertAllEliteLevelsUpgrades, metadata ID 228.
/// The observed constructor initializes no subclass fields.
/// </summary>
public readonly record struct RevertAllEliteLevelsUpgrades(ulong Target)
{
    public const ushort MessageId = 228;
    public const int TargetOffset = 0x18;
}
