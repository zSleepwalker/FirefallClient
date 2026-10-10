namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ChallengeCreate, metadata ID 94.
/// Identifier entries are opaque 8-byte values.
/// </summary>
public sealed class ChallengeCreate
{
    public const ushort MessageId = 94;
    public const int TargetOffset = 0x18;
    public const int IdentifiersOffset = 0x28;
    public const int IdentifierEntrySize = 8;

    public ChallengeCreate(ulong target, IEnumerable<ulong> identifiers)
    {
        ArgumentNullException.ThrowIfNull(identifiers);
        Target = target;
        Identifiers = Array.AsReadOnly(identifiers.ToArray());
    }

    public ulong Target { get; }
    public IReadOnlyList<ulong> Identifiers { get; }
}
