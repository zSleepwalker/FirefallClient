using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using Firefall.Reconstruction.Serialization;

static void Check(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
}

Check(Marshal.SizeOf<SdbNativeMappedTableResult>() == 0x20,
    "Native mapped-table result holder size");
Check(Marshal.OffsetOf<SdbNativeMappedTableResult>(
    nameof(SdbNativeMappedTableResult.CatalogEntryPointer32)).ToInt32() == 0,
    "Native result stores the catalog entry at +0");
Check(Marshal.OffsetOf<SdbNativeMappedTableResult>(
    nameof(SdbNativeMappedTableResult.RowCount)).ToInt32() == 4,
    "Native result stores row count at +4");
Check(Marshal.OffsetOf<SdbNativeMappedTableResult>(
    nameof(SdbNativeMappedTableResult.ExposedRowsPointer32)).ToInt32() == 0x0C,
    "Native result stores exposed rows at +0x0C");
Check(Marshal.OffsetOf<SdbNativeMappedTableResult>(
    nameof(SdbNativeMappedTableResult.RefCountedHandlePointer32)).ToInt32() == 0x18,
    "Native result stores refcounted handle at +0x18");

var mt = new SdbCipher.Mt19937(5489);
Check(mt.Next() == 3499211612u, "MT19937 reference vector #1");
Check(mt.Next() == 581869302u, "MT19937 reference vector #2");
Check(SdbCipher.ComputeNameSeed("example") == 0x137A58C4, "Name-seed reference vector");
Check(SdbCipher.ComputeNameSeed(SdbDatabase.ExpectedBuildIdentifier) == 0xC0A433A5,
    "Production build identifier seed");
byte[] streamVector = new byte[5];
SdbCipher.Apply(streamVector, 5489);
Check(streamVector.SequenceEqual(new byte[] { 0x5C, 0xBB, 0x91, 0xD0, 0xF6 }),
    "Word and trailing-byte stream vector");
byte[] threeTailBytes = new byte[7];
SdbCipher.Apply(threeTailBytes, 5489);
Check(threeTailBytes.SequenceEqual(new byte[] { 0x5C, 0xBB, 0x91, 0xD0, 0xF6, 0xEE, 0x79 }),
    "Each remaining byte uses a fresh MT19937 output");

using var decoded = new MemoryStream();
using (var writer = new BinaryWriter(decoded, Encoding.Latin1, leaveOpen: true))
{
    writer.Write(SdbDatabase.RecordMarker);
    writer.Write((ushort)1);
    writer.Write(0x12345678u);
    writer.Write((ushort)24);
    writer.Write((ushort)1);
    writer.Write((ushort)10);
    writer.Write((byte)11);
    writer.Write(0xAABBCCDDu);
    writer.Write((ushort)12);
    writer.Write((byte)0xFF);
    writer.Write((byte)4);
    writer.Write(37u);
    writer.Write(2u);
    writer.Write(37u);
    writer.Write(Enumerable.Range(0, 48).Select(i => (byte)(i + 1)).ToArray());
}

using var wrapped = new MemoryStream();
using (var writer = new BinaryWriter(wrapped, Encoding.Latin1, leaveOpen: true))
{
    writer.Write(checked((uint)decoded.Length));
    writer.Write(0u);
}
using (var zlib = new ZLibStream(wrapped, CompressionLevel.Optimal, leaveOpen: true))
    zlib.Write(decoded.ToArray());

var encoded = wrapped.ToArray();
SdbCipher.Apply(encoded, SdbCipher.ComputeNameSeed(SdbDatabase.ExpectedBuildIdentifier));
SdbCipher.Apply(encoded, SdbCipher.ComputeNameSeed(SdbDatabase.ExpectedBuildIdentifier));
Check(encoded.SequenceEqual(wrapped.ToArray()), "XOR cipher round-trip");
SdbCipher.Apply(encoded, SdbCipher.ComputeNameSeed(SdbDatabase.ExpectedBuildIdentifier));

