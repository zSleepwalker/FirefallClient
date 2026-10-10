namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ResourceNodeBeaconCalldownRequest, metadata ID 182.
/// The three subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct ResourceNodeBeaconCalldownRequest(
    ulong Target,
    uint Value28,
    uint Value2C,
    uint Value30)
{
    public const ushort MessageId = 182;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
}
