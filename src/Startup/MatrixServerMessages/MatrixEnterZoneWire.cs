namespace Firefall.Reconstruction.Startup;

public sealed record MatrixEnterZoneZoneFields(
    ulong InstanceId, uint ZoneId, ulong ValueAt38, byte PreviewFlags,
    ReadOnlyMemory<byte> TextBytesAt44, ushort WordAt48, uint WordAt4C, byte ByteAt50,
    ulong MatchId, byte ByteAt60, uint WordAt64, ReadOnlyMemory<byte> ZoneNameBytes)
{
    public MatrixEnterZoneObservedFields ObservedFields => new(InstanceId, ZoneId, MatchId, PreviewFlags);
}

public sealed record MatrixEnterZonePayload(
    MatrixEnterZoneZoneFields Zone, ReadOnlyMemory<byte> RawFieldsAt80,
    bool NormalizedFlagAtB8, byte SpectatorByteAtC0, MatrixEnterZoneMetadata? NestedMetadata)
{
    public bool ConnectAsSpectator => SpectatorByteAtC0 != 0;
    public MatrixClockParameters ClockParameters =>
        MatrixClockParameters.FromEnterZoneRawFields(RawFieldsAt80.Span, NormalizedFlagAtB8);
}

public enum MatrixEnterZoneReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload,
    UnsupportedNestedMetadata
}

public sealed record MatrixEnterZoneReadResult(
    MatrixEnterZoneReadOutcome Outcome, MatrixEnterZonePayload? Payload, int BytesConsumed);

/// <summary>
/// Normal-buffer msg-37 decoding with absent or present optional nested metadata.
/// Text stays as bytes; descriptor padding and native objects are not synthesized.
/// </summary>
public static class MatrixEnterZoneWire
{
    public const uint FactoryCaseFileOffset = 0x33D629;
    public const uint ConstructorFileOffset = 0x339EA0;
    public const int DescriptorAllocationSize = 0xC8;
    public const uint ReadBodyMethodFileOffset = 0x356E60;
    public const uint ReadZoneFieldsMethodFileOffset = 0x356770;
    public const uint ReadOptionalMetadataMethodFileOffset = 0x3565C0;
    public const uint ReadMetadataWord16MapFileOffset = 0x355140;
    public const uint ReadMetadataWord32MapFileOffset = 0x355E50;
    public const uint CopyOptionalMetadataFileOffset = 0x3563E0;
    public const uint ReadSuffixQwordsAndBooleanFileOffset = 0x356B70;
    public const int MinimumPacketSizeWithoutNestedMetadata = 103;
    public const int MinimumPacketSizeWithEmptyNestedMetadata = 105;
    public const int RawFieldByteCountAt80 = 56;

    public static MatrixEnterZoneReadResult Decode(ReadOnlySpan<byte> bytes) =>
        DecodeCore(bytes, allowNestedMetadata: true);

    /// <summary>Retains the earlier restricted decoder's explicit rejection of present metadata.</summary>
    public static MatrixEnterZoneReadResult DecodeWithoutNestedMetadata(ReadOnlySpan<byte> bytes) =>
        DecodeCore(bytes, allowNestedMetadata: false);

    private static MatrixEnterZoneReadResult DecodeCore(ReadOnlySpan<byte> bytes, bool allowNestedMetadata)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixEnterZoneReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixEnterZoneReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value != MatrixEnterZoneNativeLayout.DecodedMessageId)
            return new(MatrixEnterZoneReadOutcome.UnexpectedMessageCode, null, 0);

        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        if (!reader.TryReadUInt64(out var instanceId)
            || !reader.TryReadUInt32(out var zoneId)
            || !reader.TryReadUInt64(out var valueAt38)
            || !reader.TryReadByte(out var previewFlags)
            || !reader.TryReadNullTerminatedBytes(out var textAt44)
            || !reader.TryReadUInt16(out var wordAt48)
            || !reader.TryReadUInt32(out var wordAt4C)
            || !reader.TryReadByte(out var byteAt50)
            || !reader.TryReadUInt64(out var matchId)
            || !reader.TryReadByte(out var byteAt60)
            || !reader.TryReadUInt32(out var wordAt64)
            || !reader.TryReadNullTerminatedBytes(out var zoneName)
            || !reader.TryReadByte(out var metadataPresent))
            return new(MatrixEnterZoneReadOutcome.TruncatedPayload, null, 0);
        if (metadataPresent != 0 && !allowNestedMetadata)
            return new(MatrixEnterZoneReadOutcome.UnsupportedNestedMetadata, null, 0);

        MatrixEnterZoneMetadata? metadata = null;
        if (metadataPresent != 0 && !MatrixEnterZoneMetadataWire.TryRead(ref reader, out metadata))
            return new(MatrixEnterZoneReadOutcome.TruncatedPayload, null, 0);

        if (!reader.TryReadBytes(RawFieldByteCountAt80, out var rawFields)
            || !reader.TryReadByte(out var normalizedFlag)
            || !reader.TryReadByte(out var spectatorByte))
            return new(MatrixEnterZoneReadOutcome.TruncatedPayload, null, 0);

        var zone = new MatrixEnterZoneZoneFields(instanceId, zoneId, valueAt38, previewFlags,
            textAt44, wordAt48, wordAt4C, byteAt50, matchId, byteAt60, wordAt64, zoneName);
        var payload = new MatrixEnterZonePayload(
            zone, rawFields.ToArray(), normalizedFlag != 0, spectatorByte, metadata);
        return new(MatrixEnterZoneReadOutcome.Accepted, payload, header.BytesConsumed + reader.Position);
    }
}
