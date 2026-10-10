namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ResourceLocationInfosRequest, metadata ID 187.
/// The observed constructor initializes no subclass payload.
/// </summary>
public readonly record struct ResourceLocationInfosRequest(ulong Target)
{
    public const ushort MessageId = 187;
    public const int TargetOffset = 0x18;
}
