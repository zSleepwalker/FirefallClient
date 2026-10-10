namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::JoinSquadLeadersArc, metadata ID 213.
/// The observed constructor initializes no subclass payload.
/// </summary>
public readonly record struct JoinSquadLeadersArc(ulong Target)
{
    public const ushort MessageId = 213;
    public const int TargetOffset = 0x18;
}
