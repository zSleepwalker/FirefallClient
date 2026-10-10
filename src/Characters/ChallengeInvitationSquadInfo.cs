namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ChallengeInvitationSquadInfo, metadata ID 98.
/// Identifier entries are opaque 8-byte values; the shared-text pointer is
/// process-local.
/// </summary>
public sealed class ChallengeInvitationSquadInfo
{
    public const ushort MessageId = 98;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
    public const int SharedTextReferenceOffset = 0x38;
    public const int IdentifiersOffset = 0x3C;
    public const int IdentifierEntrySize = 8;

    public ChallengeInvitationSquadInfo(
        ulong target,
        uint value28,
        uint value2C,
        uint value30,
        uint value34,
        uint sharedTextReference,
        IEnumerable<ulong> identifiers)
    {
        ArgumentNullException.ThrowIfNull(identifiers);
        Target = target;
        Value28 = value28;
        Value2C = value2C;
        Value30 = value30;
        Value34 = value34;
        SharedTextReference = sharedTextReference;
        Identifiers = Array.AsReadOnly(identifiers.ToArray());
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public uint Value2C { get; }
    public uint Value30 { get; }
    public uint Value34 { get; }
    public uint SharedTextReference { get; }
    public IReadOnlyList<ulong> Identifiers { get; }
}
