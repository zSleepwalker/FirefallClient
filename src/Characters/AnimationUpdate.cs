namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::AnimationUpdate, metadata ID 149.
/// Its three subclass values are retained positionally.
/// </summary>
public readonly record struct AnimationUpdate(
    ulong Target,
    uint Value1,
    uint Value2,
    byte Value3)
{
    public const ushort MessageId = 149;
    public const int Value1Offset = 0x28;
    public const int Value2Offset = 0x2C;
    public const int Value3Offset = 0x30;
}