using var file = new MemoryStream();
using (var writer = new BinaryWriter(file, Encoding.Latin1, leaveOpen: true))
{
    writer.Write(SdbDatabase.Magic);
    writer.Write(SdbDatabase.Version);
    WriteBlock(writer, SdbDatabase.ExpectedBuildIdentifier, 0, decoded.ToArray());
    WriteBlock(writer, SdbDatabase.ExpectedBuildIdentifier, 0x15, encoded);
}

file.Position = 0;
var block = SdbDatabase.Read(file, modeFlag: true);
Check(block.BuildIdentifier == SdbDatabase.ExpectedBuildIdentifier && block.Flags == 0x15,
    "Block selection and build identifier");
Check(block.DecodedPayload.SequenceEqual(decoded.ToArray()), "Decrypt and inflate");
Check(block.Tables.Count == 1 && block.Tables[0].Fields.Count == 1, "Descriptor count");
Check(block.Tables[0].NameHash == 0x12345678 && block.Tables[0].RowCount == 2, "Table values");
Check(block.Tables[0].Fields[0].NameHash == 0xAABBCCDD, "Field value");
Check(block.Tables[0].RowStride == 24 && block.Tables[0].NullBitmapOffset == 10,
    "Parsed runtime row layout offsets");
Check(block.Tables[0].Fields[0].ValueOffset == 12, "Parsed field value offset");
var parsedRuntimeLayout = SdbRuntimeTableLayout.FromSdbTable(block.Tables[0]);
Check(parsedRuntimeLayout.NameHash == block.Tables[0].NameHash
    && parsedRuntimeLayout.RowStride == block.Tables[0].RowStride
    && parsedRuntimeLayout.Fields[0] == new SdbRuntimeFieldLayout(0xAABBCCDD, 0xFF, 12, 4),
    "Parsed descriptors expand into compared runtime layout fields");
Check(block.GetRawRow(0, 0).SequenceEqual(Enumerable.Range(1, 24).Select(i => (byte)i)),
    "First raw row follows the decoded offset");
Check(block.GetRawRow(0, 1).SequenceEqual(Enumerable.Range(25, 24).Select(i => (byte)i)),
    "Second raw row starts one row stride later");
var detachedRow = block.GetRawRow(0, 0);
detachedRow[0] = 0;
Check(block.GetRawRow(0, 0)[0] == 1, "Raw row accessor returns a copy");
Check(block.GetRawField(0, 0, 0)!.SequenceEqual(Enumerable.Range(13, 8).Select(i => (byte)i)),
    "Raw field bytes follow field offset and storage width");
CheckThrows<ArgumentOutOfRangeException>(() => block.GetRawRow(1, 0),
    "Unknown raw row table rejected");
CheckThrows<ArgumentOutOfRangeException>(() => block.GetRawRow(0, 2),
    "Out-of-range raw row rejected");
CheckThrows<ArgumentOutOfRangeException>(() => block.GetRawField(0, 0, 1),
    "Unknown raw field rejected");
var nullableFieldPayload = decoded.ToArray();
nullableFieldPayload[23] = 2;
nullableFieldPayload[71] |= 4;
var nullableFields = ReadPlainPayload(nullableFieldPayload);
Check(nullableFields.GetRawField(0, 0, 0) is not null
    && nullableFields.GetRawField(0, 1, 0) is null,
    "Only row with its field null bit set returns null");
var unknownTypePayload = decoded.ToArray();
unknownTypePayload[24] = 27;
Check(SdbRuntimeTableLayout.FromSdbTable(ReadPlainPayload(unknownTypePayload).Tables[0])
    .Fields[0].TypeId == 0, "Native runtime clamps out-of-range field types to zero");
CheckThrows<NotSupportedException>(
    () => ReadPlainPayload(unknownTypePayload).GetRawField(0, 0, 0),
    "Unknown field type cannot be sliced by a guessed width");
var nullUnknownTypePayload = nullableFieldPayload.ToArray();
nullUnknownTypePayload[24] = 27;
Check(ReadPlainPayload(nullUnknownTypePayload).GetRawField(0, 1, 0) is null,
    "Null fields do not require a supported value type");
var invalidFieldOffset = decoded.ToArray();
invalidFieldOffset[21] = 23;
CheckThrows<InvalidDataException>(
    () => ReadPlainPayload(invalidFieldOffset).GetRawField(0, 0, 0),
    "Field bytes cannot extend beyond the row");
