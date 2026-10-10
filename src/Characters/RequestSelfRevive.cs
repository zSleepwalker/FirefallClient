namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestSelfRevive, metadata ID 209.
/// The observed constructor initializes no subclass fields.
/// </summary>
public readonly record struct RequestSelfRevive(ulong Target)
{
    public const ushort MessageId = 209;
    public const int TargetOffset = 0x18;
}
