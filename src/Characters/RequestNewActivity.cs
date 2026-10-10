namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestNewActivity, metadata ID 172.
/// The observed constructor initializes no subclass fields.
/// </summary>
public readonly record struct RequestNewActivity(ulong Target)
{
    public const ushort MessageId = 172;
    public const int TargetOffset = 0x18;
}
