namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestTransfer, message ID 199. The domain
/// meanings of the DWORD and byte are not established by the native caller.
/// </summary>
public readonly record struct RequestTransfer(ulong Target, uint Value, byte Option)
{
    public const ushort MessageId = 199;
    public const int ValueOffset = 0x28;
    public const int OptionOffset = 0x2C;

    /// <summary>
    /// Body written by native writer 0xA4BE20: target, message ID, DWORD,
    /// byte. Connection framing is excluded.
    /// </summary>
    public byte[] EncodeBody()
    {
        using var stream = new MemoryStream(14);
        using var writer = new BinaryWriter(stream);
        writer.Write(Target);
        CharacterCommandWire.WriteMessageId(writer, MessageId);
        writer.Write(Value);
        writer.Write(Option);
        return stream.ToArray();
    }
}
