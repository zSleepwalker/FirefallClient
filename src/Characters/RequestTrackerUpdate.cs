namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestTrackerUpdate, metadata ID 217.
/// The two DWORDs are retained positionally; their meanings are unknown.
/// </summary>
public readonly record struct RequestTrackerUpdate(
    ulong Target,
    uint Value28,
    uint Value2C)
{
    public const ushort MessageId = 217;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
