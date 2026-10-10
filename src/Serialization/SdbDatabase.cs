using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Firefall.Reconstruction.Serialization;

public sealed record SdbField(uint NameHash, ushort ValueOffset, byte NullBit, byte Type)
{
    public SdbTypeInfo? TypeInfo => SdbFieldTypes.Lookup(Type);
}

public sealed record SdbTable(
    uint NameHash,
    ushort RowStride,
    ushort NullBitmapOffset,
    byte UnknownC,
    IReadOnlyList<SdbField> Fields,
    uint RowOffset,
    uint RowCount);

public sealed record SdbBlock(
    string BuildIdentifier,
    uint Flags,
    byte[] DecodedPayload,
    IReadOnlyList<SdbTable> Tables,
    uint TrailingDataOffset)
{
    public byte[] GetRawRow(int tableIndex, uint rowIndex)
    {
        if ((uint)tableIndex >= (uint)Tables.Count)
            throw new ArgumentOutOfRangeException(nameof(tableIndex));
        var table = Tables[tableIndex];
        if (rowIndex >= table.RowCount)
            throw new ArgumentOutOfRangeException(nameof(rowIndex));

        ulong offset = (ulong)table.RowOffset + (ulong)rowIndex * table.RowStride;
        if (offset + table.RowStride > (ulong)DecodedPayload.Length)
            throw new InvalidDataException("SDB row extends beyond the decoded block.");
        return DecodedPayload.AsSpan((int)offset, table.RowStride).ToArray();
    }

    public byte[]? GetRawField(int tableIndex, uint rowIndex, int fieldIndex)
    {
        if ((uint)tableIndex >= (uint)Tables.Count)
            throw new ArgumentOutOfRangeException(nameof(tableIndex));
        var table = Tables[tableIndex];
        if ((uint)fieldIndex >= (uint)table.Fields.Count)
            throw new ArgumentOutOfRangeException(nameof(fieldIndex));

        var field = table.Fields[fieldIndex];
        byte[] row = GetRawRow(tableIndex, rowIndex);
        if (field.NullBit != 0xFF
            && (ulong)table.NullBitmapOffset + (uint)(field.NullBit >> 3) >= (ulong)row.Length)
            throw new InvalidDataException("SDB field null bit exceeds its row.");
        if (SdbRuntimeTableLayout.IsSourceFieldNull(row, table.NullBitmapOffset, field.NullBit))
            return null;
        var type = field.TypeInfo
            ?? throw new NotSupportedException($"Unknown SDB field type {field.Type}.");
        if ((ulong)field.ValueOffset + (uint)type.StorageBytes > (ulong)row.Length)
            throw new InvalidDataException("SDB field value exceeds its row.");
        return row.AsSpan(field.ValueOffset, type.StorageBytes).ToArray();
    }
}

/// <summary>
/// Reconstructs the SDB envelope and schema reader, not the engine's row-to-object conversion.
/// Offsets cited in the documentation refer to offsets in the raw FirefallClient.exe image.
/// </summary>
public static class SdbDatabase
{
    public const uint Magic = 0xDA7ABA5E;
    public const uint Version = 12;
    public const uint RecordMarker = 1002;
    public const string ExpectedBuildIdentifier = "prod-1962";
    public const int MaximumDecodedLength = 0x10000000;

