namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestFrameLevelReset, metadata ID 211.
/// The populated constructor copies one opaque DWORD.
/// </summary>
public readonly record struct RequestFrameLevelReset(ulong Target, uint Value28)
{
    public const ushort MessageId = 211;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
}
