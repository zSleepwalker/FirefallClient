namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::GeographicalReportRequest, metadata ID 184.
/// The one-byte subclass field is retained positionally.
/// </summary>
public readonly record struct GeographicalReportRequest(ulong Target, byte Value28)
{
    public const ushort MessageId = 184;
    public const int Value28Offset = 0x28;
}
