namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RerollEliteLevelsAwardList, metadata ID 227.
/// The observed constructor initializes no subclass fields.
/// </summary>
public readonly record struct RerollEliteLevelsAwardList(ulong Target)
{
    public const ushort MessageId = 227;
    public const int TargetOffset = 0x18;
}
