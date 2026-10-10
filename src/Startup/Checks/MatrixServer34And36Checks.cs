using Firefall.Reconstruction.Startup;

internal static class MatrixServer34And36Checks
{
    public static void Run(Action<string, bool> check)
    {
        var buffer = Convert.FromHexString("220400FF008001FE");
        var announcement = new byte[] { 36 }.Concat("sysmsg"u8.ToArray())
            .Concat(new byte[] { 0, 0xFF, 0x80, 0 }).ToArray();
        check("Server34 is WORD-length blob plus raw flag;36 is two NUL texts",
            MatrixServerMessages34And36Wire.Decode(buffer)
                is { BytesConsumed: 8, Payload: MatrixBufferSnapshotPayload { RawFlagAt38: 0xFE } b }
                && b.BlobBytesAt28.Span.SequenceEqual(new byte[] { 0xFF, 0, 0x80, 1 })
            && MatrixServerMessages34And36Wire.Decode(announcement)
                is { Payload: MatrixAnnouncementPayload a }
                && a.TypeBytesAt28.Span.SequenceEqual("sysmsg"u8)
                && a.DataBytesAt2C.Span.SequenceEqual(new byte[] { 0xFF, 0x80 }));
        check("Server34/36 empty fields use four/three bytes; reject all incomplete prefixes and stop before trailing bytes",
            MatrixServerMessages34And36Wire.Decode(new byte[] { 34, 0, 0, 0 }).BytesConsumed == 4
            && MatrixServerMessages34And36Wire.Decode(new byte[] { 36, 0, 0 }).BytesConsumed == 3
            && new[] { buffer, announcement }.All(packet =>
                Enumerable.Range(0, packet.Length).All(length =>
                    MatrixServerMessages34And36Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer34And36ReadOutcome.Accepted)
                && MatrixServerMessages34And36Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
            && MatrixServerMessages34And36Wire.Decode(new byte[] { 0, 34, 0 }).Outcome == MatrixServer34And36ReadOutcome.NonCanonicalHeader
            && MatrixServerMessages34And36Wire.Decode(new byte[] { 35 }).Outcome == MatrixServer34And36ReadOutcome.UnexpectedMessageCode);
        var maxBlob = new byte[] { 34, 255, 255 }.Concat(Enumerable.Repeat((byte)0x80, 65535))
            .Concat(new byte[] { 255 }).ToArray();
        check("Server34 supports full unsigned65535-byte blob length without treating embedded bytes as text",
            MatrixServerMessages34And36Wire.Decode(maxBlob)
                is { BytesConsumed: 65539, Payload: MatrixBufferSnapshotPayload { RawFlagAt38: 255, BlobBytesAt28.Length: 65535 } });
        check("Server34 normalizes every nonzero flag for snapshot; zero never clears an existing pending flag",
            Enumerable.Range(0, 256).All(i =>
                MatrixBufferSnapshotNativeLayout.Select((byte)i, false)
                    == new MatrixBufferSnapshotSelection(true, i != 0, i != 0, i != 0)
                && MatrixBufferSnapshotNativeLayout.Select((byte)i, true).PendingByteAfter));
        byte[] firstChunk = [1, 0, 0xFF];
        var partial = MatrixBufferSnapshotNativeLayout.AppendAndSelectSnapshot(
            ReadOnlySpan<byte>.Empty, firstChunk, 0, false, null);
        firstChunk[0] = 9;
        var complete = MatrixBufferSnapshotNativeLayout.AppendAndSelectSnapshot(
            partial.AccumulatedBytes.Span, new byte[] { 2, 0x80 }, 0xFE, partial.PendingByte, partial.SnapshotBytes);
        check("Server34 slBlob appends chunks, snapshots the full accumulated bytes and clears logical buffer",
            partial.AccumulatedBytes.Span.SequenceEqual(new byte[] { 1, 0, 0xFF })
            && !partial.PendingByte && !partial.SnapshotBytes.HasValue
            && complete.AccumulatedBytes.IsEmpty && complete.PendingByte
            && complete.SnapshotBytes.HasValue
            && complete.SnapshotBytes.Value.Span.SequenceEqual(new byte[] { 1, 0, 0xFF, 2, 0x80 })
            && MatrixBufferSnapshotNativeLayout.Select(0xFE, false).ClearProcessedBlob);
        var nextPartial = MatrixBufferSnapshotNativeLayout.AppendAndSelectSnapshot(
            complete.AccumulatedBytes.Span, new byte[] { 3 }, 0, complete.PendingByte, complete.SnapshotBytes);
        var empty = MatrixBufferSnapshotNativeLayout.AppendAndSelectSnapshot(default, default, 1, false, null);
        check("Server34 flag0 preserves existing snapshot/pending; a completed empty snapshot is present, not null",
            nextPartial.PendingByte && nextPartial.SnapshotBytes.Equals(complete.SnapshotBytes)
            && nextPartial.AccumulatedBytes.Span.SequenceEqual(new byte[] { 3 })
            && empty.SnapshotBytes.HasValue && empty.SnapshotBytes.Value.IsEmpty && empty.PendingByte);
        byte[] snapshotSource = [4, 5];
        var clone = MatrixBufferSnapshotNativeLayout.CloneSnapshot(snapshotSource);
        snapshotSource[0] = 9;
        check("Snapshot helper deep-copies cached bytes; cache reuse requires pointer presence and exact key including0",
            clone.HasValue && clone.Value.Span.SequenceEqual(new byte[] { 4, 5 })
            && !MatrixBufferSnapshotNativeLayout.CloneSnapshot(null).HasValue
            && MatrixBufferSnapshotNativeLayout.CloneSnapshot(ReadOnlyMemory<byte>.Empty).HasValue
            && MatrixBufferSnapshotNativeLayout.SelectCachedSnapshot(true, 0, 0)
            && MatrixBufferSnapshotNativeLayout.SelectCachedSnapshot(true, 7, 7)
            && !MatrixBufferSnapshotNativeLayout.SelectCachedSnapshot(true, 7, 8)
            && !MatrixBufferSnapshotNativeLayout.SelectCachedSnapshot(false, 7, 7));
        check("Announcement36 exact case-sensitive sysmsg selects broadcast only with an existing mode",
            MatrixAnnouncementNativeLayout.SelectDisplay("sysmsg"u8, true)
            && !MatrixAnnouncementNativeLayout.SelectDisplay("sysmsg"u8, false)
            && !MatrixAnnouncementNativeLayout.SelectDisplay("SYSMSG"u8, true)
            && !MatrixAnnouncementNativeLayout.SelectDisplay("sysmsgx"u8, true)
            && !MatrixAnnouncementNativeLayout.SelectDisplay(ReadOnlySpan<byte>.Empty, true));
        int[] minimumSizes =
        [
            4, 13, 3, 103, 17, 2, 1, 17, 6, 7, 22, 21, 18, 18, 9, 11, 19, 9,
            10, 9, 13, 9, 7, 1, 42, 6, 3, 2, 9, 5, 15, 23, 18, 15, 3
        ];
        for (int i = 0; i < minimumSizes.Length; i++)
        {
            int code = 34 + i;
            var packet = new byte[minimumSizes[i]];
            packet[0] = (byte)code;
            check($"Catalog message{code} has a normal-buffer decoder accepting/consuming its exact minimum body",
                DecodeSize(code, packet) == packet.Length && DecodeSize(code, packet[..^1]) == 0);
        }
        check("Decoder coverage fixtures correspond one-to-one with all35 native receiver routes",
            minimumSizes.Length == MatrixServerMessageCatalog.Entries.Count
            && MatrixServerMessageCatalog.Entries.Select(e => e.DecodedMessageId).SequenceEqual(Enumerable.Range(34, 35).Select(i => (uint)i)));
    }

    private static int DecodeSize(int code, byte[] bytes) => code switch
    {
        34 or 36 => MatrixServerMessages34And36Wire.Decode(bytes).BytesConsumed,
        35 => MatrixMessage35Wire.Decode(bytes).BytesConsumed,
        37 => MatrixEnterZoneWire.Decode(bytes).BytesConsumed,
        38 or 39 or 62 => MatrixScalarUpdateWire.Decode(bytes).BytesConsumed,
        40 => MatrixExitZoneNativeLayout.Decode(bytes).BytesConsumed,
        >= 41 and <= 43 => MatrixServerMessages41To43Wire.Decode(bytes).BytesConsumed,
        >= 44 and <= 47 => MatrixServerMessages44To47Wire.Decode(bytes).BytesConsumed,
        >= 48 and <= 50 => MatrixServerMessages48To50Wire.Decode(bytes).BytesConsumed,
        >= 51 and <= 55 => MatrixServerMessages51To55Wire.Decode(bytes).BytesConsumed,
        >= 56 and <= 61 => MatrixServerMessages56To61Wire.Decode(bytes).BytesConsumed,
        >= 63 and <= 68 => MatrixServerMessages63To68Wire.Decode(bytes).BytesConsumed,
        _ => throw new ArgumentOutOfRangeException(nameof(code))
    };
}
