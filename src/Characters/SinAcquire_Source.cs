namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SinAcquire_Source, metadata ID 113.
/// The two subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct SinAcquire_Source(ulong Target, uint Value28, uint Value2C)
{
    public const ushort MessageId = 113;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
