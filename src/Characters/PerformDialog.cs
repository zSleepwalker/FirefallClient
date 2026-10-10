namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::PerformDialog, metadata ID 146.
/// Fields are named by native offset; unassigned gaps remain absent.
/// </summary>
public readonly record struct PerformDialog(
    ulong Target,
    uint Value28,
    uint Value30,
    uint Value34,
    uint Value38,
    uint Value3C,
    uint Value40,
    byte Value44,
    uint Value48,
    byte Value4C,
    uint Value50,
    uint Value58,
    uint Value5C)
{
    public const ushort MessageId = 146;
}
