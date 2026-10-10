namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Inputs to the observed initial connection packet. Unknown fields retain their
/// source offsets; byte-string encoding and the trailing blob are caller supplied.
/// </summary>
public sealed record MatrixInitialPacket(
    byte Mode,
    ulong OpaquePairAtConnection848,
    uint OpaqueWordAtConnection968,
    uint OpaqueWordAtConnection96C,
    ushort OpaqueWordAtConnection970,
    byte OpaqueByteAtConnection972,
    byte OpaqueGlobalByte,
    ReadOnlyMemory<byte> GeneratedTextBytes,
    ReadOnlyMemory<byte> TrailingBlobBytes)
{
    public const byte MessageId = 0x11;
    public const uint ObservedValue1962 = 1962;
    public const int FixedEncodedSize = 30;
    public const uint ConstructorFileOffset = 0x338250;
    public const uint DestructorFileOffset = 0x338120;
    public const uint EncoderFileOffset = 0x349E60;
    public const uint EmissionMethodFileOffset = 0x4980A0;
    public const uint TransportSubmissionMethodFileOffset = 0xBD3B20;

    /// <summary>
    /// Encodes only the observed open-success variant (empty first text, absent
    /// optional value). This is the application buffer, not a framed UDP datagram.
    /// Text inputs exclude the terminator added here; no string/blob lengths are emitted.
    /// </summary>
    public byte[] EncodeOpenSuccess()
    {
        int length = checked(FixedEncodedSize + GeneratedTextBytes.Length + TrailingBlobBytes.Length);
        using var stream = new MemoryStream(length);
        using var writer = new BinaryWriter(stream);
        writer.Write(MessageId);
        writer.Write(Mode);
        writer.Write(ObservedValue1962);
        writer.Write((byte)0);
        writer.Write(OpaquePairAtConnection848);
        writer.Write(OpaqueWordAtConnection968);
        writer.Write(OpaqueWordAtConnection96C);
        writer.Write(OpaqueWordAtConnection970);
        writer.Write(OpaqueByteAtConnection972);
        writer.Write((byte)0);
        writer.Write(OpaqueGlobalByte);
        writer.Write(GeneratedTextBytes.Span);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write(TrailingBlobBytes.Span);
        return stream.ToArray();
    }
}
