namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::CollectLoot, metadata ID 111.
/// The four subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct CollectLoot(
    ulong Target,
    uint Value28,
    uint Value2C,
    uint Value30,
    uint Value34)
{
    public const ushort MessageId = 111;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
}
