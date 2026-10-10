namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ActivateConsumable, metadata ID 138.
/// Two subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct ActivateConsumable(ulong Target, uint Value28, uint Value2C)
{
    public const ushort MessageId = 138;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