var invalidNullOffset = nullableFieldPayload.ToArray();
invalidNullOffset[14] = 255;
CheckThrows<InvalidDataException>(
    () => ReadPlainPayload(invalidNullOffset).GetRawField(0, 0, 0),
    "Null bit cannot extend beyond the row");
Check(block.Tables[0].Fields[0].TypeInfo?.Label == "U64", "Field type lookup");
Check(SdbFieldTypes.Lookup(15)?.StorageBytes == 64, "Matrix storage size");
Check(SdbFieldTypes.Lookup(26)?.StorageBytes == 2, "Half-float storage size");
Check(SdbFieldTypes.Lookup(27) is null, "Unknown field type");
var runtimeFields = new[]
{
    new SdbRuntimeFieldLayout(0x1111, 0xFF, 4, 3),
    new SdbRuntimeFieldLayout(0x2222, 9, 8, 9)
};
var runtimeLayout = new SdbRuntimeTableLayout(0xABC, 16, runtimeFields);
Check(SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    runtimeLayout, new(0xABC, 16, runtimeFields)), "Matching runtime layout");
Check(SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    new(0xABC, 0, []), new(0xABC, 0, [])), "Empty runtime layouts match");
Check(SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    runtimeLayout, new(0xABC, 16, [runtimeFields[0] with { NullBitIndex = 2 }, runtimeFields[1]])),
    "Runtime layout comparison ignores null-bit index");
Check(!SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    runtimeLayout, new(0xABD, 16, runtimeFields)), "Different table hash");
Check(!SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    runtimeLayout, new(0xABC, 20, runtimeFields)), "Different row stride");
Check(!SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    runtimeLayout, new(0xABC, 16, [runtimeFields[0]])), "Different field count");
Check(!SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    runtimeLayout, new(0xABC, 16, [runtimeFields[1], runtimeFields[0]])),
    "Field order is significant");
Check(!SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    runtimeLayout, new(0xABC, 16, [runtimeFields[0] with { TypeId = 10 }, runtimeFields[1]])),
    "Different field type");
Check(!SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    runtimeLayout, new(0xABC, 16, [runtimeFields[0] with { ValueOffset = 5 }, runtimeFields[1]])),
    "Different field value offset");
Check(!SdbRuntimeTableLayout.HasIdenticalStorageLayout(
    runtimeLayout, new(0xABC, 16, [runtimeFields[0] with { NameHash = 0x9999 }, runtimeFields[1]])),
    "Different field name hash");
Check(SdbRuntimeTableLayout.MapSourceFieldsByHash(
    runtimeLayout, new(0xDEF, 20,
    [
        new(0x2222, 0xFF, 0, 9),
        new(0x3333, 0xFF, 8, 1),
        new(0x1111, 0xFF, 12, 3)
    ])).SequenceEqual(new[] { 1, -1, 0 }), "Destination fields map to source hash indexes");
Check(SdbRuntimeTableLayout.MapSourceFieldsByHash(
    new(0, 4, [runtimeFields[0], runtimeFields[0]]),
    new(0, 4, [runtimeFields[0]])).SequenceEqual(new[] { 0 }),
    "Duplicate source hashes select the first field");
Check(SdbRuntimeTableLayout.MapSourceFieldsByHash(
    new(0, 0, []), new(0, 0, [])).Length == 0, "Empty field map");
var nullableSource = ReadPlainPayload(nullableFieldPayload);
var destinationRuntimeLayout = new SdbRuntimeTableLayout(
    0x12345678, 12,
    [
        new(0xAABBCCDD, 1, 0, 3),
        new(0x99999999, 0xFF, 4, 1)
    ],
    NullBitmapOffset: 10);
var convertedRows = SdbRowConverter.ConvertRows(nullableSource, 0, destinationRuntimeLayout);
Check(convertedRows.Count == 2, "Row converter retains source row count");
Check(convertedRows[0].AsSpan(0, 4).SequenceEqual(new byte[] { 13, 14, 15, 16 }),
    "Row converter maps and narrows matching field");
Check(convertedRows[0].AsSpan(4, 4).SequenceEqual(new byte[4]),
    "Unmapped destination field remains zero initialized");
