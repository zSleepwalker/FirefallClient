namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::Fabrication_FetchInstance, metadata ID 242.
/// Its subclass DWORD remains positional because its meaning is unknown.
/// </summary>
public readonly record struct Fabrication_FetchInstance(ulong Target, uint Value28)
{
    public const ushort MessageId = 242;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
