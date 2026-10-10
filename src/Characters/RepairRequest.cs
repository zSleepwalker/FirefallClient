namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RepairRequest (message ID 203).
/// Constructor 0x539D60 deep-copies 8-byte identifiers into the vector at
/// +0x28; virtual serializer 0x5B87C0 writes that vector in 255-item chunks.
/// The native caller limits the number of entries to 100.
/// </summary>
public sealed class RepairRequest
{
    public const ushort MessageId = 203;
    public const int IdentifiersOffset = 0x28;
    public const int MaxCallerIdentifiers = 100;

    public RepairRequest(ulong target, IEnumerable<ulong> identifiers)
    {
        ArgumentNullException.ThrowIfNull(identifiers);
        Target = target;
        Identifiers = Array.AsReadOnly(identifiers.ToArray());
        if (Identifiers.Count == 0 || Identifiers.Count > MaxCallerIdentifiers)
            throw new ArgumentOutOfRangeException(nameof(identifiers), "The traced sender accepts 1..100 identifiers.");
    }

    public ulong Target { get; }
    public IReadOnlyList<ulong> Identifiers { get; }

    /// <summary>Write only the payload of native virtual serializer 0x5B87C0; not outer framing.</summary>
    public void WritePayload(BinaryWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        CharacterCommandWire.WriteChunkedSequence(writer, Identifiers, static (w, value) => w.Write(value));
    }
}