Check(convertedRows[0][10] == 0, "Present source leaves destination null bit clear");
Check(convertedRows[1].AsSpan(0, 4).SequenceEqual(new byte[4])
    && convertedRows[1][10] == 2,
    "Null source sets destination null bit and leaves value zero");
CheckThrows<ArgumentOutOfRangeException>(
    () => SdbRowConverter.ConvertRows(nullableSource, 1, destinationRuntimeLayout),
    "Row converter rejects unknown source table");
CheckThrows<ArgumentException>(
    () => SdbRowConverter.ConvertRows(nullableSource, 0, destinationRuntimeLayout with { NameHash = 0 }),
    "Row converter rejects mismatched table hashes");
CheckThrows<NotSupportedException>(
    () => SdbRowConverter.ConvertRows(nullableSource, 0,
        destinationRuntimeLayout with
        {
            Fields = [destinationRuntimeLayout.Fields[0] with { TypeId = 257 }]
        }),
    "Out-of-byte-range destination type does not wrap");
Check(!SdbRuntimeTableLayout.IsSourceFieldNull([], uint.MaxValue, 0xFF),
    "No-null-bit sentinel does not read row bytes");
Check(SdbRuntimeTableLayout.IsSourceFieldNull([0, 0, 0b00000010], 1, 9),
    "Null index 9 reads bit 1 from second bitmap byte");
Check(!SdbRuntimeTableLayout.IsSourceFieldNull([0, 0, 0b00000010], 1, 8),
    "Clear bitmap bit indicates present value");
CheckThrows<ArgumentException>(
    () => SdbRuntimeTableLayout.IsSourceFieldNull(new byte[1], 1, 0),
    "Truncated null bitmap rejected");
byte[] destinationRow = [0, 0, 0b00000100];
SdbRuntimeTableLayout.MarkDestinationFieldNull(destinationRow, 1, 9);
Check(destinationRow.SequenceEqual(new byte[] { 0, 0, 0b00000110 }),
    "Destination null bit set without clearing existing bits");
SdbRuntimeTableLayout.MarkDestinationFieldNull(destinationRow, uint.MaxValue, 0xFF);
Check(destinationRow[2] == 0b00000110, "Destination null sentinel skips bitmap");
CheckThrows<ArgumentException>(
    () => SdbRuntimeTableLayout.MarkDestinationFieldNull(new byte[1], 1, 0),
    "Truncated destination null bitmap rejected");
byte[] converted = new byte[8];
SdbFieldConversion.Convert(new byte[] { 0xFF }, 5, converted, 8);
Check(converted.SequenceEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }),
    "Signed byte extends to signed 64-bit");
SdbFieldConversion.Convert(new byte[] { 0xFF }, 1, converted, 4);
Check(converted.SequenceEqual(new byte[] { 0xFF, 0, 0, 0, 0, 0, 0, 0 }),
    "Unsigned byte extends to unsigned 64-bit");
SdbFieldConversion.Convert(new byte[] { 0x34, 0x12, 0xAB, 0xCD }, 3, converted, 6);
Check(converted[0] == 0x34 && converted[1] == 0x12,
    "Unsigned 32-bit narrows to signed 16-bit by truncation");
SdbFieldConversion.Convert(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 3, converted, 8);
Check(converted.SequenceEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0 }),
    "Unsigned 32-bit zero-extends to 64-bit");
SdbFieldConversion.Convert(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 7, converted, 4);
Check(converted.SequenceEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }),
    "Signed 32-bit sign-extends to unsigned 64-bit storage");
SdbFieldConversion.Convert(new byte[] { 0x78, 0x56, 0x34, 0x12, 0xFF, 0xFF, 0xFF, 0xFF },
    8, converted, 1);
Check(converted[0] == 0x78, "Signed 64-bit narrows to low byte");
var exactFloat = new byte[] { 0, 0, 0xC0, 0x7F };
SdbFieldConversion.Convert(exactFloat, 9, converted, 9);
Check(converted.AsSpan(0, 4).SequenceEqual(exactFloat), "Matching types retain float bits");
SdbFieldConversion.Convert(BitConverter.GetBytes(1.5f), 9, converted, 10);
Check(converted.SequenceEqual(BitConverter.GetBytes(1.5)), "F32 to F64 preserves finite value");
SdbFieldConversion.Convert(BitConverter.GetBytes(double.MaxValue), 10, converted, 9);
Check(float.IsPositiveInfinity(BitConverter.ToSingle(converted, 0)),
    "F64 to F32 overflows to positive infinity");
