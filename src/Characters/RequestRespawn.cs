namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestRespawn constructor layout. The only
/// vtable-reference hit is a kind-only constructor, which initializes message
/// metadata ID 198 and writes no subclass payload fields. Its concrete body
/// writer at 0x5062D0 emits the inherited 64-bit handle followed by ID 198.
/// </summary>
public readonly record struct RequestRespawn(ulong Target)
{
    public const ushort MessageId = 198;

    /// <summary>Complete message body traced at 0x5062D0; excludes transport framing.</summary>
    public byte[] EncodeBody()
    {
        using var stream = new MemoryStream(9);
        using var writer = new BinaryWriter(stream);
        writer.Write(Target);
        CharacterCommandWire.WriteMessageId(writer, MessageId);
        return stream.ToArray();
    }
}
