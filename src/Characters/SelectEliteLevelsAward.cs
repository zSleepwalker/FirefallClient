namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SelectEliteLevelsAward, metadata ID 229.
/// The subclass field is retained positionally; its meaning is unconfirmed.
/// </summary>
public readonly record struct SelectEliteLevelsAward(ulong Target, uint Value28)
{
    public const ushort MessageId = 229;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