SdbFieldConversion.Convert(BitConverter.GetBytes(-0.0), 10, converted, 9);
Check(converted.AsSpan(0, 4).SequenceEqual(BitConverter.GetBytes(-0.0f)),
    "F64 to F32 retains negative zero");
foreach (var (half, singleBits) in new (ushort Half, int Bits)[]
{
    (0x0000, 0x00000000), (0x8000, unchecked((int)0x80000000)),
    (0x0001, 0x33800000), (0x03BF, 0x386FC000),
    (0x3C00, 0x3F800000), (0x3C01, 0x3F802000),
    (0x7BFF, 0x477FE000), (0x7C00, 0x7F800000),
    (0x7E00, 0x7FC00000), (0xFFFF, unchecked((int)0xFFFFE000))
})
{
    SdbFieldConversion.Convert(BitConverter.GetBytes(half), 26, converted, 9);
    Check(converted.AsSpan(0, 4).SequenceEqual(BitConverter.GetBytes(singleBits)),
        $"F16 0x{half:X4} expands to observed lookup value");
}
SdbFieldConversion.Convert(BitConverter.GetBytes((ushort)0xBC00), 26, converted, 10);
Check(converted.SequenceEqual(BitConverter.GetBytes(-1.0)), "F16 to F64 widens via F32");
foreach (var (value, half) in new (float Value, ushort Bits)[]
{
    (0f, 0x0000), (-0f, 0x0000), (1f, 0x3C00), (-1f, 0xBC00),
    (1.00048828125f, 0x3C01), (1.99951171875f, 0x4000),
    (BitConverter.UInt32BitsToSingle(0x33800000), 0x0001),
    (65504f, 0x7BFF), (float.PositiveInfinity, 0x7C00),
    (float.NegativeInfinity, 0xFC00),
    (BitConverter.UInt32BitsToSingle(0x7F800001), 0x7C01)
})
{
    SdbFieldConversion.Convert(BitConverter.GetBytes(value), 9, converted, 26);
    Check(converted.AsSpan(0, 2).SequenceEqual(BitConverter.GetBytes(half)),
        $"F32 {value} packs with observed half-float rules");
}
SdbFieldConversion.Convert(BitConverter.GetBytes(1.5), 10, converted, 26);
Check(converted.AsSpan(0, 2).SequenceEqual(BitConverter.GetBytes((ushort)0x3E00)),
    "F64 to F16 narrows through F32");
foreach (var (input, type, expected) in new (byte[] Bytes, byte Type, double Value)[]
{
    (new byte[] { 0xFF }, 1, 255d),
    (new byte[] { 0xFF }, 5, -1d),
    (BitConverter.GetBytes((ushort)65535), 2, 65535d),
    (BitConverter.GetBytes((short)-32768), 6, -32768d),
    (BitConverter.GetBytes(uint.MaxValue), 3, 4294967295d),
    (BitConverter.GetBytes(int.MinValue), 7, -2147483648d),
    (BitConverter.GetBytes(ulong.MaxValue), 4, 18446744073709551616d),
    (BitConverter.GetBytes(long.MinValue), 8, -9223372036854775808d)
})
{
    SdbFieldConversion.Convert(input, type, converted, 10);
    Check(converted.SequenceEqual(BitConverter.GetBytes(expected)),
        $"Integer type {type} converts to F64");
    SdbFieldConversion.Convert(input, type, converted, 9);
    Check(converted.AsSpan(0, 4).SequenceEqual(BitConverter.GetBytes((float)expected)),
        $"Integer type {type} converts to F32");
}
SdbFieldConversion.Convert(BitConverter.GetBytes(uint.MaxValue), 3, converted, 26);
Check(converted.AsSpan(0, 2).SequenceEqual(BitConverter.GetBytes((ushort)0x7C00)),
    "Unsigned 32-bit to F16 overflows to infinity");
