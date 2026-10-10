using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

internal ref struct MatrixMessageSpanReader
{
    private readonly ReadOnlySpan<byte> bytes;
    public int Position { get; private set; }

    public MatrixMessageSpanReader(ReadOnlySpan<byte> bytes)
    {
        this.bytes = bytes;
        Position = 0;
    }

    public bool TryReadBytes(int length, out ReadOnlySpan<byte> value)
    {
        value = default;
        if (bytes.Length - Position < length)
            return false;
        value = bytes.Slice(Position, length);
        Position += length;
        return true;
    }

    public bool TryReadByte(out byte value)
    {
        value = 0;
        if (!TryReadBytes(1, out var span))
            return false;
        value = span[0];
        return true;
    }

    public bool TryReadUInt16(out ushort value)
    {
        value = 0;
        if (!TryReadBytes(2, out var span))
            return false;
        value = BinaryPrimitives.ReadUInt16LittleEndian(span);
        return true;
    }

    public bool TryReadUInt32(out uint value)
    {
        value = 0;
        if (!TryReadBytes(4, out var span))
            return false;
        value = BinaryPrimitives.ReadUInt32LittleEndian(span);
        return true;
    }

    public bool TryReadUInt64(out ulong value)
    {
        value = 0;
        if (!TryReadBytes(8, out var span))
            return false;
        value = BinaryPrimitives.ReadUInt64LittleEndian(span);
        return true;
    }

    public bool TryReadWordLengthBlob(out ReadOnlyMemory<byte> value)
    {
        value = ReadOnlyMemory<byte>.Empty;
        if (!TryReadUInt16(out ushort length) || !TryReadBytes(length, out var span))
            return false;
        value = span.ToArray();
        return true;
    }

    public bool TryReadNullTerminatedBytes(out ReadOnlyMemory<byte> value)
    {
        value = ReadOnlyMemory<byte>.Empty;
        int length = bytes[Position..].IndexOf((byte)0);
        if (length < 0)
            return false;
        value = bytes.Slice(Position, length).ToArray();
        Position += length + 1;
        return true;
    }
}
