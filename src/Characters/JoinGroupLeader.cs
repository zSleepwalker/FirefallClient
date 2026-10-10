namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::JoinGroupLeader, metadata ID 87.
/// Its observed constructor has no subclass payload fields.
/// </summary>
public readonly record struct JoinGroupLeader(ulong Target)
{
    public const ushort MessageId = 87;
    public const int TargetOffset = 0x18;
}