SdbFieldConversion.Convert(new byte[] { 0xFF }, 5, converted, 26);
Check(converted.AsSpan(0, 2).SequenceEqual(BitConverter.GetBytes((ushort)0xBC00)),
    "Signed byte to F16 preserves sign");
SdbFieldConversion.Convert(BitConverter.GetBytes(ulong.MaxValue), 4, converted, 26);
Check(converted.AsSpan(0, 2).SequenceEqual(BitConverter.GetBytes((ushort)0x7C00)),
    "Unsigned 64-bit conversion reaches F16 infinity");
SdbFieldConversion.Convert(BitConverter.GetBytes(-258.75f), 9, converted, 7);
Check(converted.AsSpan(0, 4).SequenceEqual(BitConverter.GetBytes(-258)),
    "F32 to signed 32-bit truncates toward zero");
SdbFieldConversion.Convert(BitConverter.GetBytes(257.9), 10, converted, 1);
Check(converted[0] == 1, "F64 to unsigned byte retains low byte");
SdbFieldConversion.Convert(BitConverter.GetBytes((ushort)0xC200), 26, converted, 6);
Check(converted.AsSpan(0, 2).SequenceEqual(BitConverter.GetBytes((short)-3)),
    "F16 to signed 16-bit uses expanded float value");
SdbFieldConversion.Convert(BitConverter.GetBytes(double.NaN), 10, converted, 7);
Check(converted.AsSpan(0, 4).SequenceEqual(BitConverter.GetBytes(int.MinValue)),
    "SSE invalid conversion returns indefinite signed 32-bit");
SdbFieldConversion.Convert(BitConverter.GetBytes(float.PositiveInfinity), 9, converted, 2);
Check(converted.AsSpan(0, 2).SequenceEqual(new byte[2]),
    "Indefinite signed result narrows to zero low 16 bits");
SdbFieldConversion.Convert(BitConverter.GetBytes(2147483648d), 10, converted, 7);
Check(converted.AsSpan(0, 4).SequenceEqual(BitConverter.GetBytes(int.MinValue)),
    "F64 conversion above signed 32-bit range returns indefinite value");
foreach (var (value, type, expected) in new (double Value, byte Type, ulong Bits)[]
{
    (4294967295d, 3, uint.MaxValue),
    (-1.9, 3, uint.MaxValue),
    (9223372036854775808d, 3, 0),
    (-9223372036854775808d, 3, 0),
    (double.NaN, 3, 0),
    (9223372036854775808d, 4, 0x8000000000000000),
    (18446744073709549568d, 4, 18446744073709549568UL),
    (18446744073709551616d, 4, 0x8000000000000000),
    (-1.9, 4, ulong.MaxValue),
    (double.NaN, 4, 0x8000000000000000),
    (-9223372036854775808d, 8, 0x8000000000000000),
    (9223372036854775808d, 8, 0x8000000000000000),
    (-18446744073709551616d, 8, 0x8000000000000000),
    (double.PositiveInfinity, 8, 0x8000000000000000),
    (123456789.9, 8, 123456789)
})
{
    byte[] result = new byte[type == 3 ? 4 : 8];
    SdbFieldConversion.Convert(BitConverter.GetBytes(value), 10, result, type);
    Check((type == 3 ? BitConverter.ToUInt32(result) : BitConverter.ToUInt64(result)) == expected,
        $"F64 {value} to integer type {type}");
}
SdbFieldConversion.Convert(BitConverter.GetBytes(-1f), 9, converted, 4);
Check(BitConverter.ToUInt64(converted) == ulong.MaxValue, "F32 negative to U64 wraps");
SdbFieldConversion.Convert(BitConverter.GetBytes((ushort)0x3E00), 26, converted, 3);
Check(BitConverter.ToUInt32(converted) == 1u, "F16 1.5 to U32 truncates");
SdbFieldConversion.Convert(BitConverter.GetBytes(float.NaN), 9, converted, 4);
Check(BitConverter.ToUInt64(converted) == 0x8000000000000000,
    "F32 NaN to U64 produces native invalid result");
CheckThrows<NotSupportedException>(
    () => SdbFieldConversion.Convert(new byte[4], 9, new byte[8], 12),
    "Unsupported float to compound conversion is explicit");
