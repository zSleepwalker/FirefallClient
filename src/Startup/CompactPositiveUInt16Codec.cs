using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public enum CompactPositiveUInt16Outcome
{
    Accepted,
    Truncated,
    NonCanonicalExtendedValue
}

public readonly record struct CompactPositiveUInt16Result(
    CompactPositiveUInt16Outcome Outcome, uint? Value, int BytesConsumed);

/// <summary>
/// Values 1..255 use one byte; 256..65535 use zero followed by a little-endian WORD.
/// Decoding reports explicit errors, without simulating native cursor mutation or exception unwinding.
/// </summary>
public static class CompactPositiveUInt16Codec
{
    public const uint EncoderFileOffset = 0x320900;
    public const uint DecoderFileOffset = 0x34A020;

    public static byte[] Encode(uint value)
    {
        if (value is 0 or > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), "Only positive 16-bit values are supported.");
        if (value <= byte.MaxValue)
            return [(byte)value];
        var bytes = new byte[3];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(1), (ushort)value);
        return bytes;
    }

    public static CompactPositiveUInt16Result Decode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return new(CompactPositiveUInt16Outcome.Truncated, null, 0);
        if (bytes[0] != 0)
            return new(CompactPositiveUInt16Outcome.Accepted, bytes[0], 1);
        if (bytes.Length < 3)
            return new(CompactPositiveUInt16Outcome.Truncated, null, 0);
        ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes[1..]);
        return value <= byte.MaxValue
            ? new(CompactPositiveUInt16Outcome.NonCanonicalExtendedValue, null, 0)
            : new(CompactPositiveUInt16Outcome.Accepted, value, 3);
    }
}
