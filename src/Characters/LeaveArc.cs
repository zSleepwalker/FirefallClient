namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::LeaveArc, metadata ID 214.
/// Its constructor initializes no subclass payload fields.
/// </summary>
public readonly record struct LeaveArc(ulong Target)
{
    public const ushort MessageId = 214;
    public const int TargetOffset = 0x18;
}