CheckThrows<ArgumentException>(
    () => SdbFieldConversion.Convert(new byte[1], 3, converted, 8),
    "Truncated source rejected");
CheckThrows<ArgumentException>(
    () => SdbFieldConversion.Convert(new byte[4], 3, new byte[1], 8),
    "Truncated destination rejected");
Check(block.TrailingDataOffset == 37, "Trailing offset");

file.Position = 0;
var plain = SdbDatabase.Read(file, modeFlag: false);
Check(plain.BuildIdentifier == SdbDatabase.ExpectedBuildIdentifier && plain.Flags == 0,
    "Plain block selection");

file.Position = 0;
var truncated = file.ToArray()[..^1];
CheckThrows<InvalidDataException>(() => SdbDatabase.Read(new MemoryStream(truncated), true),
    "Truncated payload");
CheckThrows<InvalidDataException>(() => SdbDatabase.Read(new MemoryStream([0, 1, 2, 3]), true),
    "Truncated header");
var outOfBoundsRows = decoded.ToArray();
outOfBoundsRows[25] = 0xFF;
CheckThrows<InvalidDataException>(() => ReadPlainPayload(outOfBoundsRows),
    "Row range outside decoded block rejected");
var zeroStrideRows = decoded.ToArray();
zeroStrideRows[10] = 0;
CheckThrows<InvalidDataException>(() => ReadPlainPayload(zeroStrideRows),
    "Nonempty rows require a stride");
var overflowingRows = decoded.ToArray();
Array.Fill(overflowingRows, (byte)0xFF, 29, 4);
CheckThrows<InvalidDataException>(() => ReadPlainPayload(overflowingRows),
    "Large row count cannot wrap the decoded range");
var emptyRows = zeroStrideRows.ToArray();
Array.Clear(emptyRows, 29, 4);
Check(ReadPlainPayload(emptyRows).Tables[0].RowCount == 0,
    "Empty table does not require a row stride");
using var wrongBuild = new MemoryStream();
using (var writer = new BinaryWriter(wrongBuild, Encoding.Latin1, leaveOpen: true))
{
    writer.Write(SdbDatabase.Magic);
    writer.Write(SdbDatabase.Version);
    WriteBlock(writer, "other-build", 0, decoded.ToArray());
}
wrongBuild.Position = 0;
CheckThrows<InvalidDataException>(() => SdbDatabase.Read(wrongBuild, false),
    "Incorrect build identifier");
using var wrongEndian = new MemoryStream();
using (var writer = new BinaryWriter(wrongEndian, Encoding.Latin1, leaveOpen: true))
{
    writer.Write(SdbDatabase.Magic);
    writer.Write(SdbDatabase.Version);
    WriteBlock(writer, SdbDatabase.ExpectedBuildIdentifier, 2, decoded.ToArray());
}
wrongEndian.Position = 0;
CheckThrows<NotSupportedException>(() => SdbDatabase.Read(wrongEndian, false),
    "Wrong endian flag");
Console.WriteLine("Serialization checks passed.");

static void CheckThrows<T>(Action action, string message) where T : Exception
{
    try
    {
        action();
    }
    catch (T)
    {
        return;
    }
    throw new Exception(message);
}

static void WriteBlock(BinaryWriter writer, string name, uint flags, byte[] bytes)
{
    writer.Write(checked((uint)bytes.Length));
    writer.Write(flags);
    writer.Write(0u);
    writer.Write(0u);
    writer.Write(Encoding.Latin1.GetBytes(name));
    writer.Write((byte)0);
    while (writer.BaseStream.Position % 128 != 0)
        writer.Write((byte)0);
    writer.Write(bytes);
}

static SdbBlock ReadPlainPayload(byte[] payload)
{
    using var stream = new MemoryStream();
    using (var writer = new BinaryWriter(stream, Encoding.Latin1, leaveOpen: true))
    {
        writer.Write(SdbDatabase.Magic);
        writer.Write(SdbDatabase.Version);
        WriteBlock(writer, SdbDatabase.ExpectedBuildIdentifier, 0, payload);
    }
    stream.Position = 0;
    return SdbDatabase.Read(stream, false);
}
