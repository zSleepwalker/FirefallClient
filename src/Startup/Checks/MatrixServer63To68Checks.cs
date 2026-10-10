using Firefall.Reconstruction.Startup;

internal static class MatrixServer63To68Checks
{
    public static void Run(Action<string, bool> check)
    {
        var samples63 = Convert.FromHexString("3F0000FFFF");
        var samples64 = Convert.FromHexString("400000FFFF01008000FF00FEFF0080");
        var found = new byte[] { 65 }.Concat(Convert.FromHexString("0807060504030201FFFFFFFF1817161514131211"))
            .Concat(new byte[] { 0xFF, 0, 2, 0x80, 0, 0x80, 0 }).ToArray();
        var leader = new byte[] { 66 }.Concat(Convert.FromHexString("08070605040302011817161514131211"))
            .Concat(new byte[] { 0x80, 0 }).ToArray();
        var twoTexts = new byte[] { 67 }.Concat(Convert.FromHexString("0807060504030201"))
            .Concat(new byte[] { 0x80, 0, 0xFF, 0 }).Concat(Convert.FromHexString("FFFFFFFF")).ToArray();
        var metadata = Convert.FromHexString("440380000100610002008000FFFF017A00FFFFFFFF");
        check("Server63/64 are two/seven unsigned raw WORD samples, not DWORDs or compact values",
            MatrixServerMessages63To68Wire.Decode(samples63)
                is { BytesConsumed: 5, Payload: MatrixWordSamplesPayload { MessageCode: 63 } s63 }
                && s63.Words.SequenceEqual(new ushort[] { 0, 65535 })
            && MatrixServerMessages63To68Wire.Decode(samples64)
                is { BytesConsumed: 15, Payload: MatrixWordSamplesPayload { MessageCode: 64 } s64 }
                && s64.Words.SequenceEqual(new ushort[] { 0, 65535, 1, 128, 255, 65534, 32768 }));
        check("Server65 LFGMatchFound keeps QWORD/CategoryDWORD/QWORD/raw name/chunked texts in wire order",
            MatrixServerMessages63To68Wire.Decode(found)
                is { Payload: MatrixLfgMatchFoundPayload f }
                && f.MatchId == 0x0102030405060708 && f.LfgCategoryId == uint.MaxValue
                && f.QwordAt38 == 0x1112131415161718 && f.LeaderNameBytes.Span.SequenceEqual(new byte[] { 0xFF })
                && f.TextListAt44.Count == 2 && f.TextListAt44.All(text => text.Span.SequenceEqual(new byte[] { 0x80 })));
        check("Server66/67 retain distinct QWORD/raw-text field shapes",
            MatrixServerMessages63To68Wire.Decode(leader)
                is { Payload: MatrixLfgLeaderChangePayload { MatchId: 0x0102030405060708, QwordAt30: 0x1112131415161718 } l }
                && l.LeaderNameBytes.Span.SequenceEqual(new byte[] { 0x80 })
            && MatrixServerMessages63To68Wire.Decode(twoTexts)
                is { Payload: MatrixZoneChatPayload { RawFlags: uint.MaxValue } t }
                && t.AuthorNameBytes.Span.SequenceEqual(new byte[] { 0x80 })
                && t.TextBytes.Span.SequenceEqual(new byte[] { 0xFF }));
        check("Server68 reuses two mandatory byte-keyed maps, unsigned key sorting and last-value overwrite",
            MatrixServerMessages63To68Wire.Decode(metadata)
                is { Payload: MatrixMetadataMapsPayload m }
                && m.Metadata.Word16Entries.Count == 2
                && m.Metadata.Word16Entries[0].KeyBytes.Span.SequenceEqual(new byte[] { 0x61 })
                && m.Metadata.Word16Entries[1].KeyBytes.Span.SequenceEqual(new byte[] { 0x80 })
                && m.Metadata.Word16Entries[1].ValueBits == ushort.MaxValue
                && m.Metadata.Word32Entries.Single().ValueBits == uint.MaxValue
            && MatrixServerMessages63To68Wire.Decode(new byte[] { 68, 0, 0 }).BytesConsumed == 3);
        check("Server63-68 reject every truncated prefix, malformed/wrong headers and stop at exact body end",
            new[] { samples63, samples64, found, leader, twoTexts, metadata }.All(packet =>
                Enumerable.Range(0, packet.Length).All(length =>
                    MatrixServerMessages63To68Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer63To68ReadOutcome.Accepted)
                && MatrixServerMessages63To68Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
            && MatrixServerMessages63To68Wire.Decode(new byte[] { 0, 65, 0 }).Outcome == MatrixServer63To68ReadOutcome.NonCanonicalHeader
            && MatrixServerMessages63To68Wire.Decode(new byte[] { 62 }).Outcome == MatrixServer63To68ReadOutcome.UnexpectedMessageCode);
        foreach (int count in new[] { 255, 256 })
        {
            var foundMany = new byte[] { 65 }.Concat(new byte[21]).Concat(Chunked(count, new byte[] { 0x80, 0 })).ToArray();
            var mapMany = new byte[] { 68 }.Concat(Chunked(count, new byte[] { 0x80, 0, 0xFF, 0xFF }))
                .Concat(new byte[] { 0 }).ToArray();
            check($"Server65/68 chunk{count} preserves text duplicates but collapses duplicate map keys",
                MatrixServerMessages63To68Wire.Decode(foundMany)
                    is { Payload: MatrixLfgMatchFoundPayload foundPayload } && foundPayload.TextListAt44.Count == count
                && MatrixServerMessages63To68Wire.Decode(mapMany)
                    is { Payload: MatrixMetadataMapsPayload mapPayload } && mapPayload.Metadata.Word16Entries.Count == 1);
            if (count == 255)
                check("Server65 exact255 requires terminal zero count",
                    MatrixServerMessages63To68Wire.Decode(foundMany[..^1]).Outcome == MatrixServer63To68ReadOutcome.TruncatedPayload);
        }
        check("LFG found-match rejects a valid selected ID; leader change requires exact match even for invalid IDs",
            MatrixLfgNativeLayout.SelectMatchFound(true, true, 0, 5)
            && MatrixLfgNativeLayout.SelectMatchFound(true, true, 0xFFFFFFFFFFFFFF01, 5)
            && !MatrixLfgNativeLayout.SelectMatchFound(true, true, 0xFFFFFFFFFFFFFE01, 0)
            && MatrixLfgNativeLayout.SelectMatchFound(true, false, 7, 0)
            && !MatrixLfgNativeLayout.SelectMatchFound(false, false, 7, 0)
            && MatrixLfgNativeLayout.SelectLeaderChange(true, true, 7, 0, 7)
            && MatrixLfgNativeLayout.SelectLeaderChange(true, false, 7, 0xFFFFFFFFFFFFFF01, 0xFFFFFFFFFFFFFF01)
            && !MatrixLfgNativeLayout.SelectLeaderChange(true, true, 0xFFFFFFFFFFFFFF01, 0, 7)
            && !MatrixLfgNativeLayout.SelectLeaderChange(false, true, 7, 0, 7));
        check("WORD sample routes retain the nonmonotonic64 destinations and exact unsigned-to-float conversion",
            MatrixWordSampleNativeLayout.Message63Routes.Select(r => r.ReceiverOffset).SequenceEqual(new[] { 0x2DC, 0x2EC })
            && MatrixWordSampleNativeLayout.Message64Routes.Select(r => r.ReceiverOffset)
                .SequenceEqual(new[] { 0x34C, 0x35C, 0x36C, 0x37C, 0x2FC, 0x30C, 0x33C })
            && MatrixWordSampleNativeLayout.SelectRecording(0x80, 0)
            && !MatrixWordSampleNativeLayout.SelectRecording(0, 0)
            && !MatrixWordSampleNativeLayout.SelectRecording(1, 0x80)
            && Enumerable.Range(0, 65536).All(i => MatrixWordSampleNativeLayout.ToSample((ushort)i) == i));
    }

    private static byte[] Chunked(int total, byte[] entry)
    {
        var result = new List<byte>();
        int remaining = total, count;
        do
        {
            count = Math.Min(255, remaining);
            result.Add((byte)count);
            for (int i = 0; i < count; i++)
                result.AddRange(entry);
            remaining -= count;
        }
        while (count == 255);
        return result.ToArray();
    }
}
