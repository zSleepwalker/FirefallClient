namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SeatChangeRequest, metadata ID 216.
/// The subclass byte is retained positionally; its meaning is unconfirmed.
/// </summary>
public readonly record struct SeatChangeRequest(ulong Target, byte Value28)
{
    public const ushort MessageId = 216;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
