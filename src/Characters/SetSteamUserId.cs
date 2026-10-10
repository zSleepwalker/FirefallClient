namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SetSteamUserId, metadata ID 235.
/// The constructor copies a 64-bit value; its caller-side interpretation is unverified.
/// </summary>
public readonly record struct SetSteamUserId(ulong Target, ulong SteamUserId)
{
    public const ushort MessageId = 235;
    public const int TargetOffset = 0x18;
    public const int SteamUserIdOffset = 0x28;
}