    // Sdb_LoadDatabaseBlock at 0x1310060; this takes the first block whose
    // 0x10 flag agrees with the object's mode, rather than searching by name.
    public static SdbBlock Read(Stream input, bool modeFlag)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.CanRead)
            throw new ArgumentException("The SDB input must be readable.", nameof(input));
        using var source = new MemoryStream();
        input.CopyTo(source);
        var data = source.ToArray();
        var cursor = new SdbCursor(data);
        if (cursor.ReadU32() != Magic || cursor.ReadU32() != Version)
            throw new InvalidDataException("Invalid SDB magic or version.");

        while (cursor.Remaining > 0)
        {
            var length = cursor.ReadU32();
            var flags = cursor.ReadU32();
            cursor.Skip(8); // Two header words are read but not interpreted by the loader.
            var name = cursor.ReadNullTerminatedAscii();
            cursor.Align(128);
            if (length > cursor.Remaining)
                throw new InvalidDataException("SDB block extends beyond the input.");

            if (((flags & 0x10) != 0) != modeFlag)
            {
                cursor.Skip(checked((int)length));
                continue;
            }
            if (name != ExpectedBuildIdentifier)
                throw new InvalidDataException(
                    $"SDB build identifier '{name}' differs from '{ExpectedBuildIdentifier}'.");

            var payload = cursor.ReadBytes(checked((int)length));
            if ((flags & 1) != 0)
                SdbCipher.Apply(payload, SdbCipher.ComputeNameSeed(name));
            if ((flags & 2) != 0)
                throw new NotSupportedException("SDB block has the wrong endian type (flag 0x02).");
            if ((flags & 4) != 0)
                payload = Inflate(payload);
            return ParseBlock(name, flags, payload, modeFlag);
        }

        throw new InvalidDataException("No SDB block matches the requested mode.");
    }

    // Sdb_ParseDecodedBlock at 0x13106F0.
    private static SdbBlock ParseBlock(string buildIdentifier, uint flags, byte[] payload, bool modeFlag)
    {
        var cursor = new SdbCursor(payload);
        if (cursor.ReadU32() != RecordMarker)
            throw new InvalidDataException("Invalid SDB record marker.");
        if (((flags & 0x10) != 0) != modeFlag)
            throw new InvalidDataException("SDB block mode mismatch.");

        var count = cursor.ReadU16();
        var headers = new (uint NameHash, ushort RowStride, ushort NullBitmapOffset, byte UnknownC, ushort FieldCount)[count];
        for (var i = 0; i < count; i++)
        {
            var hash = cursor.ReadU32();
            var a = cursor.ReadU16();
            var fieldCount = cursor.ReadU16();
            var b = cursor.ReadU16();
            var c = cursor.ReadU8();
            headers[i] = (hash, a, b, c, fieldCount);
        }

        var fields = new SdbField[count][];
        for (var i = 0; i < count; i++)
        {
            fields[i] = new SdbField[headers[i].FieldCount];
            for (var j = 0; j < fields[i].Length; j++)
                fields[i][j] = new SdbField(cursor.ReadU32(), cursor.ReadU16(), cursor.ReadU8(), cursor.ReadU8());
        }

        var tables = new SdbTable[count];
        uint lastOffset = 0;
        for (var i = 0; i < count; i++)
        {
            var offset = cursor.ReadU32();
            if (offset < lastOffset)
                throw new InvalidDataException("SDB table row offsets are not sorted.");
            lastOffset = offset;
            var rowCount = cursor.ReadU32();
            if (rowCount != 0 && (headers[i].RowStride == 0
                || (ulong)offset + (ulong)rowCount * headers[i].RowStride > (ulong)payload.Length))
                throw new InvalidDataException("SDB table rows exceed the decoded block.");
            tables[i] = new SdbTable(
                headers[i].NameHash, headers[i].RowStride, headers[i].NullBitmapOffset,
                headers[i].UnknownC, fields[i], offset, rowCount);
        }

        var trailingOffset = cursor.ReadU32();
        if (trailingOffset > payload.Length)
            throw new InvalidDataException("SDB trailing data offset exceeds the decoded block.");
        return new SdbBlock(buildIdentifier, flags, payload, tables, trailingOffset);
    }

    // Sdb_InflatePayload at 0x13172C0: u32 size, u32 reserved-zero, zlib data.
    private static byte[] Inflate(byte[] payload)
    {
        if (payload.Length < 8)
            throw new InvalidDataException("Truncated SDB compressed payload.");
        var size = BinaryPrimitives.ReadUInt32LittleEndian(payload);
        if (BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(4)) != 0 ||
            size > MaximumDecodedLength)
            throw new InvalidDataException("Invalid SDB compressed payload header.");
        using var compressed = new MemoryStream(payload, 8, payload.Length - 8);
        using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);
        using var decoded = new MemoryStream();
        var buffer = new byte[16384];
        int read;
        while ((read = inflater.Read(buffer)) != 0)
        {
            if (decoded.Length + read > MaximumDecodedLength)
                throw new InvalidDataException("SDB inflated payload exceeds the loader limit.");
            decoded.Write(buffer, 0, read);
        }
        if (decoded.Length != size)
            throw new InvalidDataException("SDB inflated payload length differs from its declared length.");
        return decoded.ToArray();
    }

    private sealed class SdbCursor(byte[] data)
    {
        private int position;
        public int Remaining => data.Length - position;

        public byte ReadU8() => ReadBytes(1)[0];
        public ushort ReadU16() => BinaryPrimitives.ReadUInt16LittleEndian(ReadBytes(2));
        public uint ReadU32() => BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(4));

        public byte[] ReadBytes(int length)
        {
            if (length < 0 || length > Remaining)
                throw new InvalidDataException("Truncated SDB data.");
            var bytes = data.AsSpan(position, length).ToArray();
            position += length;
            return bytes;
        }

        public void Skip(int length)
        {
            if (length < 0 || length > Remaining)
                throw new InvalidDataException("Truncated SDB data.");
            position += length;
        }

        public string ReadNullTerminatedAscii()
        {
            var end = Array.IndexOf(data, (byte)0, position);
            if (end < 0)
                throw new InvalidDataException("Unterminated SDB block name.");
            var name = Encoding.Latin1.GetString(data, position, end - position);
            position = end + 1;
            return name;
        }

        public void Align(int boundary)
        {
            var padding = (boundary - position % boundary) % boundary;
            Skip(padding);
        }
    }
}
