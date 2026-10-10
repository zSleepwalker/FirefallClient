namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FriendsListRequest, metadata ID 248.
/// The DWORD's meaning is unknown.
/// </summary>
public readonly record struct FriendsListRequest(ulong Target, uint Value28)
{
    public const ushort MessageId = 248;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
