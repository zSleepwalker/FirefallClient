namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::UpdateFriendStatus, metadata ID 249.
/// SharedText28 and SharedText30.Value are opaque process-local shared-text pointers.
/// </summary>
public readonly record struct UpdateFriendStatus(
    ulong Target,
    uint SharedText28,
    byte Value2C,
    NativeOptionalDword SharedText30)
{
    public const ushort MessageId = 249;
    public const int TargetOffset = 0x18;
    public const int SharedText28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int HasSharedText30Offset = 0x30;
    public const int SharedText34Offset = 0x34;
}
