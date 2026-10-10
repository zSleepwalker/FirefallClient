namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ToggleMarketplace, metadata ID 86.
/// The stored byte is preserved without assuming its enabled-state meaning.
/// </summary>
public sealed class ToggleMarketplace
{
    public const ushort MessageId = 86;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;

    public ToggleMarketplace(ulong target, byte value28)
    {
        Target = target;
        Value28 = value28;
    }

    public ulong Target { get; }
    public byte Value28 { get; }
}
